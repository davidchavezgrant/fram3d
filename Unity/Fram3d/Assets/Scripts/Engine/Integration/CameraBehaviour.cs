using Fram3d.Core.Camera;
using Fram3d.Core.Common;
using Fram3d.Engine.Conversion;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraBehaviour: MonoBehaviour
    {
        private const float LENS_LERP_SPEED = 10f;

        private Camera        _unityCamera;
        private CameraElement _cameraElement;
        private float         _displayedFocalLength;

        public CameraElement CameraElement => this._cameraElement;

        private void Awake()
        {
            this._unityCamera                       = this.GetComponent<Camera>();
            this._cameraElement                     = new CameraElement(new ElementId(System.Guid.NewGuid()), "Main Camera");
            this._unityCamera.usePhysicalProperties = true;
            this._displayedFocalLength              = this._cameraElement.FocalLength;
            this._unityCamera.sensorSize            = new Vector2(24.89f, this._cameraElement.SensorHeight);
            this.Sync();
        }

        private void LateUpdate()
        {
            this.Sync();
        }

        private void Sync()
        {
            this.transform.position = this._cameraElement.Position.ToUnity();
            this.transform.rotation = this._cameraElement.Rotation.ToUnity();

            // Smooth focal length transitions — lerp the displayed value toward the domain target
            this._displayedFocalLength = Mathf.Lerp(
                this._displayedFocalLength,
                this._cameraElement.FocalLength,
                Time.deltaTime * LENS_LERP_SPEED);

            this._unityCamera.focalLength = this._displayedFocalLength;
            this._unityCamera.sensorSize  = new Vector2(24.89f, this._cameraElement.SensorHeight);
        }
    }
}
