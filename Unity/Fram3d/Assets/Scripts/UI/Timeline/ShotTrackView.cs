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
    /// MonoBehaviour orchestrator for the shot track bar. Manages the UIDocument,
    /// creates the layout, renders shot blocks, and handles all shot track interactions
    /// (selection, add, delete, reorder, boundary drag, zoom, pan, tooltips).
    /// </summary>
    public sealed class ShotTrackView : MonoBehaviour
    {
        private const float  BOUNDARY_HIT_WIDTH = 8f;
        private const double DOUBLE_CLICK_MS    = 350;
        private const int    HOLD_THRESHOLD_MS  = 200;
        private const float  TRACK_HEIGHT       = 48f;

        // ── References ──
        private ShotController       _shotController;

        // ── UI elements ──
        private VisualElement        _root;
        private VisualElement        _container;
        private VisualElement        _labelColumn;
        private VisualElement        _strip;
        private Label                _totalLabel;
        private VisualElement        _dropIndicator;
        private VisualElement        _tooltip;
        private Label                _tooltipText;
        private VisualElement        _boundaryTooltip;
        private Label                _boundaryTooltipText;

        // ── Shot block tracking ──
        private readonly List<ShotBlockElement> _blocks = new();
        private readonly List<VisualElement>    _boundaries = new();
        private TimelineViewState              _viewState;

        // ── Subscriptions ──
        private IDisposable _addedSub;
        private IDisposable _removedSub;
        private IDisposable _reorderedSub;
        private IDisposable _currentChangedSub;

        // ── Drag-and-drop state ──
        private bool               _isDragging;
        private ShotBlockElement   _dragBlock;
        private int                _dragOriginalIndex;
        private int                _dragTargetIndex;
        private long               _pointerDownTime;
        private Vector2            _pointerDownPos;
        private bool               _pointerIsDown;

        // ── Boundary drag state ──
        private bool               _isBoundaryDragging;
        private int                _boundaryLeftIndex;

        // ── Double-click state ──
        private long               _lastClickTime;
        private ShotId             _lastClickShotId;

        // ── Pan state ──
        private bool               _isPanning;
        private Vector2            _panStartPos;

        // ── Tooltip state ──
        private ShotBlockElement   _hoveredBlock;

        /// <summary>
        /// True when the pointer is over the shot track UI.
        /// </summary>
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

        /// <summary>
        /// True when a duration text field is focused.
        /// </summary>
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

        // ══════════════════════════════════════════════════════════════════
        // Lifecycle
        // ══════════════════════════════════════════════════════════════════

        private void Start()
        {
            this._shotController = FindAnyObjectByType<ShotController>();

            if (this._shotController == null)
            {
                Debug.LogWarning("ShotTrackView: No ShotController found.");
                return;
            }

            var uiDocument = this.GetComponent<UIDocument>();

            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogWarning("ShotTrackView: UIDocument or rootVisualElement is null.");
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
            this.UpdateTotalLabel();
            this.HandleMiddleClickPan();
            this.UpdateTooltipPosition();
            this.UpdateBoundaryTooltipPosition();
        }

        private void OnDestroy()
        {
            this._addedSub?.Dispose();
            this._removedSub?.Dispose();
            this._reorderedSub?.Dispose();
            this._currentChangedSub?.Dispose();
        }

        // ══════════════════════════════════════════════════════════════════
        // Layout
        // ══════════════════════════════════════════════════════════════════

        private void BuildLayout()
        {
            this._container = new VisualElement();
            this._container.AddToClassList("shot-track");
            this._container.style.height = TRACK_HEIGHT;

            // Label column
            this._labelColumn = new VisualElement();
            this._labelColumn.AddToClassList("shot-track__label-column");

            var label = new Label("SHOTS");
            label.AddToClassList("shot-track__label");
            this._labelColumn.Add(label);

            this._totalLabel = new Label("Total: 0.0s");
            this._totalLabel.AddToClassList("shot-track__total");
            this._labelColumn.Add(this._totalLabel);

            var addButton = new Button(this.OnAddShotClicked);
            addButton.text = "+ Add Shot";
            addButton.AddToClassList("shot-track__add-button");
            this._labelColumn.Add(addButton);

            this._container.Add(this._labelColumn);

            // Strip area (shot blocks go here)
            this._strip = new VisualElement();
            this._strip.AddToClassList("shot-track__strip");
            this._container.Add(this._strip);

            // Register strip interactions
            this._strip.RegisterCallback<WheelEvent>(this.OnStripWheel);
            this._strip.RegisterCallback<PointerDownEvent>(this.OnStripPointerDown);
            this._strip.RegisterCallback<PointerMoveEvent>(this.OnStripPointerMove);
            this._strip.RegisterCallback<PointerUpEvent>(this.OnStripPointerUp);

            // Drop indicator (hidden by default)
            this._dropIndicator = new VisualElement();
            this._dropIndicator.AddToClassList("shot-track__drop-indicator");
            this._dropIndicator.style.display = DisplayStyle.None;
            this._strip.Add(this._dropIndicator);

            // Hover tooltip (hidden by default)
            this._tooltip = new VisualElement();
            this._tooltip.AddToClassList("shot-tooltip");
            this._tooltip.style.display = DisplayStyle.None;
            this._tooltip.pickingMode   = PickingMode.Ignore;
            this._tooltipText           = new Label();
            this._tooltipText.AddToClassList("shot-tooltip__text");
            this._tooltip.Add(this._tooltipText);
            this._root.Add(this._tooltip);

            // Boundary drag tooltip (hidden by default)
            this._boundaryTooltip = new VisualElement();
            this._boundaryTooltip.AddToClassList("boundary-tooltip");
            this._boundaryTooltip.style.display = DisplayStyle.None;
            this._boundaryTooltip.pickingMode   = PickingMode.Ignore;
            this._boundaryTooltipText           = new Label();
            this._boundaryTooltipText.AddToClassList("boundary-tooltip__text");
            this._boundaryTooltip.Add(this._boundaryTooltipText);
            this._root.Add(this._boundaryTooltip);

            this._root.Add(this._container);

            // Initialize view state after strip has geometry
            this._strip.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                var stripWidth = this._strip.resolvedStyle.width;

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
                this.UpdateBottomInset();
            });
        }

        // ══════════════════════════════════════════════════════════════════
        // Registry subscriptions
        // ══════════════════════════════════════════════════════════════════

        private void SubscribeToRegistry()
        {
            var reg = this._shotController.Registry;
            this._addedSub          = reg.ShotAdded.Subscribe(_ => this.RebuildBlocks());
            this._removedSub        = reg.ShotRemoved.Subscribe(_ => this.RebuildBlocks());
            this._reorderedSub      = reg.Reordered.Subscribe(_ => this.RebuildBlocks());
            this._currentChangedSub = reg.CurrentShotChanged.Subscribe(_ => this.UpdateActiveStates());
        }

        // ══════════════════════════════════════════════════════════════════
        // Block management
        // ══════════════════════════════════════════════════════════════════

        private void RebuildBlocks()
        {
            // Clear existing blocks and boundaries
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

            // Recreate from registry
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

                this._strip.Insert(this._strip.childCount - 1, block); // Before drop indicator
                this._blocks.Add(block);

                // Add boundary handle between shots (not after the last one)
                if (i < reg.Shots.Count - 1)
                {
                    var boundaryIndex = i;
                    var boundary      = new VisualElement();
                    boundary.AddToClassList("shot-track__boundary");
                    boundary.RegisterCallback<PointerDownEvent>(evt =>
                        this.OnBoundaryPointerDown(evt, boundaryIndex));
                    this._strip.Insert(this._strip.childCount - 1, boundary);
                    this._boundaries.Add(boundary);
                }
            }

            this.UpdateActiveStates();
            this.UpdateBlockWidths();
            this.UpdateTotalLabel();
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

            var reg = this._shotController.Registry;

            // Position boundaries
            var boundaryIdx = 0;
            var runningTime = 0.0;

            for (var i = 0; i < this._blocks.Count; i++)
            {
                var block    = this._blocks[i];
                var shot     = block.Shot;
                var startPx  = this._viewState.TimeToPixel(runningTime);
                var endPx    = this._viewState.TimeToPixel(runningTime + shot.Duration);
                var widthPx  = Math.Max(endPx - startPx, 4.0); // Minimum 4px visible

                block.style.position = Position.Absolute;
                block.style.left     = (float)startPx;
                block.style.width    = (float)widthPx;
                block.style.top      = 0;
                block.style.bottom   = 0;

                block.Refresh();

                // Position boundary handle at the right edge of this block
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

            // Convert CSS pixels to screen pixels
            var rootW = this._root?.resolvedStyle.width ?? 0f;
            var scale = 1f;

            if (rootW > 0)
            {
                scale = Screen.width / rootW;
            }

            this._shotController.SetBottomInset(TRACK_HEIGHT * scale);
        }

        // ══════════════════════════════════════════════════════════════════
        // Add Shot
        // ══════════════════════════════════════════════════════════════════

        private void OnAddShotClicked()
        {
            var shot = this._shotController.AddShot();

            // Auto-scroll to reveal the new shot
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

            // Check for double-click
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

            // Zoom to fit this shot (8% padding)
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

            var cancelBtn = new Button(() =>
            {
                overlay.RemoveFromHierarchy();
            });
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
        // Strip pointer events (selection, drag-to-reorder)
        // ══════════════════════════════════════════════════════════════════

        private void OnStripPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 0) // Left click
            {
                this._pointerDownTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                this._pointerDownPos  = evt.localPosition;
                this._pointerIsDown   = true;

                // Find which block was clicked
                var block = FindBlockAt(evt.localPosition);

                if (block != null)
                {
                    this._dragBlock         = block;
                    this._dragOriginalIndex = this._blocks.IndexOf(block);
                }
            }
            else if (evt.button == 2) // Middle click
            {
                this._isPanning   = true;
                this._panStartPos = evt.localPosition;
                evt.StopPropagation();
            }
        }

        private void OnStripPointerMove(PointerMoveEvent evt)
        {
            // Handle panning
            if (this._isPanning && this._viewState != null)
            {
                var delta = evt.localPosition.x - this._panStartPos.x;
                this._viewState.Pan(-delta);
                this._panStartPos = evt.localPosition;
                this.UpdateBlockWidths();
                return;
            }

            // Handle drag-to-reorder
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

            // Handle boundary drag
            if (this._isBoundaryDragging)
            {
                this.UpdateBoundaryDrag(evt.localPosition);
            }
        }

        private void OnStripPointerUp(PointerUpEvent evt)
        {
            if (evt.button == 2) // Middle click release
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
                // It was a click, not a drag
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

            // Position the indicator at the insertion point
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

            // Adjust target index if moving forward (removing shifts indices)
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

            var reg       = this._shotController.Registry;
            var leftShot  = reg.Shots[this._boundaryLeftIndex];
            var startTime = reg.GetGlobalStartTime(leftShot.Id).Seconds;
            var cursorTime = this._viewState.PixelToTime(localPos.x);
            var newDuration = cursorTime - startTime;

            // Snap to frame boundaries
            var snapped = FrameRate.FPS_24.SnapToFrame(new TimePosition(Math.Max(newDuration, Shot.MIN_DURATION)));
            leftShot.Duration = snapped.Seconds;

            this.UpdateBlockWidths();
            this.UpdateTotalLabel();

            // Update boundary tooltip
            var fps    = FrameRate.FPS_24;
            var frames = snapped.ToFrame(fps);
            var mode   = "[ripple]";

            if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            {
                mode = "[shots only]";
            }

            this._boundaryTooltipText.text = $"{leftShot.Name}: {leftShot.Duration:F1}s ({frames}f) {mode}";
        }

        private void CompleteBoundaryDrag()
        {
            this._isBoundaryDragging            = false;
            this._boundaryTooltip.style.display = DisplayStyle.None;
        }

        // ══════════════════════════════════════════════════════════════════
        // Zoom (scroll wheel)
        // ══════════════════════════════════════════════════════════════════

        private void OnStripWheel(WheelEvent evt)
        {
            if (this._viewState == null)
            {
                return;
            }

            var cursorTime = this._viewState.PixelToTime(evt.localMousePosition.x);
            this._viewState.ZoomAtPoint(cursorTime, -evt.delta.y);
            this.UpdateBlockWidths();
            evt.StopPropagation();
        }

        // ══════════════════════════════════════════════════════════════════
        // Middle-click pan
        // ══════════════════════════════════════════════════════════════════

        private void HandleMiddleClickPan()
        {
            // Pan state is managed via PointerDown/Move/Up events
        }

        // ══════════════════════════════════════════════════════════════════
        // Hover tooltip
        // ══════════════════════════════════════════════════════════════════

        private void ShowTooltip(ShotBlockElement block)
        {
            this._hoveredBlock = block;
            var shot   = block.Shot;
            var fps    = FrameRate.FPS_24;
            var frames = new TimePosition(shot.Duration).ToFrame(fps);
            var kfCount = shot.TotalCameraKeyframeCount;
            this._tooltipText.text     = $"{shot.Name}\nCam A \u00b7 {shot.Duration:F1}s ({frames}f) \u00b7 {kfCount} kf";
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
            if (!this._isBoundaryDragging || this._boundaryTooltip.style.display == DisplayStyle.None)
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
