# Assembly Inventory

**Date**: 2026-03-12
**Parent**: Design Session 1 — Shared Kernel & Assembly Graph
**Status**: Decided

---

## Assembly Layering

```
Fram3d.Core      — pure C#, no dependencies
Fram3d.Services  — pure C#  → Core
Fram3d.Engine    — Unity    → Services, Core
Fram3d.UI        — Unity    → Engine, Services, Core
```

Four assemblies. Each layer references only downward.

1. **Core** — pure C#, the domain and everything it needs. Can't touch Unity.
2. **Services** — pure C#, cross-domain orchestration (AI, serialization, export, import). Can't touch Unity.
3. **Engine** — Unity integration (MonoBehaviours, evaluation pipeline, asset loading, rendering, 3D scene interaction). Can't reference UI.
4. **UI** — Unity presentation (UI Toolkit panels, overlays, 2D input). Top of the stack.

---

## `Fram3d.Core`

Pure C#, no dependencies, testable with xUnit. The domain and everything it needs. Organized by namespace.

### `Fram3d.Core.Common`

Shared foundation. Referenced by every other namespace within Core. References nothing within Core.

**Elements:**
- `Element` — base class: `ElementId`, name, `Vector3` position, `Quaternion` rotation, `float` scale, `float` BoundingRadius (System.Numerics)
- `ElementId` — identity wrapper (GUID, rejects empty)
- `ShotId` — identity wrapper (GUID, rejects empty)
- `KeyframeId` — identity wrapper (GUID, rejects empty)
- `ElementRegistry` — collection of all elements with typed queries and `IObservable` change streams

**Undo / Redo:**
- `ICommand` / `CommandStack` / `CompoundCommand` — command pattern. CompoundCommand holds `ICommand[]` — generic, no domain knowledge.

**Registries:**
- `Registry<T>` — generic typed collection with add/remove/query and `IObservable` change streams
- `IObservable<T>` support types (`Subject<T>`, etc. if needed beyond BCL)

**Value objects:**
- `TimePosition` — non-negative time, arithmetic with clamping, `ToFrames(fps)`
- `FrameRate` — frames per second, common presets
- `IInterpolatable<T>` — interface for domain types that can lerp between values

**Project & settings:**
- `Project` — root aggregate: name, save path, frame rate, default camera body name (string), default lens set name (string), `List<SceneId>` scene identifiers, `List<ElementId>` project-level character IDs. Stores IDs and primitives only — no Camera/Character imports.
- `OverlaySettings` — aspect ratio selection, guide visibility, camera info visibility, camera path visibility
- `ViewSettings` — layout choice, per-panel view type assignments
- `DirtyStateTracker` — integrates with CommandStack, tracks save point

**What code does here:** Identity, validation, collections, undo/redo. The foundation everything else builds on. No domain logic.

### `Fram3d.Core.Timeline`

Generic animation data model. References only Common.

- `Keyframe<T>` — time + value + interpolation curve
- `KeyframeManager<T>` — dual storage (sorted list + dictionary), add/remove/query by time/id
- `InterpolationCurve` — linear, ease-in, ease-out, ease-in-out, bezier with control points
- `Track` / `SubTrack` — named lane containing a `KeyframeManager`
- `GlobalTimeline` — maps ShotIds + durations to time ranges, computes global time with speed factor
- `Playhead` — current time, playback state, scrub
- `StopwatchState` — per-track recording toggle
- `AddKeyframeCommand`, `DeleteKeyframeCommand`, `MoveKeyframeCommand`, `ChangeInterpolationCommand`

**What code does here:** "Given keyframes and time T, what's the interpolated value?" Generic evaluation. Doesn't know what T represents — a position, a focal length, or a pose. Domain types implement `IInterpolatable<T>`.

### `Fram3d.Core.Assets`

Asset metadata. References only Common.

