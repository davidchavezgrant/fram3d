using System;
using System.Linq;
using FluentAssertions;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
using Fram3d.Core.Timelines;
using System.Numerics;
using Xunit;
namespace Fram3d.Core.Tests.Timelines
{
    public sealed class TimelineTests
    {
        private static Timeline Create(int shotCount = 2)
        {
            var timeline = new Timeline(FrameRate.FPS_24);

            for (var i = 0; i < shotCount; i++)
            {
                timeline.AddShot(Vector3.Zero, Quaternion.Identity);
            }

            timeline.InitializeViewRange(1000);
            return timeline;
        }

        // ── Shot lifecycle ─────────────────────────────────────────────

        [Fact]
        public void AddShot__IncrementsCount__When__Called() =>
            Create(3).Count.Should().Be(3);

        [Fact]
        public void AddShot__SetsAsCurrent__When__Called()
        {
            var t = Create(2);

            t.CurrentShot.Name.Should().Be("Shot_02");
        }

        [Fact]
        public void RemoveShot__SelectsNext__When__CurrentRemoved()
        {
            var t = Create(3);
            t.SetCurrentShot(t.Shots[1].Id);

            t.RemoveShot(t.Shots[1].Id);

            t.CurrentShot.Should().Be(t.Shots[1]); // was index 2, now index 1
        }

        [Fact]
        public void Reorder__MovesShot__When__Called()
        {
            var t     = Create(3);
            var shot3 = t.Shots[2];

            t.Reorder(shot3.Id, 0);

            t.Shots[0].Should().Be(shot3);
        }

        // ── View range ─────────────────────────────────────────────────

        [Fact]
        public void FitAll__SetsViewToFullDuration__When__Called()
        {
            var t = Create();

            t.ViewStart.Should().Be(0);
            t.ViewEnd.Should().Be(t.TotalDuration);
        }

        [Fact]
        public void TimeToPixel__ReturnsCorrectPixel__When__Called()
        {
            var t = Create();

            t.TimeToPixel(5.0).Should().BeApproximately(500, 0.1);
        }

        [Fact]
        public void PixelToTime__ReturnsCorrectTime__When__Called()
        {
            var t = Create();

            t.PixelToTime(500).Should().BeApproximately(5.0, 0.01);
        }

        [Fact]
        public void ZoomAtPoint__ZoomsIn__When__PositiveDelta()
        {
            var t      = Create();
            var before = t.VisibleDuration;

            t.ZoomAtPoint(5.0, 1f);

            t.VisibleDuration.Should().BeLessThan(before);
        }

