#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
using System.Runtime.InteropServices;
using UnityEngine;

namespace Riten.Native.Cursors
{
    /// <summary>
    /// Runtime macOS cursor service for standalone builds.
    /// Keeps the native cursor under AppKit control through the native
    /// CursorWrapper plugin, which installs the standalone tracking-area
    /// and cursor-rect integration for the player window.
    /// </summary>
    public class MacOSCursorService : MonoBehaviour, ICursorService
    {
        private NTCursors? _activeCursor;

        [DllImport("CursorWrapper")]
        private static extern void RefreshActiveCursor();

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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            var go = new GameObject("NativeCursor#MacOSCursorService")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            DontDestroyOnLoad(go);

            var service = go.AddComponent<MacOSCursorService>();
            NativeCursor.SetFallbackService(service);
            NativeCursor.SetService(service);
        }

        public bool SetCursor(NTCursors cursor)
        {
            if (cursor == NTCursors.Default || cursor == NTCursors.Arrow)
            {
                this.ResetCursor();
                return true;
            }

            this._activeCursor = cursor;

            switch (cursor)
            {
                case NTCursors.IBeam:
                    SetCursorToIBeam();
                    return true;
                case NTCursors.Crosshair:
                    SetCursorToCrosshair();
                    return true;
                case NTCursors.Link:
                    SetCursorToPointingHand();
                    return true;
                case NTCursors.Busy:
                    SetCursorToBusy();
                    return true;
                case NTCursors.Invalid:
                    SetCursorToOperationNotAllowed();
                    return true;
                case NTCursors.ResizeVertical:
                    SetCursorToResizeUpDown();
                    return true;
                case NTCursors.ResizeHorizontal:
                    SetCursorToResizeLeftRight();
                    return true;
                case NTCursors.ResizeDiagonalLeft:
                    SetCursorToResizeUp();
                    return true;
                case NTCursors.ResizeDiagonalRight:
                    SetCursorToResizeDown();
                    return true;
                case NTCursors.ResizeAll:
                    this.ResetCursor();
                    return true;
                case NTCursors.OpenHand:
                    SetCursorToOpenHand();
                    return true;
                case NTCursors.ClosedHand:
                    SetCursorToClosedHand();
                    return true;
                default:
                    this._activeCursor = null;
                    return false;
            }
        }

        public void ResetCursor()
        {
            this._activeCursor = null;
            SetCursorToArrow();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && this._activeCursor.HasValue)
            {
                RefreshActiveCursor();
            }
        }

        private void OnDisable()
        {
            this.ResetCursor();
        }
    }
}

#endif
