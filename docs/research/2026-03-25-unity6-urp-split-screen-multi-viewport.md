# Raw Research Findings: Split-Screen / Multi-Viewport Cameras in Unity 6 URP

## Queries Executed
1. "Unity 6 URP split screen multiple cameras Camera.rect viewport" - 3 useful results
2. "URP camera stacking overlay camera viewport rect split screen" - 3 useful results
3. "Unity URP Volume post-processing per camera scope layer mask" - 3 useful results
4. "Unity DefaultExecutionOrder LateUpdate script execution order" - 4 useful results
5. "Unity 6 URP Camera.rect RenderTexture viewport issues bugs 2025 2026" - 3 useful results
6. "Unity URP RenderTexture camera split screen approach advantages disadvantages performance" - 2 useful results
7. "Unity URP overlay camera inherits base camera viewport rect properties" - 2 useful results
8. "Unity URP UniversalAdditionalCameraData volumeLayerMask volumeMask API scripting" - 2 useful results

## Findings

### Finding 1: Camera.rect works for split-screen with multiple Base Cameras in URP
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Manual: Set up split-screen rendering in URP](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/rendering-to-the-same-render-target.html) - Official Unity 6.3 docs. Create two Base Cameras, set Viewport Rect on each (e.g., X:0 Y:0 W:0.5 H:1 for left, X:0.5 Y:0 W:0.5 H:1 for right). "Multiple Base Cameras or Camera Stacks can render to the same render target."
  - [URP Camera Component Reference v16](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@16.0/manual/camera-component-reference.html) - Confirms Viewport Rect is a Base Camera output property.
  - [Use multiple cameras (URP 14)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/cameras-multiple.html) - "You can define two camera stacks, and then set each of those camera stacks to render to a different area of the same render target."
- **Notes**: This is the officially documented approach. Can set via script: `myUniversalAdditionalCameraData.rect = new Rect(0.5f, 0f, 0.5f, 0f)`. When cameras overlap, the highest-priority camera draws last (on top).

