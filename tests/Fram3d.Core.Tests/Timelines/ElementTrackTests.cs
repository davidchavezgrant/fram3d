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

        // ── Scale keyframes ────────────────────────────────────────────────

        [Fact]
        public void ScaleKeyframes__IsEmpty__When__Created()
        {
            var track = new ElementTrack(new ElementId(Guid.NewGuid()));
            track.ScaleKeyframes.Count.Should().Be(0);
        }

        [Fact]
        public void EvaluateScale__ReturnsDefault__When__NoKeyframes()
        {
            var track = new ElementTrack(new ElementId(Guid.NewGuid()));
            track.EvaluateScale(TimePosition.ZERO).Should().Be(0f);
        }

        [Fact]
        public void EvaluateScale__Interpolates__When__BetweenKeyframes()
        {
            var track = new ElementTrack(new ElementId(Guid.NewGuid()));
            track.ScaleKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 1f));
            track.ScaleKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(2.0), 3f));
            track.EvaluateScale(new TimePosition(1.0)).Should().BeApproximately(2f, 0.01f);
        }

        [Fact]
        public void KeyframeCount__IncludesScale__When__ScaleKeyframesExist()
        {
            var track = new ElementTrack(new ElementId(Guid.NewGuid()));
            track.ScaleKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 1f));
            track.KeyframeCount.Should().Be(1);
        }

        // ── GetAllKeyframeTimes ────────────────────────────────────────────

        [Fact]
        public void GetAllKeyframeTimes__ReturnsEmpty__When__NoKeyframes()
        {
            var track = new ElementTrack(new ElementId(Guid.NewGuid()));
            track.GetAllKeyframeTimes().Should().BeEmpty();
        }

        [Fact]
        public void GetAllKeyframeTimes__MergesTimes__When__MultipleManagers()
        {
            var track = new ElementTrack(new ElementId(Guid.NewGuid()));
            track.PositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, Vector3.Zero));
            track.ScaleKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(1.0), 2f));
            var times = track.GetAllKeyframeTimes();
            times.Should().HaveCount(2);
            times[0].Seconds.Should().Be(0.0);
            times[1].Seconds.Should().Be(1.0);
        }

        // ── ClearAllKeyframes ─────────────────────────────────────────────

        [Fact]
        public void ClearAllKeyframes__ClearsAll__When__Called()
        {
            var track = Create();
            track.PositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, Vector3.One));
            track.RotationKeyframes.Add(
                new Keyframe<Quaternion>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, Quaternion.Identity));
            track.ScaleKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 1f));

            track.ClearAllKeyframes();

            track.PositionKeyframes.Count.Should().Be(0);
            track.RotationKeyframes.Count.Should().Be(0);
            track.ScaleKeyframes.Count.Should().Be(0);
        }

        // --- DeleteAllKeyframesAtTime ---

        [Fact]
        public void DeleteAllKeyframesAtTime__RemovesAllPropertiesAtTime__When__Called()
        {
            var track = Create();
            var t0    = TimePosition.ZERO;
            track.PositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), t0, Vector3.One));
            track.RotationKeyframes.Add(
                new Keyframe<Quaternion>(new KeyframeId(Guid.NewGuid()), t0, Quaternion.Identity));
            track.ScaleKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), t0, 1f));

            track.DeleteAllKeyframesAtTime(t0);

            track.PositionKeyframes.Count.Should().Be(0);
            track.RotationKeyframes.Count.Should().Be(0);
            track.ScaleKeyframes.Count.Should().Be(0);
        }

        [Fact]
        public void DeleteAllKeyframesAtTime__LeavesOtherTimes__When__MultipleTimesExist()
        {
            var track = Create();
            track.PositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, Vector3.Zero));
            track.PositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), new TimePosition(1.0), Vector3.One));

            track.DeleteAllKeyframesAtTime(TimePosition.ZERO);

            track.PositionKeyframes.Count.Should().Be(1);
            track.PositionKeyframes.Keyframes[0].Time.Seconds.Should().Be(1.0);
        }

        // --- MoveAllKeyframesAtTime ---

        [Fact]
        public void MoveAllKeyframesAtTime__MovesAllProperties__When__Called()
        {
            var track = Create();
            var t0    = TimePosition.ZERO;
            var t1    = new TimePosition(2.0);
            track.PositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), t0, Vector3.One));
            track.RotationKeyframes.Add(
                new Keyframe<Quaternion>(new KeyframeId(Guid.NewGuid()), t0, Quaternion.Identity));

            track.MoveAllKeyframesAtTime(t0, t1);

            track.PositionKeyframes.Keyframes[0].Time.Should().Be(t1);
            track.RotationKeyframes.Keyframes[0].Time.Should().Be(t1);
        }

        [Fact]
        public void MoveAllKeyframesAtTime__MergesSilently__When__TargetTimeExists()
        {
            var track = Create();
            var t0    = TimePosition.ZERO;
            var t1    = new TimePosition(1.0);
            track.PositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), t0, Vector3.Zero));
            track.PositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), t1, Vector3.One));

            track.MoveAllKeyframesAtTime(t0, t1);

            track.PositionKeyframes.Count.Should().Be(1);
            track.PositionKeyframes.Keyframes[0].Value.Should().Be(Vector3.Zero); // arriving value wins
        }
    }
}
