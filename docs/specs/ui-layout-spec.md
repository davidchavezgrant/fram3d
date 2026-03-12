# Fram3d UI Layout & Interaction Specification

**Date**: 2026-03-12
**Status**: Draft (derived from HTML/CSS/JS mockup, ~90% complete)

---

## Overview

This document specifies the Fram3d workspace layout, visual design, interaction patterns, and keyboard shortcuts. It describes the UI as implemented in the `ui-mockup/` prototype and serves as the canonical reference for building the production UI in Unity.

The workspace is a single-window application with a dark Premiere-style theme. The layout is a vertical stack: a view area on top, a timeline section on the bottom, separated by a draggable resize handle. Side panels dock on the right; an overview strip sits above the view area.

All terminology follows `docs/reference/domain-language.md`. See the retired terms table there for words this spec deliberately avoids.

---

## 1. Global Layout

The workspace fills the full browser/application window. No scrolling on the page itself — all scrolling is contained within individual regions.

### 1.1 Vertical Stack

From top to bottom:

| Region | Height | Description |
|--------|--------|-------------|
| **Overview** | 48px fixed | Bird's-eye view of the full timeline. Collapsible. |
| **View area** | Flex (fills remaining) | 1–3 views showing Camera View, Director View, or Designer View. |
| **Resize handle** | 5px | Draggable to resize the timeline section. |
| **Timeline section** | 320px default | Contains the shot track, active angle track, tracks, ruler, transport, and zoom bar. Min 80px, max 80vh. |

### 1.2 Horizontal Structure

The main row (everything above the timeline) is a horizontal flex:

```
[ Left gutter ] [ Center column ] [ Side panel(s) ] [ Right gutter ]
```

- **Left gutter**: Vertical tab for toggling the overview.
- **Center column**: Overview strip + view area, stacked vertically.
- **Side panels**: Elements panel and/or Assets panel (right side, 220px default width).
- **Right gutter**: Vertical tabs for toggling Elements and Assets panels.

### 1.3 Gutter Tabs

Narrow vertical strips at the left and right edges of the workspace. Each contains vertically-oriented text buttons (`writing-mode: vertical-lr`). Right gutter tabs are rotated 180 degrees so text reads top-to-bottom from the right edge.

| Gutter | Tabs |
|--------|------|
| Left | Overview |
| Right | Elements, Assets |

States: default (`#555`), hover (`#aaa` + faint overlay), active (`#ccc` + 2px accent border on interior edge).

---

## 2. View Area

The central workspace region where 3D content is displayed. Black background provides letterbox surround.

### 2.1 Layouts

Three layout modes, selected via the layout chooser (bottom-right corner of the view area):

| Layout | Arrangement | Panel visibility |
|--------|------------|------------------|
| **Single** | One view fills the entire area | Panel 0 only |
| **Side by side** | Two views, equal width, horizontal | Panels 0 and 1 |
| **One + two** | Large view on top, two smaller views below (2x2 grid, top panel spans both columns) | Panels 0, 1, and 2 |

Each panel has a view selector dropdown at the top (22px bar) offering Camera View, Director View, and Designer View. Changing a panel's view type moves the camera rendering surface between panels as needed — there is only one live Camera View instance.

### 2.2 Layout Chooser

Absolutely positioned at bottom-right of the view area. Semi-transparent dark pill containing three small SVG icons representing each layout. The active layout is highlighted.

### 2.3 Camera View

The primary rendering surface. Maintains a 16:9 aspect ratio inside its container, centered with black letterbox bars.

Contains the following overlay elements (listed by z-index, low to high):

| Z | Element | Position | Description |
|---|---------|----------|-------------|
| 5 | Aspect ratio masks | Edges of frame | Opaque black bars (letterbox or pillarbox) |
| 6 | Composition guides | Full frame SVG | Rule of thirds, center cross, safe zones |
| 10 | Shot label | Top-left | Shot number, camera letter, name, duration |
| 10 | Timecode | Bottom-center | Sequence-global timecode (HH;MM;SS;FF) |
| 10 | Camera info | Top-right | Lens, height, AOV, aperture, focus, DOF, ratio, body, lens set |
| 10 | Active tool badge | Bottom-left | Current tool icon, name, keyboard shortcut |
| 10 | Director View badge | Top-center | "DIRECTOR VIEW" warning label |
| 10 | Camera path badge | Bottom-right | "PATH" indicator |

