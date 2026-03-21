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
            var dt      = Time.deltaTime;

            // Scroll Y with modifiers
            if (Mathf.Abs(scrollY) > SCROLL_DEADZONE)
            {
                if (IsCommandHeld(keyboard) && keyboard.altKey.isPressed)
                {
                    // Cmd+Alt+Scroll Y = dolly zoom
                    this._camera.DollyZoom(scrollY * speeds.DollyZoom * dt);
                }
                else if (keyboard.altKey.isPressed)
                {
                    // Alt+Scroll Y = dolly
                    this._camera.Dolly(scrollY * speeds.Dolly * dt);
                }
                else if (keyboard.shiftKey.isPressed)
                {
                    // Shift+Scroll Y = crane
                    this._camera.Crane(scrollY * speeds.Crane * dt);
                }
                else if (keyboard.ctrlKey.isPressed)
                {
                    // Ctrl+Scroll Y = roll
                    this._camera.Roll(scrollY * speeds.Roll * dt);
                }

                // else: Scroll Y without modifiers = focal length (stub until 1.1.2)
            }

            // Cmd+Scroll X = truck
            if (Mathf.Abs(scrollX) > SCROLL_DEADZONE && IsCommandHeld(keyboard))
            {
                this._camera.Truck(scrollX * speeds.Truck * dt);
            }
        }

        private void HandleDragInput(Keyboard keyboard, Mouse mouse)
        {
            if (!mouse.leftButton.isPressed && !mouse.rightButton.isPressed && !mouse.middleButton.isPressed)
                return;

            var delta = mouse.delta.ReadValue();

            if (delta.sqrMagnitude < 0.001f)
                return;

            var speeds = this._camera.Speeds;
            var dt     = Time.deltaTime;

            if (keyboard.ctrlKey.isPressed)
            {
                // Ctrl+Drag = pan (X) + tilt (Y)
                this._camera.Pan(delta.x   * speeds.PanTilt * dt);
                this._camera.Tilt(-delta.y * speeds.PanTilt * dt);
            }
            else if (keyboard.altKey.isPressed)
            {
                // Alt+Drag = orbit
                this._camera.Orbit(delta.x * speeds.PanTilt * dt, -delta.y * speeds.PanTilt * dt);
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