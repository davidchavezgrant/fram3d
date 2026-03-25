using System;
using System.Collections.Generic;
using Fram3d.Core.Camera;
using Fram3d.Core.Input;
using Fram3d.Core.Scene;
using Fram3d.Core.Viewport;
using Fram3d.Engine.Integration;
using Fram3d.UI.Panels;
using Fram3d.UI.Views;
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
        private const    float               SCROLL_DEADZONE       = 0.01f;
        private readonly Queue<ScrollSample> _pendingScrollSamples = new();
        private readonly ScrollRouter        _scrollRouter         = new();
        private          CameraElement       _camera;
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
        private CompositionGuideView compositionGuides;

        [SerializeField]
        private ViewCameraManager viewCameraManager;

        [SerializeField]
        private GizmoController gizmoController;

        [SerializeField]
        private PropertiesPanelView propertiesPanel;

        public void Tick(Keyboard keyboard, Mouse mouse)
        {
            if (this._camera == null)
            {
                this._pendingScrollSamples.Clear();
                return;
            }

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

        private void ApplyScrollAction(ScrollAction action)
        {
            if (action.Kind == ScrollActionKind.DOLLY_ZOOM)
            {
                this._camera.DollyZoom(action.Y * MovementSpeeds.DOLLY_ZOOM);
            }
            else if (action.Kind == ScrollActionKind.FOCUS_DISTANCE)
            {
                this._camera.FocusDistance = this._camera.FocusDistance + action.Y * MovementSpeeds.FOCUS_DISTANCE;
            }
            else if (action.Kind == ScrollActionKind.DOLLY_TRUCK)
            {
                if (Math.Abs(action.Y) > SCROLL_DEADZONE)
                {
                    this._camera.Dolly(action.Y * MovementSpeeds.DOLLY);
                }

                if (Math.Abs(action.X) > SCROLL_DEADZONE)
                {
                    this._camera.Truck(action.X * MovementSpeeds.TRUCK);
                }
            }
            else if (action.Kind == ScrollActionKind.CRANE)
            {
                this._camera.Crane(action.Y * MovementSpeeds.CRANE);
            }
            else if (action.Kind == ScrollActionKind.ROLL)
            {
                this._camera.Roll(action.X * MovementSpeeds.ROLL);
            }
            else if (action.Kind == ScrollActionKind.FOCAL_LENGTH)
            {
                if (Math.Abs(action.Y) <= SCROLL_DEADZONE)
                {
                    return;
                }

                var activeLensSet = this._camera.ActiveLensSet;

                if (activeLensSet != null && !activeLensSet.IsZoom)
                {
                    if (action.Y > 0)
                    {
                        this._camera.StepFocalLengthUp();
                    }
                    else
                    {
                        this._camera.StepFocalLengthDown();
                    }
                }
                else
                {
                    this._camera.FocalLength = this._camera.FocalLength + action.Y * MovementSpeeds.FOCAL_LENGTH_SCROLL;
                }
            }
        }

        private void EnqueueScroll(InputEventPtr eventPtr, Mouse mouse)
        {
            if (!mouse.scroll.ReadValueFromEvent(eventPtr, out var scroll))
            {
                return;
            }

            if (Mathf.Abs(scroll.x) <= SCROLL_DEADZONE && Mathf.Abs(scroll.y) <= SCROLL_DEADZONE)
            {
                return;
            }

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

        // ── Keyboard input ─────────────────────────────────────────────────

        private bool HandleAspectRatio(Keyboard keyboard)
        {
            if (!keyboard.aKey.wasPressedThisFrame || keyboard.ctrlKey.isPressed || keyboard.altKey.isPressed)
            {
                return false;
            }

            if (keyboard.shiftKey.isPressed)
            {
                this.cameraBehaviour.CycleAspectRatioBackward();
            }
            else
            {
                this.cameraBehaviour.CycleAspectRatioForward();
            }

            return true;
        }

        // ── Drag input ─────────────────────────────────────────────────────

        private void HandleDragInput(Keyboard keyboard, Mouse mouse)
        {
            var delta = mouse.delta.ReadValue();

            var action = DragRouter.Route(delta.x,
                                          delta.y,
                                          keyboard.altKey.isPressed,
                                          keyboard.leftCommandKey.isPressed || keyboard.rightCommandKey.isPressed,
                                          mouse.leftButton.isPressed,
                                          mouse.middleButton.isPressed);

            if (action.Kind == DragActionKind.NONE)
            {
                return;
            }

            var dt       = Time.deltaTime;
            var panSpeed = MovementSpeeds.PAN_TILT * dt;

            if (action.Kind == DragActionKind.ORBIT)
            {
                this._camera.Orbit(action.DeltaX * panSpeed, action.DeltaY * panSpeed);
            }
            else if (action.Kind == DragActionKind.PAN_TILT)
            {
                this._camera.Pan(action.DeltaX   * panSpeed);
                this._camera.Tilt(-action.DeltaY * panSpeed);
            }
        }

        private void HandleFocalLengthPresets(Keyboard keyboard)
        {
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
                {
                    continue;
                }

                this._camera.SetFocalLengthPreset(presets[i]);
                break;
            }
        }

        private bool HandleGuideShortcuts(Keyboard keyboard)
        {
            if (!keyboard.gKey.wasPressedThisFrame || this.compositionGuides == null)
            {
                return false;
            }

            if (!keyboard.ctrlKey.isPressed && !keyboard.altKey.isPressed && !keyboard.shiftKey.isPressed)
            {
                this.compositionGuides.Settings.ToggleAll();
                return true;
            }

            if (keyboard.shiftKey.isPressed && !keyboard.ctrlKey.isPressed && !keyboard.altKey.isPressed)
            {
                this.compositionGuides.Settings.ToggleThirds();
                return true;
            }

            if (keyboard.ctrlKey.isPressed && !keyboard.altKey.isPressed && !keyboard.shiftKey.isPressed)
            {
                this.compositionGuides.Settings.ToggleCenterCross();
                return true;
            }

            if (keyboard.altKey.isPressed && !keyboard.ctrlKey.isPressed && !keyboard.shiftKey.isPressed)
            {
                this.compositionGuides.Settings.ToggleSafeZones();
                return true;
            }

            return false;
        }

        // ── Event interception ─────────────────────────────────────────────

        private void HandleInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (!IsStateEvent(eventPtr))
            {
                return;
            }

            if (device is Keyboard keyboard)
            {
                this.UpdateModifierState(eventPtr, keyboard);
                return;
            }

            if (device is Mouse mouse)
                this.EnqueueScroll(eventPtr, mouse);
        }

        private void HandleKeyboardInput(Keyboard keyboard)
        {
            if (this.HandleToolSwitching(keyboard)) { return; }

            if (this.HandlePanelToggle(keyboard)) { return; }

            if (this.HandleViewToggle(keyboard)) { return; }

            if (this.HandleAspectRatio(keyboard)) { return; }

            if (this.HandleReset(keyboard)) { return; }

            if (this.HandleToggles(keyboard)) { return; }

            if (this.HandleGuideShortcuts(keyboard)) { return; }

            this.HandleFocalLengthPresets(keyboard);
        }

        private bool HandlePanelToggle(Keyboard keyboard)
        {
            if (!keyboard.iKey.wasPressedThisFrame
             || keyboard.ctrlKey.isPressed
             || keyboard.altKey.isPressed
             || keyboard.shiftKey.isPressed
             || this.propertiesPanel == null)
            {
                return false;
            }

            this.propertiesPanel.Toggle();
            return true;
        }

        private bool HandleReset(Keyboard keyboard)
        {
            if (!keyboard.ctrlKey.isPressed || !keyboard.rKey.wasPressedThisFrame)
            {
                return false;
            }

            // If a gizmo tool is active on a selected element, reset that
            // tool's property. Otherwise fall through to camera reset.
            if (this.gizmoController != null && this.gizmoController.TryResetActiveTool())
            {
                return true;
            }

            this._camera.Reset();
            return true;
        }

        private void HandleScrollSample(ScrollSample sample)
        {
            var action = this._scrollRouter.Route(sample.X,
                                                  sample.Y,
                                                  sample.CtrlHeld,
                                                  sample.AltHeld,
                                                  sample.ShiftHeld,
                                                  sample.CommandHeld,
                                                  Time.time);

            this.ApplyScrollAction(action);
        }

        private bool HandleToggles(Keyboard keyboard)
        {
            if (keyboard.dKey.wasPressedThisFrame && keyboard.shiftKey.isPressed && !keyboard.ctrlKey.isPressed && !keyboard.altKey.isPressed)
            {
                this._camera.DofEnabled = !this._camera.DofEnabled;
                return true;
            }

            if (keyboard.leftBracketKey.wasPressedThisFrame)
            {
                this._camera.StepApertureWider();
                return true;
            }

            if (keyboard.rightBracketKey.wasPressedThisFrame)
            {
                this._camera.StepApertureNarrower();
                return true;
            }

            if (keyboard.sKey.wasPressedThisFrame && !keyboard.ctrlKey.isPressed && !keyboard.altKey.isPressed && !keyboard.shiftKey.isPressed)
            {
                this._camera.ShakeEnabled = !this._camera.ShakeEnabled;
                return true;
            }

            return false;
        }

        private bool HandleViewToggle(Keyboard keyboard)
        {
            if (!keyboard.dKey.wasPressedThisFrame || keyboard.ctrlKey.isPressed || keyboard.altKey.isPressed || keyboard.shiftKey.isPressed)
            {
                return false;
            }

            if (this.cameraBehaviour == null)
            {
                return false;
            }

            this.cameraBehaviour.ToggleDirectorView();
            this._camera = this.cameraBehaviour.ActiveCamera;
            return true;
        }

        private bool HandleToolSwitching(Keyboard keyboard)
        {
            if (this.gizmoController == null || keyboard.ctrlKey.isPressed || keyboard.altKey.isPressed || keyboard.shiftKey.isPressed)
            {
                return false;
            }

            if (keyboard.qKey.wasPressedThisFrame)
            {
                this.gizmoController.SetActiveTool(ActiveTool.SELECT);
                return true;
            }

            if (keyboard.wKey.wasPressedThisFrame)
            {
                this.gizmoController.SetActiveTool(ActiveTool.TRANSLATE);
                return true;
            }

            if (keyboard.eKey.wasPressedThisFrame)
            {
                this.gizmoController.SetActiveTool(ActiveTool.ROTATE);
                return true;
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                this.gizmoController.SetActiveTool(ActiveTool.SCALE);
                return true;
            }

            return false;
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
            if (this.viewCameraManager != null && Mouse.current != null)
            {
                // Activate the clicked viewport on mouse-down
                if (this.viewCameraManager.IsMultiView && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    this.viewCameraManager.ActivateSlotAtPosition(Mouse.current.position.ReadValue());
                }

                // Route camera movement to the active slot's camera element
                var activeCam = this.viewCameraManager.ActiveCameraElement;

                if (activeCam != null)
                {
                    this._camera = activeCam;
                }
            }

            this.Tick(Keyboard.current, Mouse.current);
        }

        /// <summary>
        /// Resyncs modifier state when the application regains focus.
        /// Without this, event-tracked modifier booleans can get stuck
        /// if the user releases a modifier key while another app is focused
        /// — Unity never sees the key-up event. This is the root cause of
        /// the "Ctrl+scroll mode flip" bug (FRA-127).
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                this.SyncModifierState(Keyboard.current);
                this._pendingScrollSamples.Clear();
            }
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