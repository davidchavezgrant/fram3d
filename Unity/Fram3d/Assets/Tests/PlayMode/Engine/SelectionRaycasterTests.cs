using System.Collections;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Fram3d.Tests.Engine
{
    /// <summary>
    /// Play Mode tests for SelectionRaycaster. Verifies physics raycasting
    /// resolves to the correct Element, including compound element detection
    /// via GetComponentInParent.
    /// </summary>
    public sealed class SelectionRaycasterTests
    {
        private Camera             _camera;
        private GameObject         _cameraGo;
        private GameObject         _cube;
        private SelectionRaycaster _raycaster;

        [UnityTest]
        public IEnumerator Raycast__ReturnsElement__When__HittingElementWithCollider()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            // Raycast to center of screen where the cube is positioned
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

            // Raycast to top-left corner where nothing is
            var corner  = new Vector2(10f, Screen.height - 10f);
            var element = this._raycaster.Raycast(corner);

            Assert.IsNull(element, "Should not hit anything in empty space");
        }

        [UnityTest]
        public IEnumerator Raycast__ReturnsNull__When__ObjectHasNoElementBehaviour()
        {
            // Remove ElementBehaviour from the cube
            var behaviour = this._cube.GetComponent<ElementBehaviour>();
            Object.DestroyImmediate(behaviour);
            yield return null;
            yield return new WaitForFixedUpdate();

            var screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var element      = this._raycaster.Raycast(screenCenter);

            Assert.IsNull(element, "Should return null for objects without ElementBehaviour");
        }

        [UnityTest]
        public IEnumerator Raycast__ResolvesParent__When__HittingChildCollider()
        {
            // Create a compound element: parent with ElementBehaviour, child with collider
            var parent = new GameObject("CompoundParent");
            parent.transform.position = new Vector3(0f, 0f, 5f);
            parent.AddComponent<ElementBehaviour>();

            var child = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            child.name                    = "ChildMesh";
            child.transform.parent        = parent.transform;
            child.transform.localPosition = Vector3.zero;

            // Remove the direct cube so only the compound element is hit
            Object.DestroyImmediate(this._cube);
            this._cube = null;
            yield return null;
            yield return new WaitForFixedUpdate();

            var screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var element      = this._raycaster.Raycast(screenCenter);

            Assert.IsNotNull(element, "Should hit the child collider");
            Assert.AreEqual("CompoundParent", element.Name,
                            "Should resolve to parent ElementBehaviour, not child");

            Object.DestroyImmediate(parent);
        }

        [UnityTest]
        public IEnumerator Raycast__IgnoresGizmoLayer__When__ObjectOnLayer6()
        {
            // Layer 6 is the Gizmo layer — raycaster should exclude it
            var gizmoObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gizmoObj.name               = "GizmoHandle";
            gizmoObj.layer              = 6;
            gizmoObj.transform.position = new Vector3(0f, 0f, 3f); // closer than TestCube
            gizmoObj.AddComponent<ElementBehaviour>();

            // Remove TestCube so only gizmoObj is in front of camera
            Object.DestroyImmediate(this._cube);
            this._cube = null;
            yield return null;
            yield return new WaitForFixedUpdate();

            var screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var element      = this._raycaster.Raycast(screenCenter);

            Assert.IsNull(element,
                          "Objects on the Gizmo layer (6) should be ignored by SelectionRaycaster");

            Object.DestroyImmediate(gizmoObj);
        }

        [SetUp]
        public void SetUp()
        {
            this._cameraGo = new GameObject("TestCamera");
            this._camera   = this._cameraGo.AddComponent<Camera>();
            this._cameraGo.transform.position = new Vector3(0f, 0f, 0f);
            this._cameraGo.transform.rotation = Quaternion.identity;
            this._raycaster = this._cameraGo.AddComponent<SelectionRaycaster>();

            // Wire camera via reflection (SerializeField)
            var field = typeof(SelectionRaycaster)
                .GetField("targetCamera",
                           System.Reflection.BindingFlags.NonPublic
                         | System.Reflection.BindingFlags.Instance);

            field.SetValue(this._raycaster, this._camera);

            // Place a cube in front of the camera
            this._cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            this._cube.name               = "TestCube";
            this._cube.transform.position = new Vector3(0f, 0f, 5f);
            this._cube.AddComponent<ElementBehaviour>();
        }

        [TearDown]
        public void TearDown()
        {
            if (this._cube != null)
                Object.DestroyImmediate(this._cube);

            Object.DestroyImmediate(this._cameraGo);
        }
    }
}
