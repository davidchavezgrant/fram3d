# Fram3d — Recommended Build Order

**Date**: 2026-03-12
**Purpose**: Resequenced roadmap optimized for dependencies, core-first development, and time-to-useful.
**Note**: This is a general guide. Features can be skipped or reordered ad-hoc. The milestone specs and feature definitions are unchanged — only the sequence.

---

## Dependency Map

```
1.1 Virtual Camera
 └→ 1.2 Camera Overlays
 └→ 1.3 Scene Management
     └→ 1.4 Shot Sequencer
         └→ 1.5 Keyframe Animation
             └→ 2.1 Undo/Redo
             └→ 2.2 Save/Load
             └→ 2.4 Export
             └→ 3.1 Lighting
             └→ 3.2 Characters ←── the complexity cliff
                 └→ 3.3 Facial Expressions
                 └→ 3.4 Camera Follow/Look-at
                     └→ 3.5 Snorricam
                 └→ 3.6 Object Linking (benefits from 3.2 but doesn't strictly require it)
             └→ 3.8 Selection Refinements (extends 1.3)
             └→ 3.9 Multi-camera (extends 1.5)
             └→ 1.7 Slow-motion (extends 1.5)
     └→ 2.3 Asset Import
         └→ 3.7 Set Decoration Library
         └→ 5.1 Premade Environments
 └→ 1.6 Viewport Panel System (extends 1.3.5 Director View)
     └→ 5.3 2D Designer (panel mode)
         └→ 5.4 Set Builder (uses 2D Designer)
 └→ 5.2 Script Import (needs 1.4.4 multi-scene + 3.2 characters)
 └→ 4.x AI features (need 1.5 + 3.2)
```

---

## Recommended Sequence

### Phase 1 — The Camera (weeks 1–4)
*You're building a camera tool. The camera comes first. Everything else supports it.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 1 | **1.1 Virtual Camera** | The product IS the camera. Movement, lens, focus, DOF, shake. |
| 2 | **1.2 Camera Overlays** | Aspect ratio defines the frame. Without it, you can't compose. HUD gives the DP data. |

**Exit criteria:** You can move a physically-accurate camera through 3D space, see the correct aspect ratio, toggle frame guides, and read focal length / height / AOV from the HUD.

---

### Phase 2 — The Scene (weeks 5–8)
*Give the camera something to look at. Click things, move things, see the result through the camera.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 3 | **1.3 Scene Management** | Selection, gizmos, ground plane, duplication, director view. The interaction model. |

**Exit criteria:** You can place objects, select them, move/rotate/scale with gizmos, duplicate, and switch between shot view and director view.

---

### Phase 3 — Time (weeks 9–14)
*Turn a static camera position into a moving shot. Multiple shots, a timeline, keyframes, playback.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 4 | **1.4 Shot Sequencer** | Shot model, sequencer UI, global object timeline. Multi-scene data model (1.4.4) built here but scene tab UI is the last feature in the milestone. Minimap (1.4.5) deferred. |
| 5 | **1.5 Keyframe Animation** | Timeline editor, tracks, stopwatch, keyframe interaction, interpolation, playback, path visualization. |

**Exit criteria:** You can create multiple shots, animate the camera and objects with keyframes, scrub the timeline, and play back a sequence. The core previs loop works end-to-end.

**Note on 1.4.4 (multi-scene):** Build the data model (Project → Scene → Shots) from the start so you don't have to refactor later. The scene tab switching UI can be the last thing implemented in this phase.

---

### Phase 4 — Persistence & I/O (weeks 15–20)
*A tool people can't save their work in doesn't get used.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 6 | **2.1 Undo/Redo** | Safety net. Without undo, users can't experiment. Wire the command pattern infrastructure incrementally as features are built, but finalize and test the full stack here. |
| 7 | **2.2 Save/Load** | Project persistence. Creation wizard, file format, auto-save, crash recovery. Scene persistence (2.2.5) included. |
| 8 | **2.3 Asset Import** | Drag-and-drop FBX/OBJ/glTF. Auto-colliders. Asset library panel. Without this, users are stuck with whatever ships built-in. |
| 9 | **2.4 Export** | Image (2.4.1) and video (2.4.2) export are essential. Storyboard export (2.4.3) and NLE export (2.4.4) can follow later. |

