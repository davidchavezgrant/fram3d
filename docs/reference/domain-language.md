# Fram3d Domain Language

Canonical terminology for Fram3d. Use these terms in specs, UI, and conversation.

---

# Part 1: Definitions

## The Hierarchy

**Project > Scene > Shot > Angle**

| Term | Definition |
|------|-----------|
| **Project** | Top-level container. One file. Contains scenes, character definitions, and project-wide settings (camera body, lens set, frame rate). |
| **Scene** | A self-contained 3D world with its own elements, lighting, environment, and shots. Multiple scenes per project. |
| **Shot** | A named time range on the shot track. Contains one or more cameras, each providing an angle. Default naming: `Shot 01`, `Shot 02`, etc. |
| **Angle** | One camera's perspective within a shot. Contains a camera animation (or not), a framing, and a lens. In multi-camera, each camera provides a distinct angle. |

## Assets & Elements

**Asset**: anything in the library. **Element**: anything in the scene. Type names are the same in both contexts.

### Asset types (in the library)

| Type | What it is |
|------|-----------|
| **Object** | A 3D model. Tables, chairs, buildings, vehicles. |
| **Character** | A poseable humanoid with skeleton, expressions, and pose library. |
| **Light** | A light source — directional, point, or spot. |
| **Costume** | Clothing or accessories for a character. |
| **Animation** | Recorded motion data for an element. |
| **Expression** | A facial state for a character. |
| **Environment** | A complete scene — walls, floors, objects, lighting. |

### How assets enter the scene

| Verb | What happens | Asset types |
|------|-------------|-------------|
| **Place** | Creates a new element in the scene with a position, rotation, and scale. | Objects, characters, lights |
| **Apply** | Modifies an existing element. No new element created. | Costumes → character appearance. Animations → element's track. Expressions → character's face. |
| **Load** | Fills a scene with contents — elements, lighting, everything. | Environments |

### Element types (in the scene)

| Type | What it is |
|------|-----------|
| **Object** | A placed 3D model. Inert until animated. |
| **Character** | A placed poseable humanoid. |
| **Light** | A placed light source. |
| **Camera** | A virtual camera. Created when you add an angle to a shot — not placed from the library. |

## The Timeline

| Term | Definition |
|------|-----------|
| **Timeline** | Where you edit animation. Contains tracks, the time ruler, and the playhead. |
| **Track** | A horizontal lane in the timeline representing one entity or structural concept. |
| **Sub-track** | A child lane within a track for a single animatable property (position, rotation, focal length, etc.). |
| **Time ruler** | Horizontal scale showing seconds and subdivisions. |
| **Playhead** | Vertical red line showing current time, spanning all tracks. |
| **Cut** | The boundary between two shots on the shot track, or between two angles on the active angle track. Draggable. Snaps to frame boundaries. |
| **Transport controls** | Play/pause, current time, shot name. Adjacent to the timeline but not part of it. |

### Track types

| Track | What it shows | Color |
|-------|--------------|-------|
| **Shot track** | Shots arranged in sequence with thumbnail, name, and duration. Always the topmost track. | — |
| **Camera track** | Camera animation within the selected shot (one per angle). | Yellow |
| **Object track** | Object animation on the global timeline. | Green |
| **Light track** | Light animation. | Amber |
| **Active angle track** | Which angle is on air at each moment (multi-camera only). | Per-camera color |

## Views & Layout

| Term | Definition |
|------|-----------|
| **View** | A rendering surface in the workspace. The workspace can hold 1–3 views. |
| **Layout** | The view arrangement: single, side-by-side, or one-large-two-small. |
| **Camera View** | Looking through the shot camera. Where you frame shots and animate. |
| **Director View** | Free utility camera decoupled from the timeline. No keyframes created. |
| **Designer View** | 2D top-down orthographic. Elements shown as icons, cameras as frustum outlines. |

## Camera & Lens

| Term | Definition |
|------|-----------|
| **Camera** | The virtual camera the user looks through and animates. |
| **Camera body** | A real-world camera model with sensor dimensions (e.g., ARRI Alexa 35). Determines FOV for a given focal length. |
| **Sensor** | The physical imaging area (width × height in mm). Part of a camera body. |
| **Focal length** | Distance in mm from lens to sensor. 14–400mm. The primary zoom control. |
| **Lens set** | A named collection of focal lengths (e.g., Cooke S4/i). Contains primes (fixed) or zooms (continuous range). |
| **Aperture / f-stop** | Lens opening. Controls depth of field. f/1.4 through f/22. |
| **Depth of field (DOF)** | Region of apparent sharpness. Controlled by focal length + aperture + focus distance. |
| **Focus distance** | Distance from camera to the plane of sharpest focus. Keyframeable. |
| **Rack focus** | Animating focus distance between subjects over time. |

