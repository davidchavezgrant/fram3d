# Raw Research Findings: Unity Play Mode Test Isolation & InputTestFixture Internals

**Date:** 2026-03-26
**Supplements:** `docs/research/2026-03-26-play-mode-test-isolation.md` (codebase-specific analysis)

## Queries Executed
1. "Unity InputTestFixture Setup TearDown what does it do internally" - 3 useful results
2. "Unity InputSystem.onEvent InputTestFixture isolation test callback" - 2 useful results
3. "Unity Play Mode tests failing double consecutive runs oscillating NUnit" - 2 useful results
4. "Unity InputTestFixture Keyboard.current Mouse.current null test device lifecycle" - 2 useful results
5. "Unity NUnit test runner execution order TestFixture class ordering Play Mode" - 3 useful results
6. "Unity InputTestFixture source code (via GitLab mirror of package cache)" - 1 result (actual source)
7. "Unity InputSystem SaveAndReset/Restore source code" - 2 results (actual source)
8. "Unity InputSystem onEvent subscriber leak between tests" - 2 useful results
9. "Unity FindObjectOfType stale object Destroy vs DestroyImmediate tests" - 2 useful results
10. "NUnit fixture execution order OneTimeSetUp namespace" - 3 useful results

---

## Findings

### Finding 1: InputTestFixture.Setup() replaces the ENTIRE InputSystem with a fresh instance

