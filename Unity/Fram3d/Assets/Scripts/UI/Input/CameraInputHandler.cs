using System;
using System.Collections.Generic;
using Fram3d.Core.Cameras;
using Fram3d.Core.Input;
using Fram3d.Core.Timelines;
using Fram3d.Engine.Integration;
using Fram3d.UI.Panels;
using Fram3d.UI.Views;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using CoreTimeline = Fram3d.Core.Timelines.Timeline;
namespace Fram3d.UI.Input
{
    /// <summary>
    /// Processes scroll and drag input for camera control. Uses event-level modifier
    /// tracking to pair each scroll event with the modifier state at the time it
    /// physically occurred, preventing bleed when a modifier key-up and scroll event
    /// land in the same frame.
    ///
    /// Keyboard shortcuts are delegated to KeyboardShortcutRouter.
    /// </summary>
    public sealed class CameraInputHandler: MonoBehaviour
    {
        private const    float                    SCROLL_DEADZONE       = 0.01f;
        private readonly KeyboardShortcutRouter   _keyboardRouter       = new();
        private readonly Queue<ScrollSample>      _pendingScrollSamples = new();
        private readonly ScrollRouter             _scrollRouter         = new();
        private          CameraElement            _camera;
        private          CameraSnapshot           _cameraBeforeDrag;
        private          bool                     _isCameraDragging;
        private          bool                     _leftAltHeld;
        private          bool                     _leftCommandHeld;
        private          bool                     _leftCtrlHeld;
        private          bool                     _leftShiftHeld;
        private          bool                     _rightAltHeld;
        private          bool                     _rightCommandHeld;
        private          bool                     _rightCtrlHeld;
        private          bool                     _rightShiftHeld;
        private          CoreTimeline             _timeline;

        [SerializeField]
        private CameraBehaviour cameraBehaviour;

        [SerializeField]
        private CompositionGuideView compositionGuides;

        [SerializeField]
        private ViewCameraManager viewCameraManager;

        [SerializeField]
        private GizmoBehaviour gizmoBehaviour;

        [SerializeField]
        private PropertiesPanelView propertiesPanel;

        [SerializeField]
        private ViewLayoutView viewLayoutView;

        private Timeline.TimelineSectionView _timelineSection;

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

            var panelHasFocus     = this.propertiesPanel != null && this.propertiesPanel.HasFocusedTextField;
            var shotTrackHasFocus = this._timelineSection != null && this._timelineSection.HasFocusedTextField;

            if (panelHasFocus || shotTrackHasFocus)
            {
                this._pendingScrollSamples.Clear();
                return;
            }

            this.HandleKeyboardInput(keyboard);

            if (this.IsPointerOverBlockingUI())
            {
                this._pendingScrollSamples.Clear();
                return;
            }

            this.ProcessQueuedScroll();
            this.HandleDragInput(keyboard, mouse);
        }

        // ── Scroll action dispatch ───────────────────────────────────────

        private void ApplyScrollAction(ScrollAction action)
        {
            var before = CameraSnapshot.FromCamera(this._camera);

            if (action.Kind == ScrollActionKind.DOLLY_ZOOM)
            {
                this._camera.DollyZoom(action.Y * MovementSpeeds.DOLLY_ZOOM);
            }
            else if (action.Kind == ScrollActionKind.FOCUS_DISTANCE)
            {
                var newDistance = this._camera.FocusDistance + action.Y * MovementSpeeds.FOCUS_DISTANCE;
                this._camera.FocusDistance = newDistance;
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
                this.ApplyFocalLengthScroll(action.Y);
            }

            var after = CameraSnapshot.FromCamera(this._camera);

            if (!this.IsDirectorView())
            {
                this._timeline?.RecordCameraManipulation(after, before);
            }
        }

        private void ApplyFocalLengthScroll(float scrollY)
        {
            if (Math.Abs(scrollY) <= SCROLL_DEADZONE)
            {
                return;
            }

            var activeLensSet = this._camera.ActiveLensSet;

            if (activeLensSet != null && !activeLensSet.IsZoom)
            {
                if (scrollY > 0)
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
                var newFocal = this._camera.FocalLength + scrollY * MovementSpeeds.FOCAL_LENGTH_SCROLL;
                this._camera.FocalLength = newFocal;
            }
        }

        // ── Event interception ───────────────────────────────────────────

        private void EnqueueScroll(InputEventPtr eventPtr, Mouse mouse)
        {
            if (!mouse.scroll.ReadValueFromEvent(eventPtr, out var scroll))
            {
                return;
            }

            var absX = Mathf.Abs(scroll.x);
            var absY = Mathf.Abs(scroll.y);

            if (absX <= SCROLL_DEADZONE && absY <= SCROLL_DEADZONE)
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
            {
                this.EnqueueScroll(eventPtr, mouse);
            }
        }

