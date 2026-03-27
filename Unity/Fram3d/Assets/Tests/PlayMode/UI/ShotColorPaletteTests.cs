using Fram3d.UI.Timeline;
using NUnit.Framework;
using UnityEngine;
namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Tests for ShotColorPalette — cycling color assignment for shot blocks.
    /// </summary>
    public sealed class ShotColorPaletteTests
    {
        [Test]
        public void GetColor__ReturnsDifferentColors__When__IndicesAreDistinct()
        {
            var colors = new Color[8];

            for (var i = 0; i < 8; i++)
            {
                colors[i] = ShotColorPalette.GetColor(i);
            }

            // Verify no two adjacent colors are the same
            for (var i = 0; i < 7; i++)
            {
                Assert.AreNotEqual(colors[i], colors[i + 1],
                    $"Color {i} and {i + 1} should be distinct");
            }
        }

        [Test]
        public void GetColor__WrapsAround__When__IndexExceedsPaletteSize()
        {
            var color0 = ShotColorPalette.GetColor(0);
            var color8 = ShotColorPalette.GetColor(8);
            Assert.AreEqual(color0, color8, "Index 8 should wrap to same color as index 0");

            var color3  = ShotColorPalette.GetColor(3);
            var color11 = ShotColorPalette.GetColor(11);
            Assert.AreEqual(color3, color11, "Index 11 should wrap to same color as index 3");
        }

        [Test]
        public void GetColor__ReturnsOpaqueColors__When__Called()
        {
            for (var i = 0; i < 8; i++)
            {
                var color = ShotColorPalette.GetColor(i);
                Assert.AreEqual(1f, color.a, $"Color {i} should be fully opaque");
            }
        }
    }
}
