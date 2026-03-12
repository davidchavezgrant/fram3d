# Fram3d Architecture Review — Chief Architect's Report

**Date**: 2026-03-12
**Scope**: All docs in `docs/reference/` plus critical specs (3.1, 3.2, 4.1, 4.2, 6.1, 6.2, 6.3, 9.1)

---

## Executive Summary

The documentation is genuinely excellent — thorough specs, disciplined terminology, smart decisions about what DDD concepts to adopt and which to skip. The domain language document alone is better than what I've seen in most shipped products. But there are structural problems hiding under the surface: cross-context dependencies that the bounded context model doesn't account for, a critical runtime pipeline that isn't documented, and several places where the architecture papers over hard problems with clean-sounding abstractions.

Organized from **most dangerous** to **least dangerous**.

---

## Critical: Things That Will Cause Rearchitecting

### 1. The Evaluation Pipeline Doesn't Exist

Every frame, the system must evaluate state at the current playhead time. This evaluation has strict ordering dependencies:

```
1. Compute global time (shot start + local time * speed factor)
2. Evaluate element keyframes at that time (position, rotation, scale)
3. Evaluate link chains in dependency order (parents before children)
4. Evaluate character poses (joint rotations) at that time
5. Evaluate camera keyframes at that time (per-shot)
6. Apply follow/watch (depends on character position from steps 2-4)
7. Apply shake (cosmetic overlay, per-shot)
8. Push all results to Unity transforms
```

This pipeline crosses **every bounded context**: Sequencing (time), Scene (elements), Characters (poses), Camera (follow, shake), and the integration layer (Unity transforms). It runs every frame. Its ordering matters — follow depends on character position, IK depends on bone transforms, link chains depend on parent evaluation.

**This is the runtime heartbeat of the application and it's not documented anywhere.**

The split model says "pure C# for domain logic, thin MonoBehaviour wrappers." But keyframe evaluation, IK solving, link chain resolution, and follow damping are all per-frame computations that need to read from and write to Unity transforms. The "thin wrappers" for this pipeline will be the thickest code in the project.

**Question**: Who orchestrates this pipeline? A single coordinator that knows about all contexts (destroying the bounded context model)? A chain of domain events (introducing frame-latency between steps)? An update loop in the integration layer that calls into each context in order?

### 2. Who Owns the Global Element Timeline?

The global element timeline is the most important data structure in the system. Elements (Scene context) animate on it. Shots (Sequencing context) are windows into it. Linking events (Scene context) live on it. Keyframes (Sequencing context) populate it.

The docs never say which context owns it.

- If **Sequencing** owns it: Scene must go through Sequencing to evaluate element positions. But Scene owns Element and needs to push transforms to Unity. Scene now depends on Sequencing depends on Scene (for element identities).
- If **Scene** owns it: Sequencing needs Scene to know where elements are during playback. Shot track, timeline UI, and keyframe editor all live in Sequencing but need data from Scene.

This is probably the single most important architectural decision, and it's unresolved. It determines the dependency graph between your two largest contexts.

**My take**: The timeline is infrastructure, not domain logic for either context. Consider a shared kernel (`Fram3d.Core` or `Fram3d.Timeline`) that owns the timeline data structures, with Scene and Sequencing both depending on it.

### 3. Bounded Context Boundaries Don't Match Runtime Dependencies

The bounded context map shows 9 clean assemblies. But the actual dependencies form a web:

- **Undo (Persistence)** wraps operations from Camera, Scene, Sequencing, and Characters. Persistence depends on everything, or everything depends on Persistence.
- **Animation tracks** (Sequencing) control properties of Elements (Scene), Lights (Scene), and Cameras (Camera). Sequencing needs to know about the property types of things it doesn't own.
- **Follow/Watch** (Camera) needs character position (Characters/Scene). Camera depends on Scene depends on Characters.
- **Export baking** (Export) needs to evaluate shake (Camera), follow (Camera), snorricam (Camera), and produce keyframes (Sequencing). Export depends on Camera and Sequencing.
- **Linking** (Scene) needs to evaluate during the pipeline, reading bone transforms from Characters.

**Question**: Have you drawn the actual assembly reference graph? Not the bounded context map — the `[assembly: InternalsVisibleTo]` and reference lines that will appear in the .asmdef files? I suspect it's a near-complete graph, not the tree the map implies.

The command pattern (undo) in particular should be a shared kernel, not a bounded context. It's infrastructure that every context uses, like event bus or value objects.

---

## Serious: Things That Will Cause Significant Rework

### 4. Shot Reordering + Global Element Timeline = Semantic Confusion

