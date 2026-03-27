using System;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Fram3d.Engine.Cursor;
using Fram3d.Engine.Integration;
using Fram3d.UI.Panels;
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
        private const float EDGE_HIT_PX    = 6f;
        private const float LABEL_COL_W    = 140f;
        private const float SECTION_HEIGHT = 320f;

        // ── References ──
        private Fram3d.Core.Timelines.Timeline _controller;
        private ShotController     _shotController;

        // ── UI root ──
        private VisualElement _root;
        private VisualElement _section;

        // ── Child components ──
        private TransportBarElement _transport;
        private RulerElement        _ruler;
        private ZoomBarElement      _zoomBar;

        // ── Shot strip elements ──
        private VisualElement _shotStrip;
        private VisualElement _shotStripPlayhead;
        private VisualElement _shotStripOutOfRange;
        private VisualElement _dropIndicator;
        private Label         _totalLabel;

        // ── Track area ──
        private VisualElement _trackContent;
        private VisualElement _trackPlayhead;
        private VisualElement _trackOutOfRange;

        // ── Tooltips ──
        private VisualElement _tooltip;
        private Label         _tooltipText;
        private VisualElement _boundaryTooltip;
        private Label         _boundaryTooltipText;

        // ── Visibility ──
        private bool _visible = true;

        // ══════════════════════════════════════════════════════════════════
        // Public API (called by keyboard router)
        // ══════════════════════════════════════════════════════════════════

        public bool HasFocusedTextField => false; // TODO: check inline duration edits

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
            this._shotController = FindAnyObjectByType<ShotController>();

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
            this._controller.ShotAdded.Subscribe(_ => this.RebuildShotBlocks());
            this._controller.ShotRemoved.Subscribe(_ => this.RebuildShotBlocks());
            this._controller.Reordered.Subscribe(_ => this.RebuildShotBlocks());
            this._controller.CurrentShotChanged.Subscribe(_ => this.UpdateActiveStates());

            this.RebuildShotBlocks();
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

            this._transport = new TransportBarElement(this.TogglePlayback);
            this._section.Add(this._transport);

            this._ruler = new RulerElement();
            this._ruler.ScrubRequested += this.OnScrub;
            this._ruler.RegisterScrubCallbacks();
            this._ruler.Content.RegisterCallback<WheelEvent>(this.OnWheel);
            this._section.Add(this._ruler);

            this.BuildShotStrip();
            this.BuildTrackArea();

            this._zoomBar = new ZoomBarElement();
            this._zoomBar.PanRequested += px => { this._controller.Pan(px); };
            this._zoomBar.RegisterDragCallbacks();
            this._section.Add(this._zoomBar);

            this.BuildTooltips();
            this._root.Add(this._section);

            this._shotStrip.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                var w = this._shotStrip.resolvedStyle.width;

                if (!float.IsNaN(w) && w > 0)
                {
                    this._controller.InitializeViewRange(w);
                    this.SyncVisuals();
                    this.UpdateBottomInset();
                }
            });
        }

        private void BuildShotStrip()
        {
            var row = new VisualElement();
            row.AddToClassList("timeline-shot-row");

            var labelCol = new VisualElement();
            labelCol.AddToClassList("timeline-label-column");

            var titleRow = new VisualElement();
            titleRow.style.flexDirection = FlexDirection.Row;
            titleRow.style.alignItems    = Align.Center;

            var title = new Label("SHOTS");
            title.AddToClassList("timeline-label-column__title");
            titleRow.Add(title);

            var addBtn = new Button(() =>
            {
                var cam = FindAnyObjectByType<CameraBehaviour>();

                if (cam != null)
                {
                    this._controller.AddShot(cam.ShotCamera.Position, cam.ShotCamera.Rotation);
                }
            });
            addBtn.text = "+";
            addBtn.AddToClassList("timeline-shot__add-button");
            titleRow.Add(addBtn);
            labelCol.Add(titleRow);

            this._totalLabel = new Label("Total: 0.0s");
            this._totalLabel.AddToClassList("timeline-label-column__subtitle");
            labelCol.Add(this._totalLabel);
            row.Add(labelCol);

            this._shotStrip = new VisualElement();
            this._shotStrip.AddToClassList("timeline-shot-strip");
            row.Add(this._shotStrip);

            this._shotStrip.RegisterCallback<WheelEvent>(this.OnWheel);
            this._shotStrip.RegisterCallback<PointerDownEvent>(this.OnStripDown);
            this._shotStrip.RegisterCallback<PointerMoveEvent>(this.OnStripMove);
            this._shotStrip.RegisterCallback<PointerUpEvent>(this.OnStripUp);

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

            var labels = new VisualElement();
            labels.AddToClassList("timeline-label-column");
            row.Add(labels);

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
        // Visual sync (reads controller state, positions elements)
        // ══════════════════════════════════════════════════════════════════

        private void SyncVisuals()
        {
            var state = this._controller;

            if (state == null)
            {
                return;
            }

            var total = this._controller.TotalDuration;
            var px    = (float)this._controller.PlayheadPixel;
            var endPx = (float)this._controller.OutOfRangeStartPixel;

            // Shot blocks
            this.UpdateShotBlockPositions();

            // Playheads
            this.SetPlayhead(this._shotStripPlayhead, px);
            this.SetPlayhead(this._trackPlayhead, px);
            this._ruler.UpdatePlayhead(state, this._controller.Playhead.CurrentTime);

            // Out-of-range
            this.SetOutOfRange(this._shotStripOutOfRange, endPx);
            this.SetOutOfRange(this._trackOutOfRange, endPx);
            this._ruler.UpdateOutOfRange(state, total);

            // Ruler + zoom + transport
            this._ruler.UpdateTicks(state, total);
            this._zoomBar.UpdateThumb(state, total);
            this._transport.UpdateTransport(
                this._controller.Playhead,
                this._controller);
            this._totalLabel.text = $"Total: {total:F1}s";

            // Drop indicator
            if (this._controller.IsDragging)
            {
                this._dropIndicator.style.display = DisplayStyle.Flex;
                var targetPx = this.ComputeDropIndicatorPx();
                this._dropIndicator.style.left = targetPx;
            }

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

        private void SetPlayhead(VisualElement el, float px)
        {
            el.style.display = DisplayStyle.Flex;
            el.style.left    = px;
        }

        private void SetOutOfRange(VisualElement el, float endPx)
        {
            el.style.position = Position.Absolute;
            el.style.left     = endPx;
            el.style.top      = 0;
            el.style.bottom   = 0;
            el.style.right    = 0;
            el.style.display  = endPx >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // ══════════════════════════════════════════════════════════════════
        // Shot blocks
        // ══════════════════════════════════════════════════════════════════

        private void RebuildShotBlocks()
        {
            // Remove all shot block children (keep playhead, out-of-range, drop indicator)
            for (var i = this._shotStrip.childCount - 1; i >= 0; i--)
            {
                var child = this._shotStrip[i];

                if (child != this._dropIndicator
                 && child != this._shotStripPlayhead
                 && child != this._shotStripOutOfRange)
                {
                    child.RemoveFromHierarchy();
                }
            }

            var shots = this._controller.Shots;

            for (var i = 0; i < shots.Count; i++)
            {
                var shot  = shots[i];
                var block = new ShotBlockElement(shot, i);

                block.RegisterCallback<PointerEnterEvent>(_ =>
                {
                    this._tooltipText.text      = this._controller.FormatShotTooltip(shot);
                    this._tooltip.style.display = DisplayStyle.Flex;
                });
                block.RegisterCallback<PointerLeaveEvent>(_ =>
                    this._tooltip.style.display = DisplayStyle.None);
                block.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
                {
                    evt.menu.AppendAction("Delete Shot", _ =>
                        this._controller.RemoveShot(shot.Id));
                });

                this._shotStrip.Insert(this._shotStrip.childCount - 3, block);
            }

            this._shotStripPlayhead.BringToFront();
            this._shotStripOutOfRange.BringToFront();

            this.UpdateActiveStates();
        }

        private void UpdateActiveStates()
        {
            var current = this._controller.CurrentShot;

            for (var i = 0; i < this._shotStrip.childCount; i++)
            {
                if (this._shotStrip[i] is ShotBlockElement block)
                {
                    block.SetActive(block.Shot == current);
                }
            }
        }

        private void UpdateShotBlockPositions()
        {
            var state = this._controller;

            if (state == null)
            {
                return;
            }

            var runningTime = 0.0;

            for (var i = 0; i < this._shotStrip.childCount; i++)
            {
                if (this._shotStrip[i] is not ShotBlockElement block)
                {
                    continue;
                }

                var startPx = state.TimeToPixel(runningTime);
                var endPx   = state.TimeToPixel(runningTime + block.Shot.Duration);
                var widthPx = Math.Max(endPx - startPx, 4.0);

                block.style.position = Position.Absolute;
                block.style.left     = (float)startPx;
                block.style.width    = (float)widthPx;
                block.style.top      = 0;
                block.style.bottom   = 0;
                block.Refresh();

                runningTime += block.Shot.Duration;
            }
        }

        private float ComputeDropIndicatorPx()
        {
            var targetIndex = this._controller.DragTargetIndex;
            var state       = this._controller;

            if (state == null)
            {
                return 0;
            }

            var runningTime = 0.0;
            var shots       = this._controller.Shots;

            for (var i = 0; i < targetIndex && i < shots.Count; i++)
            {
                runningTime += shots[i].Duration;
            }

            return (float)state.TimeToPixel(runningTime);
        }

        // ══════════════════════════════════════════════════════════════════
        // Event forwarding → Controller
        // ══════════════════════════════════════════════════════════════════

        private void OnScrub(float px)
        {
            this._controller.BeginScrub();
            this._controller.ScrubToPixel(px);
        }

        private void OnStripDown(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                var now    = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var result = this._controller.StripPointerDown(evt.localPosition.x, now);

                if (result == StripInteraction.BOUNDARY_DRAG)
                {
                    this._boundaryTooltip.style.display = DisplayStyle.Flex;
                    this._shotStrip.CapturePointer(evt.pointerId);
                    evt.StopPropagation();
                }
            }
            else if (evt.button == 2)
            {
                evt.StopPropagation();
            }
        }

        private void OnStripMove(PointerMoveEvent evt)
        {
            var now    = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var result = this._controller.StripPointerMove(evt.localPosition.x, now);

            if (result == StripInteraction.NEAR_EDGE)
            {
                CursorManager.SetCursor(CursorType.ResizeHorizontal);
            }
            else if (result == StripInteraction.NONE && !this._controller.IsBoundaryDragging)
            {
                CursorManager.ResetCursor();
            }
        }

        private void OnStripUp(PointerUpEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            var result = this._controller.StripPointerUp();

            if (result == StripInteraction.BOUNDARY_COMPLETE)
            {
                this._boundaryTooltip.style.display = DisplayStyle.None;
                CursorManager.ResetCursor();
            }

            if (result == StripInteraction.DRAG_COMPLETE)
            {
                this._dropIndicator.style.display = DisplayStyle.None;
            }

            if (this._shotStrip.HasPointerCapture(evt.pointerId))
            {
                this._shotStrip.ReleasePointer(evt.pointerId);
            }
        }

        private void OnWheel(WheelEvent evt)
        {
            if (this._controller.VisibleDuration <= 0)
            {
                return;
            }

            if (evt.ctrlKey)
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
    }
}
