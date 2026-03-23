using System.Collections;
using System.Reflection;
using Fram3d.Engine.Integration;
using Fram3d.UI.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Play Mode tests for SelectionInputHandler. Verifies click-vs-drag
    /// discrimination, modifier guards, and gizmo drag priority.
    /// </summary>
    public sealed class SelectionInputHandlerTests
    {
        private GameObject            _cameraGo;
        private GameObject            _cube;
        private SelectionInputHandler _handler;
        private SelectionHighlighter  _highlighter;
        private Keyboard              _keyboard;
        private Mouse                 _mouse;

        // --- Modifier guards: Alt/Cmd block selection ---

        [UnityTest]
        public IEnumerator AltClick__DoesNotSelect__When__AltHeld()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            // Click the cube with Alt held — should NOT select (Alt = orbit)
            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftAlt));
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                position = center,
                buttons  = 1
            });
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center });
            yield return null;

            Assert.IsNull(this._highlighter.Selection.SelectedId,
                          "Alt+click should not select an element (reserved for orbit)");
        }

        [UnityTest]
        public IEnumerator CmdClick__DoesNotSelect__When__CommandHeld()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCommand));
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                position = center,
                buttons  = 1
            });
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center });
            yield return null;

            Assert.IsNull(this._highlighter.Selection.SelectedId,
                          "Cmd+click should not select an element (reserved for pan/tilt)");
        }

        // --- Click-vs-drag threshold ---

        [UnityTest]
        public IEnumerator Click__Selects__When__MouseMovesLessThanThreshold()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);

            // Mouse down on the cube
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                position = center,
                buttons  = 1
            });
            yield return null;

            // Move mouse slightly (under 5px threshold) then release
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                position = center + new Vector2(2f, 2f),
                buttons  = 0
            });
            yield return null;

            Assert.IsNotNull(this._highlighter.Selection.SelectedId,
                             "Small mouse movement should still count as a click and select");
        }

        [UnityTest]
        public IEnumerator Drag__DoesNotSelect__When__MouseMovesMoreThanThreshold()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);

            // Mouse down
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                position = center,
                buttons  = 1
            });
            yield return null;

            // Move mouse far (over 5px threshold) — this is a drag, not a click
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                position = center + new Vector2(50f, 50f),
                buttons  = 1
            });
            yield return null;

            // Release
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                position = center + new Vector2(50f, 50f),
                buttons  = 0
            });
            yield return null;

            Assert.IsNull(this._highlighter.Selection.SelectedId,
                          "Dragging past threshold should not select");
        }

        // --- Deselect on empty space ---

        [UnityTest]
        public IEnumerator Click__Deselects__When__ClickingEmptySpace()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            // First select the cube
            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            Assert.IsNotNull(this._highlighter.Selection.SelectedId);

            // Click far from the cube (top-left corner = empty space)
            var corner = new Vector2(10f, Screen.height - 10f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                position = corner,
                buttons  = 1
            });
            yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                position = corner,
                buttons  = 0
            });
            yield return null;

            Assert.IsNull(this._highlighter.Selection.SelectedId,
                          "Clicking empty space should deselect");
        }

        [SetUp]
        public void SetUp()
        {
            this._cameraGo = new GameObject("TestCamera");
            var camera = this._cameraGo.AddComponent<Camera>();
            this._cameraGo.transform.position = Vector3.zero;
            this._cameraGo.transform.rotation = Quaternion.identity;

            var raycaster = this._cameraGo.AddComponent<SelectionRaycaster>();
            SetField(raycaster, "targetCamera", camera);

            this._highlighter = this._cameraGo.AddComponent<SelectionHighlighter>();
            this._handler     = this._cameraGo.AddComponent<SelectionInputHandler>();
            SetField(this._handler, "selectionHighlighter", this._highlighter);
            SetField(this._handler, "raycaster", raycaster);

            // Place a cube in front of the camera for selection tests
            this._cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            this._cube.name               = "TestCube";
            this._cube.transform.position = new Vector3(0f, 0f, 5f);
            this._cube.AddComponent<ElementBehaviour>();

            this._keyboard = InputSystem.AddDevice<Keyboard>();
            this._mouse    = InputSystem.AddDevice<Mouse>();
        }

        [TearDown]
        public void TearDown()
        {
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            InputSystem.RemoveDevice(this._keyboard);
            InputSystem.RemoveDevice(this._mouse);
            Object.DestroyImmediate(this._cube);
            Object.DestroyImmediate(this._cameraGo);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }
    }
}
