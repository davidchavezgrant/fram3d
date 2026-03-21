using Fram3d.Core.Camera;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fram3d.UI.Panels
{
    /// <summary>
    /// Displays live-updating camera metadata: body name, sensor dimensions,
    /// focal length, and vertical FOV. Call UpdateValues() each frame.
    /// </summary>
    public sealed class CameraInfoSection: VisualElement
    {
        private readonly InfoRow _bodyRow;
        private readonly InfoRow _focalLengthRow;
        private readonly InfoRow _fovRow;
        private readonly InfoRow _sensorRow;

        public CameraInfoSection()
        {
            this._bodyRow        = new InfoRow("Body");
            this._sensorRow      = new InfoRow("Sensor");
            this._focalLengthRow = new InfoRow("Focal Length");
            this._fovRow         = new InfoRow("FOV");

            this.Add(this._bodyRow);
            this.Add(this._sensorRow);
            this.Add(this._focalLengthRow);
            this.Add(this._fovRow);
        }

        public void UpdateValues(CameraElement camera)
        {
            this._bodyRow.Value        = camera.Body?.Name ?? "—";
            this._sensorRow.Value      = $"{camera.SensorWidth:F1} × {camera.SensorHeight:F1} mm";
            this._focalLengthRow.Value = $"{camera.FocalLength:F0} mm";
            this._fovRow.Value         = $"{camera.VerticalFov * Mathf.Rad2Deg:F1}°";
        }
    }
}