### 2.4 Director View (placeholder)

Shows a placeholder with a film clapper icon, "Director View" label, and hint text: "Free utility camera decoupled from the timeline."

### 2.5 Designer View (placeholder)

Shows a placeholder with a pencil icon, "Designer View" label, and hint text: "Top-down scene layout and blocking."

---

## 3. Camera Overlays

All overlays render inside the Camera View frame, above the 3D content.

### 3.1 Aspect Ratio Masks

Four opaque black `div` elements (top, bottom, left, right) that crop the frame to the selected aspect ratio. The frame itself is always 16:9; masks narrow it further.

Supported ratios (cycled by `A` key, `Shift+A` cycles backward):

| Name | Ratio | Mask direction |
|------|-------|---------------|
| Full Screen | — | No masks (matches window) |
| 16:9 | 1.778 | None (native) |
| 16:10 | 1.6 | Top/bottom (letterbox) |
| 1.85:1 | 1.85 | Top/bottom |
| 2:1 | 2.0 | Top/bottom |
| 2.35:1 | 2.35 | Top/bottom |
| 2.39:1 | 2.39 | Top/bottom |
| 4:3 | 1.333 | Left/right (pillarbox) |
| 1:1 | 1.0 | Left/right |
| 9:16 | 0.5625 | Left/right |

Default: 16:9. Changing the ratio also updates the "Ratio" field in the camera info overlay. See milestone 1.2 spec for the canonical list.

### 3.2 Composition Guides

SVG overlay with four guide types, all initially hidden:

| Guide | Appearance | Opacity |
|-------|-----------|---------|
| **Center cross** | Two short lines at frame center (4% each axis) | 0.25 |
| **Rule of thirds** | Two vertical + two horizontal lines at 33.3%/66.6% | 0.15 |
| **Safe title** | Dashed rectangle at 5% inset | 0.12 |
| **Safe action** | Shorter-dashed rectangle at 3.5% inset | 0.08 |

All guides toggle together with `G` key. Color: white at varying opacity. Stroke width: 1–1.5px.

### 3.3 Camera Info

Semi-transparent dark panel in the top-right corner. Displays camera metadata as key-value rows:

| Field | Example value | Source |
|-------|--------------|--------|
| Lens | 50mm | Interpolated focal length at playhead |
| Height | 1.6m | Y component of interpolated camera position |
| AOV | 39.6 degrees | Computed: `2 * atan(36 / (2 * focal)) * (180/pi)` (full-frame 35mm) |
| Aperture | f/2.8 | Static (not yet animated) |
| Focus | Detective | Focus target name (static) |
| DOF | On | Depth of field toggle (static) |
| Ratio | 2.39:1 | Current aspect ratio selection |
| Body | Generic 35mm | Camera body preset (static) |
| Lens Set | Generic Prime | Lens set preset (static) |

Toggle: `H` key. The Lens, Height, and AOV fields update in real-time as the playhead moves. Other fields are currently static placeholders.

### 3.4 Active Tool Badge

Bottom-left corner. Shows the current manipulation tool:

| Tool | Icon | Label | Key |
|------|------|-------|-----|
| Select | `diamond` | SELECT | Q |
| Translate | `crosshair` | TRANSLATE | W |
| Rotate | `circle-arrow` | ROTATE | E |
| Scale | `hexagon` | SCALE | R |

The icon is blue (`#4a9eff`). The keyboard shortcut appears in a subtle pill.

### 3.5 Director View Badge

Top-center of the frame. Pink-red pill (`#ff6688` on `rgba(255,68,102,0.25)` background). Bold uppercase "DIRECTOR VIEW" with 2px letter spacing. Visible only when Director View is toggled on.

### 3.6 Camera Path Badge

Bottom-right corner. Amber text (`#eebb44`) on dark semi-transparent background. Shows "PATH" with a path icon. Visible only when camera path visualization is toggled on.

---

## 4. Timeline Section

The bottom region of the workspace. Contains all temporal editing UI. Resizable via the drag handle above it.

### 4.1 Component Stack

From top to bottom within the timeline section:

| Component | Height | Description |
|-----------|--------|-------------|
| Scene tabs | 26px | Tab bar for switching between scenes |
| Transport bar | 28px | Play/pause, time display, shot name |
| Ruler | 22px | Time ruler with tick marks and labels |
| Shot track | Variable (20px per camera row) | Shot blocks with camera sub-rows |
| Active angle track | 24px | Which camera is "on air" (multi-camera only) |
| Track area | Flex | Element and camera animation tracks |
| Zoom bar | 18px | Zoom thumb and playhead indicator |

