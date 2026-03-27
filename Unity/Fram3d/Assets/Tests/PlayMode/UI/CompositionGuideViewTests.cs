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

            // 3 guide groups: ThirdsGuide (4 lines), CenterCrossGuide (2 lines), SafeZoneGuide (2 zones)
            var container = root[0];
            Assert.AreEqual(3, container.childCount, "Container should have 3 guide groups");
            Assert.AreEqual(4, container[0].childCount, "ThirdsGuide should have 4 lines");
            Assert.AreEqual(2, container[1].childCount, "CenterCrossGuide should have 2 lines");
            Assert.AreEqual(2, container[2].childCount, "SafeZoneGuide should have 2 zones");
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
        public IEnumerator ToggleAll__HidesAllGuides__When__SomeVisible()
        {
            yield return null;

            this._guideView.Settings.ToggleThirds();
            this._guideView.Settings.ToggleCenterCross();
            yield return null;

            this._guideView.Settings.ToggleAll(); // hide all
            yield return null;

            var container = this._uiDocument.rootVisualElement[0];

            for (var g = 0; g < container.childCount; g++)
            {
                for (var i = 0; i < container[g].childCount; i++)
                {
                    Assert.AreEqual(DisplayStyle.None, container[g][i].style.display.value,
                        $"Group {g} element {i} should be hidden after ToggleAll");
                }
            }
        }

        // --- ToggleAll ---

        [UnityTest]
        public IEnumerator ToggleAll__ShowsAllGuides__When__NoneVisible()
        {
            yield return null;
            yield return null;

            this._guideView.Settings.ToggleAll();
            yield return null;

            var container = this._uiDocument.rootVisualElement[0];

            for (var g = 0; g < container.childCount; g++)
            {
                for (var i = 0; i < container[g].childCount; i++)
                {
                    Assert.AreEqual(DisplayStyle.Flex, container[g][i].style.display.value,
                        $"Group {g} element {i} should be visible after ToggleAll");
                }
            }
        }

        // --- Toggle center cross ---

        [UnityTest]
        public IEnumerator ToggleCenterCross__ShowsCrossElements__When__Enabled()
        {
            yield return null;
            yield return null;

            this._guideView.Settings.ToggleCenterCross();
            yield return null;

            var cross = this._uiDocument.rootVisualElement[0][1]; // CenterCrossGuide

            Assert.AreEqual(DisplayStyle.Flex, cross[0].style.display.value, "Cross H should be visible");
            Assert.AreEqual(DisplayStyle.Flex, cross[1].style.display.value, "Cross V should be visible");
        }

        // --- Toggle safe zones ---

        [UnityTest]
        public IEnumerator ToggleSafeZones__ShowsSafeZoneElements__When__Enabled()
        {
            yield return null;
            yield return null;

            this._guideView.Settings.ToggleSafeZones();
            yield return null;

            var safeZones = this._uiDocument.rootVisualElement[0][2]; // SafeZoneGuide

            Assert.AreEqual(DisplayStyle.Flex, safeZones[0].style.display.value, "Title safe should be visible");
            Assert.AreEqual(DisplayStyle.Flex, safeZones[1].style.display.value, "Action safe should be visible");
        }

        [UnityTest]
        public IEnumerator ToggleThirds__HidesCrossAndSafeZones__When__OnlyThirdsEnabled()
        {
            yield return null;
            yield return null;

            this._guideView.Settings.ToggleThirds();
            yield return null;

            var cross     = this._uiDocument.rootVisualElement[0][1]; // CenterCrossGuide
            var safeZones = this._uiDocument.rootVisualElement[0][2]; // SafeZoneGuide

            Assert.AreEqual(DisplayStyle.None, cross[0].style.display.value, "Cross H should be hidden");
            Assert.AreEqual(DisplayStyle.None, cross[1].style.display.value, "Cross V should be hidden");
            Assert.AreEqual(DisplayStyle.None, safeZones[0].style.display.value, "Title safe should be hidden");
            Assert.AreEqual(DisplayStyle.None, safeZones[1].style.display.value, "Action safe should be hidden");
        }

        // --- Toggle thirds ---

        [UnityTest]
        public IEnumerator ToggleThirds__ShowsThirdsElements__When__Enabled()
        {
            yield return null;
            yield return null;

            this._guideView.Settings.ToggleThirds();
            yield return null;

            var thirds = this._uiDocument.rootVisualElement[0][0]; // ThirdsGuide

            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(DisplayStyle.Flex, thirds[i].style.display.value, $"Thirds line {i} should be visible");
            }
        }
    }
}