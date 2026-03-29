namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// The set of camera properties that can be independently recorded.
    /// Sealed class with private constructor — the set is closed.
    /// Position covers X/Y/Z together, Rotation covers pan/tilt/roll together.
    /// </summary>
    public sealed class CameraProperty
    {
        public static readonly CameraProperty APERTURE       = new(4, "Aperture");
        public static readonly CameraProperty FOCAL_LENGTH   = new(2, "Focal Length");
        public static readonly CameraProperty FOCUS_DISTANCE = new(3, "Focus Distance");
        public static readonly CameraProperty POSITION       = new(0, "Position");
        public static readonly CameraProperty ROTATION       = new(1, "Rotation");

        public static readonly int COUNT = 5;

        private CameraProperty(int index, string name)
        {
            this.Index = index;
            this.Name  = name;
        }

        public int    Index { get; }
        public string Name  { get; }

        public override string ToString() => this.Name;
    }
}
