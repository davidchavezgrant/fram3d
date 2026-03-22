using System;
namespace Fram3d.Core.Camera
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
    }
}