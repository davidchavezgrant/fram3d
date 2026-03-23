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
    /// Play Mode tests for CompositionGuideView. Verifies guide elements are created,
    /// visibility toggles work, and guides render within the unmasked area.
    /// </summary>
    public sealed class CompositionGuideViewTests
    {
        private CameraBehaviour      _behaviour;
        private GameObject           _cameraGo;
        private CompositionGuideView _guideView;
        private UIDocument           _uiDocument;
        private GameObject           _uiGo;

        // --- Settings accessible ---

        [UnityTest]
        public IEnumerator Settings__IsNotNull__When__Accessed()
        {
            yield return null;

            Assert.IsNotNull(this._guideView.Settings);
        }

        [SetUp]
        public void SetUp()
        {
            this._cameraGo   = new GameObject("TestCamera");
            this._behaviour  = this._cameraGo.AddComponent<CameraBehaviour>();
            this._uiGo       = new GameObject("TestGuides");
            this._uiDocument = this._uiGo.AddComponent<UIDocument>();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:PanelSettings");

            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                this._uiDocument.panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            }

            this._guideView = this._uiGo.AddComponent<CompositionGuideView>();
        }

        [UnityTest]
        public IEnumerator Start__AllElementsIgnoreInput__When__Initialized()
        {
            yield return null;
            yield return null;

            var container = this._uiDocument.rootVisualElement[0];

            for (var i = 0; i < container.childCount; i++)
                Assert.AreEqual(PickingMode.Ignore, container[i].pickingMode, $"Element {i} should ignore mouse events");
        }

        // --- Visibility: all hidden by default ---

        [UnityTest]
        public IEnumerator Start__AllGuidesHidden__When__Initialized()
        {
            yield return null;
            yield return null;

            Assert.IsFalse(this._guideView.Settings.AnyVisible);
        }

        // --- Structure ---

        [UnityTest]
        public IEnumerator Start__CreatesGuideElements__When__Initialized()
        {
            yield return null;
            yield return null;

            var root = this._uiDocument.rootVisualElement;
            Assert.IsNotNull(root);
            Assert.Greater(root.childCount, 0, "Container should be added to root");

            // 4 thirds lines + 2 cross lines + 2 safe zones = 8 elements
            var container = root[0];
            Assert.AreEqual(8, container.childCount, "Container should have 8 guide elements");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(this._uiGo);
            Object.DestroyImmediate(this._cameraGo);
        }

        [UnityTest]
        public IEnumerator ToggleAll__HidesAllGuides__When__SomeVisible()
        {
            yield return null;

            this._guideView.Settings.ToggleThirds();
            this._guideView.Settings.ToggleCenterCross();
            yield return null;

            this._guideView.Settings.ToggleAll(); // hide all
            yield return null;

            var container = this._uiDocument.rootVisualElement[0];

            for (var i = 0; i < container.childCount; i++)
                Assert.AreEqual(DisplayStyle.None, container[i].style.display.value, $"Element {i} should be hidden after ToggleAll");
        }

        // --- ToggleAll ---

        [UnityTest]
        public IEnumerator ToggleAll__ShowsAllGuides__When__NoneVisible()
        {
            yield return null;

            this._guideView.Settings.ToggleAll();
            yield return null;

            var container = this._uiDocument.rootVisualElement[0];

            for (var i = 0; i < container.childCount; i++)
                Assert.AreEqual(DisplayStyle.Flex, container[i].style.display.value, $"Element {i} should be visible after ToggleAll");
        }

        // --- Toggle center cross ---

        [UnityTest]
        public IEnumerator ToggleCenterCross__ShowsCrossElements__When__Enabled()
        {
            yield return null;

            this._guideView.Settings.ToggleCenterCross();
            yield return null;

            var container = this._uiDocument.rootVisualElement[0];

            // Elements 4-5 are cross
            Assert.AreEqual(DisplayStyle.Flex, container[4].style.display.value, "Cross H should be visible");
            Assert.AreEqual(DisplayStyle.Flex, container[5].style.display.value, "Cross V should be visible");
        }

        // --- Toggle safe zones ---

        [UnityTest]
        public IEnumerator ToggleSafeZones__ShowsSafeZoneElements__When__Enabled()
        {
            yield return null;

            this._guideView.Settings.ToggleSafeZones();
            yield return null;

            var container = this._uiDocument.rootVisualElement[0];

            // Elements 6-7 are safe zones
            Assert.AreEqual(DisplayStyle.Flex, container[6].style.display.value, "Title safe should be visible");
            Assert.AreEqual(DisplayStyle.Flex, container[7].style.display.value, "Action safe should be visible");
        }

        [UnityTest]
        public IEnumerator ToggleThirds__HidesCrossAndSafeZones__When__OnlyThirdsEnabled()
        {
            yield return null;

            this._guideView.Settings.ToggleThirds();
            yield return null;

            var container = this._uiDocument.rootVisualElement[0];

            // Elements 4-5 are cross, 6-7 are safe zones
            Assert.AreEqual(DisplayStyle.None, container[4].style.display.value, "Cross H should be hidden");
            Assert.AreEqual(DisplayStyle.None, container[5].style.display.value, "Cross V should be hidden");
            Assert.AreEqual(DisplayStyle.None, container[6].style.display.value, "Title safe should be hidden");
            Assert.AreEqual(DisplayStyle.None, container[7].style.display.value, "Action safe should be hidden");
        }

        // --- Toggle thirds ---

        [UnityTest]
        public IEnumerator ToggleThirds__ShowsThirdsElements__When__Enabled()
        {
            yield return null;

            this._guideView.Settings.ToggleThirds();
            yield return null;

            var container = this._uiDocument.rootVisualElement[0];

            // First 4 elements are thirds lines
            for (var i = 0; i < 4; i++)
                Assert.AreEqual(DisplayStyle.Flex, container[i].style.display.value, $"Thirds line {i} should be visible");
        }
    }
}