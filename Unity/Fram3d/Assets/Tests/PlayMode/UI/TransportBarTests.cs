using System.Collections;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Fram3d.UI.Timeline;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Tests for TransportBar — play/stop button, timecode display, shot name.
    /// Verifies structure, play/stop icon toggling, and timecode formatting
    /// via UpdateTransport.
    /// </summary>
    public sealed class TransportBarTests
    {
        private Timeline   _controller;
        private UIDocument _uiDocument;
        private GameObject _uiGo;

        [SetUp]
        public void SetUp()
        {
            this._controller = new Timeline(FrameRate.FPS_24);

            this._uiGo       = new GameObject("TestUI");
            this._uiDocument = this._uiGo.AddComponent<UIDocument>();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:PanelSettings");

            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                this._uiDocument.panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            }
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(this._uiGo);
        }

        [Test]
        public void Constructor__CreatesPlayButton__When__Created()
        {
            var bar = new TransportBar(() => { });

            Assert.IsInstanceOf<Button>(bar[0], "First child should be the play button");
            Assert.AreEqual("\u25b6", ((Button)bar[0]).text, "Play button should show play symbol");
        }

        [Test]
        public void Constructor__CreatesTimecodeLabels__When__Created()
        {
            var bar = new TransportBar(() => { });

            // Structure: play button, time label, divider, duration label, shot label
            Assert.GreaterOrEqual(bar.childCount, 5, "Transport should have 5 children");
            Assert.IsInstanceOf<Label>(bar[1], "Second child should be time label");
            Assert.IsInstanceOf<Label>(bar[2], "Third child should be divider");
            Assert.IsInstanceOf<Label>(bar[3], "Fourth child should be duration label");
            Assert.IsInstanceOf<Label>(bar[4], "Fifth child should be shot label");
        }

        [Test]
        public void Constructor__ShowsDefaultTimecode__When__Created()
        {
            var bar  = new TransportBar(() => { });
            var time = (Label)bar[1];

            Assert.AreEqual("00;00;00;00", time.text, "Default timecode should be zero");
        }

        [Test]
        public void UpdatePlayButton__ShowsStopSymbol__When__Playing()
        {
            var bar = new TransportBar(() => { });

            bar.UpdatePlayButton(true);

            var button = (Button)bar[0];
            Assert.AreEqual("\u25a0", button.text, "Should show stop symbol when playing");
            Assert.IsTrue(button.ClassListContains("timeline-transport__play--active"),
                "Should have active class when playing");
        }

        [Test]
        public void UpdatePlayButton__ShowsPlaySymbol__When__Stopped()
        {
            var bar = new TransportBar(() => { });

            bar.UpdatePlayButton(true);
            bar.UpdatePlayButton(false);

            var button = (Button)bar[0];
            Assert.AreEqual("\u25b6", button.text, "Should show play symbol when stopped");
            Assert.IsFalse(button.ClassListContains("timeline-transport__play--active"),
                "Should not have active class when stopped");
        }

        [Test]
        public void UpdateTransport__ShowsShotName__When__ShotExists()
        {
            this._controller.AddShot(System.Numerics.Vector3.Zero, System.Numerics.Quaternion.Identity);
            this._controller.InitializeViewRange(800);

            var bar = new TransportBar(() => { });

            bar.UpdateTransport(this._controller.Playhead, this._controller);

            var shotLabel = (Label)bar[4];
            Assert.IsNotEmpty(shotLabel.text, "Shot label should show current shot name");
        }

        [Test]
        public void UpdateTransport__ShowsZeroTimecode__When__NoShots()
        {
            var bar = new TransportBar(() => { });

            bar.UpdateTransport(this._controller.Playhead, this._controller);

            var timeLabel = (Label)bar[1];
            Assert.AreEqual("00;00;00;00", timeLabel.text);

            var shotLabel = (Label)bar[4];
            Assert.AreEqual("", shotLabel.text, "Shot label should be empty when no shots");
        }

        [Test]
        public void UpdateTransport__ShowsNonZeroTimecode__When__PlayheadAdvanced()
        {
            this._controller.AddShot(System.Numerics.Vector3.Zero, System.Numerics.Quaternion.Identity);
            this._controller.InitializeViewRange(800);

            // Move playhead forward
            this._controller.Playhead.Scrub(1.5, this._controller.TotalDuration);

            var bar = new TransportBar(() => { });
            bar.UpdateTransport(this._controller.Playhead, this._controller);

            var timeLabel = (Label)bar[1];
            Assert.AreNotEqual("00;00;00;00", timeLabel.text,
                "Timecode should reflect playhead position");
        }

        [Test]
        public void Constructor__CreatesPlayButtonWithCorrectText__When__Created()
        {
            var bar    = new TransportBar(() => { });
            var button = (Button)bar[0];

            Assert.AreEqual("\u25b6", button.text, "Play button should show play symbol initially");
            Assert.IsTrue(button.ClassListContains("timeline-transport__play"),
                "Play button should have the transport play class");
        }
    }
}
