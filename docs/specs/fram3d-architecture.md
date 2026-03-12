# Fram3d Architecture

**Date**: 2026-03-10
**Companion to**: `fram3d-roadmap.md`

---

## 1. Platform & Framework

- **Engine**: Unity 6000.1.11f1
- **Camera system**: Cinemachine 3.x (`CinemachineCamera`)
- **Input**: Unity Input System (`UnityEngine.InputSystem`) with direct `Keyboard.current` / `Mouse.current` access
- **UI framework**: Unity legacy UI (`UnityEngine.UI`) — Canvas, Image, Text, Button, InputField
- **UI construction**: All UI built programmatically at runtime. No prefabs, no UXML.

---

## 2. Confirmed Decisions

> **File format**: Deferred to implementation. Human-readable preferred for git diffing and debugging. Must serialize full project state (see 2.2 spec).

> **Playback frame dropping**: When scenes are too heavy for real-time playback, drop frames (maintain timing, skip visual updates) rather than slowing down. The playback system tracks wall-clock time and evaluates the timeline at the correct time regardless of render performance.

> **Rotation storage**: Quaternions internally, pan/tilt/roll displayed in UI. Prevents gimbal lock transparently.

> **Auto-keyframing**: Only changed properties (not all properties). Per-track stopwatch model (AE/Premiere). Stopwatches default to off.

> **Undo model**: Global stack, not per-shot. Cross-shot undo does not auto-switch the active shot.

> **Delete behavior**: Delete key = keyframe only. Cmd+Delete = object. Context-sensitive to avoid ambiguity.

---

## 3. Tuned Constants

Values from the previous iteration (Vismatic Studio) that were empirically tuned. Starting points — expect to re-tune.

**Camera movement speeds:**
- `DollyScrollSpeed`: 0.01
- `PanTiltSpeed`: 0.2
- `RollSpeed`: 0.03
- `PedestalSpeed`: 0.02
- `TruckSpeed`: 0.02
- `DefaultPosition`: (0, 1.6, -5)
- `DefaultRotation`: Identity quaternion

**Focus:**
- Lerp speed: 2.0
- Distance multiplier: 1.5x (breathing room when focusing on an object)
- Minimum distance: 0.1 units

**Camera shake:**
- Perlin noise-based
- Default amplitude: 0.1, frequency: 1.0
- Position scale: 0.01
- Rotation scale: 0.5 (X/Y only, no Z roll)

**Auto-keyframing thresholds:**
- Near existing keyframe: 0.1 seconds. If near: update existing. If not: create new.
- Camera change detection: position 0.001 units, rotation 0.01 degrees, focal length 0.01mm
- Object change detection: position 0.001, rotation 0.01, scale 0.001

**Input sensitivities:**
- Drag sensitivity: 0.2
- Scroll sensitivity: 0.02

**Undo coalescing:** 1000ms inactivity timeout for scroll gestures.

---

## 4. Implementation Details

**Keyframe interaction rules:**
- Moving a main keyframe moves all child property keyframes
- Moving a child keyframe creates a new main keyframe at the target time containing only that property; the original slot empties
- Deleting a main keyframe deletes all children
- Deleting all children deletes the main keyframe
- Dragging onto an existing keyframe silently merges
- Snap to 0.1s during drag

**Object linking constraints:** Max chain depth: 4.

**Input mappings:**
- Mouse: Scroll Y = focal length, +Alt = dolly, +Shift = crane, +Ctrl = roll, +Cmd+Alt = dolly zoom, Scroll X+Cmd = truck, Ctrl-drag = pan/tilt, Alt-drag = orbit (around selected or world origin)
- Keyboard: Space = play/pause, QWER = tool modes, F = focus, 1-9 = focal length presets, A/Shift+A = aspect ratio cycle, C = camera keyframe, V = object keyframe, Arrows = scrub 1 frame, Delete = context-sensitive delete, Ctrl+D = duplicate, Ctrl+R = reset camera, Cmd+Z / Cmd+Shift+Z = undo/redo

---

## 5. Architecture Considerations

### 5.1 Scene Serialization & Lazy Loading

**Source**: Multi-Scene Project Structure spec — unlimited scenes, lazy load/lazy render

Scenes must be independently loadable without pulling in sibling scene data. Only the active scene's heavy data (meshes, textures, keyframes) should be in memory; inactive scenes keep only lightweight metadata (object list, shot count, character assignments) for fast tab switching.

