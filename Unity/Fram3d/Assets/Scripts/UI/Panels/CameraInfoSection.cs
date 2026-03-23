using Fram3d.Core.Camera;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Panels
{
    /// <summary>
    /// Displays live-updating camera metadata: body, lens set, focal length,
    /// horizontal AOV, camera height, aspect ratio, sensor dimensions,
    /// and DOF settings (when enabled). Call UpdateValues() each frame.
    /// </summary>
    public sealed class CameraInfoSection: VisualElement
    {
        private readonly InfoRow _aovRow;
        private readonly InfoRow _apertureRow;
        private readonly InfoRow _aspectRatioRow;
        private readonly InfoRow _bodyRow;
        private readonly InfoRow _focalLengthRow;
        private readonly InfoRow _focusDistanceRow;
        private readonly InfoRow _heightRow;
        private readonly InfoRow _lensSetRow;
        private readonly InfoRow _sensorRow;

        public CameraInfoSection()
        {
            this._bodyRow          = new InfoRow("Body");
            this._lensSetRow       = new InfoRow("Lens Set");
            this._sensorRow        = new InfoRow("Sensor");
            this._focalLengthRow   = new InfoRow("Focal Length");
            this._aovRow           = new InfoRow("AOV");
            this._heightRow        = new InfoRow("Height");
            this._aspectRatioRow   = new InfoRow("Aspect Ratio");
            this._apertureRow      = new InfoRow("Aperture");
            this._focusDistanceRow = new InfoRow("Focus Dist");
            this.Add(this._bodyRow);
            this.Add(this._lensSetRow);
            this.Add(this._sensorRow);
            this.Add(this._focalLengthRow);
            this.Add(this._aovRow);
            this.Add(this._heightRow);
            this.Add(this._aspectRatioRow);
            this.Add(this._apertureRow);
            this.Add(this._focusDistanceRow);
        }

        public void UpdateValues(CameraElement camera, AspectRatio activeAspectRatio)
        {
            this._bodyRow.Value        = camera.Body?.Name ?? "—";
            this._lensSetRow.Value     = camera.ActiveLensSet?.Name ?? "—";
            this._sensorRow.Value      = $"{camera.SensorWidth:F1} × {camera.SensorHeight:F1} mm";
            this._focalLengthRow.Value = $"{camera.FocalLength:F0} mm";
            this._aovRow.Value         = $"{camera.HorizontalFov * Mathf.Rad2Deg:F1}°";
            this._heightRow.Value      = $"{camera.Position.Y:F1} m";
            this._aspectRatioRow.Value = activeAspectRatio.DisplayName;
            var dofOn = camera.DofEnabled;
            this._apertureRow.style.display      = dofOn ? DisplayStyle.Flex : DisplayStyle.None;
            this._focusDistanceRow.style.display = dofOn ? DisplayStyle.Flex : DisplayStyle.None;
            this._apertureRow.Value              = $"f/{camera.Aperture:G}";
            this._focusDistanceRow.Value         = camera.FocusAtInfinity ? "\u221E" : $"{camera.FocusDistance:F1} m";
        }
    }
}
