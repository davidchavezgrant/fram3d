# Design Session 2: Evaluation Pipeline

**Date**: 2026-03-12
**Status**: Decided
**Goal**: Design the per-frame evaluation pipeline — who calls what, in what order, with what inputs. Cover playback, scrubbing, recording, export, and slow-motion.
**Builds on**: Session 1 (assembly structure, Core namespace DAG)

---

## The Problem

Every frame, the system must evaluate the scene state at the current playhead time and push it to Unity. This evaluation has ordering dependencies: link chains need parent positions before child positions, follow needs the target's position before computing the camera's position, shake is additive on top of everything.

---

## Decisions

### 1. SceneEvaluator directly calls domain types

No interfaces, no per-element self-evaluation. SceneEvaluator holds references to registries and domain objects and calls their methods directly in order. ~30 lines. The explicit step-by-step ordering is the clearest way to express the pipeline. Engine references all of Core, so there's no dependency problem.

### 2. Event-driven evaluation triggers

No dirty flag that domain code must remember to set. SceneEvaluator subscribes to events that indicate re-evaluation is needed:

```csharp
public class SceneEvaluator : MonoBehaviour
{
	CommandStack _commandStack;
	Playhead _playhead;
	ElementRegistry _elements;
	ShotRegistry _shots;
	GlobalTimeline _timeline;
	LinkChainEvaluator _linkChains;
	List<ElementBehaviour> _behaviours;
	bool _needsEvaluation;

	void Start()
	{
		_commandStack.Executed.Subscribe(_ => _needsEvaluation = true);
		_commandStack.Undone.Subscribe(_ => _needsEvaluation = true);
		_commandStack.Redone.Subscribe(_ => _needsEvaluation = true);
		_playhead.Scrubbed.Subscribe(_ => _needsEvaluation = true);
	}

	void LateUpdate()
	{
		if (_playhead.IsPlaying)
		{
			_playhead.AdvanceBy(Time.deltaTime);
			Evaluate(GetCurrentTime());
		}
		else if (_needsEvaluation)
		{
			_needsEvaluation = false;
			Evaluate(GetCurrentTime());
		}

		// Always sync — covers gizmo drag updates even when no evaluation runs
		Sync();
	}
}
```

There is still a bool, but it's private to SceneEvaluator and set automatically by event subscriptions. No domain code touches it. Multiple events in one frame coalesce naturally — evaluation runs once in LateUpdate.

The always-running `Sync()` is important: during a gizmo drag, no commands fire and no evaluation runs, but the drag writes to `Element.Position` directly and Sync pushes that to Unity for live feedback.

### 3. CommandStack (renamed from UndoStack)

`CommandStack` with `Execute()`, `Undo()`, `Redo()`. Name reflects that it's a stack of commands. Publishes `IObservable` streams for Executed, Undone, Redone.

```csharp
public class CommandStack
{
	public void Execute(ICommand command);
	public void Undo();
	public void Redo();
	public bool CanUndo { get; }
	public bool CanRedo { get; }
	public int Depth { get; }

	public IObservable<ICommand> Executed => _executed;
	public IObservable<ICommand> Undone => _undone;
	public IObservable<ICommand> Redone => _redone;
}
```

### 4. Time model

| Concept | What it is | Where |
|---------|-----------|-------|
| **Playhead local time** | Current time within current shot (0 to duration) | `Playhead` in Core.Timeline |
| **Global time** | Absolute time on global element timeline | `GlobalTimeline` computes from shot + local time |
| **Wall-clock time** | Real-world elapsed time | `Time.deltaTime` in Unity |
| **Speed-adjusted time** | Global time × speed factor | `globalTime = shotStart + (localTime * speedFactor)` |

**Key distinction:** Element keyframes use **global time**. Camera keyframes use **local time** (per-shot). The evaluator passes the right time to the right evaluation.

### 5. Recording happens at command level, not evaluation level

The evaluation pipeline is read-only with respect to keyframes. Recording is handled by the input layer:

```csharp
// Engine.Integration.GizmoController
void OnDragStart(Element element)
{
	_dragTarget = element;
	_dragStartPosition = element.Position;
}

void OnDragUpdate(Vector3 worldPosition)
{
	// Direct write for live feedback. No command. No evaluation.
	// SceneEvaluator.Sync() pushes this to Unity in LateUpdate.
	_dragTarget.Position = worldPosition;
}

void OnDragEnd()
{
	var move = new MoveElementCommand(_dragTarget, _dragStartPosition, _dragTarget.Position);

	if (_stopwatch.IsRecording(_dragTarget))
	{
		var keyframe = new AddKeyframeCommand(_dragTarget, _playhead.CurrentTime);
		_commandStack.Execute(new CompoundCommand(move, keyframe));
	}
	else
	{
		_commandStack.Execute(move);
	}

	_dragTarget = null;
}
```

During a drag: no playback running, no commands, no scrubbing → `_needsEvaluation` stays false, Evaluate is skipped, only Sync runs. Keyframes don't overwrite the drag position.

On release: command fires → `_needsEvaluation = true` → next LateUpdate evaluates and syncs.

### 6. Export: bake procedural effects, then step through frames

