using System;
using Fram3d.Core.Shots;
using Fram3d.Core.Timeline;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// Transport bar: play/stop button, timecode display, shot name.
    /// </summary>
    public sealed class TransportBarElement : VisualElement
    {
        private const float HEIGHT = 28f;

        private readonly Button _playButton;
        private readonly Label  _durationLabel;
        private readonly Label  _shotLabel;
        private readonly Label  _timeLabel;

        public TransportBarElement(Action onPlayClicked)
        {
            this.AddToClassList("timeline-transport");
            this.style.height = HEIGHT;

            this._playButton = new Button(onPlayClicked);
            this._playButton.text = "\u25b6";
            this._playButton.AddToClassList("timeline-transport__play");
            this.Add(this._playButton);

            this._timeLabel = new Label("00;00;00;00");
            this._timeLabel.AddToClassList("timeline-transport__time");
            this.Add(this._timeLabel);

            var divider = new Label("/");
            divider.AddToClassList("timeline-transport__divider");
            this.Add(divider);

            this._durationLabel = new Label("00;00;05;00");
            this._durationLabel.AddToClassList("timeline-transport__time");
            this.Add(this._durationLabel);

            this._shotLabel = new Label();
            this._shotLabel.AddToClassList("timeline-transport__shot");
            this.Add(this._shotLabel);
        }

        public void UpdatePlayButton(bool isPlaying)
        {
            if (isPlaying)
            {
                this._playButton.text = "\u25a0";
                this._playButton.AddToClassList("timeline-transport__play--active");
            }
            else
            {
                this._playButton.text = "\u25b6";
                this._playButton.RemoveFromClassList("timeline-transport__play--active");
            }
        }

        public void UpdateTransport(Playhead playhead, ShotRegistry registry)
        {
            var current = registry.CurrentShot;

            if (current == null)
            {
                this._timeLabel.text     = "00;00;00;00";
                this._durationLabel.text = "00;00;00;00";
                this._shotLabel.text     = "";
                return;
            }

            var startTime = registry.GetGlobalStartTime(current.Id).Seconds;
            var localTime = Math.Max(0, playhead.CurrentTime - startTime);
            localTime     = Math.Min(localTime, current.Duration);

            this._timeLabel.text     = FormatTimecode(localTime, 24);
            this._durationLabel.text = FormatTimecode(current.Duration, 24);
            this._shotLabel.text     = current.Name;
        }

        private static string FormatTimecode(double seconds, int fps)
        {
            var totalFrames = (int)(seconds * fps);
            var h           = totalFrames / (fps * 3600);
            var remainder   = totalFrames % (fps * 3600);
            var m           = remainder / (fps * 60);
            remainder       = remainder % (fps * 60);
            var s           = remainder / fps;
            var f           = remainder % fps;
            return $"{h:D2};{m:D2};{s:D2};{f:D2}";
        }
    }
}