## Camera Movement

Six degrees of movement. No synonyms.

| Term | What it is | Axis |
|------|-----------|------|
| **Pan** | Horizontal rotation (yaw) | Vertical axis. Positive = rightward. |
| **Tilt** | Vertical rotation (pitch) | Horizontal axis. Positive = upward. |
| **Roll** | Rotation around the lens axis | Forward axis. Positive = clockwise from camera POV. |
| **Dolly** | Translation forward/backward | Camera's local Z. "Dolly in" = toward subject. |
| **Truck** | Translation left/right | Camera's local X. "Truck right" = rightward. |
| **Crane** | Translation up/down | World Y. Positive = upward. |

## Camera Behaviors

| Term | Definition |
|------|-----------|
| **Follow** | Camera moves with a target. Position derived from target + offsets (distance, height, lateral). |
| **Watch** | Camera stays in place, rotates to face a target. |
| **Snorricam** | Camera mounted to a character's body. Front mount (facing the character) or back mount (facing away). |
| **Damping** | How tightly the camera follows or watches its target. 0 = rigid, 1 = loose. |
| **Follow path** | Visible 3D spline showing the camera's computed follow trajectory. |
| **Orbit** | Camera rotation around a target point while keeping it centered. |
| **Camera shake** | Procedural noise-based rotation simulating handheld. Pan/tilt only. Cosmetic during editing, baked on export. |
| **Dolly zoom** | Simultaneous focal length change + translation to maintain subject size while changing perspective distortion. |

## Keyframes & Animation

| Term | Definition |
|------|-----------|
| **Keyframe** | A recorded set of property values at a specific point in time. Diamond on collapsed tracks, circle on sub-tracks. |
| **Recording** | When a track's stopwatch is activated. Manipulations create or update keyframes automatically. |
| **Stopwatch** | Per-track toggle that controls whether the track is recording. |
| **Animate** (verb) | To create animation by recording keyframes over time. |
| **Scrub** | Dragging the playhead to preview the scene state at different times. |
| **Interpolation** | How values transition between keyframes: linear, ease in, ease out, ease in-out, bezier. |
| **Bake** | Convert procedural effects (shake, follow, snorricam) into explicit keyframes. Happens during export. |
| **Timecode** | Time display format. Non-drop-frame for 24/25/30fps, drop-frame for 29.97/59.94. |
| **Frame rate / fps** | Frames per second. Default 24. |
| **Speed factor** | Per-shot playback multiplier. 1.0 = normal, 0.5 = slow motion, 2.0 = double speed. |

## Characters & Posing

| Term | Definition |
|------|-----------|
| **Character** | A named, poseable humanoid element. The only term for this concept. |
| **Mannequin** | The uncustomized starting humanoid mesh. Once named and styled, it's a character. |
| **Pose** | A character's body configuration. From the pose library or custom joint manipulation. |
| **Joint** | Individual articulation point — head, arm, torso, leg. Joints are connected: move a hand and the arm follows naturally. |
| **Expression** | Facial state from blend shape weights. 10 presets: neutral, happy, sad, angry, surprised, concerned, scared, disgusted, thinking, smirk. |
| **Blend shape** | Mesh deformation target for facial animation. |
| **Skeleton** | Bone hierarchy inside a character. |
| **Rig** | Skeleton + mesh binding. A rigged model can be posed and animated. |
| **Body region** | Area on a character mesh defined by bones (hand, head, torso). Used for attaching objects. |
| **Blocking** | Placing characters and objects in positions for a scene. Filmmaking rehearsal term. |
| **Walk cycle** | Animated locomotion with customizable gait (limping, sneaking). |

## Selection & Interaction

| Term | Definition |
|------|-----------|
| **Selected** | The user has clicked it. It's highlighted. Its properties/tracks are shown for editing. |
| **Active** | The system is using it to drive output. |
| **Select** | Click an element to make it the focus of editing. Highlighted in cyan. |
| **Deselect** | Click empty space to clear the selection. |
| **Hover** | Cursor over an element, not yet clicked. Yellow highlight. |
| **Multi-select** | Shift-click to add/remove elements from selection. |
| **Marquee** | Drag a rectangle to select multiple elements. Partial intersection counts. |
| **Deep select** | Cmd/Ctrl+Click to select a group member without entering the group. |

