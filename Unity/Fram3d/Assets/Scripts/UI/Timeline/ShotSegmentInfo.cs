using UnityEngine;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// Data for rendering a shot-colored background segment on a track row.
    /// </summary>
    public struct ShotSegmentInfo
    {
        public Color Color;
        public bool  IsActive;
        public float LeftPx;
        public float WidthPx;
    }
}
