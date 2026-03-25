# Interaction Patterns

UI/UX interaction rules, input mappings, and behavioral specifications.

---

## Input Mappings

**Mouse (scene) — scroll:**
- Scroll Y = focal length, +Ctrl = dolly, +Alt = crane, +Cmd+Alt = dolly zoom
- Scroll X+Ctrl = truck, +Shift = roll

**Mouse (scene) — drag (Unity-style):**
- Alt+Left-drag = orbit (around selected or world origin)
- Middle-drag = pan/tilt
- Alt+Right-drag = dolly

**Mouse (timeline):**
- Scroll = zoom at cursor position
- Shift+Scroll = pan horizontally
- Middle-click drag = pan in track area and shot bar

**Keyboard (scene):**
- Space = play/pause
- QWER = active tools (Select, Translate, Rotate, Scale)
- F = focus
- 1-9 = focal length presets
- C = camera keyframe, V = element keyframe
- Arrows = scrub 1 frame
- Delete = context-sensitive delete
- Ctrl+D = duplicate, Ctrl+R = reset camera
- Cmd+Z / Cmd+Shift+Z = undo/redo

**Keyboard (overlays):**
- A/Shift+A = aspect ratio cycle
- G = toggle all composition guides on/off
- Shift+G = toggle rule of thirds
- Ctrl+G = toggle center cross
- Alt+G = toggle safe zones
- H = camera info
- P = camera path
- D = toggle Director View
- Shift+D = toggle DOF

**Keyboard (panels):**
- O = Elements panel
- T = timeline
- Tab = toggle all panels
- Home = start, End = end
- \\ = zoom to fit

**Keyboard (multi-camera):**
- Shift+1/2/3/4 = switch active camera

---

## Keyframe Interaction Rules

- Moving a main keyframe moves all child property keyframes
- Moving a child keyframe creates a new main keyframe at the target time containing only that property; the original slot empties
- Deleting a main keyframe deletes all children
- Deleting all children deletes the main keyframe
- Dragging onto an existing keyframe silently merges
- Snap to frame boundaries during drag (1/fps — e.g., 1/24s at 24fps)

## Keyframe Interpolation Shapes (AE-style)

- Three shapes: diamond (linear), circle (smooth), square (hold)
- Alt+click a keyframe to cycle through shapes
- Default heuristic: camera keyframes → smooth, single-keyframe elements → hold, multi-keyframe elements → linear
- Between-keyframe curve indicators on expanded sub-tracks: ─ linear, ⌒ ease-in, ⌓ ease-out, ~ ease-in-out, ∿ bezier

## Live Interpolated Values

- When a track is expanded, sub-track labels show the real-time interpolated value at the playhead position (e.g., `Position (1.2, 0.9, -1.5)`)

---

## Shot Bar Interaction

- Single-click a camera row = preview that camera (dimmed display, non-destructive)
- Double-click a camera row = activate that camera AND zoom to shot (8% padding)
- Shot bar auto-adjusts row height based on max camera count across shots

## Boundary Drag (Ripple Editing)

- Dragging a shot boundary shifts all downstream content: shots, camera keyframes, angle segments, and linked periods
- Hold Shift = shots only (element keyframes stay in place)
- Snaps to frame boundaries
- Resize tooltip shows shot name, duration (seconds + frames), and ripple mode

---

## Timecode Display

- Transport bar: shot-local elapsed / duration
- View overlay: sequence-global timecode
- Format: semicolon-separated `HH;MM;SS;FF`

## Playback Auto-scroll

- During playback, if the playhead exits the visible range, the view shifts to follow while maintaining zoom level

---

## View Layout System

- Three layouts: single, side-by-side, three-view (top-wide + two bottom)
- Camera View is a single movable instance — only one exists
- When reassigning Camera View to a new view, the old view receives the displaced view type (smart swap)
- Camera View must always exist in exactly one view

## Panel System (Gutters)

- JetBrains-style vertical label strips on left/right workspace edges
- Left gutter: Overview toggle
- Right gutter: Elements, Assets toggles
- Mutual exclusion: opening Elements closes Assets and vice versa
- Tab key toggles all panels simultaneously

## Timeline and Panel Resize

- Vertical drag handle between the view area and timeline. Min 80px, max 80vh.
- Side panels have horizontal drag edges. Min 150px, max 500px.

---

## Properties Panel

- Contextual sidebar showing editable properties for the selected element
- Content changes based on selection type:
  - Element: name, position, rotation, scale
  - Character: all element properties + body type, height, build, tint, pose, expression, eye direction
  - Camera: focal length, aperture, focus distance, camera body, lens set, shake settings
  - Light: intensity, color (RGB + Kelvin), range, cone angle (spot), inner cone angle (spot)
  - Link: anchor point XYZ (visible only when element has been linked at least once)
- Dockable to left or right side of the workspace
- Appears when an element is selected, collapses when nothing is selected
- Keyboard shortcut: I (for "info" — unused key)

---

## Element Linking Constraints

- Max chain depth: 4
