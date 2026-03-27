namespace Fram3d.Core.Cameras
{
    /// <summary>
    /// Per-lens physical properties: focal length, maximum aperture (T-stop),
    /// and minimum focus distance. Used by prime lens sets to store per-focal-length
    /// constraints. Zoom lens sets use set-level values instead.
    /// </summary>
    public readonly struct LensSpec
    {
        public LensSpec(float focalLength, float maxAperture = 0f, float closeFocusM = 0f)
        {
            this.CloseFocusM = closeFocusM;
            this.FocalLength = focalLength;
            this.MaxAperture = maxAperture;
        }

        public float CloseFocusM { get; }
        public float FocalLength { get; }
        public float MaxAperture { get; }
    }
}