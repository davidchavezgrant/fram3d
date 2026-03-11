# Fram3d Product Roadmap

**Date**: 2026-03-10
**Status**: Complete
**Last Updated**: 2026-03-10

---

Fram3d is a 3D previsualization tool for filmmakers. Cinematic language over 3D complexity — focal length, dolly, crane — not Maya jargon.

---

- # 1. Core previsualization tool (Project)
	*Build the end-to-end previs loop: move a camera, frame a shot, animate it, play it back. One scene, one camera, no persistence.*

	- ### 1.1. Virtual camera (Milestone)
		*Physically-based camera rig with movement, lens, focus, and shake — controlled entirely by mouse + modifier keys.*

		- ##### 1.1.1. Camera movement (Feature)
			*Pan, tilt, dolly, truck, crane, roll, orbit, dolly zoom, reset. All speeds configurable. Dolly zoom lockable to a specific object.*

		- ##### 1.1.2. Lens system (Feature)
			*Focal length 14–400mm, physically accurate FOV from real sensor dimensions, presets, smooth transitions.*

		- ##### 1.1.3. Camera body and lens presets (Feature)
			*Camera bodies: ARRI Alexa Mini LF, ARRI Alexa 35, RED V-Raptor, Canon C300 Mark III, Canon C70, Sony FX6, Sony FX3, Generic 35mm, Super 35mm, 16mm, Super 16mm, 8mm. Lens presets: Zeiss Master Prime, Cooke S4/i, Leica Summilux-C, Sigma Cine FF, Generic Prime. Presets distinguish prime vs. zoom lenses.*

		- ##### 1.1.4. Focus (Feature)
			*Animated focus-on-object — smooth transition, calculates optimal distance from bounds and FOV, frames with breathing room. DOF focus distance automatically follows focus target.*

		- ##### 1.1.5. Depth of field preview (Feature)
			*Visualize shallow/deep DOF based on focal length, aperture, and focus distance. Aperture stops match real cinema lenses (f/1.4, f/2, f/2.8, etc.). Cinematic bokeh preview — not photorealistic, just enough to see what's sharp vs. soft.*

		- ##### 1.1.6. Camera shake (Feature)
			*Procedural handheld effect. Perlin noise, configurable amplitude/frequency, X/Y rotation only. Per-shot enable/disable. Cosmetic only — baked to keyframes only during export.*

		- ##### 1.1.7. Camera info HUD (Feature)
			*Overlay showing focal length, height, angle of view, aspect ratio, body, and lens preset. Toggleable via shortcut. Counts as an overlay for export purposes.*

	- ### 1.2. Camera overlays (Milestone)
		*Composition aids that sit on top of the 3D viewport — aspect ratio masks, frame guides, and subtitle text.*

		- ##### 1.2.1. Aspect ratio masks (Feature)
			*Letterbox/pillarbox bars. 8 ratios: Full Screen, 16:9, 16:10, 1.85:1, 2.35:1, 2.39:1, 2:1, 4:3. Default 16:9. A cycles forward, Shift+A cycles backward.*

		- ##### 1.2.2. Frame guides (Feature)
			*Rule of thirds, center cross (fixed pixel size), safe zones (title 90%, action 93% — configurable with defaults). All start hidden, toggled independently. Global visibility state, not per-shot.*

		- ##### 1.2.3. Subtitle overlay (Feature)
			*Text layer overlaid on the frame in a fixed subtitle position. Timeable within a shot (adjustable start/end time). Configurable color, size, and font per layer. No animations, no transitions.*

	- ### 1.3. Scene management (Milestone)
		*Click objects, move them around, see visual feedback. Single-selection with translate/rotate/scale gizmos. Objects exist in world space across all shots.*

		- ##### 1.3.1. Scene elements (Feature)
			*Any object with a collider is interactive. Hover highlighting, selection highlighting, click-to-select, click-empty-to-deselect. Compound objects are single selectable units.*

		- ##### 1.3.2. Transform gizmos (Feature)
			*Translate (arrows), rotate (rings), scale (uniform proportional only — single handle). Axis-colored (RGB = XYZ), always-on-top, constant screen size.*

		- ##### 1.3.3. Ground plane (Feature)
			*Infinite ground plane with visible grid for spatial reference. Always visible. Gives context for camera height and blocking distances.*

		- ##### 1.3.4. Object duplication (Feature)
			*Ctrl+D to duplicate selected object. Copy inherits position with reasonable offset. Duplicated names appended with incrementing number (e.g., Chair_1, Chair_2). No duplication during gizmo drag.*

		- ##### 1.3.5. Director view (Feature)
			*Global utility camera decoupled from the shot timeline. Move freely to position objects and cameras without creating keyframes. Separate from the virtual camera rig — does not affect shot animation.*

	- ### 1.4. Shot sequencer (Milestone)
		*Horizontal strip of shot thumbnails. Add, delete, reorder, select shots. Each shot is an independent camera animation over shared world-space objects.*

		- ##### 1.4.1. Shot model (Feature)
			*Shot = name + duration + camera animation + object animations. Auto-named Shot_01, Shot_02, etc. Objects exist in world space — all objects are present in every shot. Default duration 5 seconds, max 300 seconds.*

		- ##### 1.4.2. Sequencer UI (Feature)
			*Scrollable thumbnail strip with shot name, editable duration, drag-and-drop reordering, add/delete buttons. Aggregate duration display (total running time). Delete requires confirmation with "don't show again" option and menu item to re-enable.*

		- ##### 1.4.3. Object continuity (Feature)
			*New shots inherit current object transforms as initial state. Continuity propagates: changing animation endpoints in Shot N updates starting positions in Shot N+1. Backward navigation restores objects to their initial state for that shot.*

	- ### 1.5. Keyframe animation (Milestone)
		*Per-property keyframe timeline editor for camera and object animation within each shot. After Effects / Premiere-style property tracks with unified auto-keyframing.*

		- ##### 1.5.1. Timeline editor (Feature)
			*Three regions: transport bar, timeline area with tracks and playhead, status bar with keyboard hints. Time ruler rescales dynamically with shot duration; user can also manually zoom the ruler.*

		- ##### 1.5.2. Tracks and keyframes (Feature)
			*Camera track (yellow) always present. Object tracks (green) per animated object. Each track has a collapsible dropdown showing per-property sub-tracks with current values. Camera properties: position (x, y, z), pan, tilt, roll (degrees, can be negative), focal length (only for zoom lenses). Object properties: position (x, y, z), scale (single uniform value), rotation (x, y, z in degrees). Auto-keyframing creates a unified "main" keyframe; expanding the dropdown reveals individual property keyframes.*

		- ##### 1.5.3. Keyframe interaction (Feature)
			*Click to select, drag to reposition (snaps to 0.1s), click timeline to scrub, delete selected. Moving a main keyframe moves all child property keyframes. Moving a child keyframe creates a new main keyframe at the target time containing only that property; the original slot empties. Deleting a main keyframe deletes all children. Deleting all children deletes the main keyframe. Dragging onto an existing keyframe silently merges. Minimum 1 camera keyframe enforced.*

		- ##### 1.5.4. Interpolation and playback (Feature)
			*Smooth interpolation between keyframes. Real-time playback evaluating all tracks each frame. Stops at shot end — playhead stays at shot duration. No keyframe creation allowed during playback.*

		- ##### 1.5.5. Auto-keyframing (Feature)
			*Automatically creates/updates keyframes when the user manually moves the camera or objects (not during playback). Near existing keyframe (0.1s): update. Otherwise: create new. Always-on — no toggle.*

		- ##### 1.5.6. Camera path visualization (Feature)
			*Render the camera's dolly track as a spline in 3D space. Shows keyframe positions as nodes with frustum indicators showing camera look direction. Toggleable.*

	- ### 1.6. Input system (Milestone)
		*Mouse + keyboard control scheme. Modifier keys for camera movements, letter keys for tools and toggles.*

		- ##### 1.6.1. Mouse controls (Feature)
			*Scroll Y for focal length. Scroll Y + Alt for dolly. Scroll Y + Shift for crane. Scroll Y + Ctrl for roll. Scroll Y + Cmd+Alt for dolly zoom. Scroll X + Cmd for truck. Ctrl-drag for pan/tilt. Alt-drag for orbit (around selected object or world origin when nothing selected).*

		- ##### 1.6.2. Keyboard shortcuts (Feature)
			*Space: play/pause. QWER: tool modes. F: focus on selected. 1–9: focal length presets directly. A: cycle aspect ratio forward. Shift+A: cycle backward. C: add camera keyframe (not during playback). V: add object keyframe (not during playback). Arrows: scrub by 1 frame. Delete/Backspace: delete selected keyframe or selected object (context-sensitive). Ctrl+D: duplicate. Ctrl+R: reset camera (undoable). Cmd+Z / Cmd+Shift+Z: reserved for undo/redo.*

