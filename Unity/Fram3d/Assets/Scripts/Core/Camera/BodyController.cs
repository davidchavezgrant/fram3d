namespace Fram3d.Core.Camera
{
    /// <summary>
    /// Manages camera body, sensor mode, aspect ratio, and the resulting effective
    /// sensor dimensions. Owns all body/framing state — CameraElement delegates
    /// body operations through this.
    /// </summary>
    public sealed class BodyController
    {
        private const float DEFAULT_SENSOR_HEIGHT = 18.66f;
        private const float DEFAULT_SENSOR_WIDTH  = 24.89f;

        public AspectRatio ActiveAspectRatio { get; private set; } = AspectRatio.DEFAULT;
        public SensorMode  ActiveSensorMode  { get; private set; }
        public CameraBody  Body              { get; private set; }
        public float       SensorHeight      { get; private set; } = DEFAULT_SENSOR_HEIGHT;
        public float       SensorWidth       { get; private set; } = DEFAULT_SENSOR_WIDTH;

        public void CycleAspectRatioBackward()
        {
            this.ActiveAspectRatio = this.ActiveAspectRatio.Previous();
            this.SyncEffectiveSensor();
        }

        public void CycleAspectRatioForward()
        {
            this.ActiveAspectRatio = this.ActiveAspectRatio.Next();
            this.SyncEffectiveSensor();
        }

        public void SetBody(CameraBody body)
        {
            this.Body = body;
            this.SyncEffectiveSensor();
        }

        public void SetSensorMode(SensorMode mode)
        {
            this.ActiveSensorMode = mode;
            this.SyncEffectiveSensor();
        }

        /// <summary>
        /// Computes the effective sensor area from the active sensor mode and
        /// the selected aspect ratio. When a mode lacks sensor_area_mm, the gate
        /// width is derived from the first mode (open gate) scaled by resolution —
        /// this handles sensor-windowed crop modes (RED, ARRI, Blackmagic).
        /// </summary>
        private void SyncEffectiveSensor()
        {
            var mode      = this.ActiveSensorMode;
            var body      = this.Body;
            var gateWidth = body != null? body.ComputeGateWidth(mode) : DEFAULT_SENSOR_WIDTH;
            var gateRatio = computeGateRatio(mode, body, gateWidth);

            var gateHeight = gateWidth / gateRatio;
            var ratio      = this.ActiveAspectRatio;

            if (ratio.Value == null)
            {
                this.SensorWidth  = gateWidth;
                this.SensorHeight = gateHeight;
                return;
            }

            var deliveryRatio = ratio.Value.Value;

            if (deliveryRatio > gateRatio)
            {
                this.SensorWidth  = gateWidth;
                this.SensorHeight = gateWidth / deliveryRatio;
            }
            else
            {
                this.SensorWidth  = gateHeight * deliveryRatio;
                this.SensorHeight = gateHeight;
            }
        }

        /// <summary>
        /// The gate ratio comes from the RESOLUTION (what the mode actually outputs),
        /// not from the physical sensor dimensions. Many cameras (DSLRs, mirrorless)
        /// have a photo sensor wider than their video active area (e.g. 3:2 sensor
        /// but 16:9 video output). Falls back to physical dimensions when no mode
        /// or no resolution data is available.
        /// </summary>
        private static float computeGateRatio(SensorMode mode, CameraBody body, float gateWidth)
        {
            if (mode != null && mode.ResolutionWidth > 0 && mode.ResolutionHeight > 0)
                return (float)mode.ResolutionWidth / mode.ResolutionHeight;

            var gateHeight = mode != null && mode.SensorAreaHeightMm > 0
                ? mode.SensorAreaHeightMm
                : body?.SensorHeightMm ?? DEFAULT_SENSOR_HEIGHT;

            return gateWidth / gateHeight;
        }
    }
}