```
Export initiated
  → ShakeBaker evaluates shake at every frame → produces keyframes
  → FollowBaker evaluates follow at every frame → produces keyframes
  → SnorricamBaker evaluates snorricam at every frame → produces keyframes
  → Baked keyframes replace procedural effects
  → OfflineFrameRenderer steps through time at 1/fps:
      for t = 0 to totalDuration step 1/fps:
        SceneEvaluator.EvaluateAtTime(t)
        Render frame → capture to output
  → Baked keyframes removed (procedural effects restored)
```

SceneEvaluator has two entry points — same internal logic, different time source:
- `LateUpdate()` uses Playhead time (playback/scrub)
- `EvaluateAtTime(TimePosition time)` takes explicit time (export)

### 7. Sync via Behaviour wrappers, one-way domain → Unity

Each Behaviour holds a reference to its domain object. `Sync()` pushes domain state to Unity:

```csharp
public class ElementBehaviour : MonoBehaviour
{
	public Element DomainElement { get; set; }

	public void Sync()
	{
		transform.position = DomainElement.Position.ToUnity();
		transform.rotation = DomainElement.Rotation.ToUnity();
		transform.localScale = UnityEngine.Vector3.one * DomainElement.Scale;
	}
}

public class CameraBehaviour : MonoBehaviour
{
	public CameraElement DomainCamera { get; set; }
	UnityEngine.Camera _camera;
	Volume _postProcessVolume;
	DepthOfField _dof;

	public void Sync()
	{
		transform.position = DomainCamera.Position.ToUnity();
		transform.rotation = DomainCamera.Rotation.ToUnity();

		_camera.usePhysicalProperties = true;
		_camera.sensorSize = new Vector2(
			DomainCamera.CameraBody.SensorWidth,
			DomainCamera.CameraBody.SensorHeight);
		_camera.focalLength = DomainCamera.FocalLength.Value;

		if (DomainCamera.DOFEnabled)
		{
			_dof.active = true;
			_dof.focusDistance.value = DomainCamera.FocusDistance;
			_dof.focalLength.value = DomainCamera.FocalLength.Value;
			_dof.aperture.value = DomainCamera.Aperture;
		}
		else
		{
			_dof.active = false;
		}
	}
}
```

Data flows in a clean cycle — not bidirectional:
```
Drag:     User → Element.Position (direct) → Sync → Unity
Release:  User → Command → CommandStack → Element.Position → Evaluate → Sync → Unity
Playback: Playhead → Evaluate → Element.Position (from keyframes) → Sync → Unity
```

### 8. No Cinemachine — plain Unity Camera + URP post-processing

Cinemachine removed from the tech stack. All camera computation happens in Core (position, rotation, FOV, shake, follow, watch, snorricam). CameraBehaviour syncs to a plain `UnityEngine.Camera` with `usePhysicalProperties = true`. DOF via URP post-processing Volume with Bokeh mode (accepts focal length, aperture, focus distance directly).

**Why:** Every feature Cinemachine provides, we compute ourselves in domain code. Cinemachine would be a passthrough dependency that adds version coupling risk and redundant FOV computation for zero benefit. Unity's built-in physical camera mode + URP DOF covers everything we need.

---

## The Complete Pipeline

```csharp
// Engine.Evaluation.SceneEvaluator

void Evaluate(TimePosition globalTime, TimePosition localTime, Shot currentShot, float deltaTime)
{
	// 1. Evaluate element keyframes at global time
	foreach (var element in _elements.GetAll())
		element.EvaluateKeyframes(globalTime);

	// 2. Evaluate link chains (parents before children)
	_linkChains.EvaluateAll(globalTime);

	// 3. Evaluate character poses
	foreach (var element in _elements.GetAll())
		if (element is CharacterElement character)
			character.EvaluatePoseAt(globalTime);

	// 4. Evaluate camera keyframes at local time (per-shot)
	foreach (var angle in currentShot.Angles)
		angle.Camera.EvaluateKeyframes(localTime);

	// 5. Apply follow/watch
	foreach (var angle in currentShot.Angles)
	{
		var cam = angle.Camera;
		if (cam.IsFollowing)
			cam.EvaluateFollow(_elements.Get(cam.FollowTargetId).Position, deltaTime);
		if (cam.IsWatching)
			cam.EvaluateWatch(_elements.Get(cam.WatchTargetId).Position);
	}

	// 6. Apply shake (additive rotation)
	foreach (var angle in currentShot.Angles)
		angle.Camera.EvaluateShake(globalTime);
}

void Sync()
{
	foreach (var behaviour in _behaviours)
		behaviour.Sync();
}
```

~30 lines of evaluation. All computation in Core (pure C#). Only Sync touches Unity.

---

## Changes to Other Docs

| Doc | Change |
|-----|--------|
| `architecture.md` | Update evaluation pipeline section with event-driven triggers, Sync() always runs, drag flow. Remove Cinemachine references. |
| `tech-stack.md` | Remove Cinemachine. Add URP post-processing for DOF. |
| `decisions.md` | Add: CommandStack naming, event-driven evaluation, no Cinemachine, plain Unity Camera + URP DOF. |
| `01b-assembly-inventory.md` | Rename UndoStack → CommandStack. Remove CinemachineCamera references from CameraBehaviour. Update Engine.Integration with plain Unity Camera + Volume. |
