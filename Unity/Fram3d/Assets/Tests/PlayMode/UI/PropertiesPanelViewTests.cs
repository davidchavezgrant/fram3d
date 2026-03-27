using System.Collections;
using Fram3d.Engine.Integration;
using Fram3d.UI.Panels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Play Mode tests for PropertiesPanelView. Focuses on initialization
    /// and structural integrity — catches NullReferenceExceptions from
    /// bad section wiring without testing visual styling.
    /// </summary>
    public sealed class PropertiesPanelViewTests
    {
        private CameraBehaviour     _behaviour;
        private GameObject          _cameraGo;
        private PropertiesPanelView _panel;
        private UIDocument          _uiDocument;
        private GameObject          _uiGo;

        [SetUp]
        public void SetUp()
        {
            this._cameraGo   = new GameObject("TestCamera");
            this._behaviour  = this._cameraGo.AddComponent<CameraBehaviour>();
            this._uiGo       = new GameObject("TestPanel");
            this._uiDocument = this._uiGo.AddComponent<UIDocument>();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:PanelSettings");

            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                this._uiDocument.panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            }

            this._panel = this._uiGo.AddComponent<PropertiesPanelView>();
        }

        [UnityTest]
        public IEnumerator Start__BuildsPanelWithoutError__When__Created()
        {
            // If any section wiring throws, this test fails with the exception
            yield return null;
            yield return null;

            // Panel should have built content into the root
            var root = this._uiDocument.rootVisualElement;
            Assert.IsNotNull(root);
            Assert.Greater(root.childCount, 0, "Panel should have added elements to the root");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(this._uiGo);

            var frustums = Object.FindObjectsByType<FrustumWireframe>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var f in frustums)
            {
                Object.DestroyImmediate(f.gameObject);
            }

            Object.DestroyImmediate(this._cameraGo);
        }

        [UnityTest]
        public IEnumerator Toggle__HidesPanel__When__CalledOnce()
        {
            yield return null;
            yield return null;

            this._panel.Toggle();

            Assert.IsFalse(this._panel.IsVisible, "Panel should be hidden after single toggle");
        }

        [UnityTest]
        public IEnumerator Toggle__ShowsPanel__When__CalledTwice()
        {
            yield return null;
            yield return null;

            this._panel.Toggle(); // hide
            this._panel.Toggle(); // show again

            Assert.IsTrue(this._panel.IsVisible, "Panel should be visible after double toggle");
        }

        [Test]
        public void IsVisible__ReturnsTrue__When__Initialized()
        {
            Assert.IsTrue(this._panel.IsVisible, "Panel should be visible by default");
        }

        [Test]
        public void HasFocusedTextField__ReturnsFalse__When__NoFieldFocused()
        {
            Assert.IsFalse(this._panel.HasFocusedTextField,
                "No field should have focus before initialization");
        }

        [Test]
        public void PanelWidth__ReturnsExpectedValue__When__Accessed()
        {
            Assert.AreEqual(440f, this._panel.PanelWidth,
                "Panel width should be 440px");
        }

        [UnityTest]
        public IEnumerator Toggle__SetsRightInsetToZero__When__Hidden()
        {
            yield return null;
            yield return null;

            this._panel.Toggle(); // hide

            // CameraBehaviour should have right inset of 0 when panel is hidden
            Assert.AreEqual(0f, this._behaviour.RightInsetPixels, 0.01f,
                "Right inset should be 0 when panel is hidden");
        }

        [UnityTest]
        public IEnumerator Toggle__RestoresRightInset__When__Shown()
        {
            yield return null;
            yield return null;

            this._panel.Toggle(); // hide
            this._panel.Toggle(); // show

            // After showing, right inset should be non-zero (PANEL_WIDTH converted to screen)
            // The exact value depends on screen/CSS scaling, so just verify it's positive
            Assert.Greater(this._behaviour.RightInsetPixels, 0f,
                "Right inset should be positive when panel is visible");
        }

        [UnityTest]
        public IEnumerator Start__BuildsPanelWithHeader__When__Created()
        {
            yield return null;
            yield return null;

            var root = this._uiDocument.rootVisualElement;

            // Find the properties-panel element
            VisualElement panel = null;

            for (var i = 0; i < root.childCount; i++)
            {
                if (root[i].ClassListContains("properties-panel"))
                {
                    panel = root[i];
                    break;
                }
            }

            Assert.IsNotNull(panel, "Should find properties-panel element");
            Assert.Greater(panel.childCount, 1,
                "Panel should have header + content sections");
        }
    }
}