using System;
using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
using Fram3d.Core.Timelines;
using Xunit;

namespace Fram3d.Core.Tests.Shots
{
    public class ShotTests
    {
        private static Shot MakeShot(
            string name = "Shot_01",
            Vector3? position = null,
            Quaternion? rotation = null)
        {
            return new Shot(
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
            Action act = () => new Shot(null, "Shot_01", Vector3.Zero, Quaternion.Identity);
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
            shot.Duration.Should().Be(Shot.DEFAULT_DURATION);
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
            shot.Name.Length.Should().Be(Shot.MAX_NAME_LENGTH);
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
            shot.Duration.Should().Be(Shot.MIN_DURATION);
        }

        [Fact]
        public void Duration__ClampsToMax__When__SetAboveMaximum()
        {
            var shot = MakeShot();
            shot.Duration = 999.0;
            shot.Duration.Should().Be(Shot.MAX_DURATION);
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
            shot.Duration.Should().Be(Shot.MIN_DURATION);
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

        [Fact]
        public void TotalCameraKeyframeCount__IncludesAllManagers__When__FloatKeyframesAdded()
        {
            var shot = MakeShot();
            shot.CameraFocalLengthKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 50f));
            shot.CameraApertureKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 2.8f));
            shot.CameraFocusDistanceKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 5f));
            shot.TotalCameraKeyframeCount.Should().Be(5); // 1 pos + 1 rot + 3 float
        }

        // --- Focal Length Keyframes ---

        [Fact]
        public void CameraFocalLengthKeyframes__IsEmpty__When__Created()
        {
            var shot = MakeShot();
            shot.CameraFocalLengthKeyframes.Count.Should().Be(0);
        }

        [Fact]
        public void EvaluateCameraFocalLength__ReturnsDefault__When__NoKeyframes()
        {
            var shot = MakeShot();
            shot.EvaluateCameraFocalLength(TimePosition.ZERO).Should().Be(0f);
        }

        [Fact]
        public void EvaluateCameraFocalLength__ReturnsValue__When__SingleKeyframe()
        {
            var shot = MakeShot();
            shot.CameraFocalLengthKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 50f));
            shot.EvaluateCameraFocalLength(TimePosition.ZERO).Should().Be(50f);
        }

        [Fact]
        public void EvaluateCameraFocalLength__Interpolates__When__BetweenKeyframes()
        {
            var shot = MakeShot();
            shot.CameraFocalLengthKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 24f));
            shot.CameraFocalLengthKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(2.0), 100f));
            shot.EvaluateCameraFocalLength(new TimePosition(1.0)).Should().BeApproximately(62f, 0.1f);
        }

        // --- Aperture Keyframes ---

        [Fact]
        public void CameraApertureKeyframes__IsEmpty__When__Created()
        {
            var shot = MakeShot();
            shot.CameraApertureKeyframes.Count.Should().Be(0);
        }

        [Fact]
        public void EvaluateCameraAperture__Interpolates__When__BetweenKeyframes()
        {
            var shot = MakeShot();
            shot.CameraApertureKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 2.8f));
            shot.CameraApertureKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(2.0), 5.6f));
            shot.EvaluateCameraAperture(new TimePosition(1.0)).Should().BeApproximately(4.2f, 0.1f);
        }

        // --- Focus Distance Keyframes ---

        [Fact]
        public void CameraFocusDistanceKeyframes__IsEmpty__When__Created()
        {
            var shot = MakeShot();
            shot.CameraFocusDistanceKeyframes.Count.Should().Be(0);
        }

        [Fact]
        public void EvaluateCameraFocusDistance__Interpolates__When__BetweenKeyframes()
        {
            var shot = MakeShot();
            shot.CameraFocusDistanceKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 1.5f));
            shot.CameraFocusDistanceKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(2.0), 10f));
            shot.EvaluateCameraFocusDistance(new TimePosition(1.0)).Should().BeApproximately(5.75f, 0.1f);
        }

        // --- GetAllCameraKeyframeTimes ---

        [Fact]
        public void GetAllCameraKeyframeTimes__ReturnsInitialTime__When__OnlyDefaultKeyframes()
        {
            var shot = MakeShot();
            var times = shot.GetAllCameraKeyframeTimes();
            times.Should().HaveCount(1);
            times[0].Should().Be(TimePosition.ZERO);
        }

        [Fact]
        public void GetAllCameraKeyframeTimes__MergesDistinctTimes__When__MultipleManagers()
        {
            var shot = MakeShot();
            // Position has t=0 (default). Add rotation at t=1 and focal length at t=2.
            shot.CameraRotationKeyframes.Add(
                new Keyframe<Quaternion>(new KeyframeId(Guid.NewGuid()), new TimePosition(1.0), Quaternion.Identity));
            shot.CameraFocalLengthKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(2.0), 50f));
            var times = shot.GetAllCameraKeyframeTimes();
            times.Should().HaveCount(3);
            times[0].Seconds.Should().Be(0.0);
            times[1].Seconds.Should().Be(1.0);
            times[2].Seconds.Should().Be(2.0);
        }

        [Fact]
        public void GetAllCameraKeyframeTimes__DeduplicatesSharedTimes__When__MultipleManagersSameTime()
        {
            var shot = MakeShot();
            // Both position and rotation have t=0 by default.
            var times = shot.GetAllCameraKeyframeTimes();
            times.Should().HaveCount(1);
        }

        // --- ClearAllCameraKeyframes ---

        [Fact]
        public void ClearAllCameraKeyframes__ClearsAll__When__Called()
        {
            var shot = MakeShot();
            shot.CameraFocalLengthKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 50f));
            shot.CameraApertureKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 2.8f));
            shot.CameraFocusDistanceKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), TimePosition.ZERO, 5f));

            shot.ClearAllCameraKeyframes();

            shot.CameraPositionKeyframes.Count.Should().Be(0);
            shot.CameraRotationKeyframes.Count.Should().Be(0);
            shot.CameraFocalLengthKeyframes.Count.Should().Be(0);
            shot.CameraApertureKeyframes.Count.Should().Be(0);
            shot.CameraFocusDistanceKeyframes.Count.Should().Be(0);
        }

        [Fact]
        public void GetAllCameraKeyframeTimes__ReturnsAllDistinctTimes__When__AllFiveManagersHaveKeyframes()
        {
            var shot = MakeShot();
            // Position and rotation already have t=0
            // Add focal length at t=1
            shot.CameraFocalLengthKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(1.0), 50f));
            // Aperture at t=2
            shot.CameraApertureKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(2.0), 2.8f));
            // Focus distance at t=3
            shot.CameraFocusDistanceKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), new TimePosition(3.0), 5f));

            var times = shot.GetAllCameraKeyframeTimes();
            times.Should().HaveCount(4); // t=0 (deduped), t=1, t=2, t=3
            times[0].Seconds.Should().Be(0.0);
            times[1].Seconds.Should().Be(1.0);
            times[2].Seconds.Should().Be(2.0);
            times[3].Seconds.Should().Be(3.0);
        }

        [Fact]
        public void GetAllCameraKeyframeTimes__ReturnsEmpty__When__AllKeyframesCleared()
        {
            var shot = MakeShot();
            shot.ClearAllCameraKeyframes();
            shot.GetAllCameraKeyframeTimes().Should().BeEmpty();
        }

        // --- CanDeleteCameraKeyframesAtTime ---

        [Fact]
        public void CanDeleteCameraKeyframesAtTime__ReturnsFalse__When__OnlyOneKeyframeTime()
        {
            var shot = MakeShot();
            shot.CanDeleteCameraKeyframesAtTime(TimePosition.ZERO).Should().BeFalse();
        }

        [Fact]
        public void CanDeleteCameraKeyframesAtTime__ReturnsTrue__When__MultipleKeyframeTimes()
        {
            var shot = MakeShot();
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), new TimePosition(1.0), Vector3.One));
            shot.CanDeleteCameraKeyframesAtTime(new TimePosition(1.0)).Should().BeTrue();
        }

        // --- DeleteAllCameraKeyframesAtTime ---

        [Fact]
        public void DeleteAllCameraKeyframesAtTime__RemovesKeyframes__When__MultipleTimesExist()
        {
            var shot = MakeShot();
            var t1 = new TimePosition(1.0);
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), t1, Vector3.One));
            shot.CameraRotationKeyframes.Add(
                new Keyframe<Quaternion>(new KeyframeId(Guid.NewGuid()), t1, Quaternion.Identity));

            shot.DeleteAllCameraKeyframesAtTime(t1).Should().BeTrue();

            shot.CameraPositionKeyframes.Count.Should().Be(1); // only t=0 remains
            shot.CameraRotationKeyframes.Count.Should().Be(1);
        }

        [Fact]
        public void DeleteAllCameraKeyframesAtTime__ReturnsFalse__When__LastKeyframeTime()
        {
            var shot = MakeShot();
            shot.DeleteAllCameraKeyframesAtTime(TimePosition.ZERO).Should().BeFalse();
            shot.CameraPositionKeyframes.Count.Should().Be(1); // unchanged
        }

        [Fact]
        public void DeleteAllCameraKeyframesAtTime__DeletesAcrossAllManagers__When__FloatKeyframesExist()
        {
            var shot = MakeShot();
            var t1 = new TimePosition(1.0);
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), t1, Vector3.One));
            shot.CameraFocalLengthKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), t1, 50f));
            shot.CameraApertureKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), t1, 2.8f));
            shot.CameraFocusDistanceKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), t1, 5f));

            shot.DeleteAllCameraKeyframesAtTime(t1).Should().BeTrue();

            shot.CameraFocalLengthKeyframes.Count.Should().Be(0);
            shot.CameraApertureKeyframes.Count.Should().Be(0);
            shot.CameraFocusDistanceKeyframes.Count.Should().Be(0);
        }

        // --- MoveAllCameraKeyframesAtTime ---

        [Fact]
        public void MoveAllCameraKeyframesAtTime__MovesKeyframes__When__Called()
        {
            var shot = MakeShot();
            var from = TimePosition.ZERO;
            var to   = new TimePosition(2.0);

            shot.MoveAllCameraKeyframesAtTime(from, to);

            shot.CameraPositionKeyframes.Keyframes[0].Time.Should().Be(to);
            shot.CameraRotationKeyframes.Keyframes[0].Time.Should().Be(to);
        }

        [Fact]
        public void MoveAllCameraKeyframesAtTime__MergesSilently__When__TargetTimeHasKeyframes()
        {
            var shot = MakeShot();
            var t1 = new TimePosition(1.0);
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), t1, new Vector3(5, 5, 5)));

            // Move t=0 onto t=1 — should merge (overwrite)
            shot.MoveAllCameraKeyframesAtTime(TimePosition.ZERO, t1);

            shot.CameraPositionKeyframes.Count.Should().Be(1);
            shot.CameraPositionKeyframes.Keyframes[0].Time.Should().Be(t1);
            // The arriving value (from t=0) overwrites
            shot.CameraPositionKeyframes.Keyframes[0].Value.Should().Be(Vector3.Zero);
        }

        [Fact]
        public void MoveAllCameraKeyframesAtTime__PreservesUnaffectedManagers__When__ManagerHasNoKeyframeAtTime()
        {
            var shot = MakeShot();
            var t1 = new TimePosition(1.0);
            // Only position has t=0, focal length has nothing
            shot.CameraFocalLengthKeyframes.Count.Should().Be(0);

            shot.MoveAllCameraKeyframesAtTime(TimePosition.ZERO, t1);

            // Focal length still empty — no crash, no phantom keyframe
            shot.CameraFocalLengthKeyframes.Count.Should().Be(0);
        }

        [Fact]
        public void MoveAllCameraKeyframesAtTime__MovesFloatManagers__When__AllManagersHaveKeyframes()
        {
            var shot = MakeShot();
            var t0 = TimePosition.ZERO;
            shot.CameraFocalLengthKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), t0, 50f));
            shot.CameraApertureKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), t0, 2.8f));
            shot.CameraFocusDistanceKeyframes.Add(
                new Keyframe<float>(new KeyframeId(Guid.NewGuid()), t0, 5f));

            var t1 = new TimePosition(2.0);
            shot.MoveAllCameraKeyframesAtTime(t0, t1);

            shot.CameraFocalLengthKeyframes.Keyframes[0].Time.Should().Be(t1);
            shot.CameraApertureKeyframes.Keyframes[0].Time.Should().Be(t1);
            shot.CameraFocusDistanceKeyframes.Keyframes[0].Time.Should().Be(t1);
        }
    }
}
