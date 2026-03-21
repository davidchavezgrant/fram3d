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
        private void          Start() => this._camera = this.cameraBehaviour.CameraElement;

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
            var speeds  = this._camera.Speeds;

            // Scroll is a discrete impulse (one notch = ~120), not a continuous per-frame value.
            // Do NOT multiply by deltaTime.
            if (IsCommandHeld(keyboard) && keyboard.altKey.isPressed && Mathf.Abs(scrollY) > SCROLL_DEADZONE)
            {
                // Cmd+Alt+Scroll Y = dolly zoom
                this._camera.DollyZoom(scrollY * speeds.DollyZoom);

                return;
            }

            if (keyboard.ctrlKey.isPressed)
            {
                // Ctrl+Scroll Y = dolly
                if (Mathf.Abs(scrollY) > SCROLL_DEADZONE)
                    this._camera.Dolly(scrollY * speeds.Dolly);

                // Ctrl+Scroll X = truck
                if (Mathf.Abs(scrollX) > SCROLL_DEADZONE)
                    this._camera.Truck(scrollX * speeds.Truck);

                return;
            }

            if (keyboard.altKey.isPressed && Mathf.Abs(scrollY) > SCROLL_DEADZONE)
            {
                // Alt+Scroll Y = crane
                this._camera.Crane(scrollY * speeds.Crane);

                return;
            }

            if (keyboard.shiftKey.isPressed && Mathf.Abs(scrollX) > SCROLL_DEADZONE)
            {
                // Shift+Scroll X = roll
                this._camera.Roll(scrollX * speeds.Roll);

                return;
            }

            // Unmodified Scroll Y = focal length (stub until 1.1.2)
        }

        private void HandleDragInput(Keyboard keyboard, Mouse mouse)
        {
            var delta = mouse.delta.ReadValue();

            if (delta.sqrMagnitude < 0.001f)
                return;

            var speeds = this._camera.Speeds;
            var dt     = Time.deltaTime;

            // Alt+Left-drag = orbit (Unity convention)
            if (keyboard.altKey.isPressed && mouse.leftButton.isPressed)
            {
                this._camera.Orbit(delta.x * speeds.PanTilt * dt, -delta.y * speeds.PanTilt * dt);

                return;
            }

            // Middle-drag = pan/tilt (Unity convention)
            if (mouse.middleButton.isPressed)
            {
                this._camera.Pan(delta.x * speeds.PanTilt * dt);
                this._camera.Tilt(-delta.y * speeds.PanTilt * dt);

                return;
            }

            // Alt+Right-drag = dolly (Unity convention)
            if (keyboard.altKey.isPressed && mouse.rightButton.isPressed)
            {
                this._camera.Dolly(delta.y * speeds.Dolly * dt);
            }
        }

        private void HandleKeyboardInput(Keyboard keyboard)
        {
            // Ctrl+R = reset
            if (keyboard.ctrlKey.isPressed && keyboard.rKey.wasPressedThisFrame)
            {
                this._camera.Reset();
            }
        }

        private static bool IsCommandHeld(Keyboard keyboard)
        {
        #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            return keyboard.leftCommandKey.isPressed || keyboard.rightCommandKey.isPressed;
        #else
			return keyboard.leftCommandKey.isPressed || keyboard.rightCommandKey.isPressed;
        #endif
        }
    }
}