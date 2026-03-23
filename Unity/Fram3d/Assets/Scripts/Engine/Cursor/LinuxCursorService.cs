#if !UNITY_EDITOR && UNITY_STANDALONE_LINUX
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fram3d.Engine.Cursor
{
    internal class LinuxCursorService : MonoBehaviour, ICursorService
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            var go = new GameObject("CursorManager#Linux")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            DontDestroyOnLoad(go);

            var service = go.AddComponent<LinuxCursorService>();
            CursorManager.SetFallbackService(service);
            CursorManager.SetService(service);
        }
        
        [DllImport("libX11")]
        static extern IntPtr XOpenDisplay(string display);

        [DllImport("libX11")]
        static extern IntPtr XRootWindow(IntPtr display, int screen);
        
        [DllImport("libX11")]
        static extern IntPtr XCreateFontCursor(IntPtr display, uint shape);
        
        [DllImport("libX11")]
        static extern int XDefineCursor(IntPtr display, IntPtr window, IntPtr cursor);

        [DllImport("libX11")]
        static extern int XFlush(IntPtr display);
        
        private const uint XC_arrow = 2;                    // Arrow
        private const uint XC_xterm = 152;                  // IBeam
        private const uint XC_crosshair = 34;               // Crosshair
        private const uint XC_hand1 = 58;                   // Link
        private const uint XC_watch = 150;                  // Busy
        private const uint XC_X_cursor = 0;                 // Invalid
        private const uint XC_sb_v_double_arrow = 116;      // ResizeVertical
        private const uint XC_sb_h_double_arrow = 108;      // ResizeHorizontal
        private const uint XC_bottom_left_corner = 12;      // ResizeDiagonalLeft
        private const uint XC_bottom_right_corner = 14;     // ResizeDiagonalRight
        private const uint XC_fleur = 52;                   // ResizeAll
        private const uint XC_hand2 = 60;                   // DragDrop

        readonly Dictionary<CursorType, IntPtr> _cursors = new ();

        private IntPtr _display;
        private IntPtr _window;
        
        IntPtr Load(uint cursor)
        {
            return XCreateFontCursor(_display, cursor);
        }
        
        IntPtr LoadCursor(CursorType nativeCursor)
        {
            if (_cursors.TryGetValue(nativeCursor, out var cursor))
                return cursor;
            
            cursor = nativeCursor switch
            {
                CursorType.Default => Load(XC_arrow),
                CursorType.Arrow => Load(XC_arrow),
                CursorType.IBeam => Load(XC_xterm),
                CursorType.Crosshair => Load(XC_crosshair),
                CursorType.Link => Load(XC_hand2),
                CursorType.Busy => Load(XC_watch),
                CursorType.Invalid => Load(XC_X_cursor),
                CursorType.ResizeVertical => Load(XC_sb_v_double_arrow),
                CursorType.ResizeHorizontal => Load(XC_sb_h_double_arrow),
                CursorType.ResizeDiagonalLeft => Load(XC_bottom_left_corner),
                CursorType.ResizeDiagonalRight => Load(XC_bottom_right_corner),
                CursorType.ResizeAll => Load(XC_fleur),
                CursorType.OpenHand => Load(XC_hand1),
                CursorType.ClosedHand => Load(XC_hand1),
                _ => throw new ArgumentOutOfRangeException(nameof(cursor), cursor, null)
            };
            
            _cursors.Add(nativeCursor, cursor);
            return cursor;
        }

        private void Awake()
        {
            _display = XOpenDisplay(null);
            
            if (_display == IntPtr.Zero)
            {
                Debug.LogError("Failed to open display");
                return;
            }
            
            _window = XRootWindow(_display, 0);
            
            if (_window == IntPtr.Zero)
            {
                Debug.LogError("Failed to get root window");
                return;
            }
            
            Debug.Log($"Display: {_display}; RootWindow: {_window}");
        }

        public bool SetCursor(CursorType nativeCursorName)
        {
            var cursor = LoadCursor(nativeCursorName);

            if (cursor == IntPtr.Zero)
            {
                XFlush(_display);
                return false;
            }

            XDefineCursor(_display, _window, cursor);
            XFlush(_display);
            return true;
        }

        public void ResetCursor()
        {
            SetCursor(CursorType.Default);
        }
    }
}
#endif