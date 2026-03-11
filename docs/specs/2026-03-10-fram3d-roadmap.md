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
			*Shot = name + duration + camera animation + time range on global object timeline. Auto-named Shot_01, Shot_02, etc. Objects animate on a global timeline — shots are windows into it. Camera keyframes are per-shot. Default duration 5 seconds, max 300 seconds.*

		- ##### 1.4.2. Sequencer UI (Feature)
			*Scrollable thumbnail strip with shot name, editable duration, drag-and-drop reordering, add/delete buttons. Aggregate duration display (total running time). Delete requires confirmation with "don't show again" option and menu item to re-enable.*

		- ##### 1.4.3. Object continuity — global object timeline (Feature)
			*Objects animate on a single global timeline spanning all shots. Shots are windows into this timeline for camera purposes. No per-shot initial state, no continuity propagation — the global timeline IS the continuity. Object keyframes are global; camera keyframes are per-shot.*

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
	> - **Project creation prompt:** Creating a new project should prompt for camera body + lens selection and project resolution. This establishes the camera and output resolution from the start, matching real production workflows where these are decided before shooting begins.
	> - **File format:** The project file format decision (JSON vs YAML vs custom binary) is deferred to implementation. Human-readable is preferred for git diffing and debugging.

	- ### 2.1. Undo / Redo (Milestone)
		*Command-based undo stack. Every user action (move, keyframe, shot change) is reversible. Selection changes are not undoable.*

		- ##### 2.1.1. Undo stack (Feature)
			*Cmd+Z / Cmd+Shift+Z. Stack-based history of all user actions. Survives save — saving does not clear the stack. Cross-shot undo does not auto-switch the active shot. Auto-keyframe + movement is one compound undo step.*

		- ##### 2.1.2. Command pattern completion (Feature)
			*Wire existing command infrastructure (already scaffolded with Execute/Undo/Redo) into a working undo manager. Scroll gesture coalescing with 1000ms inactivity timeout (undo from where the user "settles").*

	- ### 2.2. Save / Load (Milestone)
		*Project persistence. Save scene state, shots, animations, and settings to disk. Reopen and continue.*

		- ##### 2.2.1. Project file format (Feature)
			*Serialize full project state: scene objects, shot sequence, all keyframe data (per-property), camera settings, overlay state, camera shake per-shot state. Asset bundling configurable: prompt on first import with "No" (by reference), "Yes (everything)", or "Yes (up to X MB)". Configurable in Settings/Preferences, auto-migrates on mode change. Human-readable format preferred (exact format deferred to implementation). Version-tolerant loading — older files load in newer versions with conversion if needed.*

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

	- ### 3.3. Object linking & grouping (Milestone)
		*Temporal linking (object follows character or object) and persistent grouping (simultaneous transforms). Flat scene graph with positional attachment — no Unity hierarchy.*

		- ##### 3.3.1. Object linking (Feature)
			*Temporal object-to-character and object-to-object linking on the global timeline. Viewport: click-drag to link, right-click to unlink. Panel: pickwhip tool. Anchor point (panel XYZ value) defines attachment offset. Linked period greyed out on timeline. Max chain depth: 4. Positional attachment — flat scene graph, transform override during linked period.*

		- ##### 3.3.2. Object grouping (Feature)
			*Persistent multiselect. Select any member → all selected. Transform any member → all transform. No timeline behavior. Right-click to group/ungroup.*

		- ##### 3.3.3. Object list panel (Feature)
			*Flat list of all scene objects with link indicators, group markers, and pickwhip handles. Click to select, pickwhip to link. Separate "Lights" section. Dockable left or right.*

	- ### 3.4. Set decoration library (Milestone)
		*Built-in and marketplace-connected asset library for dressing scenes — furniture, vehicles, props, environments. Zero friction between "I need a table" and having one in the scene.*

		- ##### 3.4.1. Built-in asset library (Feature)
			*Ship with a core set of common props: tables, chairs, doors, windows, vehicles, trees, walls, floors. Categorized and searchable. Visual style: clearly "previs" (stylized/abstract, not realistic).*

		- ##### 3.4.2. Marketplace integration (Feature)
			*Browse and import free assets from ALL three major 3D marketplaces (Sketchfab, Unity Asset Store, TurboSquid) plus Mixamo directly in-app. In-app browser shows FREE assets only. Handle format conversion and collider generation. No in-app purchases — redirect to marketplace for paid content.*

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

