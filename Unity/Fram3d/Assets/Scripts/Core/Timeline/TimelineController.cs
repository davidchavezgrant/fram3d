using System;
using System.Numerics;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
namespace Fram3d.Core.Timeline
{
    /// <summary>
    /// Orchestrates all timeline operations. Owns the Playhead, ShotTrack,
    /// and TimelineState. Handles interaction state machines (scrub, boundary
    /// drag, shot drag reorder) and computes everything the View needs.
    ///
    /// Pure C# — no Unity dependencies. The View sends commands and reads state.
    /// </summary>
    public sealed class TimelineController
    {
        private const double DOUBLE_CLICK_MS  = 350;
        private const double EDGE_TOLERANCE_PX = 6.0;
        private const int    HOLD_THRESHOLD_MS = 200;

        // ── Interaction state ──
        private int    _boundaryDragIndex = -1;
        private ShotId _dragShotId;
        private int    _dragOriginalIndex;
        private int    _dragTargetIndex;
        private bool   _isDragging;
        private bool   _isBoundaryDragging;
        private bool   _isScrubbing;
        private ShotId _lastClickShotId;
        private long   _lastClickTime;
        private double _pointerDownX;
        private long   _pointerDownTime;
        private bool   _pointerIsDown;

        public TimelineController(FrameRate frameRate)
        {
            this.Playhead = new Playhead(frameRate);
            this.Track    = new ShotTrack(frameRate);
        }

        // ── Core components ────────────────────────────────────────────

        public Playhead      Playhead { get; }
        public ShotTrack     Track    { get; }
        public TimelineState State    { get; private set; }

        // ── Interaction state (read by View) ───────────────────────────

        public int    BoundaryDragIndex => this._boundaryDragIndex;
        public int    DragTargetIndex   => this._dragTargetIndex;
        public bool   IsBoundaryDragging => this._isBoundaryDragging;
        public bool   IsDragging         => this._isDragging;
        public bool   IsScrubbing        => this._isScrubbing;
        public ShotId DragShotId         => this._dragShotId;

        // ── Computed properties (read by View) ─────────────────────────

        public double PlayheadPixel
        {
            get
            {
                if (this.State == null)
                {
                    return 0;
                }

                return this.State.TimeToPixel(this.Playhead.CurrentTime);
            }
        }

        public double OutOfRangeStartPixel
        {
            get
            {
                if (this.State == null)
                {
                    return 0;
                }

                return this.State.TimeToPixel(this.Track.TotalDuration);
            }
        }

        // ── Events (View subscribes) ───────────────────────────────────

        /// <summary>
        /// Fires when the camera should be evaluated at the current playhead
        /// position. The View routes this to CameraBehaviour.
        /// </summary>
        public event Action<Shot, TimePosition> CameraEvaluationRequested;

        /// <summary>
        /// Fires when shot blocks need rebuilding (add, remove, reorder).
        /// </summary>
        public event Action BlocksChanged;

        // ── Initialization ─────────────────────────────────────────────

        public void InitializeState(double stripWidth)
        {
            if (this.State != null)
            {
                this.State.SetStripWidth(stripWidth);
                return;
            }

            this.State = new TimelineState(this.Track.TotalDuration, stripWidth);
        }

        public Shot AddShot(Vector3 cameraPosition, Quaternion cameraRotation)
        {
            var shot = this.Track.AddShot(cameraPosition, cameraRotation);

            if (this.State != null)
            {
                this.State.SetTotalDuration(this.Track.TotalDuration);
                this.State.FitAll(this.Track.TotalDuration);
            }

            return shot;
        }

        // ── Playback ───────────────────────────────────────────────────

        /// <summary>
        /// Toggles playback. Returns true if now playing.
        /// Resets view to beginning if restarting from end.
        /// </summary>
        public bool TogglePlayback()
        {
            var totalDuration = this.Track.TotalDuration;
            var wasAtEnd      = this.Playhead.CurrentTime >= totalDuration - this.Playhead.FrameRate.FrameDuration;
            var isNowPlaying  = this.Playhead.TogglePlayback(totalDuration);

            if (isNowPlaying && wasAtEnd && this.State != null)
            {
                var duration = this.State.VisibleDuration;
                this.State.SetViewRange(0, duration);
            }

            return isNowPlaying;
        }

