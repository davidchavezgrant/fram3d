using System;
using System.Collections.Generic;
using Fram3d.Core.Common;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Registry of element animation tracks keyed by ElementId.
    /// Each element gets one track with global position and rotation keyframes.
    /// Elements without tracks stay at their current position.
    /// </summary>
    public sealed class ElementTimeline
    {
        private readonly Dictionary<ElementId, ElementTrack> _tracks = new();
        public           int                                 TrackCount => this._tracks.Count;
        public           IReadOnlyCollection<ElementTrack>   Tracks     => this._tracks.Values;

        public ElementTrack GetOrCreateTrack(ElementId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (this._tracks.TryGetValue(id, out var existing))
            {
                return existing;
            }

            var track = new ElementTrack(id);
            this._tracks[id] = track;
            return track;
        }

        public ElementTrack GetTrack(ElementId id)
        {
            this._tracks.TryGetValue(id, out var track);
            return track;
        }

        public bool HasTrack(ElementId    id) => this._tracks.ContainsKey(id);
        public bool RemoveTrack(ElementId id) => this._tracks.Remove(id);
    }
}