# Fram3d Product Roadmap

**Date**: 2026-03-10
**Status**: Active
**Last Updated**: 2026-03-11

---

Fram3d is a 3D previsualization tool for filmmakers. Cinematic language over 3D complexity — focal length, dolly, crane — not Maya jargon.

---

- # 1. Core previsualization tool (Project)
	*Build the end-to-end previs loop: move a camera, frame a shot, animate it, play it back. One scene, one camera, no persistence.*

	- ### 1.1. Virtual camera (Milestone)
		*Physically-based camera rig with movement, lens, focus, and shake — controlled entirely by mouse + modifier keys.*

		- ##### 1.1.1. Camera movement (Feature)
			*Pan, tilt, dolly, truck, crane, roll, orbit, dolly zoom, reset. All speeds configurable. Dolly zoom lockable to a specific object. Orbit pivot: persistent last-focus model (Maya/Unity).*

		- ##### 1.1.2. Lens system (Feature)
			*Focal length 14–400mm, physically accurate FOV from real sensor dimensions, presets, smooth transitions. Anamorphic lenses supported: squeeze factors (1.33x, 1.5x, 1.8x, 2x), FOV computed from squeeze factor, aspect ratio auto-locked when anamorphic lens is active.*

		- ##### 1.1.3. Camera body and lens database (Feature)
			*Full database of real-world camera bodies and lens sets. 200+ cameras across 17 manufacturers (Canon, Sony, ARRI, RED, Panasonic, Blackmagic, Fujifilm, Panavision, Vision Research Phantom, IMAX, Aaton, Bolex, Mitchell, Eclair, and others). 100+ lens sets including spherical primes, spherical zooms, anamorphic primes/zooms, and stills lenses adapted for cinema. Generic bodies available for quick start. Database includes sensor dimensions, native resolution, supported frame rates, and mount compatibility. See `reference data/camera-lens-database.json`.*

		- ##### 1.1.4. Focus (Feature)
			*Animated focus-on-object — smooth transition, calculates optimal distance from bounds and FOV, frames with breathing room. DOF focus distance automatically follows focus target.*

		- ##### 1.1.5. Depth of field preview (Feature)
			*Visualize shallow/deep DOF based on focal length, aperture, and focus distance. Aperture stops match real cinema lenses (f/1.4, f/2, f/2.8, etc.). Cinematic bokeh preview — not photorealistic, just enough to see what's sharp vs. soft. Oval bokeh when anamorphic lens is active (future polish).*

		- ##### 1.1.6. Camera shake (Feature)
			*Procedural handheld effect. Configurable amplitude/frequency, X/Y rotation only. Per-shot enable/disable. Cosmetic only — baked to keyframes only during export.*

	- ### 1.2. Camera overlays (Milestone)
		*Composition aids that sit on top of the 3D viewport — aspect ratio masks, frame guides, info HUD, and subtitle text.*

		- ##### 1.2.1. Aspect ratio masks (Feature)
			*Letterbox/pillarbox bars. 8 ratios: Full Screen, 16:9, 16:10, 1.85:1, 2.35:1, 2.39:1, 2:1, 4:3. Default 16:9. A cycles forward, Shift+A cycles backward. Auto-locked to computed delivery format when anamorphic lens is active.*

		- ##### 1.2.2. Frame guides (Feature)
			*Rule of thirds, center cross (fixed pixel size), safe zones (title 90%, action 93% — configurable with defaults). All start hidden, toggled independently. Global visibility state, not per-shot.*

		- ##### 1.2.3. Camera info HUD (Feature)
			*Overlay showing focal length, height, angle of view, aspect ratio, body, lens preset, and squeeze factor (if anamorphic). Toggleable via shortcut. Counts as an overlay for export purposes.*

		- ##### 1.2.4. Subtitle overlay (Feature)
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
			*Ctrl+D to duplicate selected object. Copy inherits position with reasonable offset. Duplicated names appended with incrementing number (e.g., Chair_1, Chair_2).*

		- ##### 1.3.5. Director view (Feature)
			*Global utility camera decoupled from the shot timeline. Move freely to position objects and cameras without creating keyframes. Separate from the virtual camera rig — does not affect shot animation.*

	- ### 1.4. Shot sequencer (Milestone)
		*Horizontal strip of shot thumbnails. Add, delete, reorder, select shots. Each shot is an independent camera animation over shared world-space objects.*

		- ##### 1.4.1. Shot model (Feature)
			*Shot = name + duration + camera animation + time range on global object timeline. Auto-named Shot_01, Shot_02, etc. Objects animate on a global timeline — shots are windows into it. Camera keyframes are per-shot. Default duration 5 seconds, max 300 seconds.*

		- ##### 1.4.2. Sequencer UI (Feature)
			*Scrollable thumbnail strip with shot name, editable duration, drag-and-drop reordering, add/delete buttons. Aggregate duration display (total running time). Delete requires confirmation with "don't show again" option.*

		- ##### 1.4.3. Object continuity — global object timeline (Feature)
			*Objects animate on a single global timeline spanning all shots. Shots are windows into this timeline for camera purposes. No per-shot initial state, no continuity propagation — the global timeline IS the continuity. Object keyframes are global; camera keyframes are per-shot.*

		- ##### 1.4.4. Slow-motion (Feature)
			*Per-shot speed factor. Playback-time transform: `globalTime = shotStart + (localTime * speedFactor)`. No keyframes modified — slow-mo is a playback presentation layer. UI: speed percentage display + playback duration readout.*

	- ### 1.5. Keyframe animation (Milestone)
		*Per-property keyframe timeline editor for camera and object animation. After Effects / Premiere-style property tracks with per-track stopwatch model.*

		- ##### 1.5.1. Timeline editor (Feature)
			*Three regions: transport bar, timeline area with tracks and playhead, status bar. Time ruler rescales dynamically with shot duration; user can also manually zoom the ruler.*

		- ##### 1.5.2. Tracks and keyframes (Feature)
			*Camera track (yellow) always present. Object tracks (green) per animated object. Each track has a collapsible dropdown showing per-property sub-tracks with current values. Camera properties: position (x, y, z), pan, tilt, roll, focal length (zoom lenses only). Object properties: position (x, y, z), scale (uniform), rotation (x, y, z). Main keyframe diamond = virtual grouping of individual property keyframes.*

		- ##### 1.5.3. Per-track stopwatch (Feature)
			*Each track has a stopwatch icon (AE/Premiere model). Stopwatch on = Animate mode (manipulations create/update keyframes). Stopwatch off = Setup mode (manipulations change values without keyframing). Top-level stopwatch enables all child property tracks. First manipulation with stopwatch on creates initial keyframe at playhead. Only changed properties get keyframed. Turning stopwatch off warns and deletes all keyframes on that track. All stopwatches default to off.*

		- ##### 1.5.4. Keyframe interaction (Feature)
			*Click to select, drag to reposition, click timeline to scrub, delete selected. Dragging onto an existing keyframe merges. Minimum 1 camera keyframe enforced.*

		- ##### 1.5.5. Interpolation and playback (Feature)
			*Smooth interpolation between keyframes. Real-time playback evaluating all tracks each frame. Stops at shot end. No keyframe creation allowed during playback.*

		- ##### 1.5.6. Path visualization (Feature)
			*Render camera and object animation paths as splines in 3D space. Shows keyframe positions as nodes. Camera nodes include frustum indicators showing look direction. Toggleable.*