### 4.2 Scene Tabs

One button per scene in the project. The active scene has a bottom border accent (`2px solid #888`). Clicking a tab calls `loadScene()` which resets the playhead to 0, reinitializes all view state, and renders the new scene.

### 4.3 Transport Bar

Single row containing:

- **Play/pause button**: 24x20px. Shows `play-triangle` when stopped, `pause` when playing. The icon turns red (`#ff4466`) during playback.
- **Current time**: Shot-local elapsed time in `HH;MM;SS;FF` format (semicolon-separated, non-drop-frame for 24fps).
- **Divider**: Forward slash.
- **Duration**: Total shot duration in the same format.
- **Shot name**: Truncated with ellipsis if needed.

### 4.4 Time Ruler

A 22px strip showing time ticks and labels. The left 140px is an empty label column (aligns with track labels). The remaining width maps to `[viewStart, viewEnd]`.

Tick intervals adapt to zoom level:

| View duration | Tick interval | Label style |
|---------------|--------------|-------------|
| 0–2s | 1 frame (1/24s) | M;SS;FF |
| 2–5s | 0.5s | M;SS;FF |
| 5–15s | 1s | M;SS |
| 15–40s | 2s | M;SS |
| 40–60s | 5s | M;SS |
| > 60s | 10s | M;SS |

Major ticks: 22px tall. Minor ticks: 10px. All ticks are 1px wide, `#3c3c3c`. Labels at 9px, `#666`, tabular-nums.

A red playhead line (`#ff4466`, 2px wide) spans the ruler at the current time position.

### 4.5 Shot Track

The shot track is the topmost data track. Its label column reads "Shots" in uppercase. The track area shows one colored block per shot, per camera.

#### Shot Blocks

Each shot x camera combination renders as a `.shot-cam` element — a colored rectangle positioned by `timeToX()`.

| Visual state | Condition | Appearance |
|-------------|-----------|------------|
| **Active camera** | This camera is active in the current shot | Bright border, brightness 1.2, high text opacity |
| **Previewed** | Single-clicked but not activated | White text, brightness 0.9 |
| **Dimmed** | Other cameras in the current shot | Brightness 0.65, opacity 0.55 |
| **Inactive** | Cameras in non-current shots | Brightness 0.45, opacity 0.35 |

Label format: `"A: SHOT NAME"` (camera letter + shot name). 9px bold.

#### Shot Bar Height

The shot bar container height adjusts to `(maxCamerasAcrossAllShots * 20 + 2)px`. Single-camera shots: 22px. Multi-camera shots with 2 cameras: 42px.

#### Shot Interactions

| Action | Behavior |
|--------|----------|
| **Single click** (different shot) | Move playhead to shot start |
| **Single click** (same shot, different camera) | Set as previewed camera |
| **Single click** (active camera) | Clear preview |
| **Double click** (within 350ms) | Activate camera, zoom to shot with 8% padding |
| **Middle-click drag** | Pan the view window |
| **Scroll wheel** | Zoom at cursor position |

#### Shot Hover Tooltip

When `feat('hover-thumbnails')` is enabled, hovering a shot block shows a floating tooltip with:
- **Name**: Shot name in bold white
- **Detail**: Duration in seconds and frames, camera letter

#### View Range Indicator

A semi-transparent overlay on the shot bar showing the currently visible time range (`[viewStart, viewEnd]`). Left and right edges marked with subtle white borders.

#### Cut Handles

Between adjacent shots, an 8px-wide invisible drag handle allows boundary dragging. On hover, a 2px white line appears at the boundary position.

**Boundary drag behavior**: Dragging a cut repositions the boundary between two shots. The shot being shortened has a minimum duration of 1 second. All subsequent shots shift in time by the same delta.

- **Default (no modifier)**: Ripple mode — element track keyframes at or after the original boundary also shift. The resize label shows `"SHOT NAME: Xs (Nf) [ripple]"`.
- **Shift held**: Shots-only mode — only shot start/end times and camera keyframes shift; element keyframes stay fixed. The resize label shows `"[shots only]"`.

### 4.6 Active Angle Track

Visible only for multi-camera shots (`feat('active-angle-track')`). Shows which camera's angle is "on air" at each moment within the current shot.

