namespace Fram3d.Core.Camera
{
    public sealed class LensSet
    {
        public string  Name          { get; }
        public bool    IsZoom        { get; }
        public bool    IsAnamorphic  { get; }
        public float   SqueezeFactor { get; }
        public float[] FocalLengths  { get; }
        public float   MinFocalLength    { get; }
        public float   MaxFocalLength    { get; }

        /// <summary>
        /// Creates a prime lens set from a list of fixed focal lengths.
        /// </summary>
        public LensSet(string  name,
                       float[] focalLengths,
                       bool    isAnamorphic,
                       float   squeezeFactor)
        {
            this.Name          = name;
            this.IsZoom        = false;
            this.IsAnamorphic  = isAnamorphic;
            this.SqueezeFactor = squeezeFactor;
            this.FocalLengths  = focalLengths;
            this.MinFocalLength    = focalLengths.Length > 0? focalLengths[0] : 0;
            this.MaxFocalLength    = focalLengths.Length > 0? focalLengths[focalLengths.Length - 1] : 0;
        }

        /// <summary>
        /// Creates a zoom lens set from a continuous focal range.
        /// </summary>
        public LensSet(string name,
                       float  minFocalLength,
                       float  maxFocalLength,
                       bool   isAnamorphic,
                       float  squeezeFactor)
        {
            this.Name          = name;
            this.IsZoom        = true;
            this.IsAnamorphic  = isAnamorphic;
            this.SqueezeFactor = squeezeFactor;
            this.FocalLengths  = System.Array.Empty<float>();
            this.MinFocalLength    = minFocalLength;
            this.MaxFocalLength    = maxFocalLength;
        }
    }
}