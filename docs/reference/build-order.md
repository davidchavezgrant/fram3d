# Fram3d — Recommended Build Order

**Date**: 2026-03-12
**Purpose**: Resequenced roadmap optimized for dependencies, core-first development, and time-to-useful.
**Note**: Milestone numbers in this document match the restructured roadmap (Phase.Milestone numbering).
**Note**: This is a general guide. Features can be skipped or reordered ad-hoc. The milestone specs and feature definitions are unchanged — only the sequence.

---

## Dependency Map

```
1.1 Virtual Camera
 └→ 1.2 Camera Overlays
 └→ 2.1 Scene Management
     └→ 3.1 Shot Structure
         └→ 3.2 Keyframe Animation
             └→ 4.1 Undo/Redo
             └→ 4.2 Save/Load
             └→ 4.4 Export
             └→ 5.1 Lighting
             └→ 6.1 Characters ←── the complexity cliff
                 └→ 7.1 Facial Expressions
                 └→ 6.2 Camera Follow/Watch
                     └→ 7.2 Snorricam
                 └→ 6.3 Element Linking (benefits from 6.1 but doesn't strictly require it)
             └→ 8.1 Selection Refinements (extends 2.1)
             └→ 9.1 Multi-camera (extends 3.2)
             └→ 8.4 Slow-motion (extends 3.2)
     └→ 4.3 Asset Import
         └→ 5.2 Set Decoration Library
         └→ 5.3 Premade Environments
 └→ 2.2 Viewport Panel System (extends 2.1.5 Director View)
     └→ 8.2 Designer View (panel mode)
         └→ 10.1 Set Builder (uses Designer View)
 └→ 8.3 Script Import (needs 3.1.4 multi-scene + 6.1 characters)
 └→ 11.x AI features (need 3.2 + 6.1)
```

---

## Recommended Sequence

### Phase 1 — The Camera (weeks 1–4)
*You're building a camera tool. The camera comes first. Everything else supports it.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 1 | **1.1 Virtual Camera** | The product IS the camera. Movement, lens, focus, DOF, shake. |
| 2 | **1.2 Camera Overlays** | Aspect ratio defines the frame. Without it, you can't compose. Camera info gives the DP data. |

**Exit criteria:** You can move a physically-accurate camera through 3D space, see the correct aspect ratio, toggle composition guides, and read focal length / height / AOV from the camera info.

---

### Phase 2 — The Scene (weeks 5–8)
*Give the camera something to look at. Click things, move things, see the result through the camera.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 3 | **2.1 Scene Management** | Selection, gizmos, ground plane, duplication, Director View. The interaction model. |
| 4 | **2.2 Viewport Panel System** | Split-view as soon as Camera View + Director View exist. Side-by-side layouts, view selector. |

**Exit criteria:** You can place elements, select them, move/rotate/scale with gizmos, duplicate, and use split-view to see Camera View and Director View simultaneously.

---

### Phase 3 — Time (weeks 9–14)
*Turn a static camera position into a moving shot. Multiple shots, a timeline, keyframes, playback.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 5 | **3.1 Shot Structure** | Shot model, shot track UI, global element timeline. Multi-scene data model (3.1.4) built here but scene tab UI is the last feature in the milestone. Overview (3.1.5) deferred. |
| 6 | **3.2 Keyframe Animation** | Timeline editor, tracks, stopwatch, keyframe interaction, interpolation, playback, path visualization. |

**Exit criteria:** You can create multiple shots, animate the camera and elements with keyframes, scrub the timeline, and play back a sequence. The core previs loop works end-to-end.

**Note on 3.1.4 (multi-scene):** Build the data model (Project → Scene → Shots) from the start so you don't have to refactor later. The scene tab switching UI can be the last thing implemented in this phase.

---

### Phase 4 — Persistence & I/O (weeks 15–20)
*A tool people can't save their work in doesn't get used.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 7 | **4.1 Undo/Redo** | Safety net. Without undo, users can't experiment. Wire the command pattern infrastructure incrementally as features are built, but finalize and test the full stack here. |
| 8 | **4.2 Save/Load** | Project persistence. Creation wizard, file format, auto-save, crash recovery. Scene persistence (4.2.5) included. |
| 9 | **4.3 Asset Import** | Drag-and-drop FBX/OBJ/glTF. Auto-colliders. Asset library panel. Without this, users are stuck with whatever ships built-in. |
| 10 | **4.4 Export** | Image (4.4.1) and video (4.4.2) export are essential. Storyboard export (4.4.3) and NLE export (4.4.4) can follow later. |

