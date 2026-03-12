# Fram3d Product Roadmap

**Status**: Active

---

Fram3d is a 3D previsualization tool for filmmakers. Cinematic language over 3D complexity — focal length, dolly, crane — not Maya jargon.

Reading order = build order. Phases are numbered in the sequence they should be built. Features within a milestone that are deferred to a later phase are marked accordingly.

---

- # Phase 1 — The Camera
	*You're building a camera tool. The camera comes first. Everything else supports it.*

	- ### 1.1. Virtual camera
		*Physically-based camera rig with movement, lens, focus, and shake — controlled entirely by mouse + modifier keys.*

		- ##### 1.1.1. Camera movement
			*Pan, tilt, dolly, truck, crane, roll, orbit, dolly zoom, reset. All speeds configurable. Dolly zoom lockable to a specific object. Orbit pivot: persistent last-focus model (Maya/Unity).*

		- ##### 1.1.2. Lens system
			*Focal length 14–400mm, physically accurate FOV from real sensor dimensions, presets, smooth transitions. Anamorphic lenses supported: squeeze factors (1.33x, 1.5x, 1.8x, 2x), FOV computed from squeeze factor, aspect ratio auto-locked when anamorphic lens is active.*

		- ##### 1.1.3. Camera body and lens database
			*Full database of real-world camera bodies and lens sets. 200+ cameras across 17 manufacturers (Canon, Sony, ARRI, RED, Panasonic, Blackmagic, Fujifilm, Panavision, Vision Research Phantom, IMAX, Aaton, Bolex, Mitchell, Eclair, and others). 100+ lens sets including spherical primes, spherical zooms, anamorphic primes/zooms, and stills lenses adapted for cinema. Generic bodies available for quick start. Database includes sensor dimensions, native resolution, supported frame rates, and mount compatibility. See `reference data/camera-lens-database.json`.*

		- ##### 1.1.4. Focus
			*Animated focus-on-object — smooth transition, calculates optimal distance from bounds and FOV, frames with breathing room. DOF focus distance automatically follows focus target.*

		- ##### 1.1.5. Depth of field preview
			*Visualize shallow/deep DOF based on focal length, aperture, and focus distance. Aperture stops match real cinema lenses (f/1.4, f/2, f/2.8, etc.). Cinematic bokeh preview — not photorealistic, just enough to see what's sharp vs. soft. Oval bokeh when anamorphic lens is active (future polish).*

		- ##### 1.1.6. Camera shake
			*Procedural handheld effect. Configurable amplitude/frequency, X/Y rotation only. Per-shot enable/disable. Cosmetic only — baked to keyframes only during export.*

	- ### 1.2. Camera overlays
		*Composition aids that sit on top of the 3D viewport — aspect ratio masks, frame guides, info HUD, and subtitle text.*

		- ##### 1.2.1. Aspect ratio masks
			*Letterbox/pillarbox bars. 8 ratios: Full Screen, 16:9, 16:10, 1.85:1, 2.35:1, 2.39:1, 2:1, 4:3. Default 16:9. A cycles forward, Shift+A cycles backward. Auto-locked to computed delivery format when anamorphic lens is active.*

		- ##### 1.2.2. Frame guides
			*Rule of thirds, center cross (fixed pixel size), safe zones (title 90%, action 93% — configurable with defaults). All start hidden, toggled independently. Global visibility state, not per-shot.*

		- ##### 1.2.3. Camera info HUD
			*Overlay showing focal length, height, angle of view, aspect ratio, body, lens preset, and squeeze factor (if anamorphic). Toggleable via shortcut. Counts as an overlay for export purposes.*

		- ##### 1.2.4. Subtitle overlay
			*Text layer overlaid on the frame in a fixed subtitle position. Timeable within a shot (adjustable start/end time). Configurable color, size, and font per layer. No animations, no transitions.*

	**Exit criteria:** You can move a physically-accurate camera through 3D space, see the correct aspect ratio, toggle frame guides, and read focal length / height / AOV from the HUD.

