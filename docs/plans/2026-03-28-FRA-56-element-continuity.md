# FRA-56: Element Continuity — Global Element Timeline

## Summary

Elements animate on a single global timeline that spans all shots. Camera keyframes are per-shot; element keyframes are global. No per-shot initial state — the global timeline IS the continuity.

## Phases

### Phase 1: Core — ElementTrack

New file: `Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementTrack.cs`

Per-element animation track with global position and rotation keyframes. Wraps two `KeyframeManager<T>` instances.

```csharp
public sealed class ElementTrack
{
    ElementId ElementId { get; }
    KeyframeManager<Vector3> PositionKeyframes { get; }
    KeyframeManager<Quaternion> RotationKeyframes { get; }
    Vector3 EvaluatePosition(TimePosition globalTime)
    Quaternion EvaluateRotation(TimePosition globalTime)
    bool HasKeyframes { get; }
    int KeyframeCount { get; }
}
```

Tests: `tests/Fram3d.Core.Tests/Timelines/ElementTrackTests.cs`

### Phase 2: Core — ElementTimeline

New file: `Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementTimeline.cs`

Registry of element tracks keyed by ElementId. Evaluates all animated elements at a given global time.

```csharp
public sealed class ElementTimeline
{
    ElementTrack GetOrCreateTrack(ElementId id)
    bool RemoveTrack(ElementId id)
    ElementTrack GetTrack(ElementId id)
    bool HasTrack(ElementId id)
    IReadOnlyCollection<ElementTrack> Tracks { get; }
    int TrackCount { get; }
}
```

Tests: `tests/Fram3d.Core.Tests/Timelines/ElementTimelineTests.cs`

### Phase 3: Core — ElementEvaluation DTO

New file: `Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementEvaluation.cs`

Emitted when elements should evaluate at a global time. Mirrors `CameraEvaluation` pattern.

```csharp
public sealed class ElementEvaluation
{
    TimePosition GlobalTime { get; }
}
```

No separate test file — tested via Timeline integration tests.

### Phase 4: Core — Wire into Timeline

Modify: `Unity/Fram3d/Assets/Scripts/Core/Timelines/Timeline.cs`

- Add `ElementTimeline` as owned sub-component
- Add `Subject<ElementEvaluation>` and expose as `IObservable<ElementEvaluation> ElementEvaluationRequested`
- Emit `ElementEvaluation` whenever `EvaluateCamera()` fires (same trigger points: scrub, playback advance, shot navigation)
- Delegate element track methods: `GetOrCreateElementTrack(ElementId)`, `RemoveElementTrack(ElementId)`, `GetElementTrack(ElementId)`
- Expose `ElementTimeline` for direct access where needed

New tests in `tests/Fram3d.Core.Tests/Timelines/TimelineTests.cs`:
- `Advance__FiresElementEvaluation__When__Playing`
- `ScrubToPixel__FiresElementEvaluation__When__Called`
- `SetCurrentShot__FiresElementEvaluation__When__ShotChanges` (via shot navigation)

### Phase 5: Engine — Wire ShotEvaluator

Modify: `Unity/Fram3d/Assets/Scripts/Engine/Integration/ShotEvaluator.cs`

- Subscribe to `ElementEvaluationRequested`
- On evaluation: find all `ElementBehaviour` instances, look up each element's track, evaluate position/rotation at global time, apply to `Element.Position`/`Element.Rotation`
- Cache `ElementBehaviour[]` and refresh on scene changes (or just `FindObjectsByType` each eval for simplicity in this milestone)

No new Play Mode tests — the evaluation just writes to Element.Position which ElementBehaviour already syncs.

## Key Design Decisions

1. **Elements without keyframes stay put.** If an element has no track (or an empty track), evaluation skips it. The element remains at whatever position it was placed at.

2. **Evaluation piggybacks on camera evaluation timing.** Same trigger points: scrub, playback, shot navigation. One global time, two evaluation paths (camera per-shot, elements global).

3. **No UI changes in this milestone.** Element tracks exist in the data model but aren't visualized in the timeline UI yet. That's a later milestone.

4. **`ElementTrack` uses `System.Numerics` types.** Same as `Shot`'s camera keyframes — pure C#, no Unity dependency.

## File Changes

| Action | Path |
|--------|------|
| Create | `Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementTrack.cs` |
| Create | `Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementTimeline.cs` |
| Create | `Unity/Fram3d/Assets/Scripts/Core/Timelines/ElementEvaluation.cs` |
| Modify | `Unity/Fram3d/Assets/Scripts/Core/Timelines/Timeline.cs` |
| Modify | `Unity/Fram3d/Assets/Scripts/Engine/Integration/ShotEvaluator.cs` |
| Create | `tests/Fram3d.Core.Tests/Timelines/ElementTrackTests.cs` |
| Create | `tests/Fram3d.Core.Tests/Timelines/ElementTimelineTests.cs` |
| Modify | `tests/Fram3d.Core.Tests/Timelines/TimelineTests.cs` |
