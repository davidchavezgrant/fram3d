using Fram3d.Core.Timelines;
using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Views
{
    /// <summary>
    /// Sequence-global timecode overlay. Large, white, centered at the bottom
    /// of the view area (above the timeline). Always visible when shots exist.
    /// Updates every frame during playback and on scrub.
    /// </summary>
    public sealed class SequenceTimecodeOverlay: MonoBehaviour
    {
        private Fram3d.Core.Timelines.Timeline _controller;
        private Label                          _label;
        private ShotEvaluator                  _shotEvaluator;

        public static string FormatTimecode(double seconds, int fps)
        {
            var totalFrames = (int)(seconds      * fps);
            var h           = totalFrames / (fps * 3600);
            var remainder   = totalFrames % (fps * 3600);
            var m           = remainder   / (fps * 60);
            remainder = remainder % (fps  * 60);
            var s = remainder             / fps;
            var f = remainder             % fps;
            return $"{h:D2};{m:D2};{s:D2};{f:D2}";
        }

        private void Start()
        {
            this._shotEvaluator = FindAnyObjectByType<ShotEvaluator>();

            if (this._shotEvaluator == null)
            {
                return;
            }

            this._controller = this._shotEvaluator.Controller;
            var uiDoc = this.GetComponent<UIDocument>();

            if (uiDoc?.rootVisualElement == null)
            {
                return;
            }

            var root = uiDoc.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;

            this._label                               = new Label();
            this._label.pickingMode                   = PickingMode.Ignore;
            this._label.style.position                = Position.Absolute;
            this._label.style.left                    = 0;
            this._label.style.right                   = 0;
            this._label.style.bottom                  = this._shotEvaluator.BottomInsetPixels + 8;
            this._label.style.color                   = Color.white;
            this._label.style.fontSize                = 16;
            this._label.style.unityFontStyleAndWeight = FontStyle.Bold;
            this._label.style.unityTextAlign          = TextAnchor.LowerCenter;
            this._label.style.textShadow              = new TextShadow
            {
                offset     = new Vector2(1, 1),
                blurRadius = 3,
                color      = new Color(0, 0, 0, 0.9f)
            };
            root.Add(this._label);
        }

        private void Update()
        {
            if (this._label == null || this._controller == null)
            {
                return;
            }

            if (this._controller.Count == 0)
            {
                this._label.style.display = DisplayStyle.None;
                return;
            }

            // Keep bottom offset in sync with timeline height
            this._label.style.bottom = this._shotEvaluator.BottomInsetPixels + 8;

            var fps = (int)this._controller.FrameRate.Fps;
            this._label.text          = FormatTimecode(this._controller.Playhead.CurrentTime, fps);
            this._label.style.display = DisplayStyle.Flex;
        }
    }
}
