# Research: Milestone 1.2.2 Composition Guides

**Date**: 2026-03-22
**Feature**: Rule of thirds, center cross, safe zones — toggled independently, drawn within unmasked area

---

## Summary

Composition guides are the second feature in milestone 1.2 (Camera Overlays). They draw rule of thirds, center cross, and safe zone lines within the unmasked area defined by the aspect ratio mask. All three guide types start hidden and are toggled independently via keyboard shortcuts. Visibility state is global (not per-shot). The existing `AspectRatioMaskView` provides the exact pattern to follow: a `MonoBehaviour` on a `UIDocument` GameObject that builds UI Toolkit `VisualElement` overlays programmatically, gets the unmasked rect from `CameraBehaviour`, and updates every frame.

No composition guide code exists yet. No `OverlaySettings` class exists in Core. The feature requires new types in Core (pure C# visibility state), a new view in UI (rendering), keyboard shortcut additions to `CameraInputHandler`, and a bootstrap entry in `SceneBootstrap`.

---

## Spec Requirements (from milestone-1.2-camera-overlays-spec.md)

### Three guide types:
1. **Rule of thirds** — 2 horizontal + 2 vertical lines at 1/3 and 2/3 of the unmasked area
2. **Center cross** — small crosshair at exact center, **fixed pixel size** (not proportional to frame)
3. **Safe zones** — two concentric rectangles:
   - Title safe: 90% of unmasked dimensions (5% inset per side)
   - Action safe: 93% of unmasked dimensions (3.5% inset per side)

### Visibility rules:
- All start hidden
- Each toggled independently: `Shift+G` = thirds, `Ctrl+G` = center cross, `Alt+G` = safe zones
- `G` toggles all as a group: if any visible → hide all; if none visible → show all that were previously enabled
- Global state, not per-shot
- Safe zone percentages are configurable with defaults

### Rendering rules:
- Drawn only within the unmasked area (respect aspect ratio mask)
- Semi-transparent (do not obscure scene)
- Z-index 6 (above aspect ratio masks at 5, below camera info at 10)
- Update when aspect ratio changes or window resizes
- Center cross remains same pixel size on window resize

### Visual spec (from ui-layout-spec.md §3.2):

| Guide | Appearance | Opacity |
|-------|-----------|---------|
| Center cross | Two short lines at frame center (4% each axis) | 0.25 |
| Rule of thirds | Two vertical + two horizontal at 33.3%/66.6% | 0.15 |
| Safe title | Dashed rectangle at 5% inset | 0.12 |
| Safe action | Shorter-dashed rectangle at 3.5% inset | 0.08 |

Color: white at varying opacity. Stroke width: 1–1.5px.

**Note**: The spec says center cross size is 4% of each axis in the ui-layout-spec, but the milestone spec says "fixed pixel size." The milestone spec takes precedence (it's more specific). The center cross should be a fixed pixel size — the 4% figure from the layout spec was likely an initial approximation.

---

## File Map

### Existing code to reference or modify:

| File | Purpose |
|------|---------|
| `Unity/Fram3d/Assets/Scripts/UI/Views/AspectRatioMaskView.cs` | **Primary pattern** — overlay MonoBehaviour building UI Toolkit elements |
| `Unity/Fram3d/Assets/Scripts/Core/Camera/AspectRatio.cs` | `ComputeUnmaskedRect()` — computes the rect guides draw within |
| `Unity/Fram3d/Assets/Scripts/Core/Camera/UnmaskedRect.cs` | Value type for the unmasked area (X, Y, Width, Height) |
| `Unity/Fram3d/Assets/Scripts/Engine/Integration/CameraBehaviour.cs` | Integration layer — exposes `ActiveAspectRatio` and `ActiveSensorMode` |
| `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs` | Keyboard shortcuts — add G, Shift+G, Ctrl+G, Alt+G |
| `Unity/Fram3d/Assets/Scripts/Editor/SceneBootstrap.cs` | Scene setup — add composition guide GameObject with UIDocument |
| `Unity/Fram3d/Assets/Scripts/UI/Panels/Theme.cs` | Shared theme constants |

### New files to create:

| File | Layer | Purpose |
|------|-------|---------|
| `Unity/Fram3d/Assets/Scripts/Core/Camera/CompositionGuideSettings.cs` | Core | Pure C# visibility state for three guide types + safe zone percentages |
| `Unity/Fram3d/Assets/Scripts/UI/Views/CompositionGuideView.cs` | UI | MonoBehaviour that renders guide lines via UI Toolkit |
| `tests/Fram3d.Core.Tests/Camera/CompositionGuideSettingsTests.cs` | Tests | Unit tests for visibility toggle logic |

---

## How It Works: Existing Pattern (AspectRatioMaskView)

The overlay pattern follows a consistent lifecycle:

1. **MonoBehaviour on a UIDocument GameObject** — `AspectRatioMaskView` is attached to a GameObject named "Aspect Ratio Mask" that also has a `UIDocument` component with `PanelSettings`.

2. **`Start()`** — finds `CameraBehaviour` via `FindAnyObjectByType<CameraBehaviour>()`, gets `UIDocument.rootVisualElement`, calls `BuildOverlay()`.

3. **`BuildOverlay()`** — creates a full-screen container (`Position.Absolute`, all edges 0), sets `pickingMode = Ignore`, creates child elements, inserts into root at index 0. Registers `GeometryChangedEvent` callback.

4. **`Update()`** — calls `UpdateBars()` every frame. This ensures the overlay tracks aspect ratio changes and window resizes without event-driven coupling.

5. **Getting the unmasked rect** — calls `this._cameraBehaviour.ActiveAspectRatio.ComputeUnmaskedRect(viewWidth, viewHeight, this._cameraBehaviour.ActiveSensorMode)` using the container's `resolvedStyle.width/height`.

### Scene wiring (SceneBootstrap):
- Creates a separate GameObject per overlay type
- Adds `UIDocument` with shared `PanelSettings`
- Uses `sortingOrder` for z-ordering: mask = 0, properties panel = 1
- Composition guides should use `sortingOrder` between mask (0) and panel (1) — so `sortingOrder = 0` on the same document, or a separate document at order 0 with guides inserted above masks

### Z-index consideration:
The spec says guides are z-index 6 (above masks at 5). Since `AspectRatioMaskView` inserts its container at index 0 of the root, composition guides should insert after the mask container (or use a higher sorting order on a separate UIDocument). The simplest approach: **separate UIDocument GameObject with `sortingOrder = 0`** and insert the guide container at root index 0. Since UI Toolkit renders documents in sorting order and elements within a document in tree order, this will work if guides use a separate UIDocument at the same or higher sort order, or if they share the same UIDocument and insert after the mask container.

The cleanest approach matching the existing pattern: **new GameObject "Composition Guides" with its own UIDocument at `sortingOrder = 0`**. The sort order ties with the mask document, but since separate UI documents at the same sort order render in the order Unity processes them (which depends on script execution order), it's safer to give the guide document `sortingOrder = 1` (bump properties panel to 2).

---

## How It Works: Keyboard Shortcuts (CameraInputHandler)

Shortcuts are handled in `HandleKeyboardInput()` (line 112), which runs in `Update()` after checking for focused text fields. The pattern for each shortcut:

```csharp
if (keyboard.gKey.wasPressedThisFrame
 && !keyboard.ctrlKey.isPressed
 && !keyboard.altKey.isPressed
 && !keyboard.shiftKey.isPressed)
{
    // unmodified G — toggle all
}
```

Modified keys check the specific modifier:
```csharp
if (keyboard.gKey.wasPressedThisFrame && keyboard.shiftKey.isPressed
 && !keyboard.ctrlKey.isPressed && !keyboard.altKey.isPressed)
{
    // Shift+G — toggle thirds
}
```

The input handler has `[SerializeField]` references to components it needs. A reference to the composition guide view (or its settings) will be needed so the input handler can call toggle methods.

Current `[SerializeField]` fields on `CameraInputHandler`:
- `cameraBehaviour` (CameraBehaviour)
- `propertiesPanel` (PropertiesPanelView)

---

## Core Domain Design: CompositionGuideSettings

This belongs in `Fram3d.Core.Camera` (pure C#, no Unity). It holds:

- `ThirdsVisible` (bool, default false)
- `CenterCrossVisible` (bool, default false)
- `SafeZonesVisible` (bool, default false)
- `TitleSafePercent` (float, default 0.90)
- `ActionSafePercent` (float, default 0.93)

The `G` key "toggle all" behavior needs a "previously enabled" memory:
- If any guide is currently visible, `ToggleAll()` saves which ones are on, then hides all
- If none are visible, `ToggleAll()` restores the previously saved set (or shows all three if nothing was previously enabled)

This logic is pure C# and testable without Unity.

---

## Rendering: CompositionGuideView

The view creates UI Toolkit `VisualElement` elements for each guide type:

### Rule of Thirds (4 lines):
- 2 vertical lines at 1/3 and 2/3 of unmasked width
- 2 horizontal lines at 1/3 and 2/3 of unmasked height
- White, opacity 0.15, 1px width
- Positioned absolutely within the unmasked rect

### Center Cross (2 lines):
- 1 horizontal + 1 vertical line at the center
- Fixed pixel size (e.g., 20px each direction — needs tuning)
- White, opacity 0.25, 1.5px width

### Safe Zones (2 rectangles):
- Title safe: rectangle at 90% of unmasked dimensions, centered
- Action safe: rectangle at 93% of unmasked dimensions, centered  
- Drawn as border-only VisualElements (transparent background, white border)
- Title safe: dashed (UI Toolkit doesn't support dashed borders natively — may need to use 4 VisualElements per side or a solid thin border)
- Opacity: title 0.12, action 0.08

**UI Toolkit limitation**: `VisualElement` borders cannot be dashed. Options:
1. Use solid borders at the spec's opacity (simpler, visually close enough)
2. Use a series of small VisualElements along each edge to simulate dashes (complex, brittle)
3. Use a custom mesh/painter via `generateVisualContent` callback

Option 1 is the pragmatic choice. The opacity difference (0.12 vs 0.08) between title and action safe already provides visual distinction.

### Update loop:
Every frame in `Update()`:
- Read visibility state from `CompositionGuideSettings`
- Set `display = DisplayStyle.None` or `DisplayStyle.Flex` per guide group
- Compute unmasked rect (same as AspectRatioMaskView)
- Position guide elements within the unmasked rect

---

## Existing Patterns to Follow

### Closed type set (AspectRatio pattern):
`AspectRatio` uses a sealed class with private constructor and static readonly instances. `CompositionGuideSettings` doesn't need this pattern — it's mutable state, not a closed set of values.

### MonoBehaviour overlay pattern:
```
MonoBehaviour + UIDocument → Start() finds CameraBehaviour → BuildOverlay() → Update() recalculates
```

### Test pattern:
Tests use `FunctionName__ExpectedBehavior__When__Condition` naming. `CompositionGuideSettings` tests should cover:
- Default state (all hidden)
- Individual toggles
- `ToggleAll` when some visible → hides all
- `ToggleAll` when none visible → restores previously enabled
- `ToggleAll` when none visible and no prior state → shows all
- Safe zone percentage defaults
- Safe zone percentage validation (clamped to reasonable range)

---

## Assembly Boundaries

| Type | Assembly | Layer |
|------|----------|-------|
| `CompositionGuideSettings` | `Fram3d.Core` | Domain (pure C#) |
| `CompositionGuideView` | `Fram3d.UI` | Presentation (Unity) |
| `CameraInputHandler` changes | `Fram3d.UI` | Presentation (Unity) |
| `SceneBootstrap` changes | Editor scripts | Editor only |

`CompositionGuideSettings` must not import `UnityEngine`. It uses no math types — just bools and floats.

---

## Where State Lives

The spec says guide visibility is **global, not per-shot**. This means:
- `CompositionGuideSettings` is NOT on `CameraElement` (which is per-camera/per-shot)
- It should be a standalone instance, probably owned by `CameraBehaviour` or a future `OverlaySettings` container
- For now, the simplest ownership: `CompositionGuideView` creates and owns a `CompositionGuideSettings` instance, and `CameraInputHandler` gets a reference to call toggle methods

Alternative: `CameraElement` owns it (since it already owns `ActiveAspectRatio` which is also global). But `ActiveAspectRatio` is on `BodyController` which is part of the camera equipment model. Composition guides are purely a display preference, not camera equipment. Keeping it separate is cleaner.

The pragmatic approach: `CompositionGuideView` owns the settings. `CameraInputHandler` gets a `[SerializeField]` reference to `CompositionGuideView` and calls toggle methods on it. This matches how the properties panel toggle works (`CameraInputHandler` has a `[SerializeField] PropertiesPanelView propertiesPanel` and calls `Toggle()` on it).

---

## Key References

- `Unity/Fram3d/Assets/Scripts/UI/Views/AspectRatioMaskView.cs` — entire file is the overlay pattern
- `Unity/Fram3d/Assets/Scripts/Core/Camera/AspectRatio.cs:46-103` — `ComputeUnmaskedRect()` method
- `Unity/Fram3d/Assets/Scripts/Core/Camera/UnmaskedRect.cs` — value type for unmasked area
- `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs:112-194` — keyboard shortcut pattern
- `Unity/Fram3d/Assets/Scripts/Editor/SceneBootstrap.cs:62-83` — overlay GameObject setup pattern
- `Unity/Fram3d/Assets/Scripts/UI/Panels/Theme.cs` — shared color constants
- `docs/specs/milestone-1.2-camera-overlays-spec.md:117-217` — full 1.2.2 spec
- `docs/specs/ui-layout-spec.md:131-142` — visual spec for guide appearance
- `docs/reference/interaction-patterns.md:34-39` — keyboard overlay shortcuts
- `tests/Fram3d.Core.Tests/Camera/AspectRatioTests.cs` — test naming and assertion patterns

## Open Questions

1. **Center cross pixel size**: The spec says "fixed pixel size" but doesn't specify the exact value. The ui-layout-spec says "4% each axis" which contradicts "fixed pixel size." The milestone spec should take precedence. A reasonable starting value: 20px arm length (40px total span). This will need tuning.

2. **Dashed borders for safe zones**: UI Toolkit doesn't support dashed borders natively. Solid borders at the specified low opacities are likely sufficient. If dashed appearance is important, `generateVisualContent` with a custom painter could draw dashed lines.

3. **Safe zone configuration UI**: The spec says safe zone percentages are "configurable with defaults" but doesn't specify where the configuration UI lives. This could go in the properties panel or be deferred to a future settings panel (noted as pending in decisions.md). For now, the domain type supports configurable percentages; UI for changing them can come later.

4. **Sort order for UIDocument**: The composition guide view needs its own UIDocument GameObject. The sort order should be between the mask (0) and properties panel (1). Options: give guides sortingOrder = 0 and rely on element insertion order within the same document, or bump properties panel to sortingOrder = 2 and give guides sortingOrder = 1. The latter is more explicit and reliable.
