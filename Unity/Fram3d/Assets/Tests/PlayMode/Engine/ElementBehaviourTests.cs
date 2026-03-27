using System.Collections;
using System.Collections.Generic;
using Fram3d.Engine.Conversion;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Fram3d.Tests.Engine
{
    public sealed class ElementBehaviourTests
    {
        private List<GameObject> _extras;
        private GameObject       _go;

        [Test]
        public void Awake__CapturesPosition__When__GameObjectAtNonOrigin()
        {
            var go = new GameObject("OffOrigin");
            go.transform.position = new Vector3(3f, 1f, -5f);
            go.AddComponent<ElementBehaviour>();
            this._extras.Add(go);
            var element = go.GetComponent<ElementBehaviour>().Element;

            // Core uses System.Numerics (right-handed, -Z forward).
            // ToSystem negates Z: Unity (3, 1, -5) → Core (3, 1, 5)
            Assert.AreEqual(3f, element.Position.X, 0.001f);
            Assert.AreEqual(1f, element.Position.Y, 0.001f);
            Assert.AreEqual(5f, element.Position.Z, 0.001f);
        }

        [Test]
        public void Awake__CapturesRotation__When__GameObjectRotated()
        {
            var go = new GameObject("Rotated");
            go.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            go.AddComponent<ElementBehaviour>();
            this._extras.Add(go);
            var element  = go.GetComponent<ElementBehaviour>().Element;
            var coreRot  = element.Rotation;
            var expected = go.transform.rotation;

            // Round-trip: Unity → ToSystem → ToUnity should reproduce original
            var roundTrip = coreRot.ToUnity();
            Assert.AreEqual(expected.x, roundTrip.x, 0.001f);
            Assert.AreEqual(expected.y, roundTrip.y, 0.001f);
            Assert.AreEqual(expected.z, roundTrip.z, 0.001f);
            Assert.AreEqual(expected.w, roundTrip.w, 0.001f);
        }

        // --- Awake tests: synchronous, no frame needed ---

        [Test]
        public void Awake__CreatesElement__When__Added()
        {
            var behaviour = this._go.GetComponent<ElementBehaviour>();
            Assert.IsNotNull(behaviour.Element);
        }

        [Test]
        public void Awake__ElementIdIsUnique__When__MultipleCreated()
        {
            var go2 = CreateExtra("TestElement2");
            var id1 = this._go.GetComponent<ElementBehaviour>().Element.Id;
            var id2 = go2.GetComponent<ElementBehaviour>().Element.Id;
            Assert.AreNotEqual(id1, id2);
        }

        [Test]
        public void Awake__ElementNameMatchesGameObject__When__Created()
        {
            var behaviour = this._go.GetComponent<ElementBehaviour>();
            Assert.AreEqual("TestElement", behaviour.Element.Name);
        }

        [UnityTest]
        public IEnumerator LateUpdate__RoundTripsPosition__When__PositionSetFromTransform()
        {
            var go = new GameObject("RoundTrip");
            go.transform.position = new Vector3(7f, 4f, 13f);
            go.AddComponent<ElementBehaviour>();
            this._extras.Add(go);
            yield return null;
            yield return null;

            var pos = go.transform.position;

            Assert.AreEqual(7f,
                            pos.x,
                            0.001f,
                            "X should not drift");

            Assert.AreEqual(4f,
                            pos.y,
                            0.001f,
                            "Y should not drift");

            Assert.AreEqual(13f,
                            pos.z,
                            0.001f,
                            "Z should not drift");
        }

        // --- LateUpdate sync: needs frames ---

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

        [SetUp]
        public void SetUp()
        {
            this._extras = new List<GameObject>();
            this._go     = new GameObject("TestElement");
            this._go.AddComponent<ElementBehaviour>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in this._extras)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }

            Object.DestroyImmediate(this._go);
        }

        // --- Ground offset ---

        [Test]
        public void Awake__ComputesGroundOffset__When__PrimitiveMeshPresent()
        {
            // A unit cube at y=2 has bounds.min.y = 1.5
            // GroundOffset = position.y - bounds.min.y = 2 - 1.5 = 0.5
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = new Vector3(0f, 2f, 0f);
            go.AddComponent<ElementBehaviour>();
            this._extras.Add(go);

            var element = go.GetComponent<ElementBehaviour>().Element;
            Assert.AreEqual(0.5f, element.GroundOffset, 0.01f,
                "Ground offset should be distance from origin to mesh bottom");
        }

        [Test]
        public void Awake__GroundOffsetIsZero__When__NoRenderer()
        {
            // A plain GameObject with no Renderer
            var element = this._go.GetComponent<ElementBehaviour>().Element;

            Assert.AreEqual(0f, element.GroundOffset, 0.001f,
                "Ground offset should be 0 when no Renderer exists");
        }

        // --- Scale capture ---

        [Test]
        public void Awake__ScaleDefaultsToOne__When__Created()
        {
            var element = this._go.GetComponent<ElementBehaviour>().Element;

            Assert.AreEqual(1f, element.Scale, 0.001f);
        }

        // --- Helpers ---

        private GameObject CreateExtra(string name)
        {
            var go = new GameObject(name);
            go.AddComponent<ElementBehaviour>();
            this._extras.Add(go);
            return go;
        }
    }
}