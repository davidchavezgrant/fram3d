using System;
using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Xunit;
namespace Fram3d.Core.Tests.Timelines
{
    public sealed class ElementTrackTests
    {
        private static readonly ElementId ELEMENT_ID = new(Guid.NewGuid());

        private static ElementTrack Create() => new(ELEMENT_ID);

        // ── Construction ─────────────────────────────────────────────────

        [Fact]
        public void Constructor__SetsElementId__When__Created()
        {
            var track = Create();

            track.ElementId.Should().Be(ELEMENT_ID);
        }

        [Fact]
        public void Constructor__ThrowsArgumentNullException__When__NullElementId() =>
            FluentActions.Invoking(() => new ElementTrack(null))
                .Should().Throw<ArgumentNullException>();

        [Fact]
        public void HasKeyframes__ReturnsFalse__When__NoKeyframes()
        {
            var track = Create();

            track.HasKeyframes.Should().BeFalse();
        }

        [Fact]
        public void KeyframeCount__ReturnsZero__When__NoKeyframes()
        {
            var track = Create();

            track.KeyframeCount.Should().Be(0);
        }

        // ── Position keyframes ───────────────────────────────────────────

        [Fact]
        public void HasKeyframes__ReturnsTrue__When__PositionKeyframeAdded()
        {
            var track = Create();
            var kf    = new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, Vector3.One);

            track.PositionKeyframes.Add(kf);

            track.HasKeyframes.Should().BeTrue();
        }

        [Fact]
        public void KeyframeCount__ReturnsCombinedCount__When__BothTracksHaveKeyframes()
        {
            var track = Create();
            track.PositionKeyframes.Add(new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, Vector3.One));
            track.RotationKeyframes.Add(new Keyframe<Quaternion>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, Quaternion.Identity));

            track.KeyframeCount.Should().Be(2);
        }

        // ── Evaluate position ────────────────────────────────────────────

        [Fact]
        public void EvaluatePosition__ReturnsDefault__When__NoKeyframes()
        {
            var track = Create();

            track.EvaluatePosition(TimePosition.ZERO).Should().Be(default(Vector3));
        }

        [Fact]
        public void EvaluatePosition__ReturnsValue__When__SingleKeyframe()
        {
            var track    = Create();
            var position = new Vector3(1, 2, 3);
            track.PositionKeyframes.Add(new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, position));

            track.EvaluatePosition(new TimePosition(5.0)).Should().Be(position);
        }

        [Fact]
        public void EvaluatePosition__Interpolates__When__BetweenKeyframes()
        {
            var track = Create();
            var start = new Vector3(0, 0, 0);
            var end   = new Vector3(10, 0, 0);
            track.PositionKeyframes.Add(new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), new TimePosition(0), start));
            track.PositionKeyframes.Add(new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), new TimePosition(10), end));

            var result = track.EvaluatePosition(new TimePosition(5.0));

            result.X.Should().BeApproximately(5.0f, 0.01f);
            result.Y.Should().BeApproximately(0f, 0.01f);
        }

        [Fact]
        public void EvaluatePosition__ClampsToFirst__When__BeforeAllKeyframes()
        {
            var track    = Create();
            var position = new Vector3(5, 5, 5);
            track.PositionKeyframes.Add(new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), new TimePosition(2.0), position));

            track.EvaluatePosition(TimePosition.ZERO).Should().Be(position);
        }

        [Fact]
        public void EvaluatePosition__ClampsToLast__When__AfterAllKeyframes()
        {
            var track    = Create();
            var position = new Vector3(5, 5, 5);
            track.PositionKeyframes.Add(new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), new TimePosition(2.0), position));

            track.EvaluatePosition(new TimePosition(99.0)).Should().Be(position);
        }

        // ── Evaluate rotation ────────────────────────────────────────────

        [Fact]
        public void EvaluateRotation__ReturnsDefault__When__NoKeyframes()
        {
            var track = Create();

            track.EvaluateRotation(TimePosition.ZERO).Should().Be(default(Quaternion));
        }

        [Fact]
        public void EvaluateRotation__ReturnsValue__When__SingleKeyframe()
        {
            var track    = Create();
            var rotation = Quaternion.CreateFromYawPitchRoll(1, 0, 0);
            track.RotationKeyframes.Add(new Keyframe<Quaternion>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, rotation));

            track.EvaluateRotation(new TimePosition(5.0)).Should().Be(rotation);
        }
    }
}
