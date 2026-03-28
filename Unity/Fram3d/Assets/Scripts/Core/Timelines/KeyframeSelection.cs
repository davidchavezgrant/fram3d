using System;
using Fram3d.Core.Common;

namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Tracks the single selected keyframe across all tracks.
    /// Selecting a keyframe on one track deselects on every other track.
    /// </summary>
    public sealed class KeyframeSelection
    {
        private readonly Subject<bool> _changed = new();

        public IObservable<bool> Changed      => this._changed;
        public bool              HasSelection => this.KeyframeId != null;
        public KeyframeId        KeyframeId   { get; private set; }
        public TimePosition      Time         { get; private set; }
        public TrackId           TrackId      { get; private set; }

        public void Clear()
        {
            this.KeyframeId = null;
            this.Time       = null;
            this.TrackId    = null;
            this._changed.OnNext(false);
        }

        public bool IsSelected(KeyframeId id) =>
            this.KeyframeId != null && this.KeyframeId.Equals(id);

        public void Select(TrackId trackId, KeyframeId keyframeId, TimePosition time)
        {
            this.TrackId    = trackId;
            this.KeyframeId = keyframeId;
            this.Time       = time;
            this._changed.OnNext(true);
        }
    }
}