        [Fact]
        public void Pan__ClampsLeft__When__PanningPastZero()
        {
            var t = Create();

            t.Pan(-99999);

            t.ViewStart.Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public void Pan__ClampsRight__When__PanningPastEnd()
        {
            var t = Create();
            t.ZoomAtPoint(2.0, 1f);

            t.Pan(99999);

            t.ViewEnd.Should().BeLessOrEqualTo(t.TotalDuration);
        }

        // ── Edge detection ─────────────────────────────────────────────

        [Fact]
        public void FindEdgeAtTime__ReturnsIndex__When__NearShotEdge()
        {
            var t = Create(3);

            t.FindEdgeAtTime(5.0, 0.1).Should().Be(0);
        }

        [Fact]
        public void FindEdgeAtTime__ReturnsNegativeOne__When__NotNearEdge()
        {
            var t = Create(3);

            t.FindEdgeAtTime(7.5, 0.1).Should().Be(-1);
        }

        // ── Resize ─────────────────────────────────────────────────────

        [Fact]
        public void ResizeShotAtEdge__SnapsToFrame__When__Called()
        {
            var t = Create(3);

            var newDuration = t.ResizeShotAtEdge(0, 3.03);

            var expected = System.Math.Round(3.03 * 24) / 24;
            newDuration.Should().BeApproximately(expected, 1e-9);
        }

        // ── Fit to shot ────────────────────────────────────────────────

        [Fact]
        public void FitToShot__FitsAll__When__SingleShot()
        {
            var t = Create(1);

            t.FitToShot(t.Shots[0].Id);

            t.ViewStart.Should().Be(0);
            t.ViewEnd.Should().Be(t.TotalDuration);
        }

        [Fact]
        public void FitToShot__NoLeftPadding__When__FirstShot()
        {
            var t = Create(3);

            t.FitToShot(t.Shots[0].Id);

            t.ViewStart.Should().Be(0);
        }

        [Fact]
        public void FitToShot__NoRightPadding__When__LastShot()
        {
            var t = Create(3);

            t.FitToShot(t.Shots[2].Id);

            t.ViewEnd.Should().Be(t.TotalDuration);
        }

        // ── Playback ───────────────────────────────────────────────────

        [Fact]
        public void TogglePlayback__StartsPlaying__When__Stopped()
        {
            var t = Create();

            t.TogglePlayback().Should().BeTrue();
            t.Playhead.IsPlaying.Should().BeTrue();
        }

        [Fact]
        public void TogglePlayback__ResetsToZero__When__AtEnd()
        {
            var t = Create();
            t.Playhead.Scrub(t.TotalDuration, t.TotalDuration);

            t.TogglePlayback();

            t.Playhead.CurrentTime.Should().Be(0);
        }

        [Fact]
        public void Advance__StopsAtEnd__When__ReachingTotalDuration()
        {
            var t = Create();
            t.TogglePlayback();

            t.Advance(999);

            t.Playhead.IsPlaying.Should().BeFalse();
        }

        [Fact]
        public void Advance__FiresCameraEvaluation__When__Playing()
        {
            var t     = Create();
            Shot shot = null;
            t.CameraEvaluationRequested.Subscribe(eval => shot = eval.Shot);
            t.TogglePlayback();

            t.Advance(0.1);

            shot.Should().NotBeNull();
        }

        // ── Scrub ──────────────────────────────────────────────────────

        [Fact]
        public void ScrubToPixel__MovesPlayhead__When__Called()
        {
            var t = Create();

            t.ScrubToPixel(500);

            t.Playhead.CurrentTime.Should().BeGreaterThan(0);
        }

        // ── Strip interaction ──────────────────────────────────────────

        [Fact]
        public void ShotTrackPointerDown__StartsBoundaryDrag__When__NearEdge()
        {
            var t      = Create();
            var edgePx = t.TimeToPixel(5.0);

            t.ShotTrackPointerDown(edgePx, 0).Should().Be(ShotTrackAction.BOUNDARY_DRAG);
        }

        [Fact]
        public void BeginBoundaryDrag__StartsBoundaryDrag__When__EdgeIndexValid()
        {
            var t = Create();

            t.BeginBoundaryDrag(0).Should().Be(ShotTrackAction.BOUNDARY_DRAG);
            t.IsBoundaryDragging.Should().BeTrue();
            t.BoundaryDragIndex.Should().Be(0);
        }

        [Fact]
        public void BeginBoundaryDrag__ClearsPendingShotPress__When__BoundaryDragStartsExplicitly()
        {
            var t     = Create();
            var midPx = t.TimeToPixel(2.5);

            t.ShotTrackPointerDown(midPx, 0).Should().Be(ShotTrackAction.POTENTIAL_CLICK);
            t.BeginBoundaryDrag(0).Should().Be(ShotTrackAction.BOUNDARY_DRAG);
            t.ShotTrackPointerUp().Should().Be(ShotTrackAction.BOUNDARY_COMPLETE);

            t.ShotTrackPointerMove(midPx + 25, 300).Should().Be(ShotTrackAction.NONE);
            t.IsDragging.Should().BeFalse();
        }

        [Fact]
        public void ShotTrackPointerDown__StartsPotentialClick__When__OnShot()
        {
            var t = Create();

            t.ShotTrackPointerDown(250, 0).Should().Be(ShotTrackAction.POTENTIAL_CLICK);
        }

        [Fact]
        public void ShotTrackPointerUp__CompletesClick__When__NoMove()
        {
            var t = Create();
            t.ShotTrackPointerDown(250, 0);

            t.ShotTrackPointerUp().Should().Be(ShotTrackAction.CLICK);
        }

        [Fact]
        public void ShotTrackPointerMove__StartsDrag__When__HeldLongEnough()
        {
            var t = Create();
            t.ShotTrackPointerDown(250, 0);

            t.ShotTrackPointerMove(250, 300).Should().Be(ShotTrackAction.DRAG_START);
        }

        // ── Formatting ─────────────────────────────────────────────────

        [Fact]
        public void FormatShotTooltip__ContainsName__When__Called()
        {
            var t = Create();

            t.FormatShotTooltip(t.Shots[0]).Should().Contain("Shot_01");
        }

        [Fact]
        public void FormatResizeTooltip__ShowsRipple__When__ShiftNotHeld()
        {
            var t = Create();

            t.FormatResizeTooltip(0, false).Should().Contain("[ripple]");
        }

        [Fact]
        public void FormatResizeTooltip__ShowsShotsOnly__When__ShiftHeld()
        {
            var t = Create();

            t.FormatResizeTooltip(0, true).Should().Contain("[shots only]");
        }

        // --- Interaction state machine ---

        [Fact]
        public void ShotTrackPointerMove__ReturnsNearEdge__When__CursorNearShotBoundary()
        {
            var t = Create();
            t.InitializeViewRange(500);
            // Shot_01 ends at 5.0s. Find that pixel position.
            var edgePx = t.TimeToPixel(5.0);

            var result = t.ShotTrackPointerMove(edgePx, 0);

            result.Should().Be(ShotTrackAction.NEAR_EDGE);
        }

        [Fact]
        public void ShotTrackPointerMove__ReturnsNone__When__CursorFarFromEdge()
        {
            var t = Create();
            t.InitializeViewRange(500);
            // Middle of shot 1 — far from any edge
            var midPx = t.TimeToPixel(2.5);

            var result = t.ShotTrackPointerMove(midPx, 0);

            result.Should().Be(ShotTrackAction.NONE);
        }

        [Fact]
        public void ShotTrackPointerUp__ReturnsBoundaryComplete__When__BoundaryDragging()
        {
            var t = Create();
            t.InitializeViewRange(500);
            var edgePx = t.TimeToPixel(5.0);

            // Start boundary drag
            t.ShotTrackPointerDown(edgePx, 0).Should().Be(ShotTrackAction.BOUNDARY_DRAG);
            t.IsBoundaryDragging.Should().BeTrue();

            // Release
            var result = t.ShotTrackPointerUp();

            result.Should().Be(ShotTrackAction.BOUNDARY_COMPLETE);
            t.IsBoundaryDragging.Should().BeFalse();
        }

        [Fact]
        public void ShotTrackPointerUp__ReturnsNone__When__NoShotCaptured()
        {
            var t = Create();
            t.InitializeViewRange(500);
            // Click in empty space past all shots
            var emptyPx = t.TimeToPixel(999.0);
            t.ShotTrackPointerDown(emptyPx, 0);

            var result = t.ShotTrackPointerUp();

            result.Should().Be(ShotTrackAction.NONE);
        }

        [Fact]
        public void ShotTrackPointerMove__ReturnsDragStart__When__HeldLongEnough()
        {
            var t = Create();
            t.InitializeViewRange(500);
            var midPx = t.TimeToPixel(2.5);

            t.ShotTrackPointerDown(midPx, 0);
            // Move after hold threshold (200ms)
            var result = t.ShotTrackPointerMove(midPx, 201);

            result.Should().Be(ShotTrackAction.DRAG_START);
            t.IsDragging.Should().BeTrue();
        }

        [Fact]
        public void ShotTrackPointerMove__ReturnsDragMove__When__AlreadyDragging()
        {
            var t = Create();
            t.InitializeViewRange(500);
            var midPx = t.TimeToPixel(2.5);

            t.ShotTrackPointerDown(midPx, 0);
            t.ShotTrackPointerMove(midPx, 201); // DRAG_START
            var result = t.ShotTrackPointerMove(midPx + 50, 300);

            result.Should().Be(ShotTrackAction.DRAG_MOVE);
        }

        [Fact]
        public void ShotTrackPointerUp__ReturnsDragComplete__When__Dragging()
        {
            var t = Create();
            t.InitializeViewRange(500);
            var midPx = t.TimeToPixel(2.5);

            t.ShotTrackPointerDown(midPx, 0);
            t.ShotTrackPointerMove(midPx, 201); // DRAG_START

            var result = t.ShotTrackPointerUp();

            result.Should().Be(ShotTrackAction.DRAG_COMPLETE);
            t.IsDragging.Should().BeFalse();
        }

        [Fact]
        public void CompleteDrag__ReordersShotForward__When__DraggedPastNextShot()
        {
            var t = Create(); // Shot_01 (5s), Shot_02 (5s)
            t.InitializeViewRange(500);
            var shot1 = t.Shots[0];
            var shot2 = t.Shots[1];

            // Start drag on Shot_01 (at 2.5s)
            var startPx = t.TimeToPixel(2.5);
            t.ShotTrackPointerDown(startPx, 0);
            t.ShotTrackPointerMove(startPx, 201); // DRAG_START

            // Move past Shot_02's midpoint (7.5s) → insertion index = 2
            var endPx = t.TimeToPixel(7.5);
            t.ShotTrackPointerMove(endPx, 300); // DRAG_MOVE

            t.ShotTrackPointerUp(); // DRAG_COMPLETE

            // Shot_01 should now be at index 1
            t.Shots[0].Should().Be(shot2);
            t.Shots[1].Should().Be(shot1);
        }

        [Fact]
        public void CompleteDrag__ReordersShotBackward__When__DraggedBeforePreviousShot()
        {
            var t = Create(); // Shot_01, Shot_02
            t.InitializeViewRange(500);
            var shot1 = t.Shots[0];
            var shot2 = t.Shots[1];

            // Start drag on Shot_02 (at 7.5s — middle of second shot)
            var startPx = t.TimeToPixel(7.5);
            t.ShotTrackPointerDown(startPx, 0);
            t.ShotTrackPointerMove(startPx, 201); // DRAG_START

            // Move before Shot_01's midpoint (2.5s) → insertion index = 0
            var endPx = t.TimeToPixel(1.0);
            t.ShotTrackPointerMove(endPx, 300); // DRAG_MOVE

            t.ShotTrackPointerUp(); // DRAG_COMPLETE

            // Shot_02 should now be at index 0
            t.Shots[0].Should().Be(shot2);
            t.Shots[1].Should().Be(shot1);
        }

        // --- FormatBoundaryTooltip ---

        [Fact]
        public void FormatBoundaryTooltip__ReturnsEmpty__When__NoBoundaryDrag()
        {
            var t = Create();

            t.FormatBoundaryTooltip(false).Should().BeEmpty();
        }

        [Fact]
        public void FormatBoundaryTooltip__ReturnsTooltip__When__BoundaryDragging()
        {
            var t = Create();
            t.InitializeViewRange(500);
            var edgePx = t.TimeToPixel(5.0);

            t.ShotTrackPointerDown(edgePx, 0); // starts boundary drag

            t.FormatBoundaryTooltip(false).Should().Contain("[ripple]");
        }

        // --- Advance viewport auto-pan ---

        [Fact]
        public void Advance__PansViewForward__When__PlayheadExceedsViewEnd()
        {
            var t = Create();
            t.InitializeViewRange(500);
            // Zoom in so view shows only 2s (from 0 to 2)
            t.SetViewRange(0, 2.0);
            t.TogglePlayback();

            // Advance past viewEnd (2.0s)
            t.Advance(2.5);

            // View should have shifted forward
            t.ViewStart.Should().BeGreaterThan(0);
        }

        // --- EnsureVisible ---

        [Fact]
        public void EnsureVisible__ShiftsViewRight__When__TimePastViewEnd()
        {
            var t = Create();
            t.InitializeViewRange(500);
            t.SetViewRange(0, 3.0);
            var oldStart = t.ViewStart;

            t.EnsureVisible(5.0);

            t.ViewStart.Should().BeGreaterThan(oldStart);
            t.ViewEnd.Should().BeGreaterThanOrEqualTo(5.0);
        }

        [Fact]
        public void EnsureVisible__ShiftsViewLeft__When__TimeBeforeViewStart()
        {
            var t = Create();
            t.InitializeViewRange(500);
            t.SetViewRange(3.0, 6.0);

            t.EnsureVisible(1.0);

            t.ViewStart.Should().BeLessOrEqualTo(1.0);
        }

        // --- FindInsertionIndex ---

        [Fact]
        public void FindInsertionIndex__ReturnsEnd__When__TimeAfterAllShots()
        {
            var t = Create();

            t.FindInsertionIndex(999.0).Should().Be(t.Count);
        }

        [Fact]
        public void FindInsertionIndex__ReturnsZero__When__TimeBeforeFirstMidpoint()
        {
            var t = Create();

            t.FindInsertionIndex(0.5).Should().Be(0);
        }

        // --- Scrub state ---

        [Fact]
        public void IsScrubbing__ReturnsFalse__When__NotScrubbing()
        {
            var t = Create();

            t.IsScrubbing.Should().BeFalse();
        }

        [Fact]
        public void BeginScrub__SetsScrubbingTrue__When__Called()
        {
            var t = Create();
            t.BeginScrub();

            t.IsScrubbing.Should().BeTrue();
        }

        [Fact]
        public void EndScrub__SetsScrubbingFalse__When__Called()
        {
            var t = Create();
            t.BeginScrub();
            t.EndScrub();

            t.IsScrubbing.Should().BeFalse();
        }

        // --- ResolveShot ---

        [Fact]
        public void ResolveShot__ReturnsFirstShot__When__PlayheadAtZero()
        {
            var t      = Create();
            var result = t.ResolveShot();

            result.Should().NotBeNull();
            result.Value.shot.Should().Be(t.Shots[0]);
            result.Value.localTime.Seconds.Should().Be(0);
        }

        // --- Observable emissions ---

        [Fact]
        public void RemoveShot__FiresShotRemoved__When__ShotRemoved()
        {
            var t    = Create();
            var shot = t.Shots[0];
            Shot removed = null;
            t.ShotRemoved.Subscribe(s => removed = s);

            t.RemoveShot(shot.Id);

            removed.Should().Be(shot);
        }

        [Fact]
        public void RemoveShot__FiresCurrentShotChanged__When__CurrentShotRemoved()
        {
            var t = Create();
            var currentBefore = t.CurrentShot;
            Shot newCurrent = null;
            t.CurrentShotChanged.Subscribe(s => newCurrent = s);

            t.RemoveShot(currentBefore.Id);

            // Should have selected a new current
            newCurrent.Should().NotBeNull();
        }

        [Fact]
        public void RemoveShot__SelectsLastShot__When__LastShotRemovedByIndex()
        {
            var t = Create(); // Shot_01, Shot_02
            // Make Shot_02 current
            t.SetCurrentShot(t.Shots[1].Id);
            var shot2 = t.Shots[1];

            // Remove Shot_02 (index 1, which equals count after removal)
            t.RemoveShot(shot2.Id);

            t.CurrentShot.Should().Be(t.Shots[0]);
        }

        [Fact]
        public void Reorder__FiresReordered__When__ShotMoved()
        {
            var t    = Create();
            var fired = false;
            t.Reordered.Subscribe(_ => fired = true);

            t.Reorder(t.Shots[0].Id, 1);

            fired.Should().BeTrue();
        }

        [Fact]
        public void Reorder__NoOp__When__SameIndex()
        {
            var t       = Create();
            var ordered = t.Shots.ToList();
            var fired   = false;
            t.Reordered.Subscribe(_ => fired = true);

            t.Reorder(t.Shots[0].Id, 0);

            fired.Should().BeFalse();
            t.Shots[0].Should().Be(ordered[0]);
        }

        // --- FitToShot ---

        [Fact]
        public void FitToShot__PinsToStart__When__FirstShot()
        {
            var t = Create(); // 2 shots
            t.AddShot(Vector3.Zero, Quaternion.Identity); // 3rd shot
            t.InitializeViewRange(500);

            t.FitToShot(t.Shots[0].Id);

            t.ViewStart.Should().Be(0);
        }

        [Fact]
        public void FitToShot__PinsToEnd__When__LastShot()
        {
            var t = Create();
            t.AddShot(Vector3.Zero, Quaternion.Identity); // 3rd shot
            t.InitializeViewRange(500);
            var lastShot = t.Shots[t.Count - 1];

            t.FitToShot(lastShot.Id);

            t.ViewEnd.Should().BeApproximately(t.TotalDuration, 0.01);
        }

        // --- ViewRange through Timeline ---

        [Fact]
        public void ZoomAtPoint__NarrowsVisibleDuration__When__PositiveScroll()
        {
            var t = Create();
            t.InitializeViewRange(500);
            var before = t.VisibleDuration;

            t.ZoomAtPoint(5.0, 1f);

            t.VisibleDuration.Should().BeLessThan(before);
        }

        [Fact]
        public void ZoomAtPoint__WidensVisibleDuration__When__NegativeScroll()
        {
            var t = Create();
            t.InitializeViewRange(500);
            t.ZoomAtPoint(5.0, 1f); // zoom in first
            var before = t.VisibleDuration;

            t.ZoomAtPoint(5.0, -1f);

            t.VisibleDuration.Should().BeGreaterThan(before);
        }

        [Fact]
        public void Pan__ShiftsViewRange__When__PositiveDelta()
        {
            var t = Create();
            t.InitializeViewRange(500);
            t.ZoomAtPoint(5.0, 1f); // zoom in so there's room to pan
            var startBefore = t.ViewStart;

            t.Pan(50);

            t.ViewStart.Should().BeGreaterThan(startBefore);
        }

        [Fact]
        public void Pan__ClampsAtZero__When__PanningLeft()
        {
            var t = Create();
            t.InitializeViewRange(500);

            t.Pan(-9999);

            t.ViewStart.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public void ScrubToPixel__FiresCameraEvaluation__When__ShotExists()
        {
            var t    = Create();
            t.InitializeViewRange(500);
            var fired = false;
            t.CameraEvaluationRequested.Subscribe(_ => fired = true);

            t.ScrubToPixel(100);

            fired.Should().BeTrue();
        }

        // --- TogglePlayback at end ---

        [Fact]
        public void TogglePlayback__ResetsViewToStart__When__AtEnd()
        {
            var t = Create();
            t.InitializeViewRange(500);
            t.SetViewRange(0, 3.0);
            // Move playhead to end
            t.TogglePlayback();
            t.Advance(999);

            // Toggle again — should restart from 0
            t.TogglePlayback();

            t.Playhead.CurrentTime.Should().Be(0);
            t.ViewStart.Should().Be(0);
        }

        // --- FindEdgeAtTime boundary ---

        [Fact]
        public void FindEdgeAtTime__ReturnsNegative__When__TimeBeforeAllShots()
        {
            var t = Create();

            t.FindEdgeAtTime(-5.0, 0.1).Should().BeLessThan(0);
        }

        [Fact]
        public void FindEdgeAtTime__ReturnsNegative__When__TimeFarFromEdge()
        {
            var t = Create();

            t.FindEdgeAtTime(2.5, 0.1).Should().BeLessThan(0);
        }

        [Fact]
        public void FindEdgeAtTime__ReturnsIndex__When__ExactlyOnEdge()
        {
            var t = Create();

            t.FindEdgeAtTime(5.0, 0.0).Should().Be(0);
        }

        // --- ViewRange precise assertions ---

        [Fact]
        public void ZoomAtPoint__PreservesAnchorPosition__When__ZoomingIn()
        {
            var t = Create();
            t.InitializeViewRange(500);
            var anchorTime = 5.0;
            var pxBefore   = t.TimeToPixel(anchorTime);

            t.ZoomAtPoint(anchorTime, 1f);

            // The anchor point should stay at roughly the same pixel position
            var pxAfter = t.TimeToPixel(anchorTime);
            pxAfter.Should().BeApproximately(pxBefore, 2.0);
        }

        [Fact]
        public void Pan__MovesViewByCorrectAmount__When__Called()
        {
            var t = Create();
            t.InitializeViewRange(500);
            t.SetViewRange(0, 5.0); // 5s visible in 500px = 100px/s
            var startBefore = t.ViewStart;

            t.Pan(100); // 100px = 1s at 100px/s

            (t.ViewStart - startBefore).Should().BeApproximately(1.0, 0.1);
        }

        [Fact]
        public void TimeToPixel__RoundTrips__When__ConvertingBackAndForth()
        {
            var t = Create();
            t.InitializeViewRange(500);
            var originalTime = 3.0;

            var px    = t.TimeToPixel(originalTime);
            var back  = t.PixelToTime(px);

            back.Should().BeApproximately(originalTime, 0.001);
        }

        [Fact]
        public void EnsureVisible__DoesNotMove__When__TimeAlreadyVisible()
        {
            var t = Create();
            t.InitializeViewRange(500);
            var startBefore = t.ViewStart;
            var endBefore   = t.ViewEnd;

            t.EnsureVisible(5.0); // middle of the visible range

            t.ViewStart.Should().Be(startBefore);
            t.ViewEnd.Should().Be(endBefore);
        }

        [Fact]
        public void SetViewRange__ClampsEnd__When__PastTotalDuration()
        {
            var t = Create();
            t.InitializeViewRange(500);

            t.SetViewRange(5, 999);

            t.ViewEnd.Should().BeLessOrEqualTo(t.TotalDuration);
        }

        [Fact]
        public void SetViewRange__ClampsStart__When__Negative()
        {
            var t = Create();
            t.InitializeViewRange(500);

            t.SetViewRange(-10, 5);

            t.ViewStart.Should().BeGreaterThanOrEqualTo(0);
        }

        // --- Observables ---

        [Fact]
        public void ViewChanged__Fires__When__ZoomApplied()
        {
            var t    = Create();
            t.InitializeViewRange(500);
            var fired = false;
            t.ViewChanged.Subscribe(_ => fired = true);

            t.ZoomAtPoint(5.0, 1f);

            fired.Should().BeTrue();
        }

        [Fact]
        public void ViewChanged__Fires__When__Panned()
        {
            var t = Create();
            t.InitializeViewRange(500);
            t.ZoomAtPoint(5.0, 1f); // zoom in first
            var fired = false;
            t.ViewChanged.Subscribe(_ => fired = true);

            t.Pan(50);

            fired.Should().BeTrue();
        }

        [Fact]
        public void ShotAdded__Fires__When__ShotAdded()
        {
            var t    = Create();
            Shot added = null;
            t.ShotAdded.Subscribe(s => added = s);

            var shot = t.AddShot(Vector3.Zero, Quaternion.Identity);

            added.Should().Be(shot);
        }

        // --- ShotTrack GetShotAtGlobalTime ---

        [Fact]
        public void GetShotAtGlobalTime__ReturnsNull__When__TimePastAllShots()
        {
            var t = Create();

            var result = t.GetShotAtGlobalTime(new TimePosition(999.0));

            result.Should().BeNull();
        }

        [Fact]
        public void GetShotAtGlobalTime__ReturnsLastShot__When__ExactlyAtEnd()
        {
            var t = Create();

            var result = t.GetShotAtGlobalTime(new TimePosition(t.TotalDuration));

            result.Should().NotBeNull();
            result.Value.shot.Should().Be(t.Shots[t.Count - 1]);
        }

        [Fact]
        public void GetShotAtGlobalTime__ReturnsSecondShot__When__TimeInSecondShot()
        {
            var t = Create(); // Shot_01 (5s), Shot_02 (5s)

            var result = t.GetShotAtGlobalTime(new TimePosition(7.0));

            result.Should().NotBeNull();
            result.Value.shot.Should().Be(t.Shots[1]);
            result.Value.localTime.Seconds.Should().BeApproximately(2.0, 0.001);
        }

        // --- Element evaluation ---

        [Fact]
        public void Advance__FiresElementEvaluation__When__Playing()
        {
            var t    = Create();
            TimePosition globalTime = null;
            t.ElementEvaluationRequested.Subscribe(eval => globalTime = eval.GlobalTime);
            t.TogglePlayback();

            t.Advance(0.1);

            globalTime.Should().NotBeNull();
            globalTime.Seconds.Should().BeApproximately(0.1, 0.01);
        }

        [Fact]
        public void ScrubToPixel__FiresElementEvaluation__When__Called()
        {
            var t    = Create();
            t.InitializeViewRange(500);
            var fired = false;
            t.ElementEvaluationRequested.Subscribe(_ => fired = true);

            t.ScrubToPixel(100);

            fired.Should().BeTrue();
        }

        [Fact]
        public void Elements__IsAccessible__When__TimelineCreated()
        {
            var t = Create();

            t.Elements.Should().NotBeNull();
            t.Elements.TrackCount.Should().Be(0);
        }

        // --- JumpToStart / JumpToEnd ---

        [Fact]
        public void JumpToStart__MovesPlayheadToZero__When__Called()
        {
            var t = Create();
            t.Playhead.Scrub(3.0, t.TotalDuration);

            t.JumpToStart();

            t.Playhead.CurrentTime.Should().Be(0);
        }

        [Fact]
        public void JumpToStart__FiresCameraEvaluation__When__Called()
        {
            var t     = Create();
            var fired = false;
            t.CameraEvaluationRequested.Subscribe(_ => fired = true);
            t.Playhead.Scrub(3.0, t.TotalDuration);

            t.JumpToStart();

            fired.Should().BeTrue();
        }

        [Fact]
        public void JumpToEnd__MovesPlayheadToEnd__When__Called()
        {
            var t = Create();

            t.JumpToEnd();

            t.Playhead.CurrentTime.Should().BeApproximately(t.TotalDuration, 0.001);
        }

        [Fact]
        public void JumpToEnd__FiresCameraEvaluation__When__Called()
        {
            var t     = Create();
            var fired = false;
            t.CameraEvaluationRequested.Subscribe(_ => fired = true);

            t.JumpToEnd();

            fired.Should().BeTrue();
        }

        // --- ResizeShotAtEdge ---

        [Fact]
        public void ResizeShotAtEdge__EnforcesMinDuration__When__ResizedToZero()
        {
            var t = Create();

            var snapped = t.ResizeShotAtEdge(0, 0.0);

            snapped.Should().BeGreaterThan(0);
        }

        // --- Advance stops correctly ---

        [Fact]
        public void Advance__ReturnsFalse__When__NotPlaying()
        {
            var t = Create();

            t.Advance(1.0).Should().BeFalse();
        }

        [Fact]
        public void Advance__ReturnsTrue__When__StillPlaying()
        {
            var t = Create();
            t.TogglePlayback();

            t.Advance(0.1).Should().BeTrue();
        }

        // --- Track view state ---

        [Fact]
        public void Selection__IsExposed__When__Accessed()
        {
            var tl = new Timeline(FrameRate.FPS_24);
            tl.Selection.Should().NotBeNull();
            tl.Selection.HasSelection.Should().BeFalse();
        }

        [Fact]
        public void Expansion__IsExposed__When__Accessed()
        {
            var tl = new Timeline(FrameRate.FPS_24);
            tl.Expansion.Should().NotBeNull();
            tl.Expansion.IsExpanded(TrackId.Camera).Should().BeFalse();
        }

        [Fact]
        public void SelectKeyframe__MovesPlayhead__When__Called()
        {
            var tl = new Timeline(FrameRate.FPS_24);
            tl.AddShot(Vector3.Zero, Quaternion.Identity);
            var kfId = tl.CurrentShot.CameraPositionKeyframes.Keyframes[0].Id;
            tl.SelectKeyframe(TrackId.Camera, kfId, new TimePosition(2.0));
            tl.Playhead.CurrentTime.Should().BeApproximately(2.0, 0.05);
            tl.Selection.IsSelected(kfId).Should().BeTrue();
        }

        [Fact]
        public void SelectKeyframe__FiresCameraEvaluation__When__Called()
        {
            var tl = new Timeline(FrameRate.FPS_24);
            tl.AddShot(Vector3.Zero, Quaternion.Identity);
            var fired = false;
            tl.CameraEvaluationRequested.Subscribe(_ => fired = true);
            var kfId = tl.CurrentShot.CameraPositionKeyframes.Keyframes[0].Id;
            tl.SelectKeyframe(TrackId.Camera, kfId, new TimePosition(1.0));
            fired.Should().BeTrue();
        }

        [Fact]
        public void SelectKeyframe__SetsTrackId__When__Called()
        {
            var tl = new Timeline(FrameRate.FPS_24);
            tl.AddShot(Vector3.Zero, Quaternion.Identity);
            var kfId = tl.CurrentShot.CameraPositionKeyframes.Keyframes[0].Id;
            tl.SelectKeyframe(TrackId.Camera, kfId, new TimePosition(1.0));
            tl.Selection.TrackId.Should().Be(TrackId.Camera);
        }

        // ── Recording API ────────────────────────────────────────────────

        [Fact]
        public void RecordCameraManipulation__DoesNotRecord__When__Playing()
        {
            var timeline = Create();
            var shot     = timeline.CurrentShot;
            shot.CameraStopwatch.SetAll(true);
            // Scrub away from t=0 so that if the guard fails, a new keyframe would be created
            timeline.Playhead.Scrub(2.0, timeline.TotalDuration);
            timeline.TogglePlayback();

            var before = new CameraSnapshot { Position = Vector3.Zero };
            var after  = new CameraSnapshot { Position = new Vector3(10, 0, 0) };

            timeline.RecordCameraManipulation(after, before);

            // Only the initial mandatory keyframe at t=0 should exist
            shot.CameraPositionKeyframes.Count.Should().Be(1);
        }

        [Fact]
        public void RecordCameraManipulation__RecordsKeyframe__When__StopwatchOnAndChanged()
        {
            var timeline = Create();
            timeline.CurrentShot.CameraStopwatch.SetAll(true);
            // Scrub into the shot so local time > 0 (avoids merge with t=0 keyframe)
            timeline.Playhead.Scrub(2.0, timeline.TotalDuration);

            var before = new CameraSnapshot { Position = Vector3.Zero };
            var after  = new CameraSnapshot { Position = new Vector3(10, 0, 0) };

            timeline.RecordCameraManipulation(after, before);

            // Should have the initial keyframe at t=0 plus a new one
            timeline.CurrentShot.CameraPositionKeyframes.Count.Should().Be(2);
        }

        [Fact]
        public void RecordCameraManipulation__DoesNotRecord__When__StopwatchOff()
        {
            var timeline = Create();
            // Stopwatch is off by default
            timeline.Playhead.Scrub(2.0, timeline.TotalDuration);

            var before = new CameraSnapshot { Position = Vector3.Zero };
            var after  = new CameraSnapshot { Position = new Vector3(10, 0, 0) };

            timeline.RecordCameraManipulation(after, before);

            // Only the initial mandatory keyframe should exist
            timeline.CurrentShot.CameraPositionKeyframes.Count.Should().Be(1);
        }

        [Fact]
        public void RecordElementManipulation__DoesNotRecord__When__Playing()
        {
            var timeline  = Create();
            var elementId = new ElementId(Guid.NewGuid());
            var track     = timeline.Elements.GetOrCreateTrack(elementId);
            track.Stopwatch.SetAll(true);
            // Scrub away from t=0 before toggling playback
            timeline.Playhead.Scrub(2.0, timeline.TotalDuration);
            timeline.TogglePlayback();

            var before = new ElementSnapshot { Position = Vector3.Zero };
            var after  = new ElementSnapshot { Position = new Vector3(10, 0, 0) };

            timeline.RecordElementManipulation(elementId, after, before);

            track.PositionKeyframes.Count.Should().Be(0);
        }

        [Fact]
        public void RecordElementManipulation__RecordsKeyframe__When__StopwatchOnAndChanged()
        {
            var timeline  = Create();
            var elementId = new ElementId(Guid.NewGuid());
            var track     = timeline.Elements.GetOrCreateTrack(elementId);
            track.Stopwatch.SetAll(true);

            var before = new ElementSnapshot { Position = Vector3.Zero };
            var after  = new ElementSnapshot { Position = new Vector3(10, 0, 0) };

            timeline.RecordElementManipulation(elementId, after, before);

            track.PositionKeyframes.Count.Should().Be(1);
        }

        [Fact]
        public void ForceRecordCamera__RecordsAll__When__NotPlaying()
        {
            var timeline = Create();
            // Scrub into the shot so local time > 0 (avoids merge with t=0 keyframe)
            timeline.Playhead.Scrub(2.0, timeline.TotalDuration);

            var snap = new CameraSnapshot
            {
                Position      = new Vector3(1, 2, 3),
                Rotation      = Quaternion.Identity,
                FocalLength   = 50f,
                FocusDistance  = 3f,
                Aperture      = 2.8f
            };

            timeline.ForceRecordCamera(snap);

            timeline.CurrentShot.CameraPositionKeyframes.Count.Should().Be(2);
            timeline.CurrentShot.CameraRotationKeyframes.Count.Should().Be(2);
            timeline.CurrentShot.CameraFocalLengthKeyframes.Count.Should().Be(1);
            timeline.CurrentShot.CameraFocusDistanceKeyframes.Count.Should().Be(1);
            timeline.CurrentShot.CameraApertureKeyframes.Count.Should().Be(1);
        }

        [Fact]
        public void ForceRecordCamera__DoesNotRecord__When__Playing()
        {
            var timeline = Create();
            var shot     = timeline.CurrentShot;
            // Scrub away from t=0 before toggling playback
            timeline.Playhead.Scrub(2.0, timeline.TotalDuration);
            timeline.TogglePlayback();

            var snap = new CameraSnapshot
            {
                Position      = new Vector3(1, 2, 3),
                Rotation      = Quaternion.Identity,
                FocalLength   = 50f,
                FocusDistance  = 3f,
                Aperture      = 2.8f
            };

            timeline.ForceRecordCamera(snap);

            // No new keyframes should have been created
            shot.CameraPositionKeyframes.Count.Should().Be(1);
            shot.CameraRotationKeyframes.Count.Should().Be(1);
            shot.CameraFocalLengthKeyframes.Count.Should().Be(0);
            shot.CameraFocusDistanceKeyframes.Count.Should().Be(0);
            shot.CameraApertureKeyframes.Count.Should().Be(0);
        }
        // --- Integration: auto-record on stopwatch enable ---

        [Fact]
        public void ForceRecordCamera__MergesAtTimeZero__When__EnabledImmediately()
        {
            var timeline = Create();
            var shot     = timeline.CurrentShot;

            // Simulate: user enables stopwatch and we force-record at t=0
            shot.CameraStopwatch.SetAll(true);
            var snap = new CameraSnapshot
            {
                Position     = new Vector3(5f, 2f, -3f),
                Rotation     = Quaternion.Identity,
                FocalLength  = 35f,
                FocusDistance = 2f,
                Aperture     = 4f
            };

            timeline.ForceRecordCamera(snap);

            // Should merge with initial keyframes at t=0, not duplicate
            shot.CameraPositionKeyframes.Count.Should().Be(1);
            shot.CameraPositionKeyframes.Keyframes[0].Value.Should().Be(new Vector3(5f, 2f, -3f));
            shot.CameraFocalLengthKeyframes.Count.Should().Be(1);
        }

        [Fact]
        public void ForceRecordCamera__CreatesNewKeyframe__When__PlayheadAwayFromZero()
        {
            var timeline = Create();
            var shot     = timeline.CurrentShot;
            shot.CameraStopwatch.SetAll(true);
            timeline.Playhead.Scrub(2.5, timeline.TotalDuration);

            var snap = new CameraSnapshot
            {
                Position     = new Vector3(1f, 1f, 1f),
                Rotation     = Quaternion.Identity,
                FocalLength  = 50f,
                FocusDistance = 5f,
                Aperture     = 2.8f
            };

            timeline.ForceRecordCamera(snap);

            // Initial at t=0 + new at t=2.5
            shot.CameraPositionKeyframes.Count.Should().Be(2);
            shot.CameraRotationKeyframes.Count.Should().Be(2);
        }

        // --- Integration: stopwatch off clears keyframes but preserves state ---

        [Fact]
        public void ClearAllCameraKeyframes__LeavesStopwatchIntact__When__TurningOff()
        {
            var timeline = Create();
            var shot     = timeline.CurrentShot;
            shot.CameraStopwatch.SetAll(true);

            // Record a keyframe
            timeline.Playhead.Scrub(2.0, timeline.TotalDuration);
            var before = new CameraSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity };
            var after  = new CameraSnapshot { Position = new Vector3(5f, 0f, 0f), Rotation = Quaternion.Identity };
            timeline.RecordCameraManipulation(after, before);
            shot.CameraPositionKeyframes.Count.Should().Be(2);

            // Simulate turn-off: clear keyframes then disable stopwatch
            shot.ClearAllCameraKeyframes();
            shot.CameraStopwatch.SetAll(false);

            shot.CameraPositionKeyframes.Count.Should().Be(0);
            shot.CameraRotationKeyframes.Count.Should().Be(0);
            shot.CameraStopwatch.AnyRecording.Should().BeFalse();
        }

        // --- Integration: recording does not happen during scrub ---

        [Fact]
        public void RecordCameraManipulation__DoesNotRecord__When__ScrubCausedPositionChange()
        {
            var timeline = Create();
            var shot     = timeline.CurrentShot;
            shot.CameraStopwatch.SetAll(true);

            // Add a second keyframe at t=3 with different position
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(
                    new KeyframeId(Guid.NewGuid()),
                    new TimePosition(3.0),
                    new Vector3(10f, 0f, 0f)));

            // Scrub to t=1.5 — position interpolates to ~5,0,0
            timeline.Playhead.Scrub(1.5, timeline.TotalDuration);
            var interpolated = shot.EvaluateCameraPosition(new TimePosition(1.5));

            // The "before" and "after" are the same because scrub evaluates, not user
            // The user hasn't manually moved anything — before and after match
            var snap = new CameraSnapshot { Position = interpolated, Rotation = Quaternion.Identity };
            timeline.RecordCameraManipulation(snap, snap);

            // No new keyframe — change is zero
            shot.CameraPositionKeyframes.Count.Should().Be(2);
        }