**Exit criteria:** The tool is genuinely usable. Save work, undo mistakes, import models, export stills and video. This is the **alpha** — a solo filmmaker could do real previs work with this.

**Implementation note for 2.1:** The command pattern infrastructure should be scaffolded early (Phase 1–3). Each new feature adds its Undo/Redo commands as it's built. Phase 4 is where you finalize, stress-test, and handle the hard edge cases (cross-shot undo, animate mode compound steps, gesture coalescing).

---

### Phase 5 — Scene Dressing (weeks 21–26)
*Solve the empty canvas problem. Give users environments and props so they can start blocking immediately.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 10 | **3.1 Lighting** | Simplest production feature. Validates property animation pipeline. Makes the viewport visually useful. A lit room is already useful for DP previs even without characters. |
| 11 | **3.7 Set Decoration Library** | Built-in props (3.7.1) ship with the app — tables, chairs, walls, vehicles. The browsing UI is simple. Marketplace integration (3.7.2) and user asset management (3.7.3) can follow later. |
| 12 | **5.1 Premade Environments** | 12-15 ready-to-use sets. Click "Office" and start blocking. Code is trivial (load a saved scene). Content creation runs in parallel from Phase 4 onward. |

**Exit criteria:** Users open the app, pick a premade environment (or browse built-in props), the scene has lighting, and it looks like a real location. No more blank canvas.

**Content pipeline note:** The actual 3D assets and environment models are art work that can be produced in parallel with Phase 4 code. The code for browsing and placing them is simple — don't let content creation block code progress.

---

### Phase 6 — Characters (weeks 27–38)
*The complexity cliff. This is where Fram3d becomes a real previsualization tool — and where the hardest engineering work lives.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 13 | **3.2 Characters** | Mannequin placement, body customization, pose library, IK posing, character animation, walk cycles. This is the longest single milestone — plan accordingly. Custom character import (3.2.5) can be deferred to end of phase. |
| 14 | **3.4 Camera Follow & Look-at** | Immediately useful after characters. "Follow the detective down the hallway" is a bread-and-butter previs shot. Every director will want this. |
| 15 | **3.6 Object Linking & Grouping** | Prop holding — character carries a briefcase, picks up a weapon. Pickwhip linking, anchor points. Core blocking capability. |

**Exit criteria:** You can drop characters into a premade environment, pose them, animate their movement, have the camera follow them, and link props to their hands. This is the feature set that differentiates Fram3d from every 2D tool.

**Why 3.6 before 3.3:** Prop holding (object linking) is fundamental blocking — a character needs to carry things, sit in chairs, interact with the environment. Facial expressions are emotional nuance. Blocking comes before performance.

---

### Phase 7 — Character Extensions (weeks 39–44)
*Enhance characters with expressions and specialized camera rigs.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 16 | **3.3 Facial Expressions** | Blend shapes, intensity slider, eye direction, expression animation. Adds emotional performance to blocking. |
| 17 | **3.5 Snorricam** | Body-mounted camera. Niche but distinctive. Requires character animation (3.2) and camera follow patterns (3.4). |

**Exit criteria:** Characters can emote and have eye direction. Snorricam shots are possible.

---

### Phase 8 — Polish & Views (weeks 45–52)
*Quality-of-life improvements and new ways to see the scene. The tool works — now make it work better.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 18 | **3.8 Selection Refinements** | Multi-select, grid snapping, custom interpolation curves. Scenes are getting complex — multi-select is needed. Bezier curves make animations professional. |
| 19 | **1.6 Viewport Panel System** | Multi-panel layouts. Side-by-side views. Pairs naturally with 2D Designer. |
| 20 | **5.3 2D Designer** | Top-down orthographic view. Directors have drawn overhead blocking diagrams for a century. Now it's a viewport panel mode. |
| 21 | **5.2 Script Import** | Final Draft / Fountain parsing. Auto-create scenes, character placeholders, dialogue library. Depends on characters (3.2) and multi-scene (1.4.4). |
| 22 | **1.7 Slow-motion** | Per-shot speed factor. Nice-to-have, not core. Simple implementation (playback-time transform). |
| 23 | **1.4.5 Timeline Minimap** | Overview navigation. Useful once projects have many shots, but not essential. |