## Manipulation

| Term | Definition |
|------|-----------|
| **Gizmo** | Visual manipulation handles — arrows (translate), rings (rotate), box (scale). |
| **Translate** | Move. Arrow handles, axis-constrained. W key. |
| **Rotate** | Rotate. Ring handles, axis-constrained. E key. |
| **Scale** | Resize. Uniform only. R key. |
| **Transform** | Position + rotation + scale. The spatial state of an element. |
| **Active tool** | The current manipulation tool: Select (Q), Translate (W), Rotate (E), Scale (R). |

## Object Relationships

| Term | Definition |
|------|-----------|
| **Linking** | Temporal parent-child relationship on the global timeline. A child follows its parent's transform for a time period. The scene graph stays flat — linking is data, not hierarchy. |
| **Linked period** | Time range when a link is active. Greyed out on the timeline. |
| **Anchor point** | XYZ offset defining where an element attaches to a parent. |
| **Link tool** | UI tool for creating links — drag from child to parent in the Elements panel. |
| **Grouping** | Persistent collection of elements with a shared transform. Structural, not temporal. Click selects the group; double-click enters it. |

## Overlays

| Term | Definition |
|------|-----------|
| **Overlay** | Any visual element rendered on top of the Camera View. |
| **Aspect ratio mask** | Black bars (letterbox/pillarbox) defining the deliverable image boundary. |
| **Composition guide** | Lines that aid composition: rule of thirds, center cross, safe zones. |
| **Camera info** | Text overlay showing camera metadata: focal length, height, angle of view, etc. |
| **Subtitle** | Text at bottom-center of the image. Per-shot, timeable, stackable. |

## Multi-Camera

| Term | Definition |
|------|-----------|
| **Multi-camera** | A shot with 2–4 cameras (A, B, C, D). Camera A always exists. |
| **Angle** | One camera's perspective within a shot. Each camera provides its own angle. |
| **Active angle track** | Timeline track showing which angle is on air at each moment. Colored segments per camera. |
| **Active camera** | The camera whose angle is used during playback. |
| **Selected camera** | The camera whose track is visible for editing. Can differ from the active camera. |

## Frame vs Framing

| Term | Definition |
|------|-----------|
| **Frame** | A single rendered image at a point in time. Always temporal: frame 1, frame 47, frame rate. |
| **Framing** | How subjects are composed within the image. Always compositional: "tighten the framing," "medium close-up framing." |

## UI

| Term | Definition |
|------|-----------|
| **Elements panel** | List of all elements in the scene. Dockable sidebar. |
| **Assets panel** | Library of available assets. Dockable sidebar. |
| **Gutter** | Narrow strip at the edge of the workspace with tab buttons for opening panels. |
| **Overview** | Collapsible bird's-eye view of the full timeline across all shots. |
| **Burn-in** | Shot name rendered as text overlay on exported frames. |

---

# Part 2: Rationale & Rules

Why certain terms were chosen, what to avoid, and how to handle ambiguity.

## Why Shot and Angle are different things

On a film set: *"We need to cover this from two angles — a wide master and an OTS on the lead."* The shot is the scene being captured. Each camera's perspective is an angle.

In Fram3d, the shot track shows shots in sequence. When you select a shot, the tracks below show each camera's angle — a specific animation, framing, and lens configuration.

In single-camera (most of the time), this collapses: one shot, one camera, one angle. Users don't need to think about the distinction until they add a second camera. But the domain language is ready for it.

## Why Frame and Framing are different words

"Frame" is temporal. "Framing" is compositional. Never use "frame" alone to mean the compositional boundary. This is why we say "composition guide" instead of "frame guide" — the word "frame" in "frame guide" was compositional, violating the rule.

## Why Selected and Active are different states

"Selected" is about user focus. "Active" is about system output. You can select Camera B to edit its keyframes while Camera A is active for playback. In single-camera, they're always the same. In multi-camera, they can differ. Getting this wrong in UI will generate endless confusion.

## Asset vs Element: two worlds

The library holds assets. The scene holds elements. Type names are the same in both (an object is an object in the library or in the scene) — context disambiguates. "Element" is the only word that means "any scene thing." Don't say "object" when you mean "element."

"Prop" is informal — it means an object with a narrative role. Fine in conversation. Not a formal type.

## Character, not Actor, not Mannequin

- **Character** is the only formal term for a named poseable humanoid. Use everywhere.
- **Mannequin** only when discussing the uncustomized base mesh before it has a name.
- **Actor**: don't use. Collides with software patterns and creates ambiguity.

