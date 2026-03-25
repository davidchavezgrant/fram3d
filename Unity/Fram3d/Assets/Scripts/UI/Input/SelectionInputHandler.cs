using Fram3d.Core.Scene;
using Fram3d.Engine.Cursor;
using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.InputSystem;
using SysVector2 = System.Numerics.Vector2;
namespace Fram3d.UI.Input
{
    /// <summary>
    /// Handles mouse input for element selection, hover, and gizmo drag.
    /// Gizmo handles take priority over selection — if a handle is under
    /// the cursor on mouse-down, a gizmo drag starts instead of a selection.
    /// Click-vs-drag discrimination is delegated to Core ClickDetector.
    /// </summary>
    public sealed class SelectionInputHandler: MonoBehaviour
    {
        private const    float         CURSOR_RESET_GRACE_SECONDS = 0.1f;
        private readonly ClickDetector _clickDetector             = new();
        private          bool          _cursorIsPointer;
        private          bool          _isGizmoDragging;
        private          float         _lastInteractiveHoverTime;
        private          Selection     _selection;

        [SerializeField]
        private GizmoController gizmoController;

        [SerializeField]
        private SelectionRaycaster raycaster;

        [SerializeField]
        private SelectionHighlighter selectionHighlighter;

        [SerializeField]
        private ViewCameraManager viewCameraManager;

        public void Tick(Mouse mouse, Keyboard keyboard)
        {
            if (this._selection == null || this.raycaster == null)
            {
                return;
            }

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

            this.HandleDuplicate(keyboard);
            this.UpdateHover(mousePosition);
            this.UpdateGizmoHover(mousePosition);
            this.UpdateCursor();
            this.UpdateSelection(mouse, keyboard, mousePosition);
        }

        private void HandleDuplicate(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return;
            }

            if (!keyboard.dKey.wasPressedThisFrame || !keyboard.ctrlKey.isPressed)
            {
                return;
            }

            ElementDuplicator.TryDuplicate(this._selection);
        }

        private void ResetPointerCursor()
        {
            if (!this._cursorIsPointer)
            {
                return;
            }

            CursorManager.ResetCursor();
            this._cursorIsPointer = false;
        }

        private void UpdateCursor()
        {
            var overElement = this._selection?.HoveredId != null;

            var overGizmo = this.gizmoController            != null
                         && this.gizmoController.ActiveTool != ActiveTool.SELECT
                         && this.gizmoController.IsHoveringHandle;

            var hasInteractiveHover = overElement || overGizmo;

            if (hasInteractiveHover)
            {
                this._lastInteractiveHoverTime = Time.unscaledTime;
            }

            var withinResetGrace = this._cursorIsPointer && Time.unscaledTime - this._lastInteractiveHoverTime <= CURSOR_RESET_GRACE_SECONDS;
            var wantPointer      = hasInteractiveHover || withinResetGrace;

            if (wantPointer == this._cursorIsPointer)
            {
                return;
            }

            if (wantPointer)
            {
                CursorManager.SetCursor(CursorType.Link);
                this._cursorIsPointer = true;
                return;
            }

            this.ResetPointerCursor();
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

        private void UpdateGizmoHover(Vector2 mousePosition)
        {
            if (this.gizmoController != null)
            {
                this.gizmoController.UpdateHover(mousePosition);
            }
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
            var cameraModifier = keyboard != null
                              && (keyboard.altKey.isPressed || keyboard.leftCommandKey.isPressed || keyboard.rightCommandKey.isPressed);

            var result = this._clickDetector.Update(mouse.leftButton.wasPressedThisFrame,
                                                    mouse.leftButton.isPressed,
                                                    mouse.leftButton.wasReleasedThisFrame,
                                                    new SysVector2(mousePosition.x, mousePosition.y),
                                                    cameraModifier);

            // Gizmo handles take priority — check on press frame
            if (mouse.leftButton.wasPressedThisFrame && !cameraModifier)
            {
                if (this.gizmoController != null && this.gizmoController.TryBeginDrag(mousePosition))
                {
                    this._isGizmoDragging = true;
                    this._clickDetector.Suppress();
                    return;
                }
            }

            if (result.Kind == ClickResultKind.CLICK)
            {
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

        private void Start()
        {
            if (this.selectionHighlighter != null)
            {
                this._selection = this.selectionHighlighter.Selection;
            }
        }

        private void Update()
        {
            if (this.viewCameraManager != null && this.viewCameraManager.IsMultiView && Mouse.current != null)
            {
                var mousePos = Mouse.current.position.ReadValue();
                var cam      = this.viewCameraManager.GetUnityCameraAtPosition(mousePos);
                this.raycaster?.SetCamera(cam);
                this.gizmoController?.SetCamera(cam);

                // Clicking an element activates that viewport
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    this.viewCameraManager.ActivateSlotAtPosition(mousePos);
                }
            }

            this.Tick(Mouse.current, Keyboard.current);
        }

        private void OnDisable()
        {
            this.ResetPointerCursor();
        }
    }
}