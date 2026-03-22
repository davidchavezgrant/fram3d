using Fram3d.Core.Camera;
using Fram3d.Core.Common;
using Fram3d.Engine.Conversion;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Fram3d.Engine.Integration
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraBehaviour: MonoBehaviour
    {
        private const float          FOCAL_LENGTH_LERP_SPEED = 10f;
        private const float          SHAKE_ROTATION_SCALE    = 0.5f;
        private const float          SHAKE_TIME_OFFSET       = 100f;
        private       CameraElement  _cameraElement;
        private       CameraDatabase _database;
        private       float          _displayedFocalLength;
        private       DepthOfField   _dof;
        private       float          _rightInsetPixels;
        private       Camera         _unityCamera;
        public        AspectRatio    ActiveAspectRatio              => this._cameraElement.ActiveAspectRatio;
        public        SensorMode     ActiveSensorMode               => this._cameraElement.ActiveSensorMode;
        public        CameraElement  CameraElement                  => this._cameraElement;
        public        CameraDatabase Database                       => this._database;
        public        void           CycleAspectRatioBackward()     => this._cameraElement.CycleAspectRatioBackward();
        public        void           CycleAspectRatioForward()      => this._cameraElement.CycleAspectRatioForward();
        public        void           SetSensorMode(SensorMode mode) => this._cameraElement.SetSensorMode(mode);

        private void ApplyShake(CameraElement cam)
        {
            if (!cam.ShakeEnabled)
                return;

            var t         = Time.time                                             * cam.ShakeFrequency;
            var amplitude = cam.ShakeAmplitude                                    * SHAKE_ROTATION_SCALE;
            var tiltNoise = (Mathf.PerlinNoise(t,  0f)                    - 0.5f) * 2f * amplitude;
            var panNoise  = (Mathf.PerlinNoise(0f, t + SHAKE_TIME_OFFSET) - 0.5f) * 2f * amplitude;
            this.transform.rotation *= Quaternion.Euler(tiltNoise, panNoise, 0f);
        }

        /// <summary>
        /// Creates a local Volume on the camera with a runtime DepthOfField override.
        /// Highest priority so it overrides the scene's default Volume profile.
        /// </summary>
        private void SetupDofVolume()
        {
            var volume = this.gameObject.AddComponent<Volume>();
            volume.isGlobal                             = true;
            volume.priority                             = 100f;
            volume.profile                              = ScriptableObject.CreateInstance<VolumeProfile>();
            this._dof                                   = volume.profile.Add<DepthOfField>();
            this._dof.active                            = true;
            this._dof.mode.overrideState                = true;
            this._dof.focusDistance.overrideState       = true;
            this._dof.aperture.overrideState            = true;
            this._dof.focalLength.overrideState         = true;
            this._dof.highQualitySampling.overrideState = true;
            this._dof.highQualitySampling.value         = true;
        }

        private void Sync()
        {
            var cam               = this._cameraElement;
            var targetFocalLength = cam.FocalLength;
            this.transform.position = cam.Position.ToUnity();
            this.transform.rotation = cam.Rotation.ToUnity();
            this.SyncFocalLength(cam, targetFocalLength);
            this.SyncDof(cam);
            this._unityCamera.focalLength = this._displayedFocalLength;
            this._unityCamera.sensorSize  = new Vector2(cam.SensorWidth, cam.SensorHeight);
            this.SyncViewportRect(cam);
            this.ApplyShake(cam);
        }

        private void SyncDof(CameraElement cam)
        {
            this._dof.mode.value          = cam.DofEnabled? DepthOfFieldMode.Bokeh : DepthOfFieldMode.Off;
            this._dof.focusDistance.value = cam.FocusDistance;
            this._dof.aperture.value      = cam.Aperture;
            this._dof.focalLength.value   = this._displayedFocalLength;
        }

        private void SyncFocalLength(CameraElement cam, float targetFocalLength)
        {
            if (cam.SnapFocalLength)
            {
                this._displayedFocalLength = targetFocalLength;
                cam.SnapFocalLength        = false;
                return;
            }

            this._displayedFocalLength = Mathf.Lerp(this._displayedFocalLength, targetFocalLength, Time.deltaTime * FOCAL_LENGTH_LERP_SPEED);

            if (Mathf.Abs(this._displayedFocalLength - targetFocalLength) < 0.01f)
                this._displayedFocalLength = targetFocalLength;
        }

        /// <summary>
        /// The normalized width of the viewport available after subtracting any
        /// right-side inset (e.g., properties panel). 1.0 when no inset.
        /// </summary>
        public float AvailableViewportWidth { get; private set; } = 1f;

        /// <summary>
        /// Called by the UI layer to reserve screen space on the right side.
        /// The 3D viewport and overlays shrink to avoid this area.
        /// </summary>
        public void SetRightInset(float pixels) => this._rightInsetPixels = pixels;

        private void SyncViewportRect(CameraElement cam)
        {
            var screenWidth  = (float)Screen.width;
            var screenHeight = (float)Screen.height;

            if (screenWidth <= 0 || screenHeight <= 0)
                return;

            var availableWidth = screenWidth - this._rightInsetPixels;
            this.AvailableViewportWidth = availableWidth / screenWidth;
            var availableAspect = availableWidth / screenHeight;
            var sensorAspect   = cam.SensorWidth / cam.SensorHeight;
            var vp             = ViewportRect.Compute(sensorAspect, availableAspect);

            // Scale and offset the viewport rect into the available area (left side of screen)
            this._unityCamera.rect = new Rect(
                vp.X * this.AvailableViewportWidth,
                vp.Y,
                vp.Width * this.AvailableViewportWidth,
                vp.Height);
        }

        private void Awake()
        {
            this._unityCamera                       = this.GetComponent<Camera>();
            this._cameraElement                     = new CameraElement(new ElementId(System.Guid.NewGuid()), "Main Camera");
            this._database                          = CameraDatabaseLoader.Load();
            this._unityCamera.usePhysicalProperties = true;
            this._unityCamera.gateFit               = Camera.GateFitMode.Overscan;
            var urpCameraData = this._unityCamera.GetUniversalAdditionalCameraData();
            urpCameraData.renderPostProcessing = true;
            var cam            = this._cameraElement;
            var defaultBody    = this._database.DefaultBody;
            var defaultLensSet = this._database.DefaultLensSet;

            if (defaultBody != null)
                cam.SetBody(defaultBody);

            if (defaultLensSet != null)
                cam.SetLensSet(defaultLensSet);

            this._displayedFocalLength   = cam.FocalLength;
            this._unityCamera.sensorSize = new Vector2(cam.SensorWidth, cam.SensorHeight);
            this.SetupDofVolume();
            this.Sync();
        }

        private void LateUpdate()
        {
            this.Sync();
        }
    }
}