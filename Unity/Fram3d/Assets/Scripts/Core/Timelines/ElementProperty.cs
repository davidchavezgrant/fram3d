namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// The set of element properties that can be independently recorded.
    /// Sealed class with private constructor — the set is closed.
    /// </summary>
    public sealed class ElementProperty
    {
        public static readonly ElementProperty POSITION = new(0, "Position");
        public static readonly ElementProperty ROTATION = new(1, "Rotation");
        public static readonly ElementProperty SCALE    = new(2, "Scale");

        public static readonly int COUNT = 3;

        private ElementProperty(int index, string name)
        {
            this.Index = index;
            this.Name  = name;
        }

        public int    Index { get; }
        public string Name  { get; }

        public override string ToString() => this.Name;
    }
}
