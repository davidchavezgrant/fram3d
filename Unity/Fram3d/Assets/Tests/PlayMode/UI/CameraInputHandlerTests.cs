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
        }

        [TearDown]
        public void TearDown()
        {
            // Release all keys before removing device
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.RemoveDevice(this._keyboard);
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
    }
}
