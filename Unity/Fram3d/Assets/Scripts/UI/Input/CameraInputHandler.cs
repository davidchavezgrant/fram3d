using System.Collections.Generic;
using Fram3d.Core.Camera;
using Fram3d.Engine.Integration;
using Fram3d.UI.Panels;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
namespace Fram3d.UI.Input
{
    /// <summary>
    /// Processes scroll and drag input for camera control. Uses event-level modifier
    /// tracking to pair each scroll event with the modifier state at the time it
    /// physically occurred, preventing bleed when a modifier key-up and scroll event
    /// land in the same frame.
    /// </summary>
    public sealed class CameraInputHandler: MonoBehaviour
    {
        private const    float               SCROLL_BLEED_COOLDOWN = 0.15f;
        private const    float               SCROLL_DEADZONE       = 0.01f;
        private readonly Queue<ScrollSample> _pendingScrollSamples = new();
        private          CameraElement       _camera;
        private          float               _lastModifierScrollTime;
        private          bool                _leftAltHeld;
        private          bool                _leftCommandHeld;
        private          bool                _leftCtrlHeld;
        private          bool                _leftShiftHeld;
        private          bool                _rightAltHeld;
        private          bool                _rightCommandHeld;
        private          bool                _rightCtrlHeld;
        private          bool                _rightShiftHeld;

        [SerializeField]
        private CameraBehaviour cameraBehaviour;

        [SerializeField]
        private PropertiesPanelView propertiesPanel;

        private void EnqueueScroll(InputEventPtr eventPtr, Mouse mouse)
        {
            if (!mouse.scroll.ReadValueFromEvent(eventPtr, out var scroll))
                return;

            if (Mathf.Abs(scroll.x) <= SCROLL_DEADZONE && Mathf.Abs(scroll.y) <= SCROLL_DEADZONE)
                return;

            this._pendingScrollSamples.Enqueue(new ScrollSample
            {
                X           = scroll.x,
                Y           = scroll.y,
                CtrlHeld    = this._leftCtrlHeld    || this._rightCtrlHeld,
                AltHeld     = this._leftAltHeld     || this._rightAltHeld,
                ShiftHeld   = this._leftShiftHeld   || this._rightShiftHeld,
                CommandHeld = this._leftCommandHeld || this._rightCommandHeld
            });
        }

        // ── Drag input ─────────────────────────────────────────────────────

        private void HandleDragInput(Keyboard keyboard, Mouse mouse)
        {
            var delta = mouse.delta.ReadValue();

            if (delta.sqrMagnitude < 0.001f)
                return;

            var dt       = Time.deltaTime;
            var panSpeed = MovementSpeeds.PAN_TILT * dt;

            if (keyboard.altKey.isPressed && mouse.leftButton.isPressed)
            {
                this._camera.Orbit(delta.x * panSpeed, -delta.y * panSpeed);
                return;
            }

            // Cmd+left click drag: pan/tilt (trackpad-friendly)
            if ((keyboard.leftCommandKey.isPressed || keyboard.rightCommandKey.isPressed)
             && mouse.leftButton.isPressed)
            {
                this._camera.Pan(delta.x   * panSpeed);
                this._camera.Tilt(-delta.y * panSpeed);
                return;
            }

            // Middle mouse: pan/tilt (external mouse fallback)
            if (mouse.middleButton.isPressed)
            {
                this._camera.Pan(delta.x   * panSpeed);
                this._camera.Tilt(-delta.y * panSpeed);
            }
        }

        // ── Event interception ─────────────────────────────────────────────

        private void HandleInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (!IsStateEvent(eventPtr))
                return;

            if (device is Keyboard keyboard)
            {
                this.UpdateModifierState(eventPtr, keyboard);
                return;
            }

            if (device is Mouse mouse)
                this.EnqueueScroll(eventPtr, mouse);
        }

        // ── Keyboard input ─────────────────────────────────────────────────

