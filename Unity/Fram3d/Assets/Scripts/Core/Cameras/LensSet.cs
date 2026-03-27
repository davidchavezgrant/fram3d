using System;
using System.Linq;
namespace Fram3d.Core.Cameras
{
    public sealed class LensSet
    {
        /// <summary>
        /// Creates a prime lens set from per-lens specs.
        /// </summary>
        public LensSet(string     name,
                       LensSpec[] specs,
                       bool       isAnamorphic,
                       float      squeezeFactor)
        {
            this.CloseFocusM    = specs.Length > 0? specs.Where(s => s.CloseFocusM > 0).Select(s => s.CloseFocusM).DefaultIfEmpty(0f).Min() : 0f;
            this.FocalLengths   = specs.Select(s => s.FocalLength).ToArray();
            this.IsAnamorphic   = isAnamorphic;
            this.IsZoom         = false;
            this.MaxAperture    = specs.Length > 0? specs.Where(s => s.MaxAperture > 0).Select(s => s.MaxAperture).DefaultIfEmpty(0f).Min() : 0f;
            this.MaxFocalLength = this.FocalLengths.Length > 0? this.FocalLengths[this.FocalLengths.Length - 1] : 0;
            this.MinFocalLength = this.FocalLengths.Length > 0? this.FocalLengths[0] : 0;
            this.Name           = name;
            this.Specs          = specs;
            this.SqueezeFactor  = squeezeFactor;
        }

        /// <summary>
        /// Convenience constructor for prime lens sets without per-lens aperture/focus data.
        /// Applies set-level values uniformly to all lenses.
        /// </summary>
        public LensSet(string  name,
                       float[] focalLengths,
                       bool    isAnamorphic,
                       float   squeezeFactor,
                       float   maxAperture = 0f,
                       float   closeFocusM = 0f): this(name,
                                                       focalLengths.Select(f => new LensSpec(f, maxAperture, closeFocusM)).ToArray(),
                                                       isAnamorphic,
                                                       squeezeFactor) {}

        /// <summary>
        /// Creates a zoom lens set from a continuous focal range.
        /// </summary>
        public LensSet(string name,
                       float  minFocalLength,
                       float  maxFocalLength,
                       bool   isAnamorphic,
                       float  squeezeFactor,
                       float  maxAperture = 0f,
                       float  closeFocusM = 0f)
        {
            this.CloseFocusM    = closeFocusM;
            this.FocalLengths   = Array.Empty<float>();
            this.IsAnamorphic   = isAnamorphic;
            this.IsZoom         = true;
            this.MaxAperture    = maxAperture;
            this.MaxFocalLength = maxFocalLength;
            this.MinFocalLength = minFocalLength;
            this.Name           = name;
            this.Specs          = Array.Empty<LensSpec>();
            this.SqueezeFactor  = squeezeFactor;
        }

        /// <summary>
        /// Minimum focusing distance in meters (set-level). 0 means unconstrained.
        /// For primes, this is the closest focus across all lenses in the set.
        /// </summary>
        public float CloseFocusM { get; }

        public float[] FocalLengths { get; }
        public bool    IsAnamorphic { get; }
        public bool    IsZoom       { get; }

        /// <summary>
        /// Widest aperture (lowest T-stop) this lens set supports (set-level). 0 means unconstrained.
        /// For primes, this is the widest T-stop across all lenses in the set.
        /// Use Specs[index] for per-lens values.
        /// </summary>
        public float MaxAperture { get; }

        public float  MaxFocalLength { get; }
        public float  MinFocalLength { get; }
        public string Name           { get; }

        /// <summary>
        /// Per-lens specs for prime sets. Empty for zoom sets.
        /// Ordered by focal length (shortest to longest).
        /// </summary>
        public LensSpec[] Specs { get; }

        public float SqueezeFactor { get; }
    }
}