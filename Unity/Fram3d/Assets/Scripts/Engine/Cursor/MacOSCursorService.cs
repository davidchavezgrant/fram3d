#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fram3d.Engine.Cursor
{
    /// <summary>
    /// Runtime macOS cursor service for standalone builds.
    /// Keeps the native cursor under AppKit control through the native
    /// CursorWrapper plugin, which installs the standalone tracking-area
    /// and cursor-rect integration for the player window.
    /// </summary>
    public class MacOSCursorService : MonoBehaviour, ICursorService
    {
        private CursorType? _activeCursor;

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
            if (cursor == CursorType.Default || cursor == CursorType.Arrow)
            {
                this.ResetCursor();
                return true;
            }

            this._activeCursor = cursor;

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
                    this.ResetCursor();
                    return true;
                case CursorType.OpenHand:
                    SetCursorToOpenHand();
                    return true;
                case CursorType.ClosedHand:
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

        private void Update()
        {
            if (this._activeCursor.HasValue)
            {
                RefreshActiveCursor();
            }
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