- **Confidence**: HIGH
- **Supporting sources**:
  - [InputTestFixture source code v1.0.0 (GitLab mirror)](https://gitlab.imt-atlantique.fr/h20damai/happy-potatoes-ihm/-/raw/master/Library/PackageCache/com.unity.inputsystem@1.0.0/Tests/TestFixture/InputTestFixture.cs):
    ```csharp
    public virtual void Setup()
    {
        runtime = new InputTestRuntime();
        InputSystem.SaveAndReset(enableRemoting: false, runtime: runtime);
        // ...
        if (InputSystem.devices.Count > 0)
            Assert.Fail("Input system should not have devices after reset");
    }
    ```
  - [InputSystem.cs source (5argon mirror)](https://github.com/5argon/NewInputLatencyTest/blob/master/Packages/com.unity.inputsystem/InputSystem/InputSystem.cs):
    ```csharp
    internal static void Save()
    {
        if (s_SerializedStateStack == null)
            s_SerializedStateStack = new List<InputManager.SerializedState>();
        s_SerializedStateStack.Add(s_Manager.SaveState());
    }
    ```
  - [Unity Input Testing docs v1.14](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Testing.html) - "sets up a blank, default-initialized version of the Input System for each test"
- **Notes**: `SaveAndReset` saves the entire `InputManager` state to a stack (via `s_Manager.SaveState()`) and then creates a new blank `InputManager` with a test runtime. After this call, `InputSystem.devices` is empty, `Keyboard.current` is null, `Mouse.current` is null. Tests must explicitly add devices.

### Finding 2: InputTestFixture.TearDown() destroys ALL root GameObjects in the scene

- **Confidence**: HIGH
- **Supporting sources**:
  - [InputTestFixture source code v1.0.0](https://gitlab.imt-atlantique.fr/h20damai/happy-potatoes-ihm/-/raw/master/Library/PackageCache/com.unity.inputsystem@1.0.0/Tests/TestFixture/InputTestFixture.cs):
    ```csharp
    public virtual void TearDown()
    {
        var scene = SceneManager.GetActiveScene();
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.hideFlags != 0 || go.name.Contains("tests runner"))
                continue;
            Object.DestroyImmediate(go);
        }
        InputSystem.Restore();
        runtime.Dispose();
    }
    ```
- **Notes**: This is aggressive -- it nukes every root GameObject that doesn't have hideFlags or "tests runner" in its name. This happens BEFORE `InputSystem.Restore()`. The `DestroyImmediate` calls will trigger `OnDisable`/`OnDestroy` on all MonoBehaviours, which means `InputSystem.onEvent` subscribers get unsubscribed as their `OnDisable` fires. Then `InputSystem.Restore()` pops the saved state and reinstates the original manager.

### Finding 3: InputSystem.onEvent callbacks are NOT part of the saved/restored state

- **Confidence**: MEDIUM
- **Supporting sources**:
  - [InputSystem.cs source (5argon mirror)](https://github.com/5argon/NewInputLatencyTest/blob/master/Packages/com.unity.inputsystem/InputSystem/InputSystem.cs) - `Save()` only saves `s_Manager.SaveState()`. The source comments `"REVIEW: what should we do with the remote here?"` and does NOT save `onEvent`, `onBeforeUpdate`, or `onAfterUpdate` callback delegates.
  - [Unity Input Testing docs v1.14](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Testing.html) - Silent on callback restoration. Says "restores the Input System to its original state" without qualifying what "state" includes.
- **Notes**: The `InputManager.SerializedState` struct captures layouts, devices, settings, and state buffers -- but callback delegates (`onEvent`, `onBeforeUpdate`, `onAfterUpdate`) are NOT serializable and are NOT included. When `Restore()` calls `Reset()` (which creates a fresh InputManager), all callback subscribers are discarded. When `RestoreState()` loads the serialized state, callbacks are not restored. **This means: any code that subscribed to `InputSystem.onEvent` before the test fixture ran will LOSE its subscription after the fixture's TearDown.**

### Finding 4: Non-InputTestFixture tests use the LIVE InputSystem with real devices

- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Input Testing docs v1.14](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Testing.html) - The fixture provides isolation; without it, tests use the real system
  - [InputTestFixture API docs v1.3](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/api/UnityEngine.InputSystem.InputTestFixture.html) - The fixture "severs the connection of the input system to the Unity runtime"
  - [InputTestFixture API docs v1.0](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputTestFixture.html) - Same behavior described
- **Notes**: When a test class does NOT extend InputTestFixture, `Keyboard.current` and `Mouse.current` point to real hardware devices. `InputSystem.AddDevice<T>()` adds to the real global system. `InputSystem.RemoveDevice()` removes from the real system. There is no automatic save/restore.

### Finding 5: Mixing InputTestFixture and non-InputTestFixture classes in one suite is hazardous

- **Confidence**: HIGH (derived from combining Findings 1-4 with execution order knowledge)
- **Supporting sources**:
  - All sources from Findings 1-4 combined
  - [NUnit fixture ordering](https://github.com/nunit/nunit/issues/345) - fixture execution order is effectively alphabetical but not guaranteed
- **Notes**: The hazard scenario:
  1. **InputTestFixture class A** runs: `SaveAndReset` saves real system state, creates blank system. Tests run. `Restore` pops saved state.
  2. **Non-fixture class B** runs next: uses real InputSystem. Adds devices via `AddDevice()`. MonoBehaviours subscribe to `InputSystem.onEvent`.
  3. **InputTestFixture class A** runs again (or class C, also InputTestFixture): `SaveAndReset` saves state -- **including the devices B added**. Creates blank. Tests run. `Restore` pops -- **restoring B's added devices BUT NOT B's onEvent subscribers** (callbacks aren't saved).
  4. B's MonoBehaviours now have stale references to the old InputSystem's onEvent and may not be subscribed to the current one.

  The reverse is also dangerous: if B runs between two runs of A, B's `RemoveDevice` in TearDown removes from the real system. Then A's `Restore` tries to restore the pre-A state which included those devices -- but the underlying native device IDs may be invalid.

### Finding 6: NUnit fixture execution order -- alphabetical in Unity, not guaranteed by NUnit

- **Confidence**: MEDIUM
- **Supporting sources**:
  - [NUnit Issue #345](https://github.com/nunit/nunit/issues/345) - Charlie Poole: "In the past they were sorted alphabetically. You can experiment to see if that's still true since it's really just a side effect. You have to take the full namespace into account."
  - [NUnit Issue #2521](https://github.com/nunit/nunit/issues/2521) - "Attribute Order is ignored, test fixtures (and tests) executed in alphabetic order"
  - [Unity Discussions](https://discussions.unity.com/t/is-it-possible-to-change-the-order-of-tests-on-unity-test-runner/720367) - "On the Unity Test Runner they are sorted alphabetically"
  - [NUnit Order attribute docs](https://docs.nunit.org/articles/nunit/writing-tests/attributes/order.html) - "Among tests with the same order value or without the attribute, execution order is indeterminate"
- **Notes**: One fixture's `TearDown`/`OneTimeTearDown` completes before the next fixture's `OneTimeSetUp`/`SetUp`. Fixtures in the same namespace with no `[Order]` attribute run in alphabetical order in practice (in Unity's test runner). This means `AspectRatioMaskViewTests` runs before `CameraInputHandlerTests` runs before `CursorBehaviourTests` runs before `SelectionInputHandlerTests`. This ordering matters for diagnosing cross-contamination.

### Finding 7: Object.Destroy is deferred; FindAnyObjectByType finds "destroyed" objects

- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity API: Object.Destroy](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Object.Destroy.html) - "The object obj is destroyed immediately after the current Update loop"
  - [Unity API: Object.DestroyImmediate](https://docs.unity3d.com/ScriptReference/Object.DestroyImmediate.html) - "Destroys the object obj immediately"
  - [Unity Discussions](https://discussions.unity.com/t/why-does-findobjectsoftype-find-objects-earlier-destroyed/186463) - Confirmed: `FindObjectsOfType` finds objects marked for deferred destruction
- **Notes**: `DestroyImmediate` is required in test TearDown. Using `Destroy` means the next test's SetUp (which may run in the same frame or the next frame) can find stale objects.

### Finding 8: The oscillating failure pattern (N -> 2N -> N) matches subscriber/object doubling

- **Confidence**: MEDIUM
- **Supporting sources**:
  - [Input System callback performed twice](https://discussions.unity.com/t/input-system-callback-performed-twice/1572642) - Community reports of callbacks firing twice
  - [Inputs fired twice - GitHub Issue #959](https://github.com/Unity-Technologies/InputSystem/issues/959) - Confirmed InputSystem bug/pattern with duplicate event firing
  - [Player Input triggering events multiple times](https://discussions.unity.com/t/player-input-component-triggering-events-multiple-times/781922) - Subscriber accumulation causing multiple fires
- **Notes**: The pattern: Run 1 (clean): N tests fail for their own reasons. Run 2: stale subscribers/objects from Run 1 double the failures (2N). Run 3: the doubled stale state is cleaned somehow (either via `DestroyImmediate` catching up or InputTestFixture reset), back to N. This requires a mechanism where stale state accumulates on even runs and gets cleaned on odd runs (or vice versa). The most likely cause is `Destroy` (not Immediate) leaving objects alive for one extra frame, combined with test runner behavior that sometimes processes the deferred destruction between runs and sometimes doesn't.

### Finding 9: Overriding Setup/TearDown with [SetUp]/[TearDown] attributes silently breaks InputTestFixture

- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Input Testing docs v1.14](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Testing.html) - "If you use NUnit's [Setup] and [TearDown] attributes on methods in your test fixture, this will override the methods inherited from InputTestFixture and thus cause them to not get executed"
  - [InputTestFixture API docs v1.0](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputTestFixture.html) - Same warning
  - [InputTestFixture API docs v1.3](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/api/UnityEngine.InputSystem.InputTestFixture.html) - Same warning
- **Notes**: NUnit finds `[SetUp]` methods by attribute, not by virtual override. If a derived class declares `[SetUp] public void Setup()` instead of `public override void Setup()`, NUnit calls the derived method (which has [SetUp]) and never calls the base class method (whose [SetUp] is hidden). The InputSystem is never saved/reset. All tests in that class run against the real, unsandboxed InputSystem. **This is a very common mistake.**

### Finding 10: InputTestFixture creates a test runtime that blocks real hardware input

- **Confidence**: HIGH
- **Supporting sources**:
  - [InputTestFixture source code v1.0.0](https://gitlab.imt-atlantique.fr/h20damai/happy-potatoes-ihm/-/raw/master/Library/PackageCache/com.unity.inputsystem@1.0.0/Tests/TestFixture/InputTestFixture.cs) - `runtime = new InputTestRuntime()` then passed to `SaveAndReset`
  - [Unity Input Testing docs v1.14](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Testing.html) - "the input system will not receive input and device discovery or removal notifications from platform code"
  - [InputTestFixture API v1.3](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/api/UnityEngine.InputSystem.InputTestFixture.html) - "severs the connection of the input system to the Unity runtime"
- **Notes**: `InputTestRuntime` is a fake runtime that does not poll hardware. No real mouse movements, keyboard presses, or device connections are seen. Tests must explicitly queue events via `InputSystem.QueueStateEvent()` or use the fixture's helper methods (`Press`, `Release`, `Set`, etc.). After TearDown, the real runtime is restored and hardware input flows again.

### Finding 11: NUnit lifecycle -- SetUp/TearDown per-test, OneTimeSetUp per-fixture

- **Confidence**: HIGH
- **Supporting sources**:
  - [NUnit OneTimeSetUp docs](https://docs.nunit.org/articles/nunit/writing-tests/attributes/onetimesetup.html) - "once prior to executing any of the tests in a fixture"
  - [NUnit Lifecycle](https://docs.educationsmediagroup.com/unit-testing-csharp/nunit/lifecycle-of-a-test-fixture) - Constructor -> OneTimeSetUp -> (SetUp -> Test -> TearDown)* -> OneTimeTearDown
  - [NUnit SetUpFixture docs](https://docs.nunit.org/articles/nunit/writing-tests/attributes/setupfixture.html) - Namespace-level setup runs before all fixtures in that namespace
- **Notes**: InputTestFixture's `Setup()`/`TearDown()` are `[SetUp]`/`[TearDown]`, meaning the entire InputSystem save/reset/restore cycle runs for **every single test method**. This is intentional -- each test gets a completely clean InputSystem. But it's expensive and means device creation (`AddDevice<Keyboard>()`) also happens per-test.

### Finding 12: Known Unity Test Framework bugs with consecutive runs

- **Confidence**: MEDIUM
- **Supporting sources**:
  - [Unity Test Framework changelog v1.1.33](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/changelog/CHANGELOG.html) - Fixed: "if the first test enters PlayMode from UnitySetup then the test body will not run on consecutive runs (case 1260901)"
  - [Unity Test Framework changelog v2.0](https://docs.unity3d.com/Packages/com.unity.test-framework@2.0/changelog/CHANGELOG.html) - Fixed: "playmode tests execution status staying running even after the test finished if domain reload was disabled (DSTR-5)"
  - [Unity Discussions](https://discussions.unity.com/t/test-framework-unable-to-run-playmode-tests-via-command-line-more-than-once/890077) - Reports of Play Mode tests failing on second command-line run
- **Notes**: Unity's test framework itself has had bugs causing tests to not run properly on consecutive executions. If the project uses an older version, upgrading may resolve some oscillating failures independent of the state isolation issues.

---

## Contradictions

### Documentation vs. Source Code on "state restoration"
- **Documentation** ([v1.14 docs](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Testing.html)): "restores the Input System to its original state" -- implies full restoration of everything
- **Source code** ([InputSystem.cs](https://github.com/5argon/NewInputLatencyTest/blob/master/Packages/com.unity.inputsystem/InputSystem/InputSystem.cs)): Only `s_Manager.SaveState()` (InputManager.SerializedState) is saved. Callback delegates (`onEvent`, `onBeforeUpdate`, etc.) are NOT serialized or restored. The `Reset()` call creates a fresh manager, discarding all callbacks.
- **Impact**: Code that subscribes to `InputSystem.onEvent` outside of test fixture control loses its subscription when a fixture-using test runs and completes.

### DestroyImmediate guidance vs. practice
- **Unity docs** ([DestroyImmediate](https://docs.unity3d.com/ScriptReference/Object.DestroyImmediate.html)): "You are strongly recommended to use Destroy instead"
- **InputTestFixture source** ([source](https://gitlab.imt-atlantique.fr/h20damai/happy-potatoes-ihm/-/raw/master/Library/PackageCache/com.unity.inputsystem@1.0.0/Tests/TestFixture/InputTestFixture.cs)): Uses `DestroyImmediate` in TearDown
- **Resolution**: The Unity docs warning applies to runtime gameplay code. In test teardown, `DestroyImmediate` is correct because deferred `Destroy` causes cross-test contamination.

---

## Source Registry

| # | Title | URL | Date | Queries |
|---|-------|-----|------|---------|
| 1 | InputTestFixture source v1.0.0 (GitLab mirror) | https://gitlab.imt-atlantique.fr/h20damai/happy-potatoes-ihm/-/raw/master/Library/PackageCache/com.unity.inputsystem@1.0.0/Tests/TestFixture/InputTestFixture.cs | 2020 | Q6 |
| 2 | InputSystem.cs source (5argon mirror) | https://github.com/5argon/NewInputLatencyTest/blob/master/Packages/com.unity.inputsystem/InputSystem/InputSystem.cs | 2019 | Q7 |
| 3 | Unity Input Testing docs v1.14 | https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Testing.html | 2024 | Q1,Q2,Q4 |
| 4 | InputTestFixture API v1.0 | https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputTestFixture.html | 2020 | Q1,Q4 |
| 5 | InputTestFixture API v1.3 | https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/api/UnityEngine.InputSystem.InputTestFixture.html | 2022 | Q1 |
| 6 | NUnit Issue #345 - Fixture Ordering | https://github.com/nunit/nunit/issues/345 | 2016 | Q5 |
| 7 | NUnit Issue #2521 - Alphabetic Order | https://github.com/nunit/nunit/issues/2521 | 2018 | Q5 |
| 8 | NUnit Order docs | https://docs.nunit.org/articles/nunit/writing-tests/attributes/order.html | current | Q5 |
| 9 | NUnit OneTimeSetUp docs | https://docs.nunit.org/articles/nunit/writing-tests/attributes/onetimesetup.html | current | Q5 |
| 10 | NUnit Lifecycle | https://docs.educationsmediagroup.com/unit-testing-csharp/nunit/lifecycle-of-a-test-fixture | current | Q5 |
| 11 | Unity API: Object.Destroy | https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Object.Destroy.html | current | Q9 |
| 12 | Unity API: Object.DestroyImmediate | https://docs.unity3d.com/ScriptReference/Object.DestroyImmediate.html | current | Q9 |
| 13 | Input System callback twice | https://discussions.unity.com/t/input-system-callback-performed-twice/1572642 | 2025 | Q8 |
| 14 | Inputs fired twice #959 | https://github.com/Unity-Technologies/InputSystem/issues/959 | 2020 | Q8 |
| 15 | Test Framework changelog v1.1.33 | https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/changelog/CHANGELOG.html | 2022 | Q3 |
| 16 | Test Framework changelog v2.0 | https://docs.unity3d.com/Packages/com.unity.test-framework@2.0/changelog/CHANGELOG.html | 2024 | Q3 |
| 17 | NUnit SetUpFixture docs | https://docs.nunit.org/articles/nunit/writing-tests/attributes/setupfixture.html | current | Q5 |
| 18 | Unity Discussions - Test order | https://discussions.unity.com/t/is-it-possible-to-change-the-order-of-tests-on-unity-test-runner/720367 | 2020 | Q5 |
| 19 | InputSystem InputManager.cs | https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/InputSystem/InputManager.cs | current | Q7 |
| 20 | InputSystem Testing.md | https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/Documentation~/Testing.md | current | Q1 |
