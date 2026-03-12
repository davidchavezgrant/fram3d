# Architectural Decisions

Confirmed decisions and pending considerations that shape the implementation.

---

## Confirmed

> **File format**: Deferred to implementation. Human-readable preferred for git diffing and debugging. Must serialize full project state (see 4.2 spec).

> **Playback frame dropping**: When scenes are too heavy for real-time playback, drop frames (maintain timing, skip visual updates) rather than slowing down. The playback system tracks wall-clock time and evaluates the timeline at the correct time regardless of render performance.

> **Rotation storage**: Quaternions internally, pan/tilt/roll displayed in UI. Prevents gimbal lock transparently.

> **Recording**: Only changed properties get keyframed (not all properties). Per-track stopwatch model (AE/Premiere). Stopwatches default to off.

> **Undo model**: Global stack, not per-shot. Cross-shot undo does not auto-switch the active shot.

> **Delete behavior**: Delete key = keyframe only. Cmd+Delete = element. Context-sensitive to avoid ambiguity.

---

## Pending: Scene Serialization & Lazy Loading

**Source**: Multi-Scene Project Structure spec — unlimited scenes, lazy load/lazy render

Scenes must be independently loadable without pulling in sibling scene data. Only the active scene's heavy data (meshes, textures, keyframes) should be in memory; inactive scenes keep only lightweight metadata (element list, shot count, character assignments) for fast tab switching.

**Implications for file format:**
- The project file format needs clear scene boundaries — either separate chunks within a single file, or separate files per scene within a project bundle (e.g., `project.fram3d/scene-1.json`, `project.fram3d/scene-2.json`)
- Scene-level save should be possible without re-serializing the entire project
- Consider: a project manifest file (settings, character definitions, scene order) + individual scene files. This also makes scene duplication and deletion cheap (copy/delete a file rather than splicing a monolithic blob)
- Character definitions live at project level but are referenced by scenes — avoid duplicating character data across scene files

**Resolve before Save/Load spec (Milestone 4.2).**

---

## Pending: Infrastructure to Build Early

Cross-cutting systems that multiple downstream features depend on. Build during Phase 4 timeframe to avoid reimplementing for each feature.

- **Settings / Preferences panel:** Centralized settings panel so new settings can be added incrementally as features ship. Avoids scattering configuration UIs across the application.
- **Panel / docking system:** General panel system with docking support. Downstream milestones (Elements panel, pose library, Assets panel) all require dockable panels.
