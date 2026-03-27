namespace Fram3d.Core.Scenes
{
    /// <summary>
    /// Which camera the user is looking through. Camera View is the shot
    /// camera; Director View is a free utility camera decoupled from the
    /// timeline. Sealed class with private constructor — the set is closed.
    /// </summary>
    public sealed class ViewMode
    {
        public static readonly ViewMode CAMERA   = new("Camera View");
        public static readonly ViewMode DIRECTOR = new("Director View");

        private ViewMode(string name) => this.Name = name;

        public string Name { get; }
        public override string ToString() => this.Name;
    }
}
