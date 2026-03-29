using System;
using System.Collections.Generic;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// A property sub-track row within an expanded track.
    /// Shows property name, live interpolated value, and keyframe diamonds.
    /// </summary>
    public sealed class SubTrackRow : VisualElement
    {
        private readonly VisualElement         _content;
        private readonly List<KeyframeDiamond> _diamonds = new();
        private readonly bool                  _isCamera;
        private readonly Label                 _nameLabel;
        private readonly Label                 _valueLabel;

        public SubTrackRow(string propertyName, bool isCamera)
        {
            this._isCamera = isCamera;
            this.AddToClassList("sub-track-row");

            var labels = new VisualElement();
            labels.AddToClassList("sub-track-label-column");

            this._nameLabel = new Label(propertyName);
            this._nameLabel.AddToClassList("sub-track-name");
            labels.Add(this._nameLabel);

            this._valueLabel = new Label("\u2014");
            this._valueLabel.AddToClassList("sub-track-value");
            labels.Add(this._valueLabel);

            this.Add(labels);

            this._content = new VisualElement();
            this._content.AddToClassList("sub-track-content");
            this.Add(this._content);
        }

        public event Action<TrackId, KeyframeId, TimePosition> DiamondClicked;

        public TrackId OwnerTrackId { get; set; }

        public void SetValue(string formattedValue) =>
            this._valueLabel.text = formattedValue;

        /// <summary>
        /// Updates diamond positions from TimePositions and KeyframeIds.
        /// Diamonds are pooled -- grown/shrunk as needed, never recreated from scratch.
        /// </summary>
        public void UpdateDiamondPositions(
            IReadOnlyList<TimePosition> times,
            IReadOnlyList<KeyframeId>   ids,
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
                    if (idx < ids.Count)
                    {
                        this.DiamondClicked?.Invoke(this.OwnerTrackId, ids[idx], times[idx]);
                    }
                });
                this._diamonds.Add(diamond);
                this._content.Add(diamond);
            }

            // Position and update selection state
            for (var i = 0; i < times.Count; i++)
            {
                var px = (float)timeToPixel(times[i].Seconds);
                this._diamonds[i].style.left = px - 5f; // center the 10px diamond
                this._diamonds[i].SetSelected(selection != null && i < ids.Count && selection.IsSelected(ids[i]));
            }
        }
    }
}
