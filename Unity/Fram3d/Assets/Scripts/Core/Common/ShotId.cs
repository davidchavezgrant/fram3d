using System;
namespace Fram3d.Core.Common
{
    public sealed class ShotId: IEquatable<ShotId>
    {
        public ShotId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("ShotId cannot be empty");
            }

            this.Value = value;
        }

        public          Guid   Value                                  { get; }
        public          bool   Equals(ShotId other)                   => other != null          && this.Value.Equals(other.Value);
        public override bool   Equals(object obj)                     => obj is ShotId other    && this.Equals(other);
        public override int    GetHashCode()                          => this.Value.GetHashCode();
        public override string ToString()                             => this.Value.ToString();
        public static   bool   operator ==(ShotId left, ShotId right) => Equals(left, right);
        public static   bool   operator !=(ShotId left, ShotId right) => !Equals(left, right);
    }
}
