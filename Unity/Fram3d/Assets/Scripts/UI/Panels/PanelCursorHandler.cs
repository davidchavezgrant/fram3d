using Fram3d.Engine.Cursor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
namespace Fram3d.UI.Panels
{
    /// <summary>
    /// Manages cursor state for a docked UI panel. Detects interactive controls
    /// (buttons, sliders, toggles, text fields) under the mouse and sets the
    /// appropriate cursor. Reusable across any panel that needs hover cursors.
    /// </summary>
    internal sealed class PanelCursorHandler
    {
        private bool       _isDraggingInteractiveControl;
        private CursorType _ownedCursor = CursorType.Default;

        public bool OwnsCustomCursor => this._ownedCursor != CursorType.Default;

        public void Release()
        {
            if (this._ownedCursor == CursorType.Default)
            {
                return;
            }

            CursorService.ResetCursor();
            this._ownedCursor = CursorType.Default;
        }

        public void Update(VisualElement root, bool panelVisible)
        {
            if (!panelVisible || root == null || root.panel == null || Mouse.current == null)
            {
                this._isDraggingInteractiveControl = false;
                this.Release();
                return;
            }

            var mousePos  = Mouse.current.position.ReadValue();
            var screenPos = new Vector2(mousePos.x, Screen.height - mousePos.y);
            var panelPos  = RuntimePanelUtils.ScreenToPanel(root.panel, screenPos);
            var picked    = root.panel.Pick(panelPos);

            if (!Mouse.current.leftButton.isPressed)
            {
                this._isDraggingInteractiveControl = false;
            }

            if (picked != null && Mouse.current.leftButton.wasPressedThisFrame && IsDraggableControl(picked))
            {
                this._isDraggingInteractiveControl = true;
            }

            if (this._isDraggingInteractiveControl)
            {
                this.ApplyCursor(CursorType.ClosedHand);
                return;
            }

            if (picked == null)
            {
                // Outside the panel: relinquish ownership without resetting
                // so scene-side cursor logic can take over in the same frame.
                this._ownedCursor = CursorType.Default;
                return;
            }

            if (HasTextFieldCursor(picked))
            {
                this.ApplyCursor(CursorType.IBeam);
                return;
            }

            if (HasInteractiveCursor(picked))
            {
                this.ApplyCursor(CursorType.Link);
                return;
            }

            this.Release();
        }

        private void ApplyCursor(CursorType cursor)
        {
            if (this._ownedCursor == cursor)
            {
                return;
            }

            if (cursor == CursorType.Default)
            {
                this.Release();
                return;
            }

            CursorService.SetCursor(cursor);
            this._ownedCursor = cursor;
        }

        private static bool HasInteractiveCursor(VisualElement element)
        {
            for (var current = element; current != null; current = current.parent)
            {
                if (current is Slider || current is Toggle || current is Button || current is DropdownField)
                {
                    return true;
                }

                if (current.ClassListContains("dropdown-selector")
                 || current.ClassListContains("dropdown-list-row")
                 || current.ClassListContains("sensor-dropdown")
                 || current.ClassListContains("unity-base-dropdown__item")
                 || current.ClassListContains("unity-base-dropdown__label"))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasTextFieldCursor(VisualElement element)
        {
            for (var current = element; current != null; current = current.parent)
            {
                if (current is TextField)
                {
                    return true;
                }

                if (current.ClassListContains("dropdown-search-field"))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDraggableControl(VisualElement element)
        {
            for (var current = element; current != null; current = current.parent)
            {
                if (current is Slider)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
