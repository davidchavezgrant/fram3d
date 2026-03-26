using System;
using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Shot;
using Fram3d.Core.Timeline;
using Xunit;

namespace Fram3d.Core.Tests.Shot
{
    public class ShotTests
    {
        private static Core.Shot.Shot MakeShot(
            string name = "Shot_01",
            Vector3? position = null,
            Quaternion? rotation = null)
        {
            return new Core.Shot.Shot(
                new ShotId(Guid.NewGuid()),
                name,
                position ?? Vector3.Zero,
                rotation ?? Quaternion.Identity
            );
        }

        // --- Constructor ---

        [Fact]
        public void Constructor__ThrowsArgumentNull__When__IdIsNull()
        {
            Action act = () => new Core.Shot.Shot(null, "Shot_01", Vector3.Zero, Quaternion.Identity);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor__ThrowsArgumentException__When__NameIsEmpty()
        {
            Action act = () => MakeShot(name: "");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor__ThrowsArgumentException__When__NameIsNull()
        {
            Action act = () => MakeShot(name: null);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor__SetsDefaultDuration__When__Created()
        {
            var shot = MakeShot();
            shot.Duration.Should().Be(Core.Shot.Shot.DEFAULT_DURATION);
        }

        [Fact]
        public void Constructor__CreatesInitialPositionKeyframe__When__Created()
        {
            var position = new Vector3(1, 2, 3);
            var shot = MakeShot(position: position);
            shot.CameraPositionKeyframes.Count.Should().Be(1);
            shot.CameraPositionKeyframes.Keyframes[0].Time.Should().Be(TimePosition.ZERO);
            shot.CameraPositionKeyframes.Keyframes[0].Value.Should().Be(position);
        }

        [Fact]
        public void Constructor__CreatesInitialRotationKeyframe__When__Created()
        {
            var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 4);
            var shot = MakeShot(rotation: rotation);
            shot.CameraRotationKeyframes.Count.Should().Be(1);
            shot.CameraRotationKeyframes.Keyframes[0].Time.Should().Be(TimePosition.ZERO);
            shot.CameraRotationKeyframes.Keyframes[0].Value.Should().Be(rotation);
        }

        [Fact]
        public void Constructor__StoresName__When__Valid()
        {
            var shot = MakeShot(name: "Wide Establishing");
            shot.Name.Should().Be("Wide Establishing");
        }

        // --- Name ---

        [Fact]
        public void Name__AcceptsNewName__When__NonEmpty()
        {
            var shot = MakeShot();
            shot.Name = "Over the Shoulder";
            shot.Name.Should().Be("Over the Shoulder");
        }

        [Fact]
        public void Name__RevertsToOld__When__SetToEmpty()
        {
            var shot = MakeShot(name: "Original");
            shot.Name = "";
            shot.Name.Should().Be("Original");
        }

        [Fact]
        public void Name__RevertsToOld__When__SetToNull()
        {
            var shot = MakeShot(name: "Original");
            shot.Name = null;
            shot.Name.Should().Be("Original");
        }

        [Fact]
        public void Name__TruncatesTo32__When__Longer()
        {
            var shot = MakeShot();
            var longName = new string('A', 50);
            shot.Name = longName;
            shot.Name.Length.Should().Be(Core.Shot.Shot.MAX_NAME_LENGTH);
        }

        [Fact]
        public void Name__AcceptsExactly32__When__ExactLength()
        {
            var shot = MakeShot();
            var exactName = new string('B', 32);
            shot.Name = exactName;
            shot.Name.Should().Be(exactName);
        }

        // --- Duration ---

        [Fact]
        public void Duration__ClampsToMin__When__SetBelowMinimum()
        {
            var shot = MakeShot();
            shot.Duration = 0.0;
            shot.Duration.Should().Be(Core.Shot.Shot.MIN_DURATION);
        }

        [Fact]
        public void Duration__ClampsToMax__When__SetAboveMaximum()
        {
            var shot = MakeShot();
            shot.Duration = 999.0;
            shot.Duration.Should().Be(Core.Shot.Shot.MAX_DURATION);
        }

        [Fact]
        public void Duration__AcceptsValid__When__InRange()
        {
            var shot = MakeShot();
            shot.Duration = 10.0;
            shot.Duration.Should().Be(10.0);
        }

        [Fact]
        public void Duration__ClampsNegative__When__SetNegative()
        {
            var shot = MakeShot();
            shot.Duration = -5.0;
            shot.Duration.Should().Be(Core.Shot.Shot.MIN_DURATION);
        }

        // --- Camera keyframes preserved when duration shortened ---

        [Fact]
        public void Duration__PreservesKeyframes__When__ShortenedBelowExistingKeyframeTimes()
        {
            var shot = MakeShot();
            shot.Duration = 10.0;
            // Add a keyframe at t=8
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(
                    new KeyframeId(Guid.NewGuid()),
                    new TimePosition(8.0),
                    new Vector3(5, 5, 5)
                )
            );
            shot.CameraPositionKeyframes.Count.Should().Be(2);

            // Shorten duration to 5 — keyframe at t=8 still exists
            shot.Duration = 5.0;
            shot.CameraPositionKeyframes.Count.Should().Be(2);
        }

        [Fact]
        public void Duration__KeyframesBecomeReachable__When__ExtendedAgain()
        {
            var shot = MakeShot();
            shot.Duration = 10.0;
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(
                    new KeyframeId(Guid.NewGuid()),
                    new TimePosition(8.0),
                    new Vector3(5, 5, 5)
                )
            );
            shot.Duration = 3.0;
            shot.Duration = 10.0;
            // Keyframe at t=8 is still there
            shot.CameraPositionKeyframes.Count.Should().Be(2);
            shot.CameraPositionKeyframes.Keyframes[1].Time.Seconds.Should().Be(8.0);
        }

