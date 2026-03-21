using System.Numerics;
namespace Fram3d.Core.Common
{
    public class Element
    {
        public ElementId  Id             { get; }
        public string     Name           { get; set; }
        public Vector3    Position       { get; set; }
        public Quaternion Rotation       { get; set; } = Quaternion.Identity;
        public float      Scale          { get; set; } = 1f;
        public float      BoundingRadius { get; set; }

        public Element(ElementId id, string name)
        {
            this.Id = id;
            this.Name  = name;
        }
    }
}