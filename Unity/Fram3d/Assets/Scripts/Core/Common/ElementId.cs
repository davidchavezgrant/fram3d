using System;
namespace Fram3d.Core.Common
{
    public sealed class ElementId: IEquatable<ElementId>
    {
        public Guid Value { get; }

        public ElementId(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("ElementId cannot be empty");

            this.Value = value;
        }

        public          bool   Equals(ElementId other)                    => other != null          && this.Value.Equals(other.Value);
        public override bool   Equals(object    obj)                      => obj is ElementId other && this.Equals(other);
        public override int    GetHashCode()                              => this.Value.GetHashCode();
        public override string ToString()                                 => this.Value.ToString();
        public static   bool operator ==(ElementId left, ElementId right) => Equals(left, right);
        public static   bool operator !=(ElementId left, ElementId right) => !Equals(left, right);
    }
}