using SysVector3 = System.Numerics.Vector3;
using SysQuaternion = System.Numerics.Quaternion;
namespace Fram3d.Engine.Conversion
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Convert UnityEngine.Vector3 to System.Numerics.Vector3.
        /// Negates Z to convert from left-handed to right-handed.
        /// </summary>
        public static SysVector3 ToSystem(this UnityEngine.Vector3 v) => new(v.x, v.y, -v.z);

        /// <summary>
        /// Convert UnityEngine.Quaternion to System.Numerics.Quaternion.
        /// Negates X and Y to convert from left-handed to right-handed.
        /// </summary>
        public static SysQuaternion ToSystem(this UnityEngine.Quaternion q) => new(-q.x,
                                                                                   -q.y,
                                                                                   q.z,
                                                                                   q.w);

        /// <summary>
        /// Convert System.Numerics.Vector3 to UnityEngine.Vector3.
        /// System.Numerics is right-handed (-Z forward), Unity is left-handed (+Z forward).
        /// We negate Z to convert between coordinate systems.
        /// </summary>
        public static UnityEngine.Vector3 ToUnity(this SysVector3 v) => new(v.X, v.Y, -v.Z);

        /// <summary>
        /// Convert System.Numerics.Quaternion to UnityEngine.Quaternion.
        /// To convert a rotation between handedness systems, negate the X and Y components
        /// (which flips the rotation axes that change direction with Z negation).
        /// </summary>
        public static UnityEngine.Quaternion ToUnity(this SysQuaternion q) => new(-q.X,
                                                                                  -q.Y,
                                                                                  q.Z,
                                                                                  q.W);
    }
}