**Label**: "Active Angle" with a hint line "option-click to split" in 7px.

Each angle is a colored segment (`angle-segment`) matching the camera's color. The segment containing the playhead gets `active-segment` styling (brighter, bordered); others get `inactive-segment` (dimmed). Camera letter label appears if segment width exceeds 20px.

#### Angle Interactions

| Action | Behavior |
|--------|----------|
| **Alt+click on segment** | Split at click position (snapped to frame). New segment uses `(camera + 1) % count`. Minimum 0.2s per segment after split. |
| **Drag divider** | Move the boundary between adjacent angle segments. Clamped to 0.2s minimum per segment, snapped to frame grid. |

### 4.7 Track Area

The main animation editing region. Contains track label column (140px, left) and track content area (flex, right).

#### Track Types Rendered

1. **Camera track** (always first): Yellow dot. Shows the display camera's keyframes for the current shot. Non-current shots show grey camera blocks. Label: display camera name (e.g., "Cam A").

2. **Element tracks**: Colored dots (one per animated element). Show keyframes spanning the full global timeline. Label: element name (e.g., "Detective", "Table").

#### Track Labels

28px height per main track. Contains:
- Color dot (8px circle)
- Track name (truncated with ellipsis)
- Expand arrow (if `feat('track-expand')`): 8px caret, rotates 90 degrees when expanded

#### Track Rows

28px height. Background shows shot-colored tints at 8% opacity (`shot-bg`), with the current shot at 18% (`active-shot`). Vertical 1px lines mark shot boundaries (`#rgba(255,255,255,0.06)`).

#### Sub-tracks

When a track is expanded, sub-tracks appear below it:

| Track type | Sub-tracks |
|-----------|-----------|
| Camera | Position, Rotation, Focal Length |
| Element | Position, Rotation, Scale |

Sub-track rows: 22px height, 28px left indent, 9px font, `#666` text. The label shows the property name and current interpolated value in parentheses (e.g., `"Position (1.0, 2.3, -0.5)"`).

Between adjacent keyframes in sub-tracks, an interpolation curve indicator symbol appears (if gap > 30px):

| Symbol | Curve |
|--------|-------|
| `─` | Linear |
| `⌒` | Ease in |
| `⌓` | Ease out |
| `~` | Ease in-out |
| `∿` | Bezier |

### 4.8 Keyframes

Diamond-shaped markers on tracks, 10x10px, rotated 45 degrees. Color matches the track color.

#### Keyframe Shape (Interpolation Type)

Three visual shapes (After Effects style):

| Shape | CSS class | Interpolation | Appearance |
|-------|-----------|--------------|------------|
| Diamond | `kf-linear` | Linear | Sharp diamond (default) |
| Circle | `kf-smooth` | Smooth/Bezier | Rounded diamond (border-radius 50%) |
| Square | `kf-hold` | Hold/Constant | Non-rotated square |

Default interpolation by context:
- Camera keyframes default to `smooth`
- Multi-keyframe element tracks default to `linear`
- Single-keyframe element tracks default to `hold`

#### Keyframe Interactions

| Action | Behavior |
|--------|----------|
| **Click** | Select (white outline). Move playhead to keyframe time. Click again to deselect. |
| **Alt+click** | Cycle interpolation type: linear -> smooth -> hold -> linear |
| **Drag** | Reposition in time (snapped to 0.1s). Camera keyframes clamped to shot boundaries. |

#### Linked Period Markers

Element tracks with linked periods show:
- Diagonal hatched region (45-degree repeating gradient in grey)
- Dashed border (`1px dashed`)
- Link label text ("-> Parent") if region width > 80px
- Tall narrow bar markers (8x14px, grey) at period start and end

### 4.9 Playhead

A 2px-wide red line (`#ff4466`) spanning the full height of the track area. A downward-pointing triangle (12x12px) sits above it. The playhead also renders in the ruler, shot bar, active angle track, and overview — each as a separate element positioned at the same time.

**Playhead positioning**: Click or drag in the track area or ruler to scrub. The playhead is pointer-events-none — clicks pass through to the track area behind it.

### 4.10 Zoom Bar

18px strip at the bottom of the timeline. Contains:

