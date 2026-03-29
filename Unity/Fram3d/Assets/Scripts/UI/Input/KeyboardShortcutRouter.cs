using Fram3d.Core.Cameras;
using Fram3d.Core.Scenes;
using Fram3d.Core.Timelines;
using Fram3d.Core.Viewports;
using Fram3d.Engine.Integration;
using Fram3d.UI.Timeline;
using Fram3d.UI.Views;
using UnityEngine.InputSystem;
using CoreTimeline = Fram3d.Core.Timelines.Timeline;
namespace Fram3d.UI.Input
{
    /// <summary>
    /// Routes keyboard shortcuts to camera, gizmo, overlay, and panel actions.
    /// Pure routing — no state, no scroll handling, no modifier tracking.
    /// </summary>
    public sealed class KeyboardShortcutRouter
    {
        private CameraBehaviour      _cameraBehaviour;
        private CompositionGuideView _compositionGuides;
        private GizmoBehaviour      _gizmoController;
        private CoreTimeline         _timeline;
        private TimelineSectionView  _timelineSection;

        public void Configure(CameraBehaviour      cameraBehaviour,
                              CompositionGuideView compositionGuides,
                              GizmoBehaviour      gizmoController,
                              TimelineSectionView  timelineSection,
                              CoreTimeline         timeline)
        {
            this._cameraBehaviour  = cameraBehaviour;
            this._compositionGuides = compositionGuides;
            this._gizmoController  = gizmoController;
            this._timeline         = timeline;
            this._timelineSection  = timelineSection;
        }

        /// <summary>
        /// Processes keyboard shortcuts for the given camera.
        /// Returns true if a shortcut was handled (early-exit chain).
        /// </summary>
        public bool Route(Keyboard keyboard, CameraElement camera)
        {
            if (this.HandleKeyframeShortcuts(keyboard, camera)) { return true; }

            if (this.HandleToolSwitching(keyboard)) { return true; }

            if (this.HandleAspectRatio(keyboard)) { return true; }

            if (this.HandleReset(keyboard, camera)) { return true; }

            if (this.HandleToggles(keyboard, camera)) { return true; }

            if (this.HandleGuideShortcuts(keyboard)) { return true; }

            this.HandleFocalLengthPresets(keyboard, camera);
            return false;
        }

        private bool CanRecord() =>
            this._timeline != null
            && (this._cameraBehaviour == null || !this._cameraBehaviour.IsDirectorView);

        private bool HandleAspectRatio(Keyboard keyboard)
        {
            if (!keyboard.aKey.wasPressedThisFrame
             || keyboard.ctrlKey.isPressed
             || keyboard.altKey.isPressed)
            {
                return false;
            }

            if (this._cameraBehaviour == null)
            {
                return false;
            }

            if (keyboard.shiftKey.isPressed)
            {
                this._cameraBehaviour.CycleAspectRatioBackward();
            }
            else
            {
                this._cameraBehaviour.CycleAspectRatioForward();
            }

            return true;
        }

        private void HandleFocalLengthPresets(Keyboard keyboard, CameraElement camera)
        {
            var activeLensSet = camera.ActiveLensSet;
            var presets       = activeLensSet != null
                ? activeLensSet.FocalLengths
                : FocalLengthPresets.QUICK;

            var digitKeys = new[]
            {
                keyboard.digit1Key, keyboard.digit2Key, keyboard.digit3Key,
                keyboard.digit4Key, keyboard.digit5Key, keyboard.digit6Key,
                keyboard.digit7Key, keyboard.digit8Key, keyboard.digit9Key
            };

            var presetCount = presets.Length < digitKeys.Length
                ? presets.Length
                : digitKeys.Length;

            for (var i = 0; i < presetCount; i++)
            {
                if (!digitKeys[i].wasPressedThisFrame)
                {
                    continue;
                }

                var before = CameraSnapshot.FromCamera(camera);
                camera.SetFocalLengthPreset(presets[i]);
                var after = CameraSnapshot.FromCamera(camera);
                if (this.CanRecord())
            {
                this._timeline.RecordCameraManipulation(after, before);
            }
                break;
            }
        }

        private bool HandleGuideShortcuts(Keyboard keyboard)
        {
            if (!keyboard.gKey.wasPressedThisFrame || this._compositionGuides == null)
            {
                return false;
            }

            var ctrl  = keyboard.ctrlKey.isPressed;
            var alt   = keyboard.altKey.isPressed;
            var shift = keyboard.shiftKey.isPressed;

            if (!ctrl && !alt && !shift)
            {
                this._compositionGuides.Settings.ToggleAll();
                return true;
            }

            if (shift && !ctrl && !alt)
            {
                this._compositionGuides.Settings.ToggleThirds();
                return true;
            }

            if (ctrl && !alt && !shift)
            {
                this._compositionGuides.Settings.ToggleCenterCross();
                return true;
            }

            if (alt && !ctrl && !shift)
            {
                this._compositionGuides.Settings.ToggleSafeZones();
                return true;
            }

            return false;
        }

