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
        public const int GIZMO_LAYER_INDEX = 6;
        private readonly GizmoState       _gizmoState          = new();
        private          DragSession      _activeDrag;
        private          GameObject       _gizmoRoot;
        private          GizmoHighlighter _highlighter;
        private          GameObject       _rotateGroup;
        private          GameObject       _scaleGroup;
        private          Selection        _selection;
        private          GameObject       _translateGroup;

        [SerializeField]
        private SelectionHighlighter selectionHighlighter;

        [SerializeField]
        private Camera targetCamera;

        public ActiveTool ActiveTool       => this._gizmoState.ActiveTool;
        public bool       IsDragging       => this._activeDrag != null;
        public bool       IsHoveringHandle => this._highlighter != null && this._highlighter.IsHoveringHandle;
        public bool       IsVisible        => this._gizmoRoot   != null && this._gizmoRoot.activeSelf;

        public void EndDrag()
        {
            // Future: create ICommand with before/after state here (milestone 4.1)
            this._highlighter.ClearDrag();
            this._activeDrag = null;
        }

        public void SetActiveTool(ActiveTool tool)
        {
            this._gizmoState.SetActiveTool(tool);
            this._highlighter.ClearHover();
            this.UpdateToolVisibility();
        }

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
            var element    = this.FindSelectedElement();

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
                var camFwd    = this.targetCamera.transform.forward.ToSystem();
                var projected = TransformOperations.ProjectOntoAxis(element.Position,
                                                                    axis.Direction,
                                                                    ray.origin.ToSystem(),
                                                                    ray.direction.ToSystem(),
                                                                    camFwd);

                var delta = projected - element.Position;
                axisOffset = SysVector3.Dot(delta, axis.Direction) * axis.Direction;
            }

            this._activeDrag = new DragSession(axis,
                                               element,
                                               screenPosition.x,
                                               screenPosition.y,
                                               axisOffset);

            this._highlighter.SetDrag(hit.collider.GetComponent<Renderer>());
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
                var ray     = this.targetCamera.ScreenPointToRay(screenPosition);
                var camFwd  = this.targetCamera.transform.forward.ToSystem();
                this._activeDrag.UpdateTranslation(ray.origin.ToSystem(), ray.direction.ToSystem(), camFwd);
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
                this._highlighter.ClearHover();
                return;
            }

            var ray = this.targetCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray,
                                out var hit,
                                1000f,
                                1 << GIZMO_LAYER_INDEX))
            {
                this._highlighter.SetHover(hit.collider.GetComponent<Renderer>());
            }
            else
            {
                this._highlighter.ClearHover();
            }
        }

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

        private void ScaleForConstantScreenSize()
        {
            var gizmoPos = this._gizmoRoot.transform.position;
            var camPos   = this.targetCamera.transform.position;
            var distance = Vector3.Distance(gizmoPos, camPos);
            var scale    = GizmoScaling.CalculateZoomScale(distance,
                                                           this.targetCamera.fieldOfView,
                                                           this.targetCamera.pixelHeight);
            this._gizmoRoot.transform.localScale = Vector3.one * scale;
        }

        private void UpdateToolVisibility()
        {
            this._translateGroup.SetActive(this.ActiveTool == ActiveTool.TRANSLATE);
            this._rotateGroup.SetActive(this.ActiveTool    == ActiveTool.ROTATE);
            this._scaleGroup.SetActive(this.ActiveTool     == ActiveTool.SCALE);
        }

        private void Awake()
        {
            var material = new Material(Shader.Find("Fram3d/GizmoHandle"));
            var handles  = GizmoHandleFactory.Build(this.transform, material);
            this._gizmoRoot      = handles.Root;
            this._translateGroup = handles.TranslateGroup;
            this._rotateGroup    = handles.RotateGroup;
            this._scaleGroup     = handles.ScaleGroup;
            this._highlighter    = new GizmoHighlighter(handles.AxisColors);
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
                this._highlighter.ClearHover();
            }

            this._gizmoRoot.SetActive(this.ActiveTool != ActiveTool.SELECT);
            this._gizmoRoot.transform.position = element.Position.ToUnity();
            this._gizmoRoot.transform.rotation = Quaternion.identity;
            this.ScaleForConstantScreenSize();
            this.UpdateToolVisibility();
        }
    }
}