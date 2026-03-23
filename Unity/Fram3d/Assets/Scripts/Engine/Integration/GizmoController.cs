using System;
using System.Collections.Generic;
using Fram3d.Core.Common;
using Fram3d.Core.Scene;
using Fram3d.Engine.Conversion;
using UnityEngine;
using SysVector3 = System.Numerics.Vector3;
using SysQuaternion = System.Numerics.Quaternion;
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
        public  const int   GIZMO_LAYER_INDEX    = 6;
        private const float MIN_SCALE            = 0.01f;
        private const float ROTATE_SENSITIVITY   = 0.5f;
        private const float SCALE_SENSITIVITY    = 0.005f;

        private static readonly Color AXIS_X      = new(0.9f, 0.2f, 0.2f, 1f);
        private static readonly Color AXIS_Y      = new(0.4f, 0.85f, 0.2f, 1f);
        private static readonly Color AXIS_Z      = new(0.2f, 0.5f, 0.95f, 1f);
        private static readonly Color DRAG_COLOR  = new(0f, 1f, 1f, 1f);
        private static readonly Color HOVER_COLOR = new(1f, 0.92f, 0.016f, 1f);
        private static readonly Color SCALE_COLOR = new(0.85f, 0.85f, 0.85f, 1f);
        private static readonly int   SHADER_COLOR = Shader.PropertyToID("_Color");

        [SerializeField]
        private SelectionHighlighter selectionHighlighter;

        [SerializeField]
        private Camera targetCamera;

        private readonly Dictionary<Renderer, Color> _axisColors = new();
        private          GizmoAxis        _dragAxis;
        private          Element          _dragElement;
        private          Renderer         _draggedRenderer;
        private          SysVector3       _dragStartAxisOffset;
        private          float            _dragStartMouseX;
        private          float            _dragStartMouseY;
        private          SysVector3       _dragStartPosition;
        private          SysQuaternion    _dragStartRotation;
        private          float            _dragStartScale;
        private          GameObject       _gizmoRoot;
        private          Material         _handleMaterial;
        private          Renderer         _hoveredRenderer;
        private          bool             _isDragging;
        private          ElementId        _lastSelectedId;
        private          GameObject       _rotateGroup;
        private          GameObject       _scaleGroup;
        private          Selection        _selection;
        private          GameObject       _translateGroup;

        public ActiveTool ActiveTool { get; private set; } = ActiveTool.TRANSLATE;
        public bool       IsVisible  => this._gizmoRoot != null && this._gizmoRoot.activeSelf;

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

            if (!Physics.Raycast(ray, out var hit, 1000f, 1 << GIZMO_LAYER_INDEX))
            {
                return false;
            }

            var handleName = hit.collider.gameObject.name;
            var element    = FindSelectedElement();

            if (element == null)
            {
                return false;
            }

            this._dragElement       = element;
            this._dragStartPosition = element.Position;
            this._dragStartRotation = element.Rotation;
            this._dragStartScale    = element.Scale;
            this._dragStartMouseX   = screenPosition.x;
            this._dragStartMouseY   = screenPosition.y;
            this._dragAxis          = ParseAxis(handleName);
            this._isDragging        = true;

            // Capture the initial offset so the element doesn't snap
            // to the mouse position on first drag frame
            if (this.ActiveTool == ActiveTool.TRANSLATE)
            {
                var axisWorld    = GetWorldAxis(this._dragAxis);
                var origin       = this._dragStartPosition.ToUnity();
                var projected    = this.ProjectMouseOntoAxis(screenPosition, axisWorld, origin);
                var initialDelta = projected - origin;
                this._dragStartAxisOffset = (Vector3.Dot(initialDelta, axisWorld) * axisWorld).ToSystem();
            }

            // Highlight the dragged handle cyan
            var renderer = hit.collider.GetComponent<Renderer>();
            this.SetDragHighlight(renderer);
            return true;
        }

        public void UpdateDrag(Vector2 screenPosition)
        {
            if (!this._isDragging || this._dragElement == null)
            {
                return;
            }

            if (this.ActiveTool == ActiveTool.TRANSLATE)
            {
                this.UpdateTranslateDrag(screenPosition);
            }
            else if (this.ActiveTool == ActiveTool.ROTATE)
            {
                this.UpdateRotateDrag(screenPosition);
            }
            else if (this.ActiveTool == ActiveTool.SCALE)
            {
                this.UpdateScaleDrag(screenPosition);
            }
        }

        public void EndDrag()
        {
            // Future: create ICommand with before/after state here (milestone 4.1)
            this.ClearDragHighlight();
            this._isDragging  = false;
            this._dragElement = null;
        }

        public bool IsDragging      => this._isDragging;
        public bool IsHoveringHandle => this._hoveredRenderer != null;

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

            if (Physics.Raycast(ray, out var hit, 1000f, 1 << GIZMO_LAYER_INDEX))
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

        public void SetActiveTool(ActiveTool tool)
        {
            this.ActiveTool = tool;
            this.ClearHoverHighlight();
            this.UpdateToolVisibility();
        }

        /// <summary>
        /// Resets the selected element's transform property corresponding to
        /// the active tool. Returns true if a reset was performed (element
        /// selected + non-Select tool active), false otherwise.
        /// </summary>
        public bool TryResetActiveTool()
        {
            var element = this.FindSelectedElement();

            if (element == null)
            {
                return false;
            }

            if (this.ActiveTool == ActiveTool.TRANSLATE)
            {
                element.Position = SysVector3.Zero;
                return true;
            }

            if (this.ActiveTool == ActiveTool.ROTATE)
            {
                element.Rotation = SysQuaternion.Identity;
                return true;
            }

            if (this.ActiveTool == ActiveTool.SCALE)
            {
                element.Scale = 1f;
                return true;
            }

            return false;
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
                this._lastSelectedId = null;
                return;
            }

            // Reset to translate tool on every new selection
            if (currentId != this._lastSelectedId)
            {
                this._lastSelectedId = currentId;
                this.ActiveTool      = ActiveTool.TRANSLATE;
                this.ClearHoverHighlight();
            }

            this._gizmoRoot.SetActive(this.ActiveTool != ActiveTool.SELECT);
            this._gizmoRoot.transform.position = element.Position.ToUnity();
            this._gizmoRoot.transform.rotation = Quaternion.identity;
            this.ScaleForConstantScreenSize();
            this.UpdateToolVisibility();
        }

        // ── Gizmo construction ──────────────────────────────────────────

        private void BuildGizmoRoot()
        {
            this._gizmoRoot = new GameObject("GizmoRoot");
            this._gizmoRoot.transform.SetParent(this.transform, false);
            SetLayerRecursive(this._gizmoRoot, GIZMO_LAYER_INDEX);
        }

        private void BuildTranslateHandles()
        {
            this._translateGroup = new GameObject("TranslateGroup");
            this._translateGroup.transform.SetParent(this._gizmoRoot.transform, false);
            var arrowMesh = GizmoMeshBuilder.CreateArrow();
            CreateHandle("TranslateY", arrowMesh, Quaternion.identity,            AXIS_Y, this._translateGroup);
            CreateHandle("TranslateX", arrowMesh, Quaternion.Euler(0f, 0f, -90f), AXIS_X, this._translateGroup);
            CreateHandle("TranslateZ", arrowMesh, Quaternion.Euler(90f, 0f, 0f),  AXIS_Z, this._translateGroup);
        }

        private void BuildRotateHandles()
        {
            this._rotateGroup = new GameObject("RotateGroup");
            this._rotateGroup.transform.SetParent(this._gizmoRoot.transform, false);
            var ringMesh = GizmoMeshBuilder.CreateRing();
            CreateHandle("RotateY", ringMesh, Quaternion.identity,            AXIS_Y, this._rotateGroup);
            CreateHandle("RotateX", ringMesh, Quaternion.Euler(0f, 0f, 90f),  AXIS_X, this._rotateGroup);
            CreateHandle("RotateZ", ringMesh, Quaternion.Euler(90f, 0f, 0f),  AXIS_Z, this._rotateGroup);
        }

        private void BuildScaleHandle()
        {
            this._scaleGroup = new GameObject("ScaleGroup");
            this._scaleGroup.transform.SetParent(this._gizmoRoot.transform, false);
            var diamondMesh = GizmoMeshBuilder.CreateDiamond();
            CreateHandle("ScaleUniform", diamondMesh, Quaternion.identity, SCALE_COLOR, this._scaleGroup);
        }

        private void CreateHandle(string    handleName,
                                  Mesh      mesh,
                                  Quaternion rotation,
                                  Color     color,
                                  GameObject parent)
        {
            var go = new GameObject(handleName);
            go.transform.SetParent(parent.transform, false);
            go.transform.localRotation = rotation;
            go.layer = GIZMO_LAYER_INDEX;

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            var mr = go.AddComponent<MeshRenderer>();
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

        // ── Display ─────────────────────────────────────────────────────

        private void ScaleForConstantScreenSize()
        {
            var gizmoPos = this._gizmoRoot.transform.position;
            var camPos   = this.targetCamera.transform.position;
            var distance = Vector3.Distance(gizmoPos, camPos);
            this._gizmoRoot.transform.localScale = Vector3.one * (distance * CONSTANT_SCREEN_SIZE);
        }

        private void UpdateToolVisibility()
        {
            this._translateGroup.SetActive(this.ActiveTool == ActiveTool.TRANSLATE);
            this._rotateGroup.SetActive(this.ActiveTool    == ActiveTool.ROTATE);
            this._scaleGroup.SetActive(this.ActiveTool     == ActiveTool.SCALE);
        }

        // ── Drag logic ──────────────────────────────────────────────────

        private void UpdateTranslateDrag(Vector2 screenPosition)
        {
            var axisWorld = GetWorldAxis(this._dragAxis);
            var origin    = this._dragStartPosition.ToUnity();
            var projected = this.ProjectMouseOntoAxis(screenPosition, axisWorld, origin);
            var delta     = projected - origin;
            var axisDelta = (Vector3.Dot(delta, axisWorld) * axisWorld).ToSystem();
            this._dragElement.Position = this._dragStartPosition + axisDelta - this._dragStartAxisOffset;
        }

        private void UpdateRotateDrag(Vector2 screenPosition)
        {
            var deltaX   = screenPosition.x - this._dragStartMouseX;
            var angle    = deltaX * ROTATE_SENSITIVITY * Mathf.Deg2Rad;
            var axis     = GetSystemAxis(this._dragAxis);
            var rotation = SysQuaternion.CreateFromAxisAngle(axis, angle);
            this._dragElement.Rotation = SysQuaternion.Normalize(rotation * this._dragStartRotation);
        }

        private void UpdateScaleDrag(Vector2 screenPosition)
        {
            var deltaY    = screenPosition.y - this._dragStartMouseY;
            var factor    = 1f + deltaY * SCALE_SENSITIVITY;
            var newScale  = this._dragStartScale * factor;
            this._dragElement.Scale = Math.Max(MIN_SCALE, newScale);
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

        private Vector3 ProjectMouseOntoAxis(Vector2 screenPos, Vector3 axisWorld, Vector3 origin)
        {
            var ray = this.targetCamera.ScreenPointToRay(screenPos);

            // Find the closest point between the ray and the axis line.
            // Use the standard closest-point-between-two-lines formula.
            var d1 = ray.direction;
            var d2 = axisWorld;
            var w  = ray.origin - origin;
            var a  = Vector3.Dot(d1, d1);
            var b  = Vector3.Dot(d1, d2);
            var c  = Vector3.Dot(d2, d2);
            var d  = Vector3.Dot(d1, w);
            var e  = Vector3.Dot(d2, w);
            var denom = a * c - b * b;

            if (Mathf.Abs(denom) < 0.0001f)
            {
                return origin;
            }

            var t = (b * e - c * d) / denom;
            var s = (a * e - b * d) / denom;
            return origin + d2 * s;
        }

        private static Vector3 GetWorldAxis(GizmoAxis axis)
        {
            if (axis == GizmoAxis.X)
            {
                return Vector3.right;
            }

            if (axis == GizmoAxis.Y)
            {
                return Vector3.up;
            }

            return Vector3.forward;
        }

        private static SysVector3 GetSystemAxis(GizmoAxis axis)
        {
            if (axis == GizmoAxis.X)
            {
                return SysVector3.UnitX;
            }

            if (axis == GizmoAxis.Y)
            {
                return SysVector3.UnitY;
            }

            return -SysVector3.UnitZ;
        }

        private static GizmoAxis ParseAxis(string handleName)
        {
            if (handleName.Contains("X"))
            {
                return GizmoAxis.X;
            }

            if (handleName.Contains("Y"))
            {
                return GizmoAxis.Y;
            }

            if (handleName.Contains("Z"))
            {
                return GizmoAxis.Z;
            }

            return GizmoAxis.Uniform;
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

        private void RestoreAxisColor(Renderer renderer)
        {
            if (this._axisColors.TryGetValue(renderer, out var axisColor))
            {
                renderer.material.SetColor(SHADER_COLOR, axisColor);
            }
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

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;

            foreach (Transform child in go.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }

        private enum GizmoAxis
        {
            X,
            Y,
            Z,
            Uniform
        }
    }
}
