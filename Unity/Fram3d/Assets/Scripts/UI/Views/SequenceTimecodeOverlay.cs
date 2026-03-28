using System;
using Fram3d.Core.Timelines;
using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Views
{
    /// <summary>
    /// Bottom-center overlay on the Camera View showing the sequence-global
    /// timecode (HH;MM;SS;FF). Updates every frame during playback and on scrub.
    /// </summary>
    public sealed class SequenceTimecodeOverlay: MonoBehaviour
    {
        private Fram3d.Core.Timelines.Timeline _controller;
        private Label                          _label;

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
            var shotEvaluator = FindAnyObjectByType<ShotEvaluator>();

            if (shotEvaluator == null)
            {
                return;
            }

            this._controller = shotEvaluator.Controller;
            var uiDoc = this.GetComponent<UIDocument>();

            if (uiDoc?.rootVisualElement == null)
            {
                return;
            }

            this._label             = new Label();
            this._label.pickingMode = PickingMode.Ignore;
            this._label.AddToClassList("sequence-timecode-overlay");
            uiDoc.rootVisualElement.Add(this._label);
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

            var fps = (int)this._controller.FrameRate.Fps;
            this._label.text          = FormatTimecode(this._controller.Playhead.CurrentTime, fps);
            this._label.style.display = DisplayStyle.Flex;
        }
    }
}