## Recording, not modes

"Mode" is overloaded and vague. Fram3d has no modes. Instead:

| Instead of | Say |
|-----------|-----|
| Animate mode | **Recording** — the track is recording. |
| Setup mode | **Not recording** — manipulations are direct. |
| Director Mode | **Director View** |
| Gizmo mode | **Active tool** — name the tool. |
| View mode | Name the view: Camera View, Director View, Designer View. |
| Slate mode | **Burn-in** |

## Track is never a verb

"Track" always means a lane in the timeline. When talking about camera following, say "follow." "The camera follows the character," not "the camera tracks the character."

## The shot track is structural

The shot track shows the sequence of shots. Selecting a shot determines which camera, object, and light tracks appear below. Other tracks show animation data; the shot track shows structure.

## Views, not viewports

Every rendering surface is a "view." "Viewport" is retired — calling one view "the viewport" implies the others aren't. "Panel" is reserved for sidebars (Assets, Elements).

## Always qualify these words

- **Preset**: Always say what kind — lens preset, pose preset, camera body preset. Never just "preset."
- **Snap**: Always say the target — grid snap, rotation snap, time snap.
- **Offset**: Always say what's being offset — follow offset, mount offset, anchor offset.

---

# Part 3: Retired Terms

| Don't use | Use instead | Why |
|-----------|------------|-----|
| actor | **character** | Ambiguous with software patterns. |
| mannequin | **character** (unless discussing uncustomized base mesh) | One concept, one name. |
| pedestal (camera move) | **crane** | One term for vertical translation. |
| director view / Director Mode | **Director View** | No modes. Capitalize consistently. |
| Animate mode | **recording** | Describe the state, not a mode. |
| Setup mode | **not recording** | Same. |
| auto-keyframe | **recording** | Not a separate concept. |
| gizmo mode | **active tool** | Name the tool. |
| view mode | name the view directly | Camera View, Director View, Designer View. |
| viewport | **view** | Every rendering surface is a view. |
| panel (for rendering surfaces) | **view** | "Panel" is for sidebars. |
| object list panel / hierarchy panel | **Elements panel** | One name for one thing. |
| asset library (as panel name) | **Assets panel** | Consistent naming. |
| sequencer | **shot track** | It's a track in the timeline. |
| coverage / coverage track | **active angle track** | Use our own angle terminology. |
| pickwhip | **link tool** | Self-explanatory. |
| look-at / camera aim | **watch** | Plain English, cinematic. |
| IK / inverse kinematics | — | Describe the behavior, not the algorithm. |
| HUD | **camera info** or **overlay** | "HUD" is ambiguous. Be specific. |
| division point | **cut** | Filmmaking language. |
| minimap | **overview** | More descriptive. |
| inspector | — | Not a Fram3d concept. |
| shot card | — | Shots live on the shot track. |
| frame guide | **composition guide** | "Frame" is temporal only. |
| timeline area | **the timeline** | No recursion. |
| hard cut | **cut** | Only one kind. |
| slate mode | **burn-in** | No modes. |
| Kinemachine | **Fram3d** | Former name. |
| Vismatic Studio | **Fram3d** | Former name. |

---

# Part 4: Reference

## Filmmaking Shot Language

Standard cinematography vocabulary. Industry terms, not Fram3d inventions.

**Sizes** (close → far): ECU (extreme close-up) → CU → MCU (medium close-up) → MS (medium shot) → MLS (medium long / cowboy) → LS (long shot) → ELS (extreme long shot)

**Angles**: eye level, low angle, high angle, bird's eye, worm's eye, dutch angle

**Compositions**: OTS (over-the-shoulder), two-shot, single, insert, establishing shot, master shot

**Spatial**: camera left/right, profile (90°), three-quarter (~45°)

## Acronyms & Uncommon Terms

| Term | Definition |
|------|-----------|
| **DOF** | Depth of field. |
| **AOV** | Angle of view (horizontal, in degrees). |
| **EDL** | Edit Decision List (CMX 3600 format). Timeline interchange for NLE import. |
| **NLE** | Non-linear editor (Premiere, Resolve, Avid). |
| **FBX / OBJ / glTF / GLB** | 3D model file formats supported for import. |
| **Anamorphic** | Lens type with a squeeze factor (1.33x–2x) producing a wider aspect ratio from the same sensor. |
| **Squeeze factor** | The horizontal compression ratio of an anamorphic lens. |
| **Super 35 / Full-frame / Large format** | Sensor size categories. |

---

*Last updated: 2026-03-12*
