using System;
using System.Collections.Generic;
using System.Numerics;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
using Xunit;

namespace Fram3d.Core.Tests.Shots
{
    public class ShotRegistryTests
    {
        private static readonly Vector3    DEFAULT_POS = new(0, 1.6f, 5);
        private static readonly Quaternion DEFAULT_ROT = Quaternion.Identity;

        private ShotRegistry MakeRegistry() => new();

        private Shot AddDefaultShot(ShotRegistry registry) =>
            registry.AddShot(DEFAULT_POS, DEFAULT_ROT);

        // --- AddShot ---

        [Fact]
        public void AddShot__IncreasesCount__When__Called()
        {
            var reg = MakeRegistry();
            AddDefaultShot(reg);
            reg.Count.Should().Be(1);
        }

        [Fact]
        public void AddShot__AutoNamesSequentially__When__MultipleAdded()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            var s2 = AddDefaultShot(reg);
            var s3 = AddDefaultShot(reg);
            s1.Name.Should().Be("Shot_01");
            s2.Name.Should().Be("Shot_02");
            s3.Name.Should().Be("Shot_03");
        }

        [Fact]
        public void AddShot__SetsCurrentShot__When__Added()
        {
            var reg = MakeRegistry();
            var shot = AddDefaultShot(reg);
            reg.CurrentShot.Should().Be(shot);
        }

        [Fact]
        public void AddShot__SetsCurrentToLatest__When__MultipleAdded()
        {
            var reg = MakeRegistry();
            AddDefaultShot(reg);
            var s2 = AddDefaultShot(reg);
            reg.CurrentShot.Should().Be(s2);
        }