- # 2. Workflow essentials (Project)
	*Make it usable for real work: undo mistakes, save projects, bring in your own assets, get frames out.*

	> **Design note — build early infrastructure:**
	> - **Settings / Preferences panel:** A centralized settings panel should be built early (Milestone 2.1 or 2.2 timeframe) so that new settings can be added incrementally as features ship. This avoids scattering configuration UIs across the application.
	> - **Panel / docking system:** A general panel system with docking support should be built early, since downstream milestones (hierarchy panel, pose library, asset library, inspector) all require dockable panels. Building the system once avoids reimplementing panel infrastructure for each feature.

	- ### 2.1. Undo / Redo (Milestone)
		*Command-based undo stack. Every user action (move, keyframe, shot change) is reversible. Selection changes are not undoable.*

		- ##### 2.1.1. Undo stack (Feature)
			*Cmd+Z / Cmd+Shift+Z. Stack-based history of all user actions. Survives save — saving does not clear the stack. Cross-shot undo does not auto-switch the active shot. Auto-keyframe + movement is one compound undo step.*

		- ##### 2.1.2. Command pattern completion (Feature)
			*Wire existing command infrastructure (already scaffolded with Execute/Undo/Redo) into a working undo manager. Scroll gesture coalescing with configurable inactivity timeout.*

	- ### 2.2. Save / Load (Milestone)
		*Project persistence. Save scene state, shots, animations, and settings to disk. Reopen and continue.*

		- ##### 2.2.1. Project file format (Feature)
			*Serialize full project state: scene objects (with embedded asset data), shot sequence, all keyframe data (per-property), camera settings, overlay state, camera shake per-shot state. Human-readable format preferred. Version-tolerant loading — older files load in newer versions with conversion if needed.*

		- ##### 2.2.2. Save / Load UI (Feature)
			*Save, Save As, Open, Recent Projects (with Clear Recent option). Auto-save on user-configurable interval (default 2 minutes). Crash recovery dialog shows timestamp only.*

		- ##### 2.2.3. Dirty state tracking (Feature)
			*Track unsaved changes. Warn before closing with unsaved work. New Project resets overlay/HUD settings to defaults.*

	- ### 2.3. Asset import (Milestone)
		*Bring in 3D models from external tools. Drag-and-drop into the scene.*

		- ##### 2.3.1. Model import (Feature)
			*Drag-and-drop FBX, OBJ, glTF files into the viewport. Auto-generate colliders for selection/gizmos. Multi-mesh models prompt user: treat as single element or unpack into individual elements. Placed at camera look-point. Supports embedded animation data (e.g., spinning fan). Re-importing same file offers to replace existing. Silent auto-scale for extreme sizes (<1cm or >100m).*

		- ##### 2.3.2. Asset library (Feature)
			*Panel showing imported assets for reuse across shots. Thumbnail previews. List-based organization. Warns user about large asset impact on project file size.*

	- ### 2.4. Export (Milestone)
		*Get work out of Fram3d — stills for storyboards, video for sharing with crew.*

		- ##### 2.4.1. Image export (Feature)
			*Export current frame as PNG/JPEG at the active aspect ratio. Optionally include overlays, DOF effect, and shaken camera position. Remembers last-used export directory across all projects.*

		- ##### 2.4.2. Video export (Feature)
			*Proper offline rendering at target resolution and frame rate. Render shot or full sequence. Modal blocking for v1. Output via ffmpeg or MCP integration with Adobe Media Encoder. Bakes camera shake into keyframes during render. Hard cuts only between shots.*

		- ##### 2.4.3. Storyboard export (Feature)
			*Export all shots as a grid/contact sheet with shot names and durations. PDF or image. User-configurable rows per page. Supports two-column layout.*

		- ##### 2.4.4. NLE export (Feature)
			*Export to Adobe Media Encoder or Premiere Pro via MCP integration. Embed full camera metadata (per-frame keyframe data for VFX use). EDL timeline format.*