- `AssetEntry` — name, source (built-in/imported/marketplace), file size, thumbnail reference
- `AssetLibrary` — collection with search by name, filter by category, `IObservable` streams
- `AssetCategory` — enum: Furniture, Architectural, Vehicles, Exterior, Interior, Structural
- `BuiltInAssetLibrary` — curated set shipped with app
- `MarketplaceAssetEntry : AssetEntry` — author, free/paid, download URL
- `UserAssetCollection` — named grouping with tags, favorites
- `AssetSearchEngine` — search by name + tags, filter by category + collection
- `EnvironmentTemplate` — element list, lighting setup, wall layout
- `EnvironmentLibrary` — premade + user-saved environments

### `Fram3d.Core.Camera`

Camera rig, lens, focus, DOF, shake, follow, watch, snorricam. References Common and Timeline.

- `CameraElement : Element` — the camera IS the rig. Properties AND operations on one type:
  - Properties: focal length, aperture, focus distance, DOF settings, shake settings, camera body, lens set
  - Movement: `Dolly()`, `Pan()`, `Tilt()`, `Crane()`, `Truck()`, `Roll()`, `Orbit()`, `DollyZoom()`, `FocusOn()`, `Reset()`
  - Follow: `Follow(ElementId target, ...)`, `StopFollowing()`, follow parameters (distance, height, lateral, damping, lead/trail)
  - Watch: `Watch(ElementId target)`, `StopWatching()`
  - Snorricam: `MountSnorricam(ElementId character, SnorricamMount mount)`, `DismountSnorricam()`
  - Internally delegates to private helpers: `ShakeGenerator`, `LensSystem`, `FollowEvaluator`, `WatchEvaluator`, `SnorricamEvaluator`
- `FocalLength` — value object, 14–400mm
- `LensSystem` *(private/internal)* — FOV from focal length + sensor dimensions, anamorphic squeeze
- `CameraBody` — sensor width/height, resolution, frame rates, mount
- `LensSet` — focal lengths, prime vs zoom
- `CameraLensDatabase` — queries camera bodies and lens sets
- `DepthOfFieldSettings` — aperture, focus distance, enabled
- `ShakeGenerator` *(private/internal)* — Perlin noise rotation offset
- `AspectRatio` — value (ratio float + display name)
- `AspectRatioSet` — ordered list, cycle forward/backward
- `OrbitPivot` — persistent last-focused element
- `DirectorCamera` — free camera position/rotation, not linked to any shot, not serialized
- `MoveCameraCommand`, `ChangeFocalLengthCommand`, `ResetCameraCommand`, `ActivateFollowCommand`, `ActivateWatchCommand`, `ActivateSnorricamCommand`, etc.

### `Fram3d.Core.Character`

Posing, skeleton, expressions, locomotion. References Common and Timeline.
Public API on `CharacterElement`. Internals (`Skeleton`, `Joint`, `IKSolver`, `IKChain`) are `internal` — Camera and Scene access bones through CharacterElement's public methods only.

- `CharacterElement : Element` — public API:
  - Properties: body type, tint color, expression state, eye direction
  - Bone access: `GetBoneWorldPosition(string boneName)`, `GetBoneWorldRotation(string boneName)`, `RootMotionDelta`
  - Posing: `ApplyPose(Pose pose)`, `SetJointRotation(string joint, Quaternion rotation)`
- `BodyType` — gender, height (155–195cm), build (slim–heavy)
- `Skeleton` *(internal)* — bone hierarchy (16 joints), parent-child
- `Joint` *(internal)* — rotation, limits, parent
- `Pose` — set of joint rotations. Implements `IInterpolatable<Pose>`.
- `PoseLibrary` — built-in + custom poses, categories
- `PoseCategory` — Standing, Sitting, Walking, Running, Conversation, Pointing, LyingDown, Action, Emotional
- `IKSolver` *(internal)* — connected joint solving
- `IKChain` *(internal)* — shoulder→elbow→wrist or hip→knee→ankle
- `Expression` — blend shape weight set, presets + custom. Implements `IInterpolatable<Expression>`.
- `BlendShape` — individual deformation target
- `EyeDirection` — look direction controls
- `WalkCycle` — pre-baked locomotion clips
- `GaitPreset` — parameterized gait variant
- `Costume` — mesh reference, bone binding regions
- `CostumeLibrary` — saved costumes, cross-project
- `ApplyPoseCommand`, `SetJointRotationCommand`, `ChangeBodyTypeCommand`, `ApplyExpressionCommand`, etc.

