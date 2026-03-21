namespace Fram3d.Core.Camera
{
    public sealed class CameraBody
    {
        public string Name             { get; }
        public string Manufacturer     { get; }
        public float  SensorWidthMm    { get; }
        public float  SensorHeightMm   { get; }
        public string Format           { get; }
        public string Mount            { get; }
        public int[]  NativeResolution { get; }
        public int[]  SupportedFrameRates     { get; }

        public CameraBody(string name,
                          string manufacturer,
                          float  sensorWidthMm,
                          float  sensorHeightMm,
                          string format,
                          string mount,
                          int[]  nativeResolution,
                          int[]  supportedFrameRates)
        {
            this.Name             = name;
            this.Manufacturer     = manufacturer;
            this.SensorWidthMm    = sensorWidthMm;
            this.SensorHeightMm   = sensorHeightMm;
            this.Format           = format;
            this.Mount            = mount;
            this.NativeResolution = nativeResolution;
            this.SupportedFrameRates     = supportedFrameRates;
        }
    }
}