        // --- Integration: multiple sequential recordings merge within window ---

        [Fact]
        public void RecordCameraManipulation__MergesSequential__When__WithinWindow()
        {
            var timeline = Create();
            var shot     = timeline.CurrentShot;
            shot.CameraStopwatch.SetAll(true);
            timeline.Playhead.Scrub(2.0, timeline.TotalDuration);

            var before  = new CameraSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity };
            var after1  = new CameraSnapshot { Position = new Vector3(1f, 0f, 0f), Rotation = Quaternion.Identity };
            timeline.RecordCameraManipulation(after1, before);

            // Second manipulation at t=2.05 (within 0.1s merge window)
            timeline.Playhead.Scrub(2.05, timeline.TotalDuration);
            var after2 = new CameraSnapshot { Position = new Vector3(3f, 0f, 0f), Rotation = Quaternion.Identity };
            timeline.RecordCameraManipulation(after2, before);

            // Should still be initial + 1 merged, not initial + 2
            shot.CameraPositionKeyframes.Count.Should().Be(2);
            shot.CameraPositionKeyframes.Keyframes[1].Value.X.Should().Be(3f);
        }

        // --- Integration: element recording creates track ---

        [Fact]
        public void RecordElementManipulation__CreatesTrack__When__FirstKeyframe()
        {
            var timeline  = Create();
            var elementId = new ElementId(Guid.NewGuid());

            // Element track doesn't exist yet
            timeline.Elements.HasTrack(elementId).Should().BeFalse();

            // Enable stopwatch on the track (GetOrCreateTrack creates it)
            var track = timeline.Elements.GetOrCreateTrack(elementId);
            track.Stopwatch.SetAll(true);

            var before = new ElementSnapshot { Position = Vector3.Zero, Rotation = Quaternion.Identity, Scale = 1f };
            var after  = new ElementSnapshot { Position = new Vector3(2f, 0f, 0f), Rotation = Quaternion.Identity, Scale = 1f };
            timeline.RecordElementManipulation(elementId, after, before);

            track.PositionKeyframes.Count.Should().Be(1);
            track.HasKeyframes.Should().BeTrue();
        }

