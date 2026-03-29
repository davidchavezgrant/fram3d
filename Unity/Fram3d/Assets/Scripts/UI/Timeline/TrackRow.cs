using System;
using System.Collections.Generic;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// A track row in the timeline. Shows the track header (collapse arrow, name)
    /// and main keyframe diamonds when collapsed. When expanded, shows SubTrackRows.
    /// </summary>
    public sealed class TrackRow : VisualElement
    {
        private const    float                   ACTIVE_ALPHA      = 0.18f;
        private const    float                   DRAG_THRESHOLD_PX = 4f;
        private const    float                   INACTIVE_ALPHA    = 0.08f;
        private readonly VisualElement         _arrow;
        private readonly VisualElement         _content;
        private readonly List<KeyframeDiamond> _diamonds     = new();
        private readonly bool                  _isCamera;
        private readonly List<VisualElement>   _segments     = new();
        private readonly VisualElement         _stopwatch;
        private readonly VisualElement         _subContainer;
        private readonly List<SubTrackRow>     _subTracks    = new();
        private readonly TrackId               _trackId;
        private          int                   _dragDiamondIdx = -1;
        private          bool                  _isDragging;
        private          int                   _pendingPointerId = -1;
        private          float                 _pointerDownX;

        // Stored reference so UpdateMainDiamonds can read times during drag
        private IReadOnlyList<TimePosition> _currentTimes;

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

            this._stopwatch = new VisualElement();
            this._stopwatch.AddToClassList("track-stopwatch");
            this._stopwatch.RegisterCallback<ClickEvent>(_ => this.StopwatchClicked?.Invoke(this._trackId));
            labels.Add(this._stopwatch);

            var typeDot = new VisualElement();
            typeDot.AddToClassList("track-type-dot");
            typeDot.AddToClassList(isCamera ? "track-type-dot--camera" : "track-type-dot--element");
            labels.Add(typeDot);

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

        public event Action<TrackId>                          ArrowClicked;
        public event Action<TrackId, KeyframeId, TimePosition> DiamondClicked;

        /// <summary>
        /// Fired continuously during a diamond drag. Provides the track-content-local X pixel.
        /// </summary>
        public event Action<TimePosition, float>      DiamondDragging;

        /// <summary>
        /// Fired when a diamond drag completes. Provides the original time and final track-content-local X pixel.
        /// </summary>
        public event Action<TimePosition, float>      DiamondDropped;

        public event Action<TrackId>                  StopwatchClicked;

        public IReadOnlyList<SubTrackRow> SubTracks => this._subTracks;
        public TrackId                    TrackId   => this._trackId;

        public SubTrackRow AddSubTrack(string propertyName)
        {
            var row = new SubTrackRow(propertyName, this._isCamera);
            row.OwnerTrackId = this._trackId;
            row.DiamondClicked += (trackId, id, time) => this.DiamondClicked?.Invoke(trackId, id, time);
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

        public void SetStopwatchState(bool anyOn, bool allOn)
        {
            this._stopwatch.EnableInClassList("track-stopwatch--on", allOn);
            this._stopwatch.EnableInClassList("track-stopwatch--partial", anyOn && !allOn);
        }

        /// <summary>
        /// Updates the colored background segments that indicate shot regions.
        /// Active shot gets full brightness; inactive shots are dimmed.
        /// For camera tracks, segments use the shot palette color.
        /// For element tracks, segments use a neutral tint.
        /// </summary>
        public void UpdateShotSegments(ShotSegmentInfo[] shots)
        {
            // Pool: remove excess
            while (this._segments.Count > shots.Length)
            {
                var last = this._segments[this._segments.Count - 1];
                this._content.Remove(last);
                this._segments.RemoveAt(this._segments.Count - 1);
            }

            // Pool: add missing
            while (this._segments.Count < shots.Length)
            {
                var seg = new VisualElement();
                seg.AddToClassList("track-shot-segment");
                seg.pickingMode = PickingMode.Ignore;
                this._segments.Add(seg);
                this._content.Insert(this._segments.Count - 1, seg);
            }

            // Position and color each segment
            var hatch = HatchTexture.Get();

            for (var i = 0; i < shots.Length; i++)
            {
                var info    = shots[i];
                var seg     = this._segments[i];
                var alpha   = info.IsActive ? ACTIVE_ALPHA : INACTIVE_ALPHA;
                var color   = this._isCamera ? info.Color : new Color(0.6f, 0.6f, 0.6f);

                seg.style.position        = Position.Absolute;
                seg.style.left            = info.LeftPx;
                seg.style.width           = info.WidthPx;
                seg.style.top             = 0;
                seg.style.bottom          = 0;
                seg.style.backgroundColor = new Color(color.r, color.g, color.b, alpha);

                if (info.IsActive)
                {
                    seg.style.backgroundImage = StyleKeyword.None;
                }
                else
                {
                    seg.style.backgroundImage = new StyleBackground(hatch);
                }

                seg.EnableInClassList("track-shot-segment--active", info.IsActive);
                seg.EnableInClassList("track-shot-segment--inactive", !info.IsActive);
            }
        }

        /// <summary>
        /// Update the main keyframe diamonds on the collapsed header row.
        /// activeStart/activeEnd define the global time range of the active shot
        /// (used to dim element keyframes in inactive regions).
        /// Pass null to skip dimming (camera tracks, where all diamonds are in-range).
        /// </summary>
        public void UpdateMainDiamonds(
            IReadOnlyList<TimePosition> times,
            Func<double, double>        timeToPixel,
            KeyframeSelection           selection,
            TimePosition                activeStart = null,
            TimePosition                activeEnd   = null)
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
                diamond.RegisterCallback<PointerDownEvent>(evt =>
                {
                    if (evt.button != 0 || idx >= this._currentTimes.Count)
                    {
                        return;
                    }

                    this._dragDiamondIdx  = idx;
                    this._isDragging      = false;
                    this._pendingPointerId = evt.pointerId;
                    this._pointerDownX    = this._content.WorldToLocal(evt.position).x;
                    diamond.CapturePointer(evt.pointerId);
                    evt.StopPropagation();
                });
                diamond.RegisterCallback<PointerMoveEvent>(evt =>
                {
                    if (this._dragDiamondIdx != idx || this._pendingPointerId < 0)
                    {
                        return;
                    }

                    var localX = this._content.WorldToLocal(evt.position).x;

                    if (!this._isDragging)
                    {
                        if (Math.Abs(localX - this._pointerDownX) < DRAG_THRESHOLD_PX)
                        {
                            return;
                        }

                        this._isDragging = true;
                    }

                    this.DiamondDragging?.Invoke(this._currentTimes[idx], localX);
                });
                diamond.RegisterCallback<PointerUpEvent>(evt =>
                {
                    if (this._dragDiamondIdx != idx)
                    {
                        return;
                    }

                    diamond.ReleasePointer(this._pendingPointerId);

                    if (this._isDragging)
                    {
                        var localX = this._content.WorldToLocal(evt.position).x;
                        this.DiamondDropped?.Invoke(this._currentTimes[idx], localX);
                    }
                    else if (idx < this._currentTimes.Count)
                    {
                        this.DiamondClicked?.Invoke(this._trackId, null, this._currentTimes[idx]);
                    }

                    this._dragDiamondIdx   = -1;
                    this._isDragging       = false;
                    this._pendingPointerId = -1;
                });
                this._diamonds.Add(diamond);
                this._content.Add(diamond);
            }

            this._currentTimes = times;

            // Position, selection, and active-region dimming
            for (var i = 0; i < times.Count; i++)
            {
                var px = (float)timeToPixel(times[i].Seconds);
                this._diamonds[i].style.left = px - 5f;

                var isSelected = selection != null
                    && selection.HasSelection
                    && selection.TrackId != null
                    && selection.TrackId.Equals(this._trackId)
                    && selection.Time != null
                    && selection.Time.Equals(times[i]);
                this._diamonds[i].SetSelected(isSelected);

                // Dim diamonds outside the active shot region (element tracks only)
                if (activeStart != null && activeEnd != null)
                {
                    var inRange = times[i] >= activeStart && times[i] <= activeEnd;
                    this._diamonds[i].style.opacity = inRange ? 1f : 0.35f;
                }
                else
                {
                    this._diamonds[i].style.opacity = 1f;
                }
            }
        }
    }
}
