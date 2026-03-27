# Implementation Plan: 3.1.2 Shot Track UI

**Date**: 2026-03-26
**Ticket**: FRA-55
**Spec**: `docs/specs/milestone-3.1-shot-sequencer-spec.md` §3.1.2
**Branch**: `me/fra-55-312-shot-track-ui`

---

## Overview

Build the shot track bar — a horizontal strip at the bottom of the screen where each shot is a colored block proportional to its duration. This is the first timeline UI component. The Core model (Shot, ShotRegistry) is complete and tested.

## Architecture

```
Engine.Integration
  └── ShotController           ← new MonoBehaviour, owns ShotRegistry

UI.Timeline
  ├── TimelineViewState        ← zoom/pan model (pure C#, no Unity deps)
  ├── ShotTrackView            ← MonoBehaviour orchestrator (UIDocument)
  ├── ShotBlockElement         ← VisualElement for one shot block
  ├── ConfirmationDialog       ← reusable modal overlay
  └── ShotColorPalette         ← static color cycling

Resources/fram3d.uss           ← new styles appended
```

## Scoping Decisions

- **Single-camera only.** Multi-camera rows are 9.1. Shot bar height = single row.
- **Ripple shift deferred.** Boundary dragging changes left shot duration. Element keyframe shifting (3.1.3) and angle segment shifting (9.1) deferred — those entities don't exist yet. Shift+drag distinction is a no-op currently.
- **No playback integration.** Playback (3.2) doesn't exist. "Stop playback on drag" is spec-compliant but a no-op.
- **No ruler or keyframe tracks.** Those are 3.2. Shot track is the first (and only) timeline component for now.

## Phases

### Phase 1: Engine Foundation

**Task 1.1: ShotController MonoBehaviour**
- File: `Unity/Fram3d/Assets/Scripts/Engine/Integration/ShotController.cs`
- Owns a `ShotRegistry` instance
- In `Start()`: creates default `Shot_01` with current camera position/rotation
- Subscribes to `ShotRegistry.CurrentShotChanged` → evaluates camera at shot's t=0 via CameraBehaviour
- Public property: `ShotRegistry Registry { get; }`
- Public method: `AddShot()` — delegates to registry with current camera state

### Phase 2: UI Infrastructure

**Task 2.1: TimelineViewState**
- File: `Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineViewState.cs`
- Properties: `ViewStart` (seconds), `ViewEnd` (seconds), `PixelsPerSecond`
- Methods: `TimeToPixel(double seconds)`, `PixelToTime(double px)`, `ZoomAtPoint(double seconds, float factor)`, `Pan(double deltaPx)`, `FitAll(double totalDuration)`
- Constructor takes initial total duration, fits to available width
- Observable: `Changed` event for when view range updates

**Task 2.2: ShotColorPalette**
- File: `Unity/Fram3d/Assets/Scripts/UI/Timeline/ShotColorPalette.cs`
- Static class with 8-color palette (Premiere-style muted tones)
- `GetColor(int index)` → cycles through palette

**Task 2.3: USS styles**
- Append to `Unity/Fram3d/Assets/Resources/fram3d.uss`
- Classes: `shot-track`, `shot-track__label-column`, `shot-track__strip`, `shot-block`, `shot-block--active`, `shot-block__name`, `shot-block__duration`, `shot-track__add-button`, `shot-track__total-label`, `shot-track__tooltip`, `confirmation-dialog`, etc.

### Phase 3: Shot Track Layout

**Task 3.1: ShotBlockElement**
- File: `Unity/Fram3d/Assets/Scripts/UI/Timeline/ShotBlockElement.cs`
- VisualElement subclass
- Constructor takes Shot + color
- Displays: shot name (top-left), duration text (bottom-right, "5.0s")
- Width set externally by ShotTrackView based on TimelineViewState
- Visual states: active (bright border, full opacity) and inactive (dimmed)
- SetActive(bool) method

**Task 3.2: ShotTrackView MonoBehaviour**
- File: `Unity/Fram3d/Assets/Scripts/UI/Timeline/ShotTrackView.cs`
- Attached to UIDocument
- Creates layout:
  - Container: fixed height at bottom of screen, full width
  - Left 140px: label column with "SHOTS" label + "+" Add Shot button + "Total: Xs"
  - Right: clip container with shot blocks laid out horizontally
