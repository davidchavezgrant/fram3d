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
            var result = CursorManager.SetCursor(CursorType.Crosshair);
            Assert.IsTrue(result);
            Assert.AreEqual(CursorType.Crosshair, this._cursorService.LastCursor);
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
            this._originalService = GetStaticField<ICursorService>(typeof(CursorManager), "_instance");
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
            public int        ResetCallCount     { get; private set; }
            public int        SetCursorCallCount { get; private set; }

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