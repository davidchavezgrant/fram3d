using System.Collections;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
using Fram3d.Core.Timelines;
using Fram3d.UI.Timeline;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Tests for ShotBlock — the VisualElement representing a single shot
    /// in the timeline shot track. Verifies structure, label content,
    /// active state toggling, and duration edit lifecycle.
    /// </summary>
    public sealed class ShotBlockTests
    {
        private Timeline    _controller;
        private UIDocument  _uiDocument;
        private GameObject  _uiGo;

        [SetUp]
        public void SetUp()
        {
            this._controller = new Timeline(FrameRate.FPS_24);
            this._controller.AddShot();

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
        public void Constructor__CreatesNameAndDurationLabels__When__Created()
        {
            var shot  = this._controller.Shots[0];
            var block = new ShotBlock(shot);

            Assert.AreEqual(2, block.childCount, "ShotBlock should have 2 children (name + duration)");
            Assert.IsInstanceOf<Label>(block[0], "First child should be name label");
            Assert.IsInstanceOf<Label>(block[1], "Second child should be duration label");
        }

        [Test]
        public void Constructor__SetsBackgroundColor__When__Created()
        {
            var shot   = this._controller.Shots[0];
            var block  = new ShotBlock(shot);
            var expected = ShotColorPalette.GetColor(shot.ColorIndex);

            Assert.AreEqual(expected, block.style.backgroundColor.value,
                "Background should match palette color for shot's color index");
        }

        [Test]
        public void Constructor__DisplaysShotName__When__Created()
        {
            var shot  = this._controller.Shots[0];
            var block = new ShotBlock(shot);
            var name  = (Label)block[0];

            Assert.AreEqual(shot.Name, name.text);
        }

        [Test]
        public void Constructor__DisplaysFormattedDuration__When__Created()
        {
            var shot     = this._controller.Shots[0];
            var block    = new ShotBlock(shot);
            var duration = (Label)block[1];
            var totalFrames = (int)(shot.Duration * 24);
            var s           = totalFrames / 24;
            var f           = totalFrames % 24;
            var expected    = $"{s};{f:D2}";

            Assert.AreEqual(expected, duration.text);
        }

        [Test]
        public void SetActive__AddsActiveClass__When__True()
        {
            var shot  = this._controller.Shots[0];
            var block = new ShotBlock(shot);

            block.SetActive(true);

            Assert.IsTrue(block.ClassListContains("shot-block--active"));
        }

        [Test]
        public void SetActive__RemovesActiveClass__When__False()
        {
            var shot  = this._controller.Shots[0];
            var block = new ShotBlock(shot);

            block.SetActive(true);
            block.SetActive(false);

            Assert.IsFalse(block.ClassListContains("shot-block--active"));
        }

        [Test]
        public void SetActive__IsIdempotent__When__CalledMultipleTimes()
        {
            var shot  = this._controller.Shots[0];
            var block = new ShotBlock(shot);

            block.SetActive(true);
            block.SetActive(true);
            block.SetActive(true);

            Assert.IsTrue(block.ClassListContains("shot-block--active"),
                "Multiple SetActive(true) calls should not break");
        }

        [Test]
        public void Refresh__UpdatesLabels__When__ShotMutated()
        {
            var shot  = this._controller.Shots[0];
            var block = new ShotBlock(shot);
            var name  = (Label)block[0];

            // Mutate shot via Timeline
            this._controller.ResizeShotAtEdge(0, 10.0);
            block.Refresh();

            var duration = (Label)block[1];
            var totalFrames = (int)(shot.Duration * 24);
            var s           = totalFrames / 24;
            var f           = totalFrames % 24;
            Assert.AreEqual($"{s};{f:D2}", duration.text,
                "Duration label should update after Refresh");
        }

        [Test]
        public void Shot__ReturnsConstructorShot__When__Accessed()
        {
            var shot  = this._controller.Shots[0];
            var block = new ShotBlock(shot);

            Assert.AreSame(shot, block.Shot);
        }

        [UnityTest]
        public IEnumerator DurationClicked__FiresEvent__When__DurationLabelClicked()
        {
            yield return null;

            var shot  = this._controller.Shots[0];
            var block = new ShotBlock(shot);
            this._uiDocument.rootVisualElement.Add(block);

            var fired = false;
            block.DurationClicked += _ => fired = true;

            // Simulate click on duration label
            var durationLabel = block[1];
            using (var evt = ClickEvent.GetPooled())
            {
                evt.target = durationLabel;
                durationLabel.SendEvent(evt);
            }

            Assert.IsTrue(fired, "DurationClicked should fire when duration label is clicked");
        }

        [Test]
        public void BeginDurationEdit__SetsIsEditing__When__Called()
        {
            var shot  = this._controller.Shots[0];
            var block = new ShotBlock(shot);

            Assert.IsFalse(block.IsEditing, "Precondition: not editing");

            block.BeginDurationEdit(_ => { });

            Assert.IsTrue(block.IsEditing, "Should be editing after BeginDurationEdit");
        }

        [Test]
        public void BeginDurationEdit__IsReentrantSafe__When__CalledTwice()
        {
            var shot  = this._controller.Shots[0];
            var block = new ShotBlock(shot);

            block.BeginDurationEdit(_ => { });
            var childCountAfterFirst = block.childCount;

            block.BeginDurationEdit(_ => { });

            Assert.AreEqual(childCountAfterFirst, block.childCount,
                "Calling BeginDurationEdit twice should not add a second text field");
        }
    }
}