- # 2. Workflow essentials (Project)
	*Make it usable for real work: undo mistakes, save projects, bring in your own assets, get frames out.*

	- ### 2.1. Undo / Redo (Milestone)
		*Global command-based undo stack. Every user action (move, keyframe, shot change) is reversible. Selection changes are not undoable.*

		- ##### 2.1.1. Undo stack (Feature)
			*Cmd+Z / Cmd+Shift+Z. Global stack-based history of all user actions. Survives save — saving does not clear the stack. Cross-shot undo does not auto-switch the active shot. Animate mode keyframe + movement is one compound undo step.*

	- ### 2.2. Save / Load (Milestone)
		*Project persistence. Save scene state, shots, animations, and settings to disk. Reopen and continue.*

		- ##### 2.2.1. Project creation wizard (Feature)
			*New Project dialog with Quick Create (name + save location + template) and Advanced Create (multi-layered camera/lens pickers, format derived from body). Two templates: Film and TV. Recent projects list on same screen. Open Project in same dialog. User-extensible templates. See Project Creation Wizard spec.*

		- ##### 2.2.2. Project file format (Feature)
			*Serialize full project state: scenes, objects, shots, keyframes, camera settings, overlay state. Asset bundling: Include All or Link All, 5MB per-asset threshold, 50MB project total. Human-readable format preferred. Version-tolerant loading.*

		- ##### 2.2.3. Save / Load UI (Feature)
			*Save, Save As, Open, Recent Projects (with Clear Recent option). Auto-save on user-configurable interval (default 2 minutes). Crash recovery dialog shows timestamp only.*

		- ##### 2.2.4. Dirty state tracking (Feature)
			*Track unsaved changes. Warn before closing with unsaved work. New Project resets overlay/HUD settings to defaults.*

		- ##### 2.2.5. Multi-scene project structure (Feature)
			*A project contains one or more scenes. Each scene is a self-contained unit — its own objects, environment, lighting, shots, and timelines. Scene tab bar for switching. Scenes are fully independent. Duplicate creates a precise copy. Character definitions are project-level; character state (pose, position) is scene-level. Unlimited scenes with lazy loading. See Multi-Scene Project Structure spec.*

	- ### 2.3. Asset import (Milestone)
		*Bring in 3D models from external tools. Drag-and-drop into the scene.*

		- ##### 2.3.1. Model import (Feature)
			*Drag-and-drop FBX, OBJ, glTF files into the viewport. Auto-generate colliders for selection/gizmos. Multi-mesh models prompt user: treat as single element or unpack. Placed at camera look-point. Supports embedded animation data. Re-importing offers to replace existing.*

		- ##### 2.3.2. Asset library (Feature)
			*Panel showing imported assets for reuse across shots. Thumbnail previews. List-based organization. Warns user about large asset impact on project file size.*

	- ### 2.4. Export (Milestone)
		*Get work out of Fram3d — stills for storyboards, video for sharing with crew.*

		- ##### 2.4.1. Image export (Feature)
			*Export current frame as PNG/JPEG at the active aspect ratio. Optionally include overlays, DOF effect, and shaken camera position. Remembers last-used export directory across all projects.*

		- ##### 2.4.2. Video export (Feature)
			*Proper offline rendering at target resolution and frame rate. Render shot, scene, or full project. Bakes camera shake into keyframes during render. Hard cuts only between shots.*

		- ##### 2.4.3. Storyboard export (Feature)
			*Export all shots as a grid/contact sheet with shot names and durations. PDF or image. User-configurable rows per page. Supports two-column layout. Scene dividers between groups of shots.*

		- ##### 2.4.4. NLE export (Feature)
			*Export to Adobe Media Encoder or Premiere Pro via MCP integration. Embed full camera metadata (per-frame keyframe data for VFX use). EDL timeline format.*

- # 3. Production features (Project)
	*Graduate from a camera tool to a full previs suite. Lighting, characters, scene management, and multi-camera coverage.*

	- ### 3.1. Lighting (Milestone)
		*Place, aim, and adjust lights to previsualize mood and contrast. Not a rendering tool — a blocking tool for DPs.*

		- ##### 3.1.1. Light types (Feature)
			*Directional (sun), point, spot. Position and aim with the same gizmo system as objects. Light type changeable after creation. Lights carry across shots via continuity (like objects). Adding the first light instantly dims default ambient.*

		- ##### 3.1.2. Light properties (Feature)
			*Intensity, color (RGB picker + Kelvin temperature), range, cone angle (spot). Cone wireframe visible when selected only. Adjustable via inspector or direct manipulation. Lights appear in a separate "Lights" section of the hierarchy panel.*

		- ##### 3.1.3. Light animation (Feature)
			*Keyframe light properties on the timeline. Animate intensity, color, position for lighting cues.*

	- ### 3.2. Characters / Actors (Milestone)
		*Poseable, animatable humanoid figures for blocking scenes. At least FrameForge-level manipulation with a better UI. The core differentiator from 2D tools and the biggest complexity leap.*

		- ##### 3.2.1. Mannequin placement and customization (Feature)
			*Drop humanoid mannequins into the scene. Male/female body type selection with continuous height and build sliders. Color-tintable. Expanded customization: age/weight sliders, skin tone, hair style presets + color, facial hair presets, clothing presets (casual, formal, uniform, workwear, athletic), accessories (glasses, hats), footwear. Customization saveable as character presets (cross-project). See Costume Generation spec for full customization table.*

		- ##### 3.2.2. Pose library (Feature)
			*Preset poses: standing, sitting, walking, running, pointing, conversation, lying down. Apply with one click, use as starting points. User-extensible — save custom poses for reuse.*

		- ##### 3.2.3. Custom posing (Feature)
			*Manipulate individual joints — head, arms, torso, legs. IK for natural limb movement. Direct joint selection and rotation. Prop holding: character holds object without precise finger alignment. Body region composition (bone-defined regions on single mesh) for v1.*

		- ##### 3.2.4. Character animation (Feature)
			*Keyframe character poses on the timeline. Interpolate between poses for blocking animation. Walk cycles with custom gaits (limping, sneaking).*

		- ##### 3.2.5. Custom character import (Feature)
			*Import rigged humanoid models (FBX). Automatic rig mapping to Fram3d's pose/animation system. If model has blend shapes with standard naming, auto-map to expression system.*

	- ### 3.3. Facial expressions (Milestone)
		*Blend shape-based facial expressions on characters. Presets, intensity control, keyframeable. Own milestone after 3.2.*

		- ##### 3.3.1. Expression system (Feature)
			*10 preset expressions (neutral, happy, sad, angry, surprised, concerned, scared, disgusted, thinking, smirk) driven by blend shape targets. Intensity slider (0-100%). Expression holds (maintain expression for a duration). Custom expression presets saveable (cross-project). See Facial Expressions spec.*

		- ##### 3.3.2. Eye direction (Feature)
			*Separate controls for eye look direction (left/right/up/down). Independent from expression presets. Keyframeable on its own sub-track.*

		- ##### 3.3.3. Expression animation (Feature)
			*Expression track as sub-track under character's timeline track. Smooth interpolation between expression keyframes. Per-keyframe hold vs interpolate option.*

	- ### 3.4. Camera follow and look-at (Milestone)
		*Camera maintains spatial relationship with a target. Implemented after characters (3.2), before snorricam. Timeline relationship model — not a mode to enter/exit.*

		- ##### 3.4.1. Camera follow (Feature)
			*Persistent relationship (like prop locking). Creates non-keyframeable segment on timeline. Follow distance, height offset, lateral offset are separately keyframeable. Response slider ("Rigid" to "Loose") controls damping. Lead/trail and look-at mode options. Follow path visualized as 3D spline. See New Feature Specs (Camera Tracking/Following) for full design.*

		- ##### 3.4.2. Look-at tracking (Feature)
			*Camera stays in position, rotates to track target. Right-click object → "Look At." Separate from follow — can be used independently on a static camera.*

	- ### 3.5. Snorricam (Milestone)
		*Body-mounted camera rig. After 3.4 (camera follow). Requires character animation.*

		- ##### 3.5.1. Snorricam (Feature)
			*Front mount (camera faces actor's face) and back mount (camera faces away). Camera locks to character's root motion. Mount offset controls (height, distance). Focal length still adjustable. Baked to keyframes on export. See New Feature Specs (Snorricam) for full design.*

	- ### 3.6. Object linking & grouping (Milestone)
		*Temporal linking (object follows character or object) and persistent grouping (simultaneous transforms). Flat scene graph with positional attachment.*

		- ##### 3.6.1. Object linking (Feature)
			*Temporal object-to-character and object-to-object linking on the global timeline. Viewport: click-drag to link, right-click to unlink. Panel: pickwhip tool. Anchor point defines attachment offset. Linked period greyed out on timeline. Positional attachment — flat scene graph, transform override during linked period.*

		- ##### 3.6.2. Object grouping (Feature)
			*Groups with own transform. Single-click selects group; double-click enters to edit members. Gizmo at bounding box center. No timeline behavior.*

		- ##### 3.6.3. Object list panel (Feature)
			*Flat list of all scene objects with link indicators, group markers, and pickwhip handles. Click to select, pickwhip to link. Separate "Lights" section. Dockable.*

	- ### 3.7. Set decoration library (Milestone)
		*Built-in and marketplace-connected asset library for dressing scenes.*

		- ##### 3.7.1. Built-in asset library (Feature)
			*Core set of common props: tables, chairs, doors, windows, vehicles, trees, walls, floors. Categorized and searchable. Visual style: clearly "previs" (stylized/abstract, not realistic).*

		- ##### 3.7.2. Marketplace integration (Feature)
			*Browse and import free assets from Sketchfab, Unity Asset Store, TurboSquid, and Mixamo directly in-app. Free assets only. Handle format conversion and collider generation.*

		- ##### 3.7.3. User asset management (Feature)
			*Organize imported and marketplace assets into user collections. Tag, favorite, quick-access for reuse across projects. Local storage only.*

	- ### 3.8. Selection and manipulation refinements (Milestone)
		*Quality-of-life improvements to object interaction as scenes get more complex.*

		- ##### 3.8.1. Multi-select (Feature)
			*Shift-click to add to selection. Drag-select (marquee) with crossing selection. Move/rotate/scale multiple objects together. Gizmo pivot at center of selection bounding box.*

		- ##### 3.8.2. Grid snapping (Feature)
			*Snap objects to grid from fixed presets (0.1, 0.25, 0.5, 1.0, 2.0m). Hold modifier to override. Rotation snap at configurable increments (default 15°).*

		- ##### 3.8.3. Custom interpolation curves (Feature)
			*Per-property easing: linear, ease-in, ease-out, ease-in-out, bezier. Curve editor UI.*

	- ### 3.9. Multi-camera (Milestone)
		*Up to four cameras per shot with independent keyframe timelines, shared object animation, and a coverage splitting track.*

		- ##### 3.9.1. Per-shot camera addition (Feature)
			*Add up to 4 cameras (A, B, C, D) per shot. Each camera labeled and color-coded (user-configurable). Camera preview elements appear above the current shot.*

		- ##### 3.9.2. Multi-camera timelines (Feature)
			*Each camera has its own camera keyframe timeline within the shot. Object keyframe timelines are shared across all cameras.*

		- ##### 3.9.3. Active camera and switching (Feature)
			*Right-click camera preview → "Set to active." Shift+1/2/3/4 to switch. Switching during playback auto-splits coverage at the switch point.*

		- ##### 3.9.4. Coverage splitting (Feature)
			*Coverage track row in the timeline. Right-click → "Split coverage" to divide. Drag division points to adjust cut timing.*

		- ##### 3.9.5. Multi-split (Feature)
			*Right-click → "Multi-split": prompted with "split every {n} frames."*

- # 4. AI-assisted previsualization (Project)
	*Use language models to accelerate previs work — describe what you want, get a starting point to refine.*

	- ### 4.1. Natural language shot description (Milestone)
		*Describe a shot in words, get camera position and framing as a starting point.*

		- ##### 4.1.1. Shot-from-text (Feature)
			*"Medium close-up of the actor at the table, slightly low angle" → camera positioned accordingly. Compound descriptions ("OTS then reverse") create multiple shots. Input via command palette.*

		- ##### 4.1.2. Shot vocabulary (Feature)
			*Understands cinematic language: shot sizes (ECU, CU, MCU, MS, MLS, LS, ELS), angles (low, high, dutch, eye-level, bird's eye, worm's eye), movement types. Directional references relative to camera.*

	- ### 4.2. Automatic blocking (Milestone)
		*Position actors and objects from a scene description.*

		- ##### 4.2.1. Scene-from-text (Feature)
			*"Two people sitting across a table, one standing by the window" → mannequins placed and posed. Handles environment generation from available assets. Supports motion descriptions. Text input for v1.*

		- ##### 4.2.2. Blocking refinement (Feature)
			*"Move the standing actor closer to the table" → incremental text adjustments. Conversational context persists. Ambiguous references prompt for clarification.*

	- ### 4.3. Camera suggestions (Milestone)
		*Given a blocked scene, suggest camera setups for coverage.*

		- ##### 4.3.1. Coverage suggestions (Feature)
			*Suggest master shot, OTS, close-ups for dialogue scenes. Three-way style toggle: conservative / minimal / comprehensive. User-configurable max shots per generation.*

		- ##### 4.3.2. Shot list generation (Feature)
			*Generate a full shot list from a scene description or script excerpt. Create shots in sequence.*

- # 5. Feature parity and extensions (Project)
	*Competitive features identified from market research (Previs Pro, RADiCAL Canvas, FrameForge). Prioritized by impact on user workflow.*

	- ### 5.1. Premade environments (Milestone) — HIGHER PRIORITY
		*Ship with a library of ready-to-use environments. Interior and exterior sets that establish context in one click.*

		- ##### 5.1.1. Environment library (Feature)
			*Premade sets: living room, office, restaurant, bar, street, alley, park, parking lot, warehouse, courtroom, hospital room, classroom. Closed walls and ceiling. Real-world scale. Each includes walls/floors/props/lighting and one character. Objects are independent after placement — fully editable. User can save custom environments as reusable templates.*

	- ### 5.2. Script import (Milestone) — HIGHER PRIORITY
		*Import screenplays to auto-populate scenes, characters, and dialogue. Bridges the gap between script and previs.*

		- ##### 5.2.1. Script parsing (Feature)
			*Import Final Draft (.fdx) and Fountain (.fountain) files. Parse scene headings, character names, and action lines. Auto-create scenes from scene headings (one scene per heading, NOT one shot). Auto-create named character placeholders. Per-scene environment picker during import. Dialogue stored as project-level reference library (user picks from list when creating subtitles). See Multi-Scene Project Structure spec for import interaction.*

	- ### 5.3. 2D overhead view (Milestone) — HIGHER PRIORITY
		*Bird's-eye 2D view of the scene showing object positions, camera frustums, and light positions.*

		- ##### 5.3.1. 2D scene view (Feature)
			*Top-down orthographic view. Objects shown as labeled icons/silhouettes. Camera shown as frustum with FOV cone. Lights shown as standard lighting diagram symbols. Drag objects to reposition. Camera view preview in corner. Movement paths shown as dotted lines with keyframe position dots. Switchable between 3D and 2D view.*

	- ### 5.4. Set builder (Milestone)
		*Lightweight environment building tool. Separate page in the application. Room shapes, materials, furniture placement, lighting presets. See Set Builder spec.*

		- ##### 5.4.1. Room construction (Feature)
			*Pick from basic shapes (rectangle, L-shape, T-shape, open) or draw custom walls. Room dimensions via number inputs or dragging. Material presets for walls/floors/ceilings. Room wizard (guided flow). Auto-furnish option.*

		- ##### 5.4.2. Wall drawing (Feature)
			*Draw walls in 2D overhead view by clicking to place segments. Supports straight and curved segments. Doors and windows as cutouts with configurable dimensions. Two-sided materials. Close room button. Room presets. Walls are real 3D geometry. See Wall Drawing spec.*

	- ### 5.5. AI prop generation (Milestone) — MEDIUM PRIORITY
		*Generate 3D props from text descriptions. Fills gaps in the asset library.*

		- ##### 5.5.1. Text-to-prop (Feature)
			*Text prompt generates a 3D model suitable for previs. Low-poly, consistent visual style. Auto-generates collider. Saved to asset library for reuse. Requires internet connection and API key.*

	- ### 5.6. Costume generation (Milestone) — LOWER PRIORITY
		*Generate 3D clothing meshes on existing mannequin. Clothing-on-mannequin approach — skeleton stays fixed, auto-binding to known skeleton is tractable. See Costume Generation spec.*

		- ##### 5.6.1. AI costume generation (Feature)
			*Text description → 3D clothing meshes (jacket, pants, hat, etc.) auto-bound to mannequin skeleton. Costume library for reuse (cross-project, cross-character). Body type changes → costume stretches. Supplements the non-AI mannequin customization (3.2.1).*

	- ### 5.7. LiDAR scanning (Milestone) — LOWER PRIORITY
		*Scan real locations with iPhone/iPad LiDAR and import as scene backgrounds. Requires companion iOS app.*

		- ##### 5.7.1. LiDAR import (Feature)
			*Import point cloud or mesh data from LiDAR scans (iPhone Pro, iPad Pro). Use as scene backdrop for accurate spatial reference. Not editable — display only.*

		- ##### 5.7.2. Companion iOS app (Feature)
			*Minimal iOS app for capturing LiDAR scans. Scan capture only.*

	- ### 5.8. Style grading (Milestone) — MAYBE
		*Apply visual styles to rendered frames and sequences. "Film noir," "golden hour," "gritty documentary."*

		- ##### 5.8.1. Frame style grading (Feature)
			*Apply visual style presets or AI-generated styles to exported frames and video. Preserves camera framing, blocking, and timing.*

	- ### 5.9. AI video generation (Milestone) — MAYBE
		*Generate stylized video from previs sequences using AI. Speculative — depends on future AI capabilities.*

	> **Explicitly NOT building:**
	> - Color grading tools (not previs — that's post-production)
	> - AR overlay (placing 3D assets on real camera feed — requires mobile-first architecture)
	> - Facial scanning / face capture (out of scope for previs)
	> - Full AI character generation (clothing-on-mannequin is the reliable path — see Costume Generation spec)

- # Sequencing principles

	1. **Ship the camera first.** The virtual camera is the product. Everything else supports it. Get camera + overlays + basic scene working before building timeline features.
	2. **Workflow before features.** Undo, save, and export come before lighting, characters, and multi-camera. A tool people can't save their work in doesn't get used.
	3. **Characters are the complexity cliff.** Poseable humanoids with IK, pose libraries, and animation are an order of magnitude harder than everything in Project 1. Don't underestimate this.
	4. **AI features are speculative.** Project 4 depends on LLM capabilities that may or may not work well enough. Build the tool so it's great without AI — AI is upside, not the product.
	5. **Don't build a 3D editor.** Every feature should pass the test: "does a director need this to plan a shot?" If the answer is "only a 3D artist would use this," cut it.
	6. **Multi-camera is a capstone.** Get single-camera workflow polished before introducing multi-cam complexity. Coverage splitting builds on mature keyframe and sequencer foundations.
	7. **Camera rigs depend on characters.** Camera follow, look-at, and snorricam all require animated characters to be meaningful. They belong after 3.2, not before.
