# Raw Research Findings: Unity Cursor/Pointer Handling

## Queries Executed
1. "Unity Cursor.SetCursor limitations problems Texture2D" - 6 useful results
2. "Unity system cursor native OS cursor NSCursor IDC_HAND P/Invoke" - 5 useful results
3. "Unity 6 cursor API system cursor improvements 2025 2026" - 3 useful results
4. "Unity UI Toolkit cursor style hover pointer change" - 4 useful results
5. "Unity native cursor plugin macOS NSCursor Windows SetCursor P/Invoke platform" - 4 useful results
6. "Unity forum system cursor request feature CursorMode ForceSoftware Auto" - 4 useful results
7. "Unity UIElements cursor keyword built-in cursor types VisualElement" - 3 useful results
8. "macOS NSCursor available system cursors list" - 2 useful results
9. "PurrNet Native Cursor Unity plugin" + GitHub source - 4 useful results
10. "Windows SetSystemCursor LoadCursor IDC_HAND Win32 API" - 3 useful results
11. "Unity ForceSoftware cursor New Input System bug" - 3 useful results
12. "Unity Cursor.SetCursor performance CPU cost" - 3 useful results

---

## Findings

### Finding 1: Unity's Cursor.SetCursor() API requires a Texture2D -- there is no built-in way to use system/native cursors at runtime
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Scripting API: Cursor.SetCursor](https://docs.unity3d.com/ScriptReference/Cursor.SetCursor.html) - The only way to change the cursor is `Cursor.SetCursor(Texture2D texture, Vector2 hotspot, CursorMode cursorMode)`. Pass `null` to restore the default OS arrow. There is no overload accepting a system cursor enum.
  - [Unity Scripting API: Cursor (Unity 6.2)](https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Cursor.html) - The Cursor class has only three members: `lockState`, `visible`, and `SetCursor`. No system cursor API exists.
  - [USS Supported Properties](https://docs.unity3d.com/Manual/UIE-USS-SupportedProperties.html) - USS cursor keywords (arrow, link, text, etc.) are explicitly documented as "only available in the Editor UI. Cursor keywords don't work in runtime UI."
- **Notes**: This is the fundamental limitation. Unity has no `SystemCursor` enum or equivalent for runtime use. The only escape hatch to get back to the default arrow is passing `null` as the texture.

### Finding 2: Cursor.SetCursor() has significant texture requirements and platform quirks
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Scripting API: Cursor.SetCursor](https://docs.unity3d.com/ScriptReference/Cursor.SetCursor.html) - Texture must have Read/Write enabled, RGBA32 format, alphaIsTransparency enabled, no mip chain for code-generated textures.
  - [Unity Issue Tracker: macOS cursor size](https://issuetracker.unity3d.com/issues/macos-cursor-size-is-not-affected-by-cursor-dot-setcursor) - On macOS, custom cursors display at the texture's native pixel size. Status: "By Design." Workaround: restrict cursor assets to 32x32 max in the inspector.
  - [Unity Issue Tracker: Windows max cursor size](https://issuetracker.unity3d.com/issues/windows-max-mouse-cursor-size-is-limited-when-using-cursor-dot-setcursor-and-cannot-be-increased-past-a-certain-threshold) - Windows has a max cursor size limit when using hardware cursors.
  - [Unity Discussions: Cursor scale on macOS / Retina](https://discussions.unity.com/t/cursor-scale-on-macos-retina/759311) - 64x64 cursors on Retina are either 2x too large at 64x64 or blurry when downscaled to 32x32 MaxSize.
- **Notes**: The 32x32 hardware cursor limit is a platform constraint (OS-level), not a Unity bug. macOS Retina handling is particularly problematic because macOS displays textures at their pixel size, not logical point size.

### Finding 3: CursorMode.ForceSoftware is broken with the New Input System
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Discussions: New Input System and CursorMode.ForceSoftware don't work together](https://discussions.unity.com/t/new-input-system-and-cursormode-forcesoftware-dont-work-together/931024) - The software cursor renders but stays stuck at its starting position and doesn't move, because the New Input System doesn't update the software cursor position.
  - [Unity Discussions: Cursor.SetCursor() ForceSoftware not working in new InputSystem](https://discussions.unity.com/t/cursor-setcursor-cursormode-forcesoftware-do-not-working-in-new-inputsystem/247487) - Same bug confirmed by multiple users.
  - [Unity Discussions: Forcing software mode gets cursor stuck](https://discussions.unity.com/t/forcing-software-mode-in-setcursor-gets-the-cursor-stuck/847890) - Further confirmation of the stuck cursor issue.
- **Notes**: This is a long-standing bug. Workaround: enable both old and new input systems simultaneously, or use `CursorMode.Auto` (hardware cursor). This means for Unity projects using only the New Input System, ForceSoftware mode is effectively unusable.

### Finding 4: CursorMode.Auto uses hardware cursors (OS-level), ForceSoftware renders a texture as a sprite
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Scripting API: CursorMode](https://docs.unity3d.com/ScriptReference/CursorMode.html) - `Auto`: "hardware cursors on supported platforms". `ForceSoftware`: "the use of software cursors" (Unity renders the texture as a sprite overlay).
  - [Unity Scripting API: Cursor.SetCursor](https://docs.unity3d.com/ScriptReference/Cursor.SetCursor.html) - Hardware cursor uses the OS cursor mechanism; software cursor is a Unity-rendered texture that follows mouse position.
- **Notes**: Hardware cursors (`Auto`) are always preferred for production because they have zero latency (rendered by the OS compositor, not the game's render loop). Software cursors have 1+ frames of latency and are only useful when you need the cursor captured in screen recordings (e.g., Unity Recorder) or need cursors larger than the hardware limit.

### Finding 5: UI Toolkit (USS) has cursor keywords, but they are EDITOR-ONLY
- **Confidence**: HIGH
- **Supporting sources**:
  - [USS Supported Properties](https://docs.unity3d.com/Manual/UIE-USS-SupportedProperties.html) - Full list of supported keywords: `arrow`, `text`, `resize-vertical`, `resize-horizontal`, `link`, `slide-arrow`, `resize-up-right`, `resize-up-left`, `move-arrow`, `rotate-arrow`, `scale-arrow`, `arrow-plus`, `arrow-minus`, `pan`, `orbit`, `zoom`, `fps`, `split-resize-up-down`, `split-resize-left-right`. Explicit note: "Cursor keywords are only available in the Editor UI. Cursor keywords don't work in runtime UI."
  - [Unity Scripting API: UIElements.Cursor](https://docs.unity3d.com/ScriptReference/UIElements.Cursor.html) - The runtime `Cursor` struct only has `texture` and `hotspot` properties -- no keyword/enum property.
- **Notes**: This is the most frustrating gap. Unity has all the right cursor abstractions for the Editor (the `MouseCursor` enum with 20 values), and USS has CSS-like cursor keywords. But none of this works at runtime. At runtime, `style.cursor` only accepts a Texture2D + hotspot, identical to `Cursor.SetCursor()`.

### Finding 6: The Editor-only MouseCursor enum has 20 cursor types
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Scripting API: MouseCursor](https://docs.unity3d.com/2019.4/Documentation/ScriptReference/MouseCursor.html) - Full enum: Arrow, Text, ResizeVertical, ResizeHorizontal, Link, SlideArrow, ResizeUpRight, ResizeUpLeft, MoveArrow, RotateArrow, ScaleArrow, ArrowPlus, ArrowMinus, Pan, Orbit, Zoom, FPS, CustomCursor, SplitResizeUpDown, SplitResizeLeftRight.
  - [Unity Scripting API: EditorGUIUtility.AddCursorRect](https://docs.unity3d.com/ScriptReference/EditorGUIUtility.AddCursorRect.html) - Used in Editor windows with the MouseCursor enum. Lives in `UnityEditor` namespace, not available at runtime.
- **Notes**: These 20 cursor types map almost 1:1 to the USS cursor keywords. They use the OS system cursors internally. The infrastructure exists inside Unity -- it's just not exposed for runtime builds.

### Finding 7: macOS NSCursor provides 17+ public system cursors
- **Confidence**: HIGH
- **Supporting sources**:
  - [macOS Headers: NSCursor.h](https://github.com/w0lfschild/macOS_headers/blob/master/macOS/Frameworks/AppKit/1559/NSCursor.h) - Public API cursors: `arrowCursor`, `IBeamCursor`, `pointingHandCursor`, `closedHandCursor`, `openHandCursor`, `resizeLeftCursor`, `resizeRightCursor`, `resizeLeftRightCursor`, `resizeUpCursor`, `resizeDownCursor`, `resizeUpDownCursor`, `crosshairCursor`, `disappearingItemCursor`, `operationNotAllowedCursor`, `busyButClickableCursor`, `contextualMenuCursor`, `IBeamCursorForVerticalLayout`. Plus 30+ private underscore-prefixed cursors including `_zoomInCursor`, `_zoomOutCursor`, `_moveCursor`, diagonal resize cursors, etc.
  - [Apple Developer Documentation: NSCursor](https://developer.apple.com/documentation/appkit/nscursor) - (JavaScript-only page, could not fetch content, but URL confirmed as authoritative source)
  - [SDL macOS cursor implementation](https://github.com/libsdl-org/SDL/blob/main/src/video/cocoa/SDL_cocoamouse.m) - SDL maps its system cursors to NSCursor: `SDL_SYSTEM_CURSOR_DEFAULT` -> `arrowCursor`, `SDL_SYSTEM_CURSOR_TEXT` -> `IBeamCursor`, `SDL_SYSTEM_CURSOR_POINTER` -> `pointingHandCursor`, `SDL_SYSTEM_CURSOR_NOT_ALLOWED` -> `operationNotAllowedCursor`, etc. For diagonal resize cursors, SDL loads custom PDF cursors from the system frameworks.
- **Notes**: NSCursor's public API covers the common cases well. The private underscore-prefixed cursors are undocumented but widely used by Apple's own apps. SDL uses them with fallbacks. For a previsualization tool (not a shipping game), using the public API cursors would cover all needed cases.

### Finding 8: Windows Win32 provides system cursors via LoadCursor + IDC_ constants
- **Confidence**: HIGH
- **Supporting sources**:
  - [Microsoft Learn: LoadCursorA function](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-loadcursora) - Pass `hInstance = NULL` and `lpCursorName` as an IDC_ constant to load predefined system cursors.
  - [Microsoft Learn: SetCursor function](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setcursor) - Sets the cursor shape. Returns the previous cursor handle.
  - Available IDC_ constants: `IDC_ARROW`, `IDC_IBEAM`, `IDC_WAIT`, `IDC_CROSS`, `IDC_UPARROW`, `IDC_SIZENWSE`, `IDC_SIZENESW`, `IDC_SIZEWE`, `IDC_SIZENS`, `IDC_SIZEALL`, `IDC_NO`, `IDC_HAND`, `IDC_APPSTARTING`.
- **Notes**: Windows P/Invoke is straightforward: `[DllImport("user32.dll")] static extern IntPtr LoadCursor(IntPtr hInstance, uint lpCursorName)` and `[DllImport("user32.dll")] static extern IntPtr SetCursor(IntPtr hCursor)`. The challenge is that Unity's event loop continuously resets the cursor via WM_SETCURSOR, so you need to hook the window procedure.

### Finding 9: NativeCursor (BlenMiner/PurrNet) is the best available third-party solution
- **Confidence**: HIGH
- **Supporting sources**:
  - [NativeCursor on Unity Asset Store](https://assetstore.unity.com/packages/tools/utilities/native-cursor-220347) - Free, 5/5 stars (6 reviews), supports Windows/macOS/Linux/WebGL, all render pipelines (Built-in, URP, HDRP). Latest version 1.1.3 (Dec 2024). User reviews: "the best cursor solution", "displays the OS' built-in cursors which look professional in desktop applications."
  - [NativeCursor GitHub: ICursorService.cs](https://github.com/BlenMiner/NativeCursor) - Enum `NTCursors`: Default, Arrow, IBeam, Crosshair, Link, Busy, Invalid, ResizeVertical, ResizeHorizontal, ResizeDiagonalLeft, ResizeDiagonalRight, ResizeAll, OpenHand, ClosedHand. API: `NativeCursor.SetCursor(NTCursors.Link)`.
  - [NativeCursor GitHub: MacOSCursorService.cs](https://github.com/BlenMiner/NativeCursor) - macOS implementation uses a native Objective-C plugin ("CursorWrapper") with P/Invoke. Functions: `SetCursorToArrow()`, `SetCursorToIBeam()`, `SetCursorToCrosshair()`, `SetCursorToOpenHand()`, `SetCursorToClosedHand()`, `SetCursorToResizeLeftRight()`, `SetCursorToResizeUpDown()`, `SetCursorToOperationNotAllowed()`, `SetCursorToPointingHand()`, `SetCursorToBusy()`.
  - [NativeCursor GitHub: WindowsCursorPatch.cs](https://github.com/BlenMiner/NativeCursor) - Windows implementation uses P/Invoke to `user32.dll`: `LoadCursor` with IDC_ constants, `SetCursor` to apply, and hooks `WM_SETCURSOR`/`WM_MOUSEMOVE` via window procedure subclassing to prevent Unity from resetting the cursor.
  - [NativeCursor Documentation](https://gameobject.xyz/nativecursor/) - "Cross-platform package that allows you to change the cursor to any of the available cursors on the OS. Relies on P/Invoke to call the native APIs of each platform."
- **Notes**: This is the most complete solution found. It solves the key problem (using OS-native cursors at runtime) with proper platform abstraction. The macOS implementation uses a compiled Objective-C native plugin (not raw objc_msgSend P/Invoke), while Windows uses direct user32.dll P/Invoke with window procedure hooking. Also supports "virtual cursor" mode using .cur/.ani files with pre-bundled cursor packs for Windows, macOS, and Linux themes.

### Finding 10: The Windows implementation requires WM_SETCURSOR hooking because Unity continuously resets the cursor
- **Confidence**: MEDIUM
- **Supporting sources**:
  - [NativeCursor GitHub: WindowsCursorPatch.cs](https://github.com/BlenMiner/NativeCursor) - The Windows implementation hooks `WM_SETCURSOR` and `WM_MOUSEMOVE` messages via window procedure subclassing to intercept and override Unity's default cursor handling. Without this, Unity resets the cursor on every WM_SETCURSOR message.
- **Notes**: This is a key implementation detail. Simply calling `SetCursor()` via P/Invoke is not sufficient on Windows because Unity's main window processes `WM_SETCURSOR` messages and resets the cursor to its own default. You must subclass the window procedure to intercept these messages. This is the same pattern used by other game engines (Unreal, Godot) for custom cursor handling on Windows.

### Finding 11: There is a community feature request for runtime native cursors, but no Unity response
- **Confidence**: MEDIUM
- **Supporting sources**:
  - [Unity Discussions: Allow native cursors to be used at runtime too](https://discussions.unity.com/t/allow-native-cursors-to-be-used-at-runtime-too/1536012) - Feature request to expose the Editor's cursor infrastructure at runtime. (Could not fetch full content - 403 error.)
  - [Unity Discussions: Setting Hardware cursor to native OS pointers](https://discussions.unity.com/t/setting-hardware-cursor-to-native-os-pointers/1518418) - Discussion about using OS-built-in cursors. (Could not fetch full content - 403 error.)
- **Notes**: The community clearly wants this feature. The infrastructure exists in Unity's Editor code. But as of Unity 6.3 (early 2026), there is no indication that Unity plans to expose it for runtime use. The Unity 6.3 release notes (checked directly) contain no cursor-related improvements.

### Finding 12: Unity 6 has no new cursor APIs compared to earlier versions
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity 6.0 Cursor API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Cursor.html) - Same three members: `lockState`, `visible`, `SetCursor`.
  - [Unity 6.1 Cursor.SetCursor](https://docs.unity3d.com/6000.1/Documentation/ScriptReference/Cursor.SetCursor.html) - Same signature, same documentation, no new overloads.
  - [Unity 6.2 Cursor class](https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Cursor.html) - Same three members. No changes.
  - [Unity 6.3 Release Notes](https://docs.unity3d.com/6000.3/Documentation/Manual/WhatsNewUnity63.html) - No cursor-related improvements mentioned.
- **Notes**: The Cursor API has been essentially unchanged since Unity 4.x when the `Cursor.SetCursor` API was introduced (the original Unity blog post about the Cursor API dates to October 2012). Unity 6 adds no improvements.

### Finding 13: Why game engines make cursor changes harder than web (CSS)
- **Confidence**: HIGH (analysis derived from documented facts)
- **Supporting sources**:
  - Multiple sources above collectively explain this.
- **Notes**: In web development, `cursor: pointer` works because browsers own the OS window, handle the WM_SETCURSOR/NSCursor lifecycle, and expose a high-level CSS abstraction that maps to system cursors. Game engines face three problems web doesn't: (1) they render to a single OS window and must manage the cursor lifecycle at the OS level, (2) they typically want games to have custom themed cursors (not system cursors), so the API is designed around Texture2D, and (3) the game's render loop fights with the OS cursor system (hence the WM_SETCURSOR hooking needed on Windows). Unity's API reflects this game-centric worldview -- it never occurred to the Unity team that developers would want the OS's native cursors for tool/productivity applications rather than custom game cursors.

### Finding 14: Cursor.SetCursor() has significant performance overhead when changing textures
- **Confidence**: HIGH
- **Supporting sources**:
  - [Unity Discussions: Cursor.SetCursor() heavy performance cost](https://discussions.unity.com/t/cursor-setcursor-heavy-performance-cost/706608) - On Windows builds, changing the cursor texture costs ~15ms per change, with total overhead of ~33ms/frame. On WebAssembly, 4-58ms per call.
  - [Unity Issue Tracker: Performance hit when icon is set as cursor for first time](https://issuetracker.unity3d.com/issues/high-performance-when-an-icon-is-set-as-a-cursor-for-the-first-time-with-setcursor-on-windows) - Confirmed performance issue on Windows.
  - [Unity Discussions: Cursor.SetCursor slow on Windows builds](https://discussions.unity.com/t/cursor-setcursor-slow-on-windows-builds/594300) - Further confirmation of Windows performance issues.
- **Notes**: The cost comes from texture upload to the OS cursor subsystem each time the texture changes. Calling SetCursor with the same texture repeatedly is cheap (it early-returns). The mitigation is to cache cursor state and only call SetCursor when the desired cursor actually changes. This is another reason system/native cursors are superior for tool applications -- the OS already has the cursor data, so switching between system cursors is essentially free compared to uploading a new texture.

### Finding 15: DIY P/Invoke approach for macOS requires a native Objective-C plugin
- **Confidence**: MEDIUM
- **Supporting sources**:
  - [NativeCursor GitHub: MacOSCursorService.cs](https://github.com/BlenMiner/NativeCursor) - Uses `[DllImport("CursorWrapper")]` to call into a compiled Objective-C native plugin, not direct objc_msgSend calls from C#.
  - [SDL macOS cursor implementation](https://github.com/libsdl-org/SDL/blob/main/src/video/cocoa/SDL_cocoamouse.m) - SDL's macOS implementation uses native Objective-C code (`[NSCursor arrowCursor]`, `[NSCursor pointingHandCursor]`, etc.) compiled as part of the library. System PDF-based cursors loaded from `/System/Library/Frameworks/ApplicationServices.framework/`.
- **Notes**: While you can technically P/Invoke `objc_msgSend` from C# to call NSCursor methods, it's fragile and requires careful selector management. The practical approach is a small Objective-C native plugin (`.bundle` on macOS) that wraps NSCursor calls into simple C functions. NativeCursor takes this approach. The plugin is tiny -- each function is essentially one line (`[[NSCursor pointingHandCursor] set]`).

### Finding 16: DIY P/Invoke approach for Windows is simpler but requires window procedure hooking
- **Confidence**: MEDIUM
- **Supporting sources**:
  - [NativeCursor GitHub: WindowsCursorPatch.cs](https://github.com/BlenMiner/NativeCursor) - Windows implementation is pure C# P/Invoke. Key imports: `user32.dll` functions `SetCursor`, `LoadCursor`, `GetActiveWindow`. Uses `SetWindowLongPtr` / `CallWindowProc` pattern to subclass the window and intercept `WM_SETCURSOR`.
  - [Microsoft Learn: SetCursor](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setcursor), [LoadCursor](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-loadcursora) - Standard Win32 API for cursor management.
- **Notes**: Unlike macOS, the Windows implementation doesn't need a native plugin -- it's all achievable with P/Invoke from C#. The tricky part is the window procedure subclassing to prevent Unity from resetting the cursor on `WM_SETCURSOR` messages. This is well-established Win32 technique.

---

## Contradictions

### Cursor size limit: 32x32 or larger?
- **Context**: Multiple sources mention 32x32 as the hardware cursor limit, but the actual behavior varies by platform.
- On macOS: [Unity Issue Tracker](https://issuetracker.unity3d.com/issues/macos-cursor-size-is-not-affected-by-cursor-dot-setcursor) - Cursors display at their native texture size. A 64x64 texture produces a 64x64 cursor (which is 2x too large on non-Retina, or correct on Retina but Unity doesn't handle the DPI). Status: "By Design."
- On Windows: [Unity Issue Tracker](https://issuetracker.unity3d.com/issues/windows-max-mouse-cursor-size-is-limited-when-using-cursor-dot-setcursor-and-cannot-be-increased-past-a-certain-threshold) - There is a max size limit, but it varies and is larger than 32x32 on modern Windows.
- The 32x32 "limit" is more of a recommendation for cross-platform compatibility than a hard technical limit.

### NativeCursor: Uses native plugin vs. pure P/Invoke?
- **Context**: The macOS implementation uses a compiled Objective-C plugin, while the documentation says "relies on P/Invoke to call the native APIs of each platform."
- The documentation is slightly misleading: on macOS it P/Invokes into a bundled native plugin (which then calls NSCursor), not directly into macOS frameworks. On Windows, it does use direct P/Invoke to user32.dll. On WebGL, it uses JavaScript interop.

---

## Source Registry

| # | Title | URL | Date | Queries that surfaced it |
|---|-------|-----|------|--------------------------|
| 1 | Unity Scripting API: Cursor.SetCursor | https://docs.unity3d.com/ScriptReference/Cursor.SetCursor.html | Evergreen | Q1, Q5, Q6 |
| 2 | Unity Scripting API: CursorMode | https://docs.unity3d.com/ScriptReference/CursorMode.html | Evergreen | Q6 |
| 3 | Unity Scripting API: Cursor (6.2) | https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Cursor.html | 2026 | Q3 |
| 4 | Unity Scripting API: UIElements.Cursor | https://docs.unity3d.com/ScriptReference/UIElements.Cursor.html | Evergreen | Q4, Q7 |
| 5 | USS Supported Properties (cursor) | https://docs.unity3d.com/Manual/UIE-USS-SupportedProperties.html | Evergreen | Q4, Q7 |
| 6 | Unity Scripting API: MouseCursor enum | https://docs.unity3d.com/2019.4/Documentation/ScriptReference/MouseCursor.html | Evergreen | Q7 |
| 7 | Unity Issue Tracker: macOS cursor size | https://issuetracker.unity3d.com/issues/macos-cursor-size-is-not-affected-by-cursor-dot-setcursor | ~2020 | Q1 |
| 8 | Unity Issue Tracker: Windows max cursor size | https://issuetracker.unity3d.com/issues/windows-max-mouse-cursor-size-is-limited-when-using-cursor-dot-setcursor-and-cannot-be-increased-past-a-certain-threshold | Unknown | Q1 |
| 9 | Unity Discussions: New Input System + ForceSoftware | https://discussions.unity.com/t/new-input-system-and-cursormode-forcesoftware-dont-work-together/931024 | Unknown | Q6 |
| 10 | Unity Discussions: Cursor scale on macOS Retina | https://discussions.unity.com/t/cursor-scale-on-macos-retina/759311 | Unknown | Q1 |
| 11 | NativeCursor - Unity Asset Store | https://assetstore.unity.com/packages/tools/utilities/native-cursor-220347 | Dec 2024 (v1.1.3) | Q2, Q5 |
| 12 | NativeCursor - GitHub (BlenMiner) | https://github.com/BlenMiner/NativeCursor | Ongoing | Q2, Q5 |
| 13 | NativeCursor Documentation | https://gameobject.xyz/nativecursor/ | Unknown | Q5 |
| 14 | macOS Headers: NSCursor.h | https://github.com/w0lfschild/macOS_headers/blob/master/macOS/Frameworks/AppKit/1559/NSCursor.h | Historical | Q8 |
| 15 | SDL macOS cursor implementation | https://github.com/libsdl-org/SDL/blob/main/src/video/cocoa/SDL_cocoamouse.m | Ongoing | Q8 |
| 16 | Apple Developer Docs: NSCursor | https://developer.apple.com/documentation/appkit/nscursor | Evergreen | Q8 |
| 17 | Microsoft Learn: SetCursor | https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setcursor | Evergreen | Q10 |
| 18 | Microsoft Learn: LoadCursorA | https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-loadcursora | Evergreen | Q10 |
| 19 | Unity Discussions: Allow native cursors at runtime | https://discussions.unity.com/t/allow-native-cursors-to-be-used-at-runtime-too/1536012 | 2024 | Q2 |
| 20 | Unity Discussions: Setting hardware cursor to native OS pointers | https://discussions.unity.com/t/setting-hardware-cursor-to-native-os-pointers/1518418 | 2024 | Q2 |
| 21 | Unity.NativeCursors GitHub (starburst997) | https://github.com/starburst997/Unity.NativeCursors | WIP/Incomplete | Q2 |
| 22 | Unity Scripting API: Cursor.SetCursor (6.1) | https://docs.unity3d.com/6000.1/Documentation/ScriptReference/Cursor.SetCursor.html | Jan 2026 | Q3 |
| 23 | Unity Discussions: ForceSoftware not working in new InputSystem | https://discussions.unity.com/t/cursor-setcursor-cursormode-forcesoftware-do-not-working-in-new-inputsystem/247487 | Unknown | Q11 |
| 24 | Unity Discussions: Cursor.SetCursor() heavy performance cost | https://discussions.unity.com/t/cursor-setcursor-heavy-performance-cost/706608 | Unknown | Q12 |
| 25 | Unity Issue Tracker: Performance hit on first SetCursor | https://issuetracker.unity3d.com/issues/high-performance-when-an-icon-is-set-as-a-cursor-for-the-first-time-with-setcursor-on-windows | Unknown | Q12 |
| 26 | Unity 6.3 Release Notes | https://docs.unity3d.com/6000.3/Documentation/Manual/WhatsNewUnity63.html | 2026 | Q3 |
