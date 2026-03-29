using Fram3d.Core.Timelines;
using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Views
{
    /// <summary>
    /// Sequence-global timecode overlay. Large, white, centered at the bottom
    /// of the view area. Uses ViewportScope to stay within the 3D view panel
    /// regardless of split layout or properties panel width.
    /// </summary>
    public sealed class SequenceTimecodeOverlay: MonoBehaviour
    {
        private CameraBehaviour _cameraBehaviour;
        private VisualElement   _container;
        private Fram3d.Core.Timelines.Timeline _controller;
        private Label                          _label;
        private VisualElement                  _root;
        private ViewCameraManager              _viewCameraManager;

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

            this._controller        = shotEvaluator.Controller;
            this._cameraBehaviour   = FindAnyObjectByType<CameraBehaviour>();
            this._viewCameraManager = FindAnyObjectByType<ViewCameraManager>();

            var uiDoc = this.GetComponent<UIDocument>();

            if (uiDoc?.rootVisualElement == null)
            {
                return;
            }

            this._root             = uiDoc.rootVisualElement;
            this._root.pickingMode = PickingMode.Ignore;

            this._container                          = new VisualElement();
            this._container.pickingMode              = PickingMode.Ignore;
            this._container.style.position           = Position.Absolute;
            this._container.style.justifyContent     = Justify.FlexEnd;
            this._container.style.alignItems         = Align.Center;
            this._root.Add(this._container);

            this._label                               = new Label();
            this._label.pickingMode                   = PickingMode.Ignore;
            this._label.style.color                   = Color.white;
            this._label.style.fontSize                = 16;
            this._label.style.unityFontStyleAndWeight = FontStyle.Bold;
            this._label.style.unityTextAlign          = TextAnchor.MiddleCenter;
            this._label.style.marginBottom            = 8;
            this._label.style.textShadow              = new TextShadow
            {
                offset     = new Vector2(1, 1),
                blurRadius = 3,
                color      = new Color(0, 0, 0, 0.9f)
            };
            this._container.Add(this._label);
        }

        private void Update()
        {
            if (this._label == null || this._controller == null)
            {
                return;
            }

            if (this._controller.Count == 0)
            {
                this._container.style.display = DisplayStyle.None;
                return;
            }

            this._container.style.display = DisplayStyle.Flex;
            var rightInset = this._cameraBehaviour != null ? this._cameraBehaviour.RightInsetPixels : 0f;
            ViewportScope.Apply(this._container, this._root, this._viewCameraManager, rightInset);

            var fps = (int)this._controller.FrameRate.Fps;
            this._label.text = FormatTimecode(this._controller.Playhead.CurrentTime, fps);
        }
    }
}
