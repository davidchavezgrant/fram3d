using System.Collections.Generic;
using Fram3d.Core.Common;

namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Tracks which element tracks are hidden by the user.
    /// All tracks default to visible.
    /// </summary>
    public sealed class TrackVisibility
    {
        private readonly HashSet<TrackId> _hidden      = new();
        private          bool             _showHidden;

        public bool ShowHidden
        {
            get => this._showHidden;
            set => this._showHidden = value;
        }

        public void Hide(TrackId trackId) => this._hidden.Add(trackId);

        public bool IsHidden(TrackId trackId) => this._hidden.Contains(trackId);

        public bool IsVisible(TrackId trackId) =>
            this._showHidden || !this._hidden.Contains(trackId);

        public void Show(TrackId trackId) => this._hidden.Remove(trackId);

        public void ToggleHidden(TrackId trackId)
        {
            if (!this._hidden.Remove(trackId))
            {
                this._hidden.Add(trackId);
            }
        }
    }
}
