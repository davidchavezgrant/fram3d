using System;
using System.Collections.Generic;
using System.Numerics;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// THE timeline. Owns a list of shots, the playhead, the visible time range,
    /// and all operations: add/remove/reorder shots, scrub, playback, zoom, pan,
    /// boundary resize, edge detection, and tooltip formatting.
    ///
    /// Pure C# — no Unity dependencies. The View sends commands and reads state.
    /// </summary>
    public sealed class Timeline
    {
        // ── Constants ──
        private const double DOUBLE_CLICK_MS   = 350;
        private const double EDGE_TOLERANCE_PX = 6.0;
        private const int    HOLD_THRESHOLD_MS = 200;
        private const double MIN_VISIBLE_DURATION = 0.5;
        private const double ZOOM_FACTOR       = 1.15;

        // ── Shot storage ──
        private readonly Subject<Shot> _added          = new();
        private readonly Subject<Shot> _currentChanged = new();
        private readonly FrameRate     _frameRate;
        private readonly Subject<Shot> _removed        = new();
        private readonly Subject<bool> _reordered      = new();
        private readonly List<Shot>    _shots          = new();
        private          Shot          _currentShot;
        private          int           _nextShotNumber = 1;

        // ── View range ──
        private double _stripWidth  = 1.0;
        private double _totalDuration = 1.0;
        private double _viewEnd;
        private double _viewStart;

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
            this._frameRate = frameRate ?? throw new ArgumentNullException(nameof(frameRate));
            this.Playhead   = new Playhead(frameRate);
        }

        // ══════════════════════════════════════════════════════════════════
        // Shot collection
        // ══════════════════════════════════════════════════════════════════

        public int                  Count       => this._shots.Count;
        public Shot                 CurrentShot => this._currentShot;
        public FrameRate            FrameRate   => this._frameRate;
        public IReadOnlyList<Shot>  Shots       => this._shots;
        public double               TotalDuration => this.ComputeTotalDuration();

        public IObservable<Shot> CurrentShotChanged => this._currentChanged;
        public IObservable<bool> Reordered          => this._reordered;
        public IObservable<Shot> ShotAdded          => this._added;
        public IObservable<Shot> ShotRemoved        => this._removed;

        public Shot AddShot(Vector3 cameraPosition, Quaternion cameraRotation)
        {
            var name = $"Shot_{this._nextShotNumber:D2}";
            this._nextShotNumber++;
            var id   = new ShotId(Guid.NewGuid());
            var shot = new Shot(id, name, cameraPosition, cameraRotation);
            this._shots.Add(shot);
            this.SetCurrentShot(id);
            this._added.OnNext(shot);

            this._totalDuration = this.TotalDuration;

            if (this._viewEnd > 0)
            {
                this.FitAll();
            }

            return shot;
        }

        public bool RemoveShot(ShotId id)
        {
            var index = this._shots.FindIndex(s => s.Id == id);

            if (index < 0)
            {
                return false;
            }

            var shot       = this._shots[index];
            var wasCurrent = this._currentShot == shot;
            this._shots.RemoveAt(index);
            this._removed.OnNext(shot);

            if (wasCurrent)
            {
                if (this._shots.Count == 0)
                {
                    this._currentShot = null;
                    this._currentChanged.OnNext(null);
                }
                else if (index < this._shots.Count)
                {
                    this.SetCurrentShot(this._shots[index].Id);
                }
                else
                {
                    this.SetCurrentShot(this._shots[this._shots.Count - 1].Id);
                }
            }

            this._totalDuration = this.TotalDuration;
            return true;
        }

        public void Reorder(ShotId id, int newIndex)
        {
            var oldIndex = this._shots.FindIndex(s => s.Id == id);

            if (oldIndex < 0 || newIndex < 0 || newIndex >= this._shots.Count || oldIndex == newIndex)
            {
                return;
            }

            var shot = this._shots[oldIndex];
            this._shots.RemoveAt(oldIndex);
            this._shots.Insert(newIndex, shot);
            this._reordered.OnNext(true);
        }

        public void SetCurrentShot(ShotId id)
        {
            var shot = this._shots.Find(s => s.Id == id);

            if (shot == null || shot == this._currentShot)
            {
                return;
            }

            this._currentShot = shot;
            this._currentChanged.OnNext(shot);
        }

        public Shot GetById(ShotId id) => this._shots.Find(s => s.Id == id);
        public int IndexOf(ShotId id) => this._shots.FindIndex(s => s.Id == id);

        public TimePosition GetGlobalStartTime(ShotId id)
        {
            var seconds = 0.0;

            foreach (var shot in this._shots)
            {
                if (shot.Id == id)
                {
                    return new TimePosition(seconds);
                }

                seconds += shot.Duration;
            }

            throw new ArgumentException($"Shot {id} not found");
        }

        public TimePosition GetGlobalEndTime(ShotId id) =>
            this.GetGlobalStartTime(id).Add(this.GetById(id).Duration);

        public (Shot shot, TimePosition localTime)? GetShotAtGlobalTime(TimePosition globalTime)
        {
            var seconds = 0.0;

            foreach (var shot in this._shots)
            {
                var end = seconds + shot.Duration;

                if (globalTime.Seconds >= seconds && globalTime.Seconds < end)
                {
                    return (shot, new TimePosition(globalTime.Seconds - seconds));
                }

                seconds = end;
            }

            if (this._shots.Count > 0 && Math.Abs(globalTime.Seconds - seconds) < 1e-9)
            {
                var last = this._shots[this._shots.Count - 1];
                return (last, new TimePosition(last.Duration));
            }

            return null;
        }

        // ══════════════════════════════════════════════════════════════════
        // Playhead
        // ══════════════════════════════════════════════════════════════════

        public Playhead Playhead { get; }

        // ══════════════════════════════════════════════════════════════════
        // View range (zoom, pan, pixel conversion)
        // ══════════════════════════════════════════════════════════════════

        public double ViewEnd   => this._viewEnd;
        public double ViewStart => this._viewStart;

        public double VisibleDuration => this._viewEnd - this._viewStart;

        public double PixelsPerSecond
        {
            get
            {
                if (this.VisibleDuration > 0)
                {
                    return this._stripWidth / this.VisibleDuration;
                }

                return this._stripWidth;
            }
        }

        public double PlayheadPixel => this.TimeToPixel(this.Playhead.CurrentTime);

        public double OutOfRangeStartPixel => this.TimeToPixel(this.TotalDuration);

        private readonly Subject<bool> _viewChanged = new();

        public IObservable<bool> ViewChanged => this._viewChanged;

        public void InitializeViewRange(double stripWidth)
        {
            this._stripWidth = Math.Max(stripWidth, 1.0);

            if (this._viewEnd <= 0)
            {
                this._totalDuration = this.TotalDuration;
                this.FitAll();
            }
            else
            {
                this._viewChanged.OnNext(true);
            }
        }

        public double TimeToPixel(double seconds) =>
            (seconds - this._viewStart) * this.PixelsPerSecond;

        public double PixelToTime(double px) =>
            this._viewStart + px / this.PixelsPerSecond;

        public void ZoomAtPoint(double anchorSeconds, float scrollDelta)
        {
            var factor = scrollDelta > 0 ? 1.0 / ZOOM_FACTOR : ZOOM_FACTOR;
            var newDuration = this.VisibleDuration * factor;
            newDuration = Math.Clamp(newDuration, MIN_VISIBLE_DURATION, this._totalDuration);

            var t = this.VisibleDuration > 0
                ? (anchorSeconds - this._viewStart) / this.VisibleDuration
                : 0.5;

            this._viewStart = anchorSeconds - t * newDuration;
            this._viewEnd   = anchorSeconds + (1.0 - t) * newDuration;
            this.ClampView();
            this._viewChanged.OnNext(true);
        }

        public void Pan(double deltaPx)
        {
            var delta = deltaPx / this.PixelsPerSecond;
            this._viewStart += delta;
            this._viewEnd   += delta;
            this.ClampView();
            this._viewChanged.OnNext(true);
        }

        public void FitAll()
        {
            var total = Math.Max(this.TotalDuration, 1.0);
            this._viewStart = 0;
            this._viewEnd   = total;
            this._viewChanged.OnNext(true);
        }

        public void FitRange(double start, double end)
        {
            var duration = Math.Max(end - start, 1.0);
            var padding  = duration * 0.08;
            this._viewStart = start - padding;
            this._viewEnd   = end + padding;
            this._viewChanged.OnNext(true);
        }

        public void SetViewRange(double start, double end)
        {
            this._viewStart = start;
            this._viewEnd   = end;
            this.ClampView();
            this._viewChanged.OnNext(true);
        }

        public void EnsureVisible(double seconds)
        {
            var duration = this.VisibleDuration;

            if (seconds < this._viewStart)
            {
                this._viewStart = seconds;
                this._viewEnd   = seconds + duration;
                this.ClampViewLeft();
                this._viewChanged.OnNext(true);
            }
            else if (seconds > this._viewEnd)
            {
                this._viewEnd   = seconds;
                this._viewStart = seconds - duration;
                this.ClampViewLeft();
                this._viewChanged.OnNext(true);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // Playback commands
        // ══════════════════════════════════════════════════════════════════

        public bool TogglePlayback()
        {
            var wasAtEnd     = this.Playhead.CurrentTime >= this.TotalDuration - this._frameRate.FrameDuration;
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

                if (this.Playhead.CurrentTime > this._viewEnd)
                {
                    var duration = this.VisibleDuration;
                    this.SetViewRange(this._viewEnd, this._viewEnd + duration);
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
        // Shot strip interaction
        // ══════════════════════════════════════════════════════════════════

        public int  BoundaryDragIndex  => this._boundaryDragIndex;
        public int  DragTargetIndex    => this._dragTargetIndex;
        public bool IsBoundaryDragging => this._isBoundaryDragging;
        public bool IsDragging         => this._isDragging;

        public StripInteraction StripPointerDown(double px, long timestampMs)
        {
            var time      = this.PixelToTime(px);
            var tolerance = this.PixelsPerSecond > 0 ? EDGE_TOLERANCE_PX / this.PixelsPerSecond : 1.0;
            var edgeIndex = this.FindEdgeAtTime(time, tolerance);

            if (edgeIndex >= 0)
            {
                this._isBoundaryDragging = true;
                this._boundaryDragIndex  = edgeIndex;
                return StripInteraction.BOUNDARY_DRAG;
            }

            this._pointerDownTime = timestampMs;
            this._pointerDownX    = px;
            this._pointerIsDown   = true;

            var shotIndex = this.FindShotIndexAtTime(time);

            if (shotIndex >= 0)
            {
                this._dragShotId        = this._shots[shotIndex].Id;
                this._dragOriginalIndex = shotIndex;
            }

            return StripInteraction.POTENTIAL_CLICK;
        }

        public StripInteraction StripPointerMove(double px, long timestampMs)
        {
            if (this._isBoundaryDragging)
            {
                this.ResizeShotAtEdge(this._boundaryDragIndex, this.PixelToTime(px));
                this._totalDuration = this.TotalDuration;
                return StripInteraction.BOUNDARY_DRAG;
            }

            if (this._pointerIsDown && this._dragShotId != null && !this._isDragging)
            {
                if (timestampMs - this._pointerDownTime >= HOLD_THRESHOLD_MS
                 || Math.Abs(px - this._pointerDownX) > 5)
                {
                    this._isDragging = true;
                    return StripInteraction.DRAG_START;
                }
            }

            if (this._isDragging)
            {
                this._dragTargetIndex = this.FindInsertionIndex(this.PixelToTime(px));
                return StripInteraction.DRAG_MOVE;
            }

            if (!this._pointerIsDown)
            {
                var time      = this.PixelToTime(px);
                var tolerance = this.PixelsPerSecond > 0 ? EDGE_TOLERANCE_PX / this.PixelsPerSecond : 1.0;

                if (this.FindEdgeAtTime(time, tolerance) >= 0)
                {
                    return StripInteraction.NEAR_EDGE;
                }
            }

            return StripInteraction.NONE;
        }

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

        // ══════════════════════════════════════════════════════════════════
        // Shot track operations
        // ══════════════════════════════════════════════════════════════════

        public int FindEdgeAtTime(double time, double toleranceSeconds)
        {
            var runningTime = 0.0;

            for (var i = 0; i < this._shots.Count; i++)
            {
                runningTime += this._shots[i].Duration;

                if (Math.Abs(time - runningTime) <= toleranceSeconds)
                {
                    return i;
                }
            }

            return -1;
        }

        public int FindInsertionIndex(double time)
        {
            var runningTime = 0.0;

            for (var i = 0; i < this._shots.Count; i++)
            {
                var midpoint = runningTime + this._shots[i].Duration / 2.0;

                if (time < midpoint)
                {
                    return i;
                }

                runningTime += this._shots[i].Duration;
            }

            return this._shots.Count;
        }

        public double ResizeShotAtEdge(int shotIndex, double newEndTime)
        {
            var shot        = this._shots[shotIndex];
            var startTime   = this.GetGlobalStartTime(shot.Id).Seconds;
            var newDuration = newEndTime - startTime;
            var snapped     = this._frameRate.SnapToFrame(
                new TimePosition(Math.Max(newDuration, Shot.MIN_DURATION)));

            shot.Duration = snapped.Seconds;
            return snapped.Seconds;
        }

        public void FitToShot(ShotId shotId)
        {
            var shotIndex = this.IndexOf(shotId);
            var start     = this.GetGlobalStartTime(shotId).Seconds;
            var end       = this.GetGlobalEndTime(shotId).Seconds;
            var duration  = end - start;
            var padding   = duration * 0.08;
            var isFirst   = shotIndex == 0;
            var isLast    = shotIndex == this._shots.Count - 1;

            if (this._shots.Count == 1)
            {
                this.FitAll();
            }
            else if (isFirst)
            {
                this.SetViewRange(0, end + padding);
            }
            else if (isLast)
            {
                this.SetViewRange(start - padding, this.TotalDuration);
            }
            else
            {
                this.FitRange(start, end);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // Formatting
        // ══════════════════════════════════════════════════════════════════

        public string FormatShotTooltip(Shot shot)
        {
            var frames  = new TimePosition(shot.Duration).ToFrame(this._frameRate);
            var kfCount = shot.TotalCameraKeyframeCount;
            return $"{shot.Name}\nCam A \u00b7 {shot.Duration:F1}s ({frames}f) \u00b7 {kfCount} kf";
        }

        public string FormatResizeTooltip(int shotIndex, bool shiftHeld)
        {
            var shot   = this._shots[shotIndex];
            var frames = new TimePosition(shot.Duration).ToFrame(this._frameRate);
            var mode   = shiftHeld ? "[shots only]" : "[ripple]";
            return $"{shot.Name}: {shot.Duration:F1}s ({frames}f) {mode}";
        }

        public string FormatBoundaryTooltip(bool shiftHeld)
        {
            if (this._boundaryDragIndex < 0 || this._boundaryDragIndex >= this._shots.Count)
            {
                return "";
            }

            return this.FormatResizeTooltip(this._boundaryDragIndex, shiftHeld);
        }

        // ══════════════════════════════════════════════════════════════════
        // Events
        // ══════════════════════════════════════════════════════════════════

        private readonly Subject<CameraEvaluation> _cameraEvaluationRequested = new();

        public IObservable<CameraEvaluation> CameraEvaluationRequested => this._cameraEvaluationRequested;

        // ══════════════════════════════════════════════════════════════════
        // Private
        // ══════════════════════════════════════════════════════════════════

        private double ComputeTotalDuration()
        {
            var total = 0.0;

            foreach (var shot in this._shots)
            {
                total += shot.Duration;
            }

            return total;
        }

        /// <summary>
        /// Resolves which shot the playhead is in and returns the shot-local time.
        /// </summary>
        public (Shot shot, TimePosition localTime)? ResolveShot() =>
            this.GetShotAtGlobalTime(new TimePosition(this.Playhead.CurrentTime));

        private void EvaluateCamera()
        {
            var result = this.ResolveShot();

            if (!result.HasValue)
            {
                return;
            }

            var shot = result.Value.shot;

            if (shot != this._currentShot)
            {
                this.SetCurrentShot(shot.Id);
            }

            this._cameraEvaluationRequested.OnNext(new CameraEvaluation(shot, result.Value.localTime));
        }

        private void HandleShotClick(ShotId shotId)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (this._lastClickShotId == shotId && now - this._lastClickTime < DOUBLE_CLICK_MS)
            {
                this.SetCurrentShot(shotId);
                this.FitToShot(shotId);
                this._lastClickShotId = null;
                return;
            }

            this._lastClickTime   = now;
            this._lastClickShotId = shotId;
            this.SetCurrentShot(shotId);
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

            if (this._dragOriginalIndex != toIndex && toIndex >= 0 && toIndex < this._shots.Count)
            {
                this.Reorder(this._dragShotId, toIndex);
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

            for (var i = 0; i < this._shots.Count; i++)
            {
                var end = runningTime + this._shots[i].Duration;

                if (time >= runningTime && time < end)
                {
                    return i;
                }

                runningTime = end;
            }

            return -1;
        }

        private void ClampView()
        {
            this.ClampViewLeft();

            if (this._viewEnd > this._totalDuration)
            {
                var duration    = this.VisibleDuration;
                this._viewEnd   = this._totalDuration;
                this._viewStart = this._totalDuration - duration;

                if (this._viewStart < 0)
                {
                    this._viewStart = 0;
                }
            }
        }

        private void ClampViewLeft()
        {
            if (this._viewStart < 0)
            {
                var shift = -this._viewStart;
                this._viewStart += shift;
                this._viewEnd   += shift;
            }
        }
    }

    /// <summary>
    /// Result of a shot strip pointer event. Sealed class pattern —
    /// closed set with typed data.
    /// </summary>
    public sealed class StripInteraction
    {
        public static readonly StripInteraction BOUNDARY_COMPLETE = new("Boundary Complete");
        public static readonly StripInteraction BOUNDARY_DRAG     = new("Boundary Drag");
        public static readonly StripInteraction CLICK             = new("Click");
        public static readonly StripInteraction DRAG_COMPLETE     = new("Drag Complete");
        public static readonly StripInteraction DRAG_MOVE         = new("Drag Move");
        public static readonly StripInteraction DRAG_START        = new("Drag Start");
        public static readonly StripInteraction NEAR_EDGE         = new("Near Edge");
        public static readonly StripInteraction NONE              = new("None");
        public static readonly StripInteraction POTENTIAL_CLICK   = new("Potential Click");

        private StripInteraction(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public override string ToString() => this.Name;
    }

    /// <summary>
    /// Value type emitted when the camera should evaluate at a shot-local time.
    /// </summary>
    public sealed class CameraEvaluation
    {
        public CameraEvaluation(Shot shot, TimePosition localTime)
        {
            this.Shot      = shot;
            this.LocalTime = localTime;
        }

        public TimePosition LocalTime { get; }
        public Shot         Shot      { get; }
    }
}
