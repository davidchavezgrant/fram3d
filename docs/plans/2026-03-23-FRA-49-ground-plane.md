# FRA-49: Ground Plane Implementation Plan

**Date**: 2026-03-23
**Ticket**: FRA-49
**Spec**: milestone-2.1-scene-management-spec.md §2.1.3

---

## Summary

Infinite ground plane with visible grid at Y=0. Major lines at 1m, minor lines subdividing. Grid fades with distance — no hard edge. Always visible, no toggle. Clicking the ground plane deselects the current selection.

## Approach

Shader-based infinite grid: a large quad at Y=0 with an analytical grid shader. Grid lines computed in fragment shader using `frac()` + `fwidth()` for anti-aliased lines. Distance-based alpha fade. A flat BoxCollider enables selection raycasts (clicking ground = deselect via existing SelectionRaycaster path).

## Files

| File | Layer | Purpose |
|------|-------|---------|
| `Unity/Fram3d/Assets/Shaders/InfiniteGrid.shader` | Engine | URP shader: analytical grid lines, distance fade, alpha blend |
| `Unity/Fram3d/Assets/Scripts/Engine/Integration/GroundPlane.cs` | Engine | MonoBehaviour: creates quad mesh + material + collider at runtime |

## Implementation

### Phase 1: Shader

1. Create `InfiniteGrid.shader` — URP-compatible, transparent, renders before scene geometry
2. Fragment shader computes world-space grid lines from world position
3. Major grid (1m) at full intensity, minor grid (0.25m) at reduced intensity
4. Anti-aliased lines via `fwidth()` screen-space derivatives
5. Distance fade: exponential falloff from camera, grid invisible at ~100m
6. Renders below scene geometry (no ZWrite, early queue)

### Phase 2: MonoBehaviour

1. Create `GroundPlane.cs` — spawns a large quad mesh at Y=0
2. Creates material from the InfiniteGrid shader at runtime
3. Adds a flat BoxCollider for raycast hit detection
4. No ElementBehaviour — raycast hits return null element → triggers deselect

### Phase 3: Integration

1. Add GroundPlane to the scene (or have it self-create)
2. Verify clicking ground deselects (existing SelectionRaycaster handles this)
3. Verify grid renders below all scene elements

### Phase 4: Ground clamping

1. Change `Element.Position` from auto-property to property with backing field
2. Setter clamps Y >= 0
3. Covers camera, elements, and drag — single enforcement point
4. xUnit tests: verify Position.Y is clamped to 0 when set to negative values

## Design decisions

- **Quad size**: 1000x1000m — large enough that the fade hides the edge
- **Minor grid subdivision**: 4 subdivisions per meter (0.25m lines)
- **Line color**: Neutral gray on dark background, axis-colored lines for X (red) and Z (blue) at origin
- **Render queue**: Geometry-1 — renders before opaque objects so elements sit on top
