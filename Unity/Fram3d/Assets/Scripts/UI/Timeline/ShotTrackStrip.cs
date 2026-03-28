using System;
using System.Collections.Generic;
using Fram3d.Core.Shots;
using Fram3d.Core.Timelines;
using Fram3d.Engine.Cursor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// Shot track strip: displays shot blocks with drag reorder, boundary drag,
    /// and context menus. Follows the same pattern as <see cref="Ruler"/> and
    /// <see cref="ZoomBar"/> — constructor builds the tree, <see cref="Bind"/>
    /// wires pointer callbacks, events fire out, update methods push state in.
    /// </summary>
    public sealed class ShotTrackStrip : VisualElement
    {
        private const float BOUNDARY_HANDLE_WIDTH = 20f;

        private readonly List<VisualElement> _boundaryHandles = new();
        private readonly VisualElement _dropIndicator;
        private readonly VisualElement _outOfRange;
        private readonly VisualElement _playhead;
        private readonly Label         _totalLabel;
        private readonly VisualElement _trackArea;

        private Fram3d.Core.Timelines.Timeline _controller;

        public ShotTrackStrip()
        {
            this.AddToClassList("timeline-shot-row");

            var labelCol = new VisualElement();
            labelCol.AddToClassList("timeline-label-column");

            var titleRow = new VisualElement();
            titleRow.style.flexDirection = FlexDirection.Row;
            titleRow.style.alignItems    = Align.Center;

            var title = new Label("SHOTS");
            title.AddToClassList("timeline-label-column__title");
            titleRow.Add(title);

            var addBtn = new Button(() => this.AddShotRequested?.Invoke());
            addBtn.text = "+";
            addBtn.AddToClassList("timeline-shot__add-button");
            titleRow.Add(addBtn);
            labelCol.Add(titleRow);

            this._totalLabel = new Label("Total: 0.0s");
            this._totalLabel.AddToClassList("timeline-label-column__subtitle");
            labelCol.Add(this._totalLabel);
            this.Add(labelCol);

            this._trackArea = new VisualElement();
            this._trackArea.AddToClassList("timeline-shot-strip");
            this.Add(this._trackArea);

            this._dropIndicator = new VisualElement();
            this._dropIndicator.AddToClassList("shot-track__drop-indicator");
            this._dropIndicator.style.display = DisplayStyle.None;
            this._dropIndicator.pickingMode   = PickingMode.Ignore;
            this._trackArea.Add(this._dropIndicator);

            this._playhead = new VisualElement();
            this._playhead.AddToClassList("timeline-playhead");
            this._playhead.style.display = DisplayStyle.None;
            this._playhead.pickingMode   = PickingMode.Ignore;
            this._trackArea.Add(this._playhead);

            this._outOfRange = new VisualElement();
            this._outOfRange.AddToClassList("timeline-out-of-range");
            this._outOfRange.pickingMode = PickingMode.Ignore;
            this._trackArea.Add(this._outOfRange);

            this._trackArea.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                var w = this._trackArea.resolvedStyle.width;

                if (!float.IsNaN(w) && w > 0)
                {
                    this.TrackAreaResized?.Invoke(w);
                }
            });
        }

        public event Action         AddShotRequested;
        public event Action         BoundaryDragEnded;
        public event Action         BoundaryDragStarted;
        public event Action         ShotHoverEnded;
        public event Action<Shot>   ShotHoverStarted;
        public event Action<float>  TrackAreaResized;

        public VisualElement TrackArea => this._trackArea;

        public void Bind(Fram3d.Core.Timelines.Timeline controller)
        {
            this._controller = controller;

            this._trackArea.RegisterCallback<PointerDownEvent>(this.OnPointerDown);
            this._trackArea.RegisterCallback<PointerMoveEvent>(this.OnPointerMove);
            this._trackArea.RegisterCallback<PointerUpEvent>(this.OnPointerUp);
        }

        public void RebuildBlocks()
        {
            for (var i = this._trackArea.childCount - 1; i >= 0; i--)
            {
                var child = this._trackArea[i];

                if (child != this._dropIndicator
                 && child != this._playhead
                 && child != this._outOfRange)
                {
                    child.RemoveFromHierarchy();
                }
            }

            var shots = this._controller.Shots;

            for (var i = 0; i < shots.Count; i++)
            {
                var shot  = shots[i];
                var block = new ShotBlock(shot, i);

                block.RegisterCallback<PointerEnterEvent>(_ =>
                    this.ShotHoverStarted?.Invoke(shot));
                block.RegisterCallback<PointerLeaveEvent>(_ =>
                    this.ShotHoverEnded?.Invoke());
                block.DurationHoverStarted += () => this.ShotHoverEnded?.Invoke();
                block.DurationHoverEnded   += () => this.ShotHoverStarted?.Invoke(shot);
                block.DurationClicked      += b => b.BeginDurationEdit(value =>
                {
                    if (double.TryParse(value, out var parsed))
                    {
                        shot.Duration = parsed;
                    }
                });
                block.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
                {
                    evt.menu.AppendAction("Delete Shot", _ =>
                        this._controller.RemoveShot(shot.Id));
                });

                this._trackArea.Insert(this._trackArea.childCount - 3, block);
            }

            this.RebuildBoundaryHandles();
            this._playhead.BringToFront();
            this._outOfRange.BringToFront();

            this.UpdateActiveStates();
        }

        public void SyncVisuals()
        {
            if (this._controller == null)
            {
                return;
            }

            var px    = (float)this._controller.PlayheadPixel;
            var endPx = (float)this._controller.OutOfRangeStartPixel;

            this.UpdateBlockPositions();
            this.UpdateBoundaryHandlePositions();
            this.UpdatePlayhead(px);
            this.UpdateOutOfRange(endPx);
            this.UpdateDropIndicator();
            this._totalLabel.text = $"Total: {this._controller.TotalDuration:F1}s";
        }

        public void UpdateActiveStates()
        {
            var current = this._controller.CurrentShot;

            for (var i = 0; i < this._trackArea.childCount; i++)
            {
                if (this._trackArea[i] is ShotBlock block)
                {
                    block.SetActive(block.Shot == current);
                }
            }
        }

        private float ComputeDropIndicatorPx()
        {
            var targetIndex = this._controller.DragTargetIndex;
            var runningTime = 0.0;
            var shots       = this._controller.Shots;

            for (var i = 0; i < targetIndex && i < shots.Count; i++)
            {
                runningTime += shots[i].Duration;
            }

            return (float)this._controller.TimeToPixel(runningTime);
        }

        private int HandleEdgeIndex(VisualElement handle) =>
            handle.userData is int edgeIndex ? edgeIndex : -1;

        /// <summary>
        /// Returns the current mouse x in track-area-local pixels.
        /// Uses the Input System's screen position with explicit
        /// screen → panel → element conversion — the same pipeline
        /// used by TimelineSectionView.HandleInputSystemScroll().
        /// This bypasses evt.position/localPosition entirely, avoiding
        /// coordinate space ambiguity during pointer capture and event
        /// bubbling from child elements.
        /// </summary>
        private float TrackLocalX()
        {
            if (Mouse.current == null || this._trackArea.panel == null)
            {
                return 0;
            }

            var mousePos  = Mouse.current.position.ReadValue();
            var screenPos = new Vector2(mousePos.x, Screen.height - mousePos.y);
            var panelPos  = RuntimePanelUtils.ScreenToPanel(this._trackArea.panel, screenPos);
            return this._trackArea.WorldToLocal(panelPos).x;
        }

        private void OnBoundaryPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            var handle   = (VisualElement)evt.currentTarget;
            var edgeIndex = this.HandleEdgeIndex(handle);
            var result   = this._controller.BeginBoundaryDrag(edgeIndex);

            if (result == ShotTrackAction.BOUNDARY_DRAG)
            {
                handle.CapturePointer(evt.pointerId);
                CursorService.SetCursor(CursorType.ResizeHorizontal);
                this.BoundaryDragStarted?.Invoke();
            }

            evt.StopPropagation();
        }

        private void OnBoundaryPointerEnter(PointerEnterEvent _)
        {
            CursorService.SetCursor(CursorType.ResizeHorizontal);
        }

        private void OnBoundaryPointerLeave(PointerLeaveEvent _)
        {
            if (!this._controller.IsBoundaryDragging)
            {
                CursorService.ResetCursor();
            }
        }

        private void OnBoundaryPointerMove(PointerMoveEvent evt)
        {
            if (!this._controller.IsBoundaryDragging)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            this._controller.ShotTrackPointerMove(this.TrackLocalX(), now);
            evt.StopPropagation();
        }

        private void OnBoundaryPointerUp(PointerUpEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            var handle = (VisualElement)evt.currentTarget;

            if (handle.HasPointerCapture(evt.pointerId))
            {
                handle.ReleasePointer(evt.pointerId);
            }

            var result = this._controller.ShotTrackPointerUp();

            if (result == ShotTrackAction.BOUNDARY_COMPLETE)
            {
                this.BoundaryDragEnded?.Invoke();
                CursorService.ResetCursor();
            }

            evt.StopPropagation();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                var now    = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var result = this._controller.ShotTrackPointerDown(this.TrackLocalX(), now);

                // Capture pointer for all left-button interactions so drag
                // continues when the pointer moves outside the strip vertically.
                this._trackArea.CapturePointer(evt.pointerId);

                if (result == ShotTrackAction.BOUNDARY_DRAG)
                {
                    this.BoundaryDragStarted?.Invoke();
                }

                evt.StopPropagation();
            }
            else if (evt.button == 2)
            {
                evt.StopPropagation();
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            var now    = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var result = this._controller.ShotTrackPointerMove(this.TrackLocalX(), now);

            if (result == ShotTrackAction.NEAR_EDGE)
            {
                CursorService.SetCursor(CursorType.ResizeHorizontal);
            }
            else if (result == ShotTrackAction.NONE && !this._controller.IsBoundaryDragging)
            {
                CursorService.ResetCursor();
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            var result = this._controller.ShotTrackPointerUp();

            if (result == ShotTrackAction.BOUNDARY_COMPLETE)
            {
                this.BoundaryDragEnded?.Invoke();
                CursorService.ResetCursor();
            }

            if (result == ShotTrackAction.DRAG_COMPLETE)
            {
                this._dropIndicator.style.display = DisplayStyle.None;
            }

            if (this._trackArea.HasPointerCapture(evt.pointerId))
            {
                this._trackArea.ReleasePointer(evt.pointerId);
            }
        }

        private void UpdateBlockPositions()
        {
            var runningTime = 0.0;

            for (var i = 0; i < this._trackArea.childCount; i++)
            {
                if (this._trackArea[i] is not ShotBlock block)
                {
                    continue;
                }

                var startPx = this._controller.TimeToPixel(runningTime);
                var endPx   = this._controller.TimeToPixel(runningTime + block.Shot.Duration);
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

        private void RebuildBoundaryHandles()
        {
            for (var i = 0; i < this._boundaryHandles.Count; i++)
            {
                this._boundaryHandles[i].RemoveFromHierarchy();
            }

            this._boundaryHandles.Clear();

            // One handle per shot boundary (after each shot except possibly the last,
            // but we create one per shot — the last handle sits at the project end).
            for (var i = 0; i < this._controller.Shots.Count; i++)
            {
                var handle = new VisualElement();
                handle.AddToClassList("shot-track__boundary");
                handle.userData                  = i;
                handle.style.position            = Position.Absolute;
                handle.style.width               = BOUNDARY_HANDLE_WIDTH;
                handle.style.top                 = 0;
                handle.style.bottom              = 0;
                handle.style.backgroundColor     = new StyleColor(Color.clear);
                handle.focusable                 = false;
                handle.RegisterCallback<PointerDownEvent>(this.OnBoundaryPointerDown);
                handle.RegisterCallback<PointerEnterEvent>(this.OnBoundaryPointerEnter);
                handle.RegisterCallback<PointerLeaveEvent>(this.OnBoundaryPointerLeave);
                handle.RegisterCallback<PointerMoveEvent>(this.OnBoundaryPointerMove);
                handle.RegisterCallback<PointerUpEvent>(this.OnBoundaryPointerUp);
                this._boundaryHandles.Add(handle);
                this._trackArea.Add(handle);
            }
        }

        private void UpdateBoundaryHandlePositions()
        {
            var runningTime = 0.0;
            var shotCount   = this._controller.Shots.Count;

            for (var i = 0; i < shotCount && i < this._boundaryHandles.Count; i++)
            {
                runningTime += this._controller.Shots[i].Duration;
                var centerPx = (float)this._controller.TimeToPixel(runningTime);
                this._boundaryHandles[i].style.left = centerPx - BOUNDARY_HANDLE_WIDTH / 2f;
            }
        }

        private void UpdateDropIndicator()
        {
            if (this._controller.IsDragging)
            {
                this._dropIndicator.style.display = DisplayStyle.Flex;
                this._dropIndicator.style.left    = this.ComputeDropIndicatorPx();
            }
        }

        private void UpdateOutOfRange(float endPx)
        {
            this._outOfRange.style.position = Position.Absolute;
            this._outOfRange.style.left     = endPx;
            this._outOfRange.style.top      = 0;
            this._outOfRange.style.bottom   = 0;
            this._outOfRange.style.right    = 0;
            this._outOfRange.style.display  = endPx >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdatePlayhead(float px)
        {
            this._playhead.style.display = DisplayStyle.Flex;
            this._playhead.style.left    = px;
        }
    }
}
