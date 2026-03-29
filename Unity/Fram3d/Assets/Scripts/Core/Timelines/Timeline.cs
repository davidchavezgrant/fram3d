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
        private const    double                     DOUBLE_CLICK_MS             = 350;
        private const    double                     EDGE_TOLERANCE_PX           = 12.0;
        private const    int                        HOLD_THRESHOLD_MS           = 200;
        private readonly Subject<CameraEvaluation>  _cameraEvaluationRequested  = new();
        private readonly Subject<ElementEvaluation> _elementEvaluationRequested = new();
        private readonly ViewRange                  _view;

        // ── Interaction state ──
        private int    _boundaryDragIndex = -1;
        private int    _dragOriginalIndex;
        private ShotId _dragShotId;
        private int    _dragTargetIndex;
        private bool   _isBoundaryDragging;
        private bool   _isDragging;
        private bool   _isScrubbing;
        private ShotId _lastClickShotId;
        private long   _lastClickTime;
        private long   _pointerDownTime;
        private double _pointerDownX;
        private bool   _pointerIsDown;

        public Timeline(FrameRate frameRate)
        {
            this.Elements = new ElementTimeline();
            this.Playhead = new Playhead(frameRate);
            this.Track    = new ShotTrack(frameRate);
            this._view    = new ViewRange();
        }

        // ══════════════════════════════════════════════════════════════════
        // Shot track interaction state machine
        // ══════════════════════════════════════════════════════════════════
        public int BoundaryDragIndex => this._boundaryDragIndex;

        // ══════════════════════════════════════════════════════════════════
        // Observables
        // ══════════════════════════════════════════════════════════════════
        public IObservable<CameraEvaluation> CameraEvaluationRequested => this._cameraEvaluationRequested;

        // ══════════════════════════════════════════════════════════════════
        // Delegated shot properties
        // ══════════════════════════════════════════════════════════════════
        public int                            Count                      => this.Track.Count;
        public Shot                           CurrentShot                => this.Track.CurrentShot;
        public IObservable<Shot>              CurrentShotChanged         => this.Track.CurrentShotChanged;
        public int                            DragTargetIndex            => this._dragTargetIndex;
        public IObservable<ElementEvaluation> ElementEvaluationRequested => this._elementEvaluationRequested;

        // ══════════════════════════════════════════════════════════════════
        // Sub-components
        // ══════════════════════════════════════════════════════════════════
        public ElementTimeline Elements           { get; }
        public TrackExpansion  Expansion          { get; } = new();
        public FrameRate       FrameRate          => this.Track.FrameRate;
        public bool            IsBoundaryDragging => this._isBoundaryDragging;
        public bool            IsDragging         => this._isDragging;

        // ══════════════════════════════════════════════════════════════════
        // Scrub
        // ══════════════════════════════════════════════════════════════════
        public bool                IsScrubbing          => this._isScrubbing;
        public double              OutOfRangeStartPixel => this._view.TimeToPixel(this.TotalDuration);
        public double              PixelsPerSecond      => this._view.PixelsPerSecond;
        public Playhead            Playhead             { get; }
        public double              PlayheadPixel        => this._view.TimeToPixel(this.Playhead.CurrentTime);
        public IObservable<bool>   Reordered            => this.Track.Reordered;
        public KeyframeSelection   Selection            { get; } = new();
        public IObservable<Shot>   ShotAdded            => this.Track.ShotAdded;
        public IObservable<Shot>   ShotRemoved          => this.Track.ShotRemoved;
        public IReadOnlyList<Shot> Shots                => this.Track.Shots;
        public double              TotalDuration        => this.Track.TotalDuration;

        // ══════════════════════════════════════════════════════════════════
        // Delegated view range properties
        // ══════════════════════════════════════════════════════════════════
        public  IObservable<bool> ViewChanged     => this._view.Changed;
        public  double            ViewEnd         => this._view.ViewEnd;
        public  double            ViewStart       => this._view.ViewStart;
        public  double            VisibleDuration => this._view.VisibleDuration;
        private ShotTrack         Track           { get; }

        // ══════════════════════════════════════════════════════════════════
        // Shot lifecycle
        // ══════════════════════════════════════════════════════════════════

        public Shot AddShot()
        {
            var shot = this.Track.AddShot();

            if (this._view.IsInitialized)
            {
                this.FitAll();
            }

            return shot;
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

        public void BeginScrub()                                      => this._isScrubbing = true;
        public void EndScrub()                                        => this._isScrubbing = false;
        public void EnsureVisible(double      seconds)                => this._view.EnsureVisible(seconds);
        public int  FindEdgeAtTime(double     time, double tolerance) => this.Track.FindEdgeAtTime(time, tolerance);
        public int  FindInsertionIndex(double time)    => this.Track.FindInsertionIndex(time);
        public void FitAll()                           => this._view.FitAll(this.TotalDuration);
        public void FitRange(double start, double end) => this._view.FitRange(start, end);

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

        public void JumpToEnd()
        {
            this.Playhead.Scrub(this.TotalDuration, this.TotalDuration);
            this.EvaluateCamera();
            this.EnsureVisible(this.TotalDuration);
        }

        public void JumpToStart()
        {
            this.Playhead.Scrub(0, this.TotalDuration);
            this.EvaluateCamera();
            this.EnsureVisible(0);
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

        public string                               FormatResizeTooltip(int index, bool shift) => this.Track.FormatResizeTooltip(index, shift);
        public string                               FormatShotTooltip(Shot shot) => this.Track.FormatShotTooltip(shot);
        public Shot                                 GetById(ShotId id) => this.Track.GetById(id);
        public TimePosition                         GetGlobalEndTime(ShotId id) => this.Track.GetGlobalEndTime(id);
        public TimePosition                         GetGlobalStartTime(ShotId id) => this.Track.GetGlobalStartTime(id);
        public (Shot shot, TimePosition localTime)? GetShotAtGlobalTime(TimePosition t) => this.Track.GetShotAtGlobalTime(t);
        public int                                  IndexOf(ShotId id) => this.Track.IndexOf(id);
        public void                                 InitializeViewRange(double trackWidth) => this._view.Initialize(trackWidth, this.TotalDuration);
        public void                                 Pan(double deltaPx) => this._view.Pan(deltaPx, this.TotalDuration);
        public double                               PixelToTime(double px) => this._view.PixelToTime(px);
        public ShotTrackAction                      BeginBoundaryDrag(int edgeIndex)
        {
            if (edgeIndex < 0 || edgeIndex >= this.Track.Count)
            {
                return ShotTrackAction.NONE;
            }

            this.ResetPointerState();
            this._isBoundaryDragging = true;
            this._boundaryDragIndex  = edgeIndex;
            return ShotTrackAction.BOUNDARY_DRAG;
        }
        public bool                                 RemoveShot(ShotId id) => this.Track.RemoveShot(id);
        public void                                 Reorder(ShotId id, int newIndex) => this.Track.Reorder(id, newIndex);
        public double                               ResizeShotAtEdge(int index, double endTime) => this.Track.ResizeShotAtEdge(index, endTime);

        // ══════════════════════════════════════════════════════════════════
        // Query
        // ══════════════════════════════════════════════════════════════════

        public TimePosition GetLocalPlayheadTime()
        {
            var result = this.ResolveShot();

            if (!result.HasValue)
            {
                return null;
            }

            return result.Value.localTime;
        }

        public (Shot shot, TimePosition localTime)? ResolveShot() => this.Track.GetShotAtGlobalTime(new TimePosition(this.Playhead.CurrentTime));

        // ══════════════════════════════════════════════════════════════════
        // Recording
        // ══════════════════════════════════════════════════════════════════

        public void ForceRecordCamera(CameraSnapshot current)
        {
            if (this.Playhead.IsPlaying)
            {
                return;
            }

            if (this.CurrentShot == null)
            {
                return;
            }

            var localTime = this.GetLocalPlayheadTime();

            if (localTime == null)
            {
                return;
            }

            KeyframeRecorder.ForceRecordCamera(this.CurrentShot, localTime, current);
        }

        public void ForceRecordElement(ElementId elementId, ElementSnapshot current)
        {
            if (this.Playhead.IsPlaying)
            {
                return;
            }

            var track      = this.Elements.GetOrCreateTrack(elementId);
            var globalTime = new TimePosition(this.Playhead.CurrentTime);

            KeyframeRecorder.ForceRecordElement(track, globalTime, current);
        }

        public void RecordCameraManipulation(CameraSnapshot current, CameraSnapshot previous)
        {
            if (this.Playhead.IsPlaying)
            {
                return;
            }

            if (this.CurrentShot == null)
            {
                return;
            }

            // When recording is off, update the shot's default camera state
            // so it holds whatever position the user places the camera at.
            if (!this.CurrentShot.CameraStopwatch.AnyRecording)
            {
                this.CurrentShot.DefaultCameraPosition = current.Position;
                this.CurrentShot.DefaultCameraRotation = current.Rotation;
                return;
            }

            var localTime = this.GetLocalPlayheadTime();

            if (localTime == null)
            {
                return;
            }

            KeyframeRecorder.RecordCamera(
                this.CurrentShot, this.CurrentShot.CameraStopwatch, localTime, current, previous);
        }

        public void RecordElementManipulation(ElementId elementId, ElementSnapshot current, ElementSnapshot previous)
        {
            if (this.Playhead.IsPlaying)
            {
                return;
            }

            var track      = this.Elements.GetOrCreateTrack(elementId);
            var globalTime = new TimePosition(this.Playhead.CurrentTime);

            KeyframeRecorder.RecordElement(track, track.Stopwatch, globalTime, current, previous);
        }

        /// <summary>
        /// Adds a keyframe for all properties of the given element at the current
        /// playhead time. No-op during playback or if no element ID is provided.
        /// </summary>
        public void AddElementKeyframeAtPlayhead(ElementId elementId, ElementSnapshot current)
        {
            if (this.Playhead.IsPlaying || elementId == null)
            {
                return;
            }

            this.ForceRecordElement(elementId, current);
        }

        /// <summary>
        /// Deletes the currently selected keyframe. Enforces minimum 1 camera
        /// keyframe per shot. Clears selection and re-evaluates camera state.
        /// Returns true if a keyframe was deleted.
        /// </summary>
        public bool DeleteSelectedKeyframe()
        {
            if (!this.Selection.HasSelection)
            {
                return false;
            }

            var trackId    = this.Selection.TrackId;
            var globalTime = this.Selection.Time;

            if (trackId == TrackId.Camera)
            {
                if (this.CurrentShot == null)
                {
                    return false;
                }

                var localTime = this.ToLocalCameraTime(globalTime);
                this.CurrentShot.DeleteAllCameraKeyframesAtTime(localTime);
            }
            else if (trackId.IsElement)
            {
                var track = this.Elements.GetTrack(trackId.ElementId);

                if (track == null)
                {
                    return false;
                }

                track.DeleteAllKeyframesAtTime(globalTime);
            }
            else
            {
                return false;
            }

            this.Selection.Clear();
            this.EvaluateCamera();
            return true;
        }

        /// <summary>
        /// Moves the currently selected keyframe (and all co-located property
        /// keyframes) to a new time. Snaps to 0.1s grid, clamps to [0, duration].
        /// Returns true if the move was performed.
        /// </summary>
        public bool MoveSelectedKeyframe(double newTimeSeconds)
        {
            if (!this.Selection.HasSelection)
            {
                return false;
            }

            var trackId       = this.Selection.TrackId;
            var oldGlobalTime = this.Selection.Time;

            // Snap to 0.1s grid (newTimeSeconds is already in the correct space:
            // local for camera, global for elements — see PixelToTrackTime)
            var snapped = Math.Round(newTimeSeconds * 10.0) / 10.0;

            if (trackId == TrackId.Camera)
            {
                if (this.CurrentShot == null)
                {
                    return false;
                }

                snapped = Math.Clamp(snapped, 0.0, this.CurrentShot.Duration);
                var newLocal = new TimePosition(snapped);
                var oldLocal = this.ToLocalCameraTime(oldGlobalTime);

                if (newLocal == oldLocal)
                {
                    return false;
                }

                this.CurrentShot.MoveAllCameraKeyframesAtTime(oldLocal, newLocal);

                // Store global time in selection, scrub playhead to global
                var shotStart   = this.Track.GetGlobalStartTime(this.CurrentShot.Id).Seconds;
                var newGlobal   = new TimePosition(snapped + shotStart);
                this.Selection.Select(trackId, this.Selection.KeyframeId, newGlobal);
                this.Playhead.Scrub(newGlobal.Seconds, this.TotalDuration);
            }
            else if (trackId.IsElement)
            {
                var track = this.Elements.GetTrack(trackId.ElementId);

                if (track == null)
                {
                    return false;
                }

                snapped = Math.Max(snapped, 0.0);
                var to = new TimePosition(snapped);

                if (to == oldGlobalTime)
                {
                    return false;
                }

                track.MoveAllKeyframesAtTime(oldGlobalTime, to);
                this.Selection.Select(trackId, this.Selection.KeyframeId, to);
                this.Playhead.Scrub(snapped, this.TotalDuration);
            }
            else
            {
                return false;
            }

            this.EvaluateCamera();
            return true;
        }

        /// <summary>
        /// Scrubs the playhead from a click in the track area and clears keyframe selection.
        /// </summary>
        public void ScrubTrackArea(double px)
        {
            this.Selection.Clear();
            this.ScrubToPixel(px);
        }

        public void ScrubToPixel(double px)
        {
            var rawTime = this.PixelToTime(px);
            this.Playhead.Scrub(rawTime, this.TotalDuration);
            this.EvaluateCamera();
            this.EnsureVisible(Math.Clamp(rawTime, 0, this.TotalDuration));
        }

        public void SelectKeyframe(TrackId trackId, KeyframeId keyframeId, TimePosition time)
        {
            this.Selection.Select(trackId, keyframeId, time);
            this.Playhead.Scrub(time.Seconds, this.TotalDuration);
            this.EvaluateCamera();
        }

        public void SetCurrentShot(ShotId id)                => this.Track.SetCurrentShot(id);
        public void SetViewRange(double   start, double end) => this._view.SetRange(start, end, this.TotalDuration);

        public ShotTrackAction ShotTrackPointerDown(double px, long timestampMs)
        {
            var time      = this.PixelToTime(px);
            var tolerance = this.PixelsPerSecond > 0 ? EDGE_TOLERANCE_PX / this.PixelsPerSecond : 1.0;
            var edgeIndex = this.Track.FindEdgeAtTime(time, tolerance);

            if (edgeIndex >= 0)
            {
                return this.BeginBoundaryDrag(edgeIndex);
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

        public ShotTrackAction ShotTrackPointerMove(double px, long timestampMs)
        {
            if (this._isBoundaryDragging)
            {
                this.Track.ResizeShotAtEdge(this._boundaryDragIndex, this.PixelToTime(px));
                return ShotTrackAction.BOUNDARY_DRAG;
            }

            if (this._pointerIsDown && this._dragShotId != null && !this._isDragging)
            {
                if (timestampMs - this._pointerDownTime >= HOLD_THRESHOLD_MS || Math.Abs(px - this._pointerDownX) > 5)
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
                var hoveredEdge = this.Track.FindEdgeAtTime(time, tolerance);

                if (hoveredEdge >= 0)
                {
                    return ShotTrackAction.NEAR_EDGE;
                }
            }

            return ShotTrackAction.NONE;
        }

        public ShotTrackAction ShotTrackPointerUp()
        {
            if (this._isBoundaryDragging)
            {
                this._isBoundaryDragging = false;
                this._boundaryDragIndex  = -1;
                this.ResetPointerState();
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

        public double TimeToPixel(double seconds) => this._view.TimeToPixel(seconds);

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

        public void ZoomAtPoint(double anchorSeconds, float scrollDelta) => this._view.ZoomAtPoint(anchorSeconds, scrollDelta, this.TotalDuration);

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
            this._elementEvaluationRequested.OnNext(new ElementEvaluation(new TimePosition(this.Playhead.CurrentTime)));
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

        /// <summary>
        /// Converts a global time to shot-local time for the current shot.
        /// </summary>
        private TimePosition ToLocalCameraTime(TimePosition globalTime)
        {
            if (this.CurrentShot == null)
            {
                return globalTime;
            }

            var shotStart = this.Track.GetGlobalStartTime(this.CurrentShot.Id).Seconds;
            return new TimePosition(Math.Max(0, globalTime.Seconds - shotStart));
        }

        private void ResetPointerState()
        {
            this._isDragging    = false;
            this._dragShotId    = null;
            this._pointerIsDown = false;
        }
    }
}
