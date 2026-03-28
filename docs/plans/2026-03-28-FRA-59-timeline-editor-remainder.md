# FRA-59: Timeline Editor — Remainder

## What already exists (from 3.1.2 Shot Track UI)

- TransportBar: play/pause, shot-local timecode, duration, shot name
- Ruler: adaptive ticks, frame dividers, playhead, click/drag scrub
- Track area: skeleton row with label column + content, playhead, out-of-range
- ZoomBar: thumb drag pan, playhead indicator
- Scroll zoom: WheelEvent on ruler/track/shot track
- Keyboard: Space (play/pause), +/- (zoom), T (toggle timeline)

## What 3.2.1 adds

### Phase 1: Core — Timeline keyboard operations

Modify: `Unity/Fram3d/Assets/Scripts/Core/Timelines/Timeline.cs`

- `FitAll()` already exists — just needs keyboard wiring
- Add `JumpToStart()` — resets playhead to 0, evaluates
- Add `JumpToEnd()` — scrubs playhead to TotalDuration, evaluates

Tests in `TimelineTests.cs`.

### Phase 2: UI — Status bar

New file: `Unity/Fram3d/Assets/Scripts/UI/Timeline/StatusBar.cs`

- VisualElement, 22px height, positioned at the bottom of the timeline section
- Displays contextual keyboard hints as a label
- Default text: "Space: Play/Pause   +/-: Zoom   T: Toggle Timeline   \: Fit All"
- Later phases (3.2.3, 3.2.4) will update this text based on selection/recording state
- For now: static default hints

### Phase 3: UI — Timeline resize handle

Modify: `Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs`

- Add a 5px drag handle between the view area (above) and the timeline section
- PointerDown starts drag, PointerMove adjusts height, PointerUp ends drag
- Clamp height: min 80px, max 80vh (compute from Screen.height)
- Update `_shotController.SetBottomInset()` during drag
- Replace the fixed `SECTION_HEIGHT` constant with a mutable `_sectionHeight` field

### Phase 4: UI — Camera View overlays (shot label + sequence timecode)

New file: `Unity/Fram3d/Assets/Scripts/UI/Views/ShotLabelOverlay.cs`

- MonoBehaviour on the same UIDocument as other Camera View overlays
- Top-left label: "Shot 1A: {name} ({duration}s)"
- Updates on shot change and during playback
- Hidden when no shot is current

New file: `Unity/Fram3d/Assets/Scripts/UI/Views/SequenceTimecodeOverlay.cs`

- MonoBehaviour on the same UIDocument as other Camera View overlays
- Bottom-center label: sequence-global timecode (HH;MM;SS;FF)
- Updates every frame during playback and on scrub
- Hidden when no shots exist

### Phase 5: UI — Keyboard shortcuts

Modify: `Unity/Fram3d/Assets/Scripts/UI/Input/KeyboardShortcutRouter.cs`

- `\` key → `FitAll()` on timeline
- Home → `JumpToStart()`
- End → `JumpToEnd()`

Modify: `Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs`

- Expose `FitAll()`, `JumpToStart()`, `JumpToEnd()` public methods
- Modify `OnWheel` to handle Shift+scroll as horizontal pan (currently all scroll zooms)

## File changes

| Action | Path |
|--------|------|
| Modify | `Unity/Fram3d/Assets/Scripts/Core/Timelines/Timeline.cs` |
| Create | `Unity/Fram3d/Assets/Scripts/UI/Timeline/StatusBar.cs` |
| Modify | `Unity/Fram3d/Assets/Scripts/UI/Timeline/TimelineSectionView.cs` |
| Create | `Unity/Fram3d/Assets/Scripts/UI/Views/ShotLabelOverlay.cs` |
| Create | `Unity/Fram3d/Assets/Scripts/UI/Views/SequenceTimecodeOverlay.cs` |
| Modify | `Unity/Fram3d/Assets/Scripts/UI/Input/KeyboardShortcutRouter.cs` |
| Modify | `tests/Fram3d.Core.Tests/Timelines/TimelineTests.cs` |
