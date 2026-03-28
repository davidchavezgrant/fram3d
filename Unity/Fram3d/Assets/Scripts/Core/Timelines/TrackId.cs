using System;
using Fram3d.Core.Common;

namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Identifies a track in the timeline. Either the single camera track
    /// or an element track keyed by ElementId.
    /// </summary>
    public sealed class TrackId : IEquatable<TrackId>
    {
        public static readonly TrackId Camera = new(null);

        private readonly ElementId _elementId;

        private TrackId(ElementId elementId)
        {
            this._elementId = elementId;
        }

        public ElementId ElementId => this._elementId;
        public bool      IsCamera  => this._elementId == null;
        public bool      IsElement => this._elementId != null;

        public static TrackId ForElement(ElementId id) =>
            new(id ?? throw new ArgumentNullException(nameof(id)));

        public          bool Equals(TrackId other) => other != null && Equals(this._elementId, other._elementId);
        public override bool Equals(object  obj)   => obj is TrackId other && this.Equals(other);
        public override int  GetHashCode()         => this._elementId?.GetHashCode() ?? 0;
    }
}
