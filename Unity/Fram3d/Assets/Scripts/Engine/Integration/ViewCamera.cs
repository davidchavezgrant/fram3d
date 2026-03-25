using Fram3d.Core.Camera;
using Fram3d.Core.Scene;
using Fram3d.Engine.Conversion;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Manages one Unity Camera + RenderTexture pair for a single view slot.
    /// Configures the camera differently based on the view type:
    /// Camera View = physical properties + DOF volume,
    /// Director View = perspective without DOF,
    /// Designer View = disabled (placeholder).
    /// </summary>
    public sealed class ViewCamera
    {
        private readonly Camera       _camera;
        private readonly GameObject   _gameObject;
        private          DepthOfField _dof;
        private          RenderTexture _renderTexture;
        private          ViewMode     _viewMode;
        private          Volume       _volume;

        public ViewCamera(int slotIndex, ViewMode viewMode)
        {
            this._gameObject      = new GameObject($"ViewCamera_Slot{slotIndex}");
            this._camera          = this._gameObject.AddComponent<Camera>();
            this._viewMode        = viewMode;
            this._camera.enabled  = viewMode != ViewMode.DESIGNER;
            this._camera.depth    = -10 + slotIndex;
            this.ConfigureForViewMode(viewMode);
        }

        public Camera        UnityCamera   => this._camera;
        public RenderTexture RenderTexture => this._renderTexture;
        public ViewMode      ViewMode      => this._viewMode;

        /// <summary>
        /// Creates or resizes the RenderTexture to match the given pixel dimensions.
        /// </summary>
        public void EnsureRenderTexture(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            if (this._renderTexture != null
             && this._renderTexture.width == width
             && this._renderTexture.height == height)
            {
                return;
            }

            if (this._renderTexture != null)
            {
                this._camera.targetTexture = null;
                this._renderTexture.Release();
                Object.Destroy(this._renderTexture);
            }

            this._renderTexture        = new RenderTexture(width, height, 24, RenderTextureFormat.Default);
            this._renderTexture.name   = $"ViewRT_{this._gameObject.name}";
            this._camera.targetTexture = this._renderTexture;
        }

        /// <summary>
        /// Syncs the camera transform and properties for Camera View.
        /// Full sync: position, rotation, physical properties, DOF, shake.
        /// </summary>
        public void SyncCameraView(CameraElement cam, float displayedFocalLength,
                                    float shakeAmplitude, float shakeFrequency,
                                    bool shakeEnabled)
        {
            this._camera.transform.position = cam.Position.ToUnity();
            this._camera.transform.rotation = cam.Rotation.ToUnity();
            this._camera.focalLength        = displayedFocalLength;
            this._camera.sensorSize         = new Vector2(cam.SensorWidth, cam.SensorHeight);

            if (shakeEnabled)
            {
                var t         = Time.time * shakeFrequency;
                var amplitude = shakeAmplitude * 0.5f;
                var tiltNoise = (Mathf.PerlinNoise(t, 0f) - 0.5f) * 2f * amplitude;
                var panNoise  = (Mathf.PerlinNoise(0f, t + 100f) - 0.5f) * 2f * amplitude;
                this._camera.transform.rotation *= Quaternion.Euler(tiltNoise, panNoise, 0f);
            }

            if (this._dof != null)
            {
                this._dof.mode.value         = cam.DofEnabled ? DepthOfFieldMode.Bokeh : DepthOfFieldMode.Off;
                this._dof.focusDistance.value = cam.FocusDistance;
                this._dof.aperture.value     = cam.Aperture;
                this._dof.focalLength.value  = displayedFocalLength;
            }
        }

        /// <summary>
        /// Syncs the camera transform for Director View. Simple position +
        /// rotation sync, no DOF or shake.
        /// </summary>
        public void SyncDirectorView(CameraElement directorCam)
        {
            this._camera.transform.position = directorCam.Position.ToUnity();
            this._camera.transform.rotation = directorCam.Rotation.ToUnity();
        }

        /// <summary>
        /// Changes the view mode for this camera. Reconfigures rendering
        /// properties as needed.
        /// </summary>
        public void SetViewMode(ViewMode viewMode)
        {
            if (this._viewMode == viewMode)
            {
                return;
            }

            this._viewMode       = viewMode;
            this._camera.enabled = viewMode != ViewMode.DESIGNER;
            this.ConfigureForViewMode(viewMode);
        }

        public void Destroy()
        {
            if (this._renderTexture != null)
            {
                this._camera.targetTexture = null;
                this._renderTexture.Release();
                Object.Destroy(this._renderTexture);
                this._renderTexture = null;
            }

            if (this._volume != null)
            {
                Object.Destroy(this._volume);
                this._volume = null;
            }

            Object.Destroy(this._gameObject);
        }

        private void ConfigureForViewMode(ViewMode viewMode)
        {
            this._camera.usePhysicalProperties = viewMode == ViewMode.CAMERA;
            this._camera.gateFit               = Camera.GateFitMode.Overscan;
            this._camera.clearFlags            = CameraClearFlags.SolidColor;
            this._camera.backgroundColor       = Color.black;

            var urpData = this._camera.GetUniversalAdditionalCameraData();

            if (viewMode == ViewMode.CAMERA)
            {
                urpData.renderPostProcessing = true;
                this.EnsureDofVolume();
            }
            else
            {
                urpData.renderPostProcessing = false;
                this.RemoveDofVolume();
            }

            // Exclude Gizmo layer from Camera View (the frustum IS this camera)
            if (viewMode == ViewMode.CAMERA)
            {
                var gizmoLayer = LayerMask.NameToLayer("Gizmo");

                if (gizmoLayer >= 0)
                {
                    this._camera.cullingMask &= ~(1 << gizmoLayer);
                }
            }
        }

        private void EnsureDofVolume()
        {
            if (this._dof != null)
            {
                return;
            }

            this._volume                                    = this._gameObject.AddComponent<Volume>();
            this._volume.isGlobal                           = false;
            this._volume.priority                           = 100f;
            this._volume.profile                            = ScriptableObject.CreateInstance<VolumeProfile>();
            this._dof                                       = this._volume.profile.Add<DepthOfField>();
            this._dof.active                                = true;
            this._dof.mode.overrideState                    = true;
            this._dof.focusDistance.overrideState            = true;
            this._dof.aperture.overrideState                = true;
            this._dof.focalLength.overrideState              = true;
            this._dof.highQualitySampling.overrideState     = true;
            this._dof.highQualitySampling.value             = true;
        }

        private void RemoveDofVolume()
        {
            if (this._volume != null)
            {
                Object.Destroy(this._volume);
                this._volume = null;
                this._dof    = null;
            }
        }
    }
}
