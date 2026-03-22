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
        private const float          FOCAL_LENGTH_LERP_SPEED    = 10f;
        private const float          SHAKE_ROTATION_SCALE       = 0.5f;
        private const float          SHAKE_TIME_OFFSET          = 100f;
        private       AspectRatio    _activeAspectRatio = AspectRatio.DEFAULT;
        private       SensorMode     _activeSensorMode;
        private       CameraElement  _cameraElement;
        private       CameraDatabase _database;
        private       DepthOfField   _dof;
        private       float          _displayedFocalLength;
        private       Camera         _unityCamera;

        public AspectRatio    ActiveAspectRatio => this._activeAspectRatio;
        public SensorMode     ActiveSensorMode  => this._activeSensorMode;
        public CameraElement  CameraElement     => this._cameraElement;
        public CameraDatabase Database          => this._database;

        public void CycleAspectRatioBackward()
        {
            this._activeAspectRatio = this._activeAspectRatio.Previous();
            this.SyncEffectiveSensor();
        }

        public void CycleAspectRatioForward()
        {
            this._activeAspectRatio = this._activeAspectRatio.Next();
            this.SyncEffectiveSensor();
        }

        public void SetSensorMode(SensorMode mode)
        {
            this._activeSensorMode = mode;
            this.SyncEffectiveSensor();
        }

        /// <summary>
        /// Constrains the camera viewport to the effective sensor aspect ratio.
        /// This prevents Unity from rendering scene content beyond the sensor gate —
        /// without it, Unity derives hFov from vFov × screenAspect, which produces
        /// wider-than-sensor horizontal content when the sensor is narrower than the screen.
        /// </summary>
        private void SyncViewportRect(CameraElement cam)
        {
            var screenAspect = (float)Screen.width / Screen.height;
            var sensorAspect = cam.SensorWidth / cam.SensorHeight;

            if (screenAspect <= 0f || sensorAspect <= 0f)
            {
                this._unityCamera.rect = new Rect(0, 0, 1, 1);
                return;
            }

            if (sensorAspect > screenAspect + 0.001f)
            {
                var height = screenAspect / sensorAspect;
                this._unityCamera.rect = new Rect(0, (1 - height) / 2f, 1, height);
            }
            else if (sensorAspect < screenAspect - 0.001f)
            {
                var width = sensorAspect / screenAspect;
                this._unityCamera.rect = new Rect((1 - width) / 2f, 0, width, 1);
            }
            else
            {
                this._unityCamera.rect = new Rect(0, 0, 1, 1);
            }
        }

        /// <summary>
        /// Computes the effective sensor area from the active sensor mode and
        /// the selected delivery ratio. When a mode lacks sensor_area_mm, it's
        /// derived from the first mode (open gate) scaled by the resolution ratio —
        /// this handles sensor-windowed crop modes (RED, ARRI, Blackmagic) where
        /// lower resolutions read a smaller center portion of the sensor.
        /// </summary>
        private void SyncEffectiveSensor()
        {
            var cam = this._cameraElement;
            if (cam == null)
                return;

            var mode      = this._activeSensorMode;
            var body      = cam.Body;
            var gateWidth = ComputeGateWidth(mode, body);

            // The gate ratio comes from the RESOLUTION (what the mode actually outputs),
            // not from the physical sensor dimensions. Many cameras (DSLRs, mirrorless)
            // have a photo sensor wider than their video active area.
            var gateRatio = mode != null && mode.ResolutionWidth > 0 && mode.ResolutionHeight > 0
                ? (float)mode.ResolutionWidth / mode.ResolutionHeight
                : gateWidth / (mode != null && mode.SensorAreaHeightMm > 0? mode.SensorAreaHeightMm : body?.SensorHeightMm ?? 18.66f);

            var gateHeight = gateWidth / gateRatio;
            var ratio      = this._activeAspectRatio;

            if (ratio.Value == null)
            {
                cam.SensorWidth  = gateWidth;
                cam.SensorHeight = gateHeight;
                return;
            }

            var deliveryRatio = ratio.Value.Value;

            if (deliveryRatio > gateRatio)
            {
                cam.SensorWidth  = gateWidth;
                cam.SensorHeight = gateWidth / deliveryRatio;
            }
            else
            {
                cam.SensorWidth  = gateHeight * deliveryRatio;
                cam.SensorHeight = gateHeight;
            }
        }

        /// <summary>
        /// Determines the physical sensor width for a mode. If the mode has explicit
        /// sensor_area_mm, uses that. Otherwise, derives it from the first mode (open gate)
        /// by scaling proportionally to the resolution — this handles sensor-windowed
        /// crop modes where lower resolutions read a smaller center portion of the sensor.
        /// </summary>
        private static float ComputeGateWidth(SensorMode mode, CameraBody body)
        {
            // Mode has explicit sensor area → use it
            if (mode != null && mode.SensorAreaWidthMm > 0)
                return mode.SensorAreaWidthMm;

            // No mode → fall back to body
            if (mode == null || body == null || !body.HasSensorModes)
                return body?.SensorWidthMm ?? 24.89f;

            // Mode has no sensor area → derive from the first mode (open gate)
            // by scaling proportionally to resolution width
            var openGate = body.SensorModes[0];

            if (openGate.SensorAreaWidthMm <= 0 || openGate.ResolutionWidth <= 0 || mode.ResolutionWidth <= 0)
                return body.SensorWidthMm;

            return openGate.SensorAreaWidthMm * ((float)mode.ResolutionWidth / openGate.ResolutionWidth);
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

            this._displayedFocalLength = cam.FocalLength;
            this.SyncEffectiveSensor();
            this._unityCamera.sensorSize = new Vector2(cam.SensorWidth, cam.SensorHeight);
            this.SetupDofVolume();
            this.Sync();
        }

        private void LateUpdate()
        {
            this.Sync();
        }

        /// <summary>
        /// Creates a local Volume on the camera with a runtime DepthOfField override.
        /// Highest priority so it overrides the scene's default Volume profile.
        /// </summary>
        private void SetupDofVolume()
        {
            var volume         = this.gameObject.AddComponent<Volume>();
            volume.isGlobal    = true;
            volume.priority    = 100f;
            volume.profile     = ScriptableObject.CreateInstance<VolumeProfile>();
            this._dof          = volume.profile.Add<DepthOfField>();
            this._dof.active   = true;
            this._dof.mode.overrideState                 = true;
            this._dof.focusDistance.overrideState         = true;
            this._dof.aperture.overrideState              = true;
            this._dof.focalLength.overrideState           = true;
            this._dof.highQualitySampling.overrideState   = true;
            this._dof.highQualitySampling.value           = true;
        }

        private void ApplyShake(CameraElement cam)
        {
            if (!cam.ShakeEnabled)
                return;

            var t         = Time.time * cam.ShakeFrequency;
            var amplitude = cam.ShakeAmplitude * SHAKE_ROTATION_SCALE;
            var tiltNoise = (Mathf.PerlinNoise(t, 0f) - 0.5f) * 2f * amplitude;
            var panNoise  = (Mathf.PerlinNoise(0f, t + SHAKE_TIME_OFFSET) - 0.5f) * 2f * amplitude;
            this.transform.rotation *= Quaternion.Euler(tiltNoise, panNoise, 0f);
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
            this._dof.mode.value          = cam.DofEnabled ? DepthOfFieldMode.Bokeh : DepthOfFieldMode.Off;
            this._dof.focusDistance.value  = cam.FocusDistance;
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
    }
}
