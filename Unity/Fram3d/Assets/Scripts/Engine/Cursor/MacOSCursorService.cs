#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fram3d.Engine.Cursor
{
    /// <summary>
    /// Runtime macOS cursor service. Extracts native cursor images at startup
    /// into Texture2D objects, then uses Unity's Cursor.SetCursor API for
    /// flicker-free cursor changes. No per-frame native calls, no NSView
    /// overlay, no cursor rect invalidation — Unity manages the hardware
    /// cursor entirely.
    /// </summary>
    public class MacOSCursorService : MonoBehaviour, ICursorService
    {
        private readonly Dictionary<CursorType, CursorData> _cursors = new();

        [DllImport("CursorWrapper")]
        private static extern int Fram3dExtractCursorImage(
            int kind, out int width, out int height,
            out float hotspotX, out float hotspotY,
            out IntPtr pixels);

        [DllImport("CursorWrapper")]
        private static extern void Fram3dFreeCursorPixels(IntPtr pixels);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            var go = new GameObject("CursorManager#MacOS")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            DontDestroyOnLoad(go);

            var service = go.AddComponent<MacOSCursorService>();
            service.ExtractAllCursors();
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

            if (!this._cursors.TryGetValue(cursor, out var data))
            {
                return false;
            }

            UnityEngine.Cursor.SetCursor(data.Texture, data.Hotspot, CursorMode.Auto);
            return true;
        }

        public void ResetCursor()
        {
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void ExtractAllCursors()
        {
            this.ExtractCursor(CursorType.Link,              10); // PointingHand
            this.ExtractCursor(CursorType.IBeam,              1); // IBeam
            this.ExtractCursor(CursorType.Crosshair,          2); // Crosshair
            this.ExtractCursor(CursorType.OpenHand,           3); // OpenHand
            this.ExtractCursor(CursorType.ClosedHand,         4); // ClosedHand
            this.ExtractCursor(CursorType.ResizeHorizontal,   5); // ResizeLeftRight
            this.ExtractCursor(CursorType.ResizeVertical,     8); // ResizeUpDown
            this.ExtractCursor(CursorType.Invalid,            9); // OperationNotAllowed
        }

        private void ExtractCursor(CursorType type, int nativeKind)
        {
            var result = Fram3dExtractCursorImage(nativeKind,
                                                   out var width, out var height,
                                                   out var hotspotX, out var hotspotY,
                                                   out var pixelPtr);

            if (result == 0 || pixelPtr == IntPtr.Zero)
            {
                Debug.LogWarning($"[Cursor] Failed to extract native cursor for {type}");
                return;
            }

            var dataSize = width * height * 4;
            var rawData  = new byte[dataSize];
            Marshal.Copy(pixelPtr, rawData, 0, dataSize);
            Fram3dFreeCursorPixels(pixelPtr);

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode   = TextureWrapMode.Clamp
            };

            texture.LoadRawTextureData(rawData);
            texture.Apply(false, true);

            this._cursors[type] = new CursorData
            {
                Texture = texture,
                Hotspot = new Vector2(hotspotX, hotspotY)
            };
        }

        private void OnDisable()
        {
            this.ResetCursor();
        }

        private struct CursorData
        {
            public Vector2   Hotspot;
            public Texture2D Texture;
        }
    }
}
#endif
