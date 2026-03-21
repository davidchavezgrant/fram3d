# Implementation Plan: 1.1.3 Camera Body and Lens Database

**Date**: 2026-03-20
**Milestone**: 1.1.3 Camera Body and Lens Database
**Spec**: `docs/specs/milestone-1.1-virtual-camera-spec.md` §1.1.3
**Linear**: FRA-38
**Status**: Draft

---

## Summary

Load 236 camera bodies and 116 lens sets from the JSON database. Body selection changes sensor dimensions (and therefore FOV). Lens set selection changes available focal length presets. Default: Generic 35mm body, Generic Prime lens set.

## Design

**Data flow:** JSON file → Engine parses at startup → populates Core types → CameraElement references a body and lens set.

**Core types (pure C#, no JSON dependency):**
- `CameraBody` — name, manufacturer, sensorWidth, sensorHeight, format, nativeResolution, supportedFps, mount
- `LensSet` — name, type (Prime/Zoom), isAnamorphic, squeezeFactor, focalLengths (primes) or focalRange (zooms)
- `CameraDatabase` — holds lists of bodies and lens sets, provides lookup/query

**Engine (Unity, handles JSON):**
- `CameraDatabaseLoader` — reads the JSON TextAsset via Unity's JsonUtility or Newtonsoft, constructs Core types, populates CameraDatabase

**CameraElement integration:**
- `SetBody(CameraBody)` — updates SensorHeight (and SensorWidth for future use), FOV recalculates automatically
- `SetLensSet(LensSet)` — updates available presets, number keys snap to lens set focal lengths instead of generic
- Body and lens set are independent choices (mix and match freely)

**Generics not in JSON:** The spec requires "Generic 35mm, Super 35mm, 16mm, Super 16mm, 8mm" bodies and a "Generic Prime" lens set. These aren't in the database JSON — they'll be hardcoded as defaults in CameraDatabase.

**Debug UI:** Simple editor window (Fram3d > Camera Debug) showing current body/lens set with dropdowns to switch. Temporary until FRA-123 (Properties Panel).

---

## Phases

### Phase 1: Core types

- `CameraBody` class
- `LensSet` class with factory methods to construct from grouped lens entries
- `CameraDatabase` with default generics
- `CameraElement.SetBody()` and `CameraElement.SetLensSet()`
- Update number key presets to use lens set focal lengths
- xUnit tests

### Phase 2: Engine — JSON loading

- Copy database JSON to `Unity/Fram3d/Assets/StreamingAssets/`
- `CameraDatabaseLoader` parses JSON, groups lenses by set name, constructs Core types
- `CameraBehaviour` populates database on Awake and sets defaults on CameraElement

### Phase 3: Debug UI

- Editor window with body/lens set dropdowns
- Shows current body name, sensor dimensions, lens set name, available focal lengths

---

## Exit Criteria

- [ ] 236 camera bodies and 116 lens sets loaded from JSON
- [ ] Switching body changes sensor dimensions and FOV
- [ ] Switching lens set changes available presets (number keys snap to lens set focal lengths)
- [ ] Default: Generic 35mm body, Generic Prime lens set
- [ ] Current focal length preserved when switching body or lens set
- [ ] Debug editor window for switching body/lens set
- [ ] xUnit tests for body/lens set switching behavior
