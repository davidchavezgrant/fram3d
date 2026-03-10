# Fram3d Product Roadmap

**Date**: 2026-03-10
**Status**: Complete

---

Fram3d is a 3D previsualization tool for filmmakers. Cinematic language over 3D complexity — focal length, dolly, crane — not Maya jargon.

---

- # 1. Core previsualization tool (Project)
	*Build the end-to-end previs loop: move a camera, frame a shot, animate it, play it back. One scene, one camera, no persistence.*

	- ### 1.1. Virtual camera (Milestone)
		*Physically-based camera rig with movement, lens, focus, and shake — controlled entirely by mouse + modifier keys.*

		- ##### 1.1.1. Camera movement (Feature)
			*Pan, tilt, dolly, truck, crane, roll, orbit, dolly zoom, reset. All speeds configurable.*

		- ##### 1.1.2. Lens system (Feature)
			*Focal length 14–400mm, physically accurate FOV from real sensor dimensions, presets, smooth transitions.*

		- ##### 1.1.3. Camera body and lens presets (Feature)
			*Camera bodies: ARRI Alexa Mini LF, ARRI Alexa 35, RED V-Raptor, Canon C300 Mark III, Canon C70, Sony FX6, Sony FX3, Generic 35mm, Super 35mm, 16mm, Super 16mm, 8mm. Lens presets: Zeiss Master Prime, Cooke S4/i, Leica Summilux-C, Sigma Cine FF, Generic Prime.*

		- ##### 1.1.4. Focus (Feature)
			*Animated focus-on-object — smooth transition, calculates optimal distance from bounds and FOV, frames with breathing room.*

		- ##### 1.1.5. Depth of field preview (Feature)
			*Visualize shallow/deep DOF based on focal length, aperture, and focus distance. Cinematic bokeh preview — not photorealistic, just enough to see what's sharp vs. soft.*

		- ##### 1.1.6. Camera shake (Feature)
			*Procedural handheld effect. Perlin noise, configurable amplitude/frequency, X/Y rotation only.*

		- ##### 1.1.7. Camera info HUD (Feature)
			*Overlay showing focal length, height, angle of view, aspect ratio, body, and lens preset.*

	- ### 1.2. Camera overlays (Milestone)
		*Composition aids that sit on top of the 3D viewport — aspect ratio masks and frame guides.*

		- ##### 1.2.1. Aspect ratio masks (Feature)
			*Letterbox/pillarbox bars. 8 ratios: Full Screen, 16:9, 16:10, 1.85:1, 2.35:1, 2.39:1, 2:1, 4:3. Default 16:9.*

		- ##### 1.2.2. Frame guides (Feature)
			*Rule of thirds, center cross, safe zones (title 90%, action 93%). All start hidden, toggled independently.*

	- ### 1.3. Scene management (Milestone)
		*Click objects, move them around, see visual feedback. Single-selection with translate/rotate/scale gizmos.*

		- ##### 1.3.1. Scene elements (Feature)
			*Any object with a collider is interactive. Hover highlighting, selection highlighting, click-to-select, click-empty-to-deselect.*

		- ##### 1.3.2. Transform gizmos (Feature)
			*Translate (arrows), rotate (rings), scale (cubes). Axis-colored (RGB = XYZ), always-on-top, constant screen size.*

		- ##### 1.3.3. Ground plane (Feature)
			*Infinite ground plane with visible grid for spatial reference. Gives context for camera height and blocking distances.*

		- ##### 1.3.4. Object duplication (Feature)
			*Ctrl+D to duplicate selected object. Copy inherits position with slight offset.*

	- ### 1.4. Shot sequencer (Milestone)
		*Horizontal strip of shot thumbnails. Add, delete, reorder, select shots. Each shot is an independent camera animation.*

		- ##### 1.4.1. Shot model (Feature)
			*Shot = name + duration + focal length + camera animation + object animations. Auto-named Shot_01, Shot_02, etc.*

		- ##### 1.4.2. Sequencer UI (Feature)
			*Scrollable thumbnail strip with shot name, editable duration, drag-and-drop reordering, add/delete buttons.*

		- ##### 1.4.3. Object continuity (Feature)
			*New shots inherit current object transforms as initial keyframes. Backward navigation restores non-animated objects to initial state.*

	- ### 1.5. Keyframe animation (Milestone)
		*Multi-track timeline editor for camera and object animation within each shot. Playback at real-time.*

		- ##### 1.5.1. Timeline editor (Feature)
			*Three regions: transport bar, timeline area with tracks and playhead, status bar with keyboard hints.*

		- ##### 1.5.2. Tracks and keyframes (Feature)
			*Camera track (yellow) always present. Object tracks (green) per animated object. Camera keyframes: position, rotation, focal length. Object keyframes: position, rotation, scale.*

		- ##### 1.5.3. Keyframe interaction (Feature)
			*Click to select, drag to reposition (snaps to 0.1s), click timeline to scrub, delete selected. Minimum 1 camera keyframe enforced.*

		- ##### 1.5.4. Interpolation and playback (Feature)
			*Smooth interpolation between keyframes. Real-time playback evaluating all tracks each frame. Stops at shot end.*

		- ##### 1.5.5. Auto-keyframing (Feature)
			*Automatically creates/updates keyframes when the user manually moves the camera or objects. Near existing keyframe (0.1s): update. Otherwise: create new.*

		- ##### 1.5.6. Camera path visualization (Feature)
			*Render the camera's dolly track as a spline in 3D space. Shows keyframe positions as nodes along the path. Toggleable.*

	- ### 1.6. Input system (Milestone)
		*Mouse + keyboard control scheme. Modifier keys for camera movements, letter keys for tools and toggles.*

		- ##### 1.6.1. Mouse controls (Feature)
			*Scroll Y + modifiers for zoom/dolly/crane/roll/dolly-zoom. Scroll X + Cmd for truck. Ctrl-drag for pan/tilt. Alt-drag for orbit.*

		- ##### 1.6.2. Keyboard shortcuts (Feature)
			*Space: play. QWER: tool modes. F: focus. C/V: add keyframes. Arrows: scrub. Plus toggles for overlays, guides, shake, presets.*

