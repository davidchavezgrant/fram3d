using System;
using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Xunit;

namespace Fram3d.Core.Tests.Timelines
{
    public class KeyframeManagerTests
    {
        private static Keyframe<float> MakeKeyframe(double time, float value)
        {
            return new Keyframe<float>(
                new KeyframeId(Guid.NewGuid()),
                new TimePosition(time),
                value
            );
        }

        private static float FloatLerp(float a, float b, float t) => a + (b - a) * t;

        // --- Add ---

        [Fact]
        public void Add__IncreasesCount__When__KeyframeAdded()
        {
            var mgr = new KeyframeManager<float>();
            mgr.Add(MakeKeyframe(0.0, 1.0f));
            mgr.Count.Should().Be(1);
        }

        [Fact]
        public void Add__ThrowsArgumentNull__When__KeyframeIsNull()
        {
            var mgr = new KeyframeManager<float>();
            Action act = () => mgr.Add(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Add__MaintainsSortOrder__When__AddedOutOfOrder()
        {
            var mgr = new KeyframeManager<float>();
            mgr.Add(MakeKeyframe(3.0, 30f));
            mgr.Add(MakeKeyframe(1.0, 10f));
            mgr.Add(MakeKeyframe(2.0, 20f));
            mgr.Keyframes[0].Time.Seconds.Should().Be(1.0);
            mgr.Keyframes[1].Time.Seconds.Should().Be(2.0);
            mgr.Keyframes[2].Time.Seconds.Should().Be(3.0);
        }

        [Fact]
        public void Add__ThrowsInvalidOperation__When__SameIdExists()
        {
            var mgr = new KeyframeManager<float>();
            var id = new KeyframeId(Guid.NewGuid());
            mgr.Add(new Keyframe<float>(id, new TimePosition(1.0), 5.0f));
            Action act = () => mgr.Add(new Keyframe<float>(id, new TimePosition(2.0), 10.0f));
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Add__ThrowsInvalidOperation__When__SameTimeExists()
        {
            var mgr = new KeyframeManager<float>();
            mgr.Add(MakeKeyframe(1.0, 5.0f));
            Action act = () => mgr.Add(MakeKeyframe(1.0, 10.0f));
            act.Should().Throw<InvalidOperationException>();
        }

        // --- Update ---

        [Fact]
        public void Update__ReplacesValue__When__SameIdExists()
        {
            var mgr = new KeyframeManager<float>();
            var id = new KeyframeId(Guid.NewGuid());
            mgr.Add(new Keyframe<float>(id, new TimePosition(1.0), 5.0f));
            mgr.Update(new Keyframe<float>(id, new TimePosition(1.0), 10.0f));
            mgr.Count.Should().Be(1);
            mgr.Keyframes[0].Value.Should().Be(10.0f);
        }

        [Fact]
        public void Update__MovesTime__When__SameIdDifferentTime()
        {
            var mgr = new KeyframeManager<float>();
            var id = new KeyframeId(Guid.NewGuid());
            mgr.Add(new Keyframe<float>(id, new TimePosition(1.0), 5.0f));
            mgr.Update(new Keyframe<float>(id, new TimePosition(3.0), 5.0f));
            mgr.Count.Should().Be(1);
            mgr.Keyframes[0].Time.Seconds.Should().Be(3.0);
        }

        [Fact]
        public void Update__ThrowsInvalidOperation__When__IdNotFound()
        {
            var mgr = new KeyframeManager<float>();
            var kf = MakeKeyframe(1.0, 5.0f);
            Action act = () => mgr.Update(kf);
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Update__ThrowsArgumentNull__When__KeyframeIsNull()
        {
            var mgr = new KeyframeManager<float>();
            Action act = () => mgr.Update(null);
            act.Should().Throw<ArgumentNullException>();
        }

        // --- SetOrMerge ---

        [Fact]
        public void SetOrMerge__AddsNew__When__NoConflict()
        {
            var mgr = new KeyframeManager<float>();
            mgr.SetOrMerge(MakeKeyframe(1.0, 5.0f));
            mgr.Count.Should().Be(1);
        }

        [Fact]
        public void SetOrMerge__ReplacesExisting__When__SameIdAdded()
        {
            var mgr = new KeyframeManager<float>();
            var id = new KeyframeId(Guid.NewGuid());
            mgr.SetOrMerge(new Keyframe<float>(id, new TimePosition(1.0), 5.0f));
            mgr.SetOrMerge(new Keyframe<float>(id, new TimePosition(1.0), 10.0f));
            mgr.Count.Should().Be(1);
            mgr.Keyframes[0].Value.Should().Be(10.0f);
        }

        [Fact]
        public void SetOrMerge__MergesAtSameTime__When__DifferentIdAtSameTime()
        {
            var mgr = new KeyframeManager<float>();
            mgr.SetOrMerge(MakeKeyframe(1.0, 5.0f));
            mgr.SetOrMerge(MakeKeyframe(1.0, 10.0f));
            mgr.Count.Should().Be(1);
            mgr.Keyframes[0].Value.Should().Be(10.0f);
        }

        [Fact]
        public void SetOrMerge__UpdatesPosition__When__SameIdDifferentTime()
        {
            var mgr = new KeyframeManager<float>();
            var id = new KeyframeId(Guid.NewGuid());
            mgr.SetOrMerge(new Keyframe<float>(id, new TimePosition(1.0), 5.0f));
            mgr.SetOrMerge(new Keyframe<float>(id, new TimePosition(3.0), 5.0f));
            mgr.Count.Should().Be(1);
            mgr.Keyframes[0].Time.Seconds.Should().Be(3.0);
        }

        [Fact]
        public void SetOrMerge__ThrowsArgumentNull__When__KeyframeIsNull()
        {
            var mgr = new KeyframeManager<float>();
            Action act = () => mgr.SetOrMerge(null);
            act.Should().Throw<ArgumentNullException>();
        }

        // --- RemoveById ---

        [Fact]
        public void RemoveById__ReturnsTrue__When__KeyframeExists()
        {
            var mgr = new KeyframeManager<float>();
            var kf = MakeKeyframe(1.0, 5.0f);
            mgr.Add(kf);
            mgr.RemoveById(kf.Id).Should().BeTrue();
            mgr.Count.Should().Be(0);
        }

        [Fact]
        public void RemoveById__ReturnsFalse__When__KeyframeDoesNotExist()
        {
            var mgr = new KeyframeManager<float>();
            mgr.RemoveById(new KeyframeId(Guid.NewGuid())).Should().BeFalse();
        }

        // --- GetById ---

        [Fact]
        public void GetById__ReturnsKeyframe__When__Exists()
        {
            var mgr = new KeyframeManager<float>();
            var kf = MakeKeyframe(1.0, 5.0f);
            mgr.Add(kf);
            mgr.GetById(kf.Id).Should().NotBeNull();
            mgr.GetById(kf.Id).Value.Should().Be(5.0f);
        }

        [Fact]
        public void GetById__ReturnsNull__When__NotFound()
        {
            var mgr = new KeyframeManager<float>();
            mgr.GetById(new KeyframeId(Guid.NewGuid())).Should().BeNull();
        }

        // --- GetInRange ---

        [Fact]
        public void GetInRange__ReturnsMatchingKeyframes__When__InRange()
        {
            var mgr = new KeyframeManager<float>();
            mgr.Add(MakeKeyframe(0.0, 0f));
            mgr.Add(MakeKeyframe(1.0, 10f));
            mgr.Add(MakeKeyframe(2.0, 20f));
            mgr.Add(MakeKeyframe(3.0, 30f));
            mgr.Add(MakeKeyframe(4.0, 40f));
            var result = mgr.GetInRange(new TimePosition(1.0), new TimePosition(3.0));
            result.Should().HaveCount(3);
            result[0].Value.Should().Be(10f);
            result[1].Value.Should().Be(20f);
            result[2].Value.Should().Be(30f);
        }

        [Fact]
        public void GetInRange__ReturnsEmpty__When__NoKeyframesInRange()
        {
            var mgr = new KeyframeManager<float>();
            mgr.Add(MakeKeyframe(0.0, 0f));
            mgr.Add(MakeKeyframe(5.0, 50f));
            var result = mgr.GetInRange(new TimePosition(2.0), new TimePosition(3.0));
            result.Should().BeEmpty();
        }

        // --- Clear ---

        [Fact]
        public void Clear__RemovesAllKeyframes__When__Called()
        {
            var mgr = new KeyframeManager<float>();
            mgr.Add(MakeKeyframe(0.0, 0f));
            mgr.Add(MakeKeyframe(1.0, 10f));
            mgr.Clear();
            mgr.Count.Should().Be(0);
            mgr.Keyframes.Should().BeEmpty();
        }

        // --- Evaluate ---

        [Fact]
        public void Evaluate__ReturnsDefault__When__NoKeyframes()
        {
            var mgr = new KeyframeManager<float>();
            mgr.Evaluate(new TimePosition(1.0), FloatLerp).Should().Be(0f);
        }

        [Fact]
        public void Evaluate__ReturnsSingleValue__When__OneKeyframe()
        {
            var mgr = new KeyframeManager<float>();
            mgr.Add(MakeKeyframe(1.0, 42.0f));
            mgr.Evaluate(new TimePosition(0.0), FloatLerp).Should().Be(42.0f);
            mgr.Evaluate(new TimePosition(1.0), FloatLerp).Should().Be(42.0f);
            mgr.Evaluate(new TimePosition(5.0), FloatLerp).Should().Be(42.0f);
        }

        [Fact]
        public void Evaluate__ClampsToFirst__When__BeforeFirstKeyframe()
        {
            var mgr = new KeyframeManager<float>();
            mgr.Add(MakeKeyframe(2.0, 10.0f));
            mgr.Add(MakeKeyframe(4.0, 20.0f));
            mgr.Evaluate(new TimePosition(0.0), FloatLerp).Should().Be(10.0f);
        }

        [Fact]
        public void Evaluate__ClampsToLast__When__AfterLastKeyframe()
        {
            var mgr = new KeyframeManager<float>();
            mgr.Add(MakeKeyframe(2.0, 10.0f));
            mgr.Add(MakeKeyframe(4.0, 20.0f));
            mgr.Evaluate(new TimePosition(10.0), FloatLerp).Should().Be(20.0f);
        }

        [Fact]
        public void Evaluate__InterpolatesLinearly__When__BetweenKeyframes()
        {
            var mgr = new KeyframeManager<float>();
            mgr.Add(MakeKeyframe(0.0, 0.0f));
            mgr.Add(MakeKeyframe(2.0, 10.0f));
            mgr.Evaluate(new TimePosition(1.0), FloatLerp).Should().Be(5.0f);
        }

        [Fact]
        public void Evaluate__InterpolatesCorrectly__When__AtExactKeyframeTime()
        {
            var mgr = new KeyframeManager<float>();
            mgr.Add(MakeKeyframe(0.0, 0.0f));
            mgr.Add(MakeKeyframe(2.0, 10.0f));
            mgr.Add(MakeKeyframe(4.0, 20.0f));
            mgr.Evaluate(new TimePosition(2.0), FloatLerp).Should().Be(10.0f);
        }

        [Fact]
        public void Evaluate__InterpolatesBetweenMiddleKeyframes__When__MultipleSegments()
        {
            var mgr = new KeyframeManager<float>();
            mgr.Add(MakeKeyframe(0.0, 0.0f));
            mgr.Add(MakeKeyframe(2.0, 10.0f));
            mgr.Add(MakeKeyframe(4.0, 30.0f));
            mgr.Evaluate(new TimePosition(3.0), FloatLerp).Should().Be(20.0f);
        }

        // --- Vector3 evaluation ---

        [Fact]
        public void Evaluate__InterpolatesVector3__When__BetweenKeyframes()
        {
            var mgr = new KeyframeManager<Vector3>();
            var id1 = new KeyframeId(Guid.NewGuid());
            var id2 = new KeyframeId(Guid.NewGuid());
            mgr.Add(new Keyframe<Vector3>(id1, new TimePosition(0.0), new Vector3(0, 0, 0)));
            mgr.Add(new Keyframe<Vector3>(id2, new TimePosition(2.0), new Vector3(10, 20, 30)));
            var result = mgr.Evaluate(new TimePosition(1.0), Vector3.Lerp);
            result.X.Should().BeApproximately(5f, 0.01f);
            result.Y.Should().BeApproximately(10f, 0.01f);
            result.Z.Should().BeApproximately(15f, 0.01f);
        }

        // --- Quaternion evaluation ---

        [Fact]
        public void Evaluate__InterpolatesQuaternion__When__BetweenKeyframes()
        {
            var mgr = new KeyframeManager<Quaternion>();
            var id1 = new KeyframeId(Guid.NewGuid());
            var id2 = new KeyframeId(Guid.NewGuid());
            var q1 = Quaternion.Identity;
            var q2 = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 2);
            mgr.Add(new Keyframe<Quaternion>(id1, new TimePosition(0.0), q1));
            mgr.Add(new Keyframe<Quaternion>(id2, new TimePosition(2.0), q2));
            var result = mgr.Evaluate(new TimePosition(1.0), Quaternion.Slerp);
            var expected = Quaternion.Slerp(q1, q2, 0.5f);
            result.X.Should().BeApproximately(expected.X, 0.01f);
            result.Y.Should().BeApproximately(expected.Y, 0.01f);
            result.Z.Should().BeApproximately(expected.Z, 0.01f);
            result.W.Should().BeApproximately(expected.W, 0.01f);
        }
    }
}
