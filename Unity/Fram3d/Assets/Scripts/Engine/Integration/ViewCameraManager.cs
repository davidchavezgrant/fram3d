using System.Collections.Generic;
using Fram3d.Core.Camera;
using Fram3d.Core.Common;
using Fram3d.Core.Scene;
using Fram3d.Engine.Conversion;
using UnityEngine;

namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Manages per-slot view cameras using Camera.rect viewports. All cameras
    /// render directly to the screen — no RenderTextures. The Camera View slot
    /// uses CameraBehaviour's main camera. Director View slots get separate
    /// cameras with simple position/rotation sync.
    /// </summary>
    public sealed class ViewCameraManager : MonoBehaviour
    {
        private static readonly Rect FULL_RECT = new(0, 0, 1, 1);

        private int                         _activeSlot;
        private Dictionary<int, ViewCamera> _directorCameras = new();

        [SerializeField]
        private CameraBehaviour cameraBehaviour;

        public int             ActiveSlot      => this._activeSlot;
        public CameraBehaviour CameraBehaviour => this.cameraBehaviour;
        public ViewSlotModel   ViewSlotModel   { get; private set; }

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
        /// Sets the active (hovered) slot for input routing.
        /// </summary>
        public void SetActiveSlot(int slotIndex) => this._activeSlot = slotIndex;

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

            this.RebuildCameras();
        }

        private void LateUpdate()
        {
            if (this.cameraBehaviour == null)
            {
                return;
            }

            var isSingleView = this.ViewSlotModel.Layout == ViewLayout.SINGLE;

            // In single-view, let CameraBehaviour handle everything as before.
            // In multi-view, we drive the main camera sync and viewport rects.
            this.cameraBehaviour.ExternalSyncEnabled = !isSingleView;

            if (!isSingleView)
            {
                this.SyncCameraViewSlot();
            }

            this.SyncDirectorCameras();
            this.UpdateViewportRects();
            this.UpdateFrustumVisibility();
        }

        private void OnDestroy()
        {
            this.DestroyDirectorCameras();

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

            this.UpdateViewportRects();
            this.UpdateFrustumVisibility();
        }

        /// <summary>
        /// Syncs CameraBehaviour's main camera in multi-view mode.
        /// Position, rotation, focal length, sensor, and shake.
        /// </summary>
        private void SyncCameraViewSlot()
        {
            var cam            = this.cameraBehaviour.ShotCamera;
            var displayedFocal = this.cameraBehaviour.DisplayedFocalLength;
            var unityCam       = this.cameraBehaviour.GetComponent<Camera>();
            var cameraTransform = unityCam.transform;

            cameraTransform.position = cam.Position.ToUnity();
            cameraTransform.rotation = cam.Rotation.ToUnity();
            unityCam.focalLength     = displayedFocal;
            unityCam.sensorSize      = new Vector2(cam.SensorWidth, cam.SensorHeight);

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

        /// <summary>
        /// Sets Camera.rect on each camera to divide the screen based on layout.
        /// SINGLE: main camera fills the screen.
        /// SIDE_BY_SIDE: two halves with a small gap.
        /// ONE_PLUS_TWO: top half + two quarter panels.
        /// </summary>
        private void UpdateViewportRects()
        {
            var mainCam = this.cameraBehaviour.GetComponent<Camera>();
            var layout  = this.ViewSlotModel.Layout;

            if (layout == ViewLayout.SINGLE)
            {
                // Don't touch Camera.rect in single-view — CameraBehaviour manages it
                // (including the properties panel inset via SyncViewportRect)
                return;
            }

            var gap   = 0.003f; // ~2px gap between viewports
            var slots = this.ComputeViewportRects(layout, gap);
            var count = this.ViewSlotModel.ActiveSlotCount;

            for (var i = 0; i < count; i++)
            {
                var mode = this.ViewSlotModel.GetSlotType(i);

                if (mode == ViewMode.CAMERA)
                {
                    mainCam.rect = slots[i];
                }
                else if (mode == ViewMode.DIRECTOR)
                {
                    var vc = this.FindDirectorCamera(i);

                    if (vc != null)
                    {
                        vc.UnityCamera.rect = slots[i];
                    }
                }
                // Designer View has no camera — the viewport area stays black
            }
        }

        private Rect[] ComputeViewportRects(ViewLayout layout, float gap)
        {
            var halfGap = gap / 2f;

            if (layout == ViewLayout.SIDE_BY_SIDE)
            {
                var halfW = 0.5f - halfGap;
                return new[]
                {
                    new Rect(0,              0, halfW, 1),
                    new Rect(0.5f + halfGap, 0, halfW, 1)
                };
            }

            // ONE_PLUS_TWO: top row full width, bottom row split
            var topH    = 0.6f - halfGap;
            var bottomH = 0.4f - halfGap;
            var bottomW = 0.5f - halfGap;

            return new[]
            {
                new Rect(0,              bottomH + gap, 1,       topH),
                new Rect(0,              0,             bottomW, bottomH),
                new Rect(0.5f + halfGap, 0,             bottomW, bottomH)
            };
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
