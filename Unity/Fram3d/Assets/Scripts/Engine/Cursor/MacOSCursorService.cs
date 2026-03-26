#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fram3d.Engine.Cursor
{
    /// <summary>
    /// Runtime macOS cursor service for standalone builds.
    /// Uses a native NSView overlay with NSTrackingArea and cursorUpdate:
    /// so cursor changes work WITH AppKit instead of fighting it.
    ///
    /// Key design: the native side only invalidates cursor rects when the
    /// cursor kind CHANGES. No per-frame invalidation — that causes flicker.
    /// </summary>
    public class MacOSCursorService : MonoBehaviour, ICursorService
    {
        private bool _overlayInstalled;

        [DllImport("CursorWrapper")]
        private static extern void Fram3dEnsureOverlay();

        [DllImport("CursorWrapper")]
        private static extern void Fram3dSetCursor(int kind);

        // Legacy entry points still exist but route through Fram3dSetCursor
        [DllImport("CursorWrapper")]
        private static extern void SetCursorToArrow();

        [DllImport("CursorWrapper")]
        private static extern void SetCursorToPointingHand();

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
        private static extern void SetCursorToBusy();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            var go = new GameObject("CursorManager#MacOS")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            DontDestroyOnLoad(go);

            var service = go.AddComponent<MacOSCursorService>();
            CursorManager.SetFallbackService(service);
            CursorManager.SetService(service);
        }

        public bool SetCursor(CursorType cursor)
        {
            this.EnsureOverlay();

            if (cursor == CursorType.Default || cursor == CursorType.Arrow)
            {
                this.ResetCursor();
                return true;
            }

            switch (cursor)
            {
                case CursorType.IBeam:             SetCursorToIBeam();               return true;
                case CursorType.Crosshair:         SetCursorToCrosshair();           return true;
                case CursorType.Link:              SetCursorToPointingHand();        return true;
                case CursorType.Busy:              SetCursorToBusy();                return true;
                case CursorType.Invalid:           SetCursorToOperationNotAllowed(); return true;
                case CursorType.ResizeVertical:    SetCursorToResizeUpDown();        return true;
                case CursorType.ResizeHorizontal:  SetCursorToResizeLeftRight();     return true;
                case CursorType.ResizeDiagonalLeft:  SetCursorToResizeUp();          return true;
                case CursorType.ResizeDiagonalRight: SetCursorToResizeDown();        return true;
                case CursorType.OpenHand:          SetCursorToOpenHand();            return true;
                case CursorType.ClosedHand:        SetCursorToClosedHand();          return true;
                case CursorType.ResizeAll:         this.ResetCursor();               return true;
                default:                                                              return false;
            }
        }

        public void ResetCursor()
        {
            SetCursorToArrow();
        }

        private void EnsureOverlay()
        {
            if (this._overlayInstalled)
            {
                return;
            }

            Fram3dEnsureOverlay();
            this._overlayInstalled = true;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                // Overlay may need re-raising after focus change
                this._overlayInstalled = false;
            }
        }

        private void OnDisable()
        {
            this.ResetCursor();
        }
    }
}
#endif
