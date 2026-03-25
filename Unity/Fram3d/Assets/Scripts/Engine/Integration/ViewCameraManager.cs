using System.Collections.Generic;
using Fram3d.Core.Camera;
using Fram3d.Core.Common;
using Fram3d.Core.Scene;
using Fram3d.Engine.Conversion;
using UnityEngine;

namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Manages split-screen view layouts. In single-view mode, this component
    /// is completely passive — CameraBehaviour runs as normal. In multi-view,
    /// it creates Director cameras, sets Camera.rect viewports on all cameras,
    /// and syncs Director cameras from CameraBehaviour.DirectorCamera.
    ///
    /// Runs after CameraBehaviour (execution order 100) so its Camera.rect
    /// overrides take effect after CameraBehaviour.SyncViewportRect().
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class ViewCameraManager : MonoBehaviour
    {
        private int                         _activeSlot;
        private Dictionary<int, ViewCamera> _directorCameras = new();

        [SerializeField]
        private CameraBehaviour cameraBehaviour;

        public  int            ActiveSlot      => this._activeSlot;
        public  CameraBehaviour CameraBehaviour => this.cameraBehaviour;
        public  ViewSlotModel  ViewSlotModel   { get; private set; }
        private Rect[]         _viewportRects  = { new(0, 0, 1, 1) };

        /// <summary>
        /// Returns the CameraElement for the active slot. The active slot is
        /// set by mouse click (ActivateSlotAtPosition), not hover.
        /// </summary>
        public CameraElement ActiveCameraElement
        {
            get
            {
                if (!this.IsMultiView)
                {
                    return this.cameraBehaviour.ActiveCamera;
                }

                var mode = this.ViewSlotModel.GetSlotType(this._activeSlot);

                if (mode == ViewMode.CAMERA)
                {
                    return this.cameraBehaviour.ShotCamera;
                }

                if (mode == ViewMode.DIRECTOR)
                {
                    return this.cameraBehaviour.DirectorCamera;
                }

                return this.cameraBehaviour.ActiveCamera;
            }
        }

        /// <summary>
        /// Returns the Unity Camera under the mouse position, for raycasting.
        /// Unlike ActiveCameraElement (click-based), this follows the cursor
        /// so hover and selection work in any viewport.
        /// </summary>
        public Camera GetUnityCameraAtPosition(Vector2 mouseScreenPos)
        {
            if (this.ViewSlotModel.Layout == ViewLayout.SINGLE)
            {
                return this.cameraBehaviour.GetComponent<Camera>();
            }

            var normalizedPos = new Vector2(mouseScreenPos.x / Screen.width,
                                            mouseScreenPos.y / Screen.height);

            var count = this.ViewSlotModel.ActiveSlotCount;

            for (var i = 0; i < count; i++)
            {
                if (i >= this._viewportRects.Length)
                {
                    break;
                }

                if (!this._viewportRects[i].Contains(normalizedPos))
                {
                    continue;
                }

                var mode = this.ViewSlotModel.GetSlotType(i);

                if (mode == ViewMode.CAMERA)
                {
                    return this.cameraBehaviour.GetComponent<Camera>();
                }

                if (mode == ViewMode.DIRECTOR)
                {
                    var vc = this._directorCameras.TryGetValue(i, out var cam) ? cam : null;

                    if (vc != null)
                    {
                        return vc.UnityCamera;
                    }
                }
            }

            return this.cameraBehaviour.GetComponent<Camera>();
        }

        /// <summary>
        /// Returns the viewport rect (in normalized screen coords) for the
        /// Camera View slot. Used by overlays to scope themselves.
        /// </summary>
        public Rect CameraViewRect
        {
            get
            {
                if (this.ViewSlotModel.Layout == ViewLayout.SINGLE)
                {
                    return new Rect(0, 0, 1, 1);
                }

                var count = this.ViewSlotModel.ActiveSlotCount;

                for (var i = 0; i < count && i < this._viewportRects.Length; i++)
                {
                    if (this.ViewSlotModel.GetSlotType(i) == ViewMode.CAMERA)
                    {
                        return this._viewportRects[i];
                    }
                }

                return new Rect(0, 0, 1, 1);
            }
        }

        public bool IsMultiView => this.ViewSlotModel.Layout != ViewLayout.SINGLE;

        /// <summary>
        /// Activates the view slot at the given screen position. Called on
        /// mouse click — the active slot determines which camera receives
        /// movement input until the user clicks in a different viewport.
        /// </summary>
        public void ActivateSlotAtPosition(Vector2 mouseScreenPos)
        {
            if (!this.IsMultiView)
            {
                return;
            }

            var normalizedPos = new Vector2(mouseScreenPos.x / Screen.width,
                                            mouseScreenPos.y / Screen.height);

            for (var i = 0; i < this.ViewSlotModel.ActiveSlotCount && i < this._viewportRects.Length; i++)
            {
                if (this._viewportRects[i].Contains(normalizedPos))
                {
                    this._activeSlot = i;
                    return;
                }
            }
        }

        /// <summary>
        /// Returns the Unity Camera for a specific slot index.
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
                return this._directorCameras.TryGetValue(slotIndex, out var vc) ? vc.UnityCamera : null;
            }

            return null;
        }

        /// <summary>
        /// Returns the viewport rect for a specific slot index.
        /// </summary>
        public Rect GetViewportRect(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < this._viewportRects.Length)
            {
                return this._viewportRects[slotIndex];
            }

            return new Rect(0, 0, 1, 1);
        }

        private void Awake()
        {
            this.ViewSlotModel          =  new ViewSlotModel();
            this.ViewSlotModel.Changed += this.OnLayoutChanged;
        }

        private void Start()
        {
            if (this.cameraBehaviour == null)
            {
                this.cameraBehaviour = FindAnyObjectByType<CameraBehaviour>();
            }
        }

        private void LateUpdate()
        {
            if (this.cameraBehaviour == null)
            {
                return;
            }

            if (this.ViewSlotModel.Layout == ViewLayout.SINGLE)
            {
                return;
            }

            // Multi-view: sync Director cameras and override viewport rects
            this.SyncDirectorCameras();
            this.ApplyViewportRects();
        }

        private void OnDestroy()
        {
            this.DestroyDirectorCameras();

            if (this.ViewSlotModel != null)
            {
                this.ViewSlotModel.Changed -= this.OnLayoutChanged;
            }
        }

        private void OnLayoutChanged()
        {
            this.DestroyDirectorCameras();

            if (this.ViewSlotModel.Layout == ViewLayout.SINGLE)
            {
                // Restore full viewport rect on the main camera
                if (this.cameraBehaviour != null)
                {
                    this.cameraBehaviour.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
                }

                // Sync CameraBehaviour's director toggle to match the slot state
                this.SyncSingleViewDirectorToggle();
                return;
            }

            // Entering multi-view: CameraBehaviour must be in Camera View mode
            // (the Director View is handled by the separate ViewCamera)
            if (this.cameraBehaviour != null && this.cameraBehaviour.IsDirectorView)
            {
                this.cameraBehaviour.ToggleDirectorView();
            }

            // Create Director cameras for Director slots
            var count = this.ViewSlotModel.ActiveSlotCount;

            for (var i = 0; i < count; i++)
            {
                if (this.ViewSlotModel.GetSlotType(i) != ViewMode.DIRECTOR)
                {
                    continue;
                }

                this._directorCameras[i] = new ViewCamera(i, ViewMode.DIRECTOR);
                this.cameraBehaviour.EnsureDirectorInitialized();
            }

            // Frustum visible in multi-view when Director View is on screen
            if (this.cameraBehaviour?.FrustumWireframe != null)
            {
                this.cameraBehaviour.FrustumWireframe.gameObject.SetActive(this._directorCameras.Count > 0);
            }
        }

        /// <summary>
        /// In single-view, syncs CameraBehaviour's director toggle to match
        /// the ViewSlotModel slot state. Called once on state change, not
        /// every frame.
        /// </summary>
        private void SyncSingleViewDirectorToggle()
        {
            if (this.cameraBehaviour == null)
            {
                return;
            }

            var wantDirector = this.ViewSlotModel.GetSlotType(0) == ViewMode.DIRECTOR;

            if (wantDirector != this.cameraBehaviour.IsDirectorView)
            {
                this.cameraBehaviour.ToggleDirectorView();
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
        /// Overrides Camera.rect on the main camera and Director cameras based
        /// on the current layout. Runs after CameraBehaviour.SyncViewportRect.
        /// </summary>
        private void ApplyViewportRects()
        {
            var mainCam = this.cameraBehaviour.GetComponent<Camera>();
            this._viewportRects = this.ComputeViewportRects();
            var count           = this.ViewSlotModel.ActiveSlotCount;

            for (var i = 0; i < count && i < this._viewportRects.Length; i++)
            {
                var mode = this.ViewSlotModel.GetSlotType(i);

                if (mode == ViewMode.CAMERA)
                {
                    mainCam.rect = this._viewportRects[i];
                }
                else if (mode == ViewMode.DIRECTOR)
                {
                    var vc = this._directorCameras.TryGetValue(i, out var cam) ? cam : null;

                    if (vc != null)
                    {
                        vc.UnityCamera.rect = this._viewportRects[i];
                    }
                }
            }
        }

        private Rect[] ComputeViewportRects()
        {
            var screenWidth   = (float)Screen.width;
            var insetPixels   = this.cameraBehaviour.RightInsetPixels;
            var availableNorm = screenWidth > 0 ? (screenWidth - insetPixels) / screenWidth : 1f;

            if (this.ViewSlotModel.Layout == ViewLayout.HORIZONTAL)
            {
                var halfW = availableNorm * 0.5f;
                return new[]
                {
                    new Rect(0,     0, halfW, 1),
                    new Rect(halfW, 0, halfW, 1)
                };
            }

            // VERTICAL: top and bottom
            return new[]
            {
                new Rect(0, 0.5f, availableNorm, 0.5f),
                new Rect(0, 0,    availableNorm, 0.5f)
            };
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
