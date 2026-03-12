# Milestone 4.3: Asset Import — Specification

**Date**: 2026-03-10
**Milestone**: 4.3
**Status**: Draft

---

- ### 4.3. Asset import (Milestone)

  Bring 3D models from external tools into Fram3d. A production designer hands the director a chair model — the director drags it into the viewport and it appears in the scene, correctly scaled, selectable, and ready to block with. No file dialogs, no import wizards, no material editors. Drag, drop, use.

  Imported models must become first-class scene elements — selectable, transformable, animatable — identical to built-in objects. This means automatic collider generation, automatic material handling, and automatic scale normalization. The filmmaker should never need to think about mesh topology, UV coordinates, or polygon counts.

  Once imported, assets should be reusable. A panel of previously imported assets lets the director place the same prop in multiple shots without re-importing. Thumbnails provide visual recognition so the director is not reading filenames.

  *Blocked by: 2.1 (scene management — imported models become scene elements)*

  - ##### 4.3.1. Model import (Feature)

    ***Drag-and-drop 3D model files into the viewport. The model appears in the scene at a predictable position and real-world scale, with automatically generated collision geometry for selection and gizmo interaction.***

    **Functional requirements:**

    - The system accepts three file formats: FBX, OBJ, glTF/GLB
    - The user imports a model by dragging a file from the OS file manager onto the viewport
    - Alternatively, the user can import via a menu action that opens the OS file picker
    - On drop, the model appears at the center of the viewport's ground plane intersection — where the camera is looking, not at the world origin
      - If the camera is aimed above the horizon (no ground intersection), the model appears at world origin on the ground plane
    - The model is placed with its bottom surface resting on the ground plane (Y=0), not embedded in it and not floating above it
    - Imported models are placed at the camera look-point (where the camera is looking, on the ground plane) — not at world origin or at a fixed offset from the camera
    - The system attempts to import at real-world scale using the unit metadata embedded in the file (FBX and glTF carry unit information)
      - If the file contains no unit metadata (common with OBJ), the system assumes 1 unit = 1 meter (industry convention)
    - If the imported model's bounding box height is less than 1 centimeter or greater than 100 meters, the system auto-scales it to a 1-meter bounding box height and displays a notification: "Model scaled to fit — original scale was unusually small/large"
      - This auto-scale is silent for extreme sizes: models smaller than 1cm or larger than 100m are automatically scaled to a reasonable size without user confirmation
    - The user can always manually rescale after import using the scale gizmo (2.1.2)
    - A mesh collider is automatically generated from the imported geometry so the model is selectable and responds to gizmos
      - For models with more than one mesh, a collider is generated per mesh
      - Collider geometry uses a simplified convex hull, not the full mesh — accurate enough for click selection, not a physics simulation
    - Multi-mesh models (e.g., a car with separate door/wheel meshes) prompt the user on import: "Treat as single element" or "Unpack into individual elements". Single element treats the whole model as one selectable unit. Unpack creates separate scene elements for each sub-mesh.
    - The imported model becomes a scene element (2.1.1) immediately — hover highlighting, selection, transform gizmos all work without any additional steps
    - Materials and textures embedded in the file or referenced by relative paths are applied automatically
      - FBX: embedded textures are extracted and applied
      - OBJ: the accompanying .mtl file and texture files are read if present in the same directory
      - glTF/GLB: PBR materials and embedded/referenced textures are applied
    - If textures are missing or cannot be resolved, the model imports with a neutral gray material — it does not fail
    - If materials reference advanced shader features not supported by Fram3d's renderer, they degrade to the closest available approximation — diffuse color and texture at minimum
    - Supports embedded animation data in imported files (e.g., a spinning fan in an FBX). The animation plays in the scene.
    - The import happens asynchronously — the viewport remains responsive during import
    - A progress indicator is visible during import for files that take longer than 500 milliseconds to process
    - If the file is corrupt or unreadable, the import fails gracefully with a notification: "Could not import [filename] — file may be damaged or in an unsupported format"
    - If the file contains no mesh data (e.g., an FBX with only animation curves), the import fails with a notification: "No geometry found in [filename]"
    - Re-importing the same file (detected by filename) offers to replace the existing asset rather than creating a duplicate entry
    - Multiple files can be dragged in at once — each is imported as a separate scene element
    - Each imported model is auto-named from the filename (without extension), e.g., "dining_chair" from "dining_chair.fbx"
    - If a name collision exists, a numeric suffix is appended: "dining_chair_2"

    **Expected behavior:**
    ``` python
      # basic drag-and-drop import
      .if the user drags an FBX file onto the viewport >>
          <== the model appears in the scene at ground level
          <== the model is positioned where the camera is looking
          <== the model is selectable by clicking on it
          <== transform gizmos work on the model
          <== the model name appears in any scene element lists

      # placement at camera look point
      .if the camera is aimed at the ground plane
      .if the user drops a model file onto the viewport >>
          <== the model appears at the ground plane intersection point of the camera's center ray
          <== the model's bottom surface rests on Y=0
          !== the model appears at world origin
          !== the model appears at a fixed offset from the camera

      # camera aimed at sky — fallback to origin
      .if the camera is aimed above the horizon
      .if the user drops a model file onto the viewport >>
          <== the model appears at world origin (0, 0, 0)
          <== the model's bottom surface rests on Y=0

      # real-world scale from file metadata
      .if the user imports a model that is 0.45m tall in its source application
      .if the file contains correct unit metadata >>
          <== the model appears at approximately 0.45m tall in Fram3d
          !== the model appears at an arbitrary scale

      # missing unit metadata defaults to meters
      .if the user imports an OBJ file with no unit information
      .if the raw vertex coordinates span 0.45 units on the Y axis >>
          <== the model appears 0.45m tall (1 unit = 1 meter assumed)

      # absurdly large model is auto-scaled
      .if the user imports a model whose bounding box is 500 meters tall >>
          <== the model is scaled to a 1-meter bounding box height
          <== a notification informs the user that scaling was applied
          <== the user can manually rescale with the scale gizmo
          !== the system prompts for confirmation before scaling

      # absurdly small model is auto-scaled
      .if the user imports a model whose bounding box is 0.002 meters tall >>
          <== the model is scaled to a 1-meter bounding box height
          <== a notification informs the user that scaling was applied
          !== the system prompts for confirmation before scaling

      # multi-mesh model — single element
      .if the user imports a model containing multiple sub-meshes (e.g., a car with doors and wheels)
      .if the user chooses "Treat as single element" >>
          <== the entire model is one selectable scene element
          <== clicking on any part of the model selects the whole thing
          !== individual sub-meshes are independently selectable

      # multi-mesh model — unpack
      .if the user imports a model containing multiple sub-meshes
      .if the user chooses "Unpack into individual elements" >>
          <== each sub-mesh becomes a separate scene element
          <== each sub-mesh is independently selectable
          <== each sub-mesh is independently transformable
          <== each sub-mesh is named from the mesh name in the file

      # multi-mesh model prompts user
      .if the user imports a model containing more than one mesh >>
          <== a prompt appears: "Treat as single element" or "Unpack into individual elements"
          !== the system silently chooses one behavior

      # embedded animation data
      .if the user imports an FBX file containing animation data (e.g., a spinning fan) >>
          <== the model appears in the scene with its animation playing
          <== the animation loops in the viewport
          !== the animation data is discarded on import

      # re-import same file offers replacement
      .if the user imports "chair.fbx"
      .if the user later imports a file also named "chair.fbx" >>
          <== the system offers to replace the existing asset
          !== a duplicate asset entry is silently created

      # re-import replacement accepted
      .if the user imports a file with the same name as an existing asset
      .if the user accepts the replacement >>
          <== the existing asset is updated with the new file's data
          <== instances already placed in the scene update to reflect the new model

      # re-import replacement declined
      .if the user imports a file with the same name as an existing asset
      .if the user declines the replacement >>
          <== a new asset entry is created with a numeric suffix
          <== the original asset is unchanged

      # materials and textures come through
      .if the user imports a model with embedded textures >>
          <== the model renders with its textures applied
          !== the model appears as untextured gray

      # missing textures degrade gracefully
      .if the user imports a model that references texture files not present on disk >>
          <== the model imports successfully
          <== surfaces with missing textures render as neutral gray
          !== the import fails or shows error dialogs

      # collider generation for selection
      .if the user imports a model
      ||> .if the user clicks on the model in the viewport >>
          <== the model is selected
          <== highlight and selection visuals appear (per 2.1.1)
          <== the active gizmo attaches to the model (per 2.1.2)

      # multi-mesh colliders
      .if the user imports a model containing multiple meshes >>
          <== clicking on any mesh within the model selects the entire model
          !== individual sub-meshes are treated as separate scene elements

      # async import with progress
      .if the user imports a large model that takes more than 500ms to process >>
          <== a progress indicator is visible during import
          <== the viewport remains responsive — the user can still orbit the camera
      ||> .if the import completes >>
          <== the progress indicator disappears
          <== the model appears in the scene

      # corrupt file handling
      .if the user drags a corrupt or unreadable file onto the viewport >>
          <== a notification appears: "Could not import [filename]"
          !== the application crashes or hangs
          !== a partial or broken model appears in the scene

      # no geometry in file
      .if the user imports an FBX that contains only animation data >>
          <== a notification appears: "No geometry found in [filename]"
          !== an invisible scene element is created

      # multiple files at once
      .if the user drags three model files onto the viewport simultaneously >>
          <== three separate scene elements appear in the scene
          <== each is independently selectable
          <== each is named from its respective filename

      # auto-naming and collision
      .if the user imports "chair.fbx"
      .if a scene element named "chair" already exists >>
          <== the new element is named "chair_2"

      # menu-based import
      .if the user opens the import dialog from the menu
      .if the user selects a model file >>
          <== the model imports identically to drag-and-drop
          <== placement follows the same camera look-point rules
    ```

    **Error cases:**
    ``` python
      # unsupported file type
      .if the user drags a non-model file (e.g., .png, .pdf, .txt) onto the viewport >>
          <== nothing happens — no import, no error
          !== the application attempts to parse a non-model file

      # file disappears during import
      .if the source file is deleted or moved while the import is in progress >>
          <== the import fails with a notification
          !== the application crashes

      # extremely high polygon count
      .if the user imports a model with millions of polygons >>
          <== the model imports (may take longer)
          <== the progress indicator reflects the longer processing time
          !== the application becomes permanently unresponsive
    ```

  - ##### 4.3.2. Asset library (Feature)

    ***A panel showing all models the user has imported, organized as a categorized list with thumbnail previews, for placing additional copies into the scene without re-importing from disk.***

    **Functional requirements:**

    - A collapsible "Assets" panel is accessible from the UI
    - Every model that has been successfully imported into the current project appears in the panel
    - Assets are displayed as a categorized list with thumbnails — list-based organization, not a thumbnail-only grid
    - Each asset entry shows a thumbnail and the asset name
    - Thumbnails are automatically generated — a small rendered preview of the model from a 3/4 front angle
    - The user places an asset into the scene by dragging it from the panel onto the viewport
    - Placement from the asset library follows the same rules as initial import: camera look-point on the ground plane, bottom-resting, correct scale
    - Each placement from the library creates an independent scene element — transforms are not linked between copies
    - Assets are embedded within the project file (4.2) — the project is fully self-contained
      - Importing a model copies the file (and its textures) into the project's asset storage
      - The original file on disk is not referenced after import — the project is self-contained
      - Moving or deleting the original file does not affect the project
    - When importing large assets, the system warns the user about the impact on project file size (since assets are embedded in the project)
    - The asset library persists with the project — closing and reopening the project restores the full library with thumbnails
    - The panel supports scrolling when the number of assets exceeds the visible area
    - Assets can be removed from the library via a right-click context menu "Remove from library"
      - Removing an asset from the library does not delete instances already placed in the scene
      - Removing an asset deletes the embedded asset data from the project
    - The panel displays a placeholder message when no assets have been imported: "Drag model files into the viewport to add assets"

    **Expected behavior:**
    ``` python
      # asset appears in library after import
      .if the user imports a model via drag-and-drop >>
          <== the model appears in the Assets panel
          <== a thumbnail preview is generated for the model
          <== the asset name matches the scene element name

      # list-based organization
      .if the user has imported multiple assets >>
          <== assets are displayed as a categorized list, not a thumbnail-only grid
          <== each list entry shows a thumbnail and the asset name
          !== assets are displayed as only thumbnails without names or categories

      # placing from the library
      .if the user drags an asset from the panel onto the viewport >>
          <== a new instance of the model appears in the scene
          <== the instance is placed at the camera look-point on the ground plane
          <== the instance is an independent scene element
          !== the new instance is linked to previous instances

      # multiple placements are independent
      .if the user places the same asset three times from the library >>
          <== three independent scene elements exist
          <== moving one does not affect the others
          <== each can be scaled, rotated, and animated independently

      # project self-containment
      .if the user imports a model from an external folder
      ||> .if the user deletes the original file from that folder >>
          <== the asset library still shows the asset
          <== placing the asset from the library still works
          <== the project file can be shared to another machine and the asset is intact

      # large asset import warning
      .if the user imports a model with a large file size >>
          <== the system warns the user about the impact on project file size
          <== the warning indicates that assets are embedded in the project
          <== the user can proceed or cancel the import

      # library persists across sessions
      .if the user imports several models
      .if the user saves and closes the project
      ||> .if the user reopens the project >>
          <== the Assets panel shows all previously imported models
          <== thumbnails are present
          <== assets can be placed into the scene

      # removing an asset from the library
      .if the user right-clicks an asset and selects "Remove from library" >>
          <== the asset is removed from the Assets panel
          <== instances already in the scene are not affected
          <== the embedded asset data is deleted from the project

      # empty state
      .if no assets have been imported >>
          <== the Assets panel displays "Drag model files into the viewport to add assets"
          !== the panel is blank with no guidance

      # scrolling
      .if the user has imported more assets than fit in the panel >>
          <== the panel scrolls to reveal additional assets
          <== thumbnails remain correctly rendered during scrolling

      # thumbnail generation
      .if the user imports a model >>
          <== the thumbnail shows a recognizable preview of the model
          <== the preview is rendered from a 3/4 front angle
          <== the model fills most of the thumbnail area regardless of its real-world size
    ```

    **Error cases:**
    ``` python
      # project asset directory is missing or corrupted
      .if the project's asset directory is damaged when the project is opened >>
          <== assets that cannot be loaded show a broken-asset placeholder thumbnail
          <== the user is notified which assets could not be loaded
          !== the project fails to open entirely

      # drag from library while panel is closing
      .if the user begins dragging an asset and the panel collapses >>
          <== the drag operation is cancelled cleanly
          !== a ghost model is left in the scene
    ```
