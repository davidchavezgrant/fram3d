using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Scenes;
using Xunit;
namespace Fram3d.Core.Tests.Scene
{
    public sealed class ClickDetectorTests
    {
        private static readonly Vector2 CENTER = new(960, 540);

        [Fact]
        public void Update__ReturnsClick__When__PressAndReleaseWithinThreshold()
        {
            var detector = new ClickDetector();

            // Frame 1: press
            detector.Update(pressed: true, held: false, released: false, CENTER, false);

            // Frame 2: held (no movement)
            detector.Update(pressed: false, held: true, released: false, CENTER, false);

            // Frame 3: release
            var result = detector.Update(pressed: false, held: false, released: true, CENTER, false);

            result.Kind.Should().Be(ClickResultKind.CLICK);
            result.Position.Should().Be(CENTER);
        }

        [Fact]
        public void Update__ReturnsDrag__When__MouseExceedsThreshold()
        {
            var detector = new ClickDetector();
            var farAway  = CENTER + new Vector2(20, 20);

            // Frame 1: press
            detector.Update(pressed: true, held: false, released: false, CENTER, false);

            // Frame 2: held with large movement
            detector.Update(pressed: false, held: true, released: false, farAway, false);

            // Frame 3: release
            var result = detector.Update(pressed: false, held: false, released: true, farAway, false);

            result.Kind.Should().Be(ClickResultKind.DRAG);
        }

        [Fact]
        public void Update__ReturnsClick__When__SmallMovementUnderThreshold()
        {
            var detector  = new ClickDetector();
            var nearCenter = CENTER + new Vector2(2, 2);

            detector.Update(pressed: true, held: false, released: false, CENTER, false);
            detector.Update(pressed: false, held: true, released: false, nearCenter, false);
            var result = detector.Update(pressed: false, held: false, released: true, nearCenter, false);

            result.Kind.Should().Be(ClickResultKind.CLICK);
        }

        [Fact]
        public void Update__ReturnsNone__When__CameraModifierHeld()
        {
            var detector = new ClickDetector();

            var result = detector.Update(pressed: true, held: false, released: false, CENTER, cameraModifierHeld: true);

            result.Kind.Should().Be(ClickResultKind.NONE);

            // Release should also be none since press was suppressed
            var releaseResult = detector.Update(pressed: false, held: false, released: true, CENTER, false);
            releaseResult.Kind.Should().Be(ClickResultKind.NONE);
        }

        [Fact]
        public void Update__ReturnsNone__When__PressFrame()
        {
            var detector = new ClickDetector();

            var result = detector.Update(pressed: true, held: false, released: false, CENTER, false);

            result.Kind.Should().Be(ClickResultKind.NONE);
        }

        [Fact]
        public void Update__ReturnsNone__When__HeldFrame()
        {
            var detector = new ClickDetector();

            detector.Update(pressed: true, held: false, released: false, CENTER, false);
            var result = detector.Update(pressed: false, held: true, released: false, CENTER, false);

            result.Kind.Should().Be(ClickResultKind.NONE);
        }

        [Fact]
        public void Suppress__PreventsClick__When__CalledAfterPress()
        {
            var detector = new ClickDetector();

            detector.Update(pressed: true, held: false, released: false, CENTER, false);
            detector.Suppress();
            var result = detector.Update(pressed: false, held: false, released: true, CENTER, false);

            result.Kind.Should().Be(ClickResultKind.NONE);
        }

        [Fact]
        public void Update__ResetsState__When__NewPressAfterRelease()
        {
            var detector = new ClickDetector();

            // First click
            detector.Update(pressed: true, held: false, released: false, CENTER, false);
            detector.Update(pressed: false, held: true, released: false, CENTER, false);
            detector.Update(pressed: false, held: false, released: true, CENTER, false);

            // Second click — should work independently
            detector.Update(pressed: true, held: false, released: false, CENTER, false);
            detector.Update(pressed: false, held: true, released: false, CENTER, false);
            var result = detector.Update(pressed: false, held: false, released: true, CENTER, false);

            result.Kind.Should().Be(ClickResultKind.CLICK);
        }

        [Fact]
        public void Update__DetectsDrag__When__ThresholdExactlyMet()
        {
            var detector = new ClickDetector();
            // Threshold is 5px. At exactly 5px diagonal (3.54, 3.54 per axis) sqrMag = 25 = threshold^2
            // Must exceed, not equal — so (4, 4) with sqrMag=32 > 25 should drag
            var moved = CENTER + new Vector2(4, 4);

            detector.Update(pressed: true, held: false, released: false, CENTER, false);
            detector.Update(pressed: false, held: true, released: false, moved, false);
            var result = detector.Update(pressed: false, held: false, released: true, moved, false);

            result.Kind.Should().Be(ClickResultKind.DRAG);
        }

        [Fact]
        public void Update__ReturnsClick__When__PressAndReleaseInSameFrame()
        {
            var detector = new ClickDetector();

            // Frame hitch: press+release land in one frame
            var result = detector.Update(pressed: true, held: false, released: true, CENTER, false);

            result.Kind.Should().Be(ClickResultKind.CLICK);
            result.Position.Should().Be(CENTER);
        }

        [Fact]
        public void Update__ReturnsNone__When__SameFrameClickWithCameraModifier()
        {
            var detector = new ClickDetector();

            var result = detector.Update(pressed: true, held: false, released: true, CENTER, cameraModifierHeld: true);

            result.Kind.Should().Be(ClickResultKind.NONE);
        }
    }
}
