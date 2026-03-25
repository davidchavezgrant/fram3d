namespace Fram3d.Core.Common
{
    /// <summary>
    /// How many views are visible and how they are arranged. Single view,
    /// horizontal split (Camera + Director side by side), or vertical split
    /// (Camera on top, Director below). Sealed class with private
    /// constructor — the set is closed.
    /// </summary>
    public sealed class ViewLayout
    {
        public static readonly ViewLayout SINGLE     = new("Single",     1);
        public static readonly ViewLayout HORIZONTAL = new("Horizontal", 2);
        public static readonly ViewLayout VERTICAL   = new("Vertical",   2);

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
