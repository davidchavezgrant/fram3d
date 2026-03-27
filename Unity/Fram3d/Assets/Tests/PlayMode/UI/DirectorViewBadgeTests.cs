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
    /// Tests for DirectorViewBadge — the "DIRECTOR VIEW" badge that appears
    /// at top-center when Director View is active.
    /// </summary>
    public sealed class DirectorViewBadgeTests
    {
        private CameraBehaviour  _behaviour;
        private GameObject       _cameraGo;
        private DirectorViewBadge _badge;
        private UIDocument       _uiDocument;
        private GameObject       _uiGo;

        [SetUp]
        public void SetUp()
        {
            foreach (var f in Object.FindObjectsByType<FrustumWireframe>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(f.gameObject);
            }

            this._cameraGo  = new GameObject("TestCamera");
            this._behaviour = this._cameraGo.AddComponent<CameraBehaviour>();

            this._uiGo       = new GameObject("TestBadge");
            this._uiDocument = this._uiGo.AddComponent<UIDocument>();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:PanelSettings");

            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                this._uiDocument.panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            }

            this._badge = this._uiGo.AddComponent<DirectorViewBadge>();
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
        public IEnumerator Badge__IsHidden__When__NotInDirectorView()
        {
            yield return null; // Awake
            yield return null; // Start

            Assert.IsFalse(this._behaviour.IsDirectorView, "Precondition: not in Director View");

            // Wait for Update to run
            yield return null;

            var root  = this._uiDocument.rootVisualElement;
            var badge = FindBadge(root);
            Assert.IsNotNull(badge, "Badge element should exist");
            Assert.AreEqual(DisplayStyle.None, badge.style.display.value,
                "Badge should be hidden when not in Director View");
        }

        [UnityTest]
        public IEnumerator Badge__IsVisible__When__DirectorViewEnabled()
        {
            yield return null; // Awake
            yield return null; // Start

            this._behaviour.ToggleDirectorView();
            Assert.IsTrue(this._behaviour.IsDirectorView, "Precondition");

            yield return null; // Update

            var root  = this._uiDocument.rootVisualElement;
            var badge = FindBadge(root);
            Assert.IsNotNull(badge, "Badge element should exist");
            Assert.AreEqual(DisplayStyle.Flex, badge.style.display.value,
                "Badge should be visible when in Director View");
        }

        [UnityTest]
        public IEnumerator Badge__HidesAgain__When__DirectorViewDisabled()
        {
            yield return null;
            yield return null;

            this._behaviour.ToggleDirectorView();
            yield return null;

            this._behaviour.ToggleDirectorView();
            Assert.IsFalse(this._behaviour.IsDirectorView, "Precondition: back to Camera View");
            yield return null;

            var root  = this._uiDocument.rootVisualElement;
            var badge = FindBadge(root);
            Assert.AreEqual(DisplayStyle.None, badge.style.display.value,
                "Badge should hide when Director View is disabled");
        }

        [UnityTest]
        public IEnumerator Badge__ContainsLabel__When__Built()
        {
            yield return null;
            yield return null;

            var root  = this._uiDocument.rootVisualElement;
            var badge = FindBadge(root);
            Assert.IsNotNull(badge, "Badge should exist");
            Assert.Greater(badge.childCount, 0, "Badge should have a label child");

            var label = badge[0] as Label;
            Assert.IsNotNull(label, "Badge child should be a Label");
            Assert.AreEqual("DIRECTOR VIEW", label.text);
        }

        [UnityTest]
        public IEnumerator Badge__IgnoresPointerInput__When__Built()
        {
            yield return null;
            yield return null;

            var root  = this._uiDocument.rootVisualElement;
            var badge = FindBadge(root);
            Assert.AreEqual(PickingMode.Ignore, badge.pickingMode,
                "Badge should not intercept pointer events");
        }

        private static VisualElement FindBadge(VisualElement root)
        {
            for (var i = 0; i < root.childCount; i++)
            {
                if (root[i].ClassListContains("director-badge"))
                {
                    return root[i];
                }
            }

            return null;
        }
    }
}