- **Zoom thumb**: Draggable rectangle representing the visible time range. Width proportional to `(viewEnd - viewStart) / totalDuration`. Minimum width 30px. `cursor: grab` (changes to `grabbing` when active). Background: semi-transparent grey.
- **Playhead indicator**: 4x8px red pill marking the playhead position within the full duration. Visible inside or outside the zoom thumb.

**Zoom bar interactions**:

| Action | Behavior |
|--------|----------|
| **Drag thumb** | Pan the visible time window (constant duration) |
| **Click outside thumb** | Jump the view center and playhead to that position |

---

## 5. Overview

A collapsible 48px strip above the view area showing a bird's-eye view of the full timeline.

### 5.1 Layout

Left 140px: label column reading "OVERVIEW" (9px uppercase, `#555`).

Remaining width: two vertically stacked regions:
- **Top 16px**: Shot blocks (small colored rectangles with 7px text labels)
- **Bottom 31px**: Track rows with miniature keyframe dots (3x3px diamonds) and linked-period regions

### 5.2 Visual Elements

- **Shot blocks**: Colored to match shot color. Current shot at full opacity, others at 0.6.
- **Track rows**: Height = `max(4, (48 - 17) / trackCount)` pixels each.
- **Keyframe dots**: 3px diamonds matching track color.
- **Link regions**: Semi-transparent grey bars.
- **View window**: White-bordered rectangle showing the currently visible time range. Hidden when the entire timeline is visible.
- **Playhead**: 1px red line.

### 5.3 Interactions

| Action | Behavior |
|--------|----------|
| **Click** | Move playhead and recenter the view window at that position |
| **Drag** | Continuously update playhead position (same as click) |

---

## 6. Side Panels

Dockable sidebars on the right side of the workspace. Only one can be visible at a time (opening one closes the other).

### 6.1 Elements Panel

Flat list of all elements in the current scene.

**Header**: "ELEMENTS" (28px, uppercase, `#666`).

Each element row (`.element-item`) contains:
- **Icon** (16px): Varies by type (`diamond` for camera/object, `circle` for character, `square` for environment, `star` for light)
- **Name**: Element name, truncated with ellipsis
- **Link indicator** (if `feat('link-indicators')` and element has a link): "-> Parent" in steel blue (`#557799`)
- **Type tag**: Uppercase 8px label (cam, character, prop, environment, light, mesh, rig)

Child elements (sub-components like Body, Face_Rig) are indented 24px with dimmer icons.

**Lights section**: When `feat('lights-section')` is enabled, lights are separated from other elements by a section header divider labeled "LIGHTS" (8px uppercase).

**Interaction**: Click to select (cyan highlight, 8% white background). Only one element selected at a time — clicking a new element deselects the previous.

### 6.2 Assets Panel

Grid of available assets for placing in the scene.

**Header**: "ASSETS" (28px, uppercase) with a search input field (100px, right-aligned).

Asset grid: responsive columns (`repeat(auto-fill, minmax(80px, 1fr))`). Each asset card is a square (`aspect-ratio: 1`) containing:
- **Icon** (22px emoji, 0.4 opacity)
- **Name** (9px, `#777`, truncated)
- **Type tag** (7px uppercase, `#444`): character, object, environment

### 6.3 Panel Resize

Each side panel has a 5px drag edge on its inner border for horizontal resizing.

---

## 7. Playback

### 7.1 Play/Pause

Toggle via the transport bar play button or `Space` key. During playback:
- Playhead advances in real time (delta-time based, not fixed step)
- Wraps to 0 when reaching `totalDuration`
- If playhead moves outside the visible time window, the view auto-scrolls: window shifts so `viewStart = playhead`, maintaining the same duration
- The play button icon switches from `play-triangle` to `pause` and turns red (`#ff4466`)
- All tracks, overlays, and the camera info update each frame

### 7.2 Timecode Display

Two timecode displays showing different time references:

| Location | Shows | Format |
|----------|-------|--------|
| Transport bar | Shot-local elapsed / shot duration | HH;MM;SS;FF |
| Camera View overlay | Sequence-global position | HH;MM;SS;FF |

Frame rate: 24fps default. Semicolons used as separators (non-drop-frame).

### 7.3 Interpolation

Values between keyframes are linearly interpolated:
- Array properties (position, rotation, scale): interpolated component-wise
- Scalar properties (focal length): interpolated directly
- Before the first keyframe: first keyframe's value
- After the last keyframe: last keyframe's value

---

## 8. Navigation & Zoom

### 8.1 Zoom

