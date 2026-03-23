# RTG vs Fram3d Gizmo Implementation Analysis

**Date:** 2026-03-23
**Source:** Runtime Transform Gizmos (Standard) package in ~/Code/projects/Kinemachine/

## Overview

Compared the third-party Runtime Transform Gizmos (RTG) package against Fram3d's gizmo implementation. RTG is a mature, feature-rich package with ~150 C# files covering transform gizmos, collider gizmos, light gizmos, undo, and a custom editor UI. The analysis focuses on the two areas most relevant to Fram3d: **display/rendering** and **screen-size scaling**, plus other findings worth adopting.

---

## 1. Rendering

### What's the same
Both use `ZTest Always` + `ZWrite Off` on a custom URP shader to render gizmos on top of everything. Both use flat unlit color output for handles.

### What RTG does differently

| | Fram3d | RTG |
|---|---|---|
| **Shader** | Flat color, no lighting | Per-shape branching in the pixel shader — inset box/circle/cylinder raycasting, arc clipping, optional lighting (nDotL + 0.1 ambient), cull sphere alpha dimming |
| **Render pipeline integration** | Relies on render queue (`Overlay+1`) | Custom `ScriptableRendererFeature` with explicit `RenderPassEvent.AfterRenderingTransparents` |
| **Material management** | One `new Material()` per handle (6-10 instances) | Dictionary cache keyed on `(zTest, zWrite, cullMode)` — reuses materials across handles with same render states |
| **Per-pixel raymarching** | None — mesh geometry is what you see | Torus, inset box, inset circle, cylinder shapes are ray-intersection tested in the pixel shader. Mesh is a bounding proxy; shader carves the true shape |

### Key insight: Per-pixel raycast rendering
RTG's rotation rings look perfectly smooth at any zoom level because the mesh is just a bounding quad and the pixel shader analytically computes whether each pixel is inside the torus cross-section. Fram3d's CPU-generated torus mesh (48×8 = 384 vertices) looks fine but will show faceting up close. For a previsualization tool this is acceptable.

### Key insight: ScriptableRendererFeature
RTG's `RTMainSRF` (ScriptableRendererFeature) injects two render passes:
- `RTCameraBGSRP` at `AfterRenderingSkybox` — background gradient
- `RTMainSRP` at `AfterRenderingTransparents` — grid + gizmos

This is more robust than render queue ordering. Queue `Overlay+1` works, but render features give explicit control over timing relative to post-processing.

---

## 2. Screen-Size Scaling

### Fram3d's approach
```csharp
scale = distance * CONSTANT_SCREEN_SIZE   // CONSTANT_SCREEN_SIZE = 0.15
```

### RTG's approach
```
Perspective:  scale = distance / (0.046 * pixelHeight * 65.0 / fieldOfView)
Orthographic: scale = (orthoSize * 2) / (0.052 * pixelHeight)
```

### Comparison

| | Fram3d | RTG |
|---|---|---|
| **FOV compensation** | None — gizmo screen size changes when FOV changes | `65/FOV` factor makes gizmo constant across FOV changes |
| **Resolution independence** | None — different sizes on 1080p vs 4K | `pixelHeight` divisor normalizes to screen resolution |
| **Orthographic cameras** | Not handled (linear distance scaling breaks in ortho) | Separate formula using `orthoSize` instead of distance |
| **Where scale is applied** | Once, on the root transform | On every dimension individually via `FVal()` |

### Why this matters for Fram3d
Fram3d changes FOV via focal length. The current formula `distance * 0.15` means the gizmo changes screen size when the user adjusts focal length — a live bug.

### RTG's FOV correction explained
`65/FOV` means at 65° FOV the factor is 1.0 (their reference point). At 130° FOV (wider), the divisor doubles, halving the scale — compensating for wide FOV making objects appear smaller. At 32.5° FOV (telephoto), the divisor halves, doubling the scale.

---

## 3. Hit Detection

### Fram3d's approach
`Physics.Raycast` against `MeshCollider` on each handle. Requires colliders on the gizmo layer.

### RTG's approach
Zero physics. Every handle type implements `Raycast(ray, out float t)` with analytical CPU math — line-segment closest-point for sliders, ray-quad/ray-sphere/ray-torus for caps, with screen-space pixel tolerance for thin geometry. Winner selected by hover priority then closest `t`.

| | Fram3d | RTG |
|---|---|---|
| **Physics dependency** | MeshCollider per handle, Physics.Raycast | None — pure analytical math |
| **Hover padding** | None — hit the mesh or miss | Configurable `hoverPadding` inflates all hit volumes |
| **Priority system** | First Physics hit wins | Priority-then-distance: plane sliders yield to axis caps even if closer |
| **Thin geometry** | Thin shafts are hard to click | Screen-space tolerance makes thin lines easy to grab |

### Key insight: Hover padding
Makes thin arrow shafts and ring cross-sections much easier to grab without making them visually thicker. Fram3d's `MeshCollider` approach means clickable area = visible geometry, which works for chunky arrow cones but frustrates on thin shafts and rings.

---

## 4. Drag Plane Construction

### Fram3d's approach
`DragSession` + `TransformOperations.ProjectOntoAxis` — projects mouse ray onto the drag axis directly.

### RTG's approach
Constructs a synthetic drag plane optimal for the current camera angle:
- **Single-axis drag:** plane normal = `normalize(cross(cross(cameraForward, axis), axis))` — faces camera while containing the drag axis
- **Two-axis drag:** plane normal = `cross(axis0, axis1)` — spanned by both axes
- **Uniform scale:** plane normal = `cameraForward` — faces camera directly
- **Rotation:** screen-space tangent vector at grab point, dotted with mouse delta each frame

### Why this matters
When viewing an axis nearly edge-on, ray-axis projection becomes degenerate — small mouse movements cause huge jumps. RTG's plane construction avoids this by always picking a plane where the mouse ray intersects cleanly.

---

## 5. Other Interesting Findings

### Custom transform hierarchy
RTG doesn't use Unity `Transform` for internal math. `GizmoTransform` is a pure C# parent-child tree with position/rotation/scale propagation. Handle positions are computed analytically; geometry drawn directly via render graph. No `transform.position = ...` each frame.

### Render-hover connections
When hovering an arrow tip (cap), the associated shaft (line slider) also highlights via `AddRenderHoverConnection()` — a declarative linkage, not manual tracking.

### Scale drag divides by zoom scale
`scaleOffset = dot(dragDelta * sensitivity / zoomScale, axis)` — dividing by `zoomScale` cancels the perspective effect, making drag feel constant regardless of camera distance.

### Rotation drag uses screen-space tangent
On drag start, projects the rotation circle's tangent at the grab point into screen space. Each frame, dots the mouse delta with this tangent vector. Feels natural regardless of where on the ring you grabbed.

### Cull sphere on rotation rings
The back half of a rotation ring is dimmed via `_CullAlphaScale` in the pixel shader — analytical test, no second render pass.

---

## Adoption Priority

1. **FOV-compensated scaling formula** — highest impact, easiest change. One function update. Fixes live bug with focal length changes.
2. **ScriptableRendererFeature** — medium effort, prevents ordering issues with post-processing.
3. **Hover padding** — inflate MeshCollider meshes or switch to analytical raycasting for thin geometry.
4. **Camera-optimal drag plane** — fixes edge-on axis dragging jitter. Add to `DragSession` in Core.