        [Fact]
        public void AddShot__CapturesCameraState__When__Called()
        {
            var reg = MakeRegistry();
            var pos = new Vector3(5, 3, 10);
            var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 4);
            var shot = reg.AddShot(pos, rot);
            shot.EvaluateCameraPosition(TimePosition.ZERO).Should().Be(pos);
        }

        [Fact]
        public void AddShot__EmitsShotAdded__When__Added()
        {
            var reg = MakeRegistry();
            Shot emitted = null;
            reg.ShotAdded.Subscribe(s => emitted = s);
            var shot = AddDefaultShot(reg);
            emitted.Should().Be(shot);
        }

        [Fact]
        public void AddShot__EmitsCurrentShotChanged__When__Added()
        {
            var reg = MakeRegistry();
            Shot emitted = null;
            reg.CurrentShotChanged.Subscribe(s => emitted = s);
            var shot = AddDefaultShot(reg);
            emitted.Should().Be(shot);
        }

        // --- RemoveShot ---

        [Fact]
        public void RemoveShot__ReturnsTrue__When__ShotExists()
        {
            var reg = MakeRegistry();
            var shot = AddDefaultShot(reg);
            reg.RemoveShot(shot.Id).Should().BeTrue();
            reg.Count.Should().Be(0);
        }

        [Fact]
        public void RemoveShot__ReturnsFalse__When__ShotDoesNotExist()
        {
            var reg = MakeRegistry();
            reg.RemoveShot(new ShotId(Guid.NewGuid())).Should().BeFalse();
        }

        [Fact]
        public void RemoveShot__SelectsNextShot__When__CurrentShotRemoved()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            var s2 = AddDefaultShot(reg);
            var s3 = AddDefaultShot(reg);
            reg.SetCurrentShot(s2.Id);
            reg.RemoveShot(s2.Id);
            reg.CurrentShot.Should().Be(s3);
        }

        [Fact]
        public void RemoveShot__SelectsPreviousShot__When__LastShotRemovedAndWasCurrent()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            var s2 = AddDefaultShot(reg);
            reg.SetCurrentShot(s2.Id);
            reg.RemoveShot(s2.Id);
            reg.CurrentShot.Should().Be(s1);
        }

        [Fact]
        public void RemoveShot__SetsCurrentToNull__When__LastShotRemoved()
        {
            var reg = MakeRegistry();
            var shot = AddDefaultShot(reg);
            reg.RemoveShot(shot.Id);
            reg.CurrentShot.Should().BeNull();
        }

        [Fact]
        public void RemoveShot__EmitsShotRemoved__When__Removed()
        {
            var reg = MakeRegistry();
            var shot = AddDefaultShot(reg);
            Shot emitted = null;
            reg.ShotRemoved.Subscribe(s => emitted = s);
            reg.RemoveShot(shot.Id);
            emitted.Should().Be(shot);
        }

        [Fact]
        public void RemoveShot__DoesNotChangeCurrentShot__When__NonCurrentRemoved()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            var s2 = AddDefaultShot(reg);
            reg.SetCurrentShot(s2.Id);
            reg.RemoveShot(s1.Id);
            reg.CurrentShot.Should().Be(s2);
        }

        // --- GetById ---

        [Fact]
        public void GetById__ReturnsShot__When__Exists()
        {
            var reg = MakeRegistry();
            var shot = AddDefaultShot(reg);
            reg.GetById(shot.Id).Should().Be(shot);
        }

        [Fact]
        public void GetById__ReturnsNull__When__NotFound()
        {
            var reg = MakeRegistry();
            reg.GetById(new ShotId(Guid.NewGuid())).Should().BeNull();
        }

        // --- IndexOf ---

        [Fact]
        public void IndexOf__ReturnsCorrectIndex__When__ShotExists()
        {
            var reg = MakeRegistry();
            AddDefaultShot(reg);
            var s2 = AddDefaultShot(reg);
            AddDefaultShot(reg);
            reg.IndexOf(s2.Id).Should().Be(1);
        }

        [Fact]
        public void IndexOf__ReturnsMinusOne__When__NotFound()
        {
            var reg = MakeRegistry();
            reg.IndexOf(new ShotId(Guid.NewGuid())).Should().Be(-1);
        }

        // --- SetCurrentShot ---

        [Fact]
        public void SetCurrentShot__ChangesCurrentShot__When__Valid()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            AddDefaultShot(reg);
            reg.SetCurrentShot(s1.Id);
            reg.CurrentShot.Should().Be(s1);
        }

        [Fact]
        public void SetCurrentShot__ThrowsArgumentException__When__NotFound()
        {
            var reg = MakeRegistry();
            Action act = () => reg.SetCurrentShot(new ShotId(Guid.NewGuid()));
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SetCurrentShot__DoesNotEmit__When__SameShot()
        {
            var reg = MakeRegistry();
            var shot = AddDefaultShot(reg);
            var emitted = false;
            reg.CurrentShotChanged.Subscribe(_ => emitted = true);
            reg.SetCurrentShot(shot.Id);
            emitted.Should().BeFalse();
        }

        // --- Reorder ---

        [Fact]
        public void Reorder__MovesShot__When__ValidIndices()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            var s2 = AddDefaultShot(reg);
            var s3 = AddDefaultShot(reg);
            reg.Reorder(s3.Id, 0);
            reg.Shots[0].Should().Be(s3);
            reg.Shots[1].Should().Be(s1);
            reg.Shots[2].Should().Be(s2);
        }

        [Fact]
        public void Reorder__PreservesNames__When__Reordered()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            var s2 = AddDefaultShot(reg);
            var s3 = AddDefaultShot(reg);
            reg.Reorder(s3.Id, 0);
            s3.Name.Should().Be("Shot_03");
            s1.Name.Should().Be("Shot_01");
        }

        [Fact]
        public void Reorder__EmitsReordered__When__Reordered()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            AddDefaultShot(reg);
            var emitted = false;
            reg.Reordered.Subscribe(_ => emitted = true);
            reg.Reorder(s1.Id, 1);
            emitted.Should().BeTrue();
        }

        [Fact]
        public void Reorder__DoesNotEmit__When__SamePosition()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            AddDefaultShot(reg);
            var emitted = false;
            reg.Reordered.Subscribe(_ => emitted = true);
            reg.Reorder(s1.Id, 0);
            emitted.Should().BeFalse();
        }

        [Fact]
        public void Reorder__ThrowsArgumentException__When__ShotNotFound()
        {
            var reg = MakeRegistry();
            AddDefaultShot(reg);
            Action act = () => reg.Reorder(new ShotId(Guid.NewGuid()), 0);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Reorder__ThrowsArgumentOutOfRange__When__IndexOutOfBounds()
        {
            var reg = MakeRegistry();
            var shot = AddDefaultShot(reg);
            Action act = () => reg.Reorder(shot.Id, 5);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        // --- TotalDuration ---

        [Fact]
        public void TotalDuration__ReturnsZero__When__NoShots()
        {
            var reg = MakeRegistry();
            reg.TotalDuration.Should().Be(0.0);
        }

        [Fact]
        public void TotalDuration__ReturnsSumOfDurations__When__MultipleShots()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            var s2 = AddDefaultShot(reg);
            s1.Duration = 3.0;
            s2.Duration = 7.0;
            reg.TotalDuration.Should().Be(10.0);
        }

        // --- GetGlobalStartTime / GetGlobalEndTime ---

        [Fact]
        public void GetGlobalStartTime__ReturnsZero__When__FirstShot()
        {
            var reg = MakeRegistry();
            var shot = AddDefaultShot(reg);
            reg.GetGlobalStartTime(shot.Id).Seconds.Should().Be(0.0);
        }

        [Fact]
        public void GetGlobalStartTime__ReturnsPreviousShotEnd__When__SecondShot()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            s1.Duration = 3.0;
            var s2 = AddDefaultShot(reg);
            reg.GetGlobalStartTime(s2.Id).Seconds.Should().Be(3.0);
        }

        [Fact]
        public void GetGlobalStartTime__ThrowsArgumentException__When__NotFound()
        {
            var reg = MakeRegistry();
            Action act = () => reg.GetGlobalStartTime(new ShotId(Guid.NewGuid()));
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetGlobalEndTime__ReturnsDuration__When__FirstShot()
        {
            var reg = MakeRegistry();
            var shot = AddDefaultShot(reg);
            shot.Duration = 4.0;
            reg.GetGlobalEndTime(shot.Id).Seconds.Should().Be(4.0);
        }

        [Fact]
        public void GetGlobalEndTime__ReturnsCumulativeEnd__When__SecondShot()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            s1.Duration = 3.0;
            var s2 = AddDefaultShot(reg);
            s2.Duration = 5.0;
            reg.GetGlobalEndTime(s2.Id).Seconds.Should().Be(8.0);
        }

        [Fact]
        public void GetGlobalStartTime__UpdatesAfterReorder__When__ShotsSwapped()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            s1.Duration = 3.0;
            var s2 = AddDefaultShot(reg);
            s2.Duration = 7.0;
            // Swap: s2 is now first
            reg.Reorder(s2.Id, 0);
            reg.GetGlobalStartTime(s2.Id).Seconds.Should().Be(0.0);
            reg.GetGlobalStartTime(s1.Id).Seconds.Should().Be(7.0);
        }

        // --- GetShotAtGlobalTime ---

        [Fact]
        public void GetShotAtGlobalTime__ReturnsFirstShot__When__TimeInFirstShot()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            s1.Duration = 5.0;
            AddDefaultShot(reg);
            var result = reg.GetShotAtGlobalTime(new TimePosition(2.5));
            result.Should().NotBeNull();
            result.Value.shot.Should().Be(s1);
            result.Value.localTime.Seconds.Should().BeApproximately(2.5, 1e-9);
        }

        [Fact]
        public void GetShotAtGlobalTime__ReturnsSecondShot__When__TimeInSecondShot()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            s1.Duration = 5.0;
            var s2 = AddDefaultShot(reg);
            s2.Duration = 5.0;
            var result = reg.GetShotAtGlobalTime(new TimePosition(7.0));
            result.Should().NotBeNull();
            result.Value.shot.Should().Be(s2);
            result.Value.localTime.Seconds.Should().BeApproximately(2.0, 1e-9);
        }

        [Fact]
        public void GetShotAtGlobalTime__ReturnsNull__When__TimeBeyondAllShots()
        {
            var reg = MakeRegistry();
            var shot = AddDefaultShot(reg);
            shot.Duration = 5.0;
            reg.GetShotAtGlobalTime(new TimePosition(10.0)).Should().BeNull();
        }

        [Fact]
        public void GetShotAtGlobalTime__ReturnsNull__When__NoShots()
        {
            var reg = MakeRegistry();
            reg.GetShotAtGlobalTime(new TimePosition(0.0)).Should().BeNull();
        }

        [Fact]
        public void GetShotAtGlobalTime__ReturnsLastShotAtEnd__When__ExactlyAtTotalDuration()
        {
            var reg = MakeRegistry();
            var shot = AddDefaultShot(reg);
            shot.Duration = 5.0;
            var result = reg.GetShotAtGlobalTime(new TimePosition(5.0));
            result.Should().NotBeNull();
            result.Value.shot.Should().Be(shot);
            result.Value.localTime.Seconds.Should().BeApproximately(5.0, 1e-9);
        }

        [Fact]
        public void GetShotAtGlobalTime__ReturnsShotBoundary__When__AtExactShotBoundary()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            s1.Duration = 5.0;
            var s2 = AddDefaultShot(reg);
            s2.Duration = 5.0;
            // At t=5.0, should return s2 at local t=0
            var result = reg.GetShotAtGlobalTime(new TimePosition(5.0));
            result.Should().NotBeNull();
            result.Value.shot.Should().Be(s2);
            result.Value.localTime.Seconds.Should().BeApproximately(0.0, 1e-9);
        }

        // --- Clear ---

        [Fact]
        public void Clear__RemovesAllShots__When__Called()
        {
            var reg = MakeRegistry();
            AddDefaultShot(reg);
            AddDefaultShot(reg);
            reg.Clear();
            reg.Count.Should().Be(0);
            reg.CurrentShot.Should().BeNull();
        }

        [Fact]
        public void Clear__ResetsNameCounter__When__Called()
        {
            var reg = MakeRegistry();
            AddDefaultShot(reg);
            AddDefaultShot(reg);
            reg.Clear();
            var s = AddDefaultShot(reg);
            s.Name.Should().Be("Shot_01");
        }

        [Fact]
        public void Clear__EmitsShotRemoved__When__Called()
        {
            var reg = MakeRegistry();
            AddDefaultShot(reg);
            AddDefaultShot(reg);
            var removedShots = new List<Shot>();
            reg.ShotRemoved.Subscribe(s => removedShots.Add(s));
            reg.Clear();
            removedShots.Should().HaveCount(2);
        }

        // --- Naming edge cases ---

        [Fact]
        public void AddShot__ContinuesNumbering__When__ShotsRemovedBetweenAdds()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg); // Shot_01
            AddDefaultShot(reg);          // Shot_02
            reg.RemoveShot(s1.Id);
            var s3 = AddDefaultShot(reg); // Shot_03, not Shot_01
            s3.Name.Should().Be("Shot_03");
        }

        // --- Acceptance criteria from spec ---

        [Fact]
        public void AcceptanceCriteria1__ThreeShotsExist__When__AddShotClickedThreeTimes()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            var s2 = AddDefaultShot(reg);
            var s3 = AddDefaultShot(reg);
            reg.Count.Should().Be(3);
            s1.Name.Should().Be("Shot_01");
            s2.Name.Should().Be("Shot_02");
            s3.Name.Should().Be("Shot_03");
            reg.CurrentShot.Should().Be(s3);
        }

        [Fact]
        public void AcceptanceCriteria2__NameChanges__When__UserRenames()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            var s2 = AddDefaultShot(reg);
            s2.Name = "Over the Shoulder";
            s2.Name.Should().Be("Over the Shoulder");
            s1.Name.Should().Be("Shot_01");
        }

        [Fact]
        public void AcceptanceCriteria3__DurationUpdates__When__UserSetsDuration()
        {
            var reg = MakeRegistry();
            var shot = AddDefaultShot(reg);
            shot.Duration = 3.0;
            shot.Duration.Should().Be(3.0);
        }

        [Fact]
        public void AcceptanceCriteria4__ShotDeleted__When__ConfirmedByUser()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            var s2 = AddDefaultShot(reg);
            var s3 = AddDefaultShot(reg);
            reg.RemoveShot(s2.Id);
            reg.Count.Should().Be(2);
            reg.Shots[0].Should().Be(s1);
            reg.Shots[1].Should().Be(s3);
        }

        [Fact]
        public void AcceptanceCriteria5__ShotDraggedToNewPosition__When__Reordered()
        {
            var reg = MakeRegistry();
            var s1 = AddDefaultShot(reg);
            var s2 = AddDefaultShot(reg);
            var s3 = AddDefaultShot(reg);
            reg.Reorder(s3.Id, 0);
            reg.Shots[0].Should().Be(s3);
            reg.Shots[1].Should().Be(s1);
            s3.Name.Should().Be("Shot_03"); // Names unchanged
            s1.Name.Should().Be("Shot_01");
        }

        [Fact]
        public void AcceptanceCriteria15__EmptySequence__When__AllShotsDeleted()
        {
            var reg = MakeRegistry();
            var shot = AddDefaultShot(reg);
            reg.RemoveShot(shot.Id);
            reg.Count.Should().Be(0);
            reg.CurrentShot.Should().BeNull();
            // Can add new shots after clearing
            var newShot = AddDefaultShot(reg);
            newShot.Should().NotBeNull();
        }

        [Fact]
        public void AcceptanceCriteria16__DurationClampsToMin__When__ZeroEntered()
        {
            var shot = new Shot(
                new ShotId(Guid.NewGuid()), "Test", Vector3.Zero, Quaternion.Identity
            );
            shot.Duration = 0.0;
            shot.Duration.Should().Be(Shot.MIN_DURATION);
        }

        [Fact]
        public void AcceptanceCriteria17__DurationClampsToMax__When__999Entered()
        {
            var shot = new Shot(
                new ShotId(Guid.NewGuid()), "Test", Vector3.Zero, Quaternion.Identity
            );
            shot.Duration = 999.0;
            shot.Duration.Should().Be(Shot.MAX_DURATION);
        }
    }
}
