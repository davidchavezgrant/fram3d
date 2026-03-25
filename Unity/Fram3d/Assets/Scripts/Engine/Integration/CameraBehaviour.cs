using Fram3d.Core.Camera;
using Fram3d.Core.Common;
using Fram3d.Core.Scene;
using Fram3d.Core.Viewport;
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
        private       CameraElement    _cameraElement;
        private       CameraDatabase   _database;
        private       CameraElement    _directorCamera;
        private       bool             _directorInitialized;
        private       float            _displayedFocalLength;
        private       DepthOfField     _dof;
        private       FrustumWireframe _frustumWireframe;
        private       float            _rightInsetPixels;
        private       Camera           _unityCamera;
        private       ViewMode         _viewMode = ViewMode.CAMERA;

        /// <summary>
        /// The camera that input should currently target. In Camera View,
        /// this is the shot camera. In Director View, the free utility camera.
        /// </summary>
        public CameraElement ActiveCamera => this._viewMode == ViewMode.DIRECTOR
                                           ? this._directorCamera
                                           : this._cameraElement;

        public        AspectRatio    ActiveAspectRatio => this._cameraElement.ActiveAspectRatio;
        public        SensorMode     ActiveSensorMode  => this._cameraElement.ActiveSensorMode;
        public        CameraElement  CameraElement     => this._cameraElement;
        public        CameraDatabase Database          => this._database;
        public        bool           IsDirectorView    => this._viewMode == ViewMode.DIRECTOR;

        /// <summary>
        /// The shot camera element. Exposed so the frustum wireframe and
        /// gizmo system can read/write the shot camera's transform even
        /// when Director View is active.
        /// </summary>
        public CameraElement ShotCamera => this._cameraElement;

        /// <summary>
        /// The right-side inset in pixels reserved for the properties panel.
        /// Overlay views read this to constrain their containers.
        /// </summary>
        public float RightInsetPixels => this._rightInsetPixels;

        public void CycleAspectRatioBackward() => this._cameraElement.CycleAspectRatioBackward();
        public void CycleAspectRatioForward()  => this._cameraElement.CycleAspectRatioForward();

        /// <summary>
        /// Called by the UI layer to reserve screen space on the right side.
        /// The 3D viewport and overlays shrink to avoid this area.
        /// </summary>
        public void SetRightInset(float pixels) => this._rightInsetPixels = pixels;

        public void SetSensorMode(SensorMode mode) => this._cameraElement.SetSensorMode(mode);

        /// <summary>
        /// Toggles between Camera View and Director View. On first entry
        /// to Director View, the director camera copies the shot camera's
        /// position and rotation. On subsequent entries, the director camera
        /// preserves its own position.
        /// </summary>
        public void ToggleDirectorView()
        {
            if (this._viewMode == ViewMode.CAMERA)
            {
                if (!this._directorInitialized)
                {
                    this._directorCamera.Position = this._cameraElement.Position;
                    this._directorCamera.Rotation = this._cameraElement.Rotation;
                    this._directorInitialized     = true;
                }

                this._viewMode = ViewMode.DIRECTOR;
            }
            else
            {
                this._viewMode = ViewMode.CAMERA;
            }

            if (this._frustumWireframe != null)
            {
                this._frustumWireframe.gameObject.SetActive(this.IsDirectorView);
            }
        }

        private void ApplyShake(CameraElement cam)
        {
            if (!cam.ShakeEnabled)
            {
                return;
            }

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
            var cam               = this.ActiveCamera;
            var targetFocalLength = cam.FocalLength;
            this.transform.position = cam.Position.ToUnity();
            this.transform.rotation = cam.Rotation.ToUnity();
            this.SyncFocalLength(cam, targetFocalLength);
            this._unityCamera.focalLength = this._displayedFocalLength;
            this._unityCamera.sensorSize  = new Vector2(cam.SensorWidth, cam.SensorHeight);
            this.SyncViewportRect();

            if (this.IsDirectorView)
            {
                // Director View: no DOF, no shake — utility view
                this._dof.mode.value = DepthOfFieldMode.Off;
            }
            else
            {
                this.SyncDof(this._cameraElement);
                this.ApplyShake(this._cameraElement);
            }
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
            {
                this._displayedFocalLength = targetFocalLength;
            }
        }

        private void SyncViewportRect()
        {
            var screenWidth = (float)Screen.width;

            if (screenWidth <= 0)
            {
                this._unityCamera.rect = new Rect(0,
                                                  0,
                                                  1,
                                                  1);

                return;
            }

            // Camera.rect only accounts for the panel inset.
            // Sensor aspect masking is handled by the UI mask bars.
            var availableWidth = (screenWidth - this._rightInsetPixels) / screenWidth;

            this._unityCamera.rect = new Rect(0,
                                              0,
                                              availableWidth,
                                              1);
        }

        private void Awake()
        {
            this._unityCamera                       = this.GetComponent<Camera>();
            this._cameraElement                     = new CameraElement(new ElementId(System.Guid.NewGuid()), "Main Camera");
            this._directorCamera                    = new CameraElement(new ElementId(System.Guid.NewGuid()), "Director Camera");
            this._database                          = CameraDatabaseLoader.Load();
            this._unityCamera.usePhysicalProperties = true;
            this._unityCamera.gateFit               = Camera.GateFitMode.Overscan;
            var urpCameraData = this._unityCamera.GetUniversalAdditionalCameraData();
            urpCameraData.renderPostProcessing = true;
            var cam            = this._cameraElement;
            var defaultBody    = this._database.DefaultBody;
            var defaultLensSet = this._database.DefaultLensSet;

            if (defaultBody != null)
            {
                cam.SetBody(defaultBody);
                this._directorCamera.SetBody(defaultBody);
            }

            if (defaultLensSet != null)
            {
                cam.SetLensSet(defaultLensSet);
            }

            this._displayedFocalLength   = cam.FocalLength;
            this._unityCamera.sensorSize = new Vector2(cam.SensorWidth, cam.SensorHeight);
            this.SetupDofVolume();
            this.CreateFrustumWireframe();
            this.Sync();
        }

        private void CreateFrustumWireframe()
        {
            var go = new GameObject("Shot Camera Frustum");
            var behaviour = go.AddComponent<ElementBehaviour>();
            behaviour.Element = this._cameraElement;
            this._frustumWireframe = go.AddComponent<FrustumWireframe>();
            this._frustumWireframe.Initialize(this._cameraElement);
            go.SetActive(false);
        }

        private void LateUpdate()
        {
            this.Sync();
        }
    }
}