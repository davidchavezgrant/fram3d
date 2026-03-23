namespace Fram3d.Core.Scene
{
    /// <summary>
    /// The current manipulation tool. Sealed class with private constructor —
    /// the set of valid tools is closed. Each instance carries a display name
    /// and keyboard shortcut character.
    /// </summary>
    public sealed class ActiveTool
    {
        public static readonly ActiveTool ROTATE    = new("Rotate", 'E');
        public static readonly ActiveTool SCALE     = new("Scale", 'R');
        public static readonly ActiveTool SELECT    = new("Select", 'Q');
        public static readonly ActiveTool TRANSLATE = new("Translate", 'W');

        private ActiveTool(string name, char shortcut)
        {
            this.Name     = name;
            this.Shortcut = shortcut;
        }

        public          string Name       { get; }
        public          char   Shortcut   { get; }
        public override string ToString() => this.Name;
    }
}