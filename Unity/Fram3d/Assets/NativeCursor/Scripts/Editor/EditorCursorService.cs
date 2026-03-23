using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Riten.Native.Cursors.Editor
{
    /// <summary>
    /// Editor cursor service that prevents macOS cursor flickering by:
    /// 1. [NSWindow disableCursorRects] — suppresses automatic cursor rect evaluation
    /// 2. Continuous re-apply in EditorApplication.update — overpowers any resets
    ///    Unity makes internally (e.g., during IMGUI repaint or [NSCursor set] calls)
    /// 3. PointerMoveEvent callback — re-applies after each macOS mouse-moved event
    ///
    /// Why all three layers are needed:
    /// - disableCursorRects alone fails if Unity re-enables cursor rects or calls
    ///   [NSCursor set] directly during its repaint phase
    /// - PointerMoveEvent alone failed (the previous approach) because the cursor
    ///   still flickers between tracking area evaluation and our callback
    /// - EditorApplication.update alone fires before repaint, so Unity can reset
    ///   the cursor after our set
    /// - Combined, they ensure at least one re-apply fires after every possible reset
    ///
    /// During mouse drag (NSMouseDragged), macOS skips cursor rect evaluation
    /// entirely, which is why drag never flickered.
    /// Same approach as Mozilla/Firefox (Bug 445567).
    /// </summary>
    public class EditorCursorService: ICursorService
    {
        NTCursors? _activeCursor;
        bool       _callbackRegistered;
    #if UNITY_EDITOR_OSX
        IntPtr _disabledWindow;

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
        static extern IntPtr objc_getClass(string className);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
        static extern IntPtr sel_registerName(string selectorName);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        static readonly IntPtr NSCursorClass                = objc_getClass("NSCursor");
        static readonly IntPtr NSApplicationClass           = objc_getClass("NSApplication");
        static readonly IntPtr SetSel                       = sel_registerName("set");
        static readonly IntPtr SharedApplicationSel         = sel_registerName("sharedApplication");
        static readonly IntPtr KeyWindowSel                 = sel_registerName("keyWindow");
        static readonly IntPtr DisableCursorRectsSel        = sel_registerName("disableCursorRects");
        static readonly IntPtr EnableCursorRectsSel         = sel_registerName("enableCursorRects");
        static readonly IntPtr ArrowCursorSel               = sel_registerName("arrowCursor");
        static readonly IntPtr PointingHandCursorSel        = sel_registerName("pointingHandCursor");
        static readonly IntPtr IBeamCursorSel               = sel_registerName("IBeamCursor");
        static readonly IntPtr CrosshairCursorSel           = sel_registerName("crosshairCursor");
        static readonly IntPtr OpenHandCursorSel            = sel_registerName("openHandCursor");
        static readonly IntPtr ClosedHandCursorSel          = sel_registerName("closedHandCursor");
        static readonly IntPtr ResizeLeftRightCursorSel     = sel_registerName("resizeLeftRightCursor");
        static readonly IntPtr ResizeUpDownCursorSel        = sel_registerName("resizeUpDownCursor");
        static readonly IntPtr OperationNotAllowedCursorSel = sel_registerName("operationNotAllowedCursor");

        static void SetNSCursor(IntPtr cursorSelector)
        {
            var cursor = objc_msgSend(NSCursorClass, cursorSelector);
            objc_msgSend(cursor, SetSel);
        }

        static void ApplyNSCursor(NTCursors ntCursor)
        {
            switch (ntCursor)
            {
                case NTCursors.Default:
                case NTCursors.Arrow: SetNSCursor(ArrowCursorSel); break;

                case NTCursors.IBeam:            SetNSCursor(IBeamCursorSel); break;
                case NTCursors.Crosshair:        SetNSCursor(CrosshairCursorSel); break;
                case NTCursors.Link:             SetNSCursor(PointingHandCursorSel); break;
                case NTCursors.OpenHand:         SetNSCursor(OpenHandCursorSel); break;
                case NTCursors.ClosedHand:       SetNSCursor(ClosedHandCursorSel); break;
                case NTCursors.ResizeVertical:   SetNSCursor(ResizeUpDownCursorSel); break;
                case NTCursors.ResizeHorizontal: SetNSCursor(ResizeLeftRightCursorSel); break;
                case NTCursors.Invalid:          SetNSCursor(OperationNotAllowedCursorSel); break;
                case NTCursors.ResizeAll:        SetNSCursor(ArrowCursorSel); break;
            }
        }

        /// <summary>
        /// Unconditionally disables cursor rects on the key window.
        /// Called every frame while a custom cursor is active, because Unity
        /// may re-enable cursor rects during its IMGUI repaint. Idempotent
        /// at the AppKit level (just sets a flag on NSWindow).
        /// </summary>
        void EnsureCursorRectsDisabled()
        {
            var app    = objc_msgSend(NSApplicationClass, SharedApplicationSel);
            var window = objc_msgSend(app,                KeyWindowSel);

            if (window == IntPtr.Zero)
                return;

            // If the key window changed, re-enable the old one
            if (_disabledWindow != IntPtr.Zero && _disabledWindow != window)
                objc_msgSend(_disabledWindow, EnableCursorRectsSel);

            objc_msgSend(window, DisableCursorRectsSel);
            _disabledWindow = window;
        }

        void EnableWindowCursorRects()
        {
            if (_disabledWindow == IntPtr.Zero)
                return;

            objc_msgSend(_disabledWindow, EnableCursorRectsSel);
            _disabledWindow = IntPtr.Zero;
        }
    #endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            Debug.Log("[NativeCursor] EditorCursorService.Setup()");
            var service = new EditorCursorService();
            NativeCursor.SetFallbackService(service);
            NativeCursor.SetService(service);
            EditorApplication.update               += service.OnEditorUpdate;
            EditorApplication.playModeStateChanged += service.OnPlayModeStateChanged;
        }

        void OnEditorUpdate()
        {
            if (!_callbackRegistered)
                TryRegisterGameViewCallbacks();

        #if UNITY_EDITOR_OSX

            // Continuously re-disable cursor rects and re-apply cursor.
            // Unity's IMGUI repaint may re-enable cursor rects or call
            // [NSCursor set] between update callbacks.
            if (_activeCursor.HasValue)
            {
                EnsureCursorRectsDisabled();
                ApplyNSCursor(_activeCursor.Value);
            }
        #endif
        }

        void TryRegisterGameViewCallbacks()
        {
            var gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");

            if (gameViewType == null)
                return;

            var gameViews = Resources.FindObjectsOfTypeAll(gameViewType);

            if (gameViews.Length == 0)
                return;

            foreach (var gv in gameViews)
            {
                var window = (EditorWindow)gv;
                window.rootVisualElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            }

            _callbackRegistered = true;
            Debug.Log($"[NativeCursor] Registered PointerMoveEvent on {gameViews.Length} GameView(s)");
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
        #if UNITY_EDITOR_OSX
            if (!_activeCursor.HasValue)
                return;

            EnsureCursorRectsDisabled();
            ApplyNSCursor(_activeCursor.Value);
        #endif
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
                return;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update               -= OnEditorUpdate;
        #if UNITY_EDITOR_OSX
            EnableWindowCursorRects();
        #endif
            ResetCursor();
        }

        public bool SetCursor(NTCursors ntCursorName)
        {
            if (ntCursorName == NTCursors.Default)
            {
                _activeCursor = null;
            #if UNITY_EDITOR_OSX
                EnableWindowCursorRects();
                SetNSCursor(ArrowCursorSel);
            #endif
                return true;
            }

            _activeCursor = ntCursorName;
        #if UNITY_EDITOR_OSX
            EnsureCursorRectsDisabled();
            ApplyNSCursor(ntCursorName);
        #endif
            return true;
        }

        public void ResetCursor()
        {
            _activeCursor = null;
        #if UNITY_EDITOR_OSX
            EnableWindowCursorRects();
            SetNSCursor(ArrowCursorSel);
        #endif
        }
    }
}