using System;
using System.Collections.Generic;
using Fram3d.Core.Common;
using Fram3d.Core.Shot;
using Fram3d.Engine.Integration;
using Fram3d.UI.Panels;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// MonoBehaviour orchestrator for the full timeline section. Contains the
    /// transport bar, time ruler, shot track, track area (placeholder), and zoom bar.
    /// Manages the shared TimelineViewState for zoom/pan across all components.
    /// </summary>
    public sealed class TimelineSectionView : MonoBehaviour
    {
        private const float  BOUNDARY_HIT_WIDTH  = 8f;
        private const double DOUBLE_CLICK_MS      = 350;
        private const int    HOLD_THRESHOLD_MS     = 200;
        private const float  LABEL_COLUMN_WIDTH    = 140f;
        private const float  RULER_HEIGHT          = 22f;
        private const float  SECTION_HEIGHT        = 320f;
        private const float  TRANSPORT_HEIGHT      = 28f;
        private const float  ZOOM_BAR_HEIGHT       = 18f;

        // ── References ──
        private ShotController _shotController;

        // ── UI root ──
        private VisualElement _root;
        private VisualElement _section;

        // ── Transport bar ──
        private Label _transportTime;
        private Label _transportDuration;
        private Label _transportShot;

        // ── Ruler ──
        private VisualElement _rulerContent;
        private VisualElement _rulerPlayhead;

        // ── Shot track ──
        private VisualElement _shotLabelColumn;
        private VisualElement _shotStrip;
        private Label         _totalLabel;
        private VisualElement _dropIndicator;

        // ── Track area ──
        private VisualElement _trackLabels;
        private VisualElement _trackContent;
        private VisualElement _trackPlayhead;

        // ── Zoom bar ──
        private VisualElement _zoomBar;
        private VisualElement _zoomThumb;
        private VisualElement _zoomPlayhead;

        // ── Tooltips ──
        private VisualElement _tooltip;
        private Label         _tooltipText;
        private VisualElement _boundaryTooltip;
        private Label         _boundaryTooltipText;

        // ── Shot block tracking ──
        private readonly List<ShotBlockElement> _blocks     = new();
        private readonly List<VisualElement>    _boundaries = new();
        private TimelineViewState _viewState;

        // ── Subscriptions ──
        private IDisposable _addedSub;
        private IDisposable _currentChangedSub;
        private IDisposable _removedSub;
        private IDisposable _reorderedSub;

        // ── Drag-and-drop state ──
        private ShotBlockElement _dragBlock;
        private int              _dragOriginalIndex;
        private int              _dragTargetIndex;
        private bool             _isDragging;
        private long             _pointerDownTime;
        private Vector2          _pointerDownPos;
        private bool             _pointerIsDown;

        // ── Boundary drag state ──
        private int  _boundaryLeftIndex;
        private bool _isBoundaryDragging;

        // ── Double-click state ──
        private ShotId _lastClickShotId;
        private long   _lastClickTime;

        // ── Pan state ──
        private bool    _isPanning;
        private Vector2 _panStartPos;

        // ── Zoom bar drag ──
        private bool  _isZoomDragging;
        private float _zoomDragStartX;

        // ── Scrub state ──
        private double _currentGlobalTime;
        private bool   _isScrubbing;

        // ── Tooltip state ──
        private ShotBlockElement _hoveredBlock;

        // ── Visibility ──
        private bool _visible = true;

        public bool HasFocusedTextField
        {
            get
            {
                foreach (var block in this._blocks)
                {
                    if (block.IsEditing)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsVisible => this._visible;

        public bool IsPointerOverUI
        {
            get
            {
                if (this._root == null || this._root.panel == null || Mouse.current == null)
                {
                    return false;
                }

                var mousePos  = Mouse.current.position.ReadValue();
                var screenPos = new Vector2(mousePos.x, Screen.height - mousePos.y);
                var panelPos  = RuntimePanelUtils.ScreenToPanel(this._root.panel, screenPos);
                var picked    = this._root.panel.Pick(panelPos);
                return picked != null;
            }
        }

        public void Toggle()
        {
            this._visible = !this._visible;

            if (this._section != null)
            {
                if (this._visible)
                {
                    this._section.style.display = DisplayStyle.Flex;
                }
                else
                {
                    this._section.style.display = DisplayStyle.None;
                }
            }

            if (this._shotController != null)
            {
                var inset = 0f;

                if (this._visible)
                {
                    inset = SECTION_HEIGHT;
                }

                this._shotController.SetBottomInset(inset);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // Lifecycle
        // ══════════════════════════════════════════════════════════════════

        private void OnDestroy()
        {
            this._addedSub?.Dispose();
            this._currentChangedSub?.Dispose();
            this._removedSub?.Dispose();
            this._reorderedSub?.Dispose();
        }

        private void Start()
        {
            this._shotController = FindAnyObjectByType<ShotController>();

            if (this._shotController == null)
            {
                Debug.LogWarning("TimelineSectionView: No ShotController found.");
                return;
            }

            var uiDocument = this.GetComponent<UIDocument>();

            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogWarning("TimelineSectionView: UIDocument or rootVisualElement is null.");
                return;
            }

            this._root = uiDocument.rootVisualElement;
            StyleSheetLoader.Apply(this._root);

            this.BuildLayout();
            this.SubscribeToRegistry();
            this.RebuildBlocks();
        }

        private void Update()
        {
            if (this._shotController == null)
            {
                return;
            }

            this.UpdateBlockWidths();
            this.UpdatePlayhead();
            this.UpdateRuler();
            this.UpdateTransport();
            this.UpdateTotalLabel();
            this.UpdateZoomBar();
            this.UpdateTooltipPosition();
            this.UpdateBoundaryTooltipPosition();
        }

        // ══════════════════════════════════════════════════════════════════
        // Layout
        // ══════════════════════════════════════════════════════════════════

        private void BuildLayout()
        {
            this._section = new VisualElement();
            this._section.AddToClassList("timeline-section");
            this._section.style.height = SECTION_HEIGHT;

            this.BuildTransportBar();
            this.BuildRuler();
            this.BuildShotTrack();
            this.BuildTrackArea();
            this.BuildZoomBar();

            // Tooltips (floating, on root)
            this.BuildTooltips();

            this._root.Add(this._section);

            // Initialize view state after shot strip has geometry
            this._shotStrip.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                var stripWidth = this._shotStrip.resolvedStyle.width;

                if (float.IsNaN(stripWidth) || stripWidth <= 0)
                {
                    return;
                }

                if (this._viewState == null)
                {
                    this._viewState = new TimelineViewState(
                        this._shotController.Registry.TotalDuration,
                        stripWidth);
                }
                else
                {
                    this._viewState.SetStripWidth(stripWidth);
                }

                this.UpdateBlockWidths();
                this.UpdateRuler();
                this.UpdateZoomBar();
                this.UpdateBottomInset();
            });
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

        private void BuildTransportBar()
        {
            var bar = new VisualElement();
            bar.AddToClassList("timeline-transport");
            bar.style.height = TRANSPORT_HEIGHT;

            var playBtn = new Button();
            playBtn.text = "\u25b6";
            playBtn.AddToClassList("timeline-transport__play");
            playBtn.SetEnabled(false);
            bar.Add(playBtn);

            this._transportTime = new Label("00;00;00;00");
            this._transportTime.AddToClassList("timeline-transport__time");
            bar.Add(this._transportTime);

            var divider = new Label("/");
            divider.AddToClassList("timeline-transport__divider");
            bar.Add(divider);

            this._transportDuration = new Label("00;00;05;00");
            this._transportDuration.AddToClassList("timeline-transport__time");
            bar.Add(this._transportDuration);

            this._transportShot = new Label();
            this._transportShot.AddToClassList("timeline-transport__shot");
            bar.Add(this._transportShot);

            this._section.Add(bar);
        }

        private void BuildRuler()
        {
            var row = new VisualElement();
            row.AddToClassList("timeline-ruler-row");
            row.style.height = RULER_HEIGHT;

            var label = new VisualElement();
            label.AddToClassList("timeline-label-column");
            row.Add(label);

            this._rulerContent = new VisualElement();
            this._rulerContent.AddToClassList("timeline-ruler");
            row.Add(this._rulerContent);

            this._rulerPlayhead = new VisualElement();
            this._rulerPlayhead.AddToClassList("timeline-playhead");
            this._rulerPlayhead.style.display = DisplayStyle.None;
            this._rulerContent.Add(this._rulerPlayhead);

            // Zoom/pan on ruler
            this._rulerContent.RegisterCallback<WheelEvent>(this.OnStripWheel);

            // Click/drag to scrub on ruler
            this._rulerContent.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0)
                {
                    return;
                }

                this._isScrubbing = true;
                this.ScrubToPixel(evt.localPosition.x);
                this._rulerContent.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });
            this._rulerContent.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (this._isScrubbing)
                {
                    this.ScrubToPixel(evt.localPosition.x);
                }
            });
            this._rulerContent.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (this._isScrubbing)
                {
                    this._isScrubbing = false;
                    this._rulerContent.ReleasePointer(evt.pointerId);
                }
            });

            this._section.Add(row);
        }

        private void BuildShotTrack()
        {
            var row = new VisualElement();
            row.AddToClassList("timeline-shot-row");

            // Label column
            this._shotLabelColumn = new VisualElement();
            this._shotLabelColumn.AddToClassList("timeline-label-column");

            var shotLabel = new Label("SHOTS");
            shotLabel.AddToClassList("timeline-label-column__title");
            this._shotLabelColumn.Add(shotLabel);

            this._totalLabel = new Label("Total: 0.0s");
            this._totalLabel.AddToClassList("timeline-label-column__subtitle");
            this._shotLabelColumn.Add(this._totalLabel);

            var addButton = new Button(this.OnAddShotClicked);
            addButton.text = "+";
            addButton.AddToClassList("timeline-shot__add-button");
            this._shotLabelColumn.Add(addButton);

            row.Add(this._shotLabelColumn);

            // Shot strip
            this._shotStrip = new VisualElement();
            this._shotStrip.AddToClassList("timeline-shot-strip");
            row.Add(this._shotStrip);

            // Register shot strip interactions
            this._shotStrip.RegisterCallback<WheelEvent>(this.OnStripWheel);
            this._shotStrip.RegisterCallback<PointerDownEvent>(this.OnStripPointerDown);
            this._shotStrip.RegisterCallback<PointerMoveEvent>(this.OnStripPointerMove);
            this._shotStrip.RegisterCallback<PointerUpEvent>(this.OnStripPointerUp);

            // Drop indicator
            this._dropIndicator = new VisualElement();
            this._dropIndicator.AddToClassList("shot-track__drop-indicator");
            this._dropIndicator.style.display = DisplayStyle.None;
            this._shotStrip.Add(this._dropIndicator);

            this._section.Add(row);
        }

        private void BuildTrackArea()
        {
            var row = new VisualElement();
            row.AddToClassList("timeline-track-row");

            this._trackLabels = new VisualElement();
            this._trackLabels.AddToClassList("timeline-label-column");
            row.Add(this._trackLabels);

            this._trackContent = new VisualElement();
            this._trackContent.AddToClassList("timeline-track-content");
            row.Add(this._trackContent);

            this._trackPlayhead = new VisualElement();
            this._trackPlayhead.AddToClassList("timeline-playhead");
            this._trackPlayhead.style.display = DisplayStyle.None;
            this._trackContent.Add(this._trackPlayhead);

            // Zoom/pan on track area
            this._trackContent.RegisterCallback<WheelEvent>(this.OnStripWheel);
            this._trackContent.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    this._isScrubbing = true;
                    this.ScrubToPixel(evt.localPosition.x);
                    this._trackContent.CapturePointer(evt.pointerId);
                    evt.StopPropagation();
                }
                else if (evt.button == 2)
                {
                    this._isPanning   = true;
                    this._panStartPos = evt.localPosition;
                    evt.StopPropagation();
                }
            });
            this._trackContent.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (this._isScrubbing && this._viewState != null)
                {
                    this.ScrubToPixel(evt.localPosition.x);
                }

                if (this._isPanning && this._viewState != null)
                {
                    var delta = evt.localPosition.x - this._panStartPos.x;
                    this._viewState.Pan(-delta);
                    this._panStartPos = evt.localPosition;
                    this.UpdateBlockWidths();
                    this.UpdatePlayhead();
                    this.UpdateRuler();
                    this.UpdateZoomBar();
                }
            });
            this._trackContent.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (evt.button == 0 && this._isScrubbing)
                {
                    this._isScrubbing = false;
                    this._trackContent.ReleasePointer(evt.pointerId);
                }

                if (evt.button == 2)
                {
                    this._isPanning = false;
                }
            });

            this._section.Add(row);
        }

        private void BuildZoomBar()
        {
            var row = new VisualElement();
            row.AddToClassList("timeline-zoom-row");
            row.style.height = ZOOM_BAR_HEIGHT;

            var label = new VisualElement();
            label.AddToClassList("timeline-label-column");
            label.style.height = ZOOM_BAR_HEIGHT;
            row.Add(label);

            this._zoomBar = new VisualElement();
            this._zoomBar.AddToClassList("timeline-zoom-bar");
            row.Add(this._zoomBar);

            this._zoomThumb = new VisualElement();
            this._zoomThumb.AddToClassList("timeline-zoom-thumb");
            this._zoomBar.Add(this._zoomThumb);

            this._zoomPlayhead = new VisualElement();
            this._zoomPlayhead.AddToClassList("timeline-zoom-playhead");
            this._zoomPlayhead.style.display = DisplayStyle.None;
            this._zoomBar.Add(this._zoomPlayhead);

            // Zoom thumb dragging
            this._zoomThumb.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0)
                {
                    return;
                }

                this._isZoomDragging = true;
                this._zoomDragStartX = evt.localPosition.x;
                this._zoomThumb.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });
            this._zoomThumb.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!this._isZoomDragging || this._viewState == null)
                {
                    return;
                }

                var delta = evt.localPosition.x - this._zoomDragStartX;
                this._viewState.Pan(delta);
                this.UpdateBlockWidths();
                this.UpdateRuler();
                this.UpdateZoomBar();
            });
            this._zoomThumb.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (this._isZoomDragging)
                {
                    this._isZoomDragging = false;
                    this._zoomThumb.ReleasePointer(evt.pointerId);
                }
            });

            this._section.Add(row);
        }

        // ══════════════════════════════════════════════════════════════════
        // Registry subscriptions
        // ══════════════════════════════════════════════════════════════════

        private void SubscribeToRegistry()
        {
            var reg = this._shotController.Registry;
            this._addedSub          = reg.ShotAdded.Subscribe(_ => this.RebuildBlocks());
            this._currentChangedSub = reg.CurrentShotChanged.Subscribe(_ => this.UpdateActiveStates());
            this._removedSub        = reg.ShotRemoved.Subscribe(_ => this.RebuildBlocks());
            this._reorderedSub      = reg.Reordered.Subscribe(_ => this.RebuildBlocks());
        }

        // ══════════════════════════════════════════════════════════════════
        // Transport bar update
        // ══════════════════════════════════════════════════════════════════

        private void UpdateTransport()
        {
            var current = this._shotController.Registry.CurrentShot;

            if (current == null)
            {
                this._transportTime.text     = "00;00;00;00";
                this._transportDuration.text = "00;00;00;00";
                this._transportShot.text     = "";
                return;
            }

            // Compute shot-local time from global scrub position
            var startTime = this._shotController.Registry.GetGlobalStartTime(current.Id).Seconds;
            var localTime = Math.Max(0, this._currentGlobalTime - startTime);
            localTime     = Math.Min(localTime, current.Duration);

            this._transportTime.text     = FormatTimecode(localTime, 24);
            this._transportDuration.text = FormatTimecode(current.Duration, 24);
            this._transportShot.text     = current.Name;
        }

        private static string FormatTimecode(double seconds, int fps)
        {
            var totalFrames = (int)(seconds * fps);
            var h           = totalFrames / (fps * 3600);
            var remainder   = totalFrames % (fps * 3600);
            var m           = remainder / (fps * 60);
            remainder       = remainder % (fps * 60);
            var s           = remainder / fps;
            var f           = remainder % fps;
            return $"{h:D2};{m:D2};{s:D2};{f:D2}";
        }

        // ══════════════════════════════════════════════════════════════════
        // Ruler rendering
        // ══════════════════════════════════════════════════════════════════

        private void UpdateRuler()
        {
            if (this._viewState == null || this._rulerContent == null)
            {
                return;
            }

            // Remove old ticks (keep playhead)
            for (var i = this._rulerContent.childCount - 1; i >= 0; i--)
            {
                var child = this._rulerContent[i];

                if (child != this._rulerPlayhead)
                {
                    child.RemoveFromHierarchy();
                }
            }

            var visibleDuration = this._viewState.VisibleDuration;
            var tickInterval    = this.ComputeTickInterval(visibleDuration);
            var majorInterval   = tickInterval * 5;

            // Find first tick within view
            var firstTick = Math.Ceiling(this._viewState.ViewStart / tickInterval) * tickInterval;

            for (var t = firstTick; t <= this._viewState.ViewEnd; t += tickInterval)
            {
                if (t < 0)
                {
                    continue;
                }

                var px      = this._viewState.TimeToPixel(t);
                var isMajor = Math.Abs(t % majorInterval) < tickInterval * 0.1;
                var height  = isMajor ? RULER_HEIGHT : 10f;

                var tick = new VisualElement();
                tick.AddToClassList("timeline-ruler__tick");
                tick.style.position = Position.Absolute;
                tick.style.left     = (float)px;
                tick.style.top      = 0;
                tick.style.width    = 1;
                tick.style.height   = height;
                this._rulerContent.Add(tick);

                if (isMajor)
                {
                    var label = new Label(FormatRulerLabel(t));
                    label.AddToClassList("timeline-ruler__label");
                    label.style.position = Position.Absolute;
                    label.style.left     = (float)px + 3f;
                    label.style.top      = 4f;
                    this._rulerContent.Add(label);
                }
            }
        }

        private double ComputeTickInterval(double visibleDuration)
        {
            if (visibleDuration <= 2)
            {
                return 1.0 / 24.0; // Per frame
            }

            if (visibleDuration <= 5)
            {
                return 0.5;
            }

            if (visibleDuration <= 15)
            {
                return 1.0;
            }

            if (visibleDuration <= 40)
            {
                return 2.0;
            }

            if (visibleDuration <= 60)
            {
                return 5.0;
            }

            return 10.0;
        }

        private static string FormatRulerLabel(double seconds)
        {
            var m = (int)(seconds / 60);
            var s = seconds % 60;
            return $"{m}:{s:00.#}";
        }

        // ══════════════════════════════════════════════════════════════════
        // Zoom bar update
        // ══════════════════════════════════════════════════════════════════

        private void UpdateZoomBar()
        {
            if (this._viewState == null || this._zoomBar == null || this._zoomThumb == null)
            {
                return;
            }

            var totalDuration = this._shotController.Registry.TotalDuration;

            if (totalDuration <= 0)
            {
                totalDuration = 5.0;
            }

            var barWidth = this._zoomBar.resolvedStyle.width;

            if (float.IsNaN(barWidth) || barWidth <= 0)
            {
                return;
            }

            var startFrac = Math.Max(0, this._viewState.ViewStart / totalDuration);
            var endFrac   = Math.Min(1, this._viewState.ViewEnd / totalDuration);
            var thumbLeft = startFrac * barWidth;
            var thumbWidth = (endFrac - startFrac) * barWidth;

            if (thumbWidth < 30)
            {
                thumbWidth = 30;
            }

            this._zoomThumb.style.position = Position.Absolute;
            this._zoomThumb.style.left     = (float)thumbLeft;
            this._zoomThumb.style.width    = (float)thumbWidth;
            this._zoomThumb.style.top      = 2;
            this._zoomThumb.style.height   = 14;
        }

        // ══════════════════════════════════════════════════════════════════
        // Playhead
        // ══════════════════════════════════════════════════════════════════

        private void ScrubToPixel(float px)
        {
            if (this._viewState == null)
            {
                return;
            }

            var time = this._viewState.PixelToTime(px);
            var totalDuration = this._shotController.Registry.TotalDuration;
            this._currentGlobalTime = Math.Clamp(time, 0, totalDuration);

            // Navigate to the shot at this time
            var result = this._shotController.Registry.GetShotAtGlobalTime(
                new TimePosition(this._currentGlobalTime));

            if (result.HasValue)
            {
                var shot = result.Value.shot;

                if (shot != this._shotController.Registry.CurrentShot)
                {
                    this._shotController.Registry.SetCurrentShot(shot.Id);
                }

                // Evaluate camera at local time within the shot
                var localTime = result.Value.localTime;
                var position  = shot.EvaluateCameraPosition(localTime);
                var rotation  = shot.EvaluateCameraRotation(localTime);
                this._shotController.Registry.CurrentShot
                    .EvaluateCameraPosition(localTime);
                var cam = FindAnyObjectByType<CameraBehaviour>();

                if (cam != null)
                {
                    cam.ShotCamera.Position = position;
                    cam.ShotCamera.Rotation = rotation;
                }
            }

            this.UpdatePlayhead();
            this.UpdateTransport();
        }

        private void UpdatePlayhead()
        {
            if (this._viewState == null)
            {
                return;
            }

            var px = (float)this._viewState.TimeToPixel(this._currentGlobalTime);

            this._rulerPlayhead.style.display = DisplayStyle.Flex;
            this._rulerPlayhead.style.left    = px;

            this._trackPlayhead.style.display = DisplayStyle.Flex;
            this._trackPlayhead.style.left    = px;
        }

        // ══════════════════════════════════════════════════════════════════
        // Shot block management
        // ══════════════════════════════════════════════════════════════════

        private void RebuildBlocks()
        {
            foreach (var block in this._blocks)
            {
                block.RemoveFromHierarchy();
            }

            this._blocks.Clear();

            foreach (var boundary in this._boundaries)
            {
                boundary.RemoveFromHierarchy();
            }

            this._boundaries.Clear();

            var reg = this._shotController.Registry;

            for (var i = 0; i < reg.Shots.Count; i++)
            {
                var shot  = reg.Shots[i];
                var block = new ShotBlockElement(shot, i);

                block.RegisterCallback<PointerEnterEvent>(_ => this.ShowTooltip(block));
                block.RegisterCallback<PointerLeaveEvent>(_ => this.HideTooltip());
                block.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
                    this.PopulateShotContextMenu(evt, block));
                block.DurationClicked += this.BeginDurationEdit;

                this._shotStrip.Insert(this._shotStrip.childCount - 1, block);
                this._blocks.Add(block);

                if (i < reg.Shots.Count - 1)
                {
                    var boundaryIndex = i;
                    var boundary      = new VisualElement();
                    boundary.AddToClassList("shot-track__boundary");
                    boundary.RegisterCallback<PointerDownEvent>(evt =>
                        this.OnBoundaryPointerDown(evt, boundaryIndex));
                    this._shotStrip.Insert(this._shotStrip.childCount - 1, boundary);
                    this._boundaries.Add(boundary);
                }
            }

            this.UpdateActiveStates();
            this.UpdateBlockWidths();
            this.UpdateTotalLabel();

            if (this._viewState != null)
            {
                this._viewState.SetTotalDuration(this._shotController.Registry.TotalDuration);
            }
        }

        private void UpdateActiveStates()
        {
            var current = this._shotController.Registry.CurrentShot;

            foreach (var block in this._blocks)
            {
                block.SetActive(block.Shot == current);
            }
        }

        private void UpdateBlockWidths()
        {
            if (this._viewState == null || this._blocks.Count == 0)
            {
                return;
            }

            var boundaryIdx = 0;
            var runningTime = 0.0;

            for (var i = 0; i < this._blocks.Count; i++)
            {
                var block   = this._blocks[i];
                var shot    = block.Shot;
                var startPx = this._viewState.TimeToPixel(runningTime);
                var endPx   = this._viewState.TimeToPixel(runningTime + shot.Duration);
                var widthPx = Math.Max(endPx - startPx, 4.0);

                block.style.position = Position.Absolute;
                block.style.left     = (float)startPx;
                block.style.width    = (float)widthPx;
                block.style.top      = 0;
                block.style.bottom   = 0;

                block.Refresh();

                if (boundaryIdx < this._boundaries.Count && i < this._blocks.Count - 1)
                {
                    var boundaryPx = this._viewState.TimeToPixel(runningTime + shot.Duration);
                    this._boundaries[boundaryIdx].style.position = Position.Absolute;
                    this._boundaries[boundaryIdx].style.left     = (float)boundaryPx - BOUNDARY_HIT_WIDTH / 2f;
                    boundaryIdx++;
                }

                runningTime += shot.Duration;
            }
        }

        private void UpdateTotalLabel()
        {
            if (this._shotController == null)
            {
                return;
            }

            var total = this._shotController.Registry.TotalDuration;
            this._totalLabel.text = $"Total: {total:F1}s";
        }

        private void UpdateBottomInset()
        {
            if (this._shotController == null)
            {
                return;
            }

            var rootW = this._root?.resolvedStyle.width ?? 0f;
            var scale = 1f;

            if (rootW > 0)
            {
                scale = Screen.width / rootW;
            }

            this._shotController.SetBottomInset(SECTION_HEIGHT * scale);
        }

        // ══════════════════════════════════════════════════════════════════
        // Add Shot
        // ══════════════════════════════════════════════════════════════════

        private void OnAddShotClicked()
        {
            this._shotController.AddShot();

            if (this._viewState != null)
            {
                var endTime = this._shotController.Registry.TotalDuration;
                this._viewState.EnsureVisible(endTime);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // Shot selection & double-click
        // ══════════════════════════════════════════════════════════════════

        private void SelectShot(ShotBlockElement block)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (this._lastClickShotId == block.Shot.Id
             && now - this._lastClickTime < DOUBLE_CLICK_MS)
            {
                this.OnDoubleClick(block);
                this._lastClickShotId = null;
                return;
            }

            this._lastClickTime   = now;
            this._lastClickShotId = block.Shot.Id;
            this._shotController.Registry.SetCurrentShot(block.Shot.Id);
        }

        private void OnDoubleClick(ShotBlockElement block)
        {
            this._shotController.Registry.SetCurrentShot(block.Shot.Id);

            if (this._viewState != null)
            {
                var reg   = this._shotController.Registry;
                var start = reg.GetGlobalStartTime(block.Shot.Id).Seconds;
                var end   = reg.GetGlobalEndTime(block.Shot.Id).Seconds;
                this._viewState.FitRange(start, end);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // Delete Shot
        // ══════════════════════════════════════════════════════════════════

        private void RequestDeleteShot(ShotBlockElement block)
        {
            var skipConfirmation = PlayerPrefs.GetInt("Fram3d_SkipDeleteConfirmation", 0) == 1;

            if (skipConfirmation)
            {
                this._shotController.Registry.RemoveShot(block.Shot.Id);
                return;
            }

            this.ShowDeleteConfirmation(block);
        }

        private void ShowDeleteConfirmation(ShotBlockElement block)
        {
            var overlay = new VisualElement();
            overlay.AddToClassList("confirmation-overlay");

            var dialog = new VisualElement();
            dialog.AddToClassList("confirmation-dialog");

            var message = new Label($"Delete {block.Shot.Name}? This cannot be undone.");
            message.AddToClassList("confirmation-dialog__message");
            dialog.Add(message);

            var checkbox = new Toggle("Don't show this again");
            checkbox.AddToClassList("confirmation-dialog__checkbox");
            dialog.Add(checkbox);

            var buttons = new VisualElement();
            buttons.AddToClassList("confirmation-dialog__buttons");

            var cancelBtn = new Button(() => overlay.RemoveFromHierarchy());
            cancelBtn.text = "Cancel";
            cancelBtn.AddToClassList("confirmation-dialog__button");
            cancelBtn.AddToClassList("confirmation-dialog__button--cancel");
            buttons.Add(cancelBtn);

            var confirmBtn = new Button(() =>
            {
                if (checkbox.value)
                {
                    PlayerPrefs.SetInt("Fram3d_SkipDeleteConfirmation", 1);
                    PlayerPrefs.Save();
                }

                overlay.RemoveFromHierarchy();
                this._shotController.Registry.RemoveShot(block.Shot.Id);
            });
            confirmBtn.text = "Delete";
            confirmBtn.AddToClassList("confirmation-dialog__button");
            confirmBtn.AddToClassList("confirmation-dialog__button--confirm");
            buttons.Add(confirmBtn);

            dialog.Add(buttons);
            overlay.Add(dialog);
            this._root.Add(overlay);
        }

        // ══════════════════════════════════════════════════════════════════
        // Context menu
        // ══════════════════════════════════════════════════════════════════

        private void PopulateShotContextMenu(ContextualMenuPopulateEvent evt, ShotBlockElement block)
        {
            evt.menu.AppendAction("Delete Shot", _ => this.RequestDeleteShot(block));
            evt.menu.AppendAction("Edit Duration", _ => this.BeginDurationEdit(block));
        }

        // ══════════════════════════════════════════════════════════════════
        // Duration editing
        // ══════════════════════════════════════════════════════════════════

        private void BeginDurationEdit(ShotBlockElement block)
        {
            block.BeginDurationEdit(text =>
            {
                if (double.TryParse(text.TrimEnd('s', 'S'), out var value))
                {
                    block.Shot.Duration = value;
                }

                block.Refresh();
                this.UpdateBlockWidths();
                this.UpdateTotalLabel();

                if (this._viewState != null)
                {
                    this._viewState.FitAll(this._shotController.Registry.TotalDuration);
                }
            });
        }

        // ══════════════════════════════════════════════════════════════════
        // Shot strip pointer events
        // ══════════════════════════════════════════════════════════════════

        private void OnStripPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                this._pointerDownTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                this._pointerDownPos  = evt.localPosition;
                this._pointerIsDown   = true;

                var block = this.FindBlockAt(evt.localPosition);

                if (block != null)
                {
                    this._dragBlock         = block;
                    this._dragOriginalIndex = this._blocks.IndexOf(block);
                }
            }
            else if (evt.button == 2)
            {
                this._isPanning   = true;
                this._panStartPos = evt.localPosition;
                evt.StopPropagation();
            }
        }

        private void OnStripPointerMove(PointerMoveEvent evt)
        {
            if (this._isPanning && this._viewState != null)
            {
                var delta = evt.localPosition.x - this._panStartPos.x;
                this._viewState.Pan(-delta);
                this._panStartPos = evt.localPosition;
                this.UpdateBlockWidths();
                this.UpdatePlayhead();
                this.UpdateRuler();
                this.UpdateZoomBar();
                return;
            }

            if (this._pointerIsDown && this._dragBlock != null && !this._isDragging)
            {
                var elapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this._pointerDownTime;
                var dist    = Vector2.Distance(evt.localPosition, this._pointerDownPos);

                if (elapsed >= HOLD_THRESHOLD_MS || dist > 5f)
                {
                    this._isDragging = true;
                    this._dragBlock.style.opacity = 0.6f;
                    this._dropIndicator.style.display = DisplayStyle.Flex;
                }
            }

            if (this._isDragging)
            {
                this.UpdateDropIndicator(evt.localPosition);
            }

            if (this._isBoundaryDragging)
            {
                this.UpdateBoundaryDrag(evt.localPosition);
            }
        }

        private void OnStripPointerUp(PointerUpEvent evt)
        {
            if (evt.button == 2)
            {
                this._isPanning = false;
                return;
            }

            if (evt.button != 0)
            {
                return;
            }

            if (this._isDragging)
            {
                this.CompleteDrag();
            }
            else if (this._isBoundaryDragging)
            {
                this.CompleteBoundaryDrag();
            }
            else if (this._dragBlock != null)
            {
                this.SelectShot(this._dragBlock);
            }

            this.ResetDragState();
        }

        private void ResetDragState()
        {
            if (this._dragBlock != null)
            {
                this._dragBlock.style.opacity = new StyleFloat(StyleKeyword.Null);
            }

            this._isDragging                    = false;
            this._isBoundaryDragging            = false;
            this._dragBlock                     = null;
            this._pointerIsDown                 = false;
            this._dropIndicator.style.display   = DisplayStyle.None;
            this._boundaryTooltip.style.display = DisplayStyle.None;
        }

        // ══════════════════════════════════════════════════════════════════
        // Drag-and-drop reordering
        // ══════════════════════════════════════════════════════════════════

        private void UpdateDropIndicator(Vector2 localPos)
        {
            var targetIndex = this.FindInsertionIndex(localPos.x);
            this._dragTargetIndex = targetIndex;

            if (targetIndex >= 0 && targetIndex <= this._blocks.Count)
            {
                float indicatorX;

                if (targetIndex < this._blocks.Count)
                {
                    indicatorX = this._blocks[targetIndex].resolvedStyle.left;
                }
                else if (this._blocks.Count > 0)
                {
                    var last = this._blocks[this._blocks.Count - 1];
                    indicatorX = last.resolvedStyle.left + last.resolvedStyle.width;
                }
                else
                {
                    indicatorX = 0;
                }

                this._dropIndicator.style.left = indicatorX - 1f;
            }
        }

        private int FindInsertionIndex(float x)
        {
            for (var i = 0; i < this._blocks.Count; i++)
            {
                var block  = this._blocks[i];
                var left   = block.resolvedStyle.left;
                var center = left + block.resolvedStyle.width / 2f;

                if (x < center)
                {
                    return i;
                }
            }

            return this._blocks.Count;
        }

        private void CompleteDrag()
        {
            if (this._dragBlock == null)
            {
                return;
            }

            var fromIndex = this._dragOriginalIndex;
            var toIndex   = this._dragTargetIndex;

            if (toIndex > fromIndex)
            {
                toIndex--;
            }

            if (fromIndex != toIndex && toIndex >= 0 && toIndex < this._shotController.Registry.Count)
            {
                this._shotController.Registry.Reorder(this._dragBlock.Shot.Id, toIndex);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // Shot boundary dragging
        // ══════════════════════════════════════════════════════════════════

        private void OnBoundaryPointerDown(PointerDownEvent evt, int leftIndex)
        {
            if (evt.button != 0)
            {
                return;
            }

            this._isBoundaryDragging            = true;
            this._boundaryLeftIndex             = leftIndex;
            this._boundaryTooltip.style.display = DisplayStyle.Flex;
            evt.StopPropagation();
        }

        private void UpdateBoundaryDrag(Vector2 localPos)
        {
            if (this._viewState == null)
            {
                return;
            }

            var reg        = this._shotController.Registry;
            var leftShot   = reg.Shots[this._boundaryLeftIndex];
            var startTime  = reg.GetGlobalStartTime(leftShot.Id).Seconds;
            var cursorTime = this._viewState.PixelToTime(localPos.x);
            var newDuration = cursorTime - startTime;

            var snapped = FrameRate.FPS_24.SnapToFrame(
                new TimePosition(Math.Max(newDuration, Shot.MIN_DURATION)));
            leftShot.Duration = snapped.Seconds;

            this.UpdateBlockWidths();
            this.UpdateTotalLabel();

            var fps    = FrameRate.FPS_24;
            var frames = snapped.ToFrame(fps);
            var mode   = "[ripple]";

            if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            {
                mode = "[shots only]";
            }

            this._boundaryTooltipText.text =
                $"{leftShot.Name}: {leftShot.Duration:F1}s ({frames}f) {mode}";
        }

        private void CompleteBoundaryDrag()
        {
            this._isBoundaryDragging            = false;
            this._boundaryTooltip.style.display = DisplayStyle.None;
        }

        // ══════════════════════════════════════════════════════════════════
        // Zoom (scroll wheel) — shared across ruler, shot strip, track area
        // ══════════════════════════════════════════════════════════════════

        private void OnStripWheel(WheelEvent evt)
        {
            if (this._viewState == null)
            {
                return;
            }

            // Pick dominant scroll axis to avoid cross-contamination on trackpads
            var absX = Math.Abs(evt.delta.x);
            var absY = Math.Abs(evt.delta.y);

            if (absY > absX)
            {
                // Vertical dominant: zoom at cursor
                var cursorTime = this._viewState.PixelToTime(evt.localMousePosition.x);
                this._viewState.ZoomAtPoint(cursorTime, -evt.delta.y);
            }
            else if (absX > 0.01f)
            {
                // Horizontal dominant: pan
                this._viewState.Pan(evt.delta.x * 2.0);
            }

            this.UpdateBlockWidths();
            this.UpdateRuler();
            this.UpdateZoomBar();
            this.UpdatePlayhead();
            evt.StopPropagation();
        }

        // ══════════════════════════════════════════════════════════════════
        // Hover tooltip
        // ══════════════════════════════════════════════════════════════════

        private void ShowTooltip(ShotBlockElement block)
        {
            this._hoveredBlock = block;
            var shot    = block.Shot;
            var fps     = FrameRate.FPS_24;
            var frames  = new TimePosition(shot.Duration).ToFrame(fps);
            var kfCount = shot.TotalCameraKeyframeCount;
            this._tooltipText.text      = $"{shot.Name}\nCam A \u00b7 {shot.Duration:F1}s ({frames}f) \u00b7 {kfCount} kf";
            this._tooltip.style.display = DisplayStyle.Flex;
        }

        private void HideTooltip()
        {
            this._hoveredBlock          = null;
            this._tooltip.style.display = DisplayStyle.None;
        }

        private void UpdateTooltipPosition()
        {
            if (this._hoveredBlock == null || this._tooltip.style.display == DisplayStyle.None)
            {
                return;
            }

            if (Mouse.current == null || this._root == null || this._root.panel == null)
            {
                return;
            }

            var mousePos  = Mouse.current.position.ReadValue();
            var screenPos = new Vector2(mousePos.x, Screen.height - mousePos.y);
            var panelPos  = RuntimePanelUtils.ScreenToPanel(this._root.panel, screenPos);
            this._tooltip.style.left = panelPos.x + 12f;
            this._tooltip.style.top  = panelPos.y - 50f;
        }

        private void UpdateBoundaryTooltipPosition()
        {
            if (!this._isBoundaryDragging
             || this._boundaryTooltip.style.display == DisplayStyle.None)
            {
                return;
            }

            if (Mouse.current == null || this._root == null || this._root.panel == null)
            {
                return;
            }

            var mousePos  = Mouse.current.position.ReadValue();
            var screenPos = new Vector2(mousePos.x, Screen.height - mousePos.y);
            var panelPos  = RuntimePanelUtils.ScreenToPanel(this._root.panel, screenPos);
            this._boundaryTooltip.style.left = panelPos.x + 12f;
            this._boundaryTooltip.style.top  = panelPos.y - 30f;
        }

        // ══════════════════════════════════════════════════════════════════
        // Helpers
        // ══════════════════════════════════════════════════════════════════

        private ShotBlockElement FindBlockAt(Vector2 localPos)
        {
            foreach (var block in this._blocks)
            {
                var left  = block.resolvedStyle.left;
                var width = block.resolvedStyle.width;

                if (!float.IsNaN(left) && !float.IsNaN(width)
                 && localPos.x >= left && localPos.x <= left + width)
                {
                    return block;
                }
            }

            return null;
        }
    }
}