- # 2. Workflow essentials (Project)
	*Make it usable for real work: undo mistakes, save projects, bring in your own assets, get frames out.*

	- ### 2.1. Undo / Redo (Milestone)
		*Command-based undo stack. Every user action (move, keyframe, shot change) is reversible.*

		- ##### 2.1.1. Undo stack (Feature)
			*Cmd+Z / Cmd+Shift+Z. Stack-based history of all user actions.*

		- ##### 2.1.2. Command pattern completion (Feature)
			*Wire existing command infrastructure (already scaffolded with Execute/Undo/Redo) into a working undo manager.*

	- ### 2.2. Save / Load (Milestone)
		*Project persistence. Save scene state, shots, animations, and settings to disk. Reopen and continue.*

		- ##### 2.2.1. Project file format (Feature)
			*Serialize full project state: scene objects, shot sequence, all keyframe data, camera settings, overlay state.*

		- ##### 2.2.2. Save / Load UI (Feature)
			*Save, Save As, Open, Recent Projects. Auto-save on interval.*

		- ##### 2.2.3. Dirty state tracking (Feature)
			*Track unsaved changes. Warn before closing with unsaved work.*

	- ### 2.3. Asset import (Milestone)
		*Bring in 3D models from external tools. Drag-and-drop into the scene.*

		- ##### 2.3.1. Model import (Feature)
			*Drag-and-drop FBX, OBJ, glTF files into the viewport. Auto-generate colliders for selection/gizmos.*

		- ##### 2.3.2. Asset library (Feature)
			*Panel showing imported assets for reuse across shots. Thumbnail previews.*

	- ### 2.4. Export (Milestone)
		*Get work out of Fram3d — stills for storyboards, video for sharing with crew.*

		- ##### 2.4.1. Image export (Feature)
			*Export current frame as PNG/JPEG at the active aspect ratio. Optionally include overlays.*

		- ##### 2.4.2. Video export (Feature)
			*Render shot or full sequence to video. Configurable resolution and frame rate.*

		- ##### 2.4.3. Storyboard export (Feature)
			*Export all shots as a grid/contact sheet with shot names and durations. PDF or image.*

		- ##### 2.4.4. NLE export (Feature)
			*Export to Adobe Media Encoder or Premiere Pro. Likely via MCP integration — send timeline data to Premiere's API.*

