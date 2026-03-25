namespace Fram3d.Core.Common
{
    /// <summary>
    /// How many views are visible and how they are arranged. Three layout
    /// options: single view, side-by-side (two), and one-large-two-small
    /// (three). Sealed class with private constructor — the set is closed.
    /// </summary>
    public sealed class ViewLayout
    {
        public static readonly ViewLayout SINGLE       = new("Single",       1);
        public static readonly ViewLayout SIDE_BY_SIDE = new("Side by Side", 2);
        public static readonly ViewLayout ONE_PLUS_TWO = new("One + Two",    3);

        private ViewLayout(string name, int viewCount)
        {
            this.Name      = name;
            this.ViewCount = viewCount;
        }

        public string Name      { get; }
        public int    ViewCount { get; }

        public override string ToString() => this.Name;
    }
}