**Exit criteria:** The tool is genuinely usable. Save work, undo mistakes, import models, export stills and video. This is the **alpha** — a solo filmmaker could do real previs work with this.

**Implementation note for 4.1:** The command pattern infrastructure should be scaffolded early (Phase 1–3). Each new feature adds its Undo/Redo commands as it's built. Phase 4 is where you finalize, stress-test, and handle the hard edge cases (cross-shot undo, animate mode compound steps, gesture coalescing).

---

### Phase 5 — Scene Dressing (weeks 21–26)
*Solve the empty canvas problem. Give users environments and props so they can start blocking immediately.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 11 | **5.1 Lighting** | Simplest production feature. Validates property animation pipeline. Makes the view visually useful. A lit room is already useful for DP previs even without characters. |
| 12 | **5.2 Set Decoration Library** | Built-in props (5.2.1) ship with the app — tables, chairs, walls, vehicles. The browsing UI is simple. Marketplace integration (5.2.2) and user asset management (5.2.3) can follow later. |
| 13 | **5.3 Premade Environments** | 12-15 ready-to-use sets. Click "Office" and start blocking. Code is trivial (load a saved scene). Content creation runs in parallel from Phase 4 onward. |

**Exit criteria:** Users open the app, pick a premade environment (or browse built-in props), the scene has lighting, and it looks like a real location. No more blank canvas.

**Content pipeline note:** The actual 3D assets and environment models are art work that can be produced in parallel with Phase 4 code. The code for browsing and placing them is simple — don't let content creation block code progress.

---

### Phase 6 — Characters (weeks 27–38)
*The complexity cliff. This is where Fram3d becomes a real previsualization tool — and where the hardest engineering work lives.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 14 | **6.1 Characters** | Mannequin placement, body customization, pose library, joint posing, character animation, walk cycles. This is the longest single milestone — plan accordingly. Custom character import (6.1.5) can be deferred to end of phase. |
| 15 | **6.2 Camera Follow & Watch** | Immediately useful after characters. "Follow the detective down the hallway" is a bread-and-butter previs shot. Every director will want this. |
| 16 | **6.3 Element Linking & Grouping** | Prop holding — character carries a briefcase, picks up a weapon. Link tool, anchor points. Core blocking capability. |

**Exit criteria:** You can drop characters into a premade environment, pose them, animate their movement, have the camera follow them, and link props to their hands. This is the feature set that differentiates Fram3d from every 2D tool.

**Why 6.3 before 7.1:** Prop holding (element linking) is fundamental blocking — a character needs to carry things, sit in chairs, interact with the environment. Facial expressions are emotional nuance. Blocking comes before performance.

---

### Phase 7 — Character Extensions (weeks 39–44)
*Enhance characters with expressions and specialized camera rigs.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 17 | **7.1 Facial Expressions** | Blend shapes, intensity slider, eye direction, expression animation. Adds emotional performance to blocking. |
| 18 | **7.2 Snorricam** | Body-mounted camera. Niche but distinctive. Requires character animation (6.1) and camera follow patterns (6.2). |

**Exit criteria:** Characters can emote and have eye direction. Snorricam shots are possible.

---

### Phase 8 — Polish & Views (weeks 45–52)
*Quality-of-life improvements and new ways to see the scene. The tool works — now make it work better.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 19 | **8.1 Selection Refinements** | Multi-select, grid snapping, custom interpolation curves. Scenes are getting complex — multi-select is needed. Bezier curves make animations professional. |
| 20 | **8.2 Designer View** | Top-down orthographic view. Directors have drawn overhead blocking diagrams for a century. Now it's a view. |
| 21 | **8.3 Script Import** | Final Draft / Fountain parsing. Auto-create scenes, character placeholders, dialogue library. Depends on characters (6.1) and multi-scene (3.1.4). |
| 22 | **8.4 Slow-motion** | Per-shot speed factor. Nice-to-have, not core. Simple implementation (playback-time transform). |
| 23 | **3.1.5 Timeline Overview** | Overview navigation. Useful once projects have many shots, but not essential. |

