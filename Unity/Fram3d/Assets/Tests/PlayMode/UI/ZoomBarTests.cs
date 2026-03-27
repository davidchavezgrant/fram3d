using Fram3d.UI.Timeline;
using NUnit.Framework;
using UnityEngine.UIElements;
namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Tests for ZoomBar — the minimap-style bar showing the visible
    /// time range with a draggable thumb.
    /// </summary>
    public sealed class ZoomBarTests
    {
        [Test]
        public void Constructor__CreatesLabelColumnAndBar__When__Created()
        {
            var bar = new ZoomBar();

            Assert.GreaterOrEqual(bar.childCount, 2,
                "ZoomBar should have label column and bar area");
        }

        [Test]
        public void Constructor__HasThumbInBar__When__Created()
        {
            var bar     = new ZoomBar();
            var barArea = bar[1]; // second child is the bar area
            var found   = false;

            for (var i = 0; i < barArea.childCount; i++)
            {
                if (barArea[i].ClassListContains("timeline-zoom-thumb"))
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, "Bar area should contain a thumb element");
        }

        [Test]
        public void Constructor__HasPlayheadInBar__When__Created()
        {
            var bar     = new ZoomBar();
            var barArea = bar[1];
            var found   = false;

            for (var i = 0; i < barArea.childCount; i++)
            {
                if (barArea[i].ClassListContains("timeline-zoom-playhead"))
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, "Bar area should contain a playhead marker");
        }

        [Test]
        public void Constructor__SetsCorrectHeight__When__Created()
        {
            var bar = new ZoomBar();

            Assert.AreEqual(18f, bar.style.height.value.value,
                "ZoomBar height should be 18px");
        }

        [Test]
        public void RegisterDragCallbacks__DoesNotThrow__When__Called()
        {
            var bar = new ZoomBar();

            Assert.DoesNotThrow(() => bar.RegisterDragCallbacks(),
                "RegisterDragCallbacks should be safe to call on a detached ZoomBar");
        }
    }
}