### Finding 2: Overlay cameras CANNOT use Viewport Rect -- they always render fullscreen within their Base Camera's viewport
- **Confidence**: HIGH
- **Supporting sources**:
  - [URP Camera Component Reference v16](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@16.0/manual/camera-component-reference.html) - Overlay cameras do not have Output section (no viewport rect, target display, HDR, MSAA, dynamic resolution). Only Base cameras control these. Overlay camera properties are limited to: Projection, Physical Camera, Rendering, Environment.
  - [Camera render types introduction (Unity 6.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/camera-types-and-render-type-introduction.html) - "The Base Camera determines most Camera Stack properties." Overlay cameras' unused properties are hidden in Inspector. Can be accessed via script but "will not affect the visual output."
  - [Unity Discussions: Why can't overlay cameras use viewport rect?](https://discussions.unity.com/t/why-cant-overlay-cameras-use-viewport-rect/798667) - Community confirmation of this limitation (page returned 403 so details not extracted, but search snippet confirmed the topic).
- **Notes**: This is by design, not a bug. Overlay cameras composite on top of the Base Camera's output, inheriting the Base Camera's viewport. For split-screen with camera stacks, each stack needs its own Base Camera with the appropriate Viewport Rect -- the overlay cameras in each stack will automatically render within that viewport region.

### Finding 3: Overlay cameras inherit the Base Camera's viewport but render their own content within it
- **Confidence**: MEDIUM
- **Supporting sources**:
  - [Camera stacking concepts (Unity 6.0)](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/cameras/camera-stacking-concepts.html) - "Camera stacking overrides the output of the Base Camera with the combined output of all the cameras." The overlay renders on top of the base within the same render target region.
  - [URP Camera Component Reference v16](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@16.0/manual/camera-component-reference.html) - Output properties (including viewport rect) are only on Base cameras. Overlay cameras don't control output destination.
- **Notes**: Practical implication for split-screen: if you have Base Camera A with rect (0, 0, 0.5, 1) and an Overlay Camera stacked on it, the overlay renders within the left half of the screen. This is the expected behavior and is how split-screen + stacking should work together.

### Finding 4: Global Volumes do NOT automatically affect all cameras -- Volume Mask controls scoping
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Manual: Add post-processing in URP (6.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/add-post-processing.html) - "Post-processing effects from a volume apply to a camera only if a value in the Volume Mask property of the camera contains the layer that the volume belongs to."
  - [Unity Manual: Apply different post-proc to cameras (6.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/cameras/apply-different-post-proc-to-cameras.html) - Full walkthrough: create separate layers, assign Volume GameObjects to different layers, set each camera's Volume Mask to only include the layer of its intended Volume. Each camera then only receives post-processing from volumes on matching layers.
- **Notes**: The Volume Mask is on the camera (Environment > Volume Mask), not on the Volume itself. A Volume's layer determines which cameras see it, based on the camera's mask. Default Volume Mask is "Default" layer, so a Global Volume on the Default layer affects all cameras with default settings. To scope: put Volumes on custom layers, restrict each camera's Volume Mask.

### Finding 5: "Some effects apply to all cameras by default" -- requires explicit overrides
- **Confidence**: MEDIUM
- **Supporting sources**:
  - [Unity Manual: Apply different post-proc to cameras (6.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/cameras/apply-different-post-proc-to-cameras.html) - "Some effects apply to all cameras in a scene by default, so you might need to add the same effect to each volume to override the effects from other volumes on individual cameras."
- **Notes**: This is vague in the docs. It likely refers to the situation where a camera's Volume Mask includes the Default layer and a Global Volume exists on that layer. To fully isolate post-processing per camera: (1) put each Volume on a unique layer, (2) set each camera's Volume Mask to ONLY its intended layer (excluding Default if needed), (3) add any override effects to each Volume to prevent inheritance from volumes on other layers.

### Finding 6: Post-processing in a camera stack applies once, from the last camera
- **Confidence**: HIGH
- **Supporting sources**:
  - [Camera stacking concepts (Unity 6.0)](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/cameras/camera-stacking-concepts.html) - "Apply post-processing to the last camera in the stack" to ensure "post-processing effects only once, not repeatedly for each camera."
  - [Camera render types introduction (Unity 6.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/camera-types-and-render-type-introduction.html) - "Post-processing applied to an Overlay Camera also apply to all the outputs the camera stack renders before the Overlay Camera."
- **Notes**: Critical gotcha. If an Overlay Camera in a stack has post-processing enabled, its effects apply to the entire combined output (including the Base Camera's content rendered before it). For split-screen with different post-processing per viewport, use separate Base Cameras (not stacks) OR ensure each stack's post-processing is configured on the last overlay camera in that stack.

### Finding 7: DefaultExecutionOrder applies to LateUpdate (it affects all lifecycle event functions)
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Scripting API: DefaultExecutionOrder (6.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/DefaultExecutionOrder.html) - "The default execution order between script components applies only for the event functions Unity calls in a determined order on all active GameObjects as part of their lifecycle, such as Awake and OnEnable." LateUpdate is a lifecycle event function.
  - [DefaultExecutionOrder (Uninomicon)](https://uninomicon.com/defaultexecutionorder) - "Modifies the execution order of all event functions in a MonoBehaviour class." Affects "all event functions" within the lifecycle.
  - [Unity Manual: Script Execution Order reference](https://docs.unity3d.com/Manual/class-MonoManager.html) - Confirms the attribute is equivalent to the Script Execution Order project settings, which apply to lifecycle callbacks.
- **Notes**: DefaultExecutionOrder controls the relative order of the SAME callback across DIFFERENT scripts. A script with order -100 will have its LateUpdate called before a script with order 0. It does NOT apply to: OnTriggerEnter, OnCollisionEnter, and other event-driven callbacks that "can happen at any time." It also does NOT apply to OnDisable, OnDestroy, or [RuntimeInitializeOnLoadMethod].

### Finding 8: DefaultExecutionOrder is overridden by Editor Script Execution Order settings
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Scripting API: DefaultExecutionOrder (6.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/DefaultExecutionOrder.html) - "If you define an execution order for a MonoBehaviour-derived type in code with DefaultExecutionOrder but define a different value for the same type in the Editor's Project settings window, Unity uses the value defined in the Editor UI."
  - [DefaultExecutionOrder (Uninomicon)](https://uninomicon.com/defaultexecutionorder) - "Scripts using this attribute don't appear in the Script Execution Order settings window," which can cause confusion.
- **Notes**: The attribute is a default -- the Editor setting wins if both exist. The attribute-defined order is NOT visible in the Project Settings UI, so team members may not know it exists. Best practice: use the attribute consistently and avoid also setting values in the Editor.

### Finding 9: Execution order between scripts with the SAME order value is non-deterministic
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Scripting API: DefaultExecutionOrder (6.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/DefaultExecutionOrder.html) - "When multiple scripts share the same order value, execution order is not deterministic."
  - [Unity Manual: Order of execution for event functions](https://docs.unity3d.com/6000.3/Documentation/Manual/execution-order.html) - "In general, you can't rely on the order in which the same event function is invoked for different GameObjects, except when the order is explicitly documented or settable."
- **Notes**: If you need Script A's LateUpdate before Script B's LateUpdate, you MUST give them different order values. Same-value ordering can vary between builds, machines, and Unity versions.

### Finding 10: Camera.rect tweening bug was fixed in 2021-era Unity -- no known Camera.rect issues in Unity 6 URP
- **Confidence**: MEDIUM
- **Supporting sources**:
  - [Unity Issue Tracker: Viewport Rect tweening error](https://issuetracker.unity3d.com/issues/urp-attempting-to-get-camera-relative-temporary-rendertexture-is-thrown-when-tweening-the-viewport-rect-values-of-a-camera) - "Attempting to get Camera relative temporary RenderTexture" error when tweening Viewport Rect values. Fixed in URP 10.7.0 (2020.3.18f1), 2021.1.27f1, 2022.1.0a2. Some regressions reported in 2021.2-2021.3 range.
  - [Unity Manual: Known issues in URP (6.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/known-issues.html) - Only 3 known issues listed, NONE related to Camera.rect, viewport, split-screen, or RenderTexture.
- **Notes**: No Camera.rect bugs are documented for Unity 6. The historical tweening bug was in 2019-2021 era. Unity 6's known issues page has no viewport-related entries. This suggests Camera.rect is stable in Unity 6 URP.

### Finding 11: RenderTexture approach has higher cost but more flexibility
- **Confidence**: MEDIUM
- **Supporting sources**:
  - [Use multiple cameras (URP 14)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/cameras-multiple.html) - "If you use multiple cameras, it might make rendering slower. An active camera runs through the entire rendering loop even if it renders nothing." Both approaches require multiple cameras, but RenderTexture adds an extra blit pass.
  - [Unity Manual: Set up split-screen rendering (6.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/rendering-to-the-same-render-target.html) - Camera.rect is the officially documented split-screen approach, not RenderTexture.
  - [URP rendering to a Render Texture (v17)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/manual/rendering-to-a-render-texture.html) - "All Cameras that render to Render Textures perform their render loops before all Cameras that render to the screen."
- **Notes**: RenderTexture advantages: full control over resolution per viewport, can display camera output on UI elements or 3D surfaces, each RT gets independent post-processing naturally. RenderTexture disadvantages: extra memory for RT allocation, extra blit/draw to display on screen, more complex setup, render order constraints (RT cameras render before screen cameras). For standard split-screen, Camera.rect is simpler and sufficient. RenderTexture is better when you need: (a) camera output on a UI element, (b) resolution independence between viewports, or (c) fully independent post-processing pipelines that can't be solved with Volume Masks.

### Finding 12: Each additional camera incurs full culling, light processing, and shadow rendering cost
- **Confidence**: HIGH
- **Supporting sources**:
  - [Use multiple cameras (URP 14)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/cameras-multiple.html) - "An active camera runs through the entire rendering loop even if it renders nothing."
  - [Unity Blog: Optimize game performance with camera usage](https://unity.com/blog/games/part-2-optimize-game-performance-with-camera-usage) - (403 error on fetch, but search snippet confirmed: "Because culling, light processing, and shadow rendering is performed per camera it is a good idea to render as few cameras as possible per frame, ideally only one.")
- **Notes**: This cost is the same whether using Camera.rect or RenderTexture -- both require separate camera objects. For a Director View with a small viewport, Camera.rect renders fewer pixels (GPU benefit) but still does full CPU-side culling and shadow setup.

### Finding 13: Volume Mask is settable at runtime via UniversalAdditionalCameraData.volumeLayerMask
- **Confidence**: HIGH
- **Supporting sources**:
  - [UniversalAdditionalCameraData API (URP 17.3)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.3/api/UnityEngine.Rendering.Universal.UniversalAdditionalCameraData.html) - `volumeLayerMask` is a `LayerMask` property with getter and setter. Also exposes `volumeTrigger` (Transform for volume blending evaluation) and `volumeStack` (current VolumeStack). Access via `camera.GetUniversalAdditionalCameraData()`.
  - [Unity Discussions: Change camera's volume layer mask through code](https://discussions.unity.com/t/urp-change-cameras-volume-layer-mask-through-code/909920) - Community confirmation that `volumeLayerMask` can be set at runtime.
- **Notes**: Requires `using UnityEngine.Rendering.Universal;`. Cache the reference for performance. The `volumeTrigger` property controls which Transform is used for local volume proximity blending -- defaults to the camera's transform but can be overridden.

### Finding 14: UniversalAdditionalCameraData does NOT expose a rect/viewport property
- **Confidence**: HIGH
- **Supporting sources**:
  - [UniversalAdditionalCameraData API (URP 17.3)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.3/api/UnityEngine.Rendering.Universal.UniversalAdditionalCameraData.html) - Full property list includes no rect or viewport property. Screen coordinate overrides exist (`useScreenCoordOverride`, `screenCoordScaleBias`, `screenSizeOverride`) but these are for coordinate remapping, not viewport definition.
- **Notes**: Set viewport rect through the standard Unity `Camera.rect` property, not through URP's additional data component. The split-screen docs confirm `Camera.rect` works directly.

## Contradictions

- **Post-processing scope -- "global" vs "per-camera"**: The docs say "some effects apply to all cameras by default" but also say "post-processing effects from a volume apply to a camera only if the Volume Mask matches." These are not truly contradictory -- the "global by default" statement refers to the Default layer being in every camera's Volume Mask by default. Removing Default from a camera's Volume Mask prevents global volumes on that layer from affecting it. This is a documentation clarity issue, not a technical contradiction.

- **Overlay camera viewport behavior**: No sources explicitly state "overlay cameras render within the base camera's viewport rect." The docs say overlay cameras don't have their own viewport rect and that the base camera determines output properties. The logical conclusion is that overlays render within the base's viewport, but this is inferred rather than directly stated. It would be worth a quick empirical test.

## Source Registry
| # | Title | URL | Date | Queries that surfaced it |
|---|-------|-----|------|--------------------------|
| 1 | Unity Manual: Set up split-screen rendering in URP (6.3) | https://docs.unity3d.com/6000.3/Documentation/Manual/urp/rendering-to-the-same-render-target.html | Current | Q1, Q6 |
| 2 | URP Camera Component Reference v16 | https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@16.0/manual/camera-component-reference.html | Current | Q2, Q7 |
| 3 | Camera render types introduction (Unity 6.3) | https://docs.unity3d.com/6000.3/Documentation/Manual/urp/camera-types-and-render-type-introduction.html | Current | Q7 |
| 4 | Camera stacking concepts (Unity 6.0) | https://docs.unity3d.com/6000.0/Documentation/Manual/urp/cameras/camera-stacking-concepts.html | Current | Q2 |
| 5 | Apply different post-proc to cameras (Unity 6.3) | https://docs.unity3d.com/6000.3/Documentation/Manual/urp/cameras/apply-different-post-proc-to-cameras.html | Current | Q3 |
| 6 | Add post-processing in URP (Unity 6.3) | https://docs.unity3d.com/6000.3/Documentation/Manual/urp/add-post-processing.html | Current | Q3 |
| 7 | DefaultExecutionOrder API (Unity 6.3) | https://docs.unity3d.com/6000.3/Documentation/ScriptReference/DefaultExecutionOrder.html | Current | Q4 |
| 8 | DefaultExecutionOrder (Uninomicon) | https://uninomicon.com/defaultexecutionorder | Unknown | Q4 |
| 9 | Script Execution Order reference | https://docs.unity3d.com/Manual/class-MonoManager.html | Current | Q4 |
| 10 | Script execution order (Unity 6.3) | https://docs.unity3d.com/6000.3/Documentation/Manual/script-execution-order.html | Current | Q4 |
| 11 | Order of execution for event functions (Unity 6.3) | https://docs.unity3d.com/6000.3/Documentation/Manual/execution-order.html | Current | Q4 |
| 12 | Unity Issue Tracker: Viewport Rect tweening error | https://issuetracker.unity3d.com/issues/urp-attempting-to-get-camera-relative-temporary-rendertexture-is-thrown-when-tweening-the-viewport-rect-values-of-a-camera | 2021 | Q5 |
| 13 | Known issues in URP (Unity 6.3) | https://docs.unity3d.com/6000.3/Documentation/Manual/urp/known-issues.html | Current | Q5 |
| 14 | Use multiple cameras (URP 14) | https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/cameras-multiple.html | Current | Q6 |
| 15 | Unity Blog: Optimize performance with camera usage | https://unity.com/blog/games/part-2-optimize-game-performance-with-camera-usage | Unknown | Q6 (403 error on fetch) |
| 16 | Unity Discussions: Why can't overlay cameras use viewport rect? | https://discussions.unity.com/t/why-cant-overlay-cameras-use-viewport-rect/798667 | Unknown | Q2 (403 error on fetch) |
| 17 | Render to a Render Texture (URP 17) | https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/manual/rendering-to-a-render-texture.html | Current | Q6 |
| 18 | UniversalAdditionalCameraData API (URP 17.3) | https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.3/api/UnityEngine.Rendering.Universal.UniversalAdditionalCameraData.html | Current | Q8 |
| 19 | Unity Discussions: Change camera's volume layer mask through code | https://discussions.unity.com/t/urp-change-cameras-volume-layer-mask-through-code/909920 | Unknown | Q8 |
