# Assembly & Namespace Map

Every milestone and feature mapped to its assembly and namespace. See `domain-model.md` for the modeling approach and `design-sessions/01` for design rationale.

## Assembly Summary

| Assembly | Layer | Description |
|----------|-------|-------------|
| `Fram3d.Core` | Domain (pure C#) | The 3D world: camera, characters, scene, shots, timeline, assets, commands, registries |
| `Fram3d.Services` | Orchestration (pure C#) | Cross-domain: AI, serialization, export logic, script import |
| `Fram3d.Engine` | Integration (Unity) | MonoBehaviours, evaluation pipeline, asset loading, rendering, 3D scene interaction |
| `Fram3d.UI` | Presentation (Unity) | UI Toolkit panels, overlays, 2D input, set builder page |

## Core Namespace Summary

| Namespace | Description |
|-----------|-------------|
| `Core.Common` | Element base class, identity types (ElementId, ShotId, KeyframeId), ICommand/CommandStack, registries, value objects (TimePosition, FrameRate), Project, settings |
| `Core.Timeline` | Keyframe\<T\>, KeyframeManager\<T\>, Track, InterpolationCurve, GlobalTimeline, Playhead, StopwatchState |
| `Core.Assets` | AssetEntry, AssetLibrary, EnvironmentTemplate, EnvironmentLibrary |
| `Core.Camera` | CameraElement, FocalLength, LensSystem, CameraBody, LensSet, ShakeGenerator, AspectRatio, FollowState, WatchState, SnorricamState, DirectorCamera |
| `Core.Character` | CharacterElement, Skeleton (internal), Pose, PoseLibrary, IKSolver (internal), Expression, WalkCycle, Costume |
| `Core.Shot` | Shot, Angle, ActiveAngleTrack, AngleSegment, Subtitle, ShotRegistry |
| `Core.Scene` | LightElement, Selection, LinkChain, Group, SnapSettings, Wall, GroundPlane, ColorTemperature |

## Feature Map

Each feature maps to a Core namespace (domain data/logic), plus Engine/UI namespaces for integration and presentation.

| # | Feature | Core namespace | Services | Engine | UI |
|---|---------|---------------|----------|--------|-----|
| **1.1** | **Virtual camera** | **Camera** | | Integration | Input |
| 1.1.1 | Camera movement | Camera | | Integration | Input |
| 1.1.2 | Lens system | Camera | | Integration | |
| 1.1.3 | Camera body and lens database | Camera | | | |
| 1.1.4 | Focus | Camera | | Integration | |
| 1.1.5 | Depth of field preview | Camera | | Integration | |
| 1.1.6 | Camera shake | Camera | | Evaluation | |
| **1.2** | **Camera overlays** | **Camera, Common** | | | Views |
| 1.2.1 | Aspect ratio masks | Camera | | | Views |
| 1.2.2 | Composition guides | Common | | | Views |
| 1.2.3 | Camera info | — | | | Views |
| 1.2.4 | Subtitle overlay | Shot | | | Views |
| **2.1** | **Scene management** | **Scene, Common** | | Integration | |
| 2.1.1 | Scene elements | Common | | Integration | |
| 2.1.2 | Transform gizmos | Scene | | Integration | |
| 2.1.3 | Ground plane | Scene | | Integration | |
| 2.1.4 | Element duplication | Common | | | |
| 2.1.5 | Director view | Camera | | Integration | Views |
| **2.2** | **View system** | **Common** | | Integration | Views |
| 2.2.1 | View layouts | Common | | Integration | Views |
| 2.2.2 | Views | Common | | Integration | Views |
| **3.1** | **Shot structure** | **Shot, Timeline** | | Rendering | Timeline |
| 3.1.1 | Shot model | Shot | | | |
| 3.1.2 | Shot track UI | — | | Rendering | Timeline |
| 3.1.3 | Element continuity — global timeline | Timeline | | | |
| **3.2** | **Keyframe animation** | **Timeline** | | Evaluation | Timeline |
| 3.2.1 | Timeline editor | — | | | Timeline |
| 3.2.2 | Tracks and keyframes | Timeline | | | Timeline |
| 3.2.3 | Per-track stopwatch | Timeline | | | |
| 3.2.4 | Keyframe interaction | Timeline | | | Timeline |
| 3.2.5 | Interpolation and playback | Timeline | | Evaluation | |
| 3.2.6 | Path visualization | — | | Integration | |
| **4.1** | **Undo / Redo** | **Common** | | | |
| 4.1.1 | Undo stack | Common | | | |
| **4.2** | **Save / Load** | **Common** | Serialization | | Panels |
| 4.2.1 | Project creation wizard | Common | Serialization | | Panels |
| 4.2.2 | Project file format | — | Serialization | | |
| 4.2.3 | Save / Load UI | Common | Serialization | | Panels |
| 4.2.4 | Dirty state tracking | Common | | | |
| 4.2.5 | Scene persistence | — | Serialization | | |
| **4.3** | **Asset import** | **Assets** | | Assets | Panels |
| 4.3.1 | Model import | Assets | | Assets | |
| 4.3.2 | Assets panel | Assets | | | Panels |
| **4.4** | **Export** | — | Export | Rendering | |
| 4.4.1 | Image export | — | Export | Rendering | |
| 4.4.2 | Video export | — | Export | Rendering | |
| 4.4.3 | Storyboard export | — | Export | Rendering | |
| 4.4.4 | NLE export | — | Export | Rendering | |
| **5.1** | **Lighting** | **Scene** | | Integration | |
| 5.1.1 | Light types | Scene | | Integration | |
| 5.1.2 | Light properties | Scene | | Integration | Panels |
| 5.1.3 | Light animation | Scene, Timeline | | Evaluation | Timeline |
| **5.2** | **Set decoration library** | **Assets** | | Assets | Panels |
| 5.2.1 | Built-in asset library | Assets | | Assets | Panels |
| 5.2.2 | Marketplace integration | Assets | | Assets | Panels |
| 5.2.3 | User asset management | Assets | | | Panels |
| **5.3** | **Premade environments** | **Assets** | | Assets | |
| 5.3.1 | Environment library | Assets | | Assets | |
| **6.1** | **Characters** | **Character** | | Integration, Assets | Panels |
| 6.1.1 | Mannequin placement and customization | Character | | Assets | Panels |
| 6.1.2 | Pose library | Character | | | Panels |
| 6.1.3 | Custom posing | Character | | Integration | |
| 6.1.4 | Character animation | Character, Timeline | | Evaluation | Timeline |
| 6.1.5 | Custom character import | Character | | Assets | |
| **6.2** | **Camera follow and watch** | **Camera** | | Evaluation | |
| 6.2.1 | Camera follow | Camera | | Evaluation | |
| 6.2.2 | Watch | Camera | | Evaluation | |
| **6.3** | **Element linking & grouping** | **Scene** | | Integration | Panels |
| 6.3.1 | Element linking | Scene | | Integration | |
| 6.3.2 | Element grouping | Scene | | Integration | |
| 6.3.3 | Elements panel | — | | | Panels |
| **7.1** | **Facial expressions** | **Character** | | Integration | Panels, Timeline |
| 7.1.1 | Expression system | Character | | Integration | Panels |
| 7.1.2 | Eye direction | Character | | Integration | Panels |
| 7.1.3 | Expression animation | Character, Timeline | | Evaluation | Timeline |
| **7.2** | **Snorricam** | **Camera** | | Evaluation | |
| 7.2.1 | Snorricam | Camera | | Evaluation | |
| **8.1** | **Selection refinements** | **Scene, Timeline** | | Integration | Timeline, Input |
| 8.1.1 | Multi-select | Scene | | Integration | Input |
| 8.1.2 | Grid snapping | Scene | | Integration | |
| 8.1.3 | Custom interpolation curves | Timeline | | | Timeline |
| **8.2** | **Designer View** | — | | Integration | Views |
| 8.2.1 | Designer View | — | | Integration | Views |
| **8.3** | **Script import** | — | ScriptImport | | Panels |
| 8.3.1 | Script parsing | — | ScriptImport | | Panels |
| **8.4** | **Slow-motion** | **Shot** | | | |
| 8.4.1 | Slow-motion | Shot | | | |
| **9.1** | **Multi-camera** | **Shot, Camera** | | | Timeline |
| 9.1.1 | Per-shot camera addition | Shot | | | |
| 9.1.2 | Multi-camera timelines | Shot | | | Timeline |
| 9.1.3 | Active camera and switching | Shot | | | Timeline |
| 9.1.4 | Active angle editing | Shot | | | Timeline |
| 9.1.5 | Multi-split | Shot | | | Timeline |
| **10.1** | **Set builder** | **Scene** | | Integration | SetBuilder |
| 10.1.1 | Room construction | Scene | | Integration | SetBuilder |
| 10.1.2 | Wall drawing | Scene | | Integration | SetBuilder |
| **11.1** | **NL shot description** | — | AI | | |
| 11.1.1 | Shot-from-text | — | AI | | |
| 11.1.2 | Shot vocabulary | — | AI | | |
| **11.2** | **Automatic blocking** | — | AI | | |
| 11.2.1 | Scene-from-text | — | AI | | |
| 11.2.2 | Blocking refinement | — | AI | | |
| **11.3** | **Camera suggestions** | — | AI | | |
| 11.3.1 | Coverage suggestions | — | AI | | |
| 11.3.2 | Shot list generation | — | AI | | |
| **12.1** | **AI prop generation** | **Assets** | AI | Assets | |
| 12.1.1 | Text-to-prop | Assets | AI | Assets | |
| **12.2** | **Costume generation** | **Character** | AI | Integration | |
| 12.2.1 | AI costume generation | Character | AI | Integration | |
| **12.3** | **LiDAR scanning** | **Scene** | | Assets | |
| 12.3.1 | LiDAR import | Scene | | Assets | |
| 12.3.2 | Companion iOS app | — | | — | — |
| **12.4** | **Style grading** | — | Export | Rendering | |
| 12.4.1 | Frame style grading | — | Export | Rendering | |
| **12.5** | **AI video generation** | — | AI | Rendering | |