Zoom changes the visible time range `[viewStart, viewEnd]` without moving the playhead.

| Input | Behavior |
|-------|----------|
| **Scroll wheel** (track area, ruler, or shot bar) | Zoom at cursor position. Factor: 0.87 (in) / 1.15 (out) |
| **Shift+scroll** | Pan by +/-10% of visible duration |
| **`\` key** | Reset to full duration (`viewStart = 0`, `viewEnd = totalDuration`) |
| **Zoom thumb drag** | Pan with constant zoom level |
| **Overview click** | Recenter view at clicked position |

Zoom is anchored to the mouse position: the time under the cursor stays fixed while the window expands or contracts around it.

**Constraints**: Minimum visible duration: 0.5s. Maximum: `totalDuration`. `viewStart` clamped to >= 0.

### 8.2 Pan

| Input | Behavior |
|-------|----------|
| **Middle-click drag** (track area or shot bar) | Pan proportionally: `-(dx / trackAreaWidth) * visibleDuration` |
| **Shift+scroll** | Pan by 10% of visible duration per scroll step |
| **Zoom thumb drag** | Pan the view window |

### 8.3 Scrubbing

| Input | Behavior |
|-------|----------|
| **Click/drag in track area** | Move playhead to clicked time position |
| **Click/drag on ruler** | Move playhead to clicked time position |
| **Click on overview** | Move playhead and recenter view |
| **Arrow left** | Step back 1 frame (1/24s) |
| **Arrow right** | Step forward 1 frame |
| **Home** | Jump to time 0 |
| **End** | Jump to `totalDuration` |

---

## 9. Shot Management

### 9.1 Adding Shots

The "+" button in the shot track label area (visible when `feat('shot-management')` is enabled) appends a new shot:
- Starts at the end of the last shot
- Duration: 5 seconds
- Color: `hsl((shotCount * 47) % 360, 40%, 45%)` — pseudo-random hue rotation
- Single camera (Cam A) with two keyframes (start and near-end)
- Active camera initialized to index 0

If the new shot extends beyond `viewEnd`, the view expands to include it.

### 9.2 Scene Switching

Scene tabs above the timeline. Clicking a tab:
1. Loads the new scene's shots, tracks, and metadata
2. Resets playhead to 0
3. Resets view to full duration
4. Clears selected keyframe and preview camera
5. Initializes all shots' active camera to index 0
6. Re-renders scene tabs, tracks, and overlays

Note: Track expand state (`trackExpanded`) persists across scene switches.

---

## 10. Multi-Camera

### 10.1 Camera Selection Model

Two independent states per shot:

| State | Meaning | How set |
|-------|---------|---------|
| **Active camera** | Drives playback output and the camera info overlay | Double-click in shot bar, or Shift+1/2/3/4 |
| **Previewed camera** | Temporarily shown for comparison (highlight, not commit) | Single-click a non-active camera in shot bar |

In single-camera shots, these are always the same. Clicking the active camera clears the preview.

### 10.2 Display Camera

The "display camera" is what the user sees and what drives the camera info values. Priority: previewed camera (if set for current shot) > active camera > camera 0.

### 10.3 Active Angle Track

See section 4.6. Only visible for shots with multiple cameras.

---

## 11. Keyboard Shortcuts

### 11.1 Always Available

| Key | Action |
|-----|--------|
| `Space` | Toggle play/pause |
| `Arrow Left` | Step back 1 frame |
| `Arrow Right` | Step forward 1 frame |
| `\` | Zoom to fit full timeline |

### 11.2 Active Tool (`feat('active-tool')`)

| Key | Action |
|-----|--------|
| `Q` | Select tool |
| `W` | Translate tool |
| `E` | Rotate tool |
| `R` | Scale tool |

### 11.3 Full Shortcuts (`feat('full-shortcuts')`)

| Key | Action |
|-----|--------|
| `Home` | Jump to start (time 0) |
| `End` | Jump to end (totalDuration) |
| `G` | Toggle composition guides |
| `H` | Toggle camera info |
| `P` | Toggle camera path (requires `feat('camera-path')`) |
| `D` | Toggle Director View badge (requires `feat('director-view')`) |
| `A` | Cycle aspect ratio forward (requires `feat('aspect-ratio')`) |

### 11.4 Multi-Camera (`feat('active-cam-keys')`)

| Key | Action |
|-----|--------|
| `Shift+1` | Set active camera to A |
| `Shift+2` | Set active camera to B |
| `Shift+3` | Set active camera to C |
| `Shift+4` | Set active camera to D |

### 11.5 Panel Shortcuts

| Key | Action |
|-----|--------|
| `O` | Toggle Elements panel |
| `A` | Toggle Assets panel (only when `feat('full-shortcuts')` is off) |
| `T` | Toggle timeline |
| `Tab` | Toggle all panels: if any visible, hide all; if all hidden, restore timeline + overview |

All keyboard shortcuts are suppressed when an `<input>` or `<select>` element has focus.

---

## 12. Visual Design

### 12.1 Color Palette

Monochromatic dark grey with a single red accent and two secondary accents.

**Backgrounds** (darkest to lightest):
- `#1a1a1a` — darkest surfaces (overview, view placeholders, headers)
- `#1e1e1e` — panel headers, track labels, gutter background
- `#222` — ruler background
- `#232323` — body background
- `#252525` — side panel background
- `#282828` — timeline panel background