### `Fram3d.Core.Shot`

Shot structure and multi-camera. References Common, Timeline, and Camera.

- `Shot` — name, duration, speed factor, time range on global timeline, camera animations per angle, `List<Subtitle>`
- `ShotRegistry` — ordered collection with queries and `IObservable` streams
- `Angle` — one camera's perspective within a shot, references `CameraElement`
- `ActiveAngleTrack` — which camera is on air at each moment
- `AngleSegment` — camera index, start time, end time
- `Subtitle` — text, start time, end time, color, size, font
- `AddShotCommand`, `DeleteShotCommand`, `ReorderShotCommand`, `ChangeShotDurationCommand`, `RenameShotCommand`, `AddCameraToShotCommand`, `SplitAngleCommand`, `MultiSplitCommand`, `ChangeSpeedFactorCommand`

### `Fram3d.Core.Scene`

Selection, lighting, linking, grouping, set building. References Common, Timeline, and Character.

- `LightElement : Element` — light type (directional/point/spot), intensity, color (RGB + Kelvin), range, cone angle, inner cone angle
- `ColorTemperature` — value object: Kelvin ↔ RGB conversion
- `Selection` — selected element(s), hover element
- `ActiveTool` — enum: Select, Translate, Rotate, Scale
- `LinkEvent` — child ElementId, parent ElementId, time, link/unlink
- `LinkChain` — ordered list of link events on global timeline
- `LinkChainEvaluator` — resolves chains in dependency order, computes child transforms. Calls `CharacterElement.GetBoneWorldPosition()` for character links.
- `AnchorPoint` — XYZ offset on child element
- `Group` — named collection of ElementIds, shared transform
- `GroupManager` — create/dissolve, enter/exit editing
- `GroundPlane` — Y=0 reference (data only)
- `SnapSettings` — grid size presets, rotation/scale increments, enabled
- `SnapController` — snaps values during gizmo drags
- `Wall` — segment: start/end points, height, thickness, materials per side
- `WallChain` — connected wall segments
- `DoorCutout` / `WindowCutout` — rectangular holes with dimensions
- `RoomPreset` — rectangle, L-shape, T-shape, open
- `ScanBackdrop` — display-only reference: file path, position, rotation, visibility
- `MoveElementCommand`, `RotateElementCommand`, `ScaleElementCommand`, `DuplicateElementCommand`, `AddLightCommand`, `ChangeLightPropertyCommand`, `CreateLinkCommand`, `CreateGroupCommand`, `AddWallCommand`, etc.

**Namespace dependency summary (intra-Core):**

| Namespace | References |
|-----------|-----------|
| Common | nothing within Core |
| Timeline | Common |
| Assets | Common |
| Camera | Common, Timeline |
| Character | Common, Timeline |
| Shot | Common, Timeline, Camera |
| Scene | Common, Timeline, Character |

DAG. No cycles. Camera and Character are siblings. Shot and Scene each have one domain dependency beyond the foundation.

---

## `Fram3d.Services`

Cross-domain orchestration. Pure C#, references Core. Four independent namespaces — they do not reference each other.

### `Fram3d.Services.AI`

- `ShotFromText` — parse NL shot description → camera parameters
- `ShotVocabulary` — shot size/angle/spatial mappings and aliases
- `SceneFromText` — parse scene description → place characters and elements
- `BlockingRefinement` — incremental text adjustments, pronoun resolution
- `CoverageSuggester` — analyze character positions → standard coverage patterns
- `ShotListGenerator` — chain blocking + camera placement + coverage