The global element timeline model is elegant on the happy path. But shot reordering exposes a fundamental tension.

Say a director blocks a scene: Shot A (0–5s) has a character walking left-to-right, Shot B (5–10s) has the character sitting down. Element keyframes at t=0 (standing left), t=5 (standing right), t=7 (sitting).

Now the director reorders: drags Shot B before Shot A. The shot track visually shows "sitting" then "walking." But the element keyframes haven't moved — they're on the global timeline at their original times. Shot B (now first, still 5–10s on the global timeline) still shows the character sitting. Shot A (now second, still 0–5s) still shows walking.

**The visual shot order no longer matches the temporal element animation.** The director sees "sitting shot, then walking shot" but the character actually walks first, then sits.

The ripple editing feature addresses boundary dragging but not full reordering. The spec says "names are unchanged — ordering does not trigger renaming" but it's silent on what happens to the global timeline mapping.

**Question**: When shots are reordered via drag-and-drop, do their global timeline time ranges swap? If Shot A was 0–5s and Shot B was 5–10s, does reordering make Shot B map to 0–5s and Shot A map to 5–10s? If so, that's a massive data transformation (all element keyframes need remapping). If not, the visual order and temporal order diverge.

### 5. Legacy UI Is a Velocity Risk for This Amount of UI

The tech stack says: legacy `UnityEngine.UI`, all programmatic, no prefabs, no UXML.

Fram3d's UI is not simple:
- Timeline editor with scrollable, zoomable tracks, sub-tracks, per-keyframe shapes, drag-to-reposition, snap indicators
- Shot track with thumbnails, drag-and-drop reordering, boundary dragging, resize tooltips
- Elements panel with flat list, link indicators, group markers, pickwhip drag
- Pose library with thumbnail grid, categories, search
- Camera preview elements above shots
- Active angle track with colored segments, draggable cut points
- View layout system with 3 arrangement modes
- Gutter system with JetBrains-style toggle tabs
- Property editing for elements, cameras, lights, characters (height/build sliders, tint, etc.)

Building **all** of this programmatically in legacy UI means:
- No visual design tools (every layout is math in code)
- Canvas rebuild performance issues with hundreds of timeline elements
- No databinding (manual sync between domain and UI everywhere)
- No styling system (every visual property is per-element code)

This isn't necessarily wrong — but the tradeoff against UI Toolkit (which has CSS-like styling, databinding, and is Unity's recommended path) should be explicitly rationalized in `decisions.md`. The current decision is implicit.

### 6. The "No Modes" Philosophy Denies Reality

The domain language says "Fram3d has no modes." But:

- **Posing context** (6.1.3): Double-click enters, click-away exits. Joint indicators appear. Gizmo changes. Available actions change.
- **Group editing** (6.3.2): Double-click enters, Escape exits. Non-group elements dim to 30%. Available actions change.
- **Recording** (stopwatch): When on, manipulations create keyframes. When off, they don't. Available actions change.

These are modes. Calling them "selection contexts" is redefining the word. The philosophy is directionally correct — make transitions seamless, don't trap the user — but the absolutist framing ("no modes") could cause developers to avoid adding clear visual indicators when the interaction context changes, which is exactly when users need them most.

**Recommendation**: Soften to "minimize modes" and require that every interaction context change has a clear visual indicator, rather than pretending the change doesn't exist.

### 7. Walk Cycles Are Massively Underspecified

Spec 6.1.4 contains this bullet point: "Walk cycles with custom gaits (limping, sneaking)."

This is an entire R&D project compressed into 8 words. It requires:
- Procedural locomotion system (or pre-baked animation clips?)
- Foot-ground contact solving (IK for uneven terrain?)
- Speed-to-gait-to-animation mapping
- Parameterized gait styles (how are "limping" and "sneaking" defined?)
- Integration with the keyframe system (can you keyframe gait parameters?)
- Transition blending between gaits

The character spec is 1400+ lines. Walk cycles get less attention than the tint color feature. This should be its own sub-feature with its own spec, or it should be explicitly deferred as "walk animations are pre-baked clips, not procedural" for v1.

### 8. No Property Editing Pattern

The domain language retires "inspector" but doesn't replace it with a concept. Characters need height/build sliders, tint color, expression controls. Cameras need focal length, aperture, focus distance. Lights need intensity, color, range, cone angle. Elements need name, transform values. Links need anchor point XYZ fields.

**Where do all these properties live in the UI?**

The specs describe per-feature property UIs scattered across different locations (inline on elements, in panels, in context menus), but there's no unifying pattern. Without one, each milestone will invent its own property editing approach, leading to inconsistency.

