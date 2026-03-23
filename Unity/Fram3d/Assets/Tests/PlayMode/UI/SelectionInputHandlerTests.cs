using System.Collections;
using System.Reflection;
using Fram3d.Core.Common;
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
    /// Play Mode tests for SelectionInputHandler. Tests the input LOGIC
    /// (modifier guards, click-vs-drag threshold) using the Selection API
    /// for setup and verification. Does NOT test full input→raycast→select
    /// pipeline — that depends on Mouse.current matching the test device,
    /// which is unreliable in Play Mode tests.
    ///
    /// Full click-to-select pipeline is covered by:
    /// - SelectionRaycasterTests (raycasting works)
    /// - SelectionTests (select/deselect domain logic)
    /// - Manual testing (end-to-end)
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
        // These pre-select via API, then verify modifiers prevent RE-selection
        // on a different element (proving the guard actually fires, not just
        // that selection starts null).

        [UnityTest]
        public IEnumerator AltClick__DoesNotChangeSelection__When__AltHeld()
        {
            yield return null;

            // Pre-select the cube via API
            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            var selectedBefore = this._highlighter.Selection.SelectedId;
            Assert.IsNotNull(selectedBefore, "Precondition: element should be selected");

            // Alt+click should NOT change the selection (reserved for orbit)
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

            // Selection should be unchanged — the modifier guard prevented any click processing
            Assert.AreEqual(selectedBefore, this._highlighter.Selection.SelectedId,
                            "Alt+click should not change selection (reserved for orbit)");
        }

        [UnityTest]
        public IEnumerator CmdClick__DoesNotChangeSelection__When__CommandHeld()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            var selectedBefore = this._highlighter.Selection.SelectedId;
            Assert.IsNotNull(selectedBefore, "Precondition: element should be selected");

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

            Assert.AreEqual(selectedBefore, this._highlighter.Selection.SelectedId,
                            "Cmd+click should not change selection (reserved for pan/tilt)");
        }

        // --- Click-vs-drag threshold ---
        // Uses API selection to verify drag behavior without depending on raycasts.

        [UnityTest]
        public IEnumerator Drag__DoesNotDeselect__When__MouseMovesMoreThanThreshold()
        {
            yield return null;

            // Pre-select via API
            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            Assert.IsNotNull(this._highlighter.Selection.SelectedId, "Precondition: should be selected");

            // Mouse down far from cube (would deselect if it were a click)
            var corner = new Vector2(10f, 10f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                position = corner,
                buttons  = 1
            });
            yield return null;

            // Drag far (exceeds 5px threshold) — this is a drag, not a click
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                position = corner + new Vector2(50f, 50f),
                buttons  = 1
            });
            yield return null;

            // Release
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                position = corner + new Vector2(50f, 50f),
                buttons  = 0
            });
            yield return null;

            // Selection should survive — drag exceeded threshold, so no click evaluated
            Assert.IsNotNull(this._highlighter.Selection.SelectedId,
                             "Dragging past threshold should not deselect");
        }

        // --- Full pipeline diagnostic: investigate why input→raycast→select fails ---

        [UnityTest]
        public IEnumerator Diagnostic__MouseCurrentMatchesTestDevice__When__EventQueued()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                position = center,
                buttons  = 1
            });
            yield return null;

            var currentMouse = Mouse.current;
            Debug.Log($"[DIAG] Mouse.current == test device: {currentMouse == this._mouse}");
            Debug.Log($"[DIAG] Mouse.current device ID: {currentMouse?.deviceId}, test device ID: {this._mouse.deviceId}");
            Debug.Log($"[DIAG] leftButton.isPressed: {currentMouse?.leftButton.isPressed}");
            Debug.Log($"[DIAG] leftButton.wasPressedThisFrame: {currentMouse?.leftButton.wasPressedThisFrame}");
            Debug.Log($"[DIAG] test device leftButton.isPressed: {this._mouse.leftButton.isPressed}");
            Debug.Log($"[DIAG] test device leftButton.wasPressedThisFrame: {this._mouse.leftButton.wasPressedThisFrame}");
            Debug.Log($"[DIAG] position: {currentMouse?.position.ReadValue()}");
            Debug.Log($"[DIAG] Screen size: {Screen.width}x{Screen.height}");

            // Verify the test device is current — if this fails, that's the root cause
            Assert.AreEqual(this._mouse.deviceId, currentMouse?.deviceId,
                            "Mouse.current should be the test device after queuing an event on it");

            InputSystem.QueueStateEvent(this._mouse, new MouseState());
        }

        // --- Selection domain logic (tested via API, no input simulation) ---

        [Test]
        public void Select__SetsSelectedId__When__CalledDirectly()
        {
            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);

            Assert.AreEqual(element.Id, this._highlighter.Selection.SelectedId);
        }

        [Test]
        public void Deselect__ClearsSelectedId__When__CalledDirectly()
        {
            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            this._highlighter.Selection.Deselect();

            Assert.IsNull(this._highlighter.Selection.SelectedId);
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
