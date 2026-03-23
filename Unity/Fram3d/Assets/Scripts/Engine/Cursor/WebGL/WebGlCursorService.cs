#if !UNITY_EDITOR && UNITY_WEBGL
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fram3d.Engine.Cursor
{
    public class WebGlCursorService : ICursorService
    {
        [DllImport("__Internal")]
        private static extern void SetCursorStyle(string cursor);
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            CursorManager.SetService(new WebGlCursorService());
        }
        
        public bool SetCursor(CursorType cursor)
        {
            string cursorName = cursor switch
            {
                CursorType.Default => "default",
                CursorType.Arrow => "default",
                CursorType.IBeam => "text",
                CursorType.Crosshair => "crosshair",
                CursorType.Link => "pointer",
                CursorType.Busy => "wait",
                CursorType.Invalid => "not-allowed",
                CursorType.ResizeVertical => "ns-resize",
                CursorType.ResizeHorizontal => "ew-resize",
                CursorType.ResizeDiagonalLeft => "nwse-resize",
                CursorType.ResizeDiagonalRight => "nesw-resize",
                CursorType.ResizeAll => "move",
                CursorType.OpenHand => "grab",
                CursorType.ClosedHand => "grabbing",
                _ => "default"
            };
            
            SetCursorStyle(cursorName);
            return true;
        }

        public void ResetCursor()
        {
            SetCursorStyle("default");
        }
    }
}
#endif