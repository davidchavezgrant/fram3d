using System.Collections;
using Fram3d.Engine.Integration;
using Fram3d.UI.Timeline;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Tests for TimelineSectionView — the main timeline section MonoBehaviour.
    /// Verifies toggle behavior, visibility tracking, and initialization.
    /// </summary>
    public sealed class TimelineSectionViewTests
    {
        private CameraBehaviour     _behaviour;
        private GameObject          _cameraGo;
        private TimelineSectionView _timeline;
        private UIDocument          _uiDocument;
        private GameObject          _uiGo;
        private GameObject          _shotEvalGo;

        [SetUp]
        public void SetUp()
        {
            foreach (var f in Object.FindObjectsByType<FrustumWireframe>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(f.gameObject);
            }

            foreach (var t in Object.FindObjectsByType<TimelineSectionView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(t.gameObject);
            }

            foreach (var s in Object.FindObjectsByType<ShotEvaluator>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(s.gameObject);
            }

            this._cameraGo  = new GameObject("TestCamera");
            this._behaviour = this._cameraGo.AddComponent<CameraBehaviour>();

            // ShotEvaluator creates Timeline in Awake, wires CameraBehaviour in Start
            this._shotEvalGo = new GameObject("TestShotEval");
            this._shotEvalGo.AddComponent<ShotEvaluator>();

            this._uiGo       = new GameObject("TestTimeline");
            this._uiDocument = this._uiGo.AddComponent<UIDocument>();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:PanelSettings");

            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                this._uiDocument.panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            }

            this._timeline = this._uiGo.AddComponent<TimelineSectionView>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(this._uiGo);
            Object.DestroyImmediate(this._shotEvalGo);

            var frustums = Object.FindObjectsByType<FrustumWireframe>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var f in frustums)
            {
                Object.DestroyImmediate(f.gameObject);
            }

            Object.DestroyImmediate(this._cameraGo);
        }

        [Test]
        public void IsVisible__ReturnsTrue__When__Initialized()
        {
            Assert.IsTrue(this._timeline.IsVisible, "Timeline should be visible by default");
        }

        [Test]
        public void Toggle__SetsVisibleFalse__When__CalledOnce()
        {
            this._timeline.Toggle();

            Assert.IsFalse(this._timeline.IsVisible);
        }

        [Test]
        public void Toggle__SetsVisibleTrue__When__CalledTwice()
        {
            this._timeline.Toggle();
            this._timeline.Toggle();

            Assert.IsTrue(this._timeline.IsVisible);
        }

        [Test]
        public void HasFocusedTextField__ReturnsFalse__When__NoFieldFocused()
        {
            Assert.IsFalse(this._timeline.HasFocusedTextField);
        }

        [UnityTest]
        public IEnumerator Start__BuildsLayout__When__ShotEvaluatorExists()
        {
            yield return null; // Awake
            yield return null; // Start

            var root = this._uiDocument.rootVisualElement;
            Assert.IsNotNull(root);
            Assert.Greater(root.childCount, 0,
                "Timeline should build its layout when ShotEvaluator is present");
        }

        [UnityTest]
        public IEnumerator Toggle__HidesSection__When__AfterStartCalled()
        {
            yield return null;
            yield return null;

            this._timeline.Toggle();

            // Find the section element by class
            var root    = this._uiDocument.rootVisualElement;
            var section = FindByClass(root, "timeline-section");

            if (section != null)
            {
                Assert.AreEqual(DisplayStyle.None, section.style.display.value,
                    "Section should be hidden after Toggle");
            }

            Assert.IsFalse(this._timeline.IsVisible);
        }

        [UnityTest]
        public IEnumerator Toggle__ShowsSection__When__ToggledTwiceAfterStart()
        {
            yield return null;
            yield return null;

            this._timeline.Toggle(); // hide
            this._timeline.Toggle(); // show

            var root    = this._uiDocument.rootVisualElement;
            var section = FindByClass(root, "timeline-section");

            if (section != null)
            {
                Assert.AreEqual(DisplayStyle.Flex, section.style.display.value,
                    "Section should be visible after double toggle");
            }

            Assert.IsTrue(this._timeline.IsVisible);
        }

        private static VisualElement FindByClass(VisualElement parent, string className)
        {
            if (parent.ClassListContains(className))
            {
                return parent;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var result = FindByClass(parent[i], className);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
