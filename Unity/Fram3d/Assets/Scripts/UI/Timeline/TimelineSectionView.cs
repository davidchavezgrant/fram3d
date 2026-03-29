using System;
using System.Collections.Generic;
using Fram3d.Core.Cameras;
using Fram3d.Core.Common;
using Fram3d.Core.Shots;
using Fram3d.Core.Timelines;
using Fram3d.Engine.Integration;
using Fram3d.UI.Panels;
using Fram3d.UI.Views;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// Thin View for the timeline section. Creates VisualElements, forwards
    /// pointer events to Timeline, reads controller state to
    /// position elements. Zero domain logic.
    /// </summary>
    public sealed class TimelineSectionView : MonoBehaviour
    {
        private const float LABEL_COL_W      = 140f;
        private const float SECTION_HEIGHT   = 320f;

        // ── References ──
        private Fram3d.Core.Timelines.Timeline _controller;
        private ShotEvaluator                  _shotController;

        // ── UI root ──
        private VisualElement _root;
        private VisualElement _section;

        // ── Child components ──
        private Ruler          _ruler;
        private ShotTrackStrip _shotTrackStrip;
        private TransportBar   _transport;
        private ZoomBar        _zoomBar;

        // ── Track area ──
        private readonly List<TrackRow> _trackRows = new();
        private TrackRow                _cameraTrackRow;
        private VisualElement           _trackContainer;
        private VisualElement           _trackOutOfRange;
        private VisualElement           _trackPlayhead;

        // ── Tooltips ──
        private VisualElement _boundaryTooltip;
        private Label         _boundaryTooltipText;
        private VisualElement _tooltip;
        private Label         _tooltipText;

        // ── Stopwatch ──
        private bool _suppressStopwatchWarning;

        // ── Visibility ──
        private bool _visible = true;

        // ══════════════════════════════════════════════════════════════════
        // Public API (called by keyboard router)
        // ══════════════════════════════════════════════════════════════════

        public bool HasFocusedTextField => this._shotTrackStrip != null && this._shotTrackStrip.HasEditingBlock;

        public bool IsPointerOverUI
        {
            get
            {
                if (this._root?.panel == null || Mouse.current == null)
                {
                    return false;
                }

                var mousePos  = Mouse.current.position.ReadValue();
                var screenPos = new Vector2(mousePos.x, Screen.height - mousePos.y);
                var panelPos  = RuntimePanelUtils.ScreenToPanel(this._root.panel, screenPos);
                return this._root.panel.Pick(panelPos) != null;
            }
        }

        public bool IsVisible => this._visible;

        public void FitAll() => this._controller?.FitAll();

        public void JumpToEnd() => this._controller?.JumpToEnd();

        public void JumpToStart() => this._controller?.JumpToStart();

        public void Toggle()
        {
            this._visible = !this._visible;

            if (this._section != null)
            {
                this._section.style.display = this._visible
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }

            this._shotController?.SetBottomInset(this._visible ? SECTION_HEIGHT : 0f);
        }

        public void TogglePlayback()
        {
            this._controller.TogglePlayback();
            this._transport.UpdatePlayButton(this._controller.Playhead.IsPlaying);
        }

        public void ZoomIn()  => this._controller?.ZoomAtPoint(this._controller.Playhead.CurrentTime, 1f);
        public void ZoomOut() => this._controller?.ZoomAtPoint(this._controller.Playhead.CurrentTime, -1f);

        // ══════════════════════════════════════════════════════════════════
        // Lifecycle
        // ══════════════════════════════════════════════════════════════════

        private void Start()
        {
            this._shotController = FindAnyObjectByType<ShotEvaluator>();

            if (this._shotController == null)
            {
                return;
            }

            this._controller = this._shotController.Controller;

            var uiDoc = this.GetComponent<UIDocument>();

            if (uiDoc?.rootVisualElement == null)
            {
                return;
            }

            this._root = uiDoc.rootVisualElement;
            StyleSheetLoader.Apply(this._root);
            this.BuildLayout();

            // Subscribe to rebuilds
            this._controller.ShotAdded.Subscribe(_ => this._shotTrackStrip.RebuildBlocks());
            this._controller.ShotRemoved.Subscribe(_ => this._shotTrackStrip.RebuildBlocks());
            this._controller.Reordered.Subscribe(_ => this._shotTrackStrip.RebuildBlocks());
            this._controller.CurrentShotChanged.Subscribe(_ => this._shotTrackStrip.UpdateActiveStates());
            this._controller.CurrentShotChanged.Subscribe(_ => this.RebuildTracks());
            this._controller.Selection.Changed.Subscribe(_ => this.SyncTrackVisuals());

            this._shotTrackStrip.RebuildBlocks();
        }

        private void Update()
        {
            if (this._controller == null)
            {
                return;
            }

            // Advance playback
            if (this._controller.Playhead.IsPlaying)
            {
                if (!this._controller.Advance(Time.deltaTime))
                {
                    this._transport.UpdatePlayButton(false);
                }
            }

            // Pinch-to-zoom via Input System
            this.HandleInputSystemScroll();

            // Update all visuals from controller state
            this.SyncVisuals();
        }

        // ══════════════════════════════════════════════════════════════════
        // Layout
        // ══════════════════════════════════════════════════════════════════

        private void BuildLayout()
        {
            this._section = new VisualElement();
            this._section.AddToClassList("timeline-section");
            this._section.style.height = SECTION_HEIGHT;

            this._transport = new TransportBar(this.TogglePlayback);
            this._section.Add(this._transport);

            this._ruler = new Ruler();
            this._ruler.ScrubRequested += this.OnScrub;
            this._ruler.RegisterScrubCallbacks();
            this._ruler.Content.RegisterCallback<WheelEvent>(this.OnWheel);
            this._section.Add(this._ruler);

            this._shotTrackStrip = new ShotTrackStrip();
            this._shotTrackStrip.AddShotRequested += this.OnAddShot;
            this._shotTrackStrip.BoundaryDragEnded += () =>
                this._boundaryTooltip.style.display = DisplayStyle.None;
            this._shotTrackStrip.BoundaryDragStarted += () =>
                this._boundaryTooltip.style.display = DisplayStyle.Flex;
            this._shotTrackStrip.ShotHoverEnded += () =>
                this._tooltip.style.display = DisplayStyle.None;
            this._shotTrackStrip.ShotHoverStarted += shot =>
            {
                this._tooltipText.text      = this._controller.FormatShotTooltip(shot);
                this._tooltip.style.display = DisplayStyle.Flex;
            };
            this._shotTrackStrip.TrackAreaResized += w =>
            {
                this._controller.InitializeViewRange(w);
                this.SyncVisuals();
                this.UpdateBottomInset();
            };
            this._shotTrackStrip.TrackArea.RegisterCallback<WheelEvent>(this.OnWheel);
            this._shotTrackStrip.Bind(this._controller);
            this._section.Add(this._shotTrackStrip);

            this.BuildTrackArea();

            this._zoomBar = new ZoomBar();
            this._zoomBar.PanRequested += px => { this._controller.Pan(px); };
            this._zoomBar.RegisterDragCallbacks();
            this._section.Add(this._zoomBar);

            this.BuildTooltips();
            this._root.Add(this._section);
        }

        private void BuildTooltips()
        {
            this._tooltip = new VisualElement();
            this._tooltip.AddToClassList("shot-tooltip");
            this._tooltip.style.display = DisplayStyle.None;
            this._tooltip.pickingMode   = PickingMode.Ignore;
            this._tooltipText           = new Label();
            this._tooltipText.AddToClassList("shot-tooltip__text");
            this._tooltip.Add(this._tooltipText);
            this._root.Add(this._tooltip);

            this._boundaryTooltip = new VisualElement();
            this._boundaryTooltip.AddToClassList("boundary-tooltip");
            this._boundaryTooltip.style.display = DisplayStyle.None;
            this._boundaryTooltip.pickingMode   = PickingMode.Ignore;
            this._boundaryTooltipText           = new Label();
            this._boundaryTooltipText.AddToClassList("boundary-tooltip__text");
            this._boundaryTooltip.Add(this._boundaryTooltipText);
            this._root.Add(this._boundaryTooltip);
        }

        private void BuildCameraSubTracks(TrackRow row)
        {
            row.AddSubTrack("Position X");
            row.AddSubTrack("Position Y");
            row.AddSubTrack("Position Z");
            row.AddSubTrack("Pan");
            row.AddSubTrack("Tilt");
            row.AddSubTrack("Roll");

            var cam = this.GetShotCamera();

            if (cam != null && cam.CanDollyZoom)
            {
                row.AddSubTrack("Focal Length");
            }

            row.AddSubTrack("Focus Distance");
            row.AddSubTrack("Aperture");
            row.SetExpanded(this._controller.Expansion.IsExpanded(TrackId.Camera));
        }

        private void BuildElementSubTracks(TrackRow row)
        {
            row.AddSubTrack("Position X");
            row.AddSubTrack("Position Y");
            row.AddSubTrack("Position Z");
            row.AddSubTrack("Scale");
            row.AddSubTrack("Rotation X");
            row.AddSubTrack("Rotation Y");
            row.AddSubTrack("Rotation Z");
        }

        private void BuildTrackArea()
        {
            this._trackContainer = new VisualElement();
            this._trackContainer.AddToClassList("timeline-track-area");

            this._trackPlayhead = new VisualElement();
            this._trackPlayhead.AddToClassList("timeline-playhead");
            this._trackPlayhead.AddToClassList("timeline-playhead--tracks");
            this._trackPlayhead.style.display  = DisplayStyle.None;
            this._trackPlayhead.pickingMode    = PickingMode.Ignore;

            this._trackOutOfRange = new VisualElement();
            this._trackOutOfRange.AddToClassList("timeline-out-of-range");
            this._trackOutOfRange.AddToClassList("timeline-out-of-range--tracks");
            this._trackOutOfRange.pickingMode = PickingMode.Ignore;

            this._trackContainer.RegisterCallback<WheelEvent>(this.OnWheel);
            this._trackContainer.RegisterCallback<ClickEvent>(this.OnTrackAreaClick);
            this._section.Add(this._trackContainer);

            this.RebuildTracks();
        }

        // ══════════════════════════════════════════════════════════════════
        // Visual sync (reads controller state, positions elements)
        // ══════════════════════════════════════════════════════════════════

        private void SyncVisuals()
        {
            if (this._controller == null)
            {
                return;
            }

            var total = this._controller.TotalDuration;
            var px    = (float)this._controller.PlayheadPixel;
            var endPx = (float)this._controller.OutOfRangeStartPixel;

            // Shot track strip (blocks, playhead, out-of-range, total, drop indicator)
            this._shotTrackStrip.SyncVisuals();

            // Track area
            this.SetPlayhead(this._trackPlayhead, px);
            this.SetOutOfRange(this._trackOutOfRange, endPx);
            this.SyncTrackVisuals();

            // Ruler + zoom + transport
            this._ruler.UpdatePlayhead(this._controller, this._controller.Playhead.CurrentTime);
            this._ruler.UpdateOutOfRange(this._controller, total);
            this._ruler.UpdateTicks(this._controller, total);
            this._zoomBar.UpdateThumb(this._controller, total);
            this._transport.UpdateTransport(
                this._controller.Playhead,
                this._controller);

            // Boundary tooltip
            if (this._controller.IsBoundaryDragging)
            {
                var shiftHeld = Keyboard.current?.leftShiftKey.isPressed ?? false;
                this._boundaryTooltipText.text = this._controller.FormatBoundaryTooltip(shiftHeld);
                this.PositionTooltipAtMouse(this._boundaryTooltip, -30f);
            }

            // Hover tooltip
            if (this._tooltip.style.display == DisplayStyle.Flex)
            {
                this.PositionTooltipAtMouse(this._tooltip, -50f);
            }
        }

        private void SetOutOfRange(VisualElement el, float endPx)
        {
            el.style.position = Position.Absolute;
            el.style.left     = endPx + LABEL_COL_W;
            el.style.top      = 0;
            el.style.bottom   = 0;
            el.style.right    = 0;
            el.style.display  = endPx >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetPlayhead(VisualElement el, float px)
        {
            el.style.display = DisplayStyle.Flex;
            el.style.left    = px + LABEL_COL_W;
        }

        // ══════════════════════════════════════════════════════════════════
        // Event forwarding → Controller
        // ══════════════════════════════════════════════════════════════════

        private void OnAddShot()
        {
            var shot = this._controller.AddShot();
            var cam  = this.GetShotCamera();

            if (cam != null)
            {
                shot.DefaultCameraPosition = cam.Position;
                shot.DefaultCameraRotation = cam.Rotation;
            }
        }

        private void OnScrub(float px)
        {
            this._controller.BeginScrub();
            this._controller.ScrubToPixel(px);
        }

        private void OnWheel(WheelEvent evt)
        {
            if (this._controller.VisibleDuration <= 0)
            {
                return;
            }

            if (evt.shiftKey)
            {
                this._controller.Pan(evt.delta.y * 2.0);
            }
            else if (evt.ctrlKey)
            {
                this._controller.ZoomAtPoint(this._controller.PixelToTime(evt.localMousePosition.x), -evt.delta.y);
            }
            else
            {
                var absX = Math.Abs(evt.delta.x);
                var absY = Math.Abs(evt.delta.y);

                if (absY > absX)
                {
                    this._controller.ZoomAtPoint(this._controller.PixelToTime(evt.localMousePosition.x), -evt.delta.y);
                }
                else if (absX > 0.01f)
                {
                    this._controller.Pan(evt.delta.x * 2.0);
                }
            }

            evt.StopPropagation();
        }

        private void HandleInputSystemScroll()
        {
            if (this._controller.VisibleDuration <= 0 || !this.IsPointerOverUI || Mouse.current == null)
            {
                return;
            }

            var scroll = Mouse.current.scroll.ReadValue();

            if (Math.Abs(scroll.y) < 0.01f)
            {
                return;
            }

            if (Keyboard.current == null || !Keyboard.current.ctrlKey.isPressed)
            {
                return;
            }

            var mousePos  = Mouse.current.position.ReadValue();
            var screenPos = new Vector2(mousePos.x, Screen.height - mousePos.y);
            var panelPos  = RuntimePanelUtils.ScreenToPanel(this._root.panel, screenPos);
            this._controller.ZoomAtPoint(this._controller.PixelToTime(panelPos.x - LABEL_COL_W), scroll.y);
        }

        // ══════════════════════════════════════════════════════════════════
        // Track management
        // ══════════════════════════════════════════════════════════════════

        private string GetElementName(ElementId id)
        {
            var elements = FindObjectsByType<ElementBehaviour>(FindObjectsSortMode.None);

            foreach (var eb in elements)
            {
                if (eb.Element?.Id != null && eb.Element.Id.Equals(id))
                {
                    return eb.Element.Name;
                }
            }

            return "Element";
        }

        private TimePosition GetLocalPlayheadTime()
        {
            var result = this._controller.ResolveShot();

            if (!result.HasValue)
            {
                return null;
            }

            return result.Value.localTime;
        }

        private CameraElement GetShotCamera()
        {
            var cam = FindAnyObjectByType<CameraBehaviour>();
            return cam?.ShotCamera;
        }

        private void HandleCameraStopwatchClick()
        {
            var shot = this._controller.CurrentShot;

            if (shot == null)
            {
                return;
            }

            var stopwatch = shot.CameraStopwatch;

            if (stopwatch.AnyRecording)
            {
                if (shot.TotalCameraKeyframeCount > 0 && !this._suppressStopwatchWarning)
                {
                    this.ShowStopwatchConfirmDialog(() =>
                    {
                        shot.ClearAllCameraKeyframes();
                        stopwatch.SetAll(false);
                        this.SyncTrackVisuals();
                    });

                    return;
                }

                stopwatch.SetAll(false);
            }
            else
            {
                stopwatch.SetAll(true);
                var cam = this.GetShotCamera();

                if (cam != null)
                {
                    var snap = CameraSnapshot.FromCamera(cam);
                    this._controller.ForceRecordCamera(snap);
                }
            }

            this.SyncTrackVisuals();
        }

        private void HandleElementStopwatchClick(ElementId elementId)
        {
            var track = this._controller.Elements.GetTrack(elementId);

            if (track == null)
            {
                return;
            }

            var stopwatch = track.Stopwatch;

            if (stopwatch.AnyRecording)
            {
                if (track.HasKeyframes && !this._suppressStopwatchWarning)
                {
                    this.ShowStopwatchConfirmDialog(() =>
                    {
                        track.ClearAllKeyframes();
                        stopwatch.SetAll(false);
                        this.RebuildTracks();
                    });

                    return;
                }

                stopwatch.SetAll(false);
            }
            else
            {
                stopwatch.SetAll(true);
            }

            this.SyncTrackVisuals();
        }

        private void OnTrackAreaClick(ClickEvent evt)
        {
            var target = evt.target as VisualElement;

            if (target == null)
            {
                return;
            }

            // Only scrub when clicking directly on a track-content area
            // (the right-hand region where diamonds live). Clicks on the
            // label column (stopwatch, arrow, name) are handled by their
            // own handlers and should not scrub.
            if (!target.ClassListContains("track-content"))
            {
                return;
            }

            if (Mouse.current == null || this._trackContainer.panel == null)
            {
                return;
            }

            var mousePos  = Mouse.current.position.ReadValue();
            var screenPos = new UnityEngine.Vector2(mousePos.x, Screen.height - mousePos.y);
            var panelPos  = RuntimePanelUtils.ScreenToPanel(this._trackContainer.panel, screenPos);
            var localX    = target.WorldToLocal(panelPos).x;
            this._controller.ScrubTrackArea(localX);
        }

        private void OnDiamondDragging(TimePosition originalTime, float localX)
        {
            var newTime = this.PixelToTrackTime(localX);
            this._controller.MoveSelectedKeyframe(newTime);
        }

        private void OnDiamondDropped(TimePosition originalTime, float localX)
        {
            var newTime = this.PixelToTrackTime(localX);
            this._controller.MoveSelectedKeyframe(newTime);
        }

        /// <summary>
        /// Converts a track-content pixel position to the correct time for the
        /// selected track. Camera tracks use shot-local time; element tracks use
        /// global time.
        /// </summary>
        private double PixelToTrackTime(float localX)
        {
            var globalTime = this._controller.PixelToTime(localX);
            var sel        = this._controller.Selection;

            if (sel.HasSelection && sel.TrackId == TrackId.Camera && this._controller.CurrentShot != null)
            {
                var shotStart = this._controller.GetGlobalStartTime(this._controller.CurrentShot.Id).Seconds;
                return globalTime - shotStart;
            }

            return globalTime;
        }

        private void OnDiamondClicked(TrackId trackId, KeyframeId keyframeId, TimePosition time)
        {
            if (time == null)
            {
                return;
            }

            // Camera keyframe times are shot-local — convert to global for scrub
            var globalTime = time;

            if (trackId == TrackId.Camera && this._controller.CurrentShot != null)
            {
                var shotStart = this._controller.GetGlobalStartTime(this._controller.CurrentShot.Id).Seconds;
                globalTime = new TimePosition(time.Seconds + shotStart);
            }

            if (keyframeId != null)
            {
                this._controller.SelectKeyframe(trackId, keyframeId, globalTime);
                return;
            }

            // Main diamond clicked — find a representative keyframe ID at this time
            KeyframeId representative = null;

            if (trackId == TrackId.Camera)
            {
                var shot = this._controller.CurrentShot;

                if (shot == null)
                {
                    return;
                }

                // Lookup uses shot-local time (what the keyframe manager stores)
                representative = shot.CameraPositionKeyframes.GetAtTime(time)?.Id
                              ?? shot.CameraRotationKeyframes.GetAtTime(time)?.Id;
            }
            else if (trackId.IsElement)
            {
                var track = this._controller.Elements.GetTrack(trackId.ElementId);

                if (track == null)
                {
                    return;
                }

                representative = track.PositionKeyframes.GetAtTime(time)?.Id
                              ?? track.RotationKeyframes.GetAtTime(time)?.Id
                              ?? track.ScaleKeyframes.GetAtTime(time)?.Id;
            }

            if (representative != null)
            {
                this._controller.SelectKeyframe(trackId, representative, globalTime);
            }
        }

        private void OnStopwatchClicked(TrackId trackId)
        {
            if (trackId.IsCamera)
            {
                this.HandleCameraStopwatchClick();
            }
            else if (trackId.IsElement)
            {
                this.HandleElementStopwatchClick(trackId.ElementId);
            }
        }

        private void OnTrackArrowClicked(TrackId trackId)
        {
            this._controller.Expansion.Toggle(trackId);
            this.SyncTrackVisuals();
        }

        private void RebuildTracks()
        {
            foreach (var row in this._trackRows)
            {
                row.RemoveFromHierarchy();
            }

            this._trackRows.Clear();

            // Camera track (always present)
            this._cameraTrackRow = new TrackRow(TrackId.Camera, "Camera", true);
            this._cameraTrackRow.ArrowClicked       += this.OnTrackArrowClicked;
            this._cameraTrackRow.DiamondClicked     += this.OnDiamondClicked;
            this._cameraTrackRow.DiamondDragging     += this.OnDiamondDragging;
            this._cameraTrackRow.DiamondDropped      += this.OnDiamondDropped;
            this._cameraTrackRow.StopwatchClicked   += this.OnStopwatchClicked;
            this.BuildCameraSubTracks(this._cameraTrackRow);
            this._trackRows.Add(this._cameraTrackRow);
            this._trackContainer.Insert(0, this._cameraTrackRow);

            // Element tracks
            foreach (var track in this._controller.Elements.Tracks)
            {
                if (!track.HasKeyframes)
                {
                    continue;
                }

                var elemRow = new TrackRow(
                    TrackId.ForElement(track.ElementId),
                    this.GetElementName(track.ElementId),
                    false);
                elemRow.ArrowClicked       += this.OnTrackArrowClicked;
                elemRow.DiamondClicked     += this.OnDiamondClicked;
                elemRow.DiamondDragging     += this.OnDiamondDragging;
                elemRow.DiamondDropped      += this.OnDiamondDropped;
                elemRow.StopwatchClicked   += this.OnStopwatchClicked;
                this.BuildElementSubTracks(elemRow);
                this._trackRows.Add(elemRow);
                this._trackContainer.Insert(this._trackRows.Count - 1, elemRow);
            }

            // Re-add playhead and out-of-range as overlays spanning all tracks
            this._trackContainer.Add(this._trackPlayhead);
            this._trackContainer.Add(this._trackOutOfRange);
        }

        private void SyncCameraSubTrackValues(Shot shot)
        {
            var localTime = this.GetLocalPlayheadTime();

            if (localTime == null || this._cameraTrackRow.SubTracks.Count == 0)
            {
                return;
            }

            var pos   = shot.EvaluateCameraPosition(localTime);
            var rot   = shot.EvaluateCameraRotation(localTime);
            var euler = EulerAngles.FromQuaternion(rot);

            var subs = this._cameraTrackRow.SubTracks;
            var idx  = 0;
            subs[idx++].SetValue(pos.X.ToString("F2"));
            subs[idx++].SetValue(pos.Y.ToString("F2"));
            subs[idx++].SetValue(pos.Z.ToString("F2"));
            subs[idx++].SetValue(euler.Pan.ToString("F1") + "\u00B0");
            subs[idx++].SetValue(euler.Tilt.ToString("F1") + "\u00B0");
            subs[idx++].SetValue(euler.Roll.ToString("F1") + "\u00B0");

            var cam = this.GetShotCamera();

            if (cam != null && cam.CanDollyZoom && idx < subs.Count)
            {
                var fl = cam.FocalLength;

                if (shot.CameraFocalLengthKeyframes.Count > 0)
                {
                    fl = shot.EvaluateCameraFocalLength(localTime);
                }

                subs[idx++].SetValue(fl.ToString("F0") + "mm");
            }

            if (idx < subs.Count)
            {
                var fd = cam?.FocusDistance ?? 0f;

                if (shot.CameraFocusDistanceKeyframes.Count > 0)
                {
                    fd = shot.EvaluateCameraFocusDistance(localTime);
                }

                subs[idx++].SetValue(fd.ToString("F1") + "m");
            }

            if (idx < subs.Count)
            {
                var ap = cam?.Aperture ?? 0f;

                if (shot.CameraApertureKeyframes.Count > 0)
                {
                    ap = shot.EvaluateCameraAperture(localTime);
                }

                subs[idx++].SetValue("f/" + ap.ToString("F1"));
            }
        }

        private void SyncTrackVisuals()
        {
            if (this._cameraTrackRow == null || this._controller.CurrentShot == null)
            {
                return;
            }

            var shot                         = this._controller.CurrentShot;
            var shotStartSec                 = this._controller.GetGlobalStartTime(shot.Id).Seconds;
            var shotEndSec                   = this._controller.GetGlobalEndTime(shot.Id).Seconds;
            var activeStart                  = new TimePosition(shotStartSec);
            var activeEnd                    = new TimePosition(shotEndSec);
            Func<double, double> timeToPixel = this._controller.TimeToPixel;

            // Build shot segments for all track rows
            var segments = this.BuildShotSegments(timeToPixel);

            // Camera track — no dimming (all diamonds are within active shot)
            this._cameraTrackRow.UpdateShotSegments(segments);
            var cameraTimes                        = shot.GetAllCameraKeyframeTimes();
            Func<double, double> cameraTimeToPixel = t => timeToPixel(t + shotStartSec);
            this._cameraTrackRow.UpdateMainDiamonds(cameraTimes, cameraTimeToPixel, this._controller.Selection);
            this._cameraTrackRow.SetExpanded(this._controller.Expansion.IsExpanded(TrackId.Camera));

            var camSw = shot.CameraStopwatch;
            this._cameraTrackRow.SetStopwatchState(camSw.AnyRecording, camSw.AllRecording);

            if (this._controller.Expansion.IsExpanded(TrackId.Camera))
            {
                this.SyncCameraSubTrackValues(shot);
            }

            // Element tracks — dim diamonds outside the active shot
            for (var i = 1; i < this._trackRows.Count; i++)
            {
                var row     = this._trackRows[i];
                var trackId = row.TrackId;

                if (trackId == null || !trackId.IsElement)
                {
                    continue;
                }

                var track = this._controller.Elements.GetTrack(trackId.ElementId);

                if (track == null)
                {
                    continue;
                }

                row.UpdateShotSegments(segments);
                var elemTimes = track.GetAllKeyframeTimes();
                row.UpdateMainDiamonds(elemTimes, timeToPixel, this._controller.Selection, activeStart, activeEnd);
                row.SetExpanded(this._controller.Expansion.IsExpanded(trackId));

                var elemSw = track.Stopwatch;
                row.SetStopwatchState(elemSw.AnyRecording, elemSw.AllRecording);
            }
        }

        private ShotSegmentInfo[] BuildShotSegments(Func<double, double> timeToPixel)
        {
            var shots       = this._controller.Shots;
            var currentShot = this._controller.CurrentShot;
            var segments    = new ShotSegmentInfo[shots.Count];
            var runningTime = 0.0;

            for (var i = 0; i < shots.Count; i++)
            {
                var s       = shots[i];
                var startPx = (float)timeToPixel(runningTime);
                var endPx   = (float)timeToPixel(runningTime + s.Duration);
                var widthPx = Math.Max(endPx - startPx, 2f);

                segments[i] = new ShotSegmentInfo
                {
                    LeftPx   = startPx,
                    WidthPx  = widthPx,
                    Color    = ShotColorPalette.GetColor(i),
                    IsActive = s == currentShot
                };

                runningTime += s.Duration;
            }

            return segments;
        }

        // ══════════════════════════════════════════════════════════════════
        // Helpers
        // ══════════════════════════════════════════════════════════════════

        private void PositionTooltipAtMouse(VisualElement el, float yOffset)
        {
            if (Mouse.current == null || this._root?.panel == null)
            {
                return;
            }

            var mousePos  = Mouse.current.position.ReadValue();
            var screenPos = new Vector2(mousePos.x, Screen.height - mousePos.y);
            var panelPos  = RuntimePanelUtils.ScreenToPanel(this._root.panel, screenPos);
            el.style.left = panelPos.x + 12f;
            el.style.top  = panelPos.y + yOffset;
        }

        private void ShowStopwatchConfirmDialog(Action onConfirm)
        {
            StopwatchConfirmDialog dialog = null;
            dialog = new StopwatchConfirmDialog(onConfirm, dontShowAgain =>
            {
                if (dontShowAgain)
                {
                    this._suppressStopwatchWarning = true;
                }

                dialog.RemoveFromHierarchy();
            });
            this._root.Add(dialog);
        }

        private void UpdateBottomInset()
        {
            if (this._shotController == null || this._root == null)
            {
                return;
            }

            this._shotController.SetBottomInset(ViewportScope.CssToScreen(this._root, SECTION_HEIGHT));
        }
    }
}
