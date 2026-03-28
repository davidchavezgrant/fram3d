using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// Status bar at the bottom of the timeline showing contextual keyboard hints.
    /// Later milestones (3.2.3 stopwatch, 3.2.4 keyframe interaction) will update
    /// the hint text based on selection and recording state.
    /// </summary>
    public sealed class StatusBar: VisualElement
    {
        private const  float HEIGHT       = 22f;
        private static readonly string DEFAULT_HINTS = "Space: Play/Pause   +/\u2212: Zoom   T: Toggle   \\: Fit All   Home/End: Jump";
        private readonly Label _hints;

        public StatusBar()
        {
            this.AddToClassList("timeline-status-bar");
            this.style.height        = HEIGHT;
            this.style.flexDirection  = FlexDirection.Row;
            this.style.alignItems    = Align.Center;
            this._hints              = new Label(DEFAULT_HINTS);
            this._hints.AddToClassList("timeline-status-bar__hints");
            this.Add(this._hints);
        }

        public void SetHints(string text) => this._hints.text = text;

        public void ResetHints() => this._hints.text = DEFAULT_HINTS;
    }
}
