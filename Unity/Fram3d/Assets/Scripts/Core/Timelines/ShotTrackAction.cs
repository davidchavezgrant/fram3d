namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Result of a shot track pointer event. Sealed class pattern —
    /// closed set, no switch statements needed.
    /// </summary>
    public sealed class ShotTrackAction
    {
        public static readonly ShotTrackAction BOUNDARY_COMPLETE = new("Boundary Complete");
        public static readonly ShotTrackAction BOUNDARY_DRAG     = new("Boundary Drag");
        public static readonly ShotTrackAction CLICK             = new("Click");
        public static readonly ShotTrackAction DRAG_COMPLETE     = new("Drag Complete");
        public static readonly ShotTrackAction DRAG_MOVE         = new("Drag Move");
        public static readonly ShotTrackAction DRAG_START        = new("Drag Start");
        public static readonly ShotTrackAction NEAR_EDGE         = new("Near Edge");
        public static readonly ShotTrackAction NONE              = new("None");
        public static readonly ShotTrackAction POTENTIAL_CLICK   = new("Potential Click");

        private ShotTrackAction(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public override string ToString() => this.Name;
    }
}