**Text** (brightest to dimmest):
- `#ccc` — primary text
- `#bbb` — element names, camera info values
- `#aaa` — secondary text, transport time
- `#888` — dim labels
- `#666` — panel headers, ruler labels, section headers
- `#555` — type tags, gutter tabs, very dim text
- `#444` — placeholders, asset types

**Borders**:
- `#3c3c3c` — primary borders (panel edges, ruler)
- `#333` — secondary borders (shot bar, section dividers)
- `#2a2a2a` — subtle borders (track row bottoms)

**Accents**:
- `#ff4466` — playhead, play indicator, Director View badge
- `#4a9eff` — active tool icon (blue)
- `#eebb44` — camera path badge (amber)
- `#557799` — link indicators (steel blue)
- `#ff6688` — Director View badge text

### 12.2 Typography

- Font stack: `-apple-system, 'Segoe UI', Roboto, monospace`
- All numeric displays use `font-variant-numeric: tabular-nums` for alignment
- Uppercase text uses `text-transform: uppercase` with `letter-spacing: 0.5–2px`
- No custom web fonts

### 12.3 Transitions

All interactive elements use 0.1–0.15s transitions for hover/state changes. Keyframe hover uses `transform` and `filter` transitions. Expand arrows use a `transform` transition for rotation.

---

## 13. Feature Toggle System

The mockup implements a progressive disclosure system where UI features can be individually enabled/disabled. A companion page (`feature-toggles.html`) provides a control panel with per-feature and per-milestone toggles.

### 13.1 Feature Registry

Each feature has an ID, display name, milestone, roadmap section reference, and enabled state. The `feat(id)` function returns whether a feature is currently enabled.

### 13.2 Features by Milestone

| Milestone | ID | Feature |
|-----------|----|---------|
| 1.1 Virtual Camera | `camera-info` | Camera Info |
| 1.2 Camera Overlays | `aspect-ratio` | Aspect Ratio Masks |
| | `composition-guides` | Composition Guides |
| 2.1 Scene Management | `active-tool` | Active Tool Indicator (QWER) |
| | `director-view` | Director View Toggle |
| 3.1 Shot Track | `shot-management` | Shot Add / Delete |
| | `shot-reorder` | Shot Drag Reorder |
| | `duration-edit` | Duration Editing |
| | `aggregate-duration` | Aggregate Duration |
| | `hover-thumbnails` | Shot Hover Thumbnails |
| | `scenes` | Scenes Switcher |
| 3.2 Keyframe Animation | `transport-bar` | Transport Bar |
| | `status-bar` | Status Bar (Keyboard Hints) |
| | `track-expand` | Track Collapse / Expand |
| | `keyframe-drag` | Keyframe Drag |
| | `camera-path` | Camera Path Toggle |
| 2.2 View Layout System | `full-shortcuts` | Panel Layout Switcher |
| 6.3 Element Linking | `link-indicators` | Link Indicators in Panel |
| | `lights-section` | Separate Lights Section |
| 8.1 Selection Refinements | `interp-curves` | Interpolation Curve Presets |
| 9.1 Multi-camera | `active-cam-keys` | Active Camera Shortcuts |
| | `active-angle-track` | Active Angle Track |
| | `multi-split` | Multi-split |

### 13.3 Visibility Control

