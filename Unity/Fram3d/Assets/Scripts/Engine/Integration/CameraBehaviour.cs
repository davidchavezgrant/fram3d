using Fram3d.Core.Camera;
using Fram3d.Core.Common;
using Fram3d.Engine.Conversion;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraBehaviour: MonoBehaviour
    {
        private Camera        _unityCamera;
        private CameraElement _cameraElement;
        public  CameraElement CameraElement => this._cameraElement;

        private void Awake()
        {
            this._unityCamera                       = this.GetComponent<Camera>();
            this._cameraElement                     = new CameraElement(ElementId.New(), "Main Camera");
            this._unityCamera.usePhysicalProperties = true;
            this._unityCamera.focalLength           = this._cameraElement.FocalLength;

            // Default Super 35 sensor size until camera body database (1.1.3)
            this._unityCamera.sensorSize = new Vector2(24.89f, 18.66f);
            this.Sync();
        }

        private void LateUpdate()
        {
            this.Sync();
        }

        private void Sync()
        {
            this.transform.position       = this._cameraElement.Position.ToUnity();
            this.transform.rotation       = this._cameraElement.Rotation.ToUnity();
            this._unityCamera.focalLength = this._cameraElement.FocalLength;
        }
    }
}