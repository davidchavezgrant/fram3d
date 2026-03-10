# Fram3d Architecture Reference

**Date**: 2026-03-10
**Companion to**: `2026-03-10-fram3d-spec.md`
**Based on**: Vismatic Studio implementation (Unity 6000.1.11f1)

This document captures implementation decisions, patterns, constants, and technical details from the original Vismatic Studio codebase. Use as a reference when rebuilding — not as a prescription.

---

## 1. Platform & Framework

- **Engine**: Unity 6000.1.11f1
- **Camera system**: Cinemachine 3.x (`CinemachineCamera`)
- **Input**: Unity Input System (`UnityEngine.InputSystem`) for raw keyboard/mouse, with direct `Keyboard.current` / `Mouse.current` access
- **UI framework**: Unity legacy UI (`UnityEngine.UI`) — Canvas, Image, Text, Button, InputField
- **UI construction**: All UI built programmatically at runtime. No prefabs, no UXML.

---

## 2. Application Architecture

### 2.1 Entry Point

`ApplicationController` (MonoBehaviour) is the single orchestrator. Its `Update()` method drives the frame loop:

1. Process input (`UserInputDriver.HandleInput`)
2. Update camera rig (`VirtualCameraRig.NextFrame` — focus lerp, lens smoothing, shake)
3. Update overlays (`CameraOverlayController.NextFrame`)
4. Update scene controller (`SceneController.NextFrame` — selection state, gizmos)
5. Handle timeline interaction (`TimelineInteractionHandler`)
6. Update timeline playback (`UpdateTimeline` command)
7. Late update: camera tracking (`CameraTracker.Update` in `LateUpdate`)

### 2.2 Service Locator

`ApplicationServiceCollection` provides lazy-initialized services via `Lazy<T>`. Services created on first access:

- `VirtualCameraRig`
- `SceneController`
- `MasterTimelineController`
- `CameraOverlayController`
- `UserInputDriver`
- `CameraTracker`

### 2.3 Command Pattern

`ICommand` interface with `Execute()` / `Undo()` / `Redo()`. Undo/Redo declared but throw `NotImplementedException` — scaffolded for future use.

Timeline commands: `AddShot`, `DeleteShot`, `AddCameraKeyframe`, `AddObjectKeyframe`, `DeleteKeyframe`, `ScrubTime`, `SetCurrentTime`, `ResetCurrentTime`, `EvaluateShotAtTime`, `EvaluateCurrentShot`, `RefreshKeyframeEditor`, `RefreshThumbnails`, `ReorderThumbnails`, `UpdateTimeline`.

### 2.4 Handler Pattern

Discrete handler classes for each interaction, instantiated per-use:
- `CameraManuallyMovedHandler`
- `KeyframeDragCompletedHandler`

### 2.5 Domain Events

Shot-level events propagate from domain to UI:
- `DurationChanged`, `KeyframeAdded`, `KeyframeRemoved`, `KeyframeMoved`

---

## 3. Camera System Implementation

### 3.1 Composition

`VirtualCameraRig` composes four controllers:

| Controller | Class |
|------------|-------|
| Movement | `CameraMovementController` |
| Lens | `CameraLensController` |
| Focus | `CameraFocusController` |
| Shake | `CameraShakeController` |

Camera state abstracted via `ICameraState` interface, implemented by `CinemachineCameraState`.

### 3.2 Movement Constants

From `CameraMovementSettings`:
- `DollyScrollSpeed`: 0.01
- `PanTiltSpeed`: 0.2
- `RollSpeed`: 0.03
- `PedestalSpeed`: 0.02
- `TruckSpeed`: 0.02
- `DefaultPosition`: (0, 1.6, -5)
- `DefaultRotation`: Identity quaternion

Movement methods: `Pan(amount, anchorPoint)`, `Tilt(amount, anchorPoint)`, `DollyInOut(amount)`, `DollySideways(amount)`, `Crane(amount)`, `Roll(amount, anchorPoint)`, `Orbit(pan, tilt, anchorPoint)`, `DollyZoom(amount)`, `ResetToDefault()`.

### 3.3 Lens Constants