- # 5. Feature parity and extensions (Project)
	*Competitive features identified from market research (Previs Pro, RADiCAL Canvas). Prioritized by impact on user workflow. See companion doc: "Fram3d Feature Parity Specs" in Obsidian vault for detailed specs and open questions.*

	- ### 5.1. Premade environments (Milestone) — HIGHER PRIORITY
		*Ship with a library of ready-to-use environments so users don't start from a blank canvas. Interior and exterior sets that establish context in one click.*

		- ##### 5.1.1. Environment library (Feature)
			*Premade sets: living room, office, restaurant, bar, street, alley, park, parking lot, warehouse, courtroom, hospital room, classroom. Each includes walls/floors/props/lighting. One-click placement into scene. Objects are independent after placement (not a locked prefab) — user can modify, delete, or rearrange anything.*

	- ### 5.2. Script import (Milestone) — HIGHER PRIORITY
		*Import screenplays to auto-populate scenes, characters, and shot structure. Bridges the gap between script and previs.*

		- ##### 5.2.1. Script parsing (Feature)
			*Import Final Draft (.fdx) and Fountain (.fountain) screenplay files. Parse scene headings, character names, and action lines. Auto-create shots from scene headings. Auto-create named character placeholders from dialogue attributions.*

	- ### 5.3. 2D overhead view (Milestone) — HIGHER PRIORITY
		*Bird's-eye 2D view of the scene showing object positions, camera frustums, and light positions. Essential for blocking and planning — the same tool a director uses when drawing diagrams on paper.*

		- ##### 5.3.1. 2D scene view (Feature)
			*Top-down orthographic view of the scene. Objects shown as labeled icons/silhouettes. Camera shown as frustum with field of view cone. Lights shown as standard lighting diagram symbols. Drag objects to reposition. Click camera to see its view in a side panel. Switchable between 3D viewport and 2D view (or split view if implemented).*

	- ### 5.4. AI prop generation (Milestone) — MEDIUM PRIORITY
		*Generate 3D props from text descriptions. "Red leather armchair" → usable 3D model in the scene. Fills gaps in the asset library without requiring manual sourcing.*

		- ##### 5.4.1. Text-to-prop (Feature)
			*Text prompt generates a 3D model suitable for previs. Low-poly, consistent visual style with bundled assets. Auto-generates collider. Saved to asset library for reuse. Requires internet connection and API key.*

	- ### 5.5. AI character generation (Milestone) — LOWER PRIORITY
		*Generate character models from text descriptions. "Tall woman in a business suit" → character model with standard rig. Supplements mannequin characters when visual distinction matters.*

		- ##### 5.5.1. Text-to-character (Feature)
			*Text prompt generates a humanoid character model compatible with Fram3d's pose/animation system. Standard rig mapping. Consistent enough for previs — not photorealistic. Saved for reuse across scenes.*

	- ### 5.6. LiDAR scanning (Milestone) — LOWER PRIORITY
		*Scan real locations with iPhone/iPad LiDAR and import as scene backgrounds. Requires companion iOS app.*

		- ##### 5.6.1. LiDAR import (Feature)
			*Import point cloud or mesh data from LiDAR scans (iPhone Pro, iPad Pro). Use as scene backdrop for accurate spatial reference during previs. Not editable — display only.*

		- ##### 5.6.2. Companion iOS app (Feature)
			*Minimal iOS app for capturing LiDAR scans and exporting in a format Fram3d can import. Not a full previs tool — scan capture only.*

	- ### 5.7. Style grading (Milestone) — MAYBE
		*Apply visual styles to rendered frames and sequences. "Film noir," "golden hour," "gritty documentary" — communicate the intended look to the team without detailed lighting work.*

		- ##### 5.7.1. Frame style grading (Feature)
			*Apply visual style presets or AI-generated styles to exported frames and video. Preserves camera framing, blocking, and timing. Output is a stylized version of the previs, not a replacement for it.*

	- ### 5.8. AI video generation (Milestone) — MAYBE
		*Generate stylized video from previs sequences using AI. Turns grey-box previs into something closer to the intended visual style. Speculative — depends on future AI capabilities.*

	> **Explicitly NOT building:**
	> - Color grading tools (not previs — that's post-production)
	> - AR overlay (placing 3D assets on real camera feed — requires mobile-first architecture)
	> - Facial scanning / face capture (out of scope for previs)

- # Sequencing principles

	1. **Ship the camera first.** The virtual camera is the product. Everything else supports it. Get camera + overlays + basic scene working before building timeline features.
	2. **Workflow before features.** Undo, save, and export come before lighting, characters, and multi-camera. A tool people can't save their work in doesn't get used.
	3. **Characters are the complexity cliff.** Poseable humanoids with IK, pose libraries, and animation are an order of magnitude harder than everything in Project 1. Don't underestimate this.
	4. **AI features are speculative.** Project 4 depends on LLM capabilities that may or may not work well enough. Build the tool so it's great without AI — AI is upside, not the product.
	5. **Don't build a 3D editor.** Every feature should pass the test: "does a director need this to plan a shot?" If the answer is "only a 3D artist would use this," cut it.
	6. **Multi-camera is a capstone.** Get single-camera workflow polished before introducing multi-cam complexity. Coverage splitting builds on mature keyframe and sequencer foundations.