- # 3. Production features (Project)
	*Graduate from a camera tool to a full previs suite. Lighting, characters, scene management, and multi-camera coverage.*

	- ### 3.1. Lighting (Milestone)
		*Place, aim, and adjust lights to previsualize mood and contrast. Not a rendering tool — a blocking tool for DPs.*

		- ##### 3.1.1. Light types (Feature)
			*Directional (sun), point, spot. Position and aim with the same gizmo system as objects. Light type changeable after creation. Lights carry across shots via continuity (like objects). Adding the first light instantly dims default ambient.*

		- ##### 3.1.2. Light properties (Feature)
			*Intensity, color (RGB picker + Kelvin temperature), range, cone angle (spot). Cone wireframe visible when selected only. Adjustable via inspector or direct manipulation. Lights appear in a separate "Lights" section of the hierarchy panel, not mixed with objects.*

		- ##### 3.1.3. Light animation (Feature)
			*Keyframe light properties on the timeline. Animate intensity, color, position for lighting cues.*

	- ### 3.2. Characters / Actors (Milestone)
		*Poseable, animatable humanoid figures for blocking scenes. At least FrameForge-level manipulation with a better UI. The core differentiator from 2D tools and the biggest complexity leap.*

		- ##### 3.2.1. Mannequin placement (Feature)
			*Drop humanoid mannequins into the scene. Male/female body type selection with continuous height and build sliders. Color-tintable to distinguish characters visually. Drag to position like any other object.*

		- ##### 3.2.2. Pose library (Feature)
			*Preset poses: standing, sitting, walking, running, pointing, conversation, lying down. Apply with one click, use as starting points. User-extensible — save custom poses for reuse.*

		- ##### 3.2.3. Custom posing (Feature)
			*Manipulate individual joints — head, arms, torso, legs. IK for natural limb movement. Direct joint selection and rotation. Prop holding: character holds object without precise finger alignment (FrameForge-style). Self-collision skipped for v1.*

		- ##### 3.2.4. Character animation (Feature)
			*Keyframe character poses on the timeline. Interpolate between poses for blocking animation (minor self-clipping acceptable). Walk cycles with custom gaits (limping, sneaking). Minimal visible foot sliding acceptable.*

		- ##### 3.2.5. Custom character import (Feature)
			*Import rigged humanoid models (FBX). Map to Fram3d's pose/animation system. Lets users bring their own characters.*

	- ### 3.3. Scene hierarchy (Milestone)
		*Parent/child relationships between objects. Group objects, attach props to characters, build compound setups.*

		- ##### 3.3.1. Parenting (Feature)
			*Parent via viewport drag (drag onto object in 3D) or hierarchy panel. Child inherits parent transforms. Unparent to detach. Deleting a parent deletes the entire subtree. Max nesting depth: 4 levels. Duplicating a parent duplicates the full subtree without confirmation. Relationships are global but dynamic between shots — a prop can be parented in one shot and independent in the next.*

		- ##### 3.3.2. Hierarchy panel (Feature)
			*Tree view of all scene objects showing parent/child relationships. Click to select, drag to reparent. Separate "Lights" section. Dockable to left or right side (configurable).*

	- ### 3.4. Set decoration library (Milestone)
		*Built-in and marketplace-connected asset library for dressing scenes — furniture, vehicles, props, environments. Zero friction between "I need a table" and having one in the scene.*

		- ##### 3.4.1. Built-in asset library (Feature)
			*Ship with a core set of common props: tables, chairs, doors, windows, vehicles, trees, walls, floors. Categorized and searchable. Visual style: clearly "previs" (stylized/abstract, not realistic).*

		- ##### 3.4.2. Marketplace integration (Feature)
			*Browse and import free assets from existing 3D marketplaces (Unity Asset Store, Sketchfab, TurboSquid) directly in-app. Handle format conversion and collider generation. No in-app purchases — redirect to marketplace for paid content.*

		- ##### 3.4.3. User asset management (Feature)
			*Organize imported and marketplace assets into user collections. Tag, favorite, and quick-access for reuse across projects. No disk space cap. Simple cache management UI. Local storage only.*

	- ### 3.5. Selection and manipulation refinements (Milestone)
		*Quality-of-life improvements to object interaction as scenes get more complex.*

		- ##### 3.5.1. Multi-select (Feature)
			*Shift-click to add to selection. Drag-select (marquee) with partial intersection (crossing selection). Move/rotate/scale multiple objects together. Gizmo pivot at center of selection bounding box.*

		- ##### 3.5.2. Grid snapping (Feature)
			*Snap objects to grid from fixed presets (0.1, 0.25, 0.5, 1.0, 2.0m). Hold modifier to override. Rotation snap at configurable increments (default 15°). Visual grid overlay.*

		- ##### 3.5.3. Custom interpolation curves (Feature)
			*Per-property easing: linear, ease-in, ease-out, ease-in-out, bezier. Default: linear with option to switch. Bezier Y values constrained to 0–1 with checkbox to allow overshoot. Curve editor UI.*

	- ### 3.6. Multi-camera (Milestone)
		*Up to four cameras per shot with independent keyframe timelines, shared object animation, and a coverage splitting track for editing camera cuts within a setup.*

		- ##### 3.6.1. Per-shot camera addition (Feature)
			*Add up to 4 cameras (A, B, C, D) per shot via keybind or UI element near the frame preview. Each camera labeled and color-coded (user-configurable colors). Camera preview elements appear above the current shot. Left-click a camera preview to view its keyframe timeline.*

		- ##### 3.6.2. Multi-camera timelines (Feature)
			*Each camera has its own camera keyframe timeline within the shot. Object keyframe timelines are shared across all cameras in the shot — simulates multiple cameras covering the same action simultaneously.*

		- ##### 3.6.3. Active camera and switching (Feature)
			*Right-click camera preview → "Set to active" designates the playback camera. Shift+1/2/3/4 to switch active camera via keybind. Switching cameras during playback auto-splits coverage at the switch point.*

		- ##### 3.6.4. Coverage splitting (Feature)
			*Toggle a "coverage" track row in the timeline. Default: solid bar matching active camera color and label. Right-click → "Split coverage" to divide at a point, selecting which cameras to assign. Drag division points to adjust cut timing. Minimum segment duration: 1 frame. Deleting a segment: left neighbor absorbs space (right neighbor if first segment). Deleting all segments reverts to single-camera active mode.*

		- ##### 3.6.5. Multi-split (Feature)
			*Right-click → "Multi-split": prompted with "split every {n} frames" — automatically creates evenly-spaced divisions. Default: same camera throughout (no change until cameras are reassigned to segments).*