        /// <summary>
        /// Advances playback by deltaSeconds. Returns false if playback stopped.
        /// Handles page-flip scrolling when playhead exits visible range.
        /// </summary>
        public bool Advance(double deltaSeconds)
        {
            var totalDuration = this.Track.TotalDuration;
            var stillPlaying  = this.Playhead.Advance(deltaSeconds, totalDuration);

            if (stillPlaying)
            {
                this.EvaluateCamera();

                // Page-flip scroll
                if (this.State != null && this.Playhead.CurrentTime > this.State.ViewEnd)
                {
                    var duration = this.State.VisibleDuration;
                    this.State.SetViewRange(this.State.ViewEnd, this.State.ViewEnd + duration);
                }
            }

            return stillPlaying;
        }

        // ── Scrub ──────────────────────────────────────────────────────

        public void BeginScrub()
        {
            this._isScrubbing = true;
        }

        public void ScrubToPixel(double px)
        {
            if (this.State == null)
            {
                return;
            }

            var rawTime       = this.State.PixelToTime(px);
            var totalDuration = this.Track.TotalDuration;
            this.Playhead.Scrub(rawTime, totalDuration);
            this.EvaluateCamera();

            var scrollTime = Math.Clamp(rawTime, 0, totalDuration);
            this.State.EnsureVisible(scrollTime);
        }

        public void EndScrub()
        {
            this._isScrubbing = false;
        }

        // ── Shot strip pointer interaction ──────────────────────────────

        /// <summary>
        /// Called when pointer goes down on the shot strip. Returns the
        /// type of interaction that started.
        /// </summary>
        public StripInteraction StripPointerDown(double px, long timestampMs)
        {
            if (this.State == null)
            {
                return StripInteraction.NONE;
            }

            // Check for boundary edge first
            var time      = this.State.PixelToTime(px);
            var tolerance = this.State.PixelsPerSecond > 0
                ? EDGE_TOLERANCE_PX / this.State.PixelsPerSecond
                : 1.0;
            var edgeIndex = this.Track.FindEdgeAtTime(time, tolerance);

            if (edgeIndex >= 0)
            {
                this._isBoundaryDragging = true;
                this._boundaryDragIndex  = edgeIndex;
                return StripInteraction.BOUNDARY_DRAG;
            }

            // Otherwise start a potential click/drag on a shot
            this._pointerDownTime = timestampMs;
            this._pointerDownX    = px;
            this._pointerIsDown   = true;

            // Find which shot was clicked
            var shotIndex = this.FindShotIndexAtPixel(px);

            if (shotIndex >= 0)
            {
                this._dragShotId        = this.Track.Shots[shotIndex].Id;
                this._dragOriginalIndex = shotIndex;
            }

            return StripInteraction.POTENTIAL_CLICK;
        }

        /// <summary>
        /// Called on pointer move over the shot strip. Returns what changed.
        /// </summary>
        public StripInteraction StripPointerMove(double px, long timestampMs)
        {
            if (this._isBoundaryDragging)
            {
                if (this.State != null)
                {
                    var cursorTime = this.State.PixelToTime(px);
                    this.Track.ResizeShotAtEdge(this._boundaryDragIndex, cursorTime);
                    this.State.SetTotalDuration(this.Track.TotalDuration);
                }

                return StripInteraction.BOUNDARY_DRAG;
            }

            if (this._pointerIsDown && this._dragShotId != null && !this._isDragging)
            {
                var elapsed = timestampMs - this._pointerDownTime;
                var dist    = Math.Abs(px - this._pointerDownX);

                if (elapsed >= HOLD_THRESHOLD_MS || dist > 5)
                {
                    this._isDragging = true;
                    return StripInteraction.DRAG_START;
                }
            }

            if (this._isDragging && this.State != null)
            {
                var time = this.State.PixelToTime(px);
                this._dragTargetIndex = this.Track.FindInsertionIndex(time);
                return StripInteraction.DRAG_MOVE;
            }

            // Hover — check if near an edge for cursor feedback
            if (!this._pointerIsDown && this.State != null)
            {
                var time      = this.State.PixelToTime(px);
                var tolerance = this.State.PixelsPerSecond > 0
                    ? EDGE_TOLERANCE_PX / this.State.PixelsPerSecond
                    : 1.0;

                if (this.Track.FindEdgeAtTime(time, tolerance) >= 0)
                {
                    return StripInteraction.NEAR_EDGE;
                }
            }

            return StripInteraction.NONE;
        }

