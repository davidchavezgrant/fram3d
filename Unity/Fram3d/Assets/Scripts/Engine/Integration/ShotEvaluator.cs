using System;
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
    public sealed class ShotEvaluator : MonoBehaviour
    {
        private float           _bottomInsetPixels;
        private CameraBehaviour _cameraBehaviour;
        private IDisposable     _cameraEvalSub;
        private IDisposable     _currentShotSub;

        public float BottomInsetPixels => this._bottomInsetPixels;

        public Timeline Controller { get; private set; }

        public void SetBottomInset(float pixels) => this._bottomInsetPixels = pixels;

        private void Awake()
        {
            this.Controller = new Timeline(FrameRate.FPS_24);
        }

        private void OnCameraEvaluationRequested(CameraEvaluation eval)
        {
            if (this._cameraBehaviour == null)
            {
                return;
            }

            var position = eval.Shot.EvaluateCameraPosition(eval.LocalTime);
            var rotation = eval.Shot.EvaluateCameraRotation(eval.LocalTime);
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
            this._cameraEvalSub?.Dispose();
            this._currentShotSub?.Dispose();
        }

        private void Start()
        {
            this._cameraBehaviour = FindAnyObjectByType<CameraBehaviour>();

            if (this._cameraBehaviour == null)
            {
                Debug.LogWarning("ShotEvaluator: No CameraBehaviour found.");
                return;
            }

            this._currentShotSub = this.Controller.CurrentShotChanged
                .Subscribe(this.OnCurrentShotChanged);

            this._cameraEvalSub = this.Controller.CameraEvaluationRequested
                .Subscribe(this.OnCameraEvaluationRequested);

            var cam = this._cameraBehaviour.ShotCamera;
            this.Controller.AddShot(cam.Position, cam.Rotation);
        }
    }
}
