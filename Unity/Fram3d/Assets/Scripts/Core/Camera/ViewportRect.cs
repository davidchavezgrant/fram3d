namespace Fram3d.Core.Camera
{
    /// <summary>
    /// A normalized (0–1) viewport rectangle constraining the camera render area
    /// to match the sensor's aspect ratio. Prevents Unity from rendering scene
    /// content beyond the sensor gate.
    /// </summary>
    public readonly struct ViewportRect
    {
        private const float EPSILON = 0.001f;

        public ViewportRect(float x, float y, float width, float height)
        {
            this.X      = x;
            this.Y      = y;
            this.Width  = width;
            this.Height = height;
        }

        public float Height { get; }
        public float Width  { get; }
        public float X      { get; }
        public float Y      { get; }

        /// <summary>
        /// Computes the viewport rect that constrains the render area to the sensor's
        /// aspect ratio within the screen. When the sensor is wider than the screen,
        /// the viewport is letterboxed (horizontal bars). When narrower, pillarboxed
        /// (vertical bars). When matching, the full viewport is used.
        /// </summary>
        public static ViewportRect Compute(float sensorAspect, float screenAspect)
        {
            if (screenAspect <= 0f || sensorAspect <= 0f)
                return new ViewportRect(0f, 0f, 1f, 1f);

            if (sensorAspect > screenAspect + EPSILON)
            {
                var height = screenAspect / sensorAspect;
                return new ViewportRect(0f, (1f - height) / 2f, 1f, height);
            }

            if (sensorAspect < screenAspect - EPSILON)
            {
                var width = sensorAspect / screenAspect;
                return new ViewportRect((1f - width) / 2f, 0f, width, 1f);
            }

            return new ViewportRect(0f, 0f, 1f, 1f);
        }
    }
}
