using System.Collections.Generic;
using Fram3d.Core.Common;
using Fram3d.Core.Scene;
using Fram3d.Engine.Conversion;
using UnityEngine;
using SysVector3 = System.Numerics.Vector3;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Manages the runtime transform gizmo — display, tool switching, and
    /// drag interaction. Shows translate/rotate/scale handles at the selected
    /// element's position. During drag, writes directly to Element properties
    /// (no command dispatch). Renders always-on-top via GizmoHandle shader.
    /// </summary>
    public sealed class GizmoController: MonoBehaviour
    {
        private const float CONSTANT_SCREEN_SIZE = 0.15f;
        public const  int   GIZMO_LAYER_INDEX    = 6;

        private static readonly Color AXIS_X = new(0.9f,
                                                   0.2f,
                                                   0.2f,
                                                   1f);

        private static readonly Color AXIS_Y = new(0.4f,
                                                   0.85f,
                                                   0.2f,
                                                   1f);

        private static readonly Color AXIS_Z = new(0.2f,
                                                   0.5f,
                                                   0.95f,
                                                   1f);

        private static readonly Color DRAG_COLOR = new(0f,
                                                       1f,
                                                       1f,
                                                       1f);

        private static readonly Color HOVER_COLOR = new(1f,
                                                        0.92f,
                                                        0.016f,
                                                        1f);

        private static readonly Color SCALE_COLOR = new(0.85f,
                                                        0.85f,
                                                        0.85f,
                                                        1f);

        private static readonly int                         SHADER_COLOR = Shader.PropertyToID("_Color");
        private                 DragSession                 _activeDrag;
        private readonly        Dictionary<Renderer, Color> _axisColors  = new();
        private                 Renderer                    _draggedRenderer;
        private readonly        GizmoState                  _gizmoState  = new();
        private                 GameObject                  _gizmoRoot;
        private                 Material                    _handleMaterial;
        private                 Renderer                    _hoveredRenderer;
        private                 GameObject                  _rotateGroup;
        private                 GameObject                  _scaleGroup;
        private                 Selection                   _selection;
        private                 GameObject                  _translateGroup;

        [SerializeField]
        private SelectionHighlighter selectionHighlighter;

        [SerializeField]
        private Camera targetCamera;

        public ActiveTool ActiveTool       => this._gizmoState.ActiveTool;
        public bool       IsDragging       => this._activeDrag != null;
        public bool       IsHoveringHandle => this._hoveredRenderer != null;
        public bool       IsVisible        => this._gizmoRoot != null && this._gizmoRoot.activeSelf;

        public void EndDrag()
        {
            // Future: create ICommand with before/after state here (milestone 4.1)
            this.ClearDragHighlight();
            this._activeDrag = null;
        }

        public void SetActiveTool(ActiveTool tool)
        {
            this._gizmoState.SetActiveTool(tool);
            this.ClearHoverHighlight();
            this.UpdateToolVisibility();
        }

        // ── Public API ──────────────────────────────────────────────────

        /// <summary>
        /// Called by SelectionInputHandler on mouse-down to check if a
        /// gizmo handle is under the cursor. Returns true if a handle
        /// was hit and a drag has started.
        /// </summary>
        public bool TryBeginDrag(Vector2 screenPosition)
        {
            if (this.ActiveTool == ActiveTool.SELECT)
            {
                return false;
            }

            if (this._selection == null || this._selection.SelectedId == null)
            {
                return false;
            }

            var ray = this.targetCamera.ScreenPointToRay(screenPosition);

            if (!Physics.Raycast(ray,
                                 out var hit,
                                 1000f,
                                 1 << GIZMO_LAYER_INDEX))
            {
                return false;
            }

            var handleName = hit.collider.gameObject.name;
            var element    = FindSelectedElement();

            if (element == null)
            {
                return false;
            }

            var axis       = GizmoAxis.Parse(handleName);
            var axisOffset = SysVector3.Zero;

            // Capture the initial offset so the element doesn't snap
            // to the mouse position on first drag frame
            if (this.ActiveTool == ActiveTool.TRANSLATE)
            {
                var projected = TransformOperations.ProjectOntoAxis(element.Position,
                                                                    axis.Direction,
                                                                    ray.origin.ToSystem(),
                                                                    ray.direction.ToSystem());

                var delta = projected - element.Position;
                axisOffset = SysVector3.Dot(delta, axis.Direction) * axis.Direction;
            }

            this._activeDrag = new DragSession(axis,
                                               element,
                                               screenPosition.x,
                                               screenPosition.y,
                                               axisOffset);

            // Highlight the dragged handle cyan
            var renderer = hit.collider.GetComponent<Renderer>();
            this.SetDragHighlight(renderer);
            return true;
        }

        /// <summary>
        /// Resets the selected element's transform property corresponding to
        /// the active tool. Returns true if a reset was performed (element
        /// selected + non-Select tool active), false otherwise.
        /// </summary>
        public bool TryResetActiveTool()
        {
            var element = this.FindSelectedElement();
            return this._gizmoState.TryResetActiveTool(element);
        }

        public void UpdateDrag(Vector2 screenPosition)
        {
            if (this._activeDrag == null)
            {
                return;
            }

            if (this.ActiveTool == ActiveTool.TRANSLATE)
            {
                var ray = this.targetCamera.ScreenPointToRay(screenPosition);
                this._activeDrag.UpdateTranslation(ray.origin.ToSystem(),
                                                   ray.direction.ToSystem());
            }
            else if (this.ActiveTool == ActiveTool.ROTATE)
            {
                this._activeDrag.UpdateRotation(screenPosition.x);
            }
            else if (this.ActiveTool == ActiveTool.SCALE)
            {
                this._activeDrag.UpdateScale(screenPosition.y);
            }
        }

        /// <summary>
        /// Updates hover highlighting on gizmo handles. Called each frame
        /// from SelectionInputHandler when not dragging.
        /// </summary>
        public void UpdateHover(Vector2 screenPosition)
        {
            if (this.ActiveTool == ActiveTool.SELECT || !this._gizmoRoot.activeSelf)
            {
                this.ClearHoverHighlight();
                return;
            }

            var ray = this.targetCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray,
                                out var hit,
                                1000f,
                                1 << GIZMO_LAYER_INDEX))
            {
                var renderer = hit.collider.GetComponent<Renderer>();

                if (renderer != this._hoveredRenderer)
                {
                    this.ClearHoverHighlight();
                    this._hoveredRenderer = renderer;
                    renderer.material.SetColor(SHADER_COLOR, HOVER_COLOR);
                }
            }
            else
            {
                this.ClearHoverHighlight();
            }
        }

        // ── Gizmo construction ──────────────────────────────────────────

        private void BuildGizmoRoot()
        {
            this._gizmoRoot = new GameObject("GizmoRoot");
            this._gizmoRoot.transform.SetParent(this.transform, false);
            SetLayerRecursive(this._gizmoRoot, GIZMO_LAYER_INDEX);
        }

        private void BuildRotateHandles()
        {
            this._rotateGroup = new GameObject("RotateGroup");
            this._rotateGroup.transform.SetParent(this._gizmoRoot.transform, false);
            var ringMesh = GizmoMeshBuilder.CreateRing();

            CreateHandle("RotateY",
                         ringMesh,
                         Quaternion.identity,
                         AXIS_Y,
                         this._rotateGroup);

            CreateHandle("RotateX",
                         ringMesh,
                         Quaternion.Euler(0f, 0f, 90f),
                         AXIS_X,
                         this._rotateGroup);

            CreateHandle("RotateZ",
                         ringMesh,
                         Quaternion.Euler(90f, 0f, 0f),
                         AXIS_Z,
                         this._rotateGroup);
        }

        private void BuildScaleHandle()
        {
            this._scaleGroup = new GameObject("ScaleGroup");
            this._scaleGroup.transform.SetParent(this._gizmoRoot.transform, false);
            var diamondMesh = GizmoMeshBuilder.CreateDiamond();

            CreateHandle("ScaleUniform",
                         diamondMesh,
                         Quaternion.identity,
                         SCALE_COLOR,
                         this._scaleGroup);
        }

        private void BuildTranslateHandles()
        {
            this._translateGroup = new GameObject("TranslateGroup");
            this._translateGroup.transform.SetParent(this._gizmoRoot.transform, false);
            var arrowMesh = GizmoMeshBuilder.CreateArrow();

            CreateHandle("TranslateY",
                         arrowMesh,
                         Quaternion.identity,
                         AXIS_Y,
                         this._translateGroup);

            CreateHandle("TranslateX",
                         arrowMesh,
                         Quaternion.Euler(0f, 0f, -90f),
                         AXIS_X,
                         this._translateGroup);

            CreateHandle("TranslateZ",
                         arrowMesh,
                         Quaternion.Euler(90f, 0f, 0f),
                         AXIS_Z,
                         this._translateGroup);
        }

        // ── Handle highlighting ─────────────────────────────────────────

        private void ClearDragHighlight()
        {
            if (this._draggedRenderer != null)
            {
                this.RestoreAxisColor(this._draggedRenderer);
                this._draggedRenderer = null;
            }
        }

        private void ClearHoverHighlight()
        {
            if (this._hoveredRenderer != null)
            {
                this.RestoreAxisColor(this._hoveredRenderer);
                this._hoveredRenderer = null;
            }
        }

        private void CreateHandle(string     handleName,
                                  Mesh       mesh,
                                  Quaternion rotation,
                                  Color      color,
                                  GameObject parent)
        {
            var go = new GameObject(handleName);
            go.transform.SetParent(parent.transform, false);
            go.transform.localRotation = rotation;
            go.layer                   = GIZMO_LAYER_INDEX;
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr  = go.AddComponent<MeshRenderer>();
            var mat = new Material(this._handleMaterial);
            mat.SetColor(SHADER_COLOR, color);
            mr.sharedMaterial = mat;
            var mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
            mc.convex     = true;

            // Store the true axis color for this handle so highlighting
            // can always restore to the correct color
            this._axisColors[mr] = color;
        }

        // ── Helpers ─────────────────────────────────────────────────────

        private Element FindSelectedElement()
        {
            if (this._selection?.SelectedId == null)
            {
                return null;
            }

            var behaviours = FindObjectsByType<ElementBehaviour>(FindObjectsSortMode.None);

            foreach (var behaviour in behaviours)
            {
                if (behaviour.Element != null && behaviour.Element.Id == this._selection.SelectedId)
                {
                    return behaviour.Element;
                }
            }

            return null;
        }

        private void RestoreAxisColor(Renderer renderer)
        {
            if (this._axisColors.TryGetValue(renderer, out var axisColor))
            {
                renderer.material.SetColor(SHADER_COLOR, axisColor);
            }
        }

        // ── Display ─────────────────────────────────────────────────────

        private void ScaleForConstantScreenSize()
        {
            var gizmoPos = this._gizmoRoot.transform.position;
            var camPos   = this.targetCamera.transform.position;
            var distance = Vector3.Distance(gizmoPos, camPos);
            this._gizmoRoot.transform.localScale = Vector3.one * (distance * CONSTANT_SCREEN_SIZE);
        }

        private void SetDragHighlight(Renderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            // Clear hover state — drag takes over
            if (renderer == this._hoveredRenderer)
            {
                this._hoveredRenderer = null;
            }

            this._draggedRenderer = renderer;
            renderer.material.SetColor(SHADER_COLOR, DRAG_COLOR);
        }

        private void UpdateToolVisibility()
        {
            this._translateGroup.SetActive(this.ActiveTool == ActiveTool.TRANSLATE);
            this._rotateGroup.SetActive(this.ActiveTool    == ActiveTool.ROTATE);
            this._scaleGroup.SetActive(this.ActiveTool     == ActiveTool.SCALE);
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;

            foreach (Transform child in go.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }

        // ── Lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            this._handleMaterial = new Material(Shader.Find("Fram3d/GizmoHandle"));
            this.BuildGizmoRoot();
            this.BuildTranslateHandles();
            this.BuildRotateHandles();
            this.BuildScaleHandle();
            this._gizmoRoot.SetActive(false);
        }

        private void Start()
        {
            if (this.selectionHighlighter != null)
            {
                this._selection = this.selectionHighlighter.Selection;
            }
        }

        private void LateUpdate()
        {
            if (this._selection == null)
            {
                return;
            }

            var currentId = this._selection.SelectedId;
            var element   = this.FindSelectedElement();

            if (element == null)
            {
                this._gizmoRoot.SetActive(false);
                return;
            }

            // Reset to translate tool on every new selection
            if (this._gizmoState.OnSelectionChanged(currentId))
            {
                this.ClearHoverHighlight();
            }

            this._gizmoRoot.SetActive(this.ActiveTool != ActiveTool.SELECT);
            this._gizmoRoot.transform.position = element.Position.ToUnity();
            this._gizmoRoot.transform.rotation = Quaternion.identity;
            this.ScaleForConstantScreenSize();
            this.UpdateToolVisibility();
        }
    }
}
