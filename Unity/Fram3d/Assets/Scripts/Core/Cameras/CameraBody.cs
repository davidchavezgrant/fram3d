using System;
namespace Fram3d.Core.Cameras
{
    public sealed class CameraBody
    {
        public static readonly SensorMode[] EMPTY_SENSOR_MODES = Array.Empty<SensorMode>();

        public CameraBody(string       name,
                          string       manufacturer,
                          int          year,
                          float        sensorWidthMm,
                          float        sensorHeightMm,
                          string       format,
                          string       mount,
                          int[]        nativeResolution,
                          int[]        supportedFrameRates,
                          SensorMode[] sensorModes = null)
        {
            this.Name                = name;
            this.Manufacturer        = manufacturer;
            this.Year                = year;
            this.SensorWidthMm       = sensorWidthMm;
            this.SensorHeightMm      = sensorHeightMm;
            this.Format              = format;
            this.Mount               = mount;
            this.NativeResolution    = nativeResolution;
            this.SupportedFrameRates = supportedFrameRates;
            this.SensorModes         = sensorModes ?? EMPTY_SENSOR_MODES;
        }

        public string       Format              { get; }
        public bool         HasSensorModes      => this.SensorModes.Length > 0;
        public string       Manufacturer        { get; }
        public string       Mount               { get; }
        public string       Name                { get; }
        public int[]        NativeResolution    { get; }
        public float        SensorHeightMm      { get; }
        public SensorMode[] SensorModes         { get; }
        public float        SensorWidthMm       { get; }
        public int[]        SupportedFrameRates { get; }
        public int          Year                { get; }

        /// <summary>
        /// Determines the physical sensor width for a mode. If the mode has explicit
        /// sensor_area_mm, uses that. Otherwise, derives it from the first mode (open gate)
        /// by scaling proportionally to the resolution — this handles sensor-windowed
        /// crop modes where lower resolutions read a smaller center portion of the sensor.
        /// </summary>
        public float ComputeGateWidth(SensorMode mode)
        {
            if (mode != null && mode.SensorAreaWidthMm > 0)
                return mode.SensorAreaWidthMm;

            if (mode == null || !this.HasSensorModes)
                return this.SensorWidthMm;

            var openGate = this.SensorModes[0];

            if (openGate.SensorAreaWidthMm <= 0 || openGate.ResolutionWidth <= 0 || mode.ResolutionWidth <= 0)
                return this.SensorWidthMm;

            return openGate.SensorAreaWidthMm * ((float)mode.ResolutionWidth / openGate.ResolutionWidth);
        }
    }
}