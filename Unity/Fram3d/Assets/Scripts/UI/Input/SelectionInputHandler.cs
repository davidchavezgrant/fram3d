using Fram3d.Core.Scene;
using Fram3d.Engine.Integration;
using Riten.Native.Cursors;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Fram3d.UI.Input
{
    /// <summary>
    /// Handles mouse input for element selection, hover, and gizmo drag.
    /// Gizmo handles take priority over selection — if a handle is under
    /// the cursor on mouse-down, a gizmo drag starts instead of a selection.
    /// Click-vs-drag discrimination prevents selection during camera drags.
    /// </summary>
    public sealed class SelectionInputHandler: MonoBehaviour
    {
        private const float CLICK_THRESHOLD = 5f;

        [SerializeField]
        private GizmoController gizmoController;

        [SerializeField]
        private SelectionHighlighter selectionHighlighter;

        [SerializeField]
        private SelectionRaycaster raycaster;

        private bool      _cursorIsPointer;
        private bool      _isDragging;
        private bool      _isGizmoDragging;
        private bool      _mouseDownValid;
        private Vector2   _mouseDownPosition;
        private Selection _selection;

        private void Start()
        {
            if (this.selectionHighlighter != null)
            {
                this._selection = this.selectionHighlighter.Selection;
            }
        }

        private void Update()
        {
            if (this._selection == null || this.raycaster == null)
            {
                return;
            }

            var mouse    = Mouse.current;
            var keyboard = Keyboard.current;

            if (mouse == null)
            {
                return;
            }

            var mousePosition = mouse.position.ReadValue();

            // During gizmo drag, only process drag updates — no hover or selection
            if (this._isGizmoDragging)
            {
                this.UpdateGizmoDrag(mouse, mousePosition);
                return;
            }

            this.UpdateHover(mousePosition);
            this.UpdateGizmoHover(mousePosition);
            this.UpdateCursor();
            this.UpdateSelection(mouse, keyboard, mousePosition);
        }

        private void UpdateGizmoHover(Vector2 mousePosition)
        {
            if (this.gizmoController != null)
            {
                this.gizmoController.UpdateHover(mousePosition);
            }
        }

        private void UpdateCursor()
        {
            var overElement = this._selection?.HoveredId != null;
            var overGizmo   = this.gizmoController != null
                           && this.gizmoController.ActiveTool != ActiveTool.SELECT
                           && this.gizmoController.IsHoveringHandle;
            var wantPointer = overElement || overGizmo;

            if (wantPointer)
            {
                NativeCursor.SetCursor(NTCursors.Link);
            }
            else if (this._cursorIsPointer)
            {
                NativeCursor.ResetCursor();
            }

            this._cursorIsPointer = wantPointer;
        }

        private void UpdateGizmoDrag(Mouse mouse, Vector2 mousePosition)
        {
            if (mouse.leftButton.isPressed)
            {
                this.gizmoController.UpdateDrag(mousePosition);
                return;
            }

            // Mouse released — end drag
            this.gizmoController.EndDrag();
            this._isGizmoDragging = false;
        }

        private void UpdateHover(Vector2 mousePosition)
        {
            var element = this.raycaster.Raycast(mousePosition);

            if (element != null)
            {
                this._selection.Hover(element.Id);
            }
            else
            {
                this._selection.ClearHover();
            }
        }

        private void UpdateSelection(Mouse mouse, Keyboard keyboard, Vector2 mousePosition)
        {
            if (mouse.leftButton.wasPressedThisFrame)
            {
                // Skip if modifier held — those are camera operations
                if (keyboard != null
                 && (keyboard.altKey.isPressed
                  || keyboard.leftCommandKey.isPressed
                  || keyboard.rightCommandKey.isPressed))
                {
                    this._mouseDownValid = false;
                    return;
                }

                // Gizmo handles take priority over selection
                if (this.gizmoController != null && this.gizmoController.TryBeginDrag(mousePosition))
                {
                    this._isGizmoDragging = true;
                    this._mouseDownValid  = false;
                    return;
                }

                this._mouseDownPosition = mousePosition;
                this._mouseDownValid    = true;
                this._isDragging        = false;
                return;
            }

            if (this._mouseDownValid && mouse.leftButton.isPressed)
            {
                var delta = mousePosition - this._mouseDownPosition;

                if (delta.sqrMagnitude > CLICK_THRESHOLD * CLICK_THRESHOLD)
                {
                    this._isDragging = true;
                }

                return;
            }

            if (this._mouseDownValid && mouse.leftButton.wasReleasedThisFrame)
            {
                this._mouseDownValid = false;

                if (this._isDragging)
                {
                    return;
                }

                var element = this.raycaster.Raycast(mousePosition);

                if (element != null)
                {
                    this._selection.Select(element.Id);
                }
                else
                {
                    this._selection.Deselect();
                }
            }
        }
    }
}
