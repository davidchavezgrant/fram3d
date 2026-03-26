using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fram3d.Engine.Cursor;
using UnityEditor;
using UnityEngine;
namespace Fram3d.Editor
{
    /// <summary>
    /// Editor cursor service for macOS Play Mode. Extracts native cursor
    /// images into Texture2D objects, then uses Unity's Cursor.SetCursor
    /// API. No NSView overlay, no cursor rect invalidation, no per-frame
    /// native calls — Unity manages the cursor entirely.
    /// </summary>
    public class EditorCursorService: ICursorService
    {
        private readonly Dictionary<CursorType, CursorData> _cursors = new();
        private          bool _extracted;

    #if UNITY_EDITOR_OSX
        [DllImport("CursorWrapper")]
        private static extern int Fram3dExtractCursorImage(
            int kind, out int width, out int height,
            out float hotspotX, out float hotspotY,
            out IntPtr pixels);

        [DllImport("CursorWrapper")]
        private static extern void Fram3dFreeCursorPixels(IntPtr pixels);
    #endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Setup()
        {
            var service = new EditorCursorService();
            CursorManager.SetFallbackService(service);
            CursorManager.SetService(service);
            EditorApplication.playModeStateChanged += service.OnPlayModeStateChanged;
        }

        public void ResetCursor()
        {
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        public bool SetCursor(CursorType cursor)
        {
            if (cursor == CursorType.Default || cursor == CursorType.Arrow)
            {
                this.ResetCursor();
                return true;
            }

            this.EnsureExtracted();

            if (!this._cursors.TryGetValue(cursor, out var data))
            {
                return false;
            }

            UnityEngine.Cursor.SetCursor(data.Texture, data.Hotspot, CursorMode.Auto);
            return true;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
            {
                return;
            }

            EditorApplication.playModeStateChanged -= this.OnPlayModeStateChanged;
            this.ResetCursor();
        }

        private void EnsureExtracted()
        {
            if (this._extracted)
            {
                return;
            }

            this._extracted = true;

        #if UNITY_EDITOR_OSX
            try
            {
                this.ExtractCursor(CursorType.Link,            10);
                this.ExtractCursor(CursorType.IBeam,            1);
                this.ExtractCursor(CursorType.Crosshair,        2);
                this.ExtractCursor(CursorType.OpenHand,         3);
                this.ExtractCursor(CursorType.ClosedHand,       4);
                this.ExtractCursor(CursorType.ResizeHorizontal, 5);
                this.ExtractCursor(CursorType.ResizeVertical,   8);
                this.ExtractCursor(CursorType.Invalid,          9);
            }
            catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException)
            {
                Debug.LogWarning($"[Cursor] Native plugin unavailable ({ex.GetType().Name}). Cursor changes disabled.");
            }
        #endif
        }

    #if UNITY_EDITOR_OSX
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
            texture.Apply(false, false);

            this._cursors[type] = new CursorData
            {
                Texture = texture,
                Hotspot = new Vector2(hotspotX, hotspotY)
            };
        }
    #endif

        private struct CursorData
        {
            public Vector2   Hotspot;
            public Texture2D Texture;
        }
    }
}