        private void HandleKeyboardInput(Keyboard keyboard)
        {
            if (keyboard.iKey.wasPressedThisFrame
             && !keyboard.ctrlKey.isPressed
             && !keyboard.altKey.isPressed
             && !keyboard.shiftKey.isPressed
             && this.propertiesPanel != null)
            {
                this.propertiesPanel.Toggle();
                return;
            }

            if (keyboard.aKey.wasPressedThisFrame
             && !keyboard.ctrlKey.isPressed
             && !keyboard.altKey.isPressed)
            {
                if (keyboard.shiftKey.isPressed)
                    this.cameraBehaviour.CycleAspectRatioBackward();
                else
                    this.cameraBehaviour.CycleAspectRatioForward();

                return;
            }

            if (keyboard.ctrlKey.isPressed && keyboard.rKey.wasPressedThisFrame)
            {
                this._camera.Reset();
                return;
            }

            if (keyboard.dKey.wasPressedThisFrame
             && !keyboard.ctrlKey.isPressed
             && !keyboard.altKey.isPressed
             && !keyboard.shiftKey.isPressed)
            {
                this._camera.DofEnabled = !this._camera.DofEnabled;
                return;
            }

            if (keyboard.leftBracketKey.wasPressedThisFrame)
            {
                this._camera.StepApertureWider();
                return;
            }

            if (keyboard.rightBracketKey.wasPressedThisFrame)
            {
                this._camera.StepApertureNarrower();
                return;
            }

            if (keyboard.sKey.wasPressedThisFrame
             && !keyboard.ctrlKey.isPressed
             && !keyboard.altKey.isPressed
             && !keyboard.shiftKey.isPressed)
            {
                this._camera.ShakeEnabled = !this._camera.ShakeEnabled;
                return;
            }

            // Number keys 1–9 = focal length presets
            // TODO: For zoom lenses, FocalLengths is empty so this falls through to QUICK.
            var activeLensSet = this._camera.ActiveLensSet;
            var presets       = activeLensSet != null? activeLensSet.FocalLengths : FocalLengthPresets.QUICK;

            var digitKeys = new[]
            {
                keyboard.digit1Key, keyboard.digit2Key, keyboard.digit3Key,
                keyboard.digit4Key, keyboard.digit5Key, keyboard.digit6Key,
                keyboard.digit7Key, keyboard.digit8Key, keyboard.digit9Key
            };

            var presetCount = presets.Length < digitKeys.Length? presets.Length : digitKeys.Length;

            for (var i = 0; i < presetCount; i++)
            {
                if (!digitKeys[i].wasPressedThisFrame)
                    continue;

                this._camera.SetFocalLengthPreset(presets[i]);
                break;
            }
        }

        private void HandleScrollSample(ScrollSample sample)
        {
            // --- Modifier + scroll: dolly zoom, dolly, truck, crane, roll ---

            if (sample.CommandHeld && sample.AltHeld && Mathf.Abs(sample.Y) > SCROLL_DEADZONE)
            {
                this._camera.DollyZoom(sample.Y * MovementSpeeds.DOLLY_ZOOM);
                this._lastModifierScrollTime = Time.time;
                return;
            }

            if (sample.CommandHeld && !sample.AltHeld && Mathf.Abs(sample.Y) > SCROLL_DEADZONE)
            {
                this._camera.FocusDistance = this._camera.FocusDistance + sample.Y * MovementSpeeds.FOCUS_DISTANCE;
                this._lastModifierScrollTime = Time.time;
                return;
            }

            if (sample.CtrlHeld)
            {
                if (Mathf.Abs(sample.Y) > SCROLL_DEADZONE)
                    this._camera.Dolly(sample.Y * MovementSpeeds.DOLLY);

                if (Mathf.Abs(sample.X) > SCROLL_DEADZONE)
                    this._camera.Truck(sample.X * MovementSpeeds.TRUCK);

                this._lastModifierScrollTime = Time.time;
                return;
            }

            if (sample.AltHeld && Mathf.Abs(sample.Y) > SCROLL_DEADZONE)
            {
                this._camera.Crane(sample.Y * MovementSpeeds.CRANE);
                this._lastModifierScrollTime = Time.time;
                return;
            }

            if (sample.ShiftHeld && Mathf.Abs(sample.X) > SCROLL_DEADZONE)
            {
                this._camera.Roll(sample.X * MovementSpeeds.ROLL);
                this._lastModifierScrollTime = Time.time;
                return;
            }

            // --- Unmodified scroll: focal length ---

            // Block trackpad momentum after modifier+scroll gestures.
            // Momentum events arrive with no modifier held (correctly — the key
            // is already released) but are physically part of the previous gesture.
            // Short gap since last modifier/momentum scroll = still momentum, block.
            // Long gap (>= 150ms) = new intentional gesture, allow through.
            var gap = Time.time - this._lastModifierScrollTime;

            if (gap < SCROLL_BLEED_COOLDOWN)
            {
                this._lastModifierScrollTime = Time.time;
                return;
            }

            if (Mathf.Abs(sample.Y) <= SCROLL_DEADZONE)
                return;

            var activeLensSet = this._camera.ActiveLensSet;

            if (activeLensSet != null && !activeLensSet.IsZoom)
            {
                if (sample.Y > 0)
                    this._camera.StepFocalLengthUp();
                else
                    this._camera.StepFocalLengthDown();
            }
            else
            {
                this._camera.FocalLength = this._camera.FocalLength + sample.Y * MovementSpeeds.FOCAL_LENGTH_SCROLL;
            }
        }

