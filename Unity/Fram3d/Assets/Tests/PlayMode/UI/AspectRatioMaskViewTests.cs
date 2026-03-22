using System.Collections;
using Fram3d.Engine.Integration;
using Fram3d.UI.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Play Mode tests for AspectRatioMaskView. Bar dimension tests require a
    /// visible Game view (UI Toolkit layout doesn't resolve without a rendering
    /// surface), so we test structure only. ComputeUnmaskedRect — the math that
    /// determines bar positions — is thoroughly tested in Core (14 tests).
    /// </summary>
    public sealed class AspectRatioMaskViewTests
    {
        private CameraBehaviour     _behaviour;
        private GameObject          _cameraGo;
        private AspectRatioMaskView _maskView;
        private GameObject          _uiGo;
        private UIDocument          _uiDocument;

        [SetUp]
        public void SetUp()
        {
            this._cameraGo = new GameObject("TestCamera");
            this._behaviour = this._cameraGo.AddComponent<CameraBehaviour>();

            this._uiGo      = new GameObject("TestUI");
            this._uiDocument = this._uiGo.AddComponent<UIDocument>();

            var guids = UnityEditor.AssetDatabase.FindAssets("t:PanelSettings");

            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                this._uiDocument.panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            }

            this._maskView = this._uiGo.AddComponent<AspectRatioMaskView>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(this._uiGo);
            Object.Destroy(this._cameraGo);
        }

        [UnityTest]
        public IEnumerator Start__CreatesFourBars__When__Initialized()
        {
            yield return null;
            yield return null;

            var root = this._uiDocument.rootVisualElement;
            Assert.IsNotNull(root);
            Assert.Greater(root.childCount, 0, "Overlay container should be added to root");

            var container = root[0];
            Assert.AreEqual(4, container.childCount, "Container should have 4 bar elements");
        }

        [UnityTest]
        public IEnumerator Start__AllBarsAreAbsolutePositioned__When__Initialized()
        {
            yield return null;
            yield return null;

            var root      = this._uiDocument.rootVisualElement;
            var container = root[0];

            for (var i = 0; i < container.childCount; i++)
            {
                var bar = container[i];
                Assert.AreEqual(Position.Absolute, bar.style.position.value,
                    $"Bar {i} should be absolutely positioned");
            }
        }

        [UnityTest]
        public IEnumerator Start__AllBarsIgnoreInput__When__Initialized()
        {
            yield return null;
            yield return null;

            var root      = this._uiDocument.rootVisualElement;
            var container = root[0];

            for (var i = 0; i < container.childCount; i++)
            {
                var bar = container[i];
                Assert.AreEqual(PickingMode.Ignore, bar.pickingMode,
                    $"Bar {i} should ignore mouse events");
            }
        }

        [UnityTest]
        public IEnumerator Start__FindsCameraBehaviour__When__InScene()
        {
            yield return null;

            // If _cameraBehaviour is null, UpdateBars early-returns and bars are never computed.
            // We can't access the private field, but we can verify bars exist (which requires
            // Start to have found the CameraBehaviour and called BuildOverlay).
            var root = this._uiDocument.rootVisualElement;
            Assert.Greater(root.childCount, 0, "BuildOverlay should have run (CameraBehaviour found)");
        }
    }
}
