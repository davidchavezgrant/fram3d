using System;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Views
{
    /// <summary>
    /// Top-left overlay on the Camera View showing shot number, camera letter,
    /// name, and duration. Example: "Shot 2A: Shot_02 (5.0s)".
    /// </summary>
    public sealed class ShotLabelOverlay: MonoBehaviour
    {
        private Fram3d.Core.Timelines.Timeline _controller;
        private Label                          _label;
        private IDisposable                    _shotChangedSub;

        private void OnDestroy()
        {
            this._shotChangedSub?.Dispose();
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

            this._label                               = new Label();
            this._label.pickingMode                   = PickingMode.Ignore;
            this._label.style.position                = Position.Absolute;
            this._label.style.left                    = 8;
            this._label.style.top                     = 24;
            this._label.style.color                   = Color.white;
            this._label.style.fontSize                = 12;
            this._label.style.unityFontStyleAndWeight = FontStyle.Bold;
            this._label.style.textShadow              = new TextShadow
            {
                offset     = new Vector2(1, 1),
                blurRadius = 2,
                color      = new Color(0, 0, 0, 0.8f)
            };
            uiDoc.rootVisualElement.Add(this._label);
            this._shotChangedSub = this._controller.CurrentShotChanged.Subscribe(_ => this.UpdateLabel());
            this.UpdateLabel();
        }

        private void Update()
        {
            this.UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (this._label == null)
            {
                return;
            }

            var shot = this._controller?.CurrentShot;

            if (shot == null)
            {
                this._label.style.display = DisplayStyle.None;
                return;
            }

            var index       = this._controller.IndexOf(shot.Id) + 1;
            var totalFrames = (int)(shot.Duration * 24);
            var s           = totalFrames / 24;
            var f           = totalFrames % 24;
            this._label.text = $"Shot {index}A: {shot.Name} ({s};{f:D2})";
            this._label.style.display = DisplayStyle.Flex;
        }
    }
}