        /// <summary>
        /// Called on pointer up. Returns what completed.
        /// </summary>
        public StripInteraction StripPointerUp()
        {
            if (this._isBoundaryDragging)
            {
                this._isBoundaryDragging = false;
                this._boundaryDragIndex  = -1;
                return StripInteraction.BOUNDARY_COMPLETE;
            }

            if (this._isDragging)
            {
                this.CompleteDrag();
                this.ResetPointerState();
                return StripInteraction.DRAG_COMPLETE;
            }

            if (this._dragShotId != null)
            {
                this.HandleShotClick(this._dragShotId);
                this.ResetPointerState();
                return StripInteraction.CLICK;
            }

            this.ResetPointerState();
            return StripInteraction.NONE;
        }

        // ── Zoom / Pan ─────────────────────────────────────────────────

        public void ZoomIn()
        {
            this.State?.ZoomAtPoint(this.Playhead.CurrentTime, 1f);
        }

        public void ZoomOut()
        {
            this.State?.ZoomAtPoint(this.Playhead.CurrentTime, -1f);
        }

        public void ZoomAtPixel(double px, float scrollDelta)
        {
            if (this.State == null)
            {
                return;
            }

            var cursorTime = this.State.PixelToTime(px);
            this.State.ZoomAtPoint(cursorTime, scrollDelta);
        }

        public void Pan(double deltaPx)
        {
            this.State?.Pan(deltaPx);
        }

        // ── Formatting ─────────────────────────────────────────────────

        public string FormatTransportTime()
        {
            var current = this.Track.CurrentShot;

            if (current == null)
            {
                return "00;00;00;00";
            }

            var startTime = this.Track.GetGlobalStartTime(current.Id).Seconds;
            var localTime = Math.Max(0, this.Playhead.CurrentTime - startTime);
            localTime     = Math.Min(localTime, current.Duration);
            return FormatTimecode(localTime);
        }

        public string FormatTransportDuration()
        {
            var current = this.Track.CurrentShot;

            if (current == null)
            {
                return "00;00;00;00";
            }

            return FormatTimecode(current.Duration);
        }

        public string FormatBoundaryTooltip(bool shiftHeld)
        {
            if (this._boundaryDragIndex < 0 || this._boundaryDragIndex >= this.Track.Count)
            {
                return "";
            }

            return this.Track.FormatResizeTooltip(this._boundaryDragIndex, shiftHeld);
        }

        // ── Private ────────────────────────────────────────────────────

        private void EvaluateCamera()
        {
            var result = this.Playhead.ResolveShot(this.Track);

            if (!result.HasValue)
            {
                return;
            }

            var shot = result.Value.shot;

            if (shot != this.Track.CurrentShot)
            {
                this.Track.SetCurrentShot(shot.Id);
            }

            this.CameraEvaluationRequested?.Invoke(shot, result.Value.localTime);
        }

        private void HandleShotClick(ShotId shotId)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (this._lastClickShotId == shotId
             && now - this._lastClickTime < DOUBLE_CLICK_MS)
            {
                // Double-click: select + fit
                this.Track.SetCurrentShot(shotId);

                if (this.State != null)
                {
                    this.Track.FitToShot(shotId, this.State);
                }

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

            var fromIndex = this._dragOriginalIndex;
            var toIndex   = this._dragTargetIndex;

            if (toIndex > fromIndex)
            {
                toIndex--;
            }

            if (fromIndex != toIndex && toIndex >= 0 && toIndex < this.Track.Count)
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

        private int FindShotIndexAtPixel(double px)
        {
            if (this.State == null)
            {
                return -1;
            }

            var time        = this.State.PixelToTime(px);
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

        private static string FormatTimecode(double seconds)
        {
            var fps         = 24;
            var totalFrames = (int)(seconds * fps);
            var h           = totalFrames / (fps * 3600);
            var remainder   = totalFrames % (fps * 3600);
            var m           = remainder / (fps * 60);
            remainder       = remainder % (fps * 60);
            var s           = remainder / fps;
            var f           = remainder % fps;
            return $"{h:D2};{m:D2};{s:D2};{f:D2}";
        }
    }

    /// <summary>
    /// Result of a shot strip pointer event, telling the View what UI to update.
    /// </summary>
    public enum StripInteraction
    {
        NONE,
        BOUNDARY_DRAG,
        BOUNDARY_COMPLETE,
        CLICK,
        DRAG_COMPLETE,
        DRAG_MOVE,
        DRAG_START,
        NEAR_EDGE,
        POTENTIAL_CLICK
    }
}