        // --- EvaluateCameraPosition ---

        [Fact]
        public void EvaluateCameraPosition__ReturnsInitialPosition__When__AtTimeZero()
        {
            var position = new Vector3(1, 2, 3);
            var shot = MakeShot(position: position);
            shot.EvaluateCameraPosition(TimePosition.ZERO).Should().Be(position);
        }

        [Fact]
        public void EvaluateCameraPosition__Interpolates__When__BetweenKeyframes()
        {
            var shot = MakeShot(position: new Vector3(0, 0, 0));
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(
                    new KeyframeId(Guid.NewGuid()),
                    new TimePosition(2.0),
                    new Vector3(10, 0, 0)
                )
            );
            var result = shot.EvaluateCameraPosition(new TimePosition(1.0));
            result.X.Should().BeApproximately(5f, 0.01f);
        }

        // --- EvaluateCameraRotation ---

        [Fact]
        public void EvaluateCameraRotation__ReturnsInitialRotation__When__AtTimeZero()
        {
            var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 4);
            var shot = MakeShot(rotation: rotation);
            var result = shot.EvaluateCameraRotation(TimePosition.ZERO);
            result.X.Should().BeApproximately(rotation.X, 0.001f);
            result.Y.Should().BeApproximately(rotation.Y, 0.001f);
            result.Z.Should().BeApproximately(rotation.Z, 0.001f);
            result.W.Should().BeApproximately(rotation.W, 0.001f);
        }

        // --- TotalCameraKeyframeCount ---

        [Fact]
        public void TotalCameraKeyframeCount__ReturnsTwo__When__JustCreated()
        {
            var shot = MakeShot();
            shot.TotalCameraKeyframeCount.Should().Be(2); // 1 position + 1 rotation
        }

        [Fact]
        public void TotalCameraKeyframeCount__IncludesBoth__When__KeyframesAdded()
        {
            var shot = MakeShot();
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(
                    new KeyframeId(Guid.NewGuid()),
                    new TimePosition(1.0),
                    new Vector3(1, 0, 0)
                )
            );
            shot.TotalCameraKeyframeCount.Should().Be(3); // 2 position + 1 rotation
        }
    }
}