From `CameraLensSpecification`:
- Focal length range: 14–200mm
- Sensor height: 18.17mm
- FOV formula: `2 * atan(halfSensorHeight / focalLength)`
- Lerp smoothing speed: 10
- Dolly zoom calculates movement distance to maintain subject size during focal length change

Zoom methods: `Zoom(scrollDelta)` (multiplier), `AdjustFocalLength(scrollDelta)` (direct delta), `DollyZoom(amount)`, `SnapToCommonFocalLength(index)`.

### 3.4 Focus Constants

- Lerp speed: 2.0
- Distance multiplier: 1.5x
- Minimum distance: 0.1 units
- Uses `BoundsCalculator` to get encapsulating bounds from renderers/colliders

### 3.5 Shake Constants

- Perlin noise-based
- Default amplitude: 0.1, frequency: 1.0
- Position scale: 0.01
- Rotation scale: 0.5 (X/Y only, no Z roll)
- Reverts previous frame's offset before applying new values

---

## 4. Timeline Implementation

### 4.1 State

`TimelineState` holds:
- List of `ShotSetup` objects
- Current/selected/previous shot indices
- Current time within shot
- `IsEvaluating` flag (prevents auto-keyframe during evaluation)
- Duration editing state
- Thumbnail references
- Keyframe marker dictionaries (camera + object)

### 4.2 Shot Aggregate

`ShotSetup` is the aggregate root:
- `ShotId` — GUID-based value object
- `CameraAnimation` — keyframed camera path
- `ShotObjectManager` — manages per-object `ObjectAnimation` instances
- Default duration: 5 seconds, min: 0.1

### 4.3 Animation Classes

Base: `Animation<TKeyframe, TCurveManager>`

**CameraAnimation**:
- Keyframe type: `CameraKeyframe` (position Vector3, rotation Quaternion, focalLength float)
- Curves: `PositionCurves` (X/Y/Z), `RotationCurves` (X/Y/Z/W), `FocalLengthCurves`
- Uses Unity `AnimationCurve` for interpolation

**ObjectAnimation**:
- Keyframe type: `ObjectKeyframe` (position, rotation, scale all Vector3 except rotation is Quaternion)
- Curves: `PositionCurves`, `RotationCurves`, `ScaleCurves`
- Captures initial state on first keyframe

### 4.4 Keyframe Classes

- `AnimationKeyframe` (base) — ID (GUID), Time, Position, Rotation
- `CameraKeyframe` — adds FocalLength, uses `CameraKeyframeState` value object
- `ObjectKeyframe` — adds Scale, uses `ObjectState` value object
- `KeyframeManager<T>` — generic manager with add/remove/move, sort by time, find closest, find by ID, dictionary lookup by `KeyframeId`

### 4.5 Playback

`PlaybackManager`: `CurrentTime` (float), `IsPlaying` (bool), `PlayStartTime` (for real-time sync). Playback advances time and evaluates current shot each frame.

---

## 5. Auto-Keyframing Implementation

### 5.1 CameraTracker

Runs in `LateUpdate`. Change thresholds:
- Position: 0.001 units
- Rotation: 0.01 degrees
- Focal length: 0.01mm

Fires `CameraManuallyMovedHandler` when changes detected and not playing.

### 5.2 ObjectTracker

Attached to each `SceneElement`. Only tracks the currently selected object. Same thresholds as camera (position: 0.001, rotation: 0.01, scale: 0.001).

### 5.3 AnimationFrameTracker

Frame counter. `MarkAsAnimated()` called during animation evaluation. `WasAnimatedThisFrame()` checked by trackers to skip auto-keyframing.

### 5.4 Near-Keyframe Logic

Threshold for "near existing keyframe": 0.1 seconds. If near: update existing. If not: create new.

---

## 6. Scene Implementation

### 6.1 SceneElement

MonoBehaviour, requires `Collider`. Implements `IPointerClickHandler`, `IPointerEnterHandler`, `IPointerExitHandler`.

Visual states: Original, Highlighted, Selected. Default materials auto-created:
- Highlight: yellow, 50% alpha
- Selection: cyan, 50% alpha

