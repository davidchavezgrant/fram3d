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
    /// Tests for Ruler — the time ruler with tick marks, playhead,
    /// out-of-range darkening, and scrub handling.
    /// </summary>
    public sealed class RulerTests
    {
        private Timeline   _controller;
        private UIDocument _uiDocument;
        private GameObject _uiGo;

        [SetUp]
        public void SetUp()
        {
            this._controller = new Timeline(FrameRate.FPS_24);
            this._controller.AddShot();
            this._controller.InitializeViewRange(800);

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
        public void Constructor__CreatesLabelColumnAndContent__When__Created()
        {
            var ruler = new Ruler();

            Assert.GreaterOrEqual(ruler.childCount, 2,
                "Ruler should have at least label column and content area");
        }

        [Test]
        public void Constructor__HasPlayheadInContent__When__Created()
        {
            var ruler   = new Ruler();
            var content = ruler.Content;
            var found   = false;

            for (var i = 0; i < content.childCount; i++)
            {
                if (content[i].ClassListContains("timeline-playhead"))
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, "Content should contain a playhead element");
        }

        [Test]
        public void Constructor__HasOutOfRangeInContent__When__Created()
        {
            var ruler   = new Ruler();
            var content = ruler.Content;
            var found   = false;

            for (var i = 0; i < content.childCount; i++)
            {
                if (content[i].ClassListContains("timeline-out-of-range"))
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, "Content should contain an out-of-range element");
        }

        [Test]
        public void IsScrubbing__ReturnsFalse__When__Initialized()
        {
            var ruler = new Ruler();

            Assert.IsFalse(ruler.IsScrubbing, "Should not be scrubbing initially");
        }

        [Test]
        public void Content__ReturnsNonNull__When__Accessed()
        {
            var ruler = new Ruler();

            Assert.IsNotNull(ruler.Content);
        }

        [Test]
        public void UpdatePlayhead__PositionsPlayhead__When__Called()
        {
            var ruler = new Ruler();
            this._uiDocument.rootVisualElement.Add(ruler);

            ruler.UpdatePlayhead(this._controller, 1.0);

            // Find the playhead and check its position
            VisualElement playhead = null;

            for (var i = 0; i < ruler.Content.childCount; i++)
            {
                if (ruler.Content[i].ClassListContains("timeline-playhead"))
                {
                    playhead = ruler.Content[i];
                    break;
                }
            }

            Assert.IsNotNull(playhead);
            Assert.AreEqual(DisplayStyle.Flex, playhead.style.display.value,
                "Playhead should be visible after UpdatePlayhead");
        }

        [Test]
        public void UpdateTicks__CreatesTicks__When__Called()
        {
            var ruler = new Ruler();
            this._uiDocument.rootVisualElement.Add(ruler);

            var childCountBefore = ruler.Content.childCount;
            ruler.UpdateTicks(this._controller, this._controller.TotalDuration);

            Assert.Greater(ruler.Content.childCount, childCountBefore,
                "UpdateTicks should add tick elements");
        }

        [Test]
        public void UpdateTicks__PreservesPlayhead__When__Called()
        {
            var ruler = new Ruler();
            this._uiDocument.rootVisualElement.Add(ruler);

            ruler.UpdateTicks(this._controller, this._controller.TotalDuration);

            // Playhead should still exist after ticks are rebuilt
            var found = false;

            for (var i = 0; i < ruler.Content.childCount; i++)
            {
                if (ruler.Content[i].ClassListContains("timeline-playhead"))
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, "Playhead should survive tick rebuild");
        }

        [Test]
        public void RegisterScrubCallbacks__DoesNotThrow__When__Called()
        {
            var ruler = new Ruler();

            Assert.DoesNotThrow(() => ruler.RegisterScrubCallbacks(),
                "RegisterScrubCallbacks should be safe to call on a detached ruler");
        }
    }
}
