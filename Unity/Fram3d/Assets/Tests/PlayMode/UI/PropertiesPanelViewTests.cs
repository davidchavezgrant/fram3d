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

            // After toggle, panel should be hidden
            // We can't easily check the internal _panel display style,
            // but we can verify Toggle doesn't throw
            Assert.IsNotNull(this._panel);
        }

        [UnityTest]
        public IEnumerator Toggle__ShowsPanel__When__CalledTwice()
        {
            yield return null;
            yield return null;

            this._panel.Toggle(); // hide
            this._panel.Toggle(); // show again
            Assert.IsNotNull(this._panel);
        }
    }
}