**Exit criteria:** The tool is polished enough for production use. Multi-select, precision placement, professional animation curves, overhead view, script-to-previs workflow, slow-motion.

---

### Phase 9 — Multi-camera (capstone)
*Get single-camera workflow polished before introducing multi-cam complexity.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 24 | **9.1 Multi-camera** | Up to 4 cameras per shot, active angle editing. Builds on mature keyframe and shot track foundations. The professional previs capstone. |

---

### Phase 10 — Set Building & Advanced Export

| # | Milestone | Rationale |
|---|-----------|-----------|
| 25 | **10.1 Set Builder** | Room construction, wall drawing. Uses Designer View. |
| 26 | **5.2.2 Marketplace Integration** | Sketchfab, Asset Store, TurboSquid, Mixamo browsing (if not done in Phase 5). |
| 27 | **4.4.3 Storyboard Export** | PDF/image grid (if not done in Phase 4). |
| 28 | **4.4.4 NLE Export** | EDL, camera metadata (if not done in Phase 4). |
| 29 | **5.2.3 User Asset Management** | Tag, favorite, organize assets for reuse (if not done in Phase 5). |

---

### Phase 11 — AI Features
*The tool must be great without AI. AI is upside, not the product.*

| # | Milestone | Rationale |
|---|-----------|-----------|
| 30 | **11.1 NL Shot Description** | Shot-from-text, cinematic vocabulary. |
| 31 | **11.2 Automatic Blocking** | Scene-from-text, blocking refinement. |
| 32 | **11.3 Camera Suggestions** | Coverage suggestions, shot list generation. |

---

### Phase 12 — Stretch Goals

| # | Milestone | Rationale |
|---|-----------|-----------|
| 33 | **12.1 AI Prop Generation** | Text-to-prop. |
| 34 | **12.2 Costume Generation** | AI clothing on mannequins. |
| 35 | **12.3 LiDAR Scanning** | Scan import + companion iOS app. |
| 36 | **12.4 Style Grading** | Maybe. |
| 37 | **12.5 AI Video Generation** | Maybe. |

---

## Key Changes from Current Roadmap Order

| Change | Current Position | New Position | Why |
|--------|-----------------|--------------|-----|
| **5.2 Set Decoration** | After 6.3 (Phase ~8) | Phase 5 (after lighting) | Users need props to build scenes. An empty grid with no built-in assets is hostile. Ship basic props early. |
| **5.3 Premade Environments** | Phase 12 | Phase 5 (after lighting) | Solves the blank canvas / new user onboarding problem. Code is trivial; content creation runs in parallel. |
| **6.3 Element Linking** | After 7.2 Snorricam | Phase 6 (right after characters) | Prop holding is core blocking. A character needs to carry a briefcase before they need facial expressions. |
| **7.1 Facial Expressions** | Right after 6.1 | Phase 7 (after camera follow + linking) | Enhancement, not core. Blocking precedes performance. |
| **2.2 Viewport Panels** | Phase 8 (original plan) | Phase 2 (moved up) | Split-view is immediately useful once Camera View + Director View exist. Don't defer what's already natural. |
| **8.4 Slow-motion** | Phase 8 | Phase 8 | Nice-to-have. Not blocking any workflow. |
| **8.3 Script Import** | Phase 8 | Phase 8 | Needs characters (6.1) and multi-scene (3.1.4) to be useful. Once those exist, this is a high-value workflow accelerator. |
| **3.1.5 Overview** | Phase 3 (3.1) | Phase 8 | Useful once projects are complex, not needed early. |
| **8.1 Selection Refinements** | After 5.2 | Phase 8 (before multi-camera) | Multi-select becomes important as scenes grow, but not needed during early development. Comes before multi-camera so the tool is more capable when multi-cam complexity arrives. |

## Unchanged (already correct)

- 1.1 → 1.2 → 2.1 → 3.1 → 3.2 sequence (Phase 1–3)
- 4.1 → 4.2 → 4.3 → 4.4 sequence (Phase 4)
- 5.1 Lighting before 6.1 Characters (lighting validates property animation, is simpler)
- 6.2 Camera Follow right after 6.1 Characters
- 7.2 Snorricam after 6.2 Camera Follow
- 9.1 Multi-camera as capstone
- 11.x AI features near the end
- 12.1–12.5 stretch goals at the end
