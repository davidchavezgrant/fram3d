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
