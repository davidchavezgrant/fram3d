using System;
using Fram3d.Core.Common;
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
        private const float DEFAULT_SECTION_HEIGHT = 320f;
        private const float LABEL_COL_W           = 140f;
        private const float MAX_SECTION_HEIGHT     = 2000f;
        private const float MIN_SECTION_HEIGHT     = 80f;
        private const float RESIZE_HANDLE_HEIGHT   = 5f;

        // ── References ──
        private Fram3d.Core.Timelines.Timeline _controller;
        private ShotEvaluator                  _shotController;

        // ── UI root ──
        private VisualElement _root;
        private VisualElement _section;

        // ── Child components ──
        private Ruler          _ruler;
        private ShotTrackStrip _shotTrackStrip;
        private StatusBar      _statusBar;
        private TransportBar   _transport;
        private ZoomBar        _zoomBar;

        // ── Track area ──
        private VisualElement _trackContent;
        private VisualElement _trackOutOfRange;
        private VisualElement _trackPlayhead;

        // ── Tooltips ──
        private VisualElement _boundaryTooltip;
        private Label         _boundaryTooltipText;
        private VisualElement _tooltip;
        private Label         _tooltipText;

        // ── Resize ──
        private bool  _isResizing;
        private float _resizeStartY;
        private float _resizeStartHeight;
        private float _sectionHeight = DEFAULT_SECTION_HEIGHT;

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

            this._shotController?.SetBottomInset(this._visible ? this._sectionHeight + RESIZE_HANDLE_HEIGHT : 0f);
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
            this.BuildResizeHandle();

            this._section = new VisualElement();
            this._section.AddToClassList("timeline-section");
            this._section.style.height = this._sectionHeight;

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

            this._statusBar = new StatusBar();
            this._section.Add(this._statusBar);

            this.BuildTooltips();
            this._root.Add(this._section);
        }

        private void BuildResizeHandle()
        {
            var handle = new VisualElement();
            handle.AddToClassList("timeline-resize-handle");
            handle.style.height = RESIZE_HANDLE_HEIGHT;
            handle.style.cursor = new Cursor { defaultCursorId = (int)CursorType.ResizeVertical };

            handle.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0)
                {
                    return;
                }

                this._isResizing        = true;
                this._resizeStartY      = evt.position.y;
                this._resizeStartHeight = this._sectionHeight;
                handle.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });

            handle.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!this._isResizing)
                {
                    return;
                }

                var delta     = this._resizeStartY - evt.position.y;
                var maxHeight = Mathf.Min(MAX_SECTION_HEIGHT, Screen.height * 0.8f);
                this._sectionHeight         = Mathf.Clamp(this._resizeStartHeight + delta, MIN_SECTION_HEIGHT, maxHeight);
                this._section.style.height  = this._sectionHeight;
                this.UpdateBottomInset();
            });

            handle.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (!this._isResizing)
                {
                    return;
                }

                this._isResizing = false;
                handle.ReleasePointer(evt.pointerId);
            });

            this._root.Add(handle);
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
            el.style.left     = endPx;
            el.style.top      = 0;
            el.style.bottom   = 0;
            el.style.right    = 0;
            el.style.display  = endPx >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetPlayhead(VisualElement el, float px)
        {
            el.style.display = DisplayStyle.Flex;
            el.style.left    = px;
        }

        // ══════════════════════════════════════════════════════════════════
        // Event forwarding → Controller
        // ══════════════════════════════════════════════════════════════════

        private void OnAddShot()
        {
            var cam = FindAnyObjectByType<CameraBehaviour>();

            if (cam != null)
            {
                this._controller.AddShot(cam.ShotCamera.Position, cam.ShotCamera.Rotation);
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
            if (this._shotController == null || this._root == null)
            {
                return;
            }

            this._shotController.SetBottomInset(ViewportScope.CssToScreen(this._root, this._sectionHeight + RESIZE_HANDLE_HEIGHT));
        }
    }
}
