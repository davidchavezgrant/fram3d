using System;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
using Fram3d.Core.Timelines;
using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Views
{
    /// <summary>
    /// Top-left overlay on the Camera View showing shot number, camera letter,
    /// name, and duration. Example: "Shot 1A: OTS DET→WIT (3.5s)".
    /// Updates on shot change and during playback.
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

            this._label             = new Label();
            this._label.pickingMode = PickingMode.Ignore;
            this._label.AddToClassList("shot-label-overlay");
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

            var index = this._controller.IndexOf(shot.Id) + 1;
            this._label.text          = $"Shot {index}A: {shot.Name} ({shot.Duration:F1}s)";
            this._label.style.display = DisplayStyle.Flex;
        }
    }
}