`applyFeatureVisibility()` runs after any feature toggle change. It:
1. Shows/hides DOM elements mapped to feature IDs (active tool badge, shot management buttons, active angle container)
2. Evaluates compound visibility (camera info requires both `feat('camera-info')` AND `cameraInfoVisible`)
3. Updates aspect ratio masks

---

## 14. Data Model (Mockup)

The mockup uses hardcoded scene data. This section documents the data shape for reference when implementing the production system.

### 14.1 Scene

```
Scene {
  name: string              // "The Interrogation"
  location: string           // "INT. INTERROGATION ROOM — NIGHT"
  shots: Shot[]
  tracks: ElementTrack[]
}
```

### 14.2 Shot

```
Shot {
  name: string              // "WIDE ESTABLISHING"
  start: number             // seconds (absolute, global timeline)
  end: number               // seconds
  color: string             // hex color
  cameras: Camera[]         // 1–4 cameras
  angles: AngleSegment[]    // active angle track data
}
```

### 14.3 Camera

```
Camera {
  name: string              // "Cam A"
  keyframes: CameraKeyframe[]
}

CameraKeyframe {
  time: number              // seconds (absolute)
  pos: [x, y, z]            // world-space position
  rot: [x, y, z]            // euler rotation (tilt, pan, roll) in degrees
  focal: number             // focal length in mm
}
```

### 14.4 Angle Segment

```
AngleSegment {
  camera: number            // index into shot.cameras
  start: number             // seconds
  end: number               // seconds
}
```

### 14.5 Element Track

```
ElementTrack {
  name: string              // "Detective"
  color: string             // hex color
  keyframes: ElementKeyframe[]
  linkedPeriods: LinkedPeriod[]
}

ElementKeyframe {
  time: number              // seconds (absolute, global timeline)
  pos: [x, y, z]
  rot: [x, y, z]
  scale: [x, y, z]
}

LinkedPeriod {
  start: number
  end: number
  parent: string            // label of parent element
}
```

### 14.6 Global State

```
SCENE.fps = 24              // frame rate
SCENE.totalDuration          // computed from last shot's end time
```

---

## 15. Sample Scene Data

The mockup ships with three scenes for testing. Key properties:

### Scene 1: The Interrogation
- Location: INT. INTERROGATION ROOM — NIGHT
- Duration: 48s (14 shots)
- Multi-camera shots: 8 of 14 shots have 2 cameras
- Elements: Detective, Witness, Evidence Folder (linked to Detective for 0–13s), Table, Key Light

### Scene 2: The Alley
- Location: EXT. BACK ALLEY — NIGHT
- Duration: 36s (9 shots)
- Multi-camera shots: 2 of 9
- Elements: Detective, Officer, Evidence Marker, Crime Tape, Streetlight

### Scene 3: The Office
- Location: INT. DETECTIVE'S OFFICE — DAY
- Duration: 32s (7 shots)
- Multi-camera shots: 2 of 7
- Elements: Detective, Evidence Board, Phone, Desk, Desk Lamp

---

## 16. Known Gaps & Future Work

This section documents features present in the roadmap or domain language but not yet in the mockup.

- **Stopwatch model**: Per-track recording toggle is not implemented in the mockup. The mockup shows keyframes as static data.
- **Status bar**: Feature flag exists (`status-bar`) but no status bar UI is rendered.
- **Shot deletion**: Only adding shots is implemented. No delete button or confirmation dialog.
- **Shot reordering**: Feature flag exists (`shot-reorder`) but drag-to-reorder is not implemented.
- **Duration editing**: Feature flag exists (`duration-edit`) but inline duration editing is not implemented.
- **Aggregate duration**: Feature flag exists (`aggregate-duration`) but total running time is not displayed.
- **Keyframe deletion**: No delete key handler for selected keyframes.
- **Designer View**: Placeholder only. No top-down orthographic rendering.
- **Director View**: Placeholder only. No free camera controls.
- **Multi-split**: Feature flag exists (`multi-split`) but the right-click -> "split every N frames" UI is not implemented.
- **Subtitle overlay**: Not present in the mockup.
- **Burn-in**: Not present in the mockup.
- **Aperture/Focus/DOF in camera info**: Displayed as static values, not interpolated from keyframe data.
- **Panel width persistence**: Side panel resize works during a session but width is not persisted.
- **Scroll sync**: Track labels and track area are not scroll-synced for vertical overflow.

---

*Derived from `ui-mockup/` prototype. Last updated: 2026-03-12.*
