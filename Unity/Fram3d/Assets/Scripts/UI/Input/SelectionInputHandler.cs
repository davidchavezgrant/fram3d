using Fram3d.Core.Scene;
using Fram3d.Engine.Integration;
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
        private Texture2D _pointerCursor;
        private Selection _selection;

        private void Start()
        {
            if (this.selectionHighlighter != null)
            {
                this._selection = this.selectionHighlighter.Selection;
            }

            this._pointerCursor = CreatePointerCursor();
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
            this.UpdateCursor(mousePosition);
            this.UpdateSelection(mouse, keyboard, mousePosition);
        }

        private void UpdateGizmoHover(Vector2 mousePosition)
        {
            if (this.gizmoController != null)
            {
                this.gizmoController.UpdateHover(mousePosition);
            }
        }

        private void UpdateCursor(Vector2 mousePosition)
        {
            var overElement = this._selection?.HoveredId != null;
            var overGizmo   = this.gizmoController != null
                           && this.gizmoController.ActiveTool != ActiveTool.SELECT
                           && this.gizmoController.IsHoveringHandle;
            var wantPointer = overElement || overGizmo;

            if (wantPointer && !this._cursorIsPointer)
            {
                Cursor.SetCursor(this._pointerCursor, new Vector2(5f, 0f), CursorMode.ForceSoftware);
                this._cursorIsPointer = true;
            }
            else if (!wantPointer && this._cursorIsPointer)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                this._cursorIsPointer = false;
            }
        }

        /// <summary>
        /// Creates a 32x32 hand pointer cursor. Unity doesn't expose system
        /// cursor types at runtime, so we draw a minimal one programmatically.
        /// </summary>
        private static Texture2D CreatePointerCursor()
        {
            // Rows are top-to-bottom as drawn. Unity textures are bottom-up, so we flip Y.
            // . = transparent, X = black (outline), W = white (fill)
            var shape = new[]
            {
                //         1111111111222222222233
                // 1234567890123456789012345678901
                "........XX......................", // 0
                "......XX.WX.....................", // 1
                "......X.WWX.....................", // 2
                "......X.WWX.....................", // 3
                "......X.WWWX....................", // 4
                "......X.WWWX....................", // 5
                "......X.WWWWX...................", // 6
                "......X.WWWWX...................", // 7
                "......X.WWWWWX..................", // 8
                "......X.WWWWWX..................", // 9
                "......X.WWWWWWX.................", // 10
                "......X.WWWWWWX.................", // 11
                "......X.WWWWWWWX................", // 12
                "......X.WWWWWWWX................", // 13
                "......X.WWWWWWWWX...............", // 14
                "......X.WWWWWWWWX...............", // 15
                "......X.WWWWWWWWWX..............", // 16
                "......X.WWWWWWWWWX..............", // 17
                "......X.WWWWWWXXXXXX............", // 18
                "......X.WWWXWWX.................", // 19
                "......X.WWXX.XWX................", // 20
                "......X.WX...XWX................", // 21
                "......X.XX....XWX...............", // 22
                "......XX......XWX...............", // 23
                "..............XWWX..............", // 24
                "...............XX...............", // 25
                "................................", // 26
                "................................", // 27
                "................................", // 28
                "................................", // 29
                "................................", // 30
                "................................", // 31
            };

            var size  = 32;
            var tex   = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var clear = new Color(0, 0, 0, 0);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    tex.SetPixel(x, y, clear);
                }
            }

            for (var row = 0; row < size; row++)
            {
                var line     = shape[row];
                var flippedY = size - 1 - row;

                for (var col = 0; col < line.Length; col++)
                {
                    var c = line[col];

                    if (c == 'X')
                    {
                        tex.SetPixel(col, flippedY, Color.black);
                    }
                    else if (c == 'W')
                    {
                        tex.SetPixel(col, flippedY, Color.white);
                    }
                }
            }

            tex.Apply();
            return tex;
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
