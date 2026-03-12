# Prior Codebase Lessons

Patterns worth carrying forward and anti-patterns to avoid, drawn from the prior codebase.

---

## Patterns to Reuse

**`ICameraState` interface isolating Cinemachine.** The interface exposes only `Position`, `Rotation`, `FieldOfView`, `Forward`, `Right`, `Translate()`, `RotateAround()`. The concrete `CinemachineCameraState` is a 49-line adapter. Every controller depends on the interface — Cinemachine is never imported outside the adapter. This is the cleanest boundary in the old codebase.

**`VirtualCameraRig` as a unified facade.** Four sub-controllers (Movement, Lens, Focus, Shake) behind a single facade that exposes named cinema operations: `Dolly()`, `Pan()`, `Tilt()`, `Orbit()`, `Crane()`, `Roll()`, `DollyZoom()`, `Zoom()`, `FocusOn()`. No caller reaches into sub-controllers directly.

**`CameraAspectRatio` — closed type hierarchy.** Abstract class with private constructor and sealed nested subclasses instead of an enum. Each value carries `DisplayName` and `Value`, supports `GetNext()` cycling and implicit float conversion. No switch statements needed.

**`KeyframeManager<T>` — dual storage.** Maintains both a `List<T>` (ordered, for iteration/evaluation) and a `Dictionary<KeyframeId, T>` (for O(1) lookup). `AddKeyframe()` removes any existing keyframe at the same time before inserting.

**`AnimationFrameTracker` — frame boundary sentinel.** Records `Time.frameCount` when animation is applied; trackers check it to distinguish animation-driven movement from user-driven movement. 9 lines, no polling or delta comparison.

**`CompositeAnimationCurveSet` — extensible multi-track curves.** `RebuildCurves()` iterates an array of `IAnimationCurveSet` implementations and populates them from keyframes. Adding a new animated property = implement the interface, add to the array. Clean extension point.

**Value objects with validation.** `KeyframeId` wraps `Guid`, rejects `Guid.Empty` at construction, implements full equality. `TimePosition` rejects negative values, provides `Add()`/`Subtract()` (clamps to zero), `ToFrames(frameRate)`, comparison operators. Prevents raw-float time bugs.

---

## Anti-patterns to Avoid

**God object state classes.** `TimelineState` held shot list, selection indices, current time, IsEvaluating flag, duration editing state, thumbnail references, AND keyframe marker dictionaries — all in one class. The aggregate design (Scene, ShotSetup, Project) exists to prevent this.

**Routing internal operations through the command pattern.** `ScrubTime`, `EvaluateShotAtTime`, `RefreshThumbnails`, `RefreshKeyframeEditor`, `ReorderThumbnails`, and `UpdateTimeline` were all `ICommand` implementations. None are user actions. None should be undoable. Commands are exclusively for user-initiated state changes (the list in spec 4.1.1). Everything else is a plain method call.

**`FindObjectOfType` for dependency resolution.** `TimelineState.EvaluateShot()` called `Object.FindObjectOfType<ApplicationController>()` to reach back into the scene graph — domain model depending on the Unity scene. `SceneElement.Awake()` made four separate `FindObjectOfType` calls. Dependencies should be injected at construction or wiring time, not scraped from the scene at runtime.

**UI references in the domain model.** `TimelineState` stored `Dictionary<AnimationKeyframe, KeyframeMarker>` where `KeyframeMarker` is a UI element. This means the domain can't be tested without Unity UI. The mapping between domain objects and their visual representation belongs in the UI layer.

**Transient command objects allocated every `Update()`.** `new TimelineInteractionHandler(...).Execute()` and `new UpdateTimeline(...).Execute()` were called every frame, allocating heap objects that immediately die. These are stateless procedures dressed as objects — they should be reused instances or static methods.

**Mixed input systems.** The project uses the New Input System (`Keyboard.current` / `Mouse.current`) in `UserInputDriver`, but `UpdateTimeline`, `TimelineInteractionHandler`, and `CameraInfoView` use legacy `UnityEngine.Input.GetKeyDown()`. Pick one input system and use it everywhere.

**Rotation shake drift.** Position shake is applied as an additive offset and reverted each frame. Rotation shake compounds via `Rotation *= Quaternion.Euler(...)` without reverting. The camera slowly drifts in orientation while shake is enabled. Both must use the revert-then-apply pattern.

**Side effects in predicates.** `ScreenState.HasStateChanged()` updates internal `_last*` cache fields when called. Calling it twice in one frame returns `true` then `false`. Predicates should be pure — separate the query from the cache update.