- # 4. AI-assisted previsualization (Project)
	*Use language models to accelerate previs work — describe what you want, get a starting point to refine.*

	- ### 4.1. Natural language shot description (Milestone)
		*Describe a shot in words, get camera position and framing as a starting point.*

		- ##### 4.1.1. Shot-from-text (Feature)
			*"Medium close-up of the actor at the table, slightly low angle" → camera positioned accordingly. Compound descriptions ("OTS then reverse") create multiple shots. Input via command palette. Interacts with auto-keyframing — AI repositioning creates keyframes. Stores movement metadata ("slow dolly in") for later use.*

		- ##### 4.1.2. Shot vocabulary (Feature)
			*Understands cinematic language: shot sizes (ECU, CU, MCU, MS, MLS, LS, ELS), angles (low, high, dutch with variable roll amounts, eye-level, bird's eye, worm's eye), movement types. Directional references relative to camera ("actor on the left" = camera-left). Ambiguous multi-character descriptions frame all as group.*

	- ### 4.2. Automatic blocking (Milestone)
		*Position actors and objects from a scene description.*

		- ##### 4.2.1. Scene-from-text (Feature)
			*"Two people sitting across a table, one standing by the window" → mannequins placed and posed. Handles environment generation using best-effort from available assets. Supports motion descriptions implying keyframed animation. Text input for v1.*

		- ##### 4.2.2. Blocking refinement (Feature)
			*"Move the standing actor closer to the table" → incremental adjustments via text. Conversational context persists across save/load. Can add new objects ("add a chair next to her"). Ambiguous references (e.g., two tables) prompt user for clarification rather than guessing.*

	- ### 4.3. Camera suggestions (Milestone)
		*Given a blocked scene, suggest camera setups for coverage.*

		- ##### 4.3.1. Coverage suggestions (Feature)
			*Suggest master shot, over-the-shoulder, close-ups for a dialogue scene. Generate as separate shots in the sequencer. Three-way style toggle: conservative / minimal / comprehensive. Include camera movement (dolly, push-in). User-configurable max shots per generation (default 3). Can request coverage for a specific character pair only.*

		- ##### 4.3.2. Shot list generation (Feature)
			*Generate a full shot list from a scene description or script excerpt. Create shots in sequence. OTS pairing determined by characters facing each other within threshold distance.*

- # Sequencing principles

	1. **Ship the camera first.** The virtual camera is the product. Everything else supports it. Get camera + overlays + basic scene working before building timeline features.
	2. **Workflow before features.** Undo, save, and export come before lighting, characters, and multi-camera. A tool people can't save their work in doesn't get used.
	3. **Characters are the complexity cliff.** Poseable humanoids with IK, pose libraries, and animation are an order of magnitude harder than everything in Project 1. Don't underestimate this.
	4. **AI features are speculative.** Project 4 depends on LLM capabilities that may or may not work well enough. Build the tool so it's great without AI — AI is upside, not the product.
	5. **Don't build a 3D editor.** Every feature should pass the test: "does a director need this to plan a shot?" If the answer is "only a 3D artist would use this," cut it.
	6. **Multi-camera is a capstone.** Get single-camera workflow polished before introducing multi-cam complexity. Coverage splitting builds on mature keyframe and sequencer foundations.