### `Fram3d.Services.Serialization`

- `ProjectSerializer` — full project state (JSON)
- `SceneSerializer` — per-scene for lazy loading
- `VersionMigrator` — older format → current, fill defaults
- `AssetBundler` — embed vs reference, size estimation + migration warning

### `Fram3d.Services.Export`

- `EDLWriter` — CMX 3600 format
- `StoryboardLayout` — grid computation, pagination
- `CameraMetadataWriter` — per-frame sidecar
- `ShakeBaker` / `FollowBaker` / `SnorricamBaker` — procedural → keyframes

### `Fram3d.Services.ScriptImport`

- `FountainParser` — .fountain format
- `FinalDraftParser` — .fdx XML format
- `ScriptToSceneMapper` — headings → scenes, names → characters, dialogue → library

---

## `Fram3d.Engine`

Engine integration. Bridges pure C# domain to Unity. Can't reference UI.

### `Fram3d.Engine.Integration`

MonoBehaviour wrappers and 3D scene interaction.

- `ElementBehaviour` — bridges `Element` ↔ Unity `Transform` + `GameObject`
- `CameraBehaviour` — bridges `CameraElement` ↔ Unity `Camera` (physical mode) + URP post-processing Volume for DOF
- `CharacterBehaviour` — bridges `CharacterElement` ↔ Unity skinned mesh, bone transforms
- `LightBehaviour` — bridges `LightElement` ↔ Unity `Light` component
- `DirectorCameraBehaviour` — free camera, renders shot camera as frustum wireframe
- `WallBehaviour` — bridges Wall data to Unity mesh
- `ScanBackdropBehaviour` — renders imported scan mesh
- Conversion extension methods: `System.Numerics.Vector3` ↔ `UnityEngine.Vector3`, etc.
- `GizmoController` — renders 3D translate/rotate/scale handles, handles drag, creates commands
- `SelectionRaycaster` — raycasts from mouse to find element, writes to Selection
- `SelectionHighlighter` — renders selection/hover outlines
- `MarqueeRaycaster` — projects screen rectangle into world, tests element bounds
- `GroundPlaneRenderer` — infinite grid, fade with distance
- `SnapGridRenderer` — visual grid matching snap settings
- `CameraPathRenderer` — 3D spline with frustum indicators at keyframe positions
- `LightIconRenderer` — constant-size type icons (arrow/sphere/cone)
- `ConeWireframeRenderer` — spot light cone, visible when selected

### `Fram3d.Engine.Evaluation`

Per-frame orchestration.

- `SceneEvaluator` — runs in `LateUpdate`: reads Playhead → evaluates all tracks → evaluates link chains → evaluates character poses → evaluates camera (follow/watch/shake) → pushes transforms to Unity. Calls into pure C# domain code, writes results to Unity scene graph. References all Core namespaces.

### `Fram3d.Engine.Assets`

Runtime asset loading.

- `RuntimeModelImporter` — FBX/OBJ/glTF parsing, mesh creation, collider generation
- `ThumbnailRenderer` — 3/4 angle preview captures
- `ShotThumbnailRenderer` — captures frame at t=0 for shot previews
- `AssetPlacementController` — places at camera look-point on ground plane
- `EnvironmentLoader` — instantiates elements from template
- `CharacterMeshGenerator` — creates mesh from body type parameters
- `BlendShapeController` — applies expression weights to Unity blend shapes
- `WallMeshGenerator` — extrudes wall segments into 3D geometry with cutouts
- `MarketplaceClient` — HTTP for marketplace APIs
- `AssetDownloadManager` — progress, caching, cancellation
- `AssetFormatConverter` — normalizes downloaded assets
- `LiDARImporter` — .ply/.obj/.usdz point cloud/mesh
- `CostumeRenderer` — auto-binds generated mesh to skeleton
- `GeneratedMeshImporter` — converts AI API response to Unity mesh

### `Fram3d.Engine.Rendering`

