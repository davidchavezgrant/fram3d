using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Fram3d.Core.Scenes;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Fram3d.Tests.Engine
{
    /// <summary>
    /// Play Mode tests for GizmoBehaviour. Verifies tool switching,
    /// show/hide based on selection, and drag guard conditions.
    ///
    /// Important lifecycle detail: LateUpdate resets the tool to TRANSLATE
    /// on every new selection. Tests that need a specific tool must:
    /// 1. Select the element and yield (let LateUpdate process the change)
    /// 2. THEN set the desired tool
    /// 3. Call the method under test WITHOUT yielding again
    /// </summary>
    public sealed class GizmoBehaviourTests
    {
        private GameObject           _cameraGo;
        private GizmoBehaviour      _controller;
        private GameObject           _cube;
        private List<GameObject>     _extras;
        private SelectionDisplay _highlighter;

        [UnityTest]
        public IEnumerator LateUpdate__HidesGizmo__When__NothingSelected()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            this._highlighter.Selection.Deselect();
            yield return null;

            Assert.IsFalse(this._controller.IsVisible, "Gizmo should be hidden when nothing is selected");
        }

        // --- Tool resets on new selection ---

        [UnityTest]
        public IEnumerator LateUpdate__ResetsToTranslate__When__NewElementSelected()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            this._controller.SetActiveTool(ActiveTool.SCALE);
            Assert.AreSame(ActiveTool.SCALE, this._controller.ActiveTool);
            var cube2 = CreateExtra(PrimitiveType.Sphere);
            yield return null;

            var element2 = cube2.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element2.Id);
            yield return null;

            Assert.AreSame(ActiveTool.TRANSLATE, this._controller.ActiveTool, "Tool should reset to Translate on new selection");
        }

        // --- Show/hide based on selection ---

        [UnityTest]
        public IEnumerator LateUpdate__ShowsGizmo__When__ElementSelected()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            Assert.IsTrue(this._controller.IsVisible, "Gizmo should be visible when element is selected");
        }

        // --- Tool switching ---

        [Test]
        public void SetActiveTool__ChangesTool__When__Called()
        {
            this._controller.SetActiveTool(ActiveTool.ROTATE);
            Assert.AreSame(ActiveTool.ROTATE, this._controller.ActiveTool);
        }

        [Test]
        public void SetActiveTool__DefaultsToTranslate__When__Created()
        {
            Assert.AreSame(ActiveTool.TRANSLATE, this._controller.ActiveTool);
        }

        [SetUp]
        public void SetUp()
        {
            this._extras   = new List<GameObject>();
            this._cameraGo = new GameObject("TestCamera");
            this._cameraGo.AddComponent<Camera>();
            this._highlighter = this._cameraGo.AddComponent<SelectionDisplay>();
            this._controller  = this._cameraGo.AddComponent<GizmoBehaviour>();
            SetField(this._controller, "selectionDisplay", this._highlighter);
            SetField(this._controller, "targetCamera",         this._cameraGo.GetComponent<Camera>());
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

            Object.DestroyImmediate(this._cube);
            Object.DestroyImmediate(this._cameraGo);
        }

        [UnityTest]
        public IEnumerator TryBeginDrag__ReturnsFalse__When__NothingSelected()
        {
            yield return null;

            this._controller.SetActiveTool(ActiveTool.TRANSLATE);
            var result = this._controller.TryBeginDrag(new Vector2(100f, 100f));
            Assert.IsFalse(result);
        }

        // --- UpdateHover ---

        [UnityTest]
        public IEnumerator UpdateHover__IsNotHovering__When__NoGizmoVisible()
        {
            yield return null;

            // No selection → gizmo hidden
            this._controller.UpdateHover(new Vector2(Screen.width / 2f, Screen.height / 2f));
            Assert.IsFalse(this._controller.IsHoveringHandle, "Should not hover when gizmo is not visible");
        }

        [UnityTest]
        public IEnumerator UpdateHover__IsNotHovering__When__SelectToolActive()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            this._controller.SetActiveTool(ActiveTool.SELECT);
            this._controller.UpdateHover(new Vector2(Screen.width / 2f, Screen.height / 2f));
            Assert.IsFalse(this._controller.IsHoveringHandle, "Should not hover when Select tool is active");
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

        // --- TryResetActiveTool ---

        [UnityTest]
        public IEnumerator TryResetActiveTool__ResetsPosition__When__TranslateActive()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            element.Position = new System.Numerics.Vector3(5f, 3f, -2f);
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            var result = this._controller.TryResetActiveTool();
            Assert.IsTrue(result);
            Assert.AreEqual(0f, element.Position.X, 0.001f);
            Assert.AreEqual(element.GroundOffset, element.Position.Y, 0.001f);
            Assert.AreEqual(0f, element.Position.Z, 0.001f);
        }

        [UnityTest]
        public IEnumerator TryResetActiveTool__ResetsRotation__When__RotateActive()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            element.Rotation = System.Numerics.Quaternion.CreateFromAxisAngle(System.Numerics.Vector3.UnitY, 1.0f);
            this._highlighter.Selection.Select(element.Id);
            yield return null;

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
            yield return null;

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
            yield return null;

            this._controller.SetActiveTool(ActiveTool.SELECT);
            var result = this._controller.TryResetActiveTool();
            Assert.IsFalse(result);
        }

        // --- Helpers ---

        private GameObject CreateExtra(PrimitiveType type)
        {
            var go = GameObject.CreatePrimitive(type);
            go.AddComponent<ElementBehaviour>();
            this._extras.Add(go);
            return go;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }
    }
}