using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Riten.Native.Cursors.Editor
{
    /// <summary>
    /// Editor cursor service using NSCursor P/Invoke triggered from PointerMoveEvent
    /// callbacks on the Game View's rootVisualElement.
    ///
    /// Why this timing matters:
    /// macOS evaluates NSTrackingArea cursor rects on every mouse move BEFORE dispatching
    /// the event to the application. By setting NSCursor in a PointerMoveEvent callback
    /// (which fires AFTER tracking area evaluation), we override the cursor after macOS
    /// has already tried to reset it. Both happen within one event dispatch cycle, before
    /// the next display refresh — so no visible flicker.
    ///
    /// Previous approaches that failed:
    /// - EditorApplication.update + P/Invoke: fires during update loop, cursor is reset
    ///   during the subsequent repaint phase
    /// - Cursor.SetCursor(): overridden by Editor's IMGUI cursor management
    /// - Harmony on Internal_AddCursorRect: native icall, can't be patched
    /// - Harmony on GameView.OnGUI: patching exception
    /// </summary>
    public class EditorCursorService : ICursorService
    {
        NTCursors? _activeCursor;
        bool _callbackRegistered;

#if UNITY_EDITOR_OSX
        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
        static extern IntPtr objc_getClass(string className);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
        static extern IntPtr sel_registerName(string selectorName);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        static readonly IntPtr NSCursorClass                = objc_getClass("NSCursor");
        static readonly IntPtr SetSel                       = sel_registerName("set");
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
                case NTCursors.Arrow:             SetNSCursor(ArrowCursorSel); break;
                case NTCursors.IBeam:             SetNSCursor(IBeamCursorSel); break;
                case NTCursors.Crosshair:         SetNSCursor(CrosshairCursorSel); break;
                case NTCursors.Link:              SetNSCursor(PointingHandCursorSel); break;
                case NTCursors.OpenHand:          SetNSCursor(OpenHandCursorSel); break;
                case NTCursors.ClosedHand:        SetNSCursor(ClosedHandCursorSel); break;
                case NTCursors.ResizeVertical:    SetNSCursor(ResizeUpDownCursorSel); break;
                case NTCursors.ResizeHorizontal:  SetNSCursor(ResizeLeftRightCursorSel); break;
                case NTCursors.Invalid:           SetNSCursor(OperationNotAllowedCursorSel); break;
                case NTCursors.ResizeAll:         SetNSCursor(ArrowCursorSel); break;
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            Debug.Log("[NativeCursor] EditorCursorService.Setup()");

            var service = new EditorCursorService();

            NativeCursor.SetFallbackService(service);
            NativeCursor.SetService(service);

            EditorApplication.update += service.OnEditorUpdate;
            EditorApplication.playModeStateChanged += service.OnPlayModeStateChanged;
        }

        void OnEditorUpdate()
        {
            if (!_callbackRegistered)
                TryRegisterGameViewCallbacks();
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
            if (_activeCursor.HasValue)
                ApplyNSCursor(_activeCursor.Value);
#endif
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
                return;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= OnEditorUpdate;
            ResetCursor();
        }

        public bool SetCursor(NTCursors ntCursorName)
        {
            if (ntCursorName == NTCursors.Default)
            {
                _activeCursor = null;
#if UNITY_EDITOR_OSX
                SetNSCursor(ArrowCursorSel);
#endif
                return true;
            }

            _activeCursor = ntCursorName;

#if UNITY_EDITOR_OSX
            ApplyNSCursor(ntCursorName);
#endif
            return true;
        }

        public void ResetCursor()
        {
            _activeCursor = null;
#if UNITY_EDITOR_OSX
            SetNSCursor(ArrowCursorSel);
#endif
        }
    }
}
