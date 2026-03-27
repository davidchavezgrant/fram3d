using System;
using Fram3d.Core.Timelines;
using Timeline = Fram3d.Core.Timelines.Timeline;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// Time ruler with adaptive tick marks, frame dividers, playhead,
    /// out-of-range darkening, and click/drag scrub handling.
    /// </summary>
    public sealed class RulerElement : VisualElement
    {
        private const float  HEIGHT         = 22f;
        private const double FRAME_DURATION = 1.0 / 24.0;

        private readonly VisualElement _content;
        private readonly VisualElement _outOfRange;
        private readonly VisualElement _playhead;

        private bool _isScrubbing;

        public RulerElement()
        {
            this.AddToClassList("timeline-ruler-row");
            this.style.height = HEIGHT;

            var labelCol = new VisualElement();
            labelCol.AddToClassList("timeline-label-column");
            this.Add(labelCol);

            this._content = new VisualElement();
            this._content.AddToClassList("timeline-ruler");
            this.Add(this._content);

            this._playhead = new VisualElement();
            this._playhead.AddToClassList("timeline-playhead");
            this._playhead.style.display = DisplayStyle.None;

            var head = new VisualElement();
            head.AddToClassList("timeline-playhead__head");
            this._playhead.Add(head);
            this._content.Add(this._playhead);

            this._outOfRange = new VisualElement();
            this._outOfRange.AddToClassList("timeline-out-of-range");
            this._outOfRange.pickingMode = PickingMode.Ignore;
            this._content.Add(this._outOfRange);
        }

        /// <summary>
        /// Fires with the local pixel X when the user clicks or drags on the ruler.
        /// </summary>
        public event Action<float> ScrubRequested;

        public VisualElement Content => this._content;

        public bool IsScrubbing => this._isScrubbing;

        public void RegisterScrubCallbacks()
        {
            this._content.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0)
                {
                    return;
                }

                this._isScrubbing = true;
                this.ScrubRequested?.Invoke(evt.localPosition.x);
                this._content.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });

            this._content.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (this._isScrubbing)
                {
                    this.ScrubRequested?.Invoke(evt.localPosition.x);
                }
            });

            this._content.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (this._isScrubbing)
                {
                    this._isScrubbing = false;
                    this._content.ReleasePointer(evt.pointerId);
                }
            });
        }

        public void UpdatePlayhead(Timeline state, double currentTime)
        {
            var px = (float)state.TimeToPixel(currentTime);
            this._playhead.style.display = DisplayStyle.Flex;
            this._playhead.style.left    = px;
        }

        public void UpdateOutOfRange(Timeline state, double totalDuration)
        {
            var endPx = (float)state.TimeToPixel(totalDuration);
            this._outOfRange.style.position = Position.Absolute;
            this._outOfRange.style.left     = endPx;
            this._outOfRange.style.top      = 0;
            this._outOfRange.style.bottom   = 0;
            this._outOfRange.style.right    = 0;
            this._outOfRange.style.display  = endPx >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void UpdateTicks(Timeline state, double totalDuration)
        {
            // Remove old ticks (keep playhead and out-of-range)
            for (var i = this._content.childCount - 1; i >= 0; i--)
            {
                var child = this._content[i];

                if (child != this._playhead && child != this._outOfRange)
                {
                    child.RemoveFromHierarchy();
                }
            }

            var visibleDuration = state.VisibleDuration;
            var tickInterval    = ComputeTickInterval(visibleDuration);
            var majorInterval   = tickInterval * 5;
            var pxPerFrame      = state.PixelsPerSecond * FRAME_DURATION;
            var showFrameTicks  = pxPerFrame >= 4.0;
            var tickEnd         = Math.Min(state.ViewEnd, totalDuration);
            var firstTick       = Math.Ceiling(state.ViewStart / tickInterval) * tickInterval;

            if (showFrameTicks)
            {
                var firstFrame = Math.Ceiling(state.ViewStart / FRAME_DURATION) * FRAME_DURATION;

                for (var t = firstFrame; t <= tickEnd; t += FRAME_DURATION)
                {
                    if (t < 0)
                    {
                        continue;
                    }

                    var framePx   = state.TimeToPixel(t);
                    var frameTick = new VisualElement();
                    frameTick.AddToClassList("timeline-ruler__frame-tick");
                    frameTick.style.position = Position.Absolute;
                    frameTick.style.left     = (float)framePx;
                    frameTick.style.top      = 0;
                    frameTick.style.width    = 1;
                    frameTick.style.height   = 8;
                    this._content.Add(frameTick);
                }
            }

            for (var t = firstTick; t <= tickEnd; t += tickInterval)
            {
                if (t < 0)
                {
                    continue;
                }

                var px      = state.TimeToPixel(t);
                var isMajor = Math.Abs(t % majorInterval) < tickInterval * 0.1;
                var height  = isMajor ? HEIGHT : 10f;

                var tick = new VisualElement();
                tick.AddToClassList("timeline-ruler__tick");
                tick.style.position = Position.Absolute;
                tick.style.left     = (float)px;
                tick.style.top      = 0;
                tick.style.width    = 1;
                tick.style.height   = height;
                this._content.Add(tick);

                if (isMajor)
                {
                    var label = new Label(FormatRulerLabel(t));
                    label.AddToClassList("timeline-ruler__label");
                    label.style.position = Position.Absolute;
                    label.style.left     = (float)px + 3f;
                    label.style.top      = 2f;
                    this._content.Add(label);
                }
            }

            this._playhead.BringToFront();
        }

        private static double ComputeTickInterval(double visibleDuration)
        {
            if (visibleDuration <= 2)  { return 1.0 / 24.0; }
            if (visibleDuration <= 5)  { return 0.5; }
            if (visibleDuration <= 15) { return 1.0; }
            if (visibleDuration <= 40) { return 2.0; }
            if (visibleDuration <= 60) { return 5.0; }

            return 10.0;
        }

        private static string FormatRulerLabel(double seconds)
        {
            var m = (int)(seconds / 60);
            var s = seconds % 60;
            return $"{m}:{s:00.#}";
        }
    }
}
