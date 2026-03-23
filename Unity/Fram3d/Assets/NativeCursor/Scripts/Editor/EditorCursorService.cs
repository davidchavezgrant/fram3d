using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Riten.Native.Cursors.Editor
{
    public class EditorCursorService : ICursorService
    {
        NTCursors? _activeCursor;

#if UNITY_EDITOR_OSX
        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
        static extern IntPtr objc_getClass(string className);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
        static extern IntPtr sel_registerName(string selectorName);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        static readonly IntPtr NSCursorClass              = objc_getClass("NSCursor");
        static readonly IntPtr SetSel                     = sel_registerName("set");
        static readonly IntPtr ArrowCursorSel             = sel_registerName("arrowCursor");
        static readonly IntPtr PointingHandCursorSel      = sel_registerName("pointingHandCursor");
        static readonly IntPtr IBeamCursorSel             = sel_registerName("IBeamCursor");
        static readonly IntPtr CrosshairCursorSel         = sel_registerName("crosshairCursor");
        static readonly IntPtr OpenHandCursorSel          = sel_registerName("openHandCursor");
        static readonly IntPtr ClosedHandCursorSel        = sel_registerName("closedHandCursor");
        static readonly IntPtr ResizeLeftRightCursorSel   = sel_registerName("resizeLeftRightCursor");
        static readonly IntPtr ResizeUpDownCursorSel      = sel_registerName("resizeUpDownCursor");
        static readonly IntPtr OperationNotAllowedCursorSel = sel_registerName("operationNotAllowedCursor");

        static void SetNSCursor(IntPtr cursorSelector)
        {
            var cursor = objc_msgSend(NSCursorClass, cursorSelector);
            objc_msgSend(cursor, SetSel);
        }

        static bool ApplyNSCursor(NTCursors ntCursor)
        {
            switch (ntCursor)
            {
                case NTCursors.Default:
                case NTCursors.Arrow:             SetNSCursor(ArrowCursorSel); return true;
                case NTCursors.IBeam:             SetNSCursor(IBeamCursorSel); return true;
                case NTCursors.Crosshair:         SetNSCursor(CrosshairCursorSel); return true;
                case NTCursors.Link:              SetNSCursor(PointingHandCursorSel); return true;
                case NTCursors.OpenHand:          SetNSCursor(OpenHandCursorSel); return true;
                case NTCursors.ClosedHand:        SetNSCursor(ClosedHandCursorSel); return true;
                case NTCursors.ResizeVertical:    SetNSCursor(ResizeUpDownCursorSel); return true;
                case NTCursors.ResizeHorizontal:  SetNSCursor(ResizeLeftRightCursorSel); return true;
                case NTCursors.Invalid:           SetNSCursor(OperationNotAllowedCursorSel); return true;
                case NTCursors.ResizeAll:         SetNSCursor(ArrowCursorSel); return true;
                default: return false;
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            Debug.Log("[NativeCursor] EditorCursorService.Setup()");
#if UNITY_EDITOR_OSX
            Debug.Log($"[NativeCursor] NSCursorClass={NSCursorClass}, SetSel={SetSel}, PointingHandSel={PointingHandCursorSel}");
#else
            Debug.Log("[NativeCursor] NOT macOS editor — UNITY_EDITOR_OSX not defined");
#endif

            var service = new EditorCursorService();

            NativeCursor.SetFallbackService(service);
            NativeCursor.SetService(service);

            EditorApplication.update += service.OnEditorUpdate;
            EditorApplication.playModeStateChanged += service.OnPlayModeStateChanged;
        }

        void OnEditorUpdate()
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

            Debug.Log("[NativeCursor] ExitingPlayMode — cleaning up");
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= OnEditorUpdate;
            ResetCursor();
        }

        public bool SetCursor(NTCursors ntCursorName)
        {
            Debug.Log($"[NativeCursor] SetCursor({ntCursorName})");

            if (ntCursorName == NTCursors.Default)
            {
                _activeCursor = null;
#if UNITY_EDITOR_OSX
                SetNSCursor(ArrowCursorSel);
                return true;
#else
                return false;
#endif
            }

            _activeCursor = ntCursorName;

#if UNITY_EDITOR_OSX
            return ApplyNSCursor(ntCursorName);
#else
            return false;
#endif
        }

        public void ResetCursor()
        {
            Debug.Log("[NativeCursor] ResetCursor()");
            _activeCursor = null;
#if UNITY_EDITOR_OSX
            SetNSCursor(ArrowCursorSel);
#endif
        }
    }
}
