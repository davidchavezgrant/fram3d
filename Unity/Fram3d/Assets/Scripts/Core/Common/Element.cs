using System;
using System.Numerics;
namespace Fram3d.Core.Common
{
    public class Element
    {
        private Vector3 _position;

        public Element(ElementId id, string name)
        {
            this.Id   = id;
            this.Name = name;
        }

        public float     BoundingRadius { get; set; }

        /// <summary>
        /// Distance from the element's origin to its lowest geometry point.
        /// Set by the Engine layer from mesh bounds. Used by Position setter
        /// to prevent the element's visible geometry from clipping through Y=0.
        /// </summary>
        public float     GroundOffset   { get; set; }

        public ElementId Id             { get; }
        public string    Name           { get; set; }

        public Vector3 Position
        {
            get => this._position;
            set => this._position = new Vector3(value.X, MathF.Max(this.GroundOffset, value.Y), value.Z);
        }

        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public float      Scale    { get; set; } = 1f;
    }
}