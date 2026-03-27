namespace Fram3d.Core.Cameras
{
    /// <summary>
    /// Raw deserialized camera body from JSON. Field names use PascalCase
    /// for Core; the Engine deserializer maps from snake_case JSON fields.
    /// </summary>
    public sealed class RawCamera
    {
        public string          Format;
        public string          Manufacturer;
        public string          Mount;
        public string          Name;
        public int[]           NativeResolution;
        public float           SensorHeightMm;
        public RawSensorMode[] SensorModes;
        public float           SensorWidthMm;
        public int[]           SupportedFps;
        public int             Year;
    }
}