**Question**: Is there a properties sidebar? A contextual panel that changes based on selection? Inline controls on the element? A floating popover? This needs to be a first-class interaction pattern in `interaction-patterns.md`.

---

## Moderate: Things That Will Cause Confusion or Bugs

### 9. Where Does `CameraElement` Live?

The naming conventions put `CameraElement` in `Fram3d.Scene` (alongside `Element`, `CharacterElement`, `LightElement`). But the bounded context map puts all camera functionality in `Fram3d.Camera`.

If `CameraElement` is in Scene:
- Camera context can't define its own aggregate root
- Camera context needs to reach into Scene to access the element it extends
- Camera-specific properties (focal length, DOF) live in a different assembly than the element they belong to

If `CameraElement` is in Camera:
- Scene context (which manages selection, gizmos, element lists) needs to reference Camera
- Every context that deals with "all elements" needs to reference every specialized context

Same problem applies to `CharacterElement` vs. Characters context, and `LightElement` vs. Scene.

**My take**: The base `Element` type and the `*Element` specializations should probably all be in a shared kernel or in Scene, with specialized behavior (camera rig, IK, light controls) living in their respective contexts and operating on the element through interfaces or events. But this means the "specialized element" pattern needs an explicit dependency direction.

### 10. Keyframe Snap vs. Frame Boundaries

Tuned constants: "Snap to 0.1s during drag" for keyframes.
Interaction patterns: "Snaps to frame boundaries" for shot boundary dragging.

At 24fps, a frame is 0.04167s. A 0.1s snap is 2.4 frames — not a frame boundary. At 25fps, 0.1s = 2.5 frames. At 30fps, 0.1s = 3 frames exactly.

A previsualization tool for filmmakers should be frame-accurate. Keyframes that can't land on frame boundaries will produce animation that doesn't align with cuts, exports, or NLE imports.

**Question**: Should keyframe snapping be to frame boundaries (1/fps) rather than 0.1s? Or is 0.1s intentional to allow sub-frame precision? If the latter, how does this interact with export (which must produce integer frames)?

### 11. `architecture.md` Is Empty

The file contains only `# Fram3d Architecture`. The bounded context map references it: "See `fram3d-architecture.md` section 12 for the domain modeling approach." This reference is broken.

The architecture lives scattered across domain-model.md, bounded-context-map.md, decisions.md, build-order.md, and prior-codebase-lessons.md. There's no central document that ties these together and addresses the cross-cutting concerns (evaluation pipeline, event system, dependency graph).

### 12. No Event System Design

The domain model says "fire an event rather than creating a direct dependency" for cross-context communication. But no event system is designed:
- No event bus or pub/sub pattern specified
- No events enumerated
- No threading model (same-frame delivery? deferred? queued?)
- No ordering guarantees

Cross-context events needed include: shot selected, element added/deleted, keyframe changed, recording toggled, link created, shot duration changed, shot reordered, element deleted (with cascading cleanup needed in timeline, links, camera follow targets, etc.).

Without this, developers will use direct method calls, creating the coupling the bounded contexts are supposed to prevent.

### 13. The Cinemachine Abstraction Will Be Tested

The prior codebase's `ICameraState` (49-line adapter) is praised as the cleanest boundary. But upcoming features will stress it:

- **Follow** (6.2): Cinemachine has damping, targeting, orbiting built in. Will you use it or reimplement?
- **Snorricam** (7.2): Body-mounted camera. Cinemachine has third-person follow but needs custom adaptation.
- **Multi-camera** (9.1): 4 virtual cameras per shot. Cinemachine manages virtual camera priority.
- **DOF** (1.1.5): Cinemachine has its own lens system.

The 49-line adapter was designed for simple camera movement. Follow, snorricam, and multi-cam are substantially more complex. Either the adapter grows into a large abstraction layer, or you bypass it for advanced features (losing the clean boundary).

**Question**: Is the plan to use Cinemachine's built-in follow/targeting features (accepting the dependency) or to reimplement follow/watch/snorricam in pure domain code (accepting the development cost)?

### 14. Slow-motion Is in the Wrong Spec

Slow-motion (3.1.4 in the shot track spec) is actually Milestone 8.4 (Phase 8 in the roadmap). The spec header even notes "Multi-scene project structure has moved to milestone 4.2.5." The slow-motion section should move to its own spec or to Phase 8 planning — having Phase 8 content in a Phase 3 spec is confusing.

### 15. No Performance Budget

The docs never mention:
- Editor target frame rate (60fps? 30fps?)
- Maximum supported elements in a scene
- Maximum keyframes per track
- Memory budget
- Export render time expectations
- Startup time goals