        private bool HandleKeyframeShortcuts(Keyboard keyboard, CameraElement camera)
        {
            if (keyboard.ctrlKey.isPressed
             || keyboard.altKey.isPressed
             || keyboard.shiftKey.isPressed)
            {
                return false;
            }

            if (keyboard.cKey.wasPressedThisFrame && this.CanRecord())
            {
                var snap = CameraSnapshot.FromCamera(camera);
                this._timeline.ForceRecordCamera(snap);
                return true;
            }

            return false;
        }

        private bool HandleReset(Keyboard keyboard, CameraElement camera)
        {
            if (!keyboard.ctrlKey.isPressed || !keyboard.rKey.wasPressedThisFrame)
            {
                return false;
            }

            if (this._gizmoController != null && this._gizmoController.TryResetActiveTool())
            {
                return true;
            }

            var before = CameraSnapshot.FromCamera(camera);
            camera.Reset();
            var after = CameraSnapshot.FromCamera(camera);
            if (this.CanRecord())
            {
                this._timeline.RecordCameraManipulation(after, before);
            }
            return true;
        }

        private bool HandleToggles(Keyboard keyboard, CameraElement camera)
        {
            var ctrl  = keyboard.ctrlKey.isPressed;
            var alt   = keyboard.altKey.isPressed;
            var shift = keyboard.shiftKey.isPressed;

            if (keyboard.dKey.wasPressedThisFrame && shift && !ctrl && !alt)
            {
                camera.DofEnabled = !camera.DofEnabled;
                return true;
            }

            if (keyboard.leftBracketKey.wasPressedThisFrame)
            {
                var before = CameraSnapshot.FromCamera(camera);
                camera.StepApertureWider();
                var after = CameraSnapshot.FromCamera(camera);
                if (this.CanRecord())
            {
                this._timeline.RecordCameraManipulation(after, before);
            }
                return true;
            }

            if (keyboard.rightBracketKey.wasPressedThisFrame)
            {
                var before = CameraSnapshot.FromCamera(camera);
                camera.StepApertureNarrower();
                var after = CameraSnapshot.FromCamera(camera);
                if (this.CanRecord())
            {
                this._timeline.RecordCameraManipulation(after, before);
            }
                return true;
            }

            if (keyboard.sKey.wasPressedThisFrame && !ctrl && !alt && !shift)
            {
                camera.ShakeEnabled = !camera.ShakeEnabled;
                return true;
            }

            if (keyboard.tKey.wasPressedThisFrame && !ctrl && !alt && !shift)
            {
                if (this._timelineSection != null)
                {
                    this._timelineSection.Toggle();
                }

                return true;
            }

            if (keyboard.spaceKey.wasPressedThisFrame && !ctrl && !alt && !shift)
            {
                if (this._timelineSection != null)
                {
                    this._timelineSection.TogglePlayback();
                }

                return true;
            }

            if (keyboard.equalsKey.wasPressedThisFrame && !alt && !shift)
            {
                if (this._timelineSection != null)
                {
                    this._timelineSection.ZoomIn();
                }

                return true;
            }

            if (keyboard.minusKey.wasPressedThisFrame && !alt && !shift)
            {
                if (this._timelineSection != null)
                {
                    this._timelineSection.ZoomOut();
                }

                return true;
            }

            if (keyboard.backslashKey.wasPressedThisFrame && !ctrl && !alt && !shift)
            {
                if (this._timelineSection != null)
                {
                    this._timelineSection.FitAll();
                }

                return true;
            }

            if (keyboard.homeKey.wasPressedThisFrame && !ctrl && !alt && !shift)
            {
                if (this._timelineSection != null)
                {
                    this._timelineSection.JumpToStart();
                }

                return true;
            }

            if (keyboard.endKey.wasPressedThisFrame && !ctrl && !alt && !shift)
            {
                if (this._timelineSection != null)
                {
                    this._timelineSection.JumpToEnd();
                }

                return true;
            }

            return false;
        }

        private bool HandleToolSwitching(Keyboard keyboard)
        {
            if (this._gizmoController == null
             || keyboard.ctrlKey.isPressed
             || keyboard.altKey.isPressed
             || keyboard.shiftKey.isPressed)
            {
                return false;
            }

            if (keyboard.qKey.wasPressedThisFrame)
            {
                this._gizmoController.SetActiveTool(ActiveTool.SELECT);
                return true;
            }

            if (keyboard.wKey.wasPressedThisFrame)
            {
                this._gizmoController.SetActiveTool(ActiveTool.TRANSLATE);
                return true;
            }

            if (keyboard.eKey.wasPressedThisFrame)
            {
                this._gizmoController.SetActiveTool(ActiveTool.ROTATE);
                return true;
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                this._gizmoController.SetActiveTool(ActiveTool.SCALE);
                return true;
            }

            return false;
        }
    }
}