        [Fact]
        public void RecordCameraManipulation__DoesNotRecord__When__NoShotsExist()
        {
            var timeline = new Timeline(FrameRate.FPS_24);
            // No AddShot — CurrentShot is null
            var before = new CameraSnapshot { Position = Vector3.Zero };
            var after  = new CameraSnapshot { Position = new Vector3(5f, 0f, 0f) };

            // Should not throw, just no-op
            timeline.RecordCameraManipulation(after, before);
        }

        [Fact]
        public void ForceRecordElement__DoesNotRecord__When__Playing()
        {
            var timeline  = Create();
            var elementId = new ElementId(Guid.NewGuid());
            var track     = timeline.Elements.GetOrCreateTrack(elementId);
            track.Stopwatch.SetAll(true);
            timeline.Playhead.Scrub(2.0, timeline.TotalDuration);
            timeline.TogglePlayback();

            var snap = new ElementSnapshot { Position = new Vector3(1f, 0f, 0f), Rotation = Quaternion.Identity, Scale = 1f };
            timeline.ForceRecordElement(elementId, snap);

            track.PositionKeyframes.Count.Should().Be(0);
        }

        // ── DeleteSelectedKeyframe ─────────────────────────────────────

        [Fact]
        public void DeleteSelectedKeyframe__ReturnsFalse__When__NoSelection()
        {
            var t = Create();

            t.DeleteSelectedKeyframe().Should().BeFalse();
        }

