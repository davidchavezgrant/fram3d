using System;
using System.Collections.Generic;
using Fram3d.Core.Common;
using Fram3d.Core.Shot;
using Fram3d.Core.Timeline;
using Fram3d.Engine.Cursor;
using Fram3d.Engine.Integration;
using Fram3d.UI.Panels;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// MonoBehaviour orchestrator for the timeline section. Delegates rendering
    /// and interaction to child components: TransportBarElement, RulerElement,
    /// ShotStripElement (inline), track area, and ZoomBarElement.
    /// </summary>
    public sealed class TimelineSectionView : MonoBehaviour
    {
        private const double DOUBLE_CLICK_MS  = 350;
        private const float  EDGE_HIT_PX      = 6f;
        private const int    FPS              = 24;
        private const double FRAME_DURATION   = 1.0 / FPS;
        private const int    HOLD_THRESHOLD_MS = 200;
        private const float  LABEL_COLUMN_WIDTH = 140f;
        private const float  SECTION_HEIGHT   = 320f;

        // ── References ──
        private CameraBehaviour _cameraBehaviour;
        private Playhead        _playhead;
        private ShotController  _shotController;
        private TimelineState   _timelineState;

        // ── UI root ──
        private VisualElement _root;
        private VisualElement _section;

        // ── Child components ──
        private TransportBarElement _transport;
        private RulerElement        _ruler;
        private ZoomBarElement      _zoomBar;

        // ── Shot strip (inline until further decomposition) ──
        private readonly List<ShotBlockElement> _blocks = new();
        private VisualElement _shotLabelColumn;
        private VisualElement _shotStrip;
        private VisualElement _shotStripOutOfRange;
        private VisualElement _shotStripPlayhead;
        private VisualElement _dropIndicator;
        private Label         _totalLabel;

        // ── Track area ──
        private VisualElement _trackContent;
        private VisualElement _trackOutOfRange;
        private VisualElement _trackPlayhead;

        // ── Tooltips ──
        private VisualElement _tooltip;
        private Label         _tooltipText;
        private VisualElement _boundaryTooltip;
        private Label         _boundaryTooltipText;

        // ── Subscriptions ──
        private IDisposable _addedSub;
        private IDisposable _currentChangedSub;
        private IDisposable _removedSub;
        private IDisposable _reorderedSub;

        // ── Drag state ──
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

        // ── Tooltip state ──
        private ShotBlockElement _hoveredBlock;

        // ── Visibility ──
        private bool _visible = true;

        // ══════════════════════════════════════════════════════════════════
        // Public API
        // ══════════════════════════════════════════════════════════════════

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
                return this._root.panel.Pick(panelPos) != null;
            }
        }

        public bool IsVisible => this._visible;

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

        public void TogglePlayback()
        {
            var totalDuration = this._shotController.Registry.TotalDuration;
            var wasAtEnd      = this._playhead.CurrentTime >= totalDuration - this._playhead.FrameRate.FrameDuration;
            var isNowPlaying  = this._playhead.TogglePlayback(totalDuration);

            if (isNowPlaying && wasAtEnd && this._timelineState != null)
            {
                var duration = this._timelineState.VisibleDuration;
                this._timelineState.SetViewRange(0, duration);
                this.RefreshAll();
            }

            this._transport.UpdatePlayButton(this._playhead.IsPlaying);
        }

        public void ZoomIn()
        {
            if (this._timelineState != null)
            {
                this._timelineState.ZoomAtPoint(this._playhead.CurrentTime, 1f);
                this.RefreshAll();
            }
        }

        public void ZoomOut()
        {
            if (this._timelineState != null)
            {
                this._timelineState.ZoomAtPoint(this._playhead.CurrentTime, -1f);
                this.RefreshAll();
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
            this._shotController  = FindAnyObjectByType<ShotController>();
            this._cameraBehaviour = FindAnyObjectByType<CameraBehaviour>();
            this._playhead        = new Playhead(FrameRate.FPS_24);

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

            this.HandlePlayback();
            this.HandleInputSystemScroll();
            this.UpdateShotBlockWidths();

            if (this._timelineState != null)
            {
                var totalDuration = this._shotController.Registry.TotalDuration;
                this._ruler.UpdateTicks(this._timelineState, totalDuration);
                this._ruler.UpdatePlayhead(this._timelineState, this._playhead.CurrentTime);
                this._ruler.UpdateOutOfRange(this._timelineState, totalDuration);
                this.UpdateShotStripPlayhead();
                this.UpdateTrackAreaPlayhead();
                this._zoomBar.UpdateThumb(this._timelineState, totalDuration);
            }

            this._transport.UpdateTransport(this._playhead, this._shotController.Registry);
            this.UpdateTotalLabel();
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

            // Transport bar
            this._transport = new TransportBarElement(this.TogglePlayback);
            this._section.Add(this._transport);

            // Ruler
            this._ruler = new RulerElement();
            this._ruler.ScrubRequested += this.ScrubToPixel;
            this._ruler.RegisterScrubCallbacks();
            this._ruler.Content.RegisterCallback<WheelEvent>(this.OnWheel);
            this._section.Add(this._ruler);

            // Shot track
            this.BuildShotTrack();

            // Track area
            this.BuildTrackArea();

            // Zoom bar
            this._zoomBar = new ZoomBarElement();
            this._zoomBar.PanRequested += this.OnZoomBarPan;
            this._zoomBar.RegisterDragCallbacks();
            this._section.Add(this._zoomBar);

            // Tooltips
            this.BuildTooltips();

            this._root.Add(this._section);

            // Initialize timeline state after strip has geometry
            this._shotStrip.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                var stripWidth = this._shotStrip.resolvedStyle.width;

                if (float.IsNaN(stripWidth) || stripWidth <= 0)
                {
                    return;
                }

                if (this._timelineState == null)
                {
                    this._timelineState = new TimelineState(
                        this._shotController.Registry.TotalDuration,
                        stripWidth);
                }
                else
                {
                    this._timelineState.SetStripWidth(stripWidth);
                }

                this.RefreshAll();
                this.UpdateBottomInset();
            });
        }

        private void BuildShotTrack()
        {
            var row = new VisualElement();
            row.AddToClassList("timeline-shot-row");

            this._shotLabelColumn = new VisualElement();
            this._shotLabelColumn.AddToClassList("timeline-label-column");

            var titleRow = new VisualElement();
            titleRow.style.flexDirection = FlexDirection.Row;
            titleRow.style.alignItems    = Align.Center;

            var shotLabel = new Label("SHOTS");
            shotLabel.AddToClassList("timeline-label-column__title");
            titleRow.Add(shotLabel);

            var addButton = new Button(this.OnAddShotClicked);
            addButton.text = "+";
            addButton.AddToClassList("timeline-shot__add-button");
            titleRow.Add(addButton);

            this._shotLabelColumn.Add(titleRow);

            this._totalLabel = new Label("Total: 0.0s");
            this._totalLabel.AddToClassList("timeline-label-column__subtitle");
            this._shotLabelColumn.Add(this._totalLabel);

            row.Add(this._shotLabelColumn);

            this._shotStrip = new VisualElement();
            this._shotStrip.AddToClassList("timeline-shot-strip");
            row.Add(this._shotStrip);

            this._shotStrip.RegisterCallback<WheelEvent>(this.OnWheel);
            this._shotStrip.RegisterCallback<PointerDownEvent>(this.OnShotStripPointerDown);
            this._shotStrip.RegisterCallback<PointerMoveEvent>(this.OnShotStripPointerMove);
            this._shotStrip.RegisterCallback<PointerUpEvent>(this.OnShotStripPointerUp);

            this._dropIndicator = new VisualElement();
            this._dropIndicator.AddToClassList("shot-track__drop-indicator");
            this._dropIndicator.style.display = DisplayStyle.None;
            this._shotStrip.Add(this._dropIndicator);

            this._shotStripPlayhead = new VisualElement();
            this._shotStripPlayhead.AddToClassList("timeline-playhead");
            this._shotStripPlayhead.style.display = DisplayStyle.None;
            this._shotStrip.Add(this._shotStripPlayhead);

            this._shotStripOutOfRange = new VisualElement();
            this._shotStripOutOfRange.AddToClassList("timeline-out-of-range");
            this._shotStripOutOfRange.pickingMode = PickingMode.Ignore;
            this._shotStrip.Add(this._shotStripOutOfRange);

            this._section.Add(row);
        }

        private void BuildTrackArea()
        {
            var row = new VisualElement();
            row.AddToClassList("timeline-track-row");

            var trackLabels = new VisualElement();
            trackLabels.AddToClassList("timeline-label-column");
            row.Add(trackLabels);

            this._trackContent = new VisualElement();
            this._trackContent.AddToClassList("timeline-track-content");
            row.Add(this._trackContent);

            this._trackPlayhead = new VisualElement();
            this._trackPlayhead.AddToClassList("timeline-playhead");
            this._trackPlayhead.style.display = DisplayStyle.None;
            this._trackContent.Add(this._trackPlayhead);

            this._trackOutOfRange = new VisualElement();
            this._trackOutOfRange.AddToClassList("timeline-out-of-range");
            this._trackOutOfRange.pickingMode = PickingMode.Ignore;
            this._trackContent.Add(this._trackOutOfRange);

            this._trackContent.RegisterCallback<WheelEvent>(this.OnWheel);
            this._trackContent.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 2)
                {
                    this._isPanning   = true;
                    this._panStartPos = evt.localPosition;
                    evt.StopPropagation();
                }
            });
            this._trackContent.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (this._isPanning && this._timelineState != null)
                {
                    var delta = evt.localPosition.x - this._panStartPos.x;
                    this._timelineState.Pan(-delta);
                    this._panStartPos = evt.localPosition;
                    this.RefreshAll();
                }
            });
            this._trackContent.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (evt.button == 2)
                {
                    this._isPanning = false;
                }
            });

            this._section.Add(row);
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

        // ══════════════════════════════════════════════════════════════════
        // Playback
        // ══════════════════════════════════════════════════════════════════

        private void HandlePlayback()
        {
            var totalDuration = this._shotController.Registry.TotalDuration;
            var stillPlaying  = this._playhead.Advance(Time.deltaTime, totalDuration);

            if (!stillPlaying)
            {
                this._transport.UpdatePlayButton(this._playhead.IsPlaying);
                return;
            }

            this.EvaluateCameraAtPlayhead();

            if (this._timelineState != null
             && this._playhead.CurrentTime > this._timelineState.ViewEnd)
            {
                var duration = this._timelineState.VisibleDuration;
                this._timelineState.SetViewRange(this._timelineState.ViewEnd, this._timelineState.ViewEnd + duration);
            }
        }

        private void EvaluateCameraAtPlayhead()
        {
            var result = this._playhead.ResolveShot(this._shotController.Registry);

            if (!result.HasValue)
            {
                return;
            }

            var shot = result.Value.shot;

            if (shot != this._shotController.Registry.CurrentShot)
            {
                this._shotController.Registry.SetCurrentShot(shot.Id);
            }

            var localTime = result.Value.localTime;
            var position  = shot.EvaluateCameraPosition(localTime);
            var rotation  = shot.EvaluateCameraRotation(localTime);

            if (this._cameraBehaviour != null)
            {
                this._cameraBehaviour.ShotCamera.Position = position;
                this._cameraBehaviour.ShotCamera.Rotation = rotation;
            }
        }

        private void HandleInputSystemScroll()
        {
            if (this._timelineState == null || !this.IsPointerOverUI || Mouse.current == null)
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
            var stripX    = panelPos.x - LABEL_COLUMN_WIDTH;
            var cursorTime = this._timelineState.PixelToTime(stripX);

            this._timelineState.ZoomAtPoint(cursorTime, scroll.y);
            this.RefreshAll();
        }

        // ══════════════════════════════════════════════════════════════════
        // Scrub
        // ══════════════════════════════════════════════════════════════════

        private void ScrubToPixel(float px)
        {
            if (this._timelineState == null)
            {
                return;
            }

            var rawTime       = this._timelineState.PixelToTime(px);
            var totalDuration = this._shotController.Registry.TotalDuration;
            this._playhead.Scrub(rawTime, totalDuration);
            this.EvaluateCameraAtPlayhead();

            var scrollTime = Math.Clamp(rawTime, 0, totalDuration);
            this._timelineState.EnsureVisible(scrollTime);
            this.RefreshAll();
        }

        // ══════════════════════════════════════════════════════════════════
        // Zoom / Pan (shared wheel handler)
        // ══════════════════════════════════════════════════════════════════

        private void OnWheel(WheelEvent evt)
        {
            if (this._timelineState == null)
            {
                return;
            }

            if (evt.ctrlKey)
            {
                var cursorTime = this._timelineState.PixelToTime(evt.localMousePosition.x);
                this._timelineState.ZoomAtPoint(cursorTime, -evt.delta.y);
            }
            else
            {
                var absX = Math.Abs(evt.delta.x);
                var absY = Math.Abs(evt.delta.y);

                if (absY > absX)
                {
                    var cursorTime = this._timelineState.PixelToTime(evt.localMousePosition.x);
                    this._timelineState.ZoomAtPoint(cursorTime, -evt.delta.y);
                }
                else if (absX > 0.01f)
                {
                    this._timelineState.Pan(evt.delta.x * 2.0);
                }
            }

            this.RefreshAll();
            evt.StopPropagation();
        }

        private void OnZoomBarPan(float delta)
        {
            if (this._timelineState != null)
            {
                this._timelineState.Pan(delta);
                this.RefreshAll();
            }
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
        // Shot blocks
        // ══════════════════════════════════════════════════════════════════

        private void RebuildBlocks()
        {
            foreach (var block in this._blocks)
            {
                block.RemoveFromHierarchy();
            }

            this._blocks.Clear();

            var reg = this._shotController.Registry;

            for (var i = 0; i < reg.Shots.Count; i++)
            {
                var shot  = reg.Shots[i];
                var block = new ShotBlockElement(shot, i);

                block.RegisterCallback<PointerEnterEvent>(_ => this.ShowTooltip(block));
                block.RegisterCallback<PointerLeaveEvent>(_ => this.HideTooltip());
                block.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
                {
                    evt.menu.AppendAction("Delete Shot", _ => this.RequestDeleteShot(block));
                    evt.menu.AppendAction("Edit Duration", _ => this.BeginDurationEdit(block));
                });
                block.DurationClicked += this.BeginDurationEdit;

                this._shotStrip.Insert(this._shotStrip.childCount - 1, block);
                this._blocks.Add(block);
            }

            this._shotStripPlayhead.BringToFront();
            this._shotStripOutOfRange.BringToFront();

            this.UpdateActiveStates();
            this.UpdateShotBlockWidths();
            this.UpdateTotalLabel();

            if (this._timelineState != null)
            {
                this._timelineState.SetTotalDuration(this._shotController.Registry.TotalDuration);
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

        private void UpdateShotBlockWidths()
        {
            if (this._timelineState == null || this._blocks.Count == 0)
            {
                return;
            }

            var runningTime = 0.0;

            for (var i = 0; i < this._blocks.Count; i++)
            {
                var block   = this._blocks[i];
                var shot    = block.Shot;
                var startPx = this._timelineState.TimeToPixel(runningTime);
                var endPx   = this._timelineState.TimeToPixel(runningTime + shot.Duration);
                var widthPx = Math.Max(endPx - startPx, 4.0);

                block.style.position = Position.Absolute;
                block.style.left     = (float)startPx;
                block.style.width    = (float)widthPx;
                block.style.top      = 0;
                block.style.bottom   = 0;

                block.Refresh();
                runningTime += shot.Duration;
            }
        }

        private void UpdateShotStripPlayhead()
        {
            if (this._timelineState == null)
            {
                return;
            }

            var px = (float)this._timelineState.TimeToPixel(this._playhead.CurrentTime);
            this._shotStripPlayhead.style.display = DisplayStyle.Flex;
            this._shotStripPlayhead.style.left    = px;

            var totalDuration = this._shotController.Registry.TotalDuration;
            var endPx         = (float)this._timelineState.TimeToPixel(totalDuration);
            this._shotStripOutOfRange.style.position = Position.Absolute;
            this._shotStripOutOfRange.style.left     = endPx;
            this._shotStripOutOfRange.style.top      = 0;
            this._shotStripOutOfRange.style.bottom   = 0;
            this._shotStripOutOfRange.style.right    = 0;
            this._shotStripOutOfRange.style.display  = endPx >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateTrackAreaPlayhead()
        {
            if (this._timelineState == null)
            {
                return;
            }

            var px = (float)this._timelineState.TimeToPixel(this._playhead.CurrentTime);
            this._trackPlayhead.style.display = DisplayStyle.Flex;
            this._trackPlayhead.style.left    = px;

            var totalDuration = this._shotController.Registry.TotalDuration;
            var endPx         = (float)this._timelineState.TimeToPixel(totalDuration);
            this._trackOutOfRange.style.position = Position.Absolute;
            this._trackOutOfRange.style.left     = endPx;
            this._trackOutOfRange.style.top      = 0;
            this._trackOutOfRange.style.bottom   = 0;
            this._trackOutOfRange.style.right    = 0;
            this._trackOutOfRange.style.display  = endPx >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateTotalLabel()
        {
            if (this._shotController == null)
            {
                return;
            }

            this._totalLabel.text = $"Total: {this._shotController.Registry.TotalDuration:F1}s";
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
        // Add / Delete / Duration
        // ══════════════════════════════════════════════════════════════════

        private void OnAddShotClicked()
        {
            this._shotController.AddShot();

            if (this._timelineState != null)
            {
                this._timelineState.SetTotalDuration(this._shotController.Registry.TotalDuration);
                this._timelineState.FitAll(this._shotController.Registry.TotalDuration);
                this.RefreshAll();
            }
        }

        private void RequestDeleteShot(ShotBlockElement block)
        {
            var skipConfirmation = PlayerPrefs.GetInt("Fram3d_SkipDeleteConfirmation", 0) == 1;

            if (skipConfirmation)
            {
                this._shotController.Registry.RemoveShot(block.Shot.Id);
                return;
            }

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

        private void BeginDurationEdit(ShotBlockElement block)
        {
            block.BeginDurationEdit(text =>
            {
                if (double.TryParse(text.TrimEnd('s', 'S'), out var value))
                {
                    block.Shot.Duration = value;
                }

                block.Refresh();
                this.UpdateShotBlockWidths();
                this.UpdateTotalLabel();

                if (this._timelineState != null)
                {
                    this._timelineState.FitAll(this._shotController.Registry.TotalDuration);
                }
            });
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

            if (this._timelineState == null)
            {
                return;
            }

            var reg           = this._shotController.Registry;
            var shotIndex     = reg.IndexOf(block.Shot.Id);
            var start         = reg.GetGlobalStartTime(block.Shot.Id).Seconds;
            var end           = reg.GetGlobalEndTime(block.Shot.Id).Seconds;
            var duration      = end - start;
            var totalDuration = reg.TotalDuration;
            var padding       = duration * 0.08;
            var isFirst       = shotIndex == 0;
            var isLast        = shotIndex == reg.Count - 1;

            if (reg.Count == 1)
            {
                this._timelineState.FitAll(totalDuration);
            }
            else if (isFirst)
            {
                this._timelineState.SetViewRange(0, end + padding);
            }
            else if (isLast)
            {
                this._timelineState.SetViewRange(start - padding, totalDuration);
            }
            else
            {
                this._timelineState.FitRange(start, end);
            }

            this.RefreshAll();
        }

        // ══════════════════════════════════════════════════════════════════
        // Shot strip pointer events
        // ══════════════════════════════════════════════════════════════════

        private void OnShotStripPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                var edgeIndex = this.FindShotEdgeAt(evt.localPosition.x);

                if (edgeIndex >= 0)
                {
                    this._isBoundaryDragging            = true;
                    this._boundaryLeftIndex              = edgeIndex;
                    this._boundaryTooltip.style.display  = DisplayStyle.Flex;
                    this._shotStrip.CapturePointer(evt.pointerId);
                    evt.StopPropagation();
                    return;
                }

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

        private void OnShotStripPointerMove(PointerMoveEvent evt)
        {
            if (this._isPanning && this._timelineState != null)
            {
                var delta = evt.localPosition.x - this._panStartPos.x;
                this._timelineState.Pan(-delta);
                this._panStartPos = evt.localPosition;
                this.RefreshAll();
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
                return;
            }

            if (this._isBoundaryDragging)
            {
                this.UpdateBoundaryDrag(evt.localPosition);
                return;
            }

            // Resize cursor near shot edges
            if (!this._pointerIsDown)
            {
                var edgeIndex = this.FindShotEdgeAt(evt.localPosition.x);

                if (edgeIndex >= 0)
                {
                    CursorManager.SetCursor(CursorType.ResizeHorizontal);
                }
                else
                {
                    CursorManager.ResetCursor();
                }
            }
        }

        private void OnShotStripPointerUp(PointerUpEvent evt)
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
                this._isBoundaryDragging            = false;
                this._boundaryTooltip.style.display = DisplayStyle.None;
                CursorManager.ResetCursor();
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

            if (this._shotStrip.HasPointerCapture(0))
            {
                this._shotStrip.ReleasePointer(0);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // Drag reorder
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
        // Boundary drag
        // ══════════════════════════════════════════════════════════════════

        private void UpdateBoundaryDrag(Vector2 localPos)
        {
            if (this._timelineState == null)
            {
                return;
            }

            var reg        = this._shotController.Registry;
            var leftShot   = reg.Shots[this._boundaryLeftIndex];
            var startTime  = reg.GetGlobalStartTime(leftShot.Id).Seconds;
            var cursorTime = this._timelineState.PixelToTime(localPos.x);
            var newDuration = cursorTime - startTime;

            var snapped = FrameRate.FPS_24.SnapToFrame(
                new TimePosition(Math.Max(newDuration, Shot.MIN_DURATION)));
            leftShot.Duration = snapped.Seconds;

            this.UpdateShotBlockWidths();
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

        // ══════════════════════════════════════════════════════════════════
        // Tooltips
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

            if (Mouse.current == null || this._root?.panel == null)
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
            if (!this._isBoundaryDragging || this._boundaryTooltip.style.display == DisplayStyle.None)
            {
                return;
            }

            if (Mouse.current == null || this._root?.panel == null)
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

        private void RefreshAll()
        {
            this.UpdateShotBlockWidths();
        }

        private int FindShotEdgeAt(float x)
        {
            if (this._timelineState == null)
            {
                return -1;
            }

            var runningTime = 0.0;

            for (var i = 0; i < this._blocks.Count; i++)
            {
                runningTime += this._blocks[i].Shot.Duration;
                var edgePx = (float)this._timelineState.TimeToPixel(runningTime);

                if (Math.Abs(x - edgePx) <= EDGE_HIT_PX)
                {
                    return i;
                }
            }

            return -1;
        }

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
