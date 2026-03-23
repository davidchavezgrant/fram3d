using System.Collections;
using Fram3d.Engine.Conversion;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Fram3d.Tests.Engine
{
    public sealed class ElementBehaviourTests
    {
        private GameObject _go;

        [UnityTest]
        public IEnumerator Awake__CreatesElement__When__Added()
        {
            yield return null;

            var behaviour = this._go.GetComponent<ElementBehaviour>();
            Assert.IsNotNull(behaviour.Element);
        }

        [UnityTest]
        public IEnumerator Awake__ElementNameMatchesGameObject__When__Created()
        {
            yield return null;

            var behaviour = this._go.GetComponent<ElementBehaviour>();
            Assert.AreEqual("TestElement", behaviour.Element.Name);
        }

        [UnityTest]
        public IEnumerator Awake__ElementIdIsUnique__When__MultipleCreated()
        {
            var go2 = new GameObject("TestElement2");
            go2.AddComponent<ElementBehaviour>();
            yield return null;

            var id1 = this._go.GetComponent<ElementBehaviour>().Element.Id;
            var id2 = go2.GetComponent<ElementBehaviour>().Element.Id;
            Assert.AreNotEqual(id1, id2);

            Object.DestroyImmediate(go2);
        }

        // --- Awake captures initial transform ---

        [UnityTest]
        public IEnumerator Awake__CapturesPosition__When__GameObjectAtNonOrigin()
        {
            var go = new GameObject("OffOrigin");
            go.transform.position = new Vector3(3f, 1f, -5f);
            go.AddComponent<ElementBehaviour>();
            yield return null;

            var element = go.GetComponent<ElementBehaviour>().Element;

            // Core uses System.Numerics (right-handed, -Z forward).
            // ToSystem negates Z: Unity (3, 1, -5) → Core (3, 1, 5)
            Assert.AreEqual(3f, element.Position.X, 0.001f);
            Assert.AreEqual(1f, element.Position.Y, 0.001f);
            Assert.AreEqual(5f, element.Position.Z, 0.001f);

            Object.DestroyImmediate(go);
        }

        [UnityTest]
        public IEnumerator Awake__CapturesRotation__When__GameObjectRotated()
        {
            var go = new GameObject("Rotated");
            go.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            go.AddComponent<ElementBehaviour>();
            yield return null;

            var element  = go.GetComponent<ElementBehaviour>().Element;
            var coreRot  = element.Rotation;
            var expected = go.transform.rotation;

            // Round-trip: Unity → ToSystem → ToUnity should reproduce original
            var roundTrip = coreRot.ToUnity();
            Assert.AreEqual(expected.x, roundTrip.x, 0.001f);
            Assert.AreEqual(expected.y, roundTrip.y, 0.001f);
            Assert.AreEqual(expected.z, roundTrip.z, 0.001f);
            Assert.AreEqual(expected.w, roundTrip.w, 0.001f);

            Object.DestroyImmediate(go);
        }

        // --- LateUpdate sync: Core → Unity Transform ---

        [UnityTest]
        public IEnumerator LateUpdate__SyncsPosition__When__ElementPositionChanged()
        {
            yield return null;

            var behaviour = this._go.GetComponent<ElementBehaviour>();
            behaviour.Element.Position = new System.Numerics.Vector3(5f, 2f, -3f);
            yield return null;

            // ToUnity negates Z: Core (5, 2, -3) → Unity (5, 2, 3)
            var pos = this._go.transform.position;
            Assert.AreEqual(5f, pos.x, 0.001f);
            Assert.AreEqual(2f, pos.y, 0.001f);
            Assert.AreEqual(3f, pos.z, 0.001f);
        }

        [UnityTest]
        public IEnumerator LateUpdate__SyncsRotation__When__ElementRotationChanged()
        {
            yield return null;

            var behaviour = this._go.GetComponent<ElementBehaviour>();
            var axis      = System.Numerics.Vector3.UnitY;
            behaviour.Element.Rotation = System.Numerics.Quaternion.CreateFromAxisAngle(axis, 1.0f);
            yield return null;

            // Rotation should be non-identity after sync
            var rot = this._go.transform.rotation;
            Assert.AreNotEqual(Quaternion.identity, rot);
        }

        [UnityTest]
        public IEnumerator LateUpdate__SyncsScale__When__ElementScaleChanged()
        {
            yield return null;

            var behaviour = this._go.GetComponent<ElementBehaviour>();
            behaviour.Element.Scale = 2.5f;
            yield return null;

            var scale = this._go.transform.localScale;
            Assert.AreEqual(2.5f, scale.x, 0.001f);
            Assert.AreEqual(2.5f, scale.y, 0.001f);
            Assert.AreEqual(2.5f, scale.z, 0.001f);
        }

        [UnityTest]
        public IEnumerator LateUpdate__RoundTripsPosition__When__PositionSetFromTransform()
        {
            // Verify the Awake capture → LateUpdate sync round-trip is stable:
            // the object shouldn't drift from its initial position
            var go = new GameObject("RoundTrip");
            go.transform.position = new Vector3(7f, -2f, 13f);
            go.AddComponent<ElementBehaviour>();
            yield return null;
            yield return null; // Two frames to let both Awake and LateUpdate run

            var pos = go.transform.position;
            Assert.AreEqual(7f,  pos.x, 0.001f, "X should not drift");
            Assert.AreEqual(-2f, pos.y, 0.001f, "Y should not drift");
            Assert.AreEqual(13f, pos.z, 0.001f, "Z should not drift");

            Object.DestroyImmediate(go);
        }

        [SetUp]
        public void SetUp()
        {
            this._go = new GameObject("TestElement");
            this._go.AddComponent<ElementBehaviour>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(this._go);
        }
    }
}
