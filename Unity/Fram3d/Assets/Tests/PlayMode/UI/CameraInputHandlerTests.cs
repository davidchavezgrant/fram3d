using System.Collections;
using System.Reflection;
using Fram3d.Core.Camera;
using Fram3d.Engine.Integration;
using Fram3d.UI.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using SysVector3 = System.Numerics.Vector3;

namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Play Mode tests for CameraInputHandler keyboard shortcuts.
    /// Queues Input System state events and lets the frame process them
    /// naturally — no manual InputSystem.Update() calls.
    /// </summary>
    public sealed class CameraInputHandlerTests
    {
        private CameraBehaviour    _behaviour;
        private CameraElement      _cam;
        private GameObject         _go;
        private CameraInputHandler _handler;
        private Keyboard           _keyboard;

        private Mouse _mouse;

        [SetUp]
        public void SetUp()
        {
            this._go        = new GameObject("TestCamera");
            this._behaviour = this._go.AddComponent<CameraBehaviour>();
            this._handler   = this._go.AddComponent<CameraInputHandler>();

            var behaviourField = typeof(CameraInputHandler).GetField("cameraBehaviour",
                BindingFlags.NonPublic | BindingFlags.Instance);
            behaviourField.SetValue(this._handler, this._behaviour);

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
            Object.Destroy(this._go);
        }

        // --- A key: cycle aspect ratio ---

        [UnityTest]
        public IEnumerator AKey__CyclesAspectRatioForward__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.ActiveAspectRatio;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.A));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreNotSame(before, this._cam.ActiveAspectRatio);
        }

        [UnityTest]
        public IEnumerator ShiftA__CyclesAspectRatioBackward__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.ActiveAspectRatio;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftShift, Key.A));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreNotSame(before, this._cam.ActiveAspectRatio);
        }

        // --- D key: toggle DOF ---

        [UnityTest]
        public IEnumerator DKey__TogglesDof__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.DofEnabled;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.D));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreNotEqual(before, this._cam.DofEnabled);
        }

        // --- S key: toggle shake ---

        [UnityTest]
        public IEnumerator SKey__TogglesShake__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.ShakeEnabled;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.S));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreNotEqual(before, this._cam.ShakeEnabled);
        }

        // --- Bracket keys: aperture ---

        [UnityTest]
        public IEnumerator LeftBracket__StepsApertureWider__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.Aperture;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftBracket));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.Less(this._cam.Aperture, before);
        }

        [UnityTest]
        public IEnumerator RightBracket__StepsApertureNarrower__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.Aperture;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.RightBracket));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.Greater(this._cam.Aperture, before);
        }

        // --- Ctrl+R: reset ---

        [UnityTest]
        public IEnumerator CtrlR__ResetsCameraPosition__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            this._cam.Dolly(5.0f);
            this._cam.Pan(1.0f);

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl, Key.R));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());

            var defaultPos = new System.Numerics.Vector3(0f, 1.6f, 5f);
            Assert.AreEqual(defaultPos.X, this._cam.Position.X, 0.01f);
            Assert.AreEqual(defaultPos.Y, this._cam.Position.Y, 0.01f);
            Assert.AreEqual(defaultPos.Z, this._cam.Position.Z, 0.01f);
        }

        // --- Number keys: focal length presets ---

        [UnityTest]
        public IEnumerator Digit1__SetsFocalLengthPreset__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.FocalLength;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.Digit1));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreNotEqual(before, this._cam.FocalLength);
        }

        // --- Modifier keys should NOT trigger unmodified shortcuts ---

        [UnityTest]
        public IEnumerator CtrlA__DoesNotCycleAspectRatio__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.ActiveAspectRatio;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl, Key.A));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreSame(before, this._cam.ActiveAspectRatio);
        }

        // --- Scroll: unmodified → focal length ---

        [UnityTest]
        public IEnumerator Scroll__ChangesFocalLength__When__Unmodified()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            // Switch to zoom lens so continuous FL change works
            this._cam.SetLensSet(new LensSet("Zoom", 24f, 200f, false, 1.0f));
            this._cam.SetFocalLengthPreset(50f);
            var before = this._cam.FocalLength;

            // Need to wait long enough for momentum guard to expire
            for (var i = 0; i < 30; i++)
                yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState { scroll = new Vector2(0, 5f) });
            yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            Assert.AreNotEqual(before, this._cam.FocalLength);
        }

        // --- Scroll: Ctrl+scroll Y → Dolly ---

        [UnityTest]
        public IEnumerator CtrlScrollY__Dollies__When__ScrolledVertically()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var posBefore = this._cam.Position;

            // Set Ctrl held via keyboard event, then scroll
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl));
            InputSystem.QueueStateEvent(this._mouse, new MouseState { scroll = new Vector2(0, 10f) });
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse, new MouseState());

            // Dolly moves along forward axis — Z should change
            Assert.AreNotEqual(posBefore.Z, this._cam.Position.Z, 0.001f);
        }

        // --- Scroll: Ctrl+scroll X → Truck ---

        [UnityTest]
        public IEnumerator CtrlScrollX__Trucks__When__ScrolledHorizontally()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var posBefore = this._cam.Position;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl));
            InputSystem.QueueStateEvent(this._mouse, new MouseState { scroll = new Vector2(10f, 0) });
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse, new MouseState());

            Assert.AreNotEqual(posBefore.X, this._cam.Position.X, 0.001f);
        }

        // --- Scroll: Alt+scroll Y → Crane ---

        [UnityTest]
        public IEnumerator AltScrollY__Cranes__When__ScrolledVertically()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var yBefore = this._cam.Position.Y;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftAlt));
            InputSystem.QueueStateEvent(this._mouse, new MouseState { scroll = new Vector2(0, 10f) });
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse, new MouseState());

            Assert.AreNotEqual(yBefore, this._cam.Position.Y, 0.001f);
        }

        // --- Scroll: Shift+scroll X → Roll ---

        [UnityTest]
        public IEnumerator ShiftScrollX__Rolls__When__ScrolledHorizontally()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var rotBefore = this._cam.Rotation;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftShift));
            InputSystem.QueueStateEvent(this._mouse, new MouseState { scroll = new Vector2(10f, 0) });
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse, new MouseState());

            Assert.AreNotEqual(rotBefore, this._cam.Rotation);
        }

        // --- Drag: middle mouse → Pan/Tilt ---

        [UnityTest]
        public IEnumerator MiddleMouseDrag__PansAndTilts__When__Dragged()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var rotBefore = this._cam.Rotation;

            // Middle button = bit 2 (value 4) in MouseState.buttons
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                delta   = new Vector2(50f, 30f),
                buttons = 4
            });
            yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState());

            Assert.AreNotEqual(rotBefore, this._cam.Rotation);
        }

        // --- Drag: Alt+left mouse → Orbit ---

        [UnityTest]
        public IEnumerator AltLeftMouseDrag__Orbits__When__Dragged()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var posBefore = this._cam.Position;

            // Left button = bit 0 (value 1)
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftAlt));
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                delta   = new Vector2(50f, 0f),
                buttons = 1
            });
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse, new MouseState());

            Assert.AreNotEqual(posBefore, this._cam.Position);
        }

        // --- Drag: Alt+right mouse → Dolly ---

        [UnityTest]
        public IEnumerator AltRightMouseDrag__Dollies__When__Dragged()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var posBefore = this._cam.Position;

            // Right button = bit 1 (value 2)
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftAlt));
            InputSystem.QueueStateEvent(this._mouse, new MouseState
            {
                delta   = new Vector2(0f, 50f),
                buttons = 2
            });
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse, new MouseState());

            Assert.AreNotEqual(posBefore.Z, this._cam.Position.Z, 0.001f);
        }
    }
}
