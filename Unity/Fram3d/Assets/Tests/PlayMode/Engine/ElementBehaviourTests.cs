using System.Collections;
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
            var go2       = new GameObject("TestElement2");
            go2.AddComponent<ElementBehaviour>();
            yield return null;

            var id1 = this._go.GetComponent<ElementBehaviour>().Element.Id;
            var id2 = go2.GetComponent<ElementBehaviour>().Element.Id;
            Assert.AreNotEqual(id1, id2);

            Object.DestroyImmediate(go2);
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
