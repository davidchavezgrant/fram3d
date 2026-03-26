using Fram3d.Core.Scene;
using Fram3d.Engine.Cursor;
using Fram3d.Engine.Integration;
using Fram3d.UI.Panels;
using Fram3d.UI.Views;
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
        private const    int           CURSOR_RESET_GRACE_FRAMES = 10;
        private const    float         HOVER_KEEP_DISTANCE_SQ   = 400f; // 20px — keep hover if mouse within this
        private readonly ClickDetector _clickDetector            = new();
        private          bool          _cursorIsClosedHand;
        private          bool          _cursorIsPointer;
        private          int           _framesWithoutHover;
        private          bool          _isGizmoDragging;
        private          Vector2       _lastHoverHitPos;
        private          PropertiesPanelView _propertiesPanel;
        private          Selection     _selection;
        private          ViewLayoutView _viewLayoutView;

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

            if (this.IsPointerOverBlockingUI())
            {
                this._selection?.ClearHover();
                this.gizmoController?.ClearHover();

                if (this._propertiesPanel != null && this._propertiesPanel.OwnsCustomCursor)
                {
                    this.ReleaseSceneCursorOwnership();
                    return;
                }

                this.ResetCustomCursor();
                return;
            }

            this.HandleDuplicate(keyboard);
            this.UpdateHover(mousePosition);
            this.UpdateGizmoHover(mousePosition);
            this.UpdateCursor();
            this.UpdateSelection(mouse, keyboard, mousePosition);
        }

        private void ClearInteractiveHover()
        {
            this._selection?.ClearHover();
            this.gizmoController?.ClearHover();
            this.ResetCustomCursor();
        }

        private void ReleaseSceneCursorOwnership()
        {
            this._framesWithoutHover = 0;
            this._cursorIsClosedHand = false;
            this._cursorIsPointer    = false;
        }

        private bool IsPointerOverBlockingUI()
        {
            return (this._propertiesPanel != null && this._propertiesPanel.IsPointerOverUI)
                || (this._viewLayoutView != null && this._viewLayoutView.IsPointerOverUI);
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

        private void ResetCustomCursor()
        {
            if (!this._cursorIsPointer && !this._cursorIsClosedHand)
            {
                return;
            }

            CursorManager.ResetCursor();
            this._cursorIsClosedHand = false;
            this._cursorIsPointer = false;
        }

        private void SetClosedHandCursor()
        {
            CursorManager.SetCursor(CursorType.ClosedHand);
            this._cursorIsClosedHand = true;
            this._cursorIsPointer    = false;
        }

        private void SetPointerCursor()
        {
            CursorManager.SetCursor(CursorType.Link);
            this._cursorIsClosedHand = false;
            this._cursorIsPointer    = true;
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
                this._framesWithoutHover = 0;

                if (!this._cursorIsPointer || this._cursorIsClosedHand)
                {
                    this.SetPointerCursor();
                }

                return;
            }

            if (!this._cursorIsPointer)
            {
                return;
            }

            this._framesWithoutHover++;

            if (this._framesWithoutHover <= CURSOR_RESET_GRACE_FRAMES)
            {
                return;
            }

            this.ResetCustomCursor();
        }

        private void UpdateGizmoDrag(Mouse mouse, Vector2 mousePosition)
        {
            if (mouse.leftButton.isPressed)
            {
                if (!this._cursorIsClosedHand)
                {
                    this.SetClosedHandCursor();
                }

                this.gizmoController.UpdateDrag(mousePosition);
                return;
            }

            // Mouse released — end drag
            this.gizmoController.EndDrag();
            this._isGizmoDragging = false;
            this.ResetCustomCursor();
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
                this._lastHoverHitPos = mousePosition;
                return;
            }

            // If the raycast missed but the mouse hasn't moved far from the
            // last hit position, keep the previous hover. This prevents
            // single-frame raycast misses (e.g., ground plane stealing the
            // hit at element edges) from clearing hover and flickering the cursor.
            if (this._selection.HoveredId != null)
            {
                var delta = mousePosition - this._lastHoverHitPos;

                if (delta.sqrMagnitude < HOVER_KEEP_DISTANCE_SQ)
                {
                    return;
                }
            }

            this._selection.ClearHover();
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
                    this.SetClosedHandCursor();
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

            this._propertiesPanel ??= FindAnyObjectByType<PropertiesPanelView>();
            this._viewLayoutView ??= FindAnyObjectByType<ViewLayoutView>();
        }

        private void Update()
        {
            var isPointerOverBlockingUI = this.IsPointerOverBlockingUI();

            if (this.viewCameraManager != null && this.viewCameraManager.IsMultiView && Mouse.current != null)
            {
                var mousePos = Mouse.current.position.ReadValue();
                var cam      = this.viewCameraManager.GetUnityCameraAtPosition(mousePos);
                this.raycaster?.SetCamera(cam);
                this.gizmoController?.SetCamera(cam);

                // Clicking an element activates that viewport
                if (Mouse.current.leftButton.wasPressedThisFrame && !isPointerOverBlockingUI)
                {
                    this.viewCameraManager.ActivateSlotAtPosition(mousePos);
                }
            }

            this.Tick(Mouse.current, Keyboard.current);
        }

        private void OnDisable()
        {
            this.ResetCustomCursor();
        }
    }
}