**Implications for file format:**
- The project file format needs clear scene boundaries — either separate chunks within a single file, or separate files per scene within a project bundle (e.g., `project.fram3d/scene-1.json`, `project.fram3d/scene-2.json`)
- Scene-level save should be possible without re-serializing the entire project
- Consider: a project manifest file (settings, character definitions, scene order) + individual scene files. This also makes scene duplication and deletion cheap (copy/delete a file rather than splicing a monolithic blob)
- Character definitions live at project level but are referenced by scenes — avoid duplicating character data across scene files

**Resolve before Save/Load spec (Milestone 2.2).**

### 5.2 Infrastructure to Build Early

Cross-cutting systems that multiple downstream features depend on. Build during Project 2 timeframe to avoid reimplementing for each feature.

- **Settings / Preferences panel:** Centralized settings panel so new settings can be added incrementally as features ship. Avoids scattering configuration UIs across the application.
- **Panel / docking system:** General panel system with docking support. Downstream milestones (hierarchy panel, pose library, asset library, inspector) all require dockable panels.

---

## 6. Domain Modeling Approach

DDD-informed, not DDD-orthodox. The cinema domain is rich enough to warrant modeling discipline; Unity is opinionated enough that full layered architecture would fight the engine.

### 6.1 What We Use

**Ubiquitous language.** Cinema terminology is consistent from roadmap to spec to class name to method name. Dolly, crane, truck, coverage, blocking, stopwatch — these terms mean the same thing everywhere. This is the single highest-value DDD concept.

**Bounded contexts via Assembly Definitions.** Each context is a separate `.asmdef`, enforcing boundaries at compile time:

| Context | Core Concepts | Assembly |
|---------|--------------|----------|
| Camera | Rig, lens, focus, shake, DOF | `Fram3d.Camera` |
| Sequencing | Shot, keyframe, track, playback, stopwatch | `Fram3d.Sequencing` |
| Scene | Element, selection, gizmo, ground plane | `Fram3d.Scene` |
| Characters | Mannequin, pose, IK, expression, skeleton | `Fram3d.Characters` |
| Persistence | Project, scene file, asset bundle | `Fram3d.Persistence` |
| Export | Renderer, storyboard, EDL | `Fram3d.Export` |

If Characters can't reference Camera internals, you physically can't create the wrong coupling.

**Aggregates.** Domain objects with clear ownership boundaries:

| Aggregate Root | Owns |
|---------------|------|
| `ShotSetup` | `CameraAnimation`, `ShotObjectManager`, per-object `ObjectAnimation` instances |
| `Character` | Pose state, expression state, skeleton mapping, customization |
| `Scene` | Objects, shots, lighting, timelines |
| `Project` | Scenes, character definitions, settings |

**Value objects.** Immutable types that carry meaning without identity:

- `ShotId`, `KeyframeId` — GUID-based identity
- `CameraKeyframeState`, `ObjectState` — state snapshots
- `FocalLength`, `AspectRatio`, `SensorDimensions` — typed domain values
- `Pose`, `Expression` — character state snapshots

**Domain events.** Cross-context communication without coupling:

- Shot-level: `DurationChanged`, `KeyframeAdded`, `KeyframeRemoved`, `KeyframeMoved`
- Selection: `ElementSelected`, `SelectionCleared`
- Scene-level: events for object add/remove, lighting changes

**Command pattern.** `ICommand` with `Execute()` / `Undo()` / `Redo()` for all user actions. Enables undo stack and action replay.

### 6.2 What We Skip

**Repositories.** Unity manages object lifecycles through `MonoBehaviour`, `Instantiate`, `Destroy`, and scene serialization. Wrapping that in repositories duplicates what the engine already does.

**Application services / use case classes.** The `ApplicationController.Update()` frame loop is inherently imperative. Routing every frame tick through application service abstractions adds indirection in a hot path.

**Full layered architecture** (domain → application → infrastructure → presentation). Unity blurs these layers by design. `MonoBehaviour` is simultaneously presentation, infrastructure, and sometimes domain logic.

### 6.3 The Split Model

The key architectural pattern: **pure C# for domain logic, thin MonoBehaviour wrappers for scene graph integration.**

- **Pure C# domain layer**: `ShotSetup`, `CameraAnimation`, `KeyframeManager<T>`, `Character`, `Pose`, value objects. No Unity dependencies. Testable with standard xUnit without Unity's test runner.
- **MonoBehaviour integration layer**: `SceneElement`, `VirtualCameraRig`, `ApplicationController`. Thin wrappers that delegate to the domain layer. These need Unity to run but contain minimal logic.

This gives testability without the ceremony of formal DDD layering.
