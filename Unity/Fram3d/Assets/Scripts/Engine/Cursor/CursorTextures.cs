using UnityEngine;

namespace Fram3d.Engine.Cursor
{
    /// <summary>
    /// Loads cursor textures from Resources/Cursors. Textures must be
    /// imported as Cursor type, readable, RGBA32, no mipmaps, with
    /// alphaIsTransparency enabled.
    /// </summary>
    public static class CursorTextures
    {
        private static readonly Vector2 CLOSED_HAND_HOTSPOT       = new(19, 4);
        private static readonly Vector2 IBEAM_HOTSPOT             = new(12, 12);
        private static readonly Vector2 POINTING_HAND_HOTSPOT     = new(19, 4);
        private static readonly Vector2 RESIZE_HORIZONTAL_HOTSPOT = new(12, 12);

        private static Texture2D _closedHand;
        private static Texture2D _ibeam;
        private static Texture2D _pointingHand;
        private static Texture2D _resizeHorizontal;

        public static Texture2D ClosedHand
        {
            get
            {
                if (_closedHand == null)
                {
                    _closedHand = Resources.Load<Texture2D>("Cursors/closedHand");

                    if (_closedHand == null)
                    {
                        Debug.LogWarning("[Cursor] Cursors/closedHand texture not found in Resources.");
                    }
                }

                return _closedHand;
            }
        }

        public static Texture2D PointingHand
        {
            get
            {
                if (_pointingHand == null)
                {
                    _pointingHand = Resources.Load<Texture2D>("Cursors/pointer");

                    if (_pointingHand == null)
                    {
                        Debug.LogWarning("[Cursor] Cursors/pointer texture not found in Resources.");
                    }
                }

                return _pointingHand;
            }
        }

        public static Texture2D IBeam
        {
            get
            {
                if (_ibeam == null)
                {
                    _ibeam = Resources.Load<Texture2D>("Cursors/ibeam");
                }

                return _ibeam;
            }
        }

        public static Texture2D ResizeHorizontal
        {
            get
            {
                if (_resizeHorizontal == null)
                {
                    _resizeHorizontal = Resources.Load<Texture2D>("Cursors/moveleftorright");
                }

                return _resizeHorizontal;
            }
        }

        public static Vector2 ClosedHandHotspot       => CLOSED_HAND_HOTSPOT;
        public static Vector2 IBeamHotspot             => IBEAM_HOTSPOT;
        public static Vector2 PointingHandHotspot      => POINTING_HAND_HOTSPOT;
        public static Vector2 ResizeHorizontalHotspot  => RESIZE_HORIZONTAL_HOTSPOT;
    }
}