- # Phase 2 — The Scene
	*Give the camera something to look at. Click things, move things, see the result through the camera. Split-view as soon as you have camera view + director view.*

	- ### 2.1. Scene management
		*Click objects, move them around, see visual feedback. Single-selection with translate/rotate/scale gizmos. Objects exist in world space across all shots.*

		- ##### 2.1.1. Scene elements
			*Any object with a collider is interactive. Hover highlighting, selection highlighting, click-to-select, click-empty-to-deselect. Compound objects are single selectable units.*

		- ##### 2.1.2. Transform gizmos
			*Translate (arrows), rotate (rings), scale (uniform proportional only — single handle). Axis-colored (RGB = XYZ), always-on-top, constant screen size.*

		- ##### 2.1.3. Ground plane
			*Infinite ground plane with visible grid for spatial reference. Always visible. Gives context for camera height and blocking distances.*

		- ##### 2.1.4. Object duplication
			*Ctrl+D to duplicate selected object. Copy inherits position with reasonable offset. Duplicated names appended with incrementing number (e.g., Chair_1, Chair_2).*

		- ##### 2.1.5. Director view
			*Global utility camera decoupled from the shot timeline. Move freely to position objects and cameras without creating keyframes. Separate from the virtual camera rig — does not affect shot animation.*

	- ### 2.2. Viewport panel system
		*Multi-panel viewport with configurable layouts. Per-panel view selector (3D Viewport, 2D Designer, Director Mode). Layout chooser for single, side-by-side, and three-panel arrangements.*

		- ##### 2.2.1. Panel layouts
			*Three layout options: single panel (default), side-by-side (two panels), and one-large-two-small (three panels). Layout chooser buttons in the viewport area. Each panel independently selectable between 3D Viewport, 2D Designer, and Director Mode.*

		- ##### 2.2.2. View modes
			*3D Viewport: standard camera view through the virtual camera rig. 2D Designer: top-down orthographic view (see 8.2). Director Mode: free utility camera decoupled from the shot timeline (see 2.1.5). Any view mode can be assigned to any panel.*

	**Exit criteria:** You can place objects, select them, move/rotate/scale with gizmos, duplicate, and use split-view to see camera view and director view simultaneously.

