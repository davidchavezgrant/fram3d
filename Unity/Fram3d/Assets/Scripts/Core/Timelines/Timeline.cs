using System;
using System.Collections.Generic;
using System.Numerics;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// THE timeline. Orchestrates ShotTrack, Playhead, and ViewRange.
    /// Owns interaction state machines (scrub, boundary drag, shot drag reorder)
    /// and playback. Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class Timeline
    {
        private const double DOUBLE_CLICK_MS   = 350;
        private const double EDGE_TOLERANCE_PX = 6.0;
        private const int    HOLD_THRESHOLD_MS = 200;

        private readonly Subject<CameraEvaluation> _cameraEvaluationRequested = new();
        private readonly ViewRange _view;

        // ── Interaction state ──
        private int    _boundaryDragIndex = -1;
        private ShotId _dragShotId;
        private int    _dragOriginalIndex;
        private int    _dragTargetIndex;
        private bool   _isBoundaryDragging;
        private bool   _isDragging;
        private bool   _isScrubbing;
        private ShotId _lastClickShotId;
        private long   _lastClickTime;
        private double _pointerDownX;
        private long   _pointerDownTime;
        private bool   _pointerIsDown;

        public Timeline(FrameRate frameRate)
        {
            this.Playhead = new Playhead(frameRate);
            this.Track    = new ShotTrack(frameRate);
            this._view    = new ViewRange();
        }

        // ══════════════════════════════════════════════════════════════════
        // Sub-components
        // ══════════════════════════════════════════════════════════════════

        public Playhead Playhead { get; }

        private ShotTrack Track { get; }

        // ══════════════════════════════════════════════════════════════════
        // Delegated shot properties
        // ══════════════════════════════════════════════════════════════════

        public int                  Count              => this.Track.Count;
        public Shot                 CurrentShot        => this.Track.CurrentShot;
        public FrameRate            FrameRate          => this.Track.FrameRate;
        public IReadOnlyList<Shot>  Shots              => this.Track.Shots;
        public double               TotalDuration      => this.Track.TotalDuration;

        public IObservable<Shot>    CurrentShotChanged => this.Track.CurrentShotChanged;
        public IObservable<bool>    Reordered          => this.Track.Reordered;
        public IObservable<Shot>    ShotAdded          => this.Track.ShotAdded;
        public IObservable<Shot>    ShotRemoved        => this.Track.ShotRemoved;

        public int              FindEdgeAtTime(double time, double tolerance) => this.Track.FindEdgeAtTime(time, tolerance);
        public int              FindInsertionIndex(double time)               => this.Track.FindInsertionIndex(time);
        public string           FormatResizeTooltip(int index, bool shift)    => this.Track.FormatResizeTooltip(index, shift);
        public string           FormatShotTooltip(Shot shot)                  => this.Track.FormatShotTooltip(shot);
        public Shot             GetById(ShotId id)                            => this.Track.GetById(id);
        public TimePosition     GetGlobalEndTime(ShotId id)                   => this.Track.GetGlobalEndTime(id);
        public TimePosition     GetGlobalStartTime(ShotId id)                 => this.Track.GetGlobalStartTime(id);
        public (Shot shot, TimePosition localTime)? GetShotAtGlobalTime(TimePosition t) => this.Track.GetShotAtGlobalTime(t);
        public int              IndexOf(ShotId id)                            => this.Track.IndexOf(id);
        public bool             RemoveShot(ShotId id)                         => this.Track.RemoveShot(id);
        public void             Reorder(ShotId id, int newIndex)              => this.Track.Reorder(id, newIndex);
        public double           ResizeShotAtEdge(int index, double endTime)   => this.Track.ResizeShotAtEdge(index, endTime);
        public void             SetCurrentShot(ShotId id)                     => this.Track.SetCurrentShot(id);

        // ══════════════════════════════════════════════════════════════════
        // Delegated view range properties
        // ══════════════════════════════════════════════════════════════════

        public IObservable<bool> ViewChanged        => this._view.Changed;
        public double            PlayheadPixel      => this._view.TimeToPixel(this.Playhead.CurrentTime);
        public double            OutOfRangeStartPixel => this._view.TimeToPixel(this.TotalDuration);
        public double            PixelsPerSecond    => this._view.PixelsPerSecond;
        public double            ViewEnd            => this._view.ViewEnd;
        public double            ViewStart          => this._view.ViewStart;
        public double            VisibleDuration    => this._view.VisibleDuration;

        public double TimeToPixel(double seconds) => this._view.TimeToPixel(seconds);
        public double PixelToTime(double px)       => this._view.PixelToTime(px);

        public void InitializeViewRange(double stripWidth) =>
            this._view.Initialize(stripWidth, this.TotalDuration);

        public void ZoomAtPoint(double anchorSeconds, float scrollDelta) =>
            this._view.ZoomAtPoint(anchorSeconds, scrollDelta, this.TotalDuration);

        public void Pan(double deltaPx) =>
            this._view.Pan(deltaPx, this.TotalDuration);

        public void FitAll() =>
            this._view.FitAll(this.TotalDuration);

        public void FitRange(double start, double end) =>
            this._view.FitRange(start, end);

        public void SetViewRange(double start, double end) =>
            this._view.SetRange(start, end, this.TotalDuration);

        public void EnsureVisible(double seconds) =>
            this._view.EnsureVisible(seconds);

        // ══════════════════════════════════════════════════════════════════
        // Shot lifecycle
        // ══════════════════════════════════════════════════════════════════

        public Shot AddShot(Vector3 cameraPosition, Quaternion cameraRotation)
        {
            var shot = this.Track.AddShot(cameraPosition, cameraRotation);

            if (this._view.IsInitialized)
            {
                this.FitAll();
            }

            return shot;
        }

        public void FitToShot(ShotId shotId)
        {
            var shotIndex = this.Track.IndexOf(shotId);
            var start     = this.Track.GetGlobalStartTime(shotId).Seconds;
            var end       = this.Track.GetGlobalEndTime(shotId).Seconds;
            var duration  = end - start;
            var padding   = duration * 0.08;

            if (this.Track.Count == 1)
            {
                this.FitAll();
            }
            else if (shotIndex == 0)
            {
                this.SetViewRange(0, end + padding);
            }
            else if (shotIndex == this.Track.Count - 1)
            {
                this.SetViewRange(start - padding, this.TotalDuration);
            }
            else
            {
                this.FitRange(start, end);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // Playback
        // ══════════════════════════════════════════════════════════════════

        public bool TogglePlayback()
        {
            var wasAtEnd     = this.Playhead.CurrentTime >= this.TotalDuration - this.FrameRate.FrameDuration;
            var isNowPlaying = this.Playhead.TogglePlayback(this.TotalDuration);

            if (isNowPlaying && wasAtEnd)
            {
                this.SetViewRange(0, this.VisibleDuration);
            }

            return isNowPlaying;
        }

        public bool Advance(double deltaSeconds)
        {
            var stillPlaying = this.Playhead.Advance(deltaSeconds, this.TotalDuration);

            if (stillPlaying)
            {
                this.EvaluateCamera();

                if (this.Playhead.CurrentTime > this._view.ViewEnd)
                {
                    var duration = this.VisibleDuration;
                    this.SetViewRange(this._view.ViewEnd, this._view.ViewEnd + duration);
                }
            }

            return stillPlaying;
        }

        // ══════════════════════════════════════════════════════════════════
        // Scrub
        // ══════════════════════════════════════════════════════════════════

        public bool IsScrubbing => this._isScrubbing;

        public void BeginScrub() => this._isScrubbing = true;
        public void EndScrub()   => this._isScrubbing = false;

        public void ScrubToPixel(double px)
        {
            var rawTime = this.PixelToTime(px);
            this.Playhead.Scrub(rawTime, this.TotalDuration);
            this.EvaluateCamera();
            this.EnsureVisible(Math.Clamp(rawTime, 0, this.TotalDuration));
        }

        // ══════════════════════════════════════════════════════════════════
        // Shot track interaction state machine
        // ══════════════════════════════════════════════════════════════════

        public int  BoundaryDragIndex  => this._boundaryDragIndex;
        public int  DragTargetIndex    => this._dragTargetIndex;
        public bool IsBoundaryDragging => this._isBoundaryDragging;
        public bool IsDragging         => this._isDragging;

        public ShotTrackAction StripPointerDown(double px, long timestampMs)
        {
            var time      = this.PixelToTime(px);
            var tolerance = this.PixelsPerSecond > 0 ? EDGE_TOLERANCE_PX / this.PixelsPerSecond : 1.0;
            var edgeIndex = this.Track.FindEdgeAtTime(time, tolerance);

            if (edgeIndex >= 0)
            {
                this._isBoundaryDragging = true;
                this._boundaryDragIndex  = edgeIndex;
                return ShotTrackAction.BOUNDARY_DRAG;
            }

            this._pointerDownTime = timestampMs;
            this._pointerDownX    = px;
            this._pointerIsDown   = true;

            var shotIndex = this.FindShotIndexAtTime(time);

            if (shotIndex >= 0)
            {
                this._dragShotId        = this.Track.Shots[shotIndex].Id;
                this._dragOriginalIndex = shotIndex;
            }

            return ShotTrackAction.POTENTIAL_CLICK;
        }

        public ShotTrackAction StripPointerMove(double px, long timestampMs)
        {
            if (this._isBoundaryDragging)
            {
                this.Track.ResizeShotAtEdge(this._boundaryDragIndex, this.PixelToTime(px));
                return ShotTrackAction.BOUNDARY_DRAG;
            }

            if (this._pointerIsDown && this._dragShotId != null && !this._isDragging)
            {
                if (timestampMs - this._pointerDownTime >= HOLD_THRESHOLD_MS
                 || Math.Abs(px - this._pointerDownX) > 5)
                {
                    this._isDragging = true;
                    return ShotTrackAction.DRAG_START;
                }
            }

            if (this._isDragging)
            {
                this._dragTargetIndex = this.Track.FindInsertionIndex(this.PixelToTime(px));
                return ShotTrackAction.DRAG_MOVE;
            }

            if (!this._pointerIsDown)
            {
                var time      = this.PixelToTime(px);
                var tolerance = this.PixelsPerSecond > 0 ? EDGE_TOLERANCE_PX / this.PixelsPerSecond : 1.0;

                if (this.Track.FindEdgeAtTime(time, tolerance) >= 0)
                {
                    return ShotTrackAction.NEAR_EDGE;
                }
            }

            return ShotTrackAction.NONE;
        }

        public ShotTrackAction StripPointerUp()
        {
            if (this._isBoundaryDragging)
            {
                this._isBoundaryDragging = false;
                this._boundaryDragIndex  = -1;
                return ShotTrackAction.BOUNDARY_COMPLETE;
            }

            if (this._isDragging)
            {
                this.CompleteDrag();
                this.ResetPointerState();
                return ShotTrackAction.DRAG_COMPLETE;
            }

            if (this._dragShotId != null)
            {
                this.HandleShotClick(this._dragShotId);
                this.ResetPointerState();
                return ShotTrackAction.CLICK;
            }

            this.ResetPointerState();
            return ShotTrackAction.NONE;
        }

        // ══════════════════════════════════════════════════════════════════
        // Formatting
        // ══════════════════════════════════════════════════════════════════

        public string FormatBoundaryTooltip(bool shiftHeld)
        {
            if (this._boundaryDragIndex < 0 || this._boundaryDragIndex >= this.Track.Count)
            {
                return "";
            }

            return this.Track.FormatResizeTooltip(this._boundaryDragIndex, shiftHeld);
        }

        // ══════════════════════════════════════════════════════════════════
        // Observables
        // ══════════════════════════════════════════════════════════════════

        public IObservable<CameraEvaluation> CameraEvaluationRequested => this._cameraEvaluationRequested;

        // ══════════════════════════════════════════════════════════════════
        // Query
        // ══════════════════════════════════════════════════════════════════

        public (Shot shot, TimePosition localTime)? ResolveShot() =>
            this.Track.GetShotAtGlobalTime(new TimePosition(this.Playhead.CurrentTime));

        // ══════════════════════════════════════════════════════════════════
        // Private
        // ══════════════════════════════════════════════════════════════════

        private void EvaluateCamera()
        {
            var result = this.ResolveShot();

            if (!result.HasValue)
            {
                return;
            }

            var shot = result.Value.shot;

            if (shot != this.Track.CurrentShot)
            {
                this.Track.SetCurrentShot(shot.Id);
            }

            this._cameraEvaluationRequested.OnNext(new CameraEvaluation(shot, result.Value.localTime));
        }

        private void HandleShotClick(ShotId shotId)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (this._lastClickShotId == shotId && now - this._lastClickTime < DOUBLE_CLICK_MS)
            {
                this.Track.SetCurrentShot(shotId);
                this.FitToShot(shotId);
                this._lastClickShotId = null;
                return;
            }

            this._lastClickTime   = now;
            this._lastClickShotId = shotId;
            this.Track.SetCurrentShot(shotId);
        }

        private void CompleteDrag()
        {
            if (this._dragShotId == null)
            {
                return;
            }

            var toIndex = this._dragTargetIndex;

            if (toIndex > this._dragOriginalIndex)
            {
                toIndex--;
            }

            if (this._dragOriginalIndex != toIndex && toIndex >= 0 && toIndex < this.Track.Count)
            {
                this.Track.Reorder(this._dragShotId, toIndex);
            }
        }

        private void ResetPointerState()
        {
            this._isDragging    = false;
            this._dragShotId    = null;
            this._pointerIsDown = false;
        }

        private int FindShotIndexAtTime(double time)
        {
            var runningTime = 0.0;

            for (var i = 0; i < this.Track.Shots.Count; i++)
            {
                var end = runningTime + this.Track.Shots[i].Duration;

                if (time >= runningTime && time < end)
                {
                    return i;
                }

                runningTime = end;
            }

            return -1;
        }
    }

    public sealed class ShotTrackAction
    {
        public static readonly ShotTrackAction BOUNDARY_COMPLETE = new("Boundary Complete");
        public static readonly ShotTrackAction BOUNDARY_DRAG     = new("Boundary Drag");
        public static readonly ShotTrackAction CLICK             = new("Click");
        public static readonly ShotTrackAction DRAG_COMPLETE     = new("Drag Complete");
        public static readonly ShotTrackAction DRAG_MOVE         = new("Drag Move");
        public static readonly ShotTrackAction DRAG_START        = new("Drag Start");
        public static readonly ShotTrackAction NEAR_EDGE         = new("Near Edge");
        public static readonly ShotTrackAction NONE              = new("None");
        public static readonly ShotTrackAction POTENTIAL_CLICK   = new("Potential Click");

        private ShotTrackAction(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public override string ToString() => this.Name;
    }

    public sealed class CameraEvaluation
    {
        public CameraEvaluation(Shot shot, TimePosition localTime)
        {
            this.LocalTime = localTime;
            this.Shot      = shot;
        }

        public TimePosition LocalTime { get; }
        public Shot         Shot      { get; }
    }
}
