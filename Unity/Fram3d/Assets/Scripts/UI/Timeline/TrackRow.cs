using System;
using System.Collections.Generic;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// A track row in the timeline. Shows the track header (collapse arrow, name)
    /// and main keyframe diamonds when collapsed. When expanded, shows SubTrackRows.
    /// </summary>
    public sealed class TrackRow : VisualElement
    {
        private readonly VisualElement         _arrow;
        private readonly VisualElement         _content;
        private readonly List<KeyframeDiamond> _diamonds     = new();
        private readonly bool                  _isCamera;
        private readonly VisualElement         _subContainer;
        private readonly List<SubTrackRow>     _subTracks    = new();
        private readonly TrackId               _trackId;

        public TrackRow(TrackId trackId, string name, bool isCamera)
        {
            this._trackId  = trackId;
            this._isCamera = isCamera;
            this.AddToClassList("track-row");

            if (isCamera)
            {
                this.AddToClassList("track-row--camera");
            }
            else
            {
                this.AddToClassList("track-row--element");
            }

            // Header row
            var header = new VisualElement();
            header.AddToClassList("track-header");

            var labels = new VisualElement();
            labels.AddToClassList("track-label-column");

            this._arrow = new VisualElement();
            this._arrow.AddToClassList("track-arrow");
            this._arrow.AddToClassList("track-arrow--collapsed");
            this._arrow.RegisterCallback<ClickEvent>(_ => this.ArrowClicked?.Invoke(this._trackId));
            labels.Add(this._arrow);

            var nameLabel = new Label(name);
            nameLabel.AddToClassList("track-name");
            labels.Add(nameLabel);

            header.Add(labels);

            this._content = new VisualElement();
            this._content.AddToClassList("track-content");
            header.Add(this._content);

            this.Add(header);

            // Sub-track container (hidden by default)
            this._subContainer = new VisualElement();
            this._subContainer.AddToClassList("sub-track-container");
            this._subContainer.style.display = DisplayStyle.None;
            this.Add(this._subContainer);
        }

        public event Action<TrackId>                  ArrowClicked;
        public event Action<KeyframeId, TimePosition> DiamondClicked;

        public IReadOnlyList<SubTrackRow> SubTracks => this._subTracks;
        public TrackId                    TrackId   => this._trackId;

        public SubTrackRow AddSubTrack(string propertyName)
        {
            var row = new SubTrackRow(propertyName, this._isCamera);
            row.DiamondClicked += (id, time) => this.DiamondClicked?.Invoke(id, time);
            this._subTracks.Add(row);
            this._subContainer.Add(row);
            return row;
        }

        public void SetExpanded(bool expanded)
        {
            if (expanded)
            {
                this._subContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                this._subContainer.style.display = DisplayStyle.None;
            }

            this._arrow.EnableInClassList("track-arrow--collapsed", !expanded);
            this._arrow.EnableInClassList("track-arrow--expanded", expanded);
        }

        /// <summary>
        /// Update the main keyframe diamonds on the collapsed header row.
        /// </summary>
        public void UpdateMainDiamonds(
            IReadOnlyList<TimePosition> times,
            Func<double, double>        timeToPixel,
            KeyframeSelection           selection)
        {
            // Remove excess diamonds
            while (this._diamonds.Count > times.Count)
            {
                var last = this._diamonds[this._diamonds.Count - 1];
                this._content.Remove(last);
                this._diamonds.RemoveAt(this._diamonds.Count - 1);
            }

            // Add missing diamonds
            while (this._diamonds.Count < times.Count)
            {
                var diamond = new KeyframeDiamond();
                diamond.SetColor(this._isCamera);
                var idx = this._diamonds.Count;
                diamond.RegisterCallback<ClickEvent>(_ =>
                {
                    if (idx < times.Count)
                    {
                        this.DiamondClicked?.Invoke(null, times[idx]);
                    }
                });
                this._diamonds.Add(diamond);
                this._content.Add(diamond);
            }

            // Position and update selection state
            for (var i = 0; i < times.Count; i++)
            {
                var px = (float)timeToPixel(times[i].Seconds);
                this._diamonds[i].style.left = px - 5f;
                // Main diamonds are selected if any keyframe at that time is selected
                var isSelected = selection != null
                    && selection.HasSelection
                    && selection.TrackId != null
                    && selection.TrackId.Equals(this._trackId)
                    && selection.Time != null
                    && selection.Time.Equals(times[i]);
                this._diamonds[i].SetSelected(isSelected);
            }
        }
    }
}
