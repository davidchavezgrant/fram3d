using System.Collections;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Fram3d.Tests.Engine
{
    /// <summary>
    /// Play Mode tests for SelectionHighlighter. Verifies that
    /// MaterialPropertyBlock color overrides are applied and removed
    /// correctly on renderers — does NOT re-test Selection domain
    /// logic (covered by xUnit).
    /// </summary>
    public sealed class SelectionHighlighterTests
    {
        private static readonly int BASE_COLOR = Shader.PropertyToID("_BaseColor");

        private GameObject _cube;
        private GameObject _highlighterGo;
        private SelectionHighlighter _highlighter;

        [UnityTest]
        public IEnumerator LateUpdate__AppliesHoverColor__When__ElementHovered()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Hover(element.Id);
            yield return null;

            var renderer = this._cube.GetComponent<Renderer>();
            var block    = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            var color = block.GetColor(BASE_COLOR);
            Assert.AreNotEqual(Color.clear, color, "PropertyBlock should have a color override");
        }

        [UnityTest]
        public IEnumerator LateUpdate__AppliesSelectColor__When__ElementSelected()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            var renderer = this._cube.GetComponent<Renderer>();
            var block    = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            var color = block.GetColor(BASE_COLOR);

            // Selection color is cyan (0, 1, 1, 1)
            Assert.AreEqual(0f, color.r, 0.01f);
            Assert.AreEqual(1f, color.g, 0.01f);
            Assert.AreEqual(1f, color.b, 0.01f);
        }

        [UnityTest]
        public IEnumerator LateUpdate__RemovesColor__When__ElementDeselected()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            this._highlighter.Selection.Deselect();
            yield return null;

            var renderer = this._cube.GetComponent<Renderer>();
            Assert.IsFalse(renderer.HasPropertyBlock(),
                           "PropertyBlock should be cleared after deselect");
        }

        [UnityTest]
        public IEnumerator LateUpdate__RemovesHoverColor__When__CursorLeavesElement()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Hover(element.Id);
            yield return null;

            this._highlighter.Selection.ClearHover();
            yield return null;

            var renderer = this._cube.GetComponent<Renderer>();
            Assert.IsFalse(renderer.HasPropertyBlock(),
                           "PropertyBlock should be cleared after hover ends");
        }

        [UnityTest]
        public IEnumerator LateUpdate__SelectOverridesHover__When__HoveredElementSelected()
        {
            yield return null;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;

            // Hover first
            this._highlighter.Selection.Hover(element.Id);
            yield return null;

            // Then select (hover clears automatically in Selection)
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            var renderer = this._cube.GetComponent<Renderer>();
            var block    = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            var color = block.GetColor(BASE_COLOR);

            // Should be selection cyan, not hover yellow
            Assert.AreEqual(0f, color.r, 0.01f, "Should be cyan (selection), not yellow (hover)");
            Assert.AreEqual(1f, color.g, 0.01f);
            Assert.AreEqual(1f, color.b, 0.01f);
        }

        [UnityTest]
        public IEnumerator LateUpdate__OriginalColorRestored__When__DeselectAfterSelect()
        {
            yield return null;

            var renderer      = this._cube.GetComponent<Renderer>();
            var originalColor = renderer.sharedMaterial.color;

            var element = this._cube.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            this._highlighter.Selection.Deselect();
            yield return null;

            // After deselect, shared material color should be unchanged
            Assert.AreEqual(originalColor, renderer.sharedMaterial.color,
                            "Shared material should not be modified by highlighting");
        }

        [UnityTest]
        public IEnumerator LateUpdate__TransfersHighlight__When__SelectionChangesRapidly()
        {
            yield return null;

            var cubeB = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cubeB.name = "TestSphere";
            cubeB.AddComponent<ElementBehaviour>();
            yield return null;

            var elementA = this._cube.GetComponent<ElementBehaviour>().Element;
            var elementB = cubeB.GetComponent<ElementBehaviour>().Element;

            // Select A then immediately select B in the same logical sequence
            this._highlighter.Selection.Select(elementA.Id);
            yield return null;

            this._highlighter.Selection.Select(elementB.Id);
            yield return null;

            // A should have no highlight, B should have selection color
            var rendererA = this._cube.GetComponent<Renderer>();
            var rendererB = cubeB.GetComponent<Renderer>();

            Assert.IsFalse(rendererA.HasPropertyBlock(),
                           "First element should have no highlight after selection transferred");

            var block = new MaterialPropertyBlock();
            rendererB.GetPropertyBlock(block);
            var color = block.GetColor(BASE_COLOR);
            Assert.AreEqual(0f, color.r, 0.01f, "Second element should have selection color (cyan)");

            Object.DestroyImmediate(cubeB);
        }

        [UnityTest]
        public IEnumerator LateUpdate__HandlesCompoundElement__When__MultipleRenderers()
        {
            yield return null;

            // Create a compound element: parent with ElementBehaviour, two child renderers
            var parent = new GameObject("Compound");
            parent.AddComponent<ElementBehaviour>();

            var childA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childA.transform.SetParent(parent.transform);

            var childB = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            childB.transform.SetParent(parent.transform);
            yield return null;

            var element = parent.GetComponent<ElementBehaviour>().Element;
            this._highlighter.Selection.Select(element.Id);
            yield return null;

            // Both child renderers should have the selection color
            var rendererA = childA.GetComponent<Renderer>();
            var rendererB = childB.GetComponent<Renderer>();
            var blockA    = new MaterialPropertyBlock();
            var blockB    = new MaterialPropertyBlock();
            rendererA.GetPropertyBlock(blockA);
            rendererB.GetPropertyBlock(blockB);

            Assert.AreNotEqual(Color.clear, blockA.GetColor(BASE_COLOR),
                               "Child A should have selection color");
            Assert.AreNotEqual(Color.clear, blockB.GetColor(BASE_COLOR),
                               "Child B should have selection color");

            Object.DestroyImmediate(parent);
        }

        [SetUp]
        public void SetUp()
        {
            this._cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            this._cube.name = "TestCube";
            this._cube.AddComponent<ElementBehaviour>();

            this._highlighterGo = new GameObject("Highlighter");
            this._highlighter   = this._highlighterGo.AddComponent<SelectionHighlighter>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(this._cube);
            Object.DestroyImmediate(this._highlighterGo);
        }
    }
}
