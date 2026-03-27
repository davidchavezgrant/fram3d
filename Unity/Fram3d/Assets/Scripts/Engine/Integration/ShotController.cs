using System;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
using Fram3d.Core.Timeline;
using Fram3d.Engine.Conversion;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Bridges the TimelineController to the Unity scene. Creates a default
    /// shot on start. Routes camera evaluation requests to CameraBehaviour.
    /// </summary>
    public sealed class ShotController : MonoBehaviour
    {
        private float              _bottomInsetPixels;
        private CameraBehaviour    _cameraBehaviour;
        private IDisposable        _currentShotSub;

        public float BottomInsetPixels => this._bottomInsetPixels;

        public TimelineController Controller { get; private set; }

        public void SetBottomInset(float pixels) => this._bottomInsetPixels = pixels;

        private void Awake()
        {
            this.Controller = new TimelineController(FrameRate.FPS_24);
        }

        private void OnCameraEvaluationRequested(Shot shot, TimePosition localTime)
        {
            if (this._cameraBehaviour == null)
            {
                return;
            }

            var position = shot.EvaluateCameraPosition(localTime);
            var rotation = shot.EvaluateCameraRotation(localTime);
            this._cameraBehaviour.ShotCamera.Position = position;
            this._cameraBehaviour.ShotCamera.Rotation = rotation;
        }

        private void OnCurrentShotChanged(Shot shot)
        {
            if (shot == null || this._cameraBehaviour == null)
            {
                return;
            }

            var position = shot.EvaluateCameraPosition(TimePosition.ZERO);
            var rotation = shot.EvaluateCameraRotation(TimePosition.ZERO);
            this._cameraBehaviour.ShotCamera.Position = position;
            this._cameraBehaviour.ShotCamera.Rotation = rotation;
        }

        private void OnDestroy()
        {
            this._currentShotSub?.Dispose();
            this.Controller.CameraEvaluationRequested -= this.OnCameraEvaluationRequested;
        }

        private void Start()
        {
            this._cameraBehaviour = FindAnyObjectByType<CameraBehaviour>();

            if (this._cameraBehaviour == null)
            {
                Debug.LogWarning("ShotController: No CameraBehaviour found.");
                return;
            }

            this._currentShotSub = this.Controller.Track.CurrentShotChanged
                .Subscribe(this.OnCurrentShotChanged);

            this.Controller.CameraEvaluationRequested += this.OnCameraEvaluationRequested;

            // Default state: one shot capturing the current camera
            var cam = this._cameraBehaviour.ShotCamera;
            this.Controller.AddShot(cam.Position, cam.Rotation);
        }
    }
}
