using System;
namespace Fram3d.Core.Common
{
    public sealed class KeyframeId: IEquatable<KeyframeId>
    {
        public KeyframeId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("KeyframeId cannot be empty");
            }

            this.Value = value;
        }

        public          Guid   Value                                          { get; }
        public          bool   Equals(KeyframeId other)                       => other != null            && this.Value.Equals(other.Value);
        public override bool   Equals(object     obj)                         => obj is KeyframeId other  && this.Equals(other);
        public override int    GetHashCode()                                  => this.Value.GetHashCode();
        public override string ToString()                                     => this.Value.ToString();
        public static   bool   operator ==(KeyframeId left, KeyframeId right) => Equals(left, right);
        public static   bool   operator !=(KeyframeId left, KeyframeId right) => !Equals(left, right);
    }
}
