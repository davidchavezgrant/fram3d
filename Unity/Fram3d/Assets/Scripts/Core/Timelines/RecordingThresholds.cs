namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Change-detection thresholds for keyframe recording. A property must change
    /// by more than its threshold to trigger a keyframe write.
    /// </summary>
    public static class RecordingThresholds
    {
        public const float  FOCAL_LENGTH   = 0.01f;
        public const float  FOCUS_DISTANCE = 0.001f;
        public const double MERGE_WINDOW   = 0.1;
        public const float  POSITION       = 0.001f;
        public const float  ROTATION_DEG   = 0.01f;
        public const float  SCALE          = 0.001f;
    }
}