- # Phase 3 — Time
	*Turn a static camera position into a moving shot. Multiple shots, a timeline, keyframes, playback.*

	- ### 3.1. Shot sequencer
		*Horizontal strip of shot thumbnails. Add, delete, reorder, select shots. Each shot is an independent camera animation over shared world-space objects. Scene tabs for multi-scene project structure.*

		- ##### 3.1.1. Shot model
			*Shot = name + duration + camera animation + time range on global object timeline. Auto-named Shot_01, Shot_02, etc. Objects animate on a global timeline — shots are windows into it. Camera keyframes are per-shot. Default duration 5 seconds, max 300 seconds.*

		- ##### 3.1.2. Sequencer UI
			*Scrollable thumbnail strip with shot name, editable duration, drag-and-drop reordering, add/delete buttons. Aggregate duration display (total running time). Delete requires confirmation with "don't show again" option.*

		- ##### 3.1.3. Object continuity — global object timeline
			*Objects animate on a single global timeline spanning all shots. Shots are windows into this timeline for camera purposes. No per-shot initial state, no continuity propagation — the global timeline IS the continuity. Object keyframes are global; camera keyframes are per-shot.*

		- ##### 3.1.4. Multi-scene project structure
			*A project contains one or more scenes. Each scene is a self-contained unit — its own objects, environment, lighting, shots, and timelines. Scene tab bar above the timeline for switching. Scenes are fully independent. Character definitions are project-level; character state (pose, position) is scene-level. Scene order defines full-project playback sequence. See Multi-Scene Project Structure spec.*

		- ##### 3.1.5. Timeline overview minimap *(deferred to Phase 8)*
			*A collapsible overview panel showing a bird's-eye view of the full timeline — all shots, all tracks, and a view window indicating the currently visible range. Clicking the minimap navigates to that position. Shows the playhead position relative to the entire project.*

	- ### 3.2. Keyframe animation
		*Per-property keyframe timeline editor for camera and object animation. After Effects / Premiere-style property tracks with per-track stopwatch model.*

		- ##### 3.2.1. Timeline editor
			*Three regions: transport bar, timeline area with tracks and playhead, status bar. Time ruler rescales dynamically with shot duration; user can also manually zoom the ruler.*

		- ##### 3.2.2. Tracks and keyframes
			*Camera track (yellow) always present. Object tracks (green) per animated object. Each track has a collapsible dropdown showing per-property sub-tracks with current values. Camera properties: position (x, y, z), pan, tilt, roll, focal length (zoom lenses only). Object properties: position (x, y, z), scale (uniform), rotation (x, y, z). Main keyframe diamond = virtual grouping of individual property keyframes.*

		- ##### 3.2.3. Per-track stopwatch
			*Each track has a stopwatch icon (AE/Premiere model). Stopwatch on = Animate mode (manipulations create/update keyframes). Stopwatch off = Setup mode (manipulations change values without keyframing). Top-level stopwatch enables all child property tracks. First manipulation with stopwatch on creates initial keyframe at playhead. Only changed properties get keyframed. Turning stopwatch off warns and deletes all keyframes on that track. All stopwatches default to off.*

		- ##### 3.2.4. Keyframe interaction
			*Click to select, drag to reposition, click timeline to scrub, delete selected. Dragging onto an existing keyframe merges. Minimum 1 camera keyframe enforced.*

		- ##### 3.2.5. Interpolation and playback
			*Smooth interpolation between keyframes. Real-time playback evaluating all tracks each frame. Stops at shot end. No keyframe creation allowed during playback.*

		- ##### 3.2.6. Path visualization
			*Render camera and object animation paths as splines in 3D space. Shows keyframe positions as nodes. Camera nodes include frustum indicators showing look direction. Toggleable.*

	**Exit criteria:** You can create multiple shots, animate the camera and objects with keyframes, scrub the timeline, and play back a sequence. The core previs loop works end-to-end.

	**Note on 3.1.4 (multi-scene):** Build the data model (Project → Scene → Shots) from the start so you don't have to refactor later. The scene tab switching UI can be the last thing implemented in this phase.

