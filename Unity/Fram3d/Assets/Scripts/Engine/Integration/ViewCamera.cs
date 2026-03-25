using Fram3d.Core.Camera;
using Fram3d.Core.Scene;
using Fram3d.Engine.Conversion;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Manages one Unity Camera for a non-Camera-View slot (Director or
    /// Designer). Renders directly to screen via Camera.rect — no
    /// RenderTextures. Camera View is handled by CameraBehaviour's main
    /// camera and is not wrapped by this class.
    /// </summary>
    public sealed class ViewCamera
    {
        private readonly Camera     _camera;
        private readonly GameObject _gameObject;

        public ViewCamera(int slotIndex, ViewMode viewMode)
        {
            this._gameObject                = new GameObject($"ViewCamera_Slot{slotIndex}");
            this._camera                    = this._gameObject.AddComponent<Camera>();
            this._camera.clearFlags         = CameraClearFlags.Skybox;
            this._camera.backgroundColor    = Color.black;
            this._camera.depth              = -10 + slotIndex;
            this._camera.enabled            = viewMode != ViewMode.DESIGNER;

            var urpData = this._camera.GetUniversalAdditionalCameraData();
            urpData.renderPostProcessing = false;
        }

        public Camera UnityCamera => this._camera;

        /// <summary>
        /// Syncs the camera transform for Director View. Simple position +
        /// rotation, no DOF or shake.
        /// </summary>
        public void SyncDirectorView(CameraElement directorCam)
        {
            this._camera.transform.position = directorCam.Position.ToUnity();
            this._camera.transform.rotation = directorCam.Rotation.ToUnity();
        }

        public void Destroy()
        {
            Object.Destroy(this._gameObject);
        }
    }
}
