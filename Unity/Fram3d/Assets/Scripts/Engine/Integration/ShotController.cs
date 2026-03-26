using System;
using Fram3d.Core.Common;
using Fram3d.Core.Shot;
using Fram3d.Engine.Conversion;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Owns the ShotRegistry and bridges it to the Unity scene.
    /// Creates a default shot on start. When the current shot changes,
    /// evaluates the camera at t=0 of the new shot.
    /// </summary>
    public sealed class ShotController : MonoBehaviour
    {
        private float       _bottomInsetPixels;
        private CameraBehaviour _cameraBehaviour;
        private IDisposable _currentShotSub;

        public float BottomInsetPixels => this._bottomInsetPixels;

        public ShotRegistry Registry { get; private set; }

        /// <summary>
        /// Adds a new shot capturing the current camera position and rotation.
        /// </summary>
        public Shot AddShot()
        {
            var cam = this._cameraBehaviour.ShotCamera;
            return this.Registry.AddShot(cam.Position, cam.Rotation);
        }

        public void SetBottomInset(float pixels) => this._bottomInsetPixels = pixels;

        private void Awake()
        {
            this.Registry = new ShotRegistry();
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
        }

        private void Start()
        {
            this._cameraBehaviour = FindAnyObjectByType<CameraBehaviour>();

            if (this._cameraBehaviour == null)
            {
                Debug.LogWarning("ShotController: No CameraBehaviour found.");
                return;
            }

            this._currentShotSub = this.Registry.CurrentShotChanged
                .Subscribe(this.OnCurrentShotChanged);

            // Default state: one shot capturing the current camera
            this.AddShot();
        }
    }
}