        // ── Keyboard routing ─────────────────────────────────────────────

        private void HandleKeyboardInput(Keyboard keyboard)
        {
            if (this._keyboardRouter.Route(keyboard, this._camera))
            {
                return;
            }

            this.HandleViewToggle(keyboard);
            this.HandlePanelToggle(keyboard);
        }

        private void HandlePanelToggle(Keyboard keyboard)
        {
            if (!keyboard.iKey.wasPressedThisFrame
             || keyboard.ctrlKey.isPressed
             || keyboard.altKey.isPressed
             || keyboard.shiftKey.isPressed
             || this.propertiesPanel == null)
            {
                return;
            }

            this.propertiesPanel.Toggle();
        }

        private void HandleViewToggle(Keyboard keyboard)
        {
            if (!keyboard.dKey.wasPressedThisFrame
             || keyboard.ctrlKey.isPressed
             || keyboard.altKey.isPressed
             || keyboard.shiftKey.isPressed)
            {
                return;
            }

            if (this.cameraBehaviour == null)
            {
                return;
            }

            this.cameraBehaviour.ToggleDirectorView();
            this._camera = this.cameraBehaviour.ActiveCamera;
        }

        // ── Drag input ───────────────────────────────────────────────────

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
                if (this._isCameraDragging)
                {
                    this._isCameraDragging = false;

                    if (!this.IsDirectorView())
                    {
                        var after = CameraSnapshot.FromCamera(this._camera);
                        this._timeline?.RecordCameraManipulation(after, this._cameraBeforeDrag);
                    }
                }

                return;
            }

            if (!this._isCameraDragging)
            {
                this._isCameraDragging  = true;
                this._cameraBeforeDrag = CameraSnapshot.FromCamera(this._camera);
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

        // ── Scroll processing ────────────────────────────────────────────

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

        private void ProcessQueuedScroll()
        {
            while (this._pendingScrollSamples.Count > 0)
            {
                this.HandleScrollSample(this._pendingScrollSamples.Dequeue());
            }
        }

        // ── Modifier state tracking ──────────────────────────────────────

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

        // ── Helpers ──────────────────────────────────────────────────────

        private static bool IsStateEvent(InputEventPtr eventPtr) =>
            eventPtr.type == StateEvent.Type || eventPtr.type == DeltaStateEvent.Type;

        private static void TryUpdateKey(KeyControl key, InputEventPtr eventPtr, ref bool held)
        {
            if (key.ReadValueFromEvent(eventPtr, out var value))
            {
                held = value >= 0.5f;
            }
        }

        private bool IsDirectorView() =>
            this.cameraBehaviour != null && this.cameraBehaviour.IsDirectorView;

        private bool IsPointerOverBlockingUI()
        {
            var overPanel    = this.propertiesPanel != null && this.propertiesPanel.IsPointerOverUI;
            var overLayout   = this.viewLayoutView  != null && this.viewLayoutView.IsPointerOverUI;
            var overShotTrack = this._timelineSection != null && this._timelineSection.IsPointerOverUI;
            return overPanel || overLayout || overShotTrack;
        }

        // ── Unity lifecycle ──────────────────────────────────────────────

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

        private void OnEnable()
        {
            InputSystem.onEvent += this.HandleInputEvent;
            this.SyncModifierState(Keyboard.current);
            this._pendingScrollSamples.Clear();
        }

        private void Start()
        {
            this._camera         = this.cameraBehaviour.CameraElement;
            this.propertiesPanel ??= FindAnyObjectByType<PropertiesPanelView>();
            this.viewLayoutView  ??= FindAnyObjectByType<ViewLayoutView>();
            this._timelineSection = FindAnyObjectByType<Timeline.TimelineSectionView>();

            var shotEvaluator = FindAnyObjectByType<ShotEvaluator>();

            if (shotEvaluator != null)
            {
                this._timeline = shotEvaluator.Controller;
            }

            if (this.gizmoBehaviour != null && this._timeline != null)
            {
                this.gizmoBehaviour.SetTimeline(this._timeline);
            }

            this._keyboardRouter.Configure(this.cameraBehaviour,
                                            this.compositionGuides,
                                            this.gizmoBehaviour,
                                            this._timelineSection,
                                            this._timeline);
        }

        private void Update()
        {
            if (this.viewCameraManager != null)
            {
                var activeCam = this.viewCameraManager.ActiveCameraElement;

                if (activeCam != null)
                {
                    this._camera = activeCam;
                }
            }

            this.Tick(Keyboard.current, Mouse.current);
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
