using System;
using Fram3d.Core.Cameras;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
using Fram3d.Core.Timelines;
using Fram3d.Engine.Conversion;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Bridges the Timeline to the Unity scene. Creates a default
    /// shot on start. Routes camera evaluation requests to CameraBehaviour.
    /// </summary>
    public sealed class ShotEvaluator: MonoBehaviour
    {
        private float           _bottomInsetPixels;
        private CameraBehaviour _cameraBehaviour;
        private IDisposable     _cameraEvalSub;
        private IDisposable     _currentShotSub;
        private IDisposable     _elementEvalSub;
        public  float           BottomInsetPixels            => this._bottomInsetPixels;
        public  Timeline        Controller                   { get; private set; }
        public  void            SetBottomInset(float pixels) => this._bottomInsetPixels = pixels;

        private void ApplyCameraProperties(Shot shot, TimePosition time, CameraElement cam)
        {
            if (shot.CameraFocalLengthKeyframes.Count > 0)
            {
                cam.FocalLength     = shot.EvaluateCameraFocalLength(time);
                cam.SnapFocalLength = true;
            }

            if (shot.CameraApertureKeyframes.Count > 0)
            {
                cam.Aperture = shot.EvaluateCameraAperture(time);
            }

            if (shot.CameraFocusDistanceKeyframes.Count > 0)
            {
                cam.FocusDistance = shot.EvaluateCameraFocusDistance(time);
            }
        }

        private void OnCameraEvaluationRequested(CameraEvaluation eval)
        {
            if (this._cameraBehaviour == null)
            {
                return;
            }

            var cam = this._cameraBehaviour.ShotCamera;
            cam.Position = eval.Shot.EvaluateCameraPosition(eval.LocalTime);
            cam.Rotation = eval.Shot.EvaluateCameraRotation(eval.LocalTime);
            this.ApplyCameraProperties(eval.Shot, eval.LocalTime, cam);
        }

        private void OnCurrentShotChanged(Shot shot)
        {
            if (shot == null || this._cameraBehaviour == null)
            {
                return;
            }

            var cam = this._cameraBehaviour.ShotCamera;
            cam.Position = shot.EvaluateCameraPosition(TimePosition.ZERO);
            cam.Rotation = shot.EvaluateCameraRotation(TimePosition.ZERO);
            this.ApplyCameraProperties(shot, TimePosition.ZERO, cam);
        }

        private void OnElementEvaluationRequested(ElementEvaluation eval)
        {
            var elements = FindObjectsByType<ElementBehaviour>(FindObjectsSortMode.None);

            foreach (var elementBehaviour in elements)
            {
                var element = elementBehaviour.Element;

                if (element == null)
                {
                    continue;
                }

                var track = this.Controller.Elements.GetTrack(element.Id);

                if (track == null || !track.HasKeyframes)
                {
                    continue;
                }

                element.Position = track.EvaluatePosition(eval.GlobalTime);
                element.Rotation = track.EvaluateRotation(eval.GlobalTime);

                if (track.ScaleKeyframes.Count > 0)
                {
                    element.Scale = track.EvaluateScale(eval.GlobalTime);
                }
            }
        }

        private void Awake()
        {
            this.Controller = new Timeline(FrameRate.FPS_24);
        }

        private void Start()
        {
            this._cameraBehaviour = FindAnyObjectByType<CameraBehaviour>();

            if (this._cameraBehaviour == null)
            {
                Debug.LogWarning("ShotEvaluator: No CameraBehaviour found.");
                return;
            }

            this._currentShotSub = this.Controller.CurrentShotChanged.Subscribe(this.OnCurrentShotChanged);
            this._cameraEvalSub  = this.Controller.CameraEvaluationRequested.Subscribe(this.OnCameraEvaluationRequested);
            this._elementEvalSub = this.Controller.ElementEvaluationRequested.Subscribe(this.OnElementEvaluationRequested);
            var shot = this.Controller.AddShot();
            var cam  = this._cameraBehaviour.ShotCamera;
            shot.DefaultCameraPosition = cam.Position;
            shot.DefaultCameraRotation = cam.Rotation;
        }

        private void OnDestroy()
        {
            this._cameraEvalSub?.Dispose();
            this._currentShotSub?.Dispose();
            this._elementEvalSub?.Dispose();
        }
    }
}