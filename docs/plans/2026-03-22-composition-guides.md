# Implementation Plan: 1.2.2 Composition Guides

**Ticket**: FRA-43
**Date**: 2026-03-22

---

## Overview

Three overlay guide types drawn within the unmasked area: rule of thirds, center cross, safe zones. All start hidden, toggled independently via keyboard shortcuts, global visibility state.

## Phase 1: Core — CompositionGuideSettings

**File**: `Unity/Fram3d/Assets/Scripts/Core/Camera/CompositionGuideSettings.cs`

Pure C# class. No Unity dependencies.

- `ThirdsVisible`, `CenterCrossVisible`, `SafeZonesVisible` — bool properties
- `TitleSafePercent` (default 0.90), `ActionSafePercent` (default 0.93) — float properties
- `ToggleThirds()`, `ToggleCenterCross()`, `ToggleSafeZones()` — flip individual
- `ToggleAll()` — if any visible → hide all and remember which were on. If none visible → restore remembered set (or show all if nothing was previously enabled).
- `AnyVisible` — computed property

**Tests**: `tests/Fram3d.Core.Tests/Camera/CompositionGuideSettingsTests.cs`
- Defaults: all hidden
- Individual toggles flip independently
- ToggleAll hides all when any visible
- ToggleAll restores previously remembered set
- ToggleAll shows all when nothing was previously enabled
- Safe zone percentages configurable

## Phase 2: UI — CompositionGuideView

**File**: `Unity/Fram3d/Assets/Scripts/UI/Views/CompositionGuideView.cs`

MonoBehaviour following `AspectRatioMaskView` pattern. Finds `CameraBehaviour` on Start, reads unmasked rect each frame, positions guide elements.

### Visual elements

- **Rule of thirds**: 4 absolutely positioned elements (2 horizontal lines, 2 vertical lines) at 33.3%/66.6% of the unmasked rect. White, opacity 0.15, 1px height/width.
- **Center cross**: 2 elements (horizontal + vertical) at center of unmasked rect. Fixed 20px arm length, white, opacity 0.25, 1.5px stroke.
- **Title safe**: 4 border elements forming a rectangle at 90% of unmasked area (5% inset each side). White, opacity 0.12, 1px border.
- **Action safe**: 4 border elements forming a rectangle at 93% of unmasked area (3.5% inset each side). White, opacity 0.08, 1px border.

All elements: `pickingMode = Ignore`, `position = Absolute`.

### Public API
- `Settings` property — returns the `CompositionGuideSettings`
- Guides recalculate every `Update()` frame

## Phase 3: Input — Keyboard Shortcuts

**File**: `Unity/Fram3d/Assets/Scripts/UI/Input/CameraInputHandler.cs`

Add `[SerializeField] private CompositionGuideView compositionGuides` field.

Shortcuts (in `HandleKeyboardInput`):
- `G` (no modifiers) → `compositionGuides.Settings.ToggleAll()`
- `Shift+G` → `compositionGuides.Settings.ToggleThirds()`
- `Ctrl+G` → `compositionGuides.Settings.ToggleCenterCross()`
- `Alt+G` → `compositionGuides.Settings.ToggleSafeZones()`

## Phase 4: Scene Wiring

**File**: `Unity/Fram3d/Assets/Scripts/Editor/SceneBootstrap.cs`

Add composition guide GameObject with UIDocument, wire `[SerializeField]` reference on CameraInputHandler.

## Phase 5: Tests

### Core tests (xUnit)
CompositionGuideSettingsTests — toggle logic, ToggleAll memory, defaults, safe zone config.

### Play Mode tests
- CompositionGuideView creates guide elements on Start
- G key toggles all guides
- Shift+G toggles thirds independently
- Guides respect aspect ratio (drawn within unmasked area)
