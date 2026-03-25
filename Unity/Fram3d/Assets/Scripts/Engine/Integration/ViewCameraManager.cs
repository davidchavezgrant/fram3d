using System.Collections.Generic;
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
        private Dictionary<int, ViewCamera> _directorCameras = new();

        [SerializeField]
        private CameraBehaviour cameraBehaviour;

        public CameraBehaviour CameraBehaviour => this.cameraBehaviour;
        public ViewSlotModel   ViewSlotModel   { get; private set; }

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

                // Show frustum wireframe based on CameraBehaviour's own state
                if (this.cameraBehaviour?.FrustumWireframe != null)
                {
                    this.cameraBehaviour.FrustumWireframe.gameObject.SetActive(this.cameraBehaviour.IsDirectorView);
                }

                return;
            }

            // Multi-view: create Director cameras and show frustum
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

            // Frustum is always visible in multi-view (Director View is on screen)
            if (this.cameraBehaviour?.FrustumWireframe != null)
            {
                this.cameraBehaviour.FrustumWireframe.gameObject.SetActive(this._directorCameras.Count > 0);
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
            var rects   = this.ComputeViewportRects();
            var count   = this.ViewSlotModel.ActiveSlotCount;

            for (var i = 0; i < count; i++)
            {
                var mode = this.ViewSlotModel.GetSlotType(i);

                if (mode == ViewMode.CAMERA)
                {
                    mainCam.rect = rects[i];
                }
                else if (mode == ViewMode.DIRECTOR)
                {
                    var vc = this._directorCameras.TryGetValue(i, out var cam) ? cam : null;

                    if (vc != null)
                    {
                        vc.UnityCamera.rect = rects[i];
                    }
                }
            }
        }

        private Rect[] ComputeViewportRects()
        {
            var gap     = 0.003f;
            var halfGap = gap / 2f;
            var layout  = this.ViewSlotModel.Layout;

            if (layout == ViewLayout.SIDE_BY_SIDE)
            {
                var halfW = 0.5f - halfGap;
                return new[]
                {
                    new Rect(0,              0, halfW, 1),
                    new Rect(0.5f + halfGap, 0, halfW, 1)
                };
            }

            // ONE_PLUS_TWO: top full width, bottom split
            var topH    = 0.6f  - halfGap;
            var bottomH = 0.4f  - halfGap;
            var bottomW = 0.5f  - halfGap;

            return new[]
            {
                new Rect(0,              bottomH + gap, 1,       topH),
                new Rect(0,              0,             bottomW, bottomH),
                new Rect(0.5f + halfGap, 0,             bottomW, bottomH)
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