        // ── Scroll processing ──────────────────────────────────────────────

        private void ProcessQueuedScroll()
        {
            while (this._pendingScrollSamples.Count > 0)
                this.HandleScrollSample(this._pendingScrollSamples.Dequeue());
        }

        private void ResetModifierState()
        {
            this._leftCtrlHeld     = false;
            this._rightCtrlHeld    = false;
            this._leftAltHeld      = false;
            this._rightAltHeld     = false;
            this._leftShiftHeld    = false;
            this._rightShiftHeld   = false;
            this._leftCommandHeld  = false;
            this._rightCommandHeld = false;
        }

        // ── Modifier state tracking ────────────────────────────────────────

        private void SyncModifierState(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                this.ResetModifierState();
                return;
            }

            this._leftCtrlHeld     = keyboard.leftCtrlKey.isPressed;
            this._rightCtrlHeld    = keyboard.rightCtrlKey.isPressed;
            this._leftAltHeld      = keyboard.leftAltKey.isPressed;
            this._rightAltHeld     = keyboard.rightAltKey.isPressed;
            this._leftShiftHeld    = keyboard.leftShiftKey.isPressed;
            this._rightShiftHeld   = keyboard.rightShiftKey.isPressed;
            this._leftCommandHeld  = keyboard.leftCommandKey.isPressed;
            this._rightCommandHeld = keyboard.rightCommandKey.isPressed;
        }

        private void UpdateModifierState(InputEventPtr eventPtr, Keyboard keyboard)
        {
            TryUpdateKey(keyboard.leftCtrlKey,     eventPtr, ref this._leftCtrlHeld);
            TryUpdateKey(keyboard.rightCtrlKey,    eventPtr, ref this._rightCtrlHeld);
            TryUpdateKey(keyboard.leftAltKey,      eventPtr, ref this._leftAltHeld);
            TryUpdateKey(keyboard.rightAltKey,     eventPtr, ref this._rightAltHeld);
            TryUpdateKey(keyboard.leftShiftKey,    eventPtr, ref this._leftShiftHeld);
            TryUpdateKey(keyboard.rightShiftKey,   eventPtr, ref this._rightShiftHeld);
            TryUpdateKey(keyboard.leftCommandKey,  eventPtr, ref this._leftCommandHeld);
            TryUpdateKey(keyboard.rightCommandKey, eventPtr, ref this._rightCommandHeld);
        }

        // ── Helpers ────────────────────────────────────────────────────────
        private static bool IsStateEvent(InputEventPtr eventPtr) => eventPtr.type == StateEvent.Type || eventPtr.type == DeltaStateEvent.Type;

        private static void TryUpdateKey(KeyControl key, InputEventPtr eventPtr, ref bool held)
        {
            if (key.ReadValueFromEvent(eventPtr, out var value))
                held = value >= 0.5f;
        }

        private void OnEnable()
        {
            InputSystem.onEvent += this.HandleInputEvent;
            this.SyncModifierState(Keyboard.current);
            this._pendingScrollSamples.Clear();
        }

        private void Start() => this._camera = this.cameraBehaviour.CameraElement;

        private void Update()
        {
            if (this._camera == null)
            {
                this._pendingScrollSamples.Clear();
                return;
            }

            var keyboard = Keyboard.current;
            var mouse    = Mouse.current;

            if (keyboard == null || mouse == null)
            {
                this._pendingScrollSamples.Clear();
                return;
            }

            if (this.propertiesPanel != null && this.propertiesPanel.HasFocusedTextField)
            {
                this._pendingScrollSamples.Clear();
                return;
            }

            this.HandleKeyboardInput(keyboard);

            if (this.propertiesPanel != null && this.propertiesPanel.IsPointerOverUI)
            {
                this._pendingScrollSamples.Clear();
                return;
            }

            this.ProcessQueuedScroll();
            this.HandleDragInput(keyboard, mouse);
        }

        private void OnDisable()
        {
            InputSystem.onEvent -= this.HandleInputEvent;
            this.ResetModifierState();
            this._pendingScrollSamples.Clear();
        }


        private struct ScrollSample
        {
            public float X;
            public float Y;
            public bool  AltHeld;
            public bool  CommandHeld;
            public bool  CtrlHeld;
            public bool  ShiftHeld;
        }
    }
}