- Subscribes to ShotRegistry events (Added, Removed, Reordered, CurrentChanged)
- Rebuilds/updates shot blocks on data changes
- Manages TimelineViewState
- Sets bottom inset on CameraBehaviour

### Phase 4: Basic Interactions

**Task 4.1: Shot selection**
- ClickEvent on ShotBlockElement → `ShotRegistry.SetCurrentShot(id)`
- Active state visual update

**Task 4.2: Add Shot button**
- Click handler on add button → `ShotController.AddShot()`
- Auto-scroll to reveal new shot (update TimelineViewState)

**Task 4.3: Aggregate duration display**
- Label in label column: "Total: {ShotRegistry.TotalDuration:F1}s"
- Updates on any shot add/remove/duration change

**Task 4.4: Duration editing**
- Click duration label → replace with TextField
- On Enter/blur → validate, apply to Shot.Duration, rebuild layout
- Non-numeric → revert, out-of-range → clamp

**Task 4.5: Delete Shot**
- Right-click context menu or small × button on hover
- ConfirmationDialog: "Delete [name]? This cannot be undone."
- "Don't show this again" checkbox → PlayerPrefs
- On confirm → `ShotRegistry.RemoveShot(id)`

### Phase 5: Advanced Interactions

**Task 5.1: Zoom and pan**
- Scroll over strip → zoom at cursor position (TimelineViewState.ZoomAtPoint)
- Middle-click drag → pan (TimelineViewState.Pan)
- Clamp view range to [0, TotalDuration] with margin

**Task 5.2: Drag-and-drop reordering**
- PointerDown + hold (200ms threshold) → enter drag mode
- Visual: shot lifts (slight scale/opacity), drop indicator (vertical line) between shots
- PointerMove → update drop indicator position
- PointerUp → `ShotRegistry.Reorder(id, newIndex)`, dragged shot stays current
- Cancel if released outside strip
- If dropped at original position → no-op

**Task 5.3: Shot boundary dragging**
- Invisible hit zones (6px wide) at boundaries between shots
- Cursor change to col-resize on hover
- Drag → adjust left shot duration, snap to frame boundaries (`FrameRate.SnapToFrame`)
- Clamp to MIN_DURATION
- Tooltip during drag: "SHOT_NAME: Xs (Nf) [ripple]"

**Task 5.4: Hover tooltips**
- PointerEnter on shot block → show tooltip after brief delay
- Content: "SHOT NAME\nCam A · 3.0s (72f) · 2 kf"
- Follows cursor
- PointerLeave → hide

### Phase 6: Integration

**Task 6.1: Keyboard shortcuts**
- Ctrl+D while shot selected → no-op (explicitly blocked)
- Wire into KeyboardShortcutRouter

**Task 6.2: CameraInputHandler integration**
- ShotTrackView exposes `IsPointerOverUI` for input blocking
- CameraInputHandler checks this before processing scroll

**Task 6.3: Double-click zoom-to-fit**
- Double-click (within 350ms) a shot → set current + zoom TimelineViewState to fit that shot (8% padding)

## File Inventory

| File | Layer | New/Modified |
|------|-------|-------------|
| `Engine/Integration/ShotController.cs` | Engine | New |
| `UI/Timeline/TimelineViewState.cs` | UI | New |
| `UI/Timeline/ShotColorPalette.cs` | UI | New |
| `UI/Timeline/ShotBlockElement.cs` | UI | New |
| `UI/Timeline/ShotTrackView.cs` | UI | New |
| `UI/Timeline/ConfirmationDialog.cs` | UI | New |
| `Resources/fram3d.uss` | Resources | Modified |
| `UI/Input/KeyboardShortcutRouter.cs` | UI | Modified (Ctrl+D) |
| `UI/Input/CameraInputHandler.cs` | UI | Modified (shot track input blocking) |

## Dependencies

- `Shot`, `ShotRegistry` — already built and tested (3.1.1)
- `ShotId`, `TimePosition`, `FrameRate` — already in Core.Common
- `CameraBehaviour` — already exists in Engine.Integration
- `StyleSheetLoader`, `Theme` — existing UI utilities