        [Fact]
        public void DeleteSelectedKeyframe__DeletesCameraKeyframe__When__MultipleExist()
        {
            var t    = Create(1);
            var shot = t.CurrentShot;
            var t1   = new TimePosition(1.0);
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), t1, Vector3.One));
            shot.CameraRotationKeyframes.Add(
                new Keyframe<Quaternion>(new KeyframeId(Guid.NewGuid()), t1, Quaternion.Identity));

            var kfId = shot.CameraPositionKeyframes.Keyframes[1].Id;
            t.SelectKeyframe(TrackId.Camera, kfId, t1);

            t.DeleteSelectedKeyframe().Should().BeTrue();

            shot.CameraPositionKeyframes.Count.Should().Be(1); // only t=0 remains
            shot.CameraRotationKeyframes.Count.Should().Be(1);
            t.Selection.HasSelection.Should().BeFalse();
        }

        [Fact]
        public void DeleteSelectedKeyframe__ReturnsFalse__When__LastCameraKeyframe()
        {
            var t    = Create(1);
            var shot = t.CurrentShot;
            var kfId = shot.CameraPositionKeyframes.Keyframes[0].Id;
            t.SelectKeyframe(TrackId.Camera, kfId, TimePosition.ZERO);

            t.DeleteSelectedKeyframe().Should().BeFalse();

            shot.CameraPositionKeyframes.Count.Should().Be(1); // still there
        }

        [Fact]
        public void DeleteSelectedKeyframe__DeletesElementKeyframe__When__Selected()
        {
            var t         = Create(1);
            var elementId = new ElementId(Guid.NewGuid());
            var track     = t.Elements.GetOrCreateTrack(elementId);
            var t0        = TimePosition.ZERO;
            track.PositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), t0, Vector3.One));
            var kfId    = track.PositionKeyframes.Keyframes[0].Id;
            var trackId = TrackId.ForElement(elementId);
            t.SelectKeyframe(trackId, kfId, t0);

            t.DeleteSelectedKeyframe().Should().BeTrue();

            track.PositionKeyframes.Count.Should().Be(0);
        }

        // ── MoveSelectedKeyframe ───────────────────────────────────────

        [Fact]
        public void MoveSelectedKeyframe__ReturnsFalse__When__NoSelection()
        {
            var t = Create();

            t.MoveSelectedKeyframe(1.0).Should().BeFalse();
        }

        [Fact]
        public void MoveSelectedKeyframe__SnapsTo01Grid__When__Called()
        {
            var t    = Create(1);
            var shot = t.CurrentShot;
            var t1   = new TimePosition(1.0);
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), t1, Vector3.One));
            var kfId = shot.CameraPositionKeyframes.Keyframes[1].Id;
            t.SelectKeyframe(TrackId.Camera, kfId, t1);

            t.MoveSelectedKeyframe(2.37).Should().BeTrue();

            shot.CameraPositionKeyframes.Keyframes[1].Time.Seconds.Should().BeApproximately(2.4, 0.001);
        }

        [Fact]
        public void MoveSelectedKeyframe__ClampsToZero__When__NegativeTime()
        {
            var t    = Create(1);
            var shot = t.CurrentShot;
            var t1   = new TimePosition(1.0);
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), t1, Vector3.One));
            var kfId = shot.CameraPositionKeyframes.Keyframes[1].Id;
            t.SelectKeyframe(TrackId.Camera, kfId, t1);

            t.MoveSelectedKeyframe(-5.0).Should().BeTrue();

            shot.CameraPositionKeyframes.Keyframes[0].Time.Seconds.Should().Be(0.0);
        }

        [Fact]
        public void MoveSelectedKeyframe__ClampsToDuration__When__BeyondShotEnd()
        {
            var t    = Create(1);
            var shot = t.CurrentShot;
            var t1   = new TimePosition(1.0);
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), t1, Vector3.One));
            var kfId = shot.CameraPositionKeyframes.Keyframes[1].Id;
            t.SelectKeyframe(TrackId.Camera, kfId, t1);

            t.MoveSelectedKeyframe(999.0).Should().BeTrue();

            shot.CameraPositionKeyframes.Keyframes[1].Time.Seconds.Should().Be(shot.Duration);
        }

        [Fact]
        public void MoveSelectedKeyframe__UpdatesSelection__When__Moved()
        {
            var t    = Create(1);
            var shot = t.CurrentShot;
            var t1   = new TimePosition(1.0);
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), t1, Vector3.One));
            var kfId = shot.CameraPositionKeyframes.Keyframes[1].Id;
            t.SelectKeyframe(TrackId.Camera, kfId, t1);

            t.MoveSelectedKeyframe(3.0);

            t.Selection.Time.Seconds.Should().BeApproximately(3.0, 0.001);
        }

        [Fact]
        public void MoveSelectedKeyframe__ReturnsFalse__When__SameTimeAfterSnap()
        {
            var t    = Create(1);
            var shot = t.CurrentShot;
            var t1   = new TimePosition(1.0);
            shot.CameraPositionKeyframes.Add(
                new Keyframe<Vector3>(new KeyframeId(Guid.NewGuid()), t1, Vector3.One));
            var kfId = shot.CameraPositionKeyframes.Keyframes[1].Id;
            t.SelectKeyframe(TrackId.Camera, kfId, t1);

            // 1.04 snaps to 1.0 — same as current
            t.MoveSelectedKeyframe(1.04).Should().BeFalse();
        }
    }
}
