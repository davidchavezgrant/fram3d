namespace Fram3d.Core.Viewports
{
    public readonly struct UnmaskedRect
    {
        public UnmaskedRect(float x,
                            float y,
                            float width,
                            float height)
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
    }
}