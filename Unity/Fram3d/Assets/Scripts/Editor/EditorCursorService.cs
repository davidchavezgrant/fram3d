using System;
using System.Runtime.InteropServices;
using Fram3d.Engine.Cursor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.Editor
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
        private CursorType? _activeCursor;
        private bool       _callbackRegistered;
    #if UNITY_EDITOR_OSX
        private IntPtr _disabledWindow;

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
        private static extern IntPtr objc_getClass(string className);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
        private static extern IntPtr sel_registerName(string selectorName);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        private static readonly IntPtr NSCursorClass                = objc_getClass("NSCursor");
        private static readonly IntPtr NSApplicationClass           = objc_getClass("NSApplication");
        private static readonly IntPtr SetSel                       = sel_registerName("set");
        private static readonly IntPtr SharedApplicationSel         = sel_registerName("sharedApplication");
        private static readonly IntPtr KeyWindowSel                 = sel_registerName("keyWindow");
        private static readonly IntPtr DisableCursorRectsSel        = sel_registerName("disableCursorRects");
        private static readonly IntPtr EnableCursorRectsSel         = sel_registerName("enableCursorRects");
        private static readonly IntPtr ArrowCursorSel               = sel_registerName("arrowCursor");
        private static readonly IntPtr PointingHandCursorSel        = sel_registerName("pointingHandCursor");
        private static readonly IntPtr IBeamCursorSel               = sel_registerName("IBeamCursor");
        private static readonly IntPtr CrosshairCursorSel           = sel_registerName("crosshairCursor");
        private static readonly IntPtr OpenHandCursorSel            = sel_registerName("openHandCursor");
        private static readonly IntPtr ClosedHandCursorSel          = sel_registerName("closedHandCursor");
        private static readonly IntPtr ResizeLeftRightCursorSel     = sel_registerName("resizeLeftRightCursor");
        private static readonly IntPtr ResizeUpDownCursorSel        = sel_registerName("resizeUpDownCursor");
        private static readonly IntPtr OperationNotAllowedCursorSel = sel_registerName("operationNotAllowedCursor");

        private static void SetNSCursor(IntPtr cursorSelector)
        {
            var cursor = objc_msgSend(NSCursorClass, cursorSelector);
            objc_msgSend(cursor, SetSel);
        }

        private static void ApplyNSCursor(CursorType ntCursor)
        {
            switch (ntCursor)
            {
                case CursorType.Default:
                case CursorType.Arrow: SetNSCursor(ArrowCursorSel); break;

                case CursorType.IBeam:            SetNSCursor(IBeamCursorSel); break;
                case CursorType.Crosshair:        SetNSCursor(CrosshairCursorSel); break;
                case CursorType.Link:             SetNSCursor(PointingHandCursorSel); break;
                case CursorType.OpenHand:         SetNSCursor(OpenHandCursorSel); break;
                case CursorType.ClosedHand:       SetNSCursor(ClosedHandCursorSel); break;
                case CursorType.ResizeVertical:   SetNSCursor(ResizeUpDownCursorSel); break;
                case CursorType.ResizeHorizontal: SetNSCursor(ResizeLeftRightCursorSel); break;
                case CursorType.Invalid:          SetNSCursor(OperationNotAllowedCursorSel); break;
                case CursorType.ResizeAll:        SetNSCursor(ArrowCursorSel); break;
            }
        }

        /// <summary>
        /// Unconditionally disables cursor rects on the key window.
        /// Called every frame while a custom cursor is active, because Unity
        /// may re-enable cursor rects during its IMGUI repaint. Idempotent
        /// at the AppKit level (just sets a flag on NSWindow).
        /// </summary>
        private void EnsureCursorRectsDisabled()
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

        private void EnableWindowCursorRects()
        {
            if (_disabledWindow == IntPtr.Zero)
                return;

            objc_msgSend(_disabledWindow, EnableCursorRectsSel);
            _disabledWindow = IntPtr.Zero;
        }
    #endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Setup()
        {
            Debug.Log("[Cursor] EditorCursorService.Setup()");
            var service = new EditorCursorService();
            CursorManager.SetFallbackService(service);
            CursorManager.SetService(service);
            EditorApplication.update               += service.OnEditorUpdate;
            EditorApplication.playModeStateChanged += service.OnPlayModeStateChanged;
        }

        private void OnEditorUpdate()
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

        private void TryRegisterGameViewCallbacks()
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
            Debug.Log($"[Cursor] Registered PointerMoveEvent on {gameViews.Length} GameView(s)");
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
        #if UNITY_EDITOR_OSX
            if (!_activeCursor.HasValue)
                return;

            EnsureCursorRectsDisabled();
            ApplyNSCursor(_activeCursor.Value);
        #endif
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
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

        public bool SetCursor(CursorType ntCursorName)
        {
            if (ntCursorName == CursorType.Default)
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