**Exit criteria:** The tool is polished enough for production use. Multi-select, precision placement, professional animation curves, overhead view, script-to-previs workflow, slow-motion.

---

### Phase 9 — Multi-camera (capstone)
*Get single-camera workflow polished before introducing multi-cam complexity.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 24 | **3.9 Multi-camera** | Up to 4 cameras per shot, coverage splitting. Builds on mature keyframe and sequencer foundations. This is the professional previs capstone. |

---

### Phase 10 — Set Building & Advanced Export

| # | Milestone | Rationale |
|---|-----------|-----------|
| 25 | **5.4 Set Builder** | Room construction, wall drawing. Uses 2D Designer. |
| 26 | **3.7.2 Marketplace Integration** | Sketchfab, Asset Store, TurboSquid, Mixamo browsing (if not done in Phase 5). |
| 27 | **2.4.3 Storyboard Export** | PDF/image grid (if not done in Phase 4). |
| 28 | **2.4.4 NLE Export** | EDL, camera metadata (if not done in Phase 4). |

---

### Phase 11 — AI Features
*The tool must be great without AI. AI is upside, not the product.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 29 | **4.1 NL Shot Description** | Shot-from-text, cinematic vocabulary. |
| 30 | **4.2 Automatic Blocking** | Scene-from-text, blocking refinement. |
| 31 | **4.3 Camera Suggestions** | Coverage suggestions, shot list generation. |

---

### Phase 12 — Stretch Goals

| # | Milestone | Rationale |
|---|-----------|-----------|
| 32 | **5.5 AI Prop Generation** | Text-to-prop. |
| 33 | **5.6 Costume Generation** | AI clothing on mannequins. |
| 34 | **5.7 LiDAR Scanning** | Scan import + companion iOS app. |
| 35 | **5.8 Style Grading** | Maybe. |
| 36 | **5.9 AI Video Generation** | Maybe. |

---

## Key Changes from Current Roadmap Order

| Change | Current Position | New Position | Why |
|--------|-----------------|--------------|-----|
| **3.7 Set Decoration** | After 3.6 (Phase ~8) | Phase 5 (after lighting) | Users need props to build scenes. An empty grid with no built-in assets is hostile. Ship basic props early. |
| **5.1 Premade Environments** | Project 5 | Phase 5 (after lighting) | Solves the blank canvas / new user onboarding problem. Code is trivial; content creation runs in parallel. |
| **3.6 Object Linking** | After 3.5 Snorricam | Phase 6 (right after characters) | Prop holding is core blocking. A character needs to carry a briefcase before they need facial expressions. |
| **3.3 Facial Expressions** | Right after 3.2 | Phase 7 (after camera follow + linking) | Enhancement, not core. Blocking precedes performance. |
| **1.6 Viewport Panels** | Project 1 (Phase ~6) | Phase 8 | Single-viewport toggle is sufficient for early development. Panel system pairs with 2D Designer. |
| **1.7 Slow-motion** | Project 1 (Phase ~7) | Phase 8 | Nice-to-have. Not blocking any workflow. |
| **5.2 Script Import** | Project 5 | Phase 8 | Needs characters (3.2) and multi-scene (1.4.4) to be useful. Once those exist, this is a high-value workflow accelerator. |
| **1.4.5 Minimap** | Project 1 (1.4) | Phase 8 | Useful once projects are complex, not needed early. |
| **3.8 Selection Refinements** | After 3.7 | Phase 8 (before multi-camera) | Multi-select becomes important as scenes grow, but not needed during early development. Comes before multi-camera so the tool is more capable when multi-cam complexity arrives. |

## Unchanged (already correct)

- 1.1 → 1.2 → 1.3 → 1.4 → 1.5 sequence (Phase 1–3)
- 2.1 → 2.2 → 2.3 → 2.4 sequence (Phase 4)
- 3.1 Lighting before 3.2 Characters (lighting validates property animation, is simpler)
- 3.4 Camera Follow right after 3.2 Characters
- 3.5 Snorricam after 3.4 Camera Follow
- 3.9 Multi-camera as capstone
- 4.x AI features near the end
- 5.5–5.9 stretch goals at the end
