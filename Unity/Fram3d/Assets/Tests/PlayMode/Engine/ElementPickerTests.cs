using System.Collections;
using System.Collections.Generic;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Fram3d.Tests.Engine
{
    /// <summary>
    /// Play Mode tests for ElementPicker. Verifies physics raycasting
    /// resolves to the correct Element, including compound element detection
    /// via GetComponentInParent.
    /// </summary>
    public sealed class ElementPickerTests
    {
        private Camera             _camera;
        private GameObject         _cameraGo;
        private GameObject         _cube;
        private List<GameObject>   _extras;
        private ElementPicker _raycaster;

        [UnityTest]
        public IEnumerator Raycast__IgnoresGizmoLayer__When__ObjectOnGizmoLayer()
        {
            var gizmoObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gizmoObj.name               = "GizmoHandle";
            gizmoObj.layer              = GizmoBehaviour.GIZMO_LAYER_INDEX;
            gizmoObj.transform.position = new Vector3(0f, 0f, 3f);
            gizmoObj.AddComponent<ElementBehaviour>();
            this._extras.Add(gizmoObj);
            Object.DestroyImmediate(this._cube);
            this._cube = null;
            yield return null;
            yield return new WaitForFixedUpdate();

            var screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var element      = this._raycaster.Raycast(screenCenter);
            Assert.IsNull(element, "Objects on the Gizmo layer should be ignored by ElementPicker");
        }

        [UnityTest]
        public IEnumerator Raycast__ResolvesParent__When__HittingChildCollider()
        {
            var parent = new GameObject("CompoundParent");
            parent.transform.position = new Vector3(0f, 0f, 5f);
            parent.AddComponent<ElementBehaviour>();
            this._extras.Add(parent);
            var child = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            child.name                    = "ChildMesh";
            child.transform.parent        = parent.transform;
            child.transform.localPosition = Vector3.zero;
            Object.DestroyImmediate(this._cube);
            this._cube = null;
            yield return null;
            yield return new WaitForFixedUpdate();

            var screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var element      = this._raycaster.Raycast(screenCenter);
            Assert.IsNotNull(element, "Should hit the child collider");
            Assert.AreEqual("CompoundParent", element.Name, "Should resolve to parent ElementBehaviour, not child");
        }

        [UnityTest]
        public IEnumerator Raycast__ReturnsElement__When__HittingElementWithCollider()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            var screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var element      = this._raycaster.Raycast(screenCenter);
            Assert.IsNotNull(element, "Should hit the cube in front of the camera");
            Assert.AreEqual("TestCube", element.Name);
        }

        [UnityTest]
        public IEnumerator Raycast__ReturnsNull__When__HittingEmptySpace()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            var corner  = new Vector2(10f, Screen.height - 10f);
            var element = this._raycaster.Raycast(corner);
            Assert.IsNull(element, "Should not hit anything in empty space");
        }

        [UnityTest]
        public IEnumerator Raycast__ReturnsNull__When__ObjectHasNoElementBehaviour()
        {
            var behaviour = this._cube.GetComponent<ElementBehaviour>();
            Object.DestroyImmediate(behaviour);
            yield return null;
            yield return new WaitForFixedUpdate();

            var screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var element      = this._raycaster.Raycast(screenCenter);
            Assert.IsNull(element, "Should return null for objects without ElementBehaviour");
        }

        [SetUp]
        public void SetUp()
        {
            this._extras                      = new System.Collections.Generic.List<GameObject>();
            this._cameraGo                    = new GameObject("TestCamera");
            this._camera                      = this._cameraGo.AddComponent<Camera>();
            this._cameraGo.transform.position = new Vector3(0f, 0f, 0f);
            this._cameraGo.transform.rotation = Quaternion.identity;
            this._raycaster                   = this._cameraGo.AddComponent<ElementPicker>();

            var field = typeof(ElementPicker).GetField("targetCamera",
                                                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            field.SetValue(this._raycaster, this._camera);
            this._cube                    = GameObject.CreatePrimitive(PrimitiveType.Cube);
            this._cube.name               = "TestCube";
            this._cube.transform.position = new Vector3(0f, 0f, 5f);
            this._cube.AddComponent<ElementBehaviour>();
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

            if (this._cube != null)
            {
                Object.DestroyImmediate(this._cube);
            }

            Object.DestroyImmediate(this._cameraGo);
        }
    }
}