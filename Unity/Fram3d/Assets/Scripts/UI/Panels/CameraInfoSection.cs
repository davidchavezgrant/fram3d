using Fram3d.Core.Camera;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Panels
{
    /// <summary>
    /// Displays live-updating camera metadata: body name, sensor dimensions,
    /// focal length, vertical FOV, aspect ratio, and DOF settings (when enabled).
    /// Call UpdateValues() each frame.
    /// </summary>
    public sealed class CameraInfoSection: VisualElement
    {
        private readonly InfoRow _apertureRow;
        private readonly InfoRow _aspectRatioRow;
        private readonly InfoRow _bodyRow;
        private readonly InfoRow _focalLengthRow;
        private readonly InfoRow _focusDistanceRow;
        private readonly InfoRow _fovRow;
        private readonly InfoRow _sensorRow;

        public CameraInfoSection()
        {
            this._bodyRow          = new InfoRow("Body");
            this._sensorRow        = new InfoRow("Sensor");
            this._focalLengthRow   = new InfoRow("Focal Length");
            this._fovRow           = new InfoRow("FOV");
            this._aspectRatioRow   = new InfoRow("Aspect Ratio");
            this._apertureRow      = new InfoRow("Aperture");
            this._focusDistanceRow = new InfoRow("Focus Dist");
            this.Add(this._bodyRow);
            this.Add(this._sensorRow);
            this.Add(this._focalLengthRow);
            this.Add(this._fovRow);
            this.Add(this._aspectRatioRow);
            this.Add(this._apertureRow);
            this.Add(this._focusDistanceRow);
        }

        public void UpdateValues(CameraElement camera, AspectRatio activeAspectRatio)
        {
            this._bodyRow.Value        = camera.Body?.Name ?? "—";
            this._sensorRow.Value      = $"{camera.SensorWidth:F1} × {camera.SensorHeight:F1} mm";
            this._focalLengthRow.Value = $"{camera.FocalLength:F0} mm";
            this._fovRow.Value         = $"{camera.VerticalFov * Mathf.Rad2Deg:F1}°";
            this._aspectRatioRow.Value = activeAspectRatio.DisplayName;
            var dofOn = camera.DofEnabled;
            this._apertureRow.style.display      = dofOn? DisplayStyle.Flex : DisplayStyle.None;
            this._focusDistanceRow.style.display = dofOn? DisplayStyle.Flex : DisplayStyle.None;
            this._apertureRow.Value              = $"f/{camera.Aperture:G}";
            this._focusDistanceRow.Value         = camera.FocusAtInfinity? "\u221E" : $"{camera.FocusDistance:F1} m";
        }
    }
}
