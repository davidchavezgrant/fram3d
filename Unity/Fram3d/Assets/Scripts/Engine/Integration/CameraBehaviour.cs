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
        private       CameraElement  _cameraElement;
        private       CameraDatabase _database;
        private       DepthOfField   _dof;
        private       float          _displayedFocalLength;
        private       Camera         _unityCamera;
        public        CameraElement  CameraElement => this._cameraElement;
        public        CameraDatabase Database      => this._database;

        private void Awake()
        {
            this._unityCamera                       = this.GetComponent<Camera>();
            this._cameraElement                     = new CameraElement(new ElementId(System.Guid.NewGuid()), "Main Camera");
            this._database                          = CameraDatabaseLoader.Load();
            this._unityCamera.usePhysicalProperties = true;
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
            this._dof.mode.overrideState          = true;
            this._dof.focusDistance.overrideState  = true;
            this._dof.aperture.overrideState       = true;
            this._dof.focalLength.overrideState    = true;
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
