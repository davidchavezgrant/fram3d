using Fram3d.Core.Scene;
using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Fram3d.UI.Input
{
    /// <summary>
    /// Handles mouse input for element selection and hover. Left-click
    /// selects/deselects elements; mouse movement updates hover state.
    /// Click-vs-drag discrimination prevents selection during camera drags.
    /// </summary>
    public sealed class SelectionInputHandler: MonoBehaviour
    {
        private const float CLICK_THRESHOLD = 5f;

        [SerializeField]
        private SelectionHighlighter selectionHighlighter;

        [SerializeField]
        private SelectionRaycaster raycaster;

        private bool      _isDragging;
        private bool      _mouseDownValid;
        private Vector2   _mouseDownPosition;
        private Selection _selection;

        private void Start()
        {
            if (this.selectionHighlighter != null)
                this._selection = this.selectionHighlighter.Selection;
        }

        private void Update()
        {
            if (this._selection == null || this.raycaster == null)
                return;

            var mouse    = Mouse.current;
            var keyboard = Keyboard.current;

            if (mouse == null)
                return;

            var mousePosition = mouse.position.ReadValue();

            this.UpdateHover(mousePosition);
            this.UpdateSelection(mouse, keyboard, mousePosition);
        }

        private void UpdateHover(Vector2 mousePosition)
        {
            var element = this.raycaster.Raycast(mousePosition);

            if (element != null)
                this._selection.Hover(element.Id);
            else
                this._selection.ClearHover();
        }

        private void UpdateSelection(Mouse mouse, Keyboard keyboard, Vector2 mousePosition)
        {
            // Track mouse-down for click-vs-drag detection
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

                this._mouseDownPosition = mousePosition;
                this._mouseDownValid    = true;
                this._isDragging        = false;
                return;
            }

            // While held, check if we've exceeded the drag threshold
            if (this._mouseDownValid && mouse.leftButton.isPressed)
            {
                var delta = mousePosition - this._mouseDownPosition;

                if (delta.sqrMagnitude > CLICK_THRESHOLD * CLICK_THRESHOLD)
                    this._isDragging = true;

                return;
            }

            // On release, evaluate selection if it was a clean click (not a drag)
            if (this._mouseDownValid && mouse.leftButton.wasReleasedThisFrame)
            {
                this._mouseDownValid = false;

                if (this._isDragging)
                    return;

                var element = this.raycaster.Raycast(mousePosition);

                if (element != null)
                    this._selection.Select(element.Id);
                else
                    this._selection.Deselect();
            }
        }
    }
}
