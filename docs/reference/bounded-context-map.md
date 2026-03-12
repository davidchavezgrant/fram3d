# Bounded Context Map

Every milestone and feature mapped to its primary bounded context (Assembly Definition). See `fram3d-architecture.md` section 12 for the domain modeling approach.

## Context Summary

| Context | Assembly | Description |
|---------|----------|-------------|
| Camera | `Fram3d.Camera` | Camera rig, lens, focus, shake, DOF, overlays, follow/watch, snorricam, multi-camera |
| Sequencing | `Fram3d.Sequencing` | Shot model, keyframe timeline, tracks, playback, slow-motion |
| Scene | `Fram3d.Scene` | Scene elements, selection, gizmos, lighting, element linking, set builder |
| Viewport | `Fram3d.Viewport` | Panel layouts, views, Designer View |
| Characters | `Fram3d.Characters` | Mannequins, posing, expressions, costume generation |
| Persistence | `Fram3d.Persistence` | Undo/redo, save/load, project files, script import |
| Assets | `Fram3d.Assets` | Model import, asset library, set decoration, premade environments, LiDAR |
| Export | `Fram3d.Export` | Image, video, storyboard, NLE export, style grading |
| AI | `Fram3d.AI` | NL shot description, automatic blocking, camera suggestions, prop generation |

## Feature Map

| # | Feature | Context |
|---|---------|---------|
| **1.1** | **Virtual camera** | **Camera** |
| 1.1.1 | Camera movement | Camera |
| 1.1.2 | Lens system | Camera |
| 1.1.3 | Camera body and lens database | Camera |
| 1.1.4 | Focus | Camera |
| 1.1.5 | Depth of field preview | Camera |
| 1.1.6 | Camera shake | Camera |
| **1.2** | **Camera overlays** | **Camera** |
| 1.2.1 | Aspect ratio masks | Camera |
| 1.2.2 | Composition guides | Camera |
| 1.2.3 | Camera info | Camera |
| 1.2.4 | Subtitle overlay | Camera |
| **2.1** | **Scene management** | **Scene** |
| 2.1.1 | Scene elements | Scene |
| 2.1.2 | Transform gizmos | Scene |
| 2.1.3 | Ground plane | Scene |
| 2.1.4 | Element duplication | Scene |
| 2.1.5 | Director view | Scene |
| **2.2** | **Viewport panel system** | **Viewport** |
| 2.2.1 | Panel layouts | Viewport |
| 2.2.2 | Views | Viewport |
| **3.1** | **Shot structure** | **Sequencing** |
| 3.1.1 | Shot model | Sequencing |
| 3.1.2 | Shot track UI | Sequencing |
| 3.1.3 | Element continuity — global element timeline | Sequencing |
| 3.1.4 | Multi-scene project structure | Sequencing |
| 3.1.5 | Timeline overview | Sequencing |
| **3.2** | **Keyframe animation** | **Sequencing** |
| 3.2.1 | Timeline editor | Sequencing |
| 3.2.2 | Tracks and keyframes | Sequencing |
| 3.2.3 | Per-track stopwatch | Sequencing |
| 3.2.4 | Keyframe interaction | Sequencing |
| 3.2.5 | Interpolation and playback | Sequencing |
| 3.2.6 | Path visualization | Sequencing |
| **4.1** | **Undo / Redo** | **Persistence** |
| 4.1.1 | Undo stack | Persistence |
| **4.2** | **Save / Load** | **Persistence** |
| 4.2.1 | Project creation wizard | Persistence |
| 4.2.2 | Project file format | Persistence |
| 4.2.3 | Save / Load UI | Persistence |
| 4.2.4 | Dirty state tracking | Persistence |
| 4.2.5 | Scene persistence | Persistence |
| **4.3** | **Asset import** | **Assets** |
| 4.3.1 | Model import | Assets |
| 4.3.2 | Asset library | Assets |
| **4.4** | **Export** | **Export** |
| 4.4.1 | Image export | Export |
| 4.4.2 | Video export | Export |
| 4.4.3 | Storyboard export | Export |
| 4.4.4 | NLE export | Export |
| **5.1** | **Lighting** | **Scene** |
| 5.1.1 | Light types | Scene |
| 5.1.2 | Light properties | Scene |
| 5.1.3 | Light animation | Scene |
| **5.2** | **Set decoration library** | **Assets** |
| 5.2.1 | Built-in asset library | Assets |
| 5.2.2 | Marketplace integration | Assets |
| 5.2.3 | User asset management | Assets |
| **5.3** | **Premade environments** | **Assets** |
| 5.3.1 | Environment library | Assets |
| **6.1** | **Characters** | **Characters** |
| 6.1.1 | Mannequin placement and customization | Characters |
| 6.1.2 | Pose library | Characters |
| 6.1.3 | Custom posing | Characters |
| 6.1.4 | Character animation | Characters |
| 6.1.5 | Custom character import | Characters |
| **6.2** | **Camera follow and watch** | **Camera** |
| 6.2.1 | Camera follow | Camera |
| 6.2.2 | Watch | Camera |
| **6.3** | **Element linking & grouping** | **Scene** |
| 6.3.1 | Element linking | Scene |
| 6.3.2 | Element grouping | Scene |
| 6.3.3 | Elements panel | Scene |
| **7.1** | **Facial expressions** | **Characters** |
| 7.1.1 | Expression system | Characters |
| 7.1.2 | Eye direction | Characters |
| 7.1.3 | Expression animation | Characters |
| **7.2** | **Snorricam** | **Camera** |
| 7.2.1 | Snorricam | Camera |
| **8.1** | **Selection and manipulation refinements** | **Scene** |
| 8.1.1 | Multi-select | Scene |
| 8.1.2 | Grid snapping | Scene |
| 8.1.3 | Custom interpolation curves | Sequencing |
| **8.2** | **Designer View** | **Viewport** |
| 8.2.1 | Designer View | Viewport |
| **8.3** | **Script import** | **Persistence** |
| 8.3.1 | Script parsing | Persistence |
| **8.4** | **Slow-motion** | **Sequencing** |
| 8.4.1 | Slow-motion | Sequencing |
| **9.1** | **Multi-camera** | **Camera** |
| 9.1.1 | Per-shot camera addition | Camera |
| 9.1.2 | Multi-camera timelines | Camera |
| 9.1.3 | Active camera and switching | Camera |
| 9.1.4 | Active angle editing | Camera |
| 9.1.5 | Multi-split | Camera |
| **10.1** | **Set builder** | **Scene** |
| 10.1.1 | Room construction | Scene |
| 10.1.2 | Wall drawing | Scene |
| **11.1** | **Natural language shot description** | **AI** |
| 11.1.1 | Shot-from-text | AI |
| 11.1.2 | Shot vocabulary | AI |
| **11.2** | **Automatic blocking** | **AI** |
| 11.2.1 | Scene-from-text | AI |
| 11.2.2 | Blocking refinement | AI |
| **11.3** | **Camera suggestions** | **AI** |
| 11.3.1 | Coverage suggestions | AI |
| 11.3.2 | Shot list generation | AI |
| **12.1** | **AI prop generation** | **AI** |
| 12.1.1 | Text-to-prop | AI |
| **12.2** | **Costume generation** | **Characters** |
| 12.2.1 | AI costume generation | Characters |
| **12.3** | **LiDAR scanning** | **Assets** |
| 12.3.1 | LiDAR import | Assets |
| 12.3.2 | Companion iOS app | Assets |
| **12.4** | **Style grading** | **Export** |
| 12.4.1 | Frame style grading | Export |
| **12.5** | **AI video generation** | **AI** |
