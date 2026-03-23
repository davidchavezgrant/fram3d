using System.Numerics;
namespace Fram3d.Core.Scene
{
    /// <summary>
    /// The axis constraint for a gizmo drag operation.
    /// Maps to world-space unit vectors via AxisDirection.
    /// </summary>
    public sealed class GizmoAxis
    {
        public static readonly GizmoAxis UNIFORM = new("Uniform", Vector3.Zero);
        public static readonly GizmoAxis X       = new("X", Vector3.UnitX);
        public static readonly GizmoAxis Y       = new("Y", Vector3.UnitY);
        public static readonly GizmoAxis Z       = new("Z", -Vector3.UnitZ);

        public string  Name      { get; }
        public Vector3 Direction { get; }

        private GizmoAxis(string name, Vector3 direction)
        {
            this.Name      = name;
            this.Direction = direction;
        }

        public static GizmoAxis Parse(string handleName)
        {
            if (handleName.Contains("X"))
            {
                return X;
            }

            if (handleName.Contains("Y"))
            {
                return Y;
            }

            if (handleName.Contains("Z"))
            {
                return Z;
            }

            return UNIFORM;
        }
    }
}
