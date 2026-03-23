using System.Collections;
using Fram3d.Engine.Conversion;
using NUnit.Framework;
using UnityEngine.TestTools;
using SysVector3 = System.Numerics.Vector3;
using SysQuaternion = System.Numerics.Quaternion;
using UnityVector3 = UnityEngine.Vector3;
using UnityQuaternion = UnityEngine.Quaternion;
namespace Fram3d.Tests.Engine
{
    /// <summary>
    /// Direct tests for coordinate system conversion between System.Numerics
    /// (right-handed, -Z forward) and Unity (left-handed, +Z forward).
    /// Catches conversion bugs immediately instead of surfacing as confusing
    /// position/rotation failures in CameraBehaviour tests.
    /// </summary>
    public sealed class VectorExtensionsTests
    {
        // --- Roundtrip: Quaternion ---

        [Test]
        public void Roundtrip__ProducesOriginal__When__QuaternionConvertedBothWays()
        {
            var original = new SysQuaternion(0.1f,
                                             0.2f,
                                             0.3f,
                                             0.9f);

            var roundtrip = original.ToUnity().ToSystem();
            Assert.AreEqual(original.X, roundtrip.X, 0.001f);
            Assert.AreEqual(original.Y, roundtrip.Y, 0.001f);
            Assert.AreEqual(original.Z, roundtrip.Z, 0.001f);
            Assert.AreEqual(original.W, roundtrip.W, 0.001f);
        }

        // --- Roundtrip: Vector3 ---

        [Test]
        public void Roundtrip__ProducesOriginal__When__Vector3ConvertedBothWays()
        {
            var original  = new SysVector3(3f, -7f, 11f);
            var roundtrip = original.ToUnity().ToSystem();
            Assert.AreEqual(original.X, roundtrip.X, 0.001f);
            Assert.AreEqual(original.Y, roundtrip.Y, 0.001f);
            Assert.AreEqual(original.Z, roundtrip.Z, 0.001f);
        }

        // --- Quaternion: ToSystem ---

        [Test]
        public void ToSystem__NegatesXY__When__ConvertingQuaternion()
        {
            var unity = new UnityQuaternion(0.1f,
                                            0.2f,
                                            0.3f,
                                            0.9f);

            var sys = unity.ToSystem();
            Assert.AreEqual(-0.1f, sys.X, 0.001f);
            Assert.AreEqual(-0.2f, sys.Y, 0.001f);
            Assert.AreEqual(0.3f,  sys.Z, 0.001f);
            Assert.AreEqual(0.9f,  sys.W, 0.001f);
        }

        // --- Vector3: ToSystem ---

        [Test]
        public void ToSystem__NegatesZ__When__ConvertingVector3()
        {
            var unity = new UnityVector3(1f, 2f, 3f);
            var sys   = unity.ToSystem();
            Assert.AreEqual(1f,  sys.X, 0.001f);
            Assert.AreEqual(2f,  sys.Y, 0.001f);
            Assert.AreEqual(-3f, sys.Z, 0.001f);
        }

        // --- Quaternion: ToUnity ---

        [Test]
        public void ToUnity__NegatesXY__When__ConvertingQuaternion()
        {
            var sys = new SysQuaternion(0.1f,
                                        0.2f,
                                        0.3f,
                                        0.9f);

            var unity = sys.ToUnity();
            Assert.AreEqual(-0.1f, unity.x, 0.001f);
            Assert.AreEqual(-0.2f, unity.y, 0.001f);
            Assert.AreEqual(0.3f,  unity.z, 0.001f);
            Assert.AreEqual(0.9f,  unity.w, 0.001f);
        }

        // --- Vector3: ToUnity ---

        [Test]
        public void ToUnity__NegatesZ__When__ConvertingVector3()
        {
            var sys   = new SysVector3(1f, 2f, 3f);
            var unity = sys.ToUnity();
            Assert.AreEqual(1f,  unity.x, 0.001f);
            Assert.AreEqual(2f,  unity.y, 0.001f);
            Assert.AreEqual(-3f, unity.z, 0.001f);
        }

        [Test]
        public void ToUnity__PreservesXY__When__ConvertingVector3()
        {
            var sys   = new SysVector3(-5f, 10f, 0f);
            var unity = sys.ToUnity();
            Assert.AreEqual(-5f, unity.x, 0.001f);
            Assert.AreEqual(10f, unity.y, 0.001f);
        }
    }
}