Static events: `ElementClicked`, `ElementDestroyed`.

### 6.2 Selection

`SceneSelectionManager` composed of:
- `ElementEventSubscriber` — listens to `SceneElement.ElementClicked`
- `SelectionState` — manages current selection, fires `ElementSelected` / `SelectionCleared`
- `ClickValidator` — empty-space click detection for deselection

### 6.3 Gizmos

`GizmoManager` manages lifecycle. Three controller types:

| Gizmo | Controller | Handle Shape |
|-------|------------|-------------|
| Translation | `TranslationGizmoController` | Arrows |
| Rotation | `RotationGizmoController` | Rings |
| Scale | `ScaleGizmoController` | Cubes |

Shared infrastructure:
- `GizmoControllerBase` — abstract base with handle management
- `GizmoHandle` — individual axis handle with drag behavior
- `DragOperationHandler` / `DragStateManager` — drag interaction
- `GizmoScalerBase` — constant screen-space size
- `MouseListener` / `CoordinateProjector` / `RayPlaneIntersectionCalculator` — math utilities
- `GizmoColorConfig` — axis colors
- Custom `AlwaysOnTop` shader on layer 8

---

## 7. Overlay Implementation

### 7.1 Aspect Ratio Masks

`AspectRatioMaskManager` renders 4 `MaskBar` UI elements (top, bottom, left, right). Canvas sorting order: 49. `ScreenState` detects dimension changes for efficient updates.

### 7.2 Frame Guides

`FrameGuideManager` manages three guide types. Canvas sorting order: 50.

| Guide | Class | Details |
|-------|-------|---------|
| Rule of Thirds | `RuleOfThirdsGuide` | White, 30% alpha |
| Center Cross | `CenterCrossGuide` | White, 50% alpha, 20px size, 3x dot multiplier |
| Safe Zones | `SafeZoneGuide` | Title safe 90% (yellow, 40% alpha), Action safe 93% (green, 30% alpha) |

---

## 8. UI Layout Details

### 8.1 Panel Dimensions

| Panel | Y Range | Canvas Order |
|-------|---------|-------------|
| Shot Sequencer | 0–140px from bottom | Default |
| Keyframe Editor | 140–390px from bottom | Default |
| Aspect Ratio Masks | Full screen | 49 |
| Frame Guides | Within aspect bounds | 50 |
| Camera Info HUD | Top-left corner | 100 |

### 8.2 Sequencer Thumbnails

120x100px each. Show shot name + editable duration. Drag-and-drop with drop indicators.

### 8.3 Keyframe Editor Regions

- Transport bar: top 30%
- Timeline area: middle 50%
- Status bar: bottom 20%

### 8.4 Color Palette

| Element | Color |
|---------|-------|
| Timeline background | (0.2, 0.2, 0.2, 0.95) |
| Sequencer background | (0.1, 0.1, 0.1, 0.95) |
| Track background | (0.1, 0.1, 0.1, 0.5) |
| Camera keyframes | Yellow |
| Object keyframes | Green |
| Selected/dragged keyframe | Cyan |
| Playhead | Red |
| Aspect ratio bars | Black |

---

## 9. Input Implementation

### 9.1 Input System

Uses Unity Input System (`Controls` input asset) for Scroll and MouseDelta actions. Most handling done directly via `Keyboard.current` and `Mouse.current` in `UserInputDriver`.

### 9.2 Sensitivities

- Drag sensitivity: 0.2
- Scroll sensitivity: 0.02

---

## 10. Key Namespaces (Original)

- `VismaticStudio` — main application, input
- `VismaticStudio.Abstract` — interfaces
- `VismaticStudio.CameraRig` — camera system
  - `.Controllers.Focus`, `.Controllers.Lens`, `.Controllers.Movement`, `.Controllers.Shake`
- `VismaticStudio.Scenes` — scene management
  - `.Elements`, `.Gizmos`, `.Selection`
- `VismaticStudio.Sequencing` — timeline
  - `.Animation`, `.Shots`, `.UI`
- `VismaticStudio.Overlays` — viewport overlays
- `VismaticStudio.Tracking` — auto-keyframing
