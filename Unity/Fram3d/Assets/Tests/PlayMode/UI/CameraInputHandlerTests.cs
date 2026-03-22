using System.Collections;
using System.Reflection;
using Fram3d.Core.Camera;
using Fram3d.Engine.Integration;
using Fram3d.UI.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Play Mode tests for CameraInputHandler keyboard shortcuts.
    /// Uses Input System test infrastructure to simulate key presses
    /// and verifies the correct CameraElement state changes.
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

            // Wire the [SerializeField] fields via reflection
            var behaviourField = typeof(CameraInputHandler).GetField("cameraBehaviour",
                BindingFlags.NonPublic | BindingFlags.Instance);
            behaviourField.SetValue(this._handler, this._behaviour);

            // Add a virtual keyboard for input simulation
            this._keyboard = InputSystem.AddDevice<Keyboard>();
        }

        [TearDown]
        public void TearDown()
        {
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

            PressAndRelease(this._keyboard.aKey);
            yield return null;

            Assert.AreNotSame(before, this._cam.ActiveAspectRatio);
        }

        [UnityTest]
        public IEnumerator ShiftA__CyclesAspectRatioBackward__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.ActiveAspectRatio;

            Press(this._keyboard.leftShiftKey);
            PressAndRelease(this._keyboard.aKey);
            yield return null;

            Release(this._keyboard.leftShiftKey);
            Assert.AreNotSame(before, this._cam.ActiveAspectRatio);
        }

        // --- D key: toggle DOF ---

        [UnityTest]
        public IEnumerator DKey__TogglesDof__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.DofEnabled;

            PressAndRelease(this._keyboard.dKey);
            yield return null;

            Assert.AreNotEqual(before, this._cam.DofEnabled);
        }

        // --- S key: toggle shake ---

        [UnityTest]
        public IEnumerator SKey__TogglesShake__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.ShakeEnabled;

            PressAndRelease(this._keyboard.sKey);
            yield return null;

            Assert.AreNotEqual(before, this._cam.ShakeEnabled);
        }

        // --- Bracket keys: aperture ---

        [UnityTest]
        public IEnumerator LeftBracket__StepsApertureWider__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.Aperture; // f/5.6

            PressAndRelease(this._keyboard.leftBracketKey);
            yield return null;

            Assert.Less(this._cam.Aperture, before);
        }

        [UnityTest]
        public IEnumerator RightBracket__StepsApertureNarrower__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.Aperture; // f/5.6

            PressAndRelease(this._keyboard.rightBracketKey);
            yield return null;

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
            var movedPos = this._cam.Position;

            Press(this._keyboard.leftCtrlKey);
            PressAndRelease(this._keyboard.rKey);
            yield return null;

            Release(this._keyboard.leftCtrlKey);

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

            PressAndRelease(this._keyboard.digit1Key);
            yield return null;

            // Digit 1 maps to first preset in the active lens set
            Assert.AreNotEqual(before, this._cam.FocalLength);
        }

        // --- Modifier keys should NOT trigger unmodified shortcuts ---

        [UnityTest]
        public IEnumerator CtrlA__DoesNotCycleAspectRatio__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.ActiveAspectRatio;

            Press(this._keyboard.leftCtrlKey);
            PressAndRelease(this._keyboard.aKey);
            yield return null;

            Release(this._keyboard.leftCtrlKey);
            Assert.AreSame(before, this._cam.ActiveAspectRatio);
        }

        // --- Helpers ---

        private static void Press(KeyControl key)
        {
            InputSystem.QueueStateEvent(key.device, new KeyboardState(key.keyCode));
            InputSystem.Update();
        }

        private static void Release(KeyControl key)
        {
            InputSystem.QueueStateEvent(key.device, new KeyboardState());
            InputSystem.Update();
        }

        private static void PressAndRelease(KeyControl key)
        {
            InputSystem.QueueStateEvent(key.device, new KeyboardState(key.keyCode));
            InputSystem.Update();
            InputSystem.QueueStateEvent(key.device, new KeyboardState());
            InputSystem.Update();
        }
    }
}
