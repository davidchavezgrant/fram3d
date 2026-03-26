using System.Collections;
using System.Reflection;
using Fram3d.Engine.Integration;
using Fram3d.UI.Input;
using NUnit.Framework;
using Fram3d.Engine.Cursor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Tests that the cursor management pipeline (SelectionInputHandler →
    /// CursorManager → ICursorService) calls the right service methods at the
    /// right time. Uses a RecordingCursorService injected via CursorManager.SetService()
    /// to observe calls without touching platform-specific cursor code.
    ///
    /// Does NOT test the native macOS cursor implementation (EditorCursorService,
    /// CursorWrapper.dylib) — those are AppKit-level and can only be verified
    /// manually. These tests guard against regression in the decision logic.
    /// </summary>
    public sealed class CursorBehaviourTests
    {
        private GameObject             _cameraGo;
        private GameObject             _cube;
        private RecordingCursorService _cursorService;
        private SelectionInputHandler  _handler;
        private SelectionHighlighter   _highlighter;
        private Mouse                  _mouse;
        private ICursorService         _originalService;

        // --- OnDisable cleanup ---

        [UnityTest]
        public IEnumerator OnDisable__ResetsCursor__When__CursorIsPointer()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            // Hover cube to set pointer
            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center });
            yield return null;

            Assert.AreEqual(CursorType.Link, this._cursorService.LastCursor, "Precondition");
            this._cursorService.ResetCounts();
            this._handler.enabled = false;
            Assert.IsTrue(this._cursorService.ResetCallCount > 0, "Disabling the handler should reset the cursor");
        }

        [Test]
        public void ResetCursor__DelegatesToService__When__ServiceIsSet()
        {
            CursorManager.SetCursor(CursorType.Link);
            this._cursorService.ResetCounts();
            CursorManager.ResetCursor();
            Assert.IsTrue(this._cursorService.ResetCallCount > 0);
        }

        // --- CursorManager facade routing ---

        [Test]
        public void SetCursor__DelegatesToService__When__ServiceIsSet()
        {
            var result = CursorManager.SetCursor(CursorType.Link);
            Assert.IsTrue(result);
            Assert.AreEqual(CursorType.Link, this._cursorService.LastCursor);
        }

        [Test]
        public void SetCursor__ReturnsFalse__When__NoServiceSet()
        {
            CursorManager.SetService(null);
            var result = CursorManager.SetCursor(CursorType.Link);
            Assert.IsFalse(result);

            // Restore for subsequent tests
            CursorManager.SetService(this._cursorService);
            this._cursorService.ResetCounts();
        }

        // --- Setup / TearDown ---

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
            this._mouse = InputSystem.AddDevice<Mouse>();

            // Inject recording cursor service, save original for restore
            this._originalService = GetStaticField<ICursorService>(typeof(CursorManager), "_service");
            this._cursorService   = new RecordingCursorService();
            CursorManager.SetService(this._cursorService);
            this._cursorService.ResetCounts();
        }

        [TearDown]
        public void TearDown()
        {
            // Restore original cursor service before cleanup
            if (this._originalService != null)
            {
                CursorManager.SetService(this._originalService);
            }

            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            InputSystem.RemoveDevice(this._mouse);
            Object.DestroyImmediate(this._cube);
            Object.DestroyImmediate(this._cameraGo);
        }

        [UnityTest]
        public IEnumerator UpdateCursor__NoRedundantCalls__When__ContinuouslyHovering()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center });
            yield return null;

            Assert.AreEqual(CursorType.Link, this._cursorService.LastCursor, "Precondition");
            var callsAfterFirstSet = this._cursorService.SetCursorCallCount;

            // Continue hovering for 5 frames
            for (var i = 0; i < 5; i++)
            {
                yield return null;
            }

            Assert.AreEqual(callsAfterFirstSet,
                            this._cursorService.SetCursorCallCount,
                            "SetCursor should not be called again when hover state hasn't changed");
        }

        [UnityTest]
        public IEnumerator UpdateCursor__ResetsCursor__When__LeavingElement()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            // Hover cube to set pointer
            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center });
            yield return null;

            Assert.AreEqual(CursorType.Link, this._cursorService.LastCursor, "Precondition: cursor should be Link");
            this._cursorService.ResetCounts();

            // Move away from cube
            var corner = new Vector2(10f, Screen.height - 10f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = corner });

            // Poll until cursor resets after the 100ms grace period expires.
            // Editor frames can be as fast as 1-2ms, so we need many iterations.
            for (var i = 0; i < 600; i++)
            {
                yield return null;

                if (this._cursorService.ResetCallCount > 0)
                {
                    break;
                }
            }

            Assert.IsTrue(this._cursorService.ResetCallCount > 0, "Cursor should reset after leaving hover and grace period expiring");
        }

        // --- Hover → cursor set/reset ---

        [UnityTest]
        public IEnumerator UpdateCursor__SetsLink__When__HoveringElement()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center });
            yield return null;

            Assert.AreEqual(CursorType.Link, this._cursorService.LastCursor, "Hovering over an element should set cursor to Link");
        }

        [UnityTest]
        public IEnumerator UpdateCursor__StaysPointer__When__WithinGracePeriod()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            // Hover cube to set pointer
            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center });
            yield return null;

            Assert.AreEqual(CursorType.Link, this._cursorService.LastCursor, "Precondition: cursor should be Link");
            this._cursorService.ResetCounts();

            // Move away — but only wait 1 frame (well within 100ms grace)
            var corner = new Vector2(10f, Screen.height - 10f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = corner });
            yield return null;

            Assert.AreEqual(0, this._cursorService.ResetCallCount, "Cursor should not reset within the grace period");
        }

        // --- Hover keep distance (FRA-52 anti-flicker) ---

        [UnityTest]
        public IEnumerator UpdateHover__KeepsHover__When__MouseMovesSlightlyOffElement()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            // Hover cube to establish hover
            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center });
            yield return null;

            Assert.AreEqual(CursorType.Link, this._cursorService.LastCursor, "Precondition: cursor on element");
            this._cursorService.ResetCounts();

            // Move mouse slightly off the element — within the 20px keep distance.
            // The element is at Z=5, filling roughly 100px at screen center.
            // Moving 15px from center should miss the raycast but stay within
            // HOVER_KEEP_DISTANCE_SQ (400 = 20^2).
            var nearbyPos = center + new Vector2(0f, 80f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = nearbyPos });
            yield return null;

            // Cursor should still be Link — hover kept due to proximity
            Assert.AreEqual(0, this._cursorService.ResetCallCount,
                "Cursor should not reset when mouse stays near the element");
        }

        [UnityTest]
        public IEnumerator UpdateHover__ClearsHover__When__MouseMovesFarFromElement()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            // Hover cube
            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center });
            yield return null;

            Assert.AreEqual(CursorType.Link, this._cursorService.LastCursor, "Precondition");
            this._cursorService.ResetCounts();

            // Move far away — well beyond the 20px keep distance
            var farPos = new Vector2(10f, 10f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = farPos });

            // Wait for grace frames to expire
            for (var i = 0; i < 600; i++)
            {
                yield return null;

                if (this._cursorService.ResetCallCount > 0)
                {
                    break;
                }
            }

            Assert.IsTrue(this._cursorService.ResetCallCount > 0,
                "Cursor should reset when mouse moves far from the element");
        }

        // --- ClosedHand cursor during gizmo drag ---

        [UnityTest]
        public IEnumerator UpdateCursor__SetsClosedHand__When__GizmoDragActive()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            // Set up a GizmoController so gizmo drag cursor logic activates.
            // We simulate the drag state via reflection since setting up actual
            // gizmo handle raycasts is prohibitively complex.
            var gizmoController = this._cameraGo.AddComponent<GizmoController>();
            SetField(gizmoController, "selectionHighlighter", this._highlighter);
            SetField(gizmoController, "targetCamera", this._cameraGo.GetComponent<Camera>());
            SetField(this._handler, "gizmoController", gizmoController);

            // Select the cube
            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            // Simulate active gizmo drag by setting _isGizmoDragging via reflection
            SetField(this._handler, "_isGizmoDragging", true);
            this._cursorService.ResetCounts();

            // Mouse button held during drag
            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center, buttons = 1 });
            yield return null;

            Assert.AreEqual(CursorType.ClosedHand, this._cursorService.LastCursor,
                "Cursor should be ClosedHand during gizmo drag");

            // Release mouse to end drag
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center, buttons = 0 });
            yield return null;

            Assert.IsTrue(this._cursorService.ResetCallCount > 0,
                "Cursor should reset when gizmo drag ends");
        }

        // --- Cursor resets to default when moving from element to empty space ---

        [UnityTest]
        public IEnumerator UpdateCursor__TransitionsCorrectly__When__HoverThenLeave()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            // Start with no cursor
            Assert.IsNull(this._cursorService.LastCursor, "Precondition: no cursor set");

            // Hover the element
            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center });
            yield return null;

            Assert.AreEqual(CursorType.Link, this._cursorService.LastCursor, "Should set Link on hover");
            var setCountAfterHover = this._cursorService.SetCursorCallCount;

            // Stay on element — no additional SetCursor calls
            for (var i = 0; i < 3; i++)
            {
                yield return null;
            }

            Assert.AreEqual(setCountAfterHover, this._cursorService.SetCursorCallCount,
                "No redundant SetCursor while continuously hovering");

            // Move far away
            this._cursorService.ResetCounts();
            var corner = new Vector2(10f, Screen.height - 10f);
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = corner });

            // Wait for grace period to expire
            for (var i = 0; i < 600; i++)
            {
                yield return null;

                if (this._cursorService.ResetCallCount > 0)
                {
                    break;
                }
            }

            Assert.IsTrue(this._cursorService.ResetCallCount > 0, "Should reset after leaving");

            // Return to element — should set Link again
            this._cursorService.ResetCounts();
            InputSystem.QueueStateEvent(this._mouse, new MouseState { position = center });
            yield return null;

            Assert.AreEqual(CursorType.Link, this._cursorService.LastCursor,
                "Should re-set Link when re-entering hover");
        }

        private static T GetStaticField<T>(System.Type type, string fieldName)
        {
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            return (T)field.GetValue(null);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }


        /// <summary>
        /// Test double that records all calls to ICursorService methods.
        /// Injected via CursorManager.SetService() to observe cursor changes
        /// without touching platform-specific native code.
        /// </summary>
        private sealed class RecordingCursorService: ICursorService
        {
            public CursorType? LastCursor         { get; private set; }
            public int         ResetCallCount     { get; private set; }
            public int         SetCursorCallCount { get; private set; }

            public void ResetCounts()
            {
                this.LastCursor         = null;
                this.ResetCallCount     = 0;
                this.SetCursorCallCount = 0;
            }

            public void ResetCursor()
            {
                this.LastCursor = null;
                this.ResetCallCount++;
            }

            public bool SetCursor(CursorType cursor)
            {
                this.LastCursor = cursor;
                this.SetCursorCallCount++;
                return true;
            }
        }
    }
}