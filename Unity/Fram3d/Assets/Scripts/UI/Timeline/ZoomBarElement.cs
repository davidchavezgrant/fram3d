using System;
using Fram3d.Core.Timelines;
using Timeline = Fram3d.Core.Timelines.Timeline;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// Zoom bar with a draggable thumb representing the visible time range.
    /// </summary>
    public sealed class ZoomBarElement : VisualElement
    {
        private const float HEIGHT = 18f;

        private readonly VisualElement _bar;
        private readonly VisualElement _playhead;
        private readonly VisualElement _thumb;

        private bool  _isDragging;
        private float _dragStartX;

        public ZoomBarElement()
        {
            this.AddToClassList("timeline-zoom-row");
            this.style.height = HEIGHT;

            var labelCol = new VisualElement();
            labelCol.AddToClassList("timeline-label-column");
            labelCol.style.height = HEIGHT;
            this.Add(labelCol);

            this._bar = new VisualElement();
            this._bar.AddToClassList("timeline-zoom-bar");
            this.Add(this._bar);

            this._thumb = new VisualElement();
            this._thumb.AddToClassList("timeline-zoom-thumb");
            this._bar.Add(this._thumb);

            this._playhead = new VisualElement();
            this._playhead.AddToClassList("timeline-zoom-playhead");
            this._playhead.style.display = DisplayStyle.None;
            this._bar.Add(this._playhead);
        }

        /// <summary>
        /// Fires with pixel delta when the user drags the zoom thumb.
        /// </summary>
        public event Action<float> PanRequested;

        public void RegisterDragCallbacks()
        {
            this._thumb.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0)
                {
                    return;
                }

                this._isDragging = true;
                this._dragStartX = evt.localPosition.x;
                this._thumb.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });

            this._thumb.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!this._isDragging)
                {
                    return;
                }

                var delta = evt.localPosition.x - this._dragStartX;
                this.PanRequested?.Invoke(delta);
            });

            this._thumb.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (this._isDragging)
                {
                    this._isDragging = false;
                    this._thumb.ReleasePointer(evt.pointerId);
                }
            });
        }

        public void UpdateThumb(Timeline state, double totalDuration)
        {
            if (totalDuration <= 0)
            {
                totalDuration = 5.0;
            }

            var barWidth = this._bar.resolvedStyle.width;

            if (float.IsNaN(barWidth) || barWidth <= 0)
            {
                return;
            }

            var startFrac  = Math.Max(0, state.ViewStart / totalDuration);
            var endFrac    = Math.Min(1, state.ViewEnd / totalDuration);
            var thumbLeft  = startFrac * barWidth;
            var thumbWidth = (endFrac - startFrac) * barWidth;

            if (thumbWidth < 30)
            {
                thumbWidth = 30;
            }

            this._thumb.style.position = Position.Absolute;
            this._thumb.style.left     = (float)thumbLeft;
            this._thumb.style.width    = (float)thumbWidth;
            this._thumb.style.top      = 2;
            this._thumb.style.height   = 14;
        }
    }
}
