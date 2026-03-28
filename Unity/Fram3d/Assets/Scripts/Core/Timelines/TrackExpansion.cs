using System.Collections.Generic;

namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Tracks which timeline tracks are expanded (showing sub-tracks).
    /// All tracks default to collapsed.
    /// </summary>
    public sealed class TrackExpansion
    {
        private readonly HashSet<TrackId> _expanded = new();

        public bool IsExpanded(TrackId trackId) => this._expanded.Contains(trackId);

        public void Toggle(TrackId trackId)
        {
            if (!this._expanded.Remove(trackId))
            {
                this._expanded.Add(trackId);
            }
        }
    }
}
