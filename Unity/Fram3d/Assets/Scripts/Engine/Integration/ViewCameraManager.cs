using System.Collections.Generic;
using Fram3d.Core.Camera;
using Fram3d.Core.Common;
using Fram3d.Core.Scene;
using Fram3d.Engine.Conversion;
using UnityEngine;

namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Manages per-slot view cameras. The Camera View slot delegates to
    /// CameraBehaviour (which owns the shot camera element, DOF, shake).
    /// Director View slots get separate cameras with simple sync.
    /// Designer View slots are placeholders (no camera).
    /// </summary>
    public sealed class ViewCameraManager : MonoBehaviour
    {
        private int                       _activeSlot;
        private Dictionary<int, ViewCamera> _directorCameras = new();
        private RenderTexture             _cameraViewRT;

        [SerializeField]
        private CameraBehaviour cameraBehaviour;

        public int            ActiveSlot    => this._activeSlot;
        public CameraBehaviour CameraBehaviour => this.cameraBehaviour;
        public ViewSlotModel  ViewSlotModel { get; private set; }

        /// <summary>
        /// The camera element for the given slot. Camera View → shot camera,
        /// Director View → director camera, Designer View → null.
        /// </summary>
        public CameraElement GetCameraElement(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= this.ViewSlotModel.ActiveSlotCount)
            {
                return this.cameraBehaviour.ShotCamera;
            }

            var mode = this.ViewSlotModel.GetSlotType(slotIndex);

            if (mode == ViewMode.CAMERA)
            {
                return this.cameraBehaviour.ShotCamera;
            }

            if (mode == ViewMode.DIRECTOR)
            {
                return this.cameraBehaviour.DirectorCamera;
            }

            return null;
        }

        /// <summary>
        /// The Unity Camera for the given slot. Used for raycasting.
        /// </summary>
        public Camera GetUnityCamera(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= this.ViewSlotModel.ActiveSlotCount)
            {
                return null;
            }

            var mode = this.ViewSlotModel.GetSlotType(slotIndex);

            if (mode == ViewMode.CAMERA)
            {
                return this.cameraBehaviour.GetComponent<Camera>();
            }

            if (mode == ViewMode.DIRECTOR)
            {
                return this.FindDirectorCamera(slotIndex)?.UnityCamera;
            }

            return null;
        }

        /// <summary>
        /// The RenderTexture for the given slot. Used by UI to display.
        /// </summary>
        public RenderTexture GetRenderTexture(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= this.ViewSlotModel.ActiveSlotCount)
            {
                return null;
            }

            var mode = this.ViewSlotModel.GetSlotType(slotIndex);

            if (mode == ViewMode.CAMERA)
            {
                return this._cameraViewRT;
            }

            if (mode == ViewMode.DIRECTOR)
            {
                return this.FindDirectorCamera(slotIndex)?.RenderTexture;
            }

            return null;
        }

        /// <summary>
        /// Resize the RenderTexture for the given slot.
        /// </summary>
        public void ResizeSlot(int slotIndex, int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var mode = this.ViewSlotModel.GetSlotType(slotIndex);

            if (mode == ViewMode.CAMERA)
            {
                this.EnsureCameraViewRT(width, height);
                return;
            }

            if (mode == ViewMode.DIRECTOR)
            {
                this.FindDirectorCamera(slotIndex)?.EnsureRenderTexture(width, height);
            }
        }

        /// <summary>
        /// Sets the active (hovered) slot for input routing.
        /// </summary>
        public void SetActiveSlot(int slotIndex)
        {
            this._activeSlot = slotIndex;
        }

        /// <summary>
        /// The camera element for the currently active (hovered) slot.
        /// </summary>
        public CameraElement ActiveCameraElement => this.GetCameraElement(this._activeSlot);

        /// <summary>
        /// The Unity Camera for the currently active slot.
        /// </summary>
        public Camera ActiveUnityCamera => this.GetUnityCamera(this._activeSlot);

        /// <summary>
        /// True if any visible slot shows Camera View.
        /// </summary>
        public bool HasCameraViewSlot
        {
            get
            {
                var count = this.ViewSlotModel.ActiveSlotCount;

                for (var i = 0; i < count; i++)
                {
                    if (this.ViewSlotModel.GetSlotType(i) == ViewMode.CAMERA)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private void Awake()
        {
            this.ViewSlotModel          =  new ViewSlotModel();
            this.ViewSlotModel.Changed += this.RebuildCameras;
        }

        private void Start()
        {
            if (this.cameraBehaviour == null)
            {
                this.cameraBehaviour = FindAnyObjectByType<CameraBehaviour>();
            }

            if (this.cameraBehaviour != null)
            {
                this.cameraBehaviour.ExternalSyncEnabled = true;
            }

            this.RebuildCameras();
        }

        private void LateUpdate()
        {
            if (this.cameraBehaviour == null)
            {
                return;
            }

            // Sync Camera View: CameraBehaviour's camera renders to the CameraView RT
            this.SyncCameraViewSlot();

            // Sync Director View cameras
            this.SyncDirectorCameras();

            // Update frustum visibility: visible when any slot shows Director View
            this.UpdateFrustumVisibility();
        }

        private void OnDestroy()
        {
            this.DestroyDirectorCameras();
            this.DestroyCameraViewRT();

            if (this.ViewSlotModel != null)
            {
                this.ViewSlotModel.Changed -= this.RebuildCameras;
            }
        }

        private void RebuildCameras()
        {
            this.DestroyDirectorCameras();

            var count = this.ViewSlotModel.ActiveSlotCount;

            for (var i = 0; i < count; i++)
            {
                if (this.ViewSlotModel.GetSlotType(i) != ViewMode.DIRECTOR)
                {
                    continue;
                }

                this._directorCameras[i] = new ViewCamera(i, ViewMode.DIRECTOR);

                if (this.cameraBehaviour != null)
                {
                    this.cameraBehaviour.EnsureDirectorInitialized();
                }
            }

            this.UpdateFrustumVisibility();
        }

        private void SyncCameraViewSlot()
        {
            var cam               = this.cameraBehaviour.ShotCamera;
            var displayedFocal    = this.cameraBehaviour.DisplayedFocalLength;
            var unityCam          = this.cameraBehaviour.GetComponent<Camera>();
            var cameraTransform   = unityCam.transform;

            cameraTransform.position = cam.Position.ToUnity();
            cameraTransform.rotation = cam.Rotation.ToUnity();
            unityCam.focalLength     = displayedFocal;
            unityCam.sensorSize      = new Vector2(cam.SensorWidth, cam.SensorHeight);

            // Camera.rect = full when rendering to RT (no panel inset needed)
            unityCam.rect = new Rect(0, 0, 1, 1);

            // Apply shake
            if (cam.ShakeEnabled)
            {
                var t         = Time.time * cam.ShakeFrequency;
                var amplitude = cam.ShakeAmplitude * 0.5f;
                var tiltNoise = (Mathf.PerlinNoise(t, 0f) - 0.5f) * 2f * amplitude;
                var panNoise  = (Mathf.PerlinNoise(0f, t + 100f) - 0.5f) * 2f * amplitude;
                cameraTransform.rotation *= Quaternion.Euler(tiltNoise, panNoise, 0f);
            }
        }

        private void SyncDirectorCameras()
        {
            var directorElement = this.cameraBehaviour.DirectorCamera;

            foreach (var vc in this._directorCameras.Values)
            {
                vc.SyncDirectorView(directorElement);
            }
        }

        private void UpdateFrustumVisibility()
        {
            if (this.cameraBehaviour?.FrustumWireframe == null)
            {
                return;
            }

            var hasDirectorView = false;
            var count           = this.ViewSlotModel.ActiveSlotCount;

            for (var i = 0; i < count; i++)
            {
                if (this.ViewSlotModel.GetSlotType(i) == ViewMode.DIRECTOR)
                {
                    hasDirectorView = true;
                    break;
                }
            }

            this.cameraBehaviour.FrustumWireframe.gameObject.SetActive(hasDirectorView);
        }

        private ViewCamera FindDirectorCamera(int slotIndex) =>
            this._directorCameras.TryGetValue(slotIndex, out var vc) ? vc : null;

        private void EnsureCameraViewRT(int width, int height)
        {
            if (this._cameraViewRT != null
             && this._cameraViewRT.width == width
             && this._cameraViewRT.height == height)
            {
                return;
            }

            this.DestroyCameraViewRT();
            this._cameraViewRT      = new RenderTexture(width, height, 24, RenderTextureFormat.Default);
            this._cameraViewRT.name = "CameraViewRT";
            this.cameraBehaviour.SetTargetTexture(this._cameraViewRT);
        }

        private void DestroyCameraViewRT()
        {
            if (this._cameraViewRT != null)
            {
                this.cameraBehaviour?.SetTargetTexture(null);
                this._cameraViewRT.Release();
                Destroy(this._cameraViewRT);
                this._cameraViewRT = null;
            }
        }

        private void DestroyDirectorCameras()
        {
            foreach (var vc in this._directorCameras.Values)
            {
                vc?.Destroy();
            }

            this._directorCameras.Clear();
        }
    }
}