- # 3. Production features (Project)
	*Graduate from a camera tool to a full previs suite. Lighting, characters, multi-camera, and the scene management to support complex setups.*

	- ### 3.1. Lighting (Milestone)
		*Place, aim, and adjust lights to previsualize mood and contrast. Not a rendering tool — a blocking tool for DPs.*

		- ##### 3.1.1. Light types (Feature)
			*Directional (sun), point, spot. Position and aim with the same gizmo system as objects.*

		- ##### 3.1.2. Light properties (Feature)
			*Intensity, color, range, cone angle (spot). Adjustable via inspector or direct manipulation.*

		- ##### 3.1.3. Light animation (Feature)
			*Keyframe light properties on the timeline. Animate intensity, color, position for lighting cues.*

	- ### 3.2. Characters / Actors (Milestone)
		*Poseable humanoid figures for blocking scenes. The core differentiator from ShotDesigner (2D) and the complexity leap from simple objects.*

		- ##### 3.2.1. Mannequin placement (Feature)
			*Drop poseable humanoid mannequins into the scene. Drag to position like any other object.*

		- ##### 3.2.2. Pose library (Feature)
			*Preset poses: standing, sitting, walking, running, pointing, conversation. Apply with one click.*

		- ##### 3.2.3. Custom posing (Feature)
			*Manipulate individual joints — head, arms, torso, legs. IK or FK.*

		- ##### 3.2.4. Character animation (Feature)
			*Keyframe character poses on the timeline. Interpolate between poses for blocking animation.*

	- ### 3.3. Multi-camera (Milestone)
		*Multiple cameras in the same scene. Switch between angles, compare framings, build a multi-cam sequence.*

		- ##### 3.3.1. Camera creation (Feature)
			*Add/remove cameras. Each has independent position, lens, and animation. Named Camera_A, Camera_B, etc.*

		- ##### 3.3.2. Camera switching (Feature)
			*Assign cameras to shots. View through any camera. Quick-switch for comparing framings.*

		- ##### 3.3.3. Multi-cam preview (Feature)
			*Split-screen or picture-in-picture view of multiple cameras simultaneously.*

	- ### 3.4. Scene hierarchy (Milestone)
		*Parent/child relationships between objects. Group objects, attach props to characters, build compound setups.*

		- ##### 3.4.1. Parenting (Feature)
			*Drag object onto another to parent. Child inherits parent transforms. Unparent to detach.*

		- ##### 3.4.2. Hierarchy panel (Feature)
			*Tree view of all scene objects showing parent/child relationships. Click to select, drag to reparent.*

	- ### 3.5. Selection and manipulation refinements (Milestone)
		*Quality-of-life improvements to object interaction as scenes get more complex.*

		- ##### 3.5.1. Multi-select (Feature)
			*Shift-click to add to selection. Drag-select (marquee). Move/rotate/scale multiple objects together.*

		- ##### 3.5.2. Grid snapping (Feature)
			*Snap objects to configurable grid. Hold modifier to override. Visual grid overlay.*

		- ##### 3.5.3. Custom interpolation curves (Feature)
			*Per-keyframe easing: linear, ease-in, ease-out, ease-in-out, bezier. Curve editor UI.*

- # 4. AI-assisted previsualization (Project)
	*Use language models to accelerate previs work — describe what you want, get a starting point to refine.*

	- ### 4.1. Natural language shot description (Milestone)
		*Describe a shot in words, get camera position and framing as a starting point.*

		- ##### 4.1.1. Shot-from-text (Feature)
			*"Medium close-up of the actor at the table, slightly low angle" → camera positioned accordingly.*

		- ##### 4.1.2. Shot vocabulary (Feature)
			*Understands cinematic language: shot sizes (ECU, CU, MCU, MS, MLS, LS, ELS), angles (low, high, dutch, eye-level, bird's eye, worm's eye), movement types.*

	- ### 4.2. Automatic blocking (Milestone)
		*Position actors and objects from a scene description.*

		- ##### 4.2.1. Scene-from-text (Feature)
			*"Two people sitting across a table, one standing by the window" → mannequins placed and posed.*

		- ##### 4.2.2. Blocking refinement (Feature)
			*"Move the standing actor closer to the table" → incremental adjustments by voice/text.*

	- ### 4.3. Camera suggestions (Milestone)
		*Given a blocked scene, suggest camera setups for coverage.*

		- ##### 4.3.1. Coverage suggestions (Feature)
			*Suggest master shot, over-the-shoulder, close-ups for a dialogue scene. Generate as separate shots in the sequencer.*

		- ##### 4.3.2. Shot list generation (Feature)
			*Generate a full shot list from a scene description or script excerpt. Create shots in sequence.*

- # Sequencing principles

	1. **Ship the camera first.** The virtual camera is the product. Everything else supports it. Get camera + overlays + basic scene working before building timeline features.
	2. **Workflow before features.** Undo, save, and export come before lighting, characters, and multi-camera. A tool people can't save their work in doesn't get used.
	3. **Characters are the complexity cliff.** Poseable humanoids with IK, pose libraries, and animation are an order of magnitude harder than everything in Project 1. Don't underestimate this.
	4. **AI features are speculative.** Project 4 depends on LLM capabilities that may or may not work well enough. Build the tool so it's great without AI — AI is upside, not the product.
	5. **Don't build a 3D editor.** Every feature should pass the test: "does a director need this to plan a shot?" If the answer is "only a 3D artist would use this," cut it.
