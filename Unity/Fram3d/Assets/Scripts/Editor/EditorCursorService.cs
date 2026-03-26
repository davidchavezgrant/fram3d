using System;
using System.Runtime.InteropServices;
using Fram3d.Engine.Cursor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.Editor
{
    /// <summary>
    /// Editor cursor service for macOS Play Mode.
    /// Uses the same native CursorWrapper overlay as standalone builds so the
    /// cursor participates in AppKit cursor rect evaluation instead of fighting
    /// Unity's editor repaint loop from managed code.
    /// </summary>
    public class EditorCursorService: ICursorService
    {
        private CursorType? _activeCursor;
        private bool        _callbackRegistered;

    #if UNITY_EDITOR_OSX
        private bool   _useManagedFallback;
        private IntPtr _disabledWindow;

        private static bool sLoggedNativePluginFailure;

        [DllImport("CursorWrapper")]
        private static extern void RefreshActiveCursor();

        [DllImport("CursorWrapper")]
        private static extern void Fram3dReapplyCursor();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToArrow();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToIBeam();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToCrosshair();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToOpenHand();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToClosedHand();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToResizeLeftRight();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToResizeUp();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToResizeDown();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToResizeUpDown();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToOperationNotAllowed();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToPointingHand();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToBusy();

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

        public void ResetCursor()
        {
            _activeCursor = null;
        #if UNITY_EDITOR_OSX
            if (!TryResetNativeCursor())
            {
                EnableWindowCursorRects();
                SetManagedCursor(ArrowCursorSel);
            }
        #endif
        }

        public bool SetCursor(CursorType cursor)
        {
            if (cursor == CursorType.Default || cursor == CursorType.Arrow)
            {
                ResetCursor();
                return true;
            }

            _activeCursor = cursor;
        #if UNITY_EDITOR_OSX
            if (TryApplyNativeCursor(cursor))
                return true;

            EnsureCursorRectsDisabled();
            ApplyManagedCursor(cursor);
            return true;
        #else
            return true;
        #endif
        }

        private void OnEditorUpdate()
        {
        #if UNITY_EDITOR_OSX
            if (_useManagedFallback && !_callbackRegistered)
                TryRegisterGameViewCallbacks();

            // Re-apply [cursor set] every frame to counteract Unity's Editor
            // repaint resetting the cursor. This does NOT invalidate cursor
            // rects — that's the key difference from the old code that flickered.
            if (_activeCursor.HasValue)
            {
                if (!TryReapplyNativeCursor())
                {
                    EnsureCursorRectsDisabled();
                    ApplyManagedCursor(_activeCursor.Value);
                }
            }
        #endif
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
                return;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update               -= OnEditorUpdate;
            ResetCursor();
        }

    #if UNITY_EDITOR_OSX
        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_useManagedFallback || !_activeCursor.HasValue)
                return;

            EnsureCursorRectsDisabled();
            ApplyManagedCursor(_activeCursor.Value);
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
        }

        private bool TryReapplyNativeCursor()
        {
            if (_useManagedFallback)
                return false;

            try
            {
                Fram3dReapplyCursor();
                return true;
            }
            catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException)
            {
                ActivateManagedFallback(ex);
                return false;
            }
        }

        private bool TryRefreshNativeCursor()
        {
            if (_useManagedFallback)
                return false;

            try
            {
                RefreshActiveCursor();
                return true;
            }
            catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException)
            {
                ActivateManagedFallback(ex);
                return false;
            }
        }

        private bool TryResetNativeCursor()
        {
            if (_useManagedFallback)
                return false;

            try
            {
                SetCursorToArrow();
                return true;
            }
            catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException)
            {
                ActivateManagedFallback(ex);
                return false;
            }
        }

        private bool TryApplyNativeCursor(CursorType cursor)
        {
            if (_useManagedFallback)
                return false;

            try
            {
                return ApplyNativeCursor(cursor);
            }
            catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException)
            {
                ActivateManagedFallback(ex);
                return false;
            }
        }

        private bool ApplyNativeCursor(CursorType cursor)
        {
            switch (cursor)
            {
                case CursorType.IBeam:
                    SetCursorToIBeam();
                    return true;
                case CursorType.Crosshair:
                    SetCursorToCrosshair();
                    return true;
                case CursorType.Link:
                    SetCursorToPointingHand();
                    return true;
                case CursorType.Busy:
                    SetCursorToBusy();
                    return true;
                case CursorType.Invalid:
                    SetCursorToOperationNotAllowed();
                    return true;
                case CursorType.ResizeVertical:
                    SetCursorToResizeUpDown();
                    return true;
                case CursorType.ResizeHorizontal:
                    SetCursorToResizeLeftRight();
                    return true;
                case CursorType.ResizeDiagonalLeft:
                    SetCursorToResizeUp();
                    return true;
                case CursorType.ResizeDiagonalRight:
                    SetCursorToResizeDown();
                    return true;
                case CursorType.ResizeAll:
                    ResetCursor();
                    return true;
                case CursorType.OpenHand:
                    SetCursorToOpenHand();
                    return true;
                case CursorType.ClosedHand:
                    SetCursorToClosedHand();
                    return true;
                default:
                    return false;
            }
        }

        private void ActivateManagedFallback(Exception ex)
        {
            _useManagedFallback = true;

            if (sLoggedNativePluginFailure)
                return;

            sLoggedNativePluginFailure = true;
            Debug.LogWarning($"[Cursor] CursorWrapper native plugin is unavailable in this Editor session ({ex.GetType().Name}). Falling back to the managed macOS cursor path. Restart Unity after reimporting native plugin changes to enable the AppKit overlay fix.");
        }

        private static void SetManagedCursor(IntPtr cursorSelector)
        {
            var cursor = objc_msgSend(NSCursorClass, cursorSelector);
            objc_msgSend(cursor, SetSel);
        }

        private static void ApplyManagedCursor(CursorType cursor)
        {
            switch (cursor)
            {
                case CursorType.Default:
                case CursorType.Arrow:
                    SetManagedCursor(ArrowCursorSel);
                    break;
                case CursorType.IBeam:
                    SetManagedCursor(IBeamCursorSel);
                    break;
                case CursorType.Crosshair:
                    SetManagedCursor(CrosshairCursorSel);
                    break;
                case CursorType.Link:
                    SetManagedCursor(PointingHandCursorSel);
                    break;
                case CursorType.OpenHand:
                    SetManagedCursor(OpenHandCursorSel);
                    break;
                case CursorType.ClosedHand:
                    SetManagedCursor(ClosedHandCursorSel);
                    break;
                case CursorType.ResizeVertical:
                    SetManagedCursor(ResizeUpDownCursorSel);
                    break;
                case CursorType.ResizeHorizontal:
                    SetManagedCursor(ResizeLeftRightCursorSel);
                    break;
                case CursorType.Invalid:
                    SetManagedCursor(OperationNotAllowedCursorSel);
                    break;
                case CursorType.ResizeAll:
                    SetManagedCursor(ArrowCursorSel);
                    break;
            }
        }

        private void EnsureCursorRectsDisabled()
        {
            var app    = objc_msgSend(NSApplicationClass, SharedApplicationSel);
            var window = objc_msgSend(app,                KeyWindowSel);

            if (window == IntPtr.Zero)
                return;

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
    }
}
