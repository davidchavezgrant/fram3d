using System;
namespace Fram3d.Core.Camera
{
    /// <summary>
    /// A recording mode available on a camera body. Each mode defines a resolution,
    /// optional active sensor area in mm, and maximum frame rate. Camera bodies that
    /// support multiple modes (e.g., 4K vs C4K, Open Gate vs 16:9 crop, high-speed
    /// crops) have multiple SensorMode entries.
    /// </summary>
    public sealed class SensorMode
    {
        public SensorMode(string name,
                          int    resolutionWidth,
                          int    resolutionHeight,
                          float  sensorAreaWidthMm,
                          float  sensorAreaHeightMm,
                          int    maxFps)
        {
            this.Name                = name;
            this.ResolutionWidth     = resolutionWidth;
            this.ResolutionHeight    = resolutionHeight;
            this.SensorAreaWidthMm   = sensorAreaWidthMm;
            this.SensorAreaHeightMm = sensorAreaHeightMm;
            this.MaxFps              = maxFps;
        }

        public int    MaxFps              { get; }
        public string Name                { get; }
        public int    ResolutionHeight    { get; }
        public int    ResolutionWidth     { get; }
        public float  SensorAreaHeightMm { get; }
        public float  SensorAreaWidthMm   { get; }

        /// <summary>
        /// The native aspect ratio of this sensor mode, derived from resolution.
        /// </summary>
        public float AspectRatio => this.ResolutionHeight > 0
            ? (float)this.ResolutionWidth / this.ResolutionHeight
            : 0f;

        public override string ToString() =>
            $"{this.Name} ({this.ResolutionWidth}x{this.ResolutionHeight}, max {this.MaxFps}fps)";
    }
}