Offline rendering and export.

- `OfflineFrameRenderer` — captures frames at target resolution, decoupled from real-time
- `VideoEncoder` — MP4/MOV output
- `StoryboardRenderer` — thumbnail grid to PDF/PNG (uses Services.Export.StoryboardLayout for layout)
- `ImageExporter` — single frame PNG/JPEG with optional overlays
- `CameraViewRenderer` / `DirectorViewRenderer` / `DesignerViewRenderer` — per-view-type render cameras

---

## `Fram3d.UI`

All presentation. UI Toolkit. Top of the stack — references Engine, Services, and Core.

### `Fram3d.UI.Timeline`

- `TimelineEditorView` — ruler, transport bar, track area container
- `TimeRulerView` — tick marks, adaptive intervals
- `TransportBarView` — play/pause, time display, shot name
- `ShotTrackView` — shot blocks, drag-and-drop, boundary dragging with ripple/shift
- `ShotHoverTooltip` — shot name, camera, duration, keyframe count
- `ActiveAngleTrackView` — colored segments, cut point dragging
- `TrackView` — renders track row with keyframe markers
- `SubTrackView` — expanded property row with interpolated value labels and curve indicator symbols
- `KeyframeMarkerView` — diamond (linear), circle (smooth), square (hold)
- `CurveEditorView` — inline bezier handle editor below track
- `OverviewStripView` — bird's-eye minimap of full timeline
- `CameraPreviewView` — preview thumbnails above shot in shot track

### `Fram3d.UI.Panels`

- `ElementsPanelView` — flat list with typeswitch icons, link indicators, lights section
- `PropertiesPanelView` — contextual sidebar, content changes by selection type (element/camera/character/light)
- `AssetsPanelView` — library grid, search, category filter, drag-to-place
- `PoseLibraryPanelView` — thumbnail grid, categories, search, custom pose save
- `GutterView` — JetBrains-style vertical toggle tabs
- `MarketplaceTabView` — search, browse, preview, download marketplace assets
- `CollectionManagerView` — create/rename/delete asset collections
- `ScriptImportDialogView` — detected scenes/characters, per-scene environment picker
- `MultiSplitDialogView` — "split every N frames" prompt

### `Fram3d.UI.Views`

- `ViewLayoutManager` — single, side-by-side, three-view arrangements
- `ViewPanel` — individual view container with dropdown selector
- `ViewLayoutChooser` — bottom-right layout buttons
- `AspectRatioMaskRenderer` — black bars based on ratio + view dimensions
- `CompositionGuideRenderer` — thirds, center cross, safe zones
- `CameraInfoOverlay` — focal length, height, AOV, body, lens, aperture
- `SubtitleRenderer` — text at bottom-center of unmasked area
- `DirectorViewBadge` — "DIRECTOR VIEW" pink-red pill
- `CameraPathBadge` — "PATH" amber indicator
- `ActiveToolBadge` — current tool icon + shortcut
- `DesignerViewOverlay` — element labels, camera preview inset

### `Fram3d.UI.Input`

Keyboard shortcuts and 2D screen-space interaction only. 3D scene interaction (raycasting, gizmo drags) is in Engine.Integration.

- `KeyboardShortcutHandler` — maps keys to commands (QWER tools, Space play, A/G/H/P/D overlays, etc.)
- `CameraInputHandler` — maps mouse + modifier keys to CameraElement methods
- `TimelineInputHandler` — timeline/shot track mouse interaction (click keyframes, drag to reposition, scrub)
- `MarqueeSelectionView` — draws dashed rectangle during drag (2D overlay)

### `Fram3d.UI.SetBuilder`

- `SetBuilderPageView` — full-screen workspace for environment construction
- `WallDrawingToolView` — click-to-place wall segments in 2D overhead
- `RoomWizardView` — guided flow
- `MaterialPickerView` — wall/floor/ceiling presets
- `LightingPresetPickerView` — daytime/night/overhead/single/exterior
