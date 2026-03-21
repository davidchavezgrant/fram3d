namespace Fram3d.Core.Camera
{
    public sealed class CameraBody
    {
        public CameraBody(string name,
                          string manufacturer,
                          int    year,
                          float  sensorWidthMm,
                          float  sensorHeightMm,
                          string format,
                          string mount,
                          int[]  nativeResolution,
                          int[]  supportedFrameRates)
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
        }

        public string Format              { get; }
        public string Manufacturer        { get; }
        public string Mount               { get; }
        public string Name                { get; }
        public int[]  NativeResolution    { get; }
        public float  SensorHeightMm      { get; }
        public float  SensorWidthMm       { get; }
        public int[]  SupportedFrameRates { get; }
        public int    Year                { get; }
    }
}