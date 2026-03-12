# Milestone 2.2: Save / Load — Specification

**Date**: 2026-03-10
**Milestone**: 2.2
**Status**: Draft
**Blocked by**: 2.1 (Undo / Redo — dirty state tracking integrates with the undo stack)

---

- ### 2.2. Save / Load (Milestone)

  Project persistence. Save scene state, shots, animations, and settings to disk. Reopen and continue working exactly where you left off.

  Save/load is what separates a demo from a tool. If a filmmaker can't close the app and come back tomorrow to the same scene, the same shots, the same keyframes — nothing else matters. The file must capture everything the user built and nothing they didn't intend to keep. Auto-save must protect against crashes without surprising the user. Dirty tracking must be precise enough that people trust the asterisk: if it's clean, their work is saved.

  *Blocked by: 2.1 (undo/redo — save integrates with undo stack for dirty tracking)*

  - ##### 2.2.1. Project creation wizard (Feature)

    ***New Project dialog with Quick Create and Advanced Create paths. Quick Create: name + save location + template (Film or TV). Advanced Create: multi-layered camera/lens pickers, format derived from body. Recent projects list on the same screen. User-extensible templates.***

    > **Note**: Full specification is in the Project Creation Wizard spec. This section establishes the feature's position in the save/load milestone. See the dedicated spec for complete functional requirements, expected behavior, and error cases.

    **Key requirements:**
    - New Project dialog appears on Cmd+N and on first launch
    - Two creation paths: Quick Create (3 fields) and Advanced Create (camera body → lens → format derivation)
    - Two built-in templates: Film and TV (different default resolutions, frame rates, aspect ratios)
    - Recent projects list integrated into the same dialog (not a separate screen)
    - Open Project button in the same dialog
    - User-extensible templates: save current project settings as a reusable template
    - The wizard produces a project with: name, save location, resolution, frame rate, aspect ratio, camera body, lens set

  - ##### 2.2.2. Project file format (Feature)

    ***Serialize full project state: scene objects, shot sequence, all keyframe data, camera settings, overlay configuration, and HUD state — everything needed to reconstruct the project exactly as it was.***

    **Functional requirements:**
    - A project file captures the complete state required to reopen the project indistinguishably from when it was saved
    - The file includes a format version identifier
    - Asset bundling is user-configurable. On first import of any asset, the system prompts: **"Do you want to bundle assets with your project?"** with three options:
      - **"No"** — all assets stored by reference (external file paths). Projects are lightweight but not portable.
      - **"Yes (everything)"** — all assets embedded in the project file. Projects are fully self-contained and portable.
      - **"Yes (up to X MB)"** — assets up to the specified size are embedded; larger assets are stored by reference. Default threshold: 5MB per asset, 50MB project total.
    - This preference is configurable in the Settings/Preferences panel and can be changed at any time.
    - When the bundling mode changes, the system auto-migrates existing assets to match the new setting (embedding referenced assets or extracting embedded assets to references) on the next save.
    - All keyframe data is saved per-property (position x/y/z, pan, tilt, roll, focal length, scale, rotation x/y/z)
    - Camera shake per-shot state (enabled/disabled, amplitude, frequency) is saved
    - Human-readable format preferred (for git diffing). If binary/compressed would be significantly better for performance or file size, document the tradeoff.
    - Version-tolerant loading: older project files load in newer versions of the application. Conversion happens automatically if needed. The app should not refuse to open an older file.
    - The following state is saved:
      - Scene objects: type, position, rotation, scale, name, material/appearance properties, embedded asset geometry
      - Shot sequence: ordered list of shots, each with name, duration, shot ID
      - Camera animation per shot: all camera keyframes per-property (time, position x/y/z, pan, tilt, roll, focal length)
      - Object animation per shot: all object keyframes per-property (time, position x/y/z, rotation x/y/z, scale) and initial object state
      - Camera settings: current focal length, camera body preset, lens preset, sensor dimensions
      - Camera position and rotation (current viewport state)
      - Overlay state: selected aspect ratio, frame guide visibility (thirds, center cross, safe zones)
      - HUD state: camera info HUD visibility
      - Camera shake settings per shot: enabled/disabled, amplitude, frequency
      - Depth of field settings: enabled/disabled, aperture, focus distance
      - Selected shot index and current time within shot
      - Camera path visualization: enabled/disabled
    - The following state is NOT saved:
      - Window position and dimensions
      - Scroll position within the shot sequencer
      - Scroll position within the keyframe editor
      - Object selection state (which object is selected)
      - Gizmo mode (translate/rotate/scale)
      - Playback state (playing/paused)
      - Undo/redo history
      - Drag operation state
      - Hover/highlight state
    - Object identity is preserved across save/load — references between objects (e.g., keyframe-to-object mappings) survive serialization
    - Shot IDs are preserved across save/load — the same shot maintains its identity
    - Keyframe IDs are preserved across save/load

    **Expected behavior:**
    ``` python
      # round-trip fidelity
      .if the user has a project with scene objects, shots, keyframes, and camera settings
      .if the user saves the project
      .if the user closes and reopens the project >>
          <== every scene object is at its saved position, rotation, and scale
          <== every shot appears in the sequencer in the same order
          <== every keyframe is at its saved time with its saved values
          <== the camera is at its saved position, rotation, and focal length
          <== the selected aspect ratio is restored
          <== frame guide visibility matches the saved state
          <== camera shake settings are restored
          <== the camera info HUD visibility is restored

      # embedded assets are self-contained (when bundling is enabled)
      .if the user has selected "Yes (everything)" for asset bundling
      .if the user adds objects to the scene
      .if the user saves the project
      .if the user moves the project file to a different machine >>
          <== the project opens successfully
          <== all scene objects appear with their correct geometry
          !== the application reports missing asset files
          !== any object appears as a placeholder or error shape

      # referenced assets require original paths
      .if the user has selected "No" for asset bundling
      .if the user moves the project file to a different machine >>
          <== assets with broken references show a placeholder or warning
          <== the user is prompted to relocate missing assets

      # per-property keyframe fidelity
      .if the user sets keyframes on individual properties (e.g., position x only, or focal length only)
      .if the user saves and reopens the project >>
          <== only the properties that had keyframes still have keyframes
          !== un-keyed properties gain keyframes
          <== keyframe values match exactly

      # camera shake per-shot state
      .if shot A has camera shake enabled with amplitude 0.5 and frequency 3
      .if shot B has camera shake disabled
      .if the user saves and reopens the project >>
          <== shot A has camera shake enabled with amplitude 0.5 and frequency 3
          <== shot B has camera shake disabled

      # ephemeral state is not restored
      .if the user had an object selected before saving
      .if the user reopens the project >>
          <== no object is selected
          <== the gizmo is not visible
          !== the previously selected object appears selected

      # playback state is not restored
      .if playback was running when the user saved
      .if the user reopens the project >>
          <== playback is paused
          <== the playhead is at the saved time position

      # empty project
      .if the user saves a project with no shots and no added objects >>
          <== the file is created successfully
      ||> .if the user reopens it >>
          <== the project loads to the default state
    ```

    **Version compatibility:**
    ``` python
      # file from same version
      .if the user opens a file saved by the same application version >>
          <== the project loads with full fidelity

      # file from older version — automatic migration
      .if the user opens a file saved by an older version of the application >>
          <== the project loads successfully
          <== fields added in newer versions use sensible defaults
          <== no data from the older file is lost or corrupted
          <== conversion happens automatically without user intervention
          !== the application refuses to open the file
          !== the user is asked to manually convert the file

      # file from newer version
      .if the user opens a file saved by a newer version of the application >>
          <== the application displays a warning that the file was created by a newer version
          <== the application attempts to load the file
          <== fields the current version does not recognize are ignored without error
          !== the application crashes or corrupts the file

      # version migration preserves all existing data
      .if the user opens a v1 file in a v2 application
      .if v2 added new fields (e.g., depth of field settings) >>
          <== all v1 data loads correctly
          <== new v2 fields are initialized to defaults
          <== the project is usable immediately
      ||> .if the user saves the project >>
          <== the file is written in v2 format
          <== opening it again does not trigger migration

      # corrupted file
      .if the user opens a file that is corrupted or not a valid project file >>
          <== the application displays an error message identifying the problem
          <== the current project (if any) remains open and unaffected
          !== the application crashes
          !== the current project is closed or replaced with a partial load
    ```

  - ##### 2.2.3. Save / Load UI (Feature)

    ***Save, Save As, Open, and Recent Projects commands — plus automatic background saves to protect work in progress.***

    **Functional requirements:**
    - **Save** (Cmd+S): writes to the current file path. If the project has never been saved, behaves as Save As.
    - **Save As** (Cmd+Shift+S): prompts for a file name and location, then writes to the chosen path. The project is now associated with the new path.
    - **Open** (Cmd+O): prompts the user to select a project file. If the current project has unsaved changes, prompts to save first (see 2.2.3).
    - **New Project** (Cmd+N): resets to a clean default state. If the current project has unsaved changes, prompts to save first.
    - **Recent Projects**: the application tracks the last 10 opened or saved project paths
    - Recent projects are displayed in order of most recently accessed
    - Recent Projects list includes a "Clear Recent" option that removes all entries from the list
    - Selecting a recent project opens it (with the same unsaved-changes guard as Open)
    - If a recent project file no longer exists at its recorded path, the entry is displayed but marked as unavailable
    - Attempting to open a missing recent project displays an error and offers to remove it from the list
    - Auto-save writes to a dedicated auto-save location, not to the user's project file
    - Auto-save interval is user-configurable, default 2 minutes
    - Auto-save does not reset dirty state — the title bar asterisk remains until the user explicitly saves
    - Auto-save does not trigger if the project has no unsaved changes since the last auto-save
    - On launch, if an auto-save file exists that is newer than the last explicit save, the application offers to recover it
    - The user can decline recovery, in which case the auto-save file is discarded
    - After successful explicit save, the auto-save file is deleted
    - Crash recovery dialog shows only the timestamp of the auto-save (no preview or summary)

    **Expected behavior:**
    ``` python
      # first save prompts for location
      .if the project has never been saved
      .if the user presses Cmd+S >>
          <== a file dialog appears prompting for name and location
      ||> .if the user confirms >>
          <== the project is saved to the chosen path
          <== the title bar shows the project name without an asterisk
          <== future Cmd+S saves to the same path without prompting

      # save as creates a new file
      .if the project is saved as "Scene_A"
      .if the user chooses Save As and names it "Scene_B" >>
          <== the project is written to "Scene_B"
          <== the project is now associated with "Scene_B"
          <== future Cmd+S saves to "Scene_B"
          <== "Scene_A" still exists on disk with its previous contents

      # open replaces the current project
      .if the user opens a project file >>
          <== the current project is replaced by the loaded project
          <== the title bar shows the loaded project name
          <== the loaded project is added to Recent Projects

      # new project resets state
      .if the user creates a new project >>
          <== the scene is cleared to the default state
          <== the sequencer is empty
          <== the camera returns to its default position and focal length
          <== the title bar shows "Untitled" (or equivalent)
          <== overlay settings return to defaults

      # recent projects list
      .if the user opens "Project_A", then "Project_B", then "Project_C" >>
          <== the recent projects list shows Project_C first, then Project_B, then Project_A
      ||> .if the user opens "Project_A" again >>
          <== Project_A moves to the top of the recent list

      # recent projects overflow
      .if the recent projects list contains 10 entries
      .if the user opens a new project >>
          <== the oldest entry is removed from the list
          <== the new project appears at the top

      # clear recent projects
      .if the recent projects list has entries
      .if the user selects "Clear Recent" >>
          <== all entries are removed from the recent projects list
          <== the list is empty
          !== the current project is affected

      # missing recent project
      .if a file in the recent projects list has been moved or deleted
      .if the user views the recent projects list >>
          <== the entry appears but is visually marked as unavailable
      ||> .if the user selects the missing entry >>
          <== an error message states the file could not be found
          <== the user is offered the option to remove it from the list
          !== the application crashes

      # cancel during save dialog
      .if the user presses Cmd+S on an unsaved project
      .if the file dialog appears
      .if the user cancels the dialog >>
          <== no file is created
          <== the project remains unsaved
          <== the dirty state is unchanged
    ```

    **Auto-save behavior:**
    ``` python
      # auto-save triggers on interval
      .if the project has unsaved changes
      .if the configured auto-save interval has elapsed since the last auto-save >>
          <== an auto-save file is written in the background
          <== the title bar asterisk remains (dirty state unchanged)
          !== the user is interrupted or shown a dialog

      # auto-save interval is configurable
      .if the user sets the auto-save interval to 5 minutes >>
          <== auto-saves occur every 5 minutes instead of the default 2
      ||> .if the user sets the interval back to the default >>
          <== auto-saves occur every 2 minutes

      # auto-save skips clean projects
      .if the project has no unsaved changes >>
          !== an auto-save file is written
          !== any timer or background operation runs

      # auto-save during drag operation
      .if the user is in the middle of a drag operation (moving an object, dragging a keyframe)
      .if the auto-save timer fires >>
          <== the auto-save is deferred until the drag operation completes
          !== the drag is interrupted
          !== the saved state captures a mid-drag position

      # auto-save during playback
      .if playback is running
      .if the auto-save timer fires >>
          <== the auto-save is deferred until playback stops
          !== playback is interrupted

      # crash recovery
      .if the application terminated unexpectedly
      .if an auto-save file exists
      .if the user relaunches the application >>
          <== a dialog offers to recover the auto-saved work
          <== the dialog shows only the timestamp of the auto-save
          !== the dialog shows a preview or summary of the project contents
      ||> .if the user accepts recovery >>
          <== the auto-saved project is loaded
          <== the project is marked as dirty (needs explicit save)
      ||> .if the user declines recovery >>
          <== the auto-save file is discarded
          <== the application starts normally

      # auto-save cleanup after explicit save
      .if the user explicitly saves the project (Save or Save As) >>
          <== any existing auto-save file for this project is deleted
    ```

  - ##### 2.2.4. Dirty state tracking (Feature)

    ***Track whether the project has unsaved changes. Show a visual indicator and warn before discarding work.***

    **Functional requirements:**
    - The title bar displays an asterisk (*) after the project name when there are unsaved changes
    - A project starts clean (no asterisk) when opened or newly created
    - After an explicit save, the project returns to clean state (asterisk removed)
    - Creating a New Project resets overlay/HUD settings to defaults (does not preserve user preferences from previous project)
    - The following actions mark the project as dirty:
      - Adding, removing, or moving a scene object
      - Changing an object's properties (scale, rotation)
      - Adding, deleting, or reordering a shot
      - Changing a shot's name or duration
      - Adding, deleting, or moving a keyframe
      - Moving the camera (position or rotation) outside of playback
      - Changing focal length outside of playback
      - Changing the camera body or lens preset
      - Changing the aspect ratio
      - Toggling frame guides (thirds, center cross, safe zones)
      - Toggling the camera info HUD
      - Changing camera shake settings
      - Changing depth of field settings
      - Duplicating an object
      - Toggling camera path visualization
    - The following actions do NOT mark the project as dirty:
      - Scrolling the shot sequencer
      - Scrolling the keyframe editor timeline
      - Scrubbing the playhead (moving current time)
      - Playing/pausing playback
      - Selecting or deselecting an object
      - Hovering over an object
      - Switching gizmo mode (translate/rotate/scale)
      - Resizing the window
      - Camera movement during playback (playback is evaluating, not authoring)
      - Undo/redo that returns to the last saved state (see below)
    - Dirty state integrates with the undo stack: if the user makes a change (dirty), then undoes it back to the saved state, the project returns to clean
    - If the user undoes past the save point (saved, then undid earlier changes), the project is dirty again

    **Expected behavior:**
    ``` python
      # clean on open
      .if the user opens a project >>
          <== the title bar shows the project name without an asterisk
          <== the project is in a clean state

      # dirty after modification
      .if the project is clean
      .if the user moves a scene object >>
          <== the title bar shows an asterisk after the project name
          <== the project is in a dirty state

      # clean after save
      .if the project is dirty
      .if the user saves the project >>
          <== the asterisk is removed from the title bar
          <== the project is in a clean state

      # new project resets overlay/HUD to defaults
      .if the user has a project open with custom overlay settings (e.g., thirds enabled, specific aspect ratio)
      .if the user creates a new project >>
          <== overlay settings are reset to defaults
          <== HUD settings are reset to defaults
          !== overlay or HUD settings from the previous project carry over

      # non-dirtying actions
      .if the project is clean
      .if the user scrubs the playhead >>
          <== the title bar remains without an asterisk
          !== the project becomes dirty
      ||> .if the user selects an object >>
          <== the title bar remains without an asterisk
      ||> .if the user scrolls the sequencer >>
          <== the title bar remains without an asterisk

      # undo restores clean state
      .if the project is clean (just saved)
      .if the user moves an object (project becomes dirty)
      .if the user undoes the move >>
          <== the project returns to clean state
          <== the asterisk is removed

      # undo past save point
      .if the user makes change A
      .if the user saves (clean)
      .if the user undoes change A >>
          <== the project is dirty (state differs from what was saved)
          <== the asterisk appears

      # redo back to save point
      .if the project is dirty because the user undid past the save point
      .if the user redoes back to the saved state >>
          <== the project returns to clean
          <== the asterisk is removed

      # multiple changes then undo
      .if the project is clean (just saved)
      .if the user makes changes A, B, and C
      .if the user undoes C >>
          <== the project is still dirty (A and B remain)
      ||> .if the user undoes B >>
          <== the project is still dirty (A remains)
      ||> .if the user undoes A >>
          <== the project returns to clean
    ```

    **Unsaved changes warning:**
    ``` python
      # closing with unsaved changes
      .if the project has unsaved changes
      .if the user attempts to close the application >>
          <== a dialog appears: "You have unsaved changes. Save before closing?"
          <== the dialog offers three options: Save, Don't Save, Cancel
      ||> .if the user chooses Save >>
          <== the project is saved
          <== the application closes
      ||> .if the user chooses Don't Save >>
          <== the project is not saved
          <== the application closes
      ||> .if the user chooses Cancel >>
          <== the application remains open
          <== the project state is unchanged

      # closing a clean project
      .if the project has no unsaved changes
      .if the user attempts to close the application >>
          <== the application closes immediately
          !== a save dialog appears

      # opening with unsaved changes
      .if the project has unsaved changes
      .if the user attempts to open another project >>
          <== the same three-option dialog appears before opening
      ||> .if the user chooses Save >>
          <== the current project is saved
          <== the new project is opened
      ||> .if the user chooses Don't Save >>
          <== the current project is discarded
          <== the new project is opened
      ||> .if the user chooses Cancel >>
          <== the open operation is cancelled
          <== the current project remains open

      # new project with unsaved changes
      .if the project has unsaved changes
      .if the user attempts to create a new project >>
          <== the same three-option dialog appears
          <== behavior matches the open-with-unsaved-changes flow

      # first save during close dialog
      .if the project has never been saved
      .if the user attempts to close the application
      .if the dialog appears and the user chooses Save >>
          <== a file dialog appears for Save As
      ||> .if the user completes the Save As >>
          <== the project is saved to the chosen path
          <== the application closes
      ||> .if the user cancels the Save As dialog >>
          <== the application remains open
          <== the project is still unsaved
    ```

    **Error cases:**
    ``` python
      # save failure
      .if the user attempts to save
      .if the write fails (disk full, permissions error, path invalid) >>
          <== an error message describes what went wrong
          <== the project remains in memory, unchanged
          <== the dirty state is unchanged (still dirty)
          !== the application crashes
          !== the user loses their work

      # save failure during close
      .if the user is closing with unsaved changes
      .if the user chooses Save
      .if the save fails >>
          <== an error message is displayed
          <== the application remains open
          !== the application closes with unsaved work
    ```

  - ##### 2.2.5. Multi-scene project structure (Feature)

    ***A project contains one or more scenes. Each scene is a self-contained unit with its own objects, environment, lighting, shots, and timelines. Scene tab bar for switching. Characters are project-level definitions; character state (pose, position) is scene-level.***

    > **Note**: Full specification is in the Multi-Scene Project Structure spec. This section establishes the feature's position in the save/load milestone. See the dedicated spec for complete functional requirements, expected behavior, and error cases.

    **Key requirements:**
    - A project contains one or more scenes
    - Each scene is fully self-contained: its own objects, environment, lighting, shots, and timelines
    - Scene tab bar at the top for switching between scenes
    - Scenes are fully independent — changing one does not affect another
    - Duplicate scene creates a precise copy (all objects, shots, keyframes, camera settings)
    - Character definitions are project-level (name, appearance, customization)
    - Character state (pose, position, keyframes) is scene-level
    - Unlimited scenes with lazy loading (only the active scene is fully loaded in memory)
    - New projects start with one default scene
    - Scene names are editable
    - Deleting a scene requires confirmation; the last scene cannot be deleted
