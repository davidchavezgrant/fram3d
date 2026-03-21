using Fram3d.Core.Camera;
using Fram3d.Core.Common;
using Fram3d.Engine.Conversion;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraBehaviour: MonoBehaviour
    {
        private const float          FOCAL_LENGTH_LERP_SPEED = 10f;
        private       Camera         _unityCamera;
        private       CameraElement  _cameraElement;
        private       CameraDatabase _database;
        private       float          _displayedFocalLength;
        public        CameraElement  CameraElement => this._cameraElement;
        public        CameraDatabase Database      => this._database;

        private void Awake()
        {
            this._unityCamera                       = this.GetComponent<Camera>();
            this._cameraElement                     = new CameraElement(new ElementId(System.Guid.NewGuid()), "Main Camera");
            this._database                          = CameraDatabaseLoader.Load();
            this._unityCamera.usePhysicalProperties = true;

            // Set defaults from database
            var defaultBody    = this._database.DefaultBody;
            var defaultLensSet = this._database.DefaultLensSet;

            if (defaultBody != null)
                this._cameraElement.SetBody(defaultBody);

            if (defaultLensSet != null)
                this._cameraElement.SetLensSet(defaultLensSet);

            this._displayedFocalLength   = this._cameraElement.FocalLength;
            this._unityCamera.sensorSize = new Vector2(this._cameraElement.SensorWidth, this._cameraElement.SensorHeight);
            this.Sync();
        }

        private void LateUpdate()
        {
            this.Sync();
        }

        private void Sync()
        {
            var cam = this._cameraElement;

            this.transform.position = cam.Position.ToUnity();
            this.transform.rotation = cam.Rotation.ToUnity();

            // Dolly zoom requires instant sync to keep position and focal length perfectly paired.
            // All other focal length changes lerp for smooth visual transitions.
            var targetFocalLength = cam.FocalLength;

            if (cam.SnapFocalLength)
            {
                this._displayedFocalLength = targetFocalLength;
                cam.SnapFocalLength        = false;
            }
            else
            {
                this._displayedFocalLength = Mathf.Lerp(
                    this._displayedFocalLength,
                    targetFocalLength,
                    Time.deltaTime * FOCAL_LENGTH_LERP_SPEED);

                // Snap to target when close enough to prevent asymptotic drift
                if (Mathf.Abs(this._displayedFocalLength - targetFocalLength) < 0.01f)
                    this._displayedFocalLength = targetFocalLength;
            }

            this._unityCamera.focalLength = this._displayedFocalLength;
            this._unityCamera.sensorSize  = new Vector2(cam.SensorWidth, cam.SensorHeight);
        }
    }
}