For a real-time 3D tool, these constraints inform architecture. 200 keyframes is different from 20,000. 20 elements is different from 500. The keyframe evaluation strategy, timeline rendering approach, and asset loading strategy all depend on expected scale.

---

## Minor: Inconsistencies and Gaps

### 16. Character Import Without Mecanim

Character import (6.1.5) says "automatic rig mapping to Fram3d's pose/animation system." Prior codebase lessons say "Never use Unity's Animation/Animator." If you're not using Unity's humanoid rig system (Mecanim), you're writing your own rig mapper from scratch. This is doable but non-trivial — bone naming conventions vary across tools (Blender, Maya, Mixamo all export different hierarchies).

### 17. "No Repositories" But You Need Collections

The domain model rejects the Repository pattern. But pure C# domain types can't use Unity's scene management. Something needs to hold the collection of shots, the collection of elements, and answer queries like "all elements linked to this character at time t" or "all keyframes in the range 5.0–10.0s."

That's a repository by another name. The pattern was rejected but not replaced.

### 18. Asset Bundling Auto-Migration Risk

The save/load spec says bundling mode "can be changed at any time" with auto-migration on next save. Switching from "No" (reference) to "Yes (everything)" could silently turn a 2MB project into a 500MB file during a routine save. This should warn the user with an estimated size impact.

### 19. Scale Limits Don't Generalize

Characters clamp scale to 50%–200%. Elements have no scale limit. But a character IS an element. If someone writes generic element-scaling code, it won't enforce the character-specific limit. This is a polymorphism/LSP issue that needs either: (a) per-type scale limits on the base Element, or (b) the character override to be explicit in the posing/joint system rather than on the element.

### 20. Missing: Where Characters Appear in the Panel

The Elements panel (6.3.3) shows a flat list with a separate "Lights" section. Characters are elements. Do they get their own section? Do they appear mixed with tables and chairs? The Elements panel spec doesn't mention characters at all.

### 21. Stale Spec Reference

The bounded context map says: "See `fram3d-architecture.md` section 12 for the domain modeling approach." The file is `architecture.md` (no `fram3d-` prefix), and it's empty.

---

## What's Really Good (Don't Change These)

To be clear — this architecture has serious strengths that should be preserved:

1. **Domain Language document** — rigorous, well-reasoned, actually enforced. The frame/framing distinction, selected/active distinction, and "no modes" philosophy (even if overstated) show real thoughtfulness.

2. **Split Model** — pure C# domain + thin MonoBehaviour wrappers is the right approach for testability. The challenge is keeping the wrappers thin where the per-frame pipeline lives.

3. **Global element timeline** — the fundamental model (elements continuous, cameras per-shot) matches how directors think. The edge cases around reordering need resolution, but the core concept is sound.

4. **Prior Codebase Lessons** — knowing what went wrong (`TimelineState` god object, commands for non-user actions, `FindObjectOfType` for DI) is invaluable. These lessons should be mandatory reading before any implementation starts.

5. **Value Objects** — `FocalLength`, `KeyframeId`, `TimePosition` preventing primitive obsession. This pays for itself immediately.

6. **Command pattern scoped to user actions only** — the explicit list of what IS and ISN'T a command prevents the prior codebase's anti-pattern of routing everything through commands.

7. **Build order** — the phased approach with clear exit criteria is well-reasoned. "Ship the camera first" is correct. "Characters are the complexity cliff" is correct. "AI is upside, not the product" is correct.

---

## Recommended Next Steps

1. **Design the evaluation pipeline** — document the per-frame update order, who orchestrates it, and how it crosses context boundaries. This is the most impactful missing piece.

2. **Draw the real dependency graph** — map every assembly reference that will exist in practice. Find the cycles. Decide if you need a shared kernel (`Fram3d.Core`).

3. **Resolve timeline ownership** — explicitly decide which context owns the global element timeline data structure and how other contexts access it.

4. **Design the event system** — enumerate cross-context events, specify delivery semantics (synchronous? queued?), and build it before Phase 3.

5. **Decide on property editing pattern** — before building any UI, decide where properties are edited. This affects every milestone.

6. **Rationalize the UI framework choice** — add a decision to `decisions.md` explaining why legacy UI over UI Toolkit, with the known tradeoffs.

7. **Separate walk cycles into their own spec** — either scope them as "pre-baked animation clips" for v1 or write a real spec for procedural locomotion.

8. **Add performance budgets** — target fps, max elements, max keyframes. These constraints will change architecture decisions.
