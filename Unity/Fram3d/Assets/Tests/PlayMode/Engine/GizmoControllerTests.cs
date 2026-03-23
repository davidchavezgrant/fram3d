using System.Collections;
using System.Reflection;
using Fram3d.Core.Scene;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Fram3d.Tests.Engine
{
    /// <summary>
    /// Play Mode tests for GizmoController. Verifies tool switching,
    /// show/hide based on selection, and drag guard conditions.
    ///
    /// Important lifecycle detail: LateUpdate resets the tool to TRANSLATE
    /// on every new selection. Tests that need a specific tool must:
    /// 1. Select the element and yield (let LateUpdate process the change)
    /// 2. THEN set the desired tool
    /// 3. Call the method under test WITHOUT yielding again
    /// </summary>
    public sealed class GizmoControllerTests
    {
        private GameObject           _cameraGo;
        private GizmoController      _controller;
        private GameObject           _cube;
        private SelectionHighlighter _highlighter;

        // --- Tool switching ---

        [UnityTest]
        public IEnumerator SetActiveTool__ChangesTool__When__Called()
        {
            yield return null;

            this._controller.SetActiveTool(ActiveTool.ROTATE);

            Assert.AreSame(ActiveTool.ROTATE, this._controller.ActiveTool);
        }

        [UnityTest]
        public IEnumerator SetActiveTool__DefaultsToTranslate__When__Created()
        {
            yield return null;

            Assert.AreSame(ActiveTool.TRANSLATE, this._controller.ActiveTool);
        }

        // --- Show/hide based on selection ---

        [UnityTest]
        public IEnumerator LateUpdate__ShowsGizmo__When__ElementSelected()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            Assert.IsTrue(this._controller.IsVisible,
                          "Gizmo should be visible when element is selected");
        }

        [UnityTest]
        public IEnumerator LateUpdate__HidesGizmo__When__NothingSelected()
        {
            yield return null;

            // Select then deselect
            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            this._highlighter.Selection.Deselect();
            yield return null;

            Assert.IsFalse(this._controller.IsVisible,
                           "Gizmo should be hidden when nothing is selected");
        }

        // --- Tool resets on new selection ---

        [UnityTest]
        public IEnumerator LateUpdate__ResetsToTranslate__When__NewElementSelected()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            // Switch to Scale after selection settled
            this._controller.SetActiveTool(ActiveTool.SCALE);
            Assert.AreSame(ActiveTool.SCALE, this._controller.ActiveTool);

            // Select a different element
            var cube2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cube2.AddComponent<ElementBehaviour>();
            yield return null;

            var element2 = cube2.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element2.Id);
            yield return null;

            Assert.AreSame(ActiveTool.TRANSLATE, this._controller.ActiveTool,
                           "Tool should reset to Translate on new selection");

            Object.DestroyImmediate(cube2);
        }

        // --- TryResetActiveTool ---

        [UnityTest]
        public IEnumerator TryResetActiveTool__ResetsPosition__When__TranslateActive()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            element.Position = new System.Numerics.Vector3(5f, 3f, -2f);
            this._highlighter.Selection.Select(element.Id);
            yield return null; // LateUpdate processes selection, resets to TRANSLATE

            // Tool is already TRANSLATE after selection — no need to set it
            var result = this._controller.TryResetActiveTool();

            Assert.IsTrue(result);
            Assert.AreEqual(0f, element.Position.X, 0.001f);
            Assert.AreEqual(0f, element.Position.Y, 0.001f);
            Assert.AreEqual(0f, element.Position.Z, 0.001f);
        }

        [UnityTest]
        public IEnumerator TryResetActiveTool__ResetsRotation__When__RotateActive()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            element.Rotation = System.Numerics.Quaternion.CreateFromAxisAngle(
                System.Numerics.Vector3.UnitY, 1.0f);
            this._highlighter.Selection.Select(element.Id);
            yield return null; // LateUpdate processes selection

            // Set tool AFTER selection settled, call reset WITHOUT yielding
            this._controller.SetActiveTool(ActiveTool.ROTATE);
            var result = this._controller.TryResetActiveTool();

            Assert.IsTrue(result);
            Assert.AreEqual(System.Numerics.Quaternion.Identity, element.Rotation);
        }

        [UnityTest]
        public IEnumerator TryResetActiveTool__ResetsScale__When__ScaleActive()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            element.Scale = 3f;
            this._highlighter.Selection.Select(element.Id);
            yield return null; // LateUpdate processes selection

            this._controller.SetActiveTool(ActiveTool.SCALE);
            var result = this._controller.TryResetActiveTool();

            Assert.IsTrue(result);
            Assert.AreEqual(1f, element.Scale, 0.001f);
        }

        [UnityTest]
        public IEnumerator TryResetActiveTool__ReturnsFalse__When__NothingSelected()
        {
            yield return null;

            this._controller.SetActiveTool(ActiveTool.TRANSLATE);

            var result = this._controller.TryResetActiveTool();

            Assert.IsFalse(result);
        }

        [UnityTest]
        public IEnumerator TryResetActiveTool__ReturnsFalse__When__SelectToolActive()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null; // LateUpdate processes selection

            // Set to SELECT after selection settled, call without yielding
            this._controller.SetActiveTool(ActiveTool.SELECT);
            var result = this._controller.TryResetActiveTool();

            Assert.IsFalse(result);
        }

        // --- TryBeginDrag guard conditions ---

        [UnityTest]
        public IEnumerator TryBeginDrag__ReturnsFalse__When__SelectToolActive()
        {
            yield return null;

            this._controller.SetActiveTool(ActiveTool.SELECT);

            var result = this._controller.TryBeginDrag(new Vector2(100f, 100f));

            Assert.IsFalse(result);
        }

        [UnityTest]
        public IEnumerator TryBeginDrag__ReturnsFalse__When__NothingSelected()
        {
            yield return null;

            this._controller.SetActiveTool(ActiveTool.TRANSLATE);

            var result = this._controller.TryBeginDrag(new Vector2(100f, 100f));

            Assert.IsFalse(result);
        }

        [SetUp]
        public void SetUp()
        {
            this._cameraGo = new GameObject("TestCamera");
            this._cameraGo.AddComponent<Camera>();
            this._highlighter = this._cameraGo.AddComponent<SelectionHighlighter>();
            this._controller  = this._cameraGo.AddComponent<GizmoController>();

            SetField(this._controller, "selectionHighlighter", this._highlighter);
            SetField(this._controller, "targetCamera", this._cameraGo.GetComponent<Camera>());

            this._cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            this._cube.name               = "TestCube";
            this._cube.transform.position = new Vector3(0f, 0f, 5f);
            this._cube.AddComponent<ElementBehaviour>();
        }

        [TearDown]
        public void TearDown()
        {
            // GizmoRoot is a scene root object created in GizmoController.Awake.
            // Destroying the controller doesn't destroy it, so clean up manually.
            // Collect first to avoid modifying the iterator when children are destroyed.
            var toDestroy = new System.Collections.Generic.List<GameObject>();
            var all       = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var t in all)
            {
                if (t != null && t.gameObject.name == "GizmoRoot" && t.parent == null)
                {
                    toDestroy.Add(t.gameObject);
                }
            }

            foreach (var go in toDestroy)
            {
                Object.DestroyImmediate(go);
            }

            Object.DestroyImmediate(this._cube);
            Object.DestroyImmediate(this._cameraGo);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }
    }
}
