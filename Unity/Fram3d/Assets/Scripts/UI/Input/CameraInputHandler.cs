using Fram3d.Core.Camera;
using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fram3d.UI.Input
{
    public sealed class CameraInputHandler: MonoBehaviour
    {
        private const float SCROLL_DEADZONE = 0.01f;

        [SerializeField]
        private CameraBehaviour cameraBehaviour;

        private CameraElement _camera;

        private void Start() => this._camera = this.cameraBehaviour.CameraElement;

        private void Update()
        {
            if (this._camera == null)
                return;

            var keyboard = Keyboard.current;
            var mouse    = Mouse.current;

            if (keyboard == null || mouse == null)
                return;

            this.HandleScrollInput(keyboard, mouse);
            this.HandleDragInput(keyboard, mouse);
            this.HandleKeyboardInput(keyboard);
        }

        private void HandleScrollInput(Keyboard keyboard, Mouse mouse)
        {
            var scroll  = mouse.scroll.ReadValue();
            var scrollY = scroll.y;
            var scrollX = scroll.x;

            // Scroll is a discrete impulse (one notch = ~120), not a continuous per-frame value.
            // Do NOT multiply by deltaTime.
            if (IsCommandHeld(keyboard) && keyboard.altKey.isPressed && Mathf.Abs(scrollY) > SCROLL_DEADZONE)
            {
                this._camera.DollyZoom(scrollY * MovementSpeeds.DOLLY_ZOOM);

                return;
            }

            if (keyboard.ctrlKey.isPressed)
            {
                if (Mathf.Abs(scrollY) > SCROLL_DEADZONE)
                    this._camera.Dolly(scrollY * MovementSpeeds.DOLLY);

                if (Mathf.Abs(scrollX) > SCROLL_DEADZONE)
                    this._camera.Truck(scrollX * MovementSpeeds.TRUCK);

                return;
            }

            if (keyboard.altKey.isPressed && Mathf.Abs(scrollY) > SCROLL_DEADZONE)
            {
                this._camera.Crane(scrollY * MovementSpeeds.CRANE);

                return;
            }

            if (keyboard.shiftKey.isPressed && Mathf.Abs(scrollX) > SCROLL_DEADZONE)
            {
                this._camera.Roll(scrollX * MovementSpeeds.ROLL);

                return;
            }

            // Unmodified Scroll Y = focal length
            if (Mathf.Abs(scrollY) > SCROLL_DEADZONE)
            {
                var activeLensSet = this._camera.ActiveLensSet;

                if (activeLensSet != null && !activeLensSet.IsZoom)
                    if (scrollY > 0)
                        this._camera.StepFocalLengthUp();
                    else
                        this._camera.StepFocalLengthDown();
                else
                    this._camera.FocalLength = this._camera.FocalLength + scrollY * MovementSpeeds.FOCAL_LENGTH_SCROLL;
            }
        }

        private void HandleDragInput(Keyboard keyboard, Mouse mouse)
        {
            var delta = mouse.delta.ReadValue();

            if (delta.sqrMagnitude < 0.001f)
                return;

            var dt       = Time.deltaTime;
            var panSpeed = MovementSpeeds.PAN_TILT * dt;

            // Alt+Left-drag = orbit (Unity convention)
            if (keyboard.altKey.isPressed && mouse.leftButton.isPressed)
            {
                this._camera.Orbit(delta.x * panSpeed, -delta.y * panSpeed);

                return;
            }

            // Middle-drag = pan/tilt (Unity convention)
            if (mouse.middleButton.isPressed)
            {
                this._camera.Pan(delta.x * panSpeed);
                this._camera.Tilt(-delta.y * panSpeed);

                return;
            }

            // Alt+Right-drag = dolly (Unity convention)
            if (keyboard.altKey.isPressed && mouse.rightButton.isPressed)
                this._camera.Dolly(delta.y * MovementSpeeds.DOLLY * dt);
        }

        private void HandleKeyboardInput(Keyboard keyboard)
        {
            if (keyboard.ctrlKey.isPressed && keyboard.rKey.wasPressedThisFrame)
            {
                this._camera.Reset();

                return;
            }

            // Number keys 1–9 = focal length presets (from active lens set, or generic fallback)
            // TODO: For zoom lenses, FocalLengths is empty so this falls through to QUICK.
            //   Consider disabling number keys for zooms or mapping to evenly-spaced values in the range.
            var activeLensSet = this._camera.ActiveLensSet;
            var presets       = activeLensSet != null ? activeLensSet.FocalLengths : FocalLengthPresets.QUICK;

            var digitKeys = new[]
            {
                keyboard.digit1Key, keyboard.digit2Key, keyboard.digit3Key,
                keyboard.digit4Key, keyboard.digit5Key, keyboard.digit6Key,
                keyboard.digit7Key, keyboard.digit8Key, keyboard.digit9Key
            };

            var presetCount = presets.Length < digitKeys.Length ? presets.Length : digitKeys.Length;

            for (var i = 0; i < presetCount; i++)
            {
                if (!digitKeys[i].wasPressedThisFrame)
                    continue;

                this._camera.SetFocalLengthPreset(presets[i]);

                break;
            }
        }

        private static bool IsCommandHeld(Keyboard keyboard) =>
            keyboard.leftCommandKey.isPressed || keyboard.rightCommandKey.isPressed;
    }
}
