# Raw Research Findings: Flicker-Free Cursor Changes on macOS in Unity

## Queries Executed
1. "unity Cursor.SetCursor flicker macos" - 3 useful results
2. "unity custom cursor hover 3D objects macOS flickering fix" - 2 useful results
3. "unity UI Toolkit cursor style USS pointer link how it works internally" - 3 useful results
4. "unity NSCursor macOS native cursor plugin github" - 2 useful results (NativeCursor, Unity.NativeCursors)
5. "unity hardware cursor CursorMode.Auto vs ForceSoftware macOS" - 2 useful results
6. "macOS NSCursor resetCursorRects cursorUpdate tracking area cursor flicker cocoa" - 3 useful results
7. "unity macOS cursor reset mouse move NSCursor push pop workaround" - 1 useful result
8. "unity 6 UI Toolkit cursor runtime native keyword support 2025 2026" - 2 useful results
9. "unity macOS Cursor.SetCursor internally uses NSCursor" - 1 useful result
10. "unity fullscreen VisualElement pickingMode.Ignore cursor style change 3D scene hover" - 2 useful results
11. "BlenMiner NativeCursor Harmony patch" - 1 useful result

## Findings

### Finding 1: macOS cursor flickering is caused by calling `NSCursor.set()` in response to mouse-moved events instead of using the Cocoa cursor-rect system
- **Confidence**: HIGH
- **Supporting sources**:
  - [Apple Developer: Mouse-Tracking and Cursor-Update Events](https://developer.apple.com/library/archive/documentation/Cocoa/Conceptual/EventOverview/MouseTrackingEvents/MouseTrackingEvents.html) - "Cursor rectangles are a specialized type of tracking rectangle for automatic cursor management." When using cursor rects via `resetCursorRects`, the Application Kit manages cursor changes centrally with no flickering. Manual cursor setting in `mouseMoved:` causes repeated cursor-setting calls, causing flicker.
  - [CocoaDev: CursorFlashingProblems](https://cocoadev.github.io/CursorFlashingProblems/) - Documents that cursor flashing occurs when the cursor stack conflicts with other views (e.g., NSScrollView pushing its default cursor). The fix is to call `[NSWindow disableCursorRects]` during operations and `[NSWindow enableCursorRects]` when done.
  - [Apple Developer: Setting the Current Cursor](https://developer.apple.com/library/archive/documentation/Cocoa/Conceptual/CursorMgmt/Tasks/ChangingCursors.html) - Four ways to set cursor: (1) direct `set`, (2) push/pop stack, (3) cursor rectangles on mouse enter, (4) cursor rectangles on mouse exit. Cursor rectangles via `resetCursorRects` are the recommended persistent approach.
- **Notes**: This is the root cause. Unity's `Cursor.SetCursor` likely calls `[[NSCursor alloc] initWithImage:hotspot:] set` directly, which conflicts with macOS's own cursor rect system that resets the cursor on every mouse-moved event. The proper Cocoa pattern is `addCursorRect:cursor:` inside `resetCursorRects`, which the Application Kit manages automatically.

### Finding 2: Unity's `Cursor.SetCursor` creates a hardware cursor from a Texture2D on macOS -- it does NOT use native system cursor shapes
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Scripting API: Cursor.SetCursor](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Cursor.SetCursor.html) - "Changes the appearance of the hardware mouse pointer by setting a custom cursor texture." Requires Read/Write enabled, RGBA32 format. Pass `null` to restore default system cursor.
  - [Unity Scripting API: CursorMode](https://docs.unity3d.com/ScriptReference/CursorMode.html) - `CursorMode.Auto` uses hardware cursors on supported platforms (managed by OS, framerate-independent). `CursorMode.ForceSoftware` renders cursor via Unity after everything else.
  - [Unity Blog: Cursor API](https://unity.com/blog/technology/cursor-api) - Hardware cursors on macOS are framerate-independent and work like OS cursors. On Windows, limited to 32x32. macOS allows custom sizes.
- **Notes**: `Cursor.SetCursor` only accepts a `Texture2D` -- there is no overload to request a system cursor shape (hand, resize, etc.). To get a native hand cursor, you need either: (a) a pixel-perfect Texture2D that matches the system cursor, or (b) bypass Unity entirely with a native plugin that calls NSCursor class methods directly.

### Finding 3: The BlenMiner/NativeCursor plugin solves this by P/Invoking into a compiled Objective-C bundle that calls `[[NSCursor pointingHandCursor] set]`
- **Confidence**: HIGH
- **Supporting sources**:
  - [GitHub: BlenMiner/NativeCursor - CursorWrapper.m](https://github.com/BlenMiner/NativeCursor/tree/main/MacOS) - Objective-C implementation with 10 functions (`SetCursorToArrow`, `SetCursorToPointingHand`, etc.), each dispatching to main thread and calling `[[NSCursor cursorType] set]`.
  - [GitHub: BlenMiner/NativeCursor - MacOSCursorService.cs](https://github.com/BlenMiner/NativeCursor/tree/main/Assets/NativeCursor/Scripts/Native/MacOS) - C# service using `[DllImport("CursorWrapper")]` to P/Invoke the native functions. Maps an `NTCursors` enum to native calls via switch statement.
  - [NativeCursor Documentation](https://gameobject.xyz/nativecursor/) - "Relies on P/Invoke to call the native APIs of each platform."
- **Notes**: The NativeCursor plugin ships a compiled `.bundle` for macOS. The Objective-C wrapper is straightforward -- each function is just `dispatch_async(dispatch_get_main_queue(), ^{ [[NSCursor cursorType] set]; })`. **However, this approach has the same flickering problem as `Cursor.SetCursor`** because it uses `NSCursor.set` (direct set) rather than the Cocoa cursor-rect system. The plugin includes `0Harmony.dll` (Harmony patching library) in its Plugins directory, suggesting it may use runtime method patching to intercept Unity's cursor reset -- but this was not confirmed from the available source code.

### Finding 4: The NativeCursor plugin uses a cursor stack with priority-based management and pause-rendering to reduce flicker
- **Confidence**: MEDIUM
- **Supporting sources**:
  - [GitHub: BlenMiner/NativeCursor - CursorStack.cs](https://github.com/BlenMiner/NativeCursor/tree/main/Assets/NativeCursor/Scripts/Core) - Priority-based stack where push/pop only trigger `OnStackChanged()` when the top-of-stack item actually changes. Includes `PauseRendering(bool)` to decouple stack updates from visual changes, preventing rapid visual transitions.
  - [GitHub: BlenMiner/NativeCursor - NativeCursor.cs](https://github.com/BlenMiner/NativeCursor/tree/main/Assets/NativeCursor/Scripts/Core) - Facade class with a guard (`if (_instance == service) return`) to prevent reassigning the same service. But `SetCursor()` itself has no guard against repeated calls with the same cursor.
- **Notes**: The stack-based approach is a client-side mitigation (don't call the native layer unnecessarily) rather than a fix for the root cause (macOS resetting the cursor on mouse-moved events). It reduces flicker frequency but may not eliminate it entirely.

### Finding 5: UI Toolkit cursor keywords do NOT work at runtime -- only in the Editor
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Discussions: USS cursor via variables](https://discussions.unity.com/t/uss-setting-cursor-via-variables/842802) - "Cursor keywords don't work in runtime UI. In runtime UI, you must use a texture for custom cursors."
  - [Unity Discussions: Allow native cursors at runtime](https://discussions.unity.com/t/allow-native-cursors-to-be-used-at-runtime-too/1536012) - Feature request for native cursor keyword support at runtime. Not yet implemented as of 2025.
  - [Unity Discussions: Setting cursor styles at runtime](https://discussions.unity.com/t/setting-cursor-cursor-styles-at-runtime-using-c/930957) - Confirms runtime limitation: only texture-based cursors work.
  - [com.unity.ui CHANGELOG](https://github.com/needle-mirror/com.unity.ui/blob/master/CHANGELOG.md) - "Fixed bug where runtime cursor should not be reset unless it was overridden" (1.0.0-preview.15, 2021). This fix prevents Unity from resetting the cursor when no UI element has explicitly set one.
- **Notes**: This rules out the approach of using USS `cursor: link` on a VisualElement overlay at runtime. Even in USS, custom cursors and keywords are mutually exclusive (no fallback chains like CSS). Unity 6.3 (latest LTS, Dec 2025) added UI Toolkit improvements (custom shaders, SVG) but did NOT add runtime native cursor keywords.

### Finding 6: A fullscreen VisualElement overlay with `PickingMode.Ignore` cannot change cursor style (it won't receive hover events)
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Scripting API: PickingMode](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/UIElements.PickingMode.html) - `PickingMode.Ignore` prevents the element from being the target of pointer events or being returned by `IPanel.Pick`.
  - [Unity Scripting API: VisualElement.pickingMode](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/UIElements.VisualElement-pickingMode.html) - Elements with `PickingMode.Ignore` never receive the `:hover` pseudo-state.
- **Notes**: This rules out the "invisible overlay" approach. If a VisualElement has `PickingMode.Ignore` (needed to let clicks pass through to the 3D scene), it cannot receive hover events or trigger cursor changes. If it has `PickingMode.Position`, it blocks interaction with the 3D scene below.

### Finding 7: `Cursor.SetCursor` is expensive when changing textures (15ms+ on Windows) and should only be called when the cursor actually changes
- **Confidence**: MEDIUM
- **Supporting sources**:
  - [Unity Discussions: Cursor.SetCursor slow on Windows](https://discussions.unity.com/t/cursor-setcursor-slow-on-windows-builds/594300) - "Cursor.SetCursor takes a long time whenever it actually changes the texture -- approximately 15ms per change, and 33ms if called twice per frame."
  - [Unity Scripting API: Cursor.SetCursor](https://docs.unity3d.com/ScriptReference/Cursor.SetCursor.html) - The official example uses `OnMouseEnter()`/`OnMouseExit()` callbacks, not `Update()`, showing the intended usage pattern.
- **Notes**: The 15ms figure is Windows-specific. macOS performance may differ. But the principle holds: avoid calling `SetCursor` every frame. Track the current cursor state and only call when transitioning.

### Finding 8: The proper macOS approach is `cursorUpdate:` with `NSTrackingArea` (NSTrackingCursorUpdate option), not direct `NSCursor.set()` in response to mouse events
- **Confidence**: HIGH
- **Supporting sources**:
  - [Apple Developer: Mouse-Tracking and Cursor-Update Events](https://developer.apple.com/library/archive/documentation/Cocoa/Conceptual/EventOverview/MouseTrackingEvents/MouseTrackingEvents.html) - "You change the cursor image for your view in an override of the NSResponder method `cursorUpdate:`. To receive this message, create an NSTrackingArea with NSTrackingCursorUpdate option." The Application Kit handles cursor updates centrally when using this API, avoiding flickering.
  - [Apple Developer: Using Tracking-Area Objects](https://developer.apple.com/library/archive/documentation/Cocoa/Conceptual/EventOverview/TrackingAreaObjects/TrackingAreaObjects.html) - NSTrackingArea is the modern replacement for the legacy cursor-rect APIs. Options include `NSTrackingCursorUpdate`, `NSTrackingActiveAlways`, `NSTrackingMouseEnteredAndExited`.
- **Notes**: The key insight is that macOS has a built-in cursor management system. When you use `cursorUpdate:`, macOS knows a custom cursor is active and doesn't try to reset it. When you use `NSCursor.set()` in `mouseMoved:`, macOS doesn't know about your cursor and resets it on every mouse-moved event. This is the fundamental architectural difference.

### Finding 9: The SwiftUI approach uses `NSCursor.push()` with a guard against redundant pushes
- **Confidence**: MEDIUM
- **Supporting sources**:
  - [GitHub Gist: SwiftUI cursor on hover](https://gist.github.com/Amzd/cb8ba40625aeb6a015101d357acaad88) - Uses `cursor.push()` on hover enter (with `guard NSCursor.current != cursor` to prevent redundant pushes) and `NSCursor.pop()` on hover exit. On macOS 13+, uses `onContinuousHover` with phase tracking.
- **Notes**: `push()`/`pop()` is different from `set()`. `push()` adds to the cursor stack, and the system respects the top of the stack. This is more robust than `set()` for overlapping regions. However, in Unity's context, this still has the same fundamental problem -- Unity's own window management may reset the cursor between your push and when macOS processes it.

### Finding 10: Unity's CursorMode.ForceSoftware avoids macOS cursor conflicts but introduces lag
- **Confidence**: MEDIUM
- **Supporting sources**:
  - [Unity Scripting API: CursorMode](https://docs.unity3d.com/ScriptReference/CursorMode.html) - "ForceSoftware: Unity will render the cursor for you after everything else in the scene has been rendered." Hardware cursors (Auto) are OS-managed and framerate-independent.
  - [Unity Discussions: Cursor.SetCursor not working on Mac](https://discussions.unity.com/t/using-custom-cursor-texture-with-cursor-setcursor-is-not-working-on-mac/79928) - "When setting it to ForceSoftware mode, the problem does not occur, though this approach can be laggy."
- **Notes**: ForceSoftware eliminates the macOS cursor-reset conflict because Unity renders the cursor itself (bypassing NSCursor entirely). But software cursors trail behind the actual pointer position by at least one frame, which looks unprofessional for a native-feeling app. Not suitable for a previsualization tool.

### Finding 11: Calling `Cursor.SetCursor` every frame with the SAME texture is cheap and may prevent flickering on some platforms
- **Confidence**: LOW
- **Supporting sources**:
  - Multiple Unity forum posts mention that "Cursor.SetCursor may need to be called every frame to work reliably" as a workaround. The expensive part is texture upload, which Unity may skip when the texture hasn't changed.
- **Notes**: This is a brute-force workaround. It "outpaces" the macOS cursor reset by re-setting the cursor every frame. Whether this actually eliminates visible flicker on macOS depends on timing (does Unity's cursor set happen before or after macOS's cursor reset in the same frame?). This approach is fragile and platform-dependent.

## Contradictions

### Whether NativeCursor's `NSCursor.set()` approach actually prevents flickering on macOS
- **NativeCursor plugin** (GitHub): Calls `[[NSCursor pointingHandCursor] set]` via `dispatch_async` to main thread. The plugin includes Harmony (0Harmony.dll) suggesting it may patch Unity internals to prevent cursor reset, but no confirmation.
- **Apple documentation**: States that calling `NSCursor.set()` manually will flicker because macOS resets the cursor on mouse-moved events. The proper approach is `cursorUpdate:` with `NSTrackingArea`.
- **Assessment**: The NativeCursor plugin may work flicker-free if Harmony patches intercept Unity's cursor management, or it may still flicker. Without access to the Harmony patch code, this is unresolved.

### Whether calling Cursor.SetCursor every frame is a valid workaround
- **Some forum users**: Report that calling SetCursor every frame prevents flickering.
- **Other users and Apple docs**: Setting the cursor on every mouse event is exactly what causes flickering on macOS.
- **Assessment**: This likely depends on whether Unity re-uploads the texture or just re-sets the NSCursor. If Unity caches the NSCursor object and just calls `set` again, it may work but at the cost of fighting the OS cursor system every frame.

## Analysis: Viable Approaches for Fram3d

### Approach A: Native macOS plugin using `cursorUpdate:` with `NSTrackingArea` (Best theoretical solution)
Write an Objective-C `.bundle` plugin that:
1. Gets the Unity player window's `NSView`
2. Creates an `NSTrackingArea` covering the full view with `NSTrackingCursorUpdate | NSTrackingActiveAlways`
3. Overrides `cursorUpdate:` to set the appropriate NSCursor based on a value set from C#
4. Exposes C functions like `SetDesiredCursor(int cursorType)` and `ResetDesiredCursor()` via P/Invoke

**Pros**: Works with macOS's own cursor system. No flickering by design. Uses actual OS cursors (perfect Retina rendering). Framerate-independent.
**Cons**: Requires Objective-C native plugin development. Must find/subclass Unity's NSView. May conflict with Unity's own cursor management. macOS-only (would need Windows implementation too).

### Approach B: `Cursor.SetCursor` with texture-based cursors and state tracking (Simplest cross-platform)
1. Create Texture2D assets that match system cursor shapes (arrow, hand pointer)
2. Track the current cursor state in C# (enum)
3. Only call `Cursor.SetCursor` on state transitions (not every frame)
4. Use `CursorMode.Auto` for hardware cursor

**Pros**: Cross-platform. Simple. No native code.
**Cons**: May flicker on macOS on state transitions. Textures won't match system cursor perfectly (different across macOS versions, Retina scaling). Won't respect user's cursor size/color accessibility settings.

### Approach C: NativeCursor plugin (BlenMiner) as a dependency
Use the existing NativeCursor Unity package which wraps NSCursor calls.

**Pros**: Already implemented. Cursor stack with priority management. Cross-platform. Uses actual OS cursors.
**Cons**: Third-party dependency. May still flicker (uses `NSCursor.set()` not `cursorUpdate:`). Includes Harmony which is a heavy dependency for runtime patching. Asset Store package (commercial).

### Approach D: Minimal native plugin using NSCursor.set() with per-frame re-application
Write a minimal Objective-C plugin (like NativeCursor's CursorWrapper.m pattern) but call `set` every frame from Unity's `LateUpdate`.

**Pros**: Small, self-contained. Uses actual OS cursors. No third-party dependencies.
**Cons**: Fights the OS every frame. May still show momentary flicker between Unity's cursor reset and the re-application.

## Source Registry
| # | Title | URL | Date | Queries |
|---|-------|-----|------|---------|
| 1 | Unity Scripting API: Cursor.SetCursor (6000.3) | https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Cursor.SetCursor.html | Current | Q1, Q5 |
| 2 | Unity Scripting API: CursorMode | https://docs.unity3d.com/ScriptReference/CursorMode.html | Current | Q5 |
| 3 | GitHub: BlenMiner/NativeCursor | https://github.com/BlenMiner/NativeCursor | Active | Q4 |
| 4 | GitHub: BlenMiner/NativeCursor - CursorWrapper.m | https://github.com/BlenMiner/NativeCursor/tree/main/MacOS | Active | Q4 |
| 5 | GitHub: BlenMiner/NativeCursor - MacOSCursorService.cs | https://github.com/BlenMiner/NativeCursor/tree/main/Assets/NativeCursor/Scripts/Native/MacOS | Active | Q4, Q11 |
| 6 | GitHub: BlenMiner/NativeCursor - CursorStack.cs | https://github.com/BlenMiner/NativeCursor/tree/main/Assets/NativeCursor/Scripts/Core | Active | Q4 |
| 7 | GitHub: starburst997/Unity.NativeCursors | https://github.com/starburst997/Unity.NativeCursors | WIP/Empty | Q4 |
| 8 | NativeCursor Documentation | https://gameobject.xyz/nativecursor/ | Active | Q4 |
| 9 | Apple Developer: Mouse-Tracking and Cursor-Update Events | https://developer.apple.com/library/archive/documentation/Cocoa/Conceptual/EventOverview/MouseTrackingEvents/MouseTrackingEvents.html | Archive | Q6 |
| 10 | Apple Developer: Setting the Current Cursor | https://developer.apple.com/library/archive/documentation/Cocoa/Conceptual/CursorMgmt/Tasks/ChangingCursors.html | Archive | Q6 |
| 11 | CocoaDev: CursorFlashingProblems | https://cocoadev.github.io/CursorFlashingProblems/ | Archive | Q6 |
| 12 | Unity Discussions: Allow native cursors at runtime | https://discussions.unity.com/t/allow-native-cursors-to-be-used-at-runtime-too/1536012 | 2024 | Q3, Q8 |
| 13 | Unity Discussions: USS cursor via variables | https://discussions.unity.com/t/uss-setting-cursor-via-variables/842802 | 2023 | Q3 |
| 14 | Unity Discussions: Cursor.SetCursor slow on Windows | https://discussions.unity.com/t/cursor-setcursor-slow-on-windows-builds/594300 | 2019 | Q1 |
| 15 | Unity Scripting API: PickingMode | https://docs.unity3d.com/6000.3/Documentation/ScriptReference/UIElements.PickingMode.html | Current | Q10 |
| 16 | Unity Scripting API: UIElements.Cursor | https://docs.unity3d.com/ScriptReference/UIElements.Cursor.html | Current | Q3, Q10 |
| 17 | GitHub Gist: SwiftUI cursor on hover (Amzd) | https://gist.github.com/Amzd/cb8ba40625aeb6a015101d357acaad88 | 2022 | Q6 |
| 18 | com.unity.ui CHANGELOG | https://github.com/needle-mirror/com.unity.ui/blob/master/CHANGELOG.md | Ongoing | Q3, Q8 |
| 19 | Unity Discussions: Setting hardware cursor to native OS pointers | https://discussions.unity.com/t/setting-hardware-cursor-to-native-os-pointers/1518418 | 2024 | Q4, Q11 |
| 20 | Apple Developer: NSCursor Documentation | https://developer.apple.com/documentation/appkit/nscursor | Current | Q6 |
| 21 | Unity Blog: Cursor API | https://unity.com/blog/technology/cursor-api | 2012 | Q5 |
| 22 | Native Cursor - Unity Asset Store | https://assetstore.unity.com/packages/tools/utilities/native-cursor-220347 | Active | Q11 |
| 23 | Unity Manual: Building plug-ins for desktop platforms | https://docs.unity3d.com/6000.2/Documentation/Manual/plug-ins-for-desktop.html | Current | Q4 |
