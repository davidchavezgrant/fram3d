namespace Fram3d.Core.Camera
{
    public sealed class LensSet
    {
        /// <summary>
        /// Creates a prime lens set from a list of fixed focal lengths.
        /// </summary>
        public LensSet(string  name,
                       float[] focalLengths,
                       bool    isAnamorphic,
                       float   squeezeFactor,
                       float   maxAperture  = 0f,
                       float   closeFocusM  = 0f)
        {
            this.CloseFocusM    = closeFocusM;
            this.FocalLengths   = focalLengths;
            this.IsAnamorphic   = isAnamorphic;
            this.IsZoom         = false;
            this.MaxAperture    = maxAperture;
            this.MaxFocalLength = focalLengths.Length > 0? focalLengths[focalLengths.Length - 1] : 0;
            this.MinFocalLength = focalLengths.Length > 0? focalLengths[0] : 0;
            this.Name           = name;
            this.SqueezeFactor  = squeezeFactor;
        }

        /// <summary>
        /// Creates a zoom lens set from a continuous focal range.
        /// </summary>
        public LensSet(string name,
                       float  minFocalLength,
                       float  maxFocalLength,
                       bool   isAnamorphic,
                       float  squeezeFactor,
                       float  maxAperture  = 0f,
                       float  closeFocusM  = 0f)
        {
            this.CloseFocusM    = closeFocusM;
            this.FocalLengths   = System.Array.Empty<float>();
            this.IsAnamorphic   = isAnamorphic;
            this.IsZoom         = true;
            this.MaxAperture    = maxAperture;
            this.MaxFocalLength = maxFocalLength;
            this.MinFocalLength = minFocalLength;
            this.Name           = name;
            this.SqueezeFactor  = squeezeFactor;
        }

        /// <summary>
        /// Minimum focusing distance in meters. 0 means unconstrained.
        /// </summary>
        public float CloseFocusM { get; }

        public float[] FocalLengths { get; }
        public bool    IsAnamorphic { get; }
        public bool    IsZoom       { get; }

        /// <summary>
        /// Widest aperture (lowest T-stop) this lens set supports. 0 means unconstrained (use full f/1.4–f/22 range).
        /// For prime sets, this is the widest T-stop across all lenses in the set.
        /// </summary>
        public float MaxAperture    { get; }

        public float MaxFocalLength { get; }
        public float MinFocalLength { get; }
        public string Name          { get; }
        public float  SqueezeFactor { get; }
    }
}