- # Phase 4 — Persistence & I/O
	*A tool people can't save their work in doesn't get used.*

	- ### 4.1. Undo / Redo
		*Global command-based undo stack. Every user action (move, keyframe, shot change) is reversible. Selection changes are not undoable.*

		- ##### 4.1.1. Undo stack
			*Cmd+Z / Cmd+Shift+Z. Global stack-based history of all user actions. Survives save — saving does not clear the stack. Cross-shot undo does not auto-switch the active shot. Animate mode keyframe + movement is one compound undo step.*

	- ### 4.2. Save / Load
		*Project persistence. Save scene state, shots, animations, and settings to disk. Reopen and continue.*

		- ##### 4.2.1. Project creation wizard
			*New Project dialog with Quick Create (name + save location + template) and Advanced Create (multi-layered camera/lens pickers, format derived from body). Two templates: Film and TV. Recent projects list on same screen. Open Project in same dialog. User-extensible templates. See Project Creation Wizard spec.*

		- ##### 4.2.2. Project file format
			*Serialize full project state: scenes, objects, shots, keyframes, camera settings, overlay state. Asset bundling: Include All or Link All, 5MB per-asset threshold, 50MB project total. Human-readable format preferred. Version-tolerant loading.*

		- ##### 4.2.3. Save / Load UI
			*Save, Save As, Open, Recent Projects (with Clear Recent option). Auto-save on user-configurable interval (default 2 minutes). Crash recovery dialog shows timestamp only.*

		- ##### 4.2.4. Dirty state tracking
			*Track unsaved changes. Warn before closing with unsaved work. New Project resets overlay/HUD settings to defaults.*

		- ##### 4.2.5. Scene persistence
			*Save and load multi-scene projects. Serialize each scene independently with lazy loading. Duplicate scene creates a precise copy. Scene data includes all objects, environment, lighting, shots, and timelines. Project-level data includes character definitions and settings. See Multi-Scene Project Structure spec and 3.1.4.*

	- ### 4.3. Asset import
		*Bring in 3D models from external tools. Drag-and-drop into the scene.*

		- ##### 4.3.1. Model import
			*Drag-and-drop FBX, OBJ, glTF files into the viewport. Auto-generate colliders for selection/gizmos. Multi-mesh models prompt user: treat as single element or unpack. Placed at camera look-point. Supports embedded animation data. Re-importing offers to replace existing.*

		- ##### 4.3.2. Asset library
			*Panel showing imported assets for reuse across shots. Thumbnail previews. List-based organization. Warns user about large asset impact on project file size.*

	- ### 4.4. Export
		*Get work out of Fram3d — stills for storyboards, video for sharing with crew.*

		- ##### 4.4.1. Image export
			*Export current frame as PNG/JPEG at the active aspect ratio. Optionally include overlays, DOF effect, and shaken camera position. Remembers last-used export directory across all projects.*

		- ##### 4.4.2. Video export
			*Proper offline rendering at target resolution and frame rate. Render shot, scene, or full project. Bakes camera shake into keyframes during render. Hard cuts only between shots.*

		- ##### 4.4.3. Storyboard export *(deferred to Phase 10)*
			*Export all shots as a grid/contact sheet with shot names and durations. PDF or image. User-configurable rows per page. Supports two-column layout. Scene dividers between groups of shots.*

		- ##### 4.4.4. NLE export *(deferred to Phase 10)*
			*Export to Adobe Media Encoder or Premiere Pro via MCP integration. Embed full camera metadata (per-frame keyframe data for VFX use). EDL timeline format.*

	**Exit criteria:** The tool is genuinely usable. Save work, undo mistakes, import models, export stills and video. This is the **alpha** — a solo filmmaker could do real previs work with this.

	**Implementation note for 4.1:** The command pattern infrastructure should be scaffolded early (Phases 1–3). Each new feature adds its Undo/Redo commands as it's built. Phase 4 is where you finalize, stress-test, and handle the hard edge cases (cross-shot undo, animate mode compound steps, gesture coalescing).

- # Phase 5 — Scene Dressing
	*Solve the empty canvas problem. Give users environments and props so they can start blocking immediately.*

	- ### 5.1. Lighting
		*Place, aim, and adjust lights to previsualize mood and contrast. Not a rendering tool — a blocking tool for DPs.*

		- ##### 5.1.1. Light types
			*Directional (sun), point, spot. Position and aim with the same gizmo system as objects. Light type changeable after creation. Lights carry across shots via continuity (like objects). Adding the first light instantly dims default ambient.*

		- ##### 5.1.2. Light properties
			*Intensity, color (RGB picker + Kelvin temperature), range, cone angle (spot). Cone wireframe visible when selected only. Adjustable via inspector or direct manipulation. Lights appear in a separate "Lights" section of the hierarchy panel.*

		- ##### 5.1.3. Light animation
			*Keyframe light properties on the timeline. Animate intensity, color, position for lighting cues.*

	- ### 5.2. Set decoration library
		*Built-in and marketplace-connected asset library for dressing scenes.*

		- ##### 5.2.1. Built-in asset library
			*Core set of common props: tables, chairs, doors, windows, vehicles, trees, walls, floors. Categorized and searchable. Visual style: clearly "previs" (stylized/abstract, not realistic).*

		- ##### 5.2.2. Marketplace integration *(deferred to Phase 10)*
			*Browse and import free assets from Sketchfab, Unity Asset Store, TurboSquid, and Mixamo directly in-app. Free assets only. Handle format conversion and collider generation.*

		- ##### 5.2.3. User asset management *(deferred to Phase 10)*
			*Organize imported and marketplace assets into user collections. Tag, favorite, quick-access for reuse across projects. Local storage only.*

	- ### 5.3. Premade environments
		*Ship with a library of ready-to-use environments. Interior and exterior sets that establish context in one click.*

		- ##### 5.3.1. Environment library
			*Premade sets: living room, office, restaurant, bar, street, alley, park, parking lot, warehouse, courtroom, hospital room, classroom. Closed walls and ceiling. Real-world scale. Each includes walls/floors/props/lighting and one character. Objects are independent after placement — fully editable. User can save custom environments as reusable templates.*

	**Exit criteria:** Users open the app, pick a premade environment (or browse built-in props), the scene has lighting, and it looks like a real location. No more blank canvas.

	**Content pipeline note:** The actual 3D assets and environment models are art work that can be produced in parallel with Phase 4 code. The code for browsing and placing them is simple — don't let content creation block code progress.

- # Phase 6 — Characters
	*The complexity cliff. This is where Fram3d becomes a real previsualization tool — and where the hardest engineering work lives.*

	- ### 6.1. Characters / Actors
		*Poseable, animatable humanoid figures for blocking scenes. At least FrameForge-level manipulation with a better UI. The core differentiator from 2D tools and the biggest complexity leap.*

		- ##### 6.1.1. Mannequin placement and customization
			*Drop humanoid mannequins into the scene. Male/female body type selection with continuous height and build sliders. Color-tintable. Expanded customization: age/weight sliders, skin tone, hair style presets + color, facial hair presets, basic clothing color/style presets (solid color changes, not detailed costumes — detailed costumes are in 12.2 AI Costume Generation), accessories (glasses, hats), footwear. Customization saveable as character presets (cross-project).*

		- ##### 6.1.2. Pose library
			*Preset poses: standing, sitting, walking, running, pointing, conversation, lying down. Apply with one click, use as starting points. User-extensible — save custom poses for reuse.*

		- ##### 6.1.3. Custom posing
			*Manipulate individual joints — head, arms, torso, legs. IK for natural limb movement. Direct joint selection and rotation. Prop holding: character holds object without precise finger alignment. Body region composition (bone-defined regions on single mesh) for v1.*

		- ##### 6.1.4. Character animation
			*Keyframe character poses on the timeline. Interpolate between poses for blocking animation. Walk cycles with custom gaits (limping, sneaking).*

		- ##### 6.1.5. Custom character import
			*Import rigged humanoid models (FBX). Automatic rig mapping to Fram3d's pose/animation system. If model has blend shapes with standard naming, auto-map to expression system.*

	- ### 6.2. Camera follow and look-at
		*Camera maintains spatial relationship with a target. Implemented after characters (6.1), before snorricam. Timeline relationship model — not a mode to enter/exit.*

		- ##### 6.2.1. Camera follow
			*Persistent relationship (like prop locking). Creates non-keyframeable segment on timeline. Follow distance, height offset, lateral offset are separately keyframeable. Response slider ("Rigid" to "Loose") controls damping. Lead/trail and look-at mode options. Follow path visualized as 3D spline. See New Feature Specs (Camera Tracking/Following) for full design.*

		- ##### 6.2.2. Look-at tracking
			*Camera stays in position, rotates to track target. Right-click object → "Look At." Separate from follow — can be used independently on a static camera.*

	- ### 6.3. Object linking & grouping
		*Temporal linking (object follows character or object) and persistent grouping (simultaneous transforms). Flat scene graph with positional attachment.*

		- ##### 6.3.1. Object linking
			*Temporal object-to-character and object-to-object linking on the global timeline. Viewport: click-drag to link, right-click to unlink. Panel: pickwhip tool. Anchor point defines attachment offset. Linked period greyed out on timeline. Positional attachment — flat scene graph, transform override during linked period.*

		- ##### 6.3.2. Object grouping
			*Groups with own transform. Single-click selects group; double-click enters to edit members. Gizmo at bounding box center. No timeline behavior.*

		- ##### 6.3.3. Object list panel
			*Flat list of all scene objects with link indicators, group markers, and pickwhip handles. Click to select, pickwhip to link. Separate "Lights" section. Dockable.*

	**Exit criteria:** You can drop characters into a premade environment, pose them, animate their movement, have the camera follow them, and link props to their hands. This is the feature set that differentiates Fram3d from every 2D tool.

	**Why 6.3 before 7.1:** Prop holding (object linking) is fundamental blocking — a character needs to carry a briefcase before they need facial expressions. Blocking comes before performance.

- # Phase 7 — Character Extensions
	*Enhance characters with expressions and specialized camera rigs.*

	- ### 7.1. Facial expressions
		*Blend shape-based facial expressions on characters. Presets, intensity control, keyframeable. Own milestone after 6.1.*

		- ##### 7.1.1. Expression system
			*10 preset expressions (neutral, happy, sad, angry, surprised, concerned, scared, disgusted, thinking, smirk) driven by blend shape targets. Intensity slider (0-100%). Expression holds (maintain expression for a duration). Custom expression presets saveable (cross-project). See Facial Expressions spec.*

		- ##### 7.1.2. Eye direction
			*Separate controls for eye look direction (left/right/up/down). Independent from expression presets. Keyframeable on its own sub-track.*

		- ##### 7.1.3. Expression animation
			*Expression track as sub-track under character's timeline track. Smooth interpolation between expression keyframes. Per-keyframe hold vs interpolate option.*

	- ### 7.2. Snorricam
		*Body-mounted camera rig. After 6.2 (camera follow). Requires character animation.*

		- ##### 7.2.1. Snorricam
			*Front mount (camera faces actor's face) and back mount (camera faces away). Camera locks to character's root motion. Mount offset controls (height, distance). Focal length still adjustable. Baked to keyframes on export. See New Feature Specs (Snorricam) for full design.*

	**Exit criteria:** Characters can emote and have eye direction. Snorricam shots are possible.

- # Phase 8 — Polish & Views
	*Quality-of-life improvements and new ways to see the scene. The tool works — now make it work better.*

	- ### 8.1. Selection and manipulation refinements
		*Quality-of-life improvements to object interaction as scenes get more complex.*

		- ##### 8.1.1. Multi-select
			*Shift-click to add to selection. Drag-select (marquee) with crossing selection. Move/rotate/scale multiple objects together. Gizmo pivot at center of selection bounding box.*

		- ##### 8.1.2. Grid snapping
			*Snap objects to grid from fixed presets (0.1, 0.25, 0.5, 1.0, 2.0m). Hold modifier to override. Rotation snap at configurable increments (default 15°).*

		- ##### 8.1.3. Custom interpolation curves
			*Per-property easing: linear, ease-in, ease-out, ease-in-out, bezier. Curve editor UI.*

	- ### 8.2. 2D Designer
		*Bird's-eye 2D view of the scene showing object positions, camera frustums, and light positions. Part of the viewport panel system (2.2) — available as a panel view mode.*

		- ##### 8.2.1. 2D Designer view
			*Top-down orthographic view. Objects shown as labeled icons/silhouettes. Camera shown as frustum with FOV cone. Lights shown as standard lighting diagram symbols. Drag objects to reposition. Camera view preview in corner. Movement paths shown as dotted lines with keyframe position dots. Available as a viewport panel mode alongside 3D Viewport and Director Mode.*

	- ### 8.3. Script import
		*Import screenplays to auto-populate scenes, characters, and dialogue. Bridges the gap between script and previs.*

		- ##### 8.3.1. Script parsing
			*Import Final Draft (.fdx) and Fountain (.fountain) files. Parse scene headings, character names, and action lines. Auto-create scenes from scene headings (one scene per heading, NOT one shot). Auto-create named character placeholders. Per-scene environment picker during import. Dialogue stored as project-level reference library (user picks from list when creating subtitles). See Multi-Scene Project Structure spec for import interaction.*

	- ### 8.4. Slow-motion
		*Per-shot speed factor that changes the playback clock without modifying keyframes. A playback presentation layer.*

		- ##### 8.4.1. Slow-motion
			*Per-shot speed factor. Playback-time transform: `globalTime = shotStart + (localTime * speedFactor)`. No keyframes modified — slow-mo is a playback presentation layer. UI: text input for speed percentage (like Premiere/AE), text input for target frame rate, dropdown showing camera-specific options ("X% (Y fps)"), checkbox to ignore camera limitations. Sequencer shows playback duration (not authored duration) and speed percentage overlay on shot card. Per-shot only for v1.*

	**Exit criteria:** The tool is polished enough for production use. Multi-select, precision placement, professional animation curves, overhead view, script-to-previs workflow, slow-motion.

- # Phase 9 — Multi-camera
	*Get single-camera workflow polished before introducing multi-cam complexity.*

	- ### 9.1. Multi-camera
		*Up to four cameras per shot with independent keyframe timelines, shared object animation, and a coverage splitting track.*

		- ##### 9.1.1. Per-shot camera addition
			*Add up to 4 cameras (A, B, C, D) per shot. Each camera labeled and color-coded (user-configurable). Camera preview elements appear above the current shot.*

		- ##### 9.1.2. Multi-camera timelines
			*Each camera has its own camera keyframe timeline within the shot. Object keyframe timelines are shared across all cameras.*

		- ##### 9.1.3. Active camera and switching
			*Right-click camera preview → "Set to active." Shift+1/2/3/4 to switch. Switching during playback auto-splits coverage at the switch point.*

		- ##### 9.1.4. Coverage splitting
			*Coverage track row in the timeline. Right-click → "Split coverage" to divide. Drag division points to adjust cut timing.*

		- ##### 9.1.5. Multi-split
			*Right-click → "Multi-split": prompted with "split every {n} frames."*

- # Phase 10 — Set Building
	*Lightweight environment construction tool. Uses the 2D Designer from Phase 8.*

	- ### 10.1. Set builder
		*Lightweight environment building tool. Separate page in the application. Room shapes, materials, furniture placement, lighting presets. See Set Builder spec.*

		- ##### 10.1.1. Room construction
			*Pick from basic shapes (rectangle, L-shape, T-shape, open) or draw custom walls. Room dimensions via number inputs or dragging. Material presets for walls/floors/ceilings. Room wizard (guided flow). Auto-furnish option.*

		- ##### 10.1.2. Wall drawing
			*Draw walls in 2D overhead view by clicking to place segments. Supports straight and curved segments. Doors and windows as cutouts with configurable dimensions. Two-sided materials. Close room button. Room presets. Walls are real 3D geometry. See Wall Drawing spec.*

- # Phase 11 — AI Features
	*The tool must be great without AI. AI is upside, not the product.*

	- ### 11.1. Natural language shot description
		*Describe a shot in words, get camera position and framing as a starting point.*

		- ##### 11.1.1. Shot-from-text
			*"Medium close-up of the actor at the table, slightly low angle" → camera positioned accordingly. Compound descriptions ("OTS then reverse") create multiple shots. Input via command palette.*

		- ##### 11.1.2. Shot vocabulary
			*Understands cinematic language: shot sizes (ECU, CU, MCU, MS, MLS, LS, ELS), angles (low, high, dutch, eye-level, bird's eye, worm's eye), movement types. Directional references relative to camera.*

	- ### 11.2. Automatic blocking
		*Position actors and objects from a scene description.*

		- ##### 11.2.1. Scene-from-text
			*"Two people sitting across a table, one standing by the window" → mannequins placed and posed. Handles environment generation from available assets. Supports motion descriptions. Text input for v1.*

		- ##### 11.2.2. Blocking refinement
			*"Move the standing actor closer to the table" → incremental text adjustments. Conversational context persists. Ambiguous references prompt for clarification.*

	- ### 11.3. Camera suggestions
		*Given a blocked scene, suggest camera setups for coverage.*

		- ##### 11.3.1. Coverage suggestions
			*Suggest master shot, OTS, close-ups for dialogue scenes. Three-way style toggle: conservative / minimal / comprehensive. User-configurable max shots per generation.*

		- ##### 11.3.2. Shot list generation
			*Generate a full shot list from a scene description or script excerpt. Create shots in sequence.*

- # Phase 12 — Stretch Goals

	- ### 12.1. AI prop generation
		*Generate 3D props from text descriptions. Fills gaps in the asset library.*

		- ##### 12.1.1. Text-to-prop
			*Text prompt generates a 3D model suitable for previs. Low-poly, consistent visual style. Auto-generates collider. Saved to asset library for reuse. Requires internet connection and API key.*

	- ### 12.2. Costume generation
		*Generate 3D clothing meshes on existing mannequin. Clothing-on-mannequin approach — skeleton stays fixed, auto-binding to known skeleton is tractable. See Costume Generation spec.*

		- ##### 12.2.1. AI costume generation
			*Text description → 3D clothing meshes (jacket, pants, hat, etc.) auto-bound to mannequin skeleton. Costume library for reuse (cross-project, cross-character). Body type changes → costume stretches. Supplements the non-AI mannequin customization (6.1.1).*

	- ### 12.3. LiDAR scanning
		*Scan real locations with iPhone/iPad LiDAR and import as scene backgrounds. Requires companion iOS app.*

		- ##### 12.3.1. LiDAR import
			*Import point cloud or mesh data from LiDAR scans (iPhone Pro, iPad Pro). Use as scene backdrop for accurate spatial reference. Not editable — display only.*

		- ##### 12.3.2. Companion iOS app
			*Minimal iOS app for capturing LiDAR scans. Scan capture only.*

	- ### 12.4. Style grading — MAYBE
		*Apply visual styles to rendered frames and sequences. "Film noir," "golden hour," "gritty documentary."*

		- ##### 12.4.1. Frame style grading
			*Apply visual style presets or AI-generated styles to exported frames and video. Preserves camera framing, blocking, and timing.*

	- ### 12.5. AI video generation — MAYBE
		*Generate stylized video from previs sequences using AI. Speculative — depends on future AI capabilities.*

	> **Explicitly NOT building:**
	> - Color grading tools (not previs — that's post-production)
	> - AR overlay (placing 3D assets on real camera feed — requires mobile-first architecture)
	> - Facial scanning / face capture (out of scope for previs)
	> - Full AI character generation (clothing-on-mannequin is the reliable path — see Costume Generation spec)

- # Sequencing principles

	1. **Ship the camera first.** The virtual camera is the product. Everything else supports it. Get camera + overlays + basic scene working before building timeline features.
	2. **Workflow before features.** Undo, save, and export come before lighting, characters, and multi-camera. A tool people can't save their work in doesn't get used.
	3. **Characters are the complexity cliff.** Poseable humanoids with IK, pose libraries, and animation are an order of magnitude harder than everything in Phases 1–4. Don't underestimate this.
	4. **AI features are speculative.** Phase 11 depends on LLM capabilities that may or may not work well enough. Build the tool so it's great without AI — AI is upside, not the product.
	5. **Don't build a 3D editor.** Every feature should pass the test: "does a director need this to plan a shot?" If the answer is "only a 3D artist would use this," cut it.
	6. **Multi-camera is a capstone.** Get single-camera workflow polished before introducing multi-cam complexity. Coverage splitting builds on mature keyframe and sequencer foundations.
	7. **Camera rigs depend on characters.** Camera follow, look-at, and snorricam all require animated characters to be meaningful. They belong after 6.1, not before.
