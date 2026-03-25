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
    /// Play Mode tests for SelectionInputHandler. Tests the full input
    /// pipeline (input → raycast → select) and input logic (modifier
    /// guards, click-vs-drag threshold).
    ///
    /// Tests call Tick() explicitly with test devices after each yield to
    /// avoid Keyboard.current / Mouse.current identity issues in Play Mode.
    ///
    /// Important: the click lifecycle spans 3 frames, not 2:
    ///   Frame 1: wasPressedThisFrame → records mouse-down position
    ///   Frame 2: isPressed → checks drag threshold (button still held)
    ///   Frame 3: wasReleasedThisFrame → evaluates raycast → select/deselect
    /// Tests must yield between press and release to allow the held frame.
    /// </summary>
    public sealed class SelectionInputHandlerTests
    {
        private GameObject            _cameraGo;
        private GameObject            _cube;
        private SelectionInputHandler _handler;
        private SelectionHighlighter  _highlighter;
        private Keyboard              _keyboard;
        private Mouse                 _mouse;

        // --- Modifier guards ---

        [UnityTest]
        public IEnumerator AltClick__DoesNotChangeSelection__When__AltHeld()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            var selectedBefore = this._highlighter.Selection.SelectedId;
            Assert.IsNotNull(selectedBefore, "Precondition: element should be selected");
            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftAlt));

            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            position = center,
                                            buttons  = 1
                                        });

            yield return null;
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse,    new MouseState { position = center });
            yield return null;

            Assert.AreEqual(selectedBefore, this._highlighter.Selection.SelectedId, "Alt+click should not change selection (reserved for orbit)");
        }

        [UnityTest]
        public IEnumerator Click__Deselects__When__ClickingEmptySpace()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            // Pre-select via API
            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            Assert.IsNotNull(this._highlighter.Selection.SelectedId, "Precondition");

            // Click empty space (top-left corner, far from cube)
            var corner = new Vector2(10f, Screen.height - 10f);

            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            position = corner,
                                            buttons  = 1
                                        });

            yield return null;

            this._handler.Tick(this._mouse, this._keyboard);
            yield return null;

            this._handler.Tick(this._mouse, this._keyboard);

            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            position = corner,
                                            buttons  = 0
                                        });

            yield return null;

            this._handler.Tick(this._mouse, this._keyboard);
            Assert.IsNull(this._highlighter.Selection.SelectedId, "Clicking empty space should deselect");
        }

        // --- Full pipeline: click to select ---

        [UnityTest]
        public IEnumerator Click__Selects__When__ClickingElement()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);

            // Frame 1: mouse down
            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            position = center,
                                            buttons  = 1
                                        });

            yield return null;

            this._handler.Tick(this._mouse, this._keyboard);

            // Frame 2: held (drag threshold check)
            yield return null;

            this._handler.Tick(this._mouse, this._keyboard);

            // Frame 3: mouse up → raycast → select
            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            position = center,
                                            buttons  = 0
                                        });

            yield return null;

            this._handler.Tick(this._mouse, this._keyboard);
            Assert.IsNotNull(this._highlighter.Selection.SelectedId, "Clicking an element should select it");
        }

        // --- Click-vs-drag threshold ---

        [UnityTest]
        public IEnumerator Click__Selects__When__MouseMovesLessThanThreshold()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);

            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            position = center,
                                            buttons  = 1
                                        });

            yield return null;

            this._handler.Tick(this._mouse, this._keyboard);

            // Small movement (under 5px threshold)
            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            position = center + new Vector2(2f, 2f),
                                            buttons  = 1
                                        });

            yield return null;

            this._handler.Tick(this._mouse, this._keyboard);

            // Release
            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            position = center + new Vector2(2f, 2f),
                                            buttons  = 0
                                        });

            yield return null;

            this._handler.Tick(this._mouse, this._keyboard);
            Assert.IsNotNull(this._highlighter.Selection.SelectedId, "Small mouse movement should still count as a click and select");
        }

        /// <summary>
        /// Ctrl+D duplicates the selected element. Don't call Tick() manually —
        /// Update() already calls it. Calling both would fire the shortcut twice
        /// in one frame (wasPressedThisFrame stays true for the whole frame).
        /// </summary>
        [UnityTest]
        public IEnumerator CtrlD__DuplicatesElement__When__ElementSelected()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            Assert.IsNotNull(this._highlighter.Selection.SelectedId, "Precondition: element selected");

            // Press Ctrl+D — Update() processes it on the next frame
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl, Key.D));
            yield return null;

            // Release
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            yield return null;

            // Verify duplication occurred — selection should have changed to the duplicate
            Assert.IsNotNull(this._highlighter.Selection.SelectedId, "Something should be selected");
            Assert.AreNotEqual(element.Id, this._highlighter.Selection.SelectedId, "Duplicate should be selected, not original");

            // Count elements — should be 2 (original + duplicate)
            var behaviours = Object.FindObjectsByType<ElementBehaviour>(FindObjectsSortMode.None);
            Assert.AreEqual(2, behaviours.Length, "Should have original + duplicate");

            // Clean up the duplicate
            foreach (var b in behaviours)
            {
                if (b.gameObject != this._cube)
                {
                    Object.DestroyImmediate(b.gameObject);
                }
            }
        }

        [UnityTest]
        public IEnumerator CtrlD__DoesNothing__When__NothingSelected()
        {
            yield return null;

            Assert.IsNull(this._highlighter.Selection.SelectedId, "Precondition: nothing selected");

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl, Key.D));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            yield return null;

            // No elements should have been created
            var behaviours = Object.FindObjectsByType<ElementBehaviour>(FindObjectsSortMode.None);
            Assert.AreEqual(1, behaviours.Length, "No duplicate should be created when nothing selected");
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

            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            position = center,
                                            buttons  = 1
                                        });

            yield return null;
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse,    new MouseState { position = center });
            yield return null;

            Assert.AreEqual(selectedBefore, this._highlighter.Selection.SelectedId, "Cmd+click should not change selection (reserved for pan/tilt)");
        }

        [UnityTest]
        public IEnumerator Drag__DoesNotDeselect__When__MouseMovesMoreThanThreshold()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            Assert.IsNotNull(this._highlighter.Selection.SelectedId, "Precondition");

            // Mouse down far from cube
            var corner = new Vector2(10f, 10f);

            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            position = corner,
                                            buttons  = 1
                                        });

            yield return null;

            // Drag far (exceeds 5px threshold)
            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            position = corner + new Vector2(50f, 50f),
                                            buttons  = 1
                                        });

            yield return null;

            // Release
            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            position = corner + new Vector2(50f, 50f),
                                            buttons  = 0
                                        });

            yield return null;

            Assert.IsNotNull(this._highlighter.Selection.SelectedId, "Dragging past threshold should not deselect");
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
            SetField(this._handler, "raycaster",            raycaster);
            this._cube                    = GameObject.CreatePrimitive(PrimitiveType.Cube);
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
            InputSystem.QueueStateEvent(this._mouse,    new MouseState());
            InputSystem.RemoveDevice(this._keyboard);
            InputSystem.RemoveDevice(this._mouse);
            Object.DestroyImmediate(this._cube);
            Object.DestroyImmediate(this._cameraGo);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }
    }
}