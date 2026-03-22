using System.Collections;
using Fram3d.Core.Camera;
using Fram3d.Engine.Integration;
using Fram3d.UI.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Play Mode integration tests for AspectRatioMaskView. Verifies that
    /// the four mask bars are positioned correctly for different aspect ratios.
    /// ComputeUnmaskedRect logic is tested in Core — these tests verify the
    /// wiring from Core results to UI Toolkit style properties.
    /// </summary>
    public sealed class AspectRatioMaskViewTests
    {
        private CameraBehaviour    _behaviour;
        private GameObject         _cameraGo;
        private AspectRatioMaskView _maskView;
        private GameObject         _uiGo;
        private UIDocument         _uiDocument;

        [SetUp]
        public void SetUp()
        {
            this._cameraGo = new GameObject("TestCamera");
            this._behaviour = this._cameraGo.AddComponent<CameraBehaviour>();

            this._uiGo      = new GameObject("TestUI");
            this._uiDocument = this._uiGo.AddComponent<UIDocument>();

            var panelSettings = Resources.Load<PanelSettings>("PanelSettings");

            if (panelSettings == null)
            {
                // Fall back to finding it in the project
                var guids = UnityEditor.AssetDatabase.FindAssets("t:PanelSettings");

                if (guids.Length > 0)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
                }
            }

            this._uiDocument.panelSettings = panelSettings;

            // Force a known size on the root so layout resolves to non-zero dimensions.
            // Without this, the container resolves to 0×0 in the test environment.
            var root = this._uiDocument.rootVisualElement;
            root.style.width  = 1920;
            root.style.height = 1080;

            this._maskView = this._uiGo.AddComponent<AspectRatioMaskView>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(this._uiGo);
            Object.Destroy(this._cameraGo);
        }

        // --- Bar structure ---

        [UnityTest]
        public IEnumerator UpdateBars__CreatesFourBars__When__Started()
        {
            yield return null;
            yield return null;

            var root = this._uiDocument.rootVisualElement;
            Assert.IsNotNull(root);

            // Container is inserted at index 0, with 4 bar children
            var container = root[0];
            Assert.AreEqual(4, container.childCount);
        }

        // --- Pillarbox (4:3 on wider screen) ---

        [UnityTest]
        public IEnumerator UpdateBars__LeftAndRightBarsVisible__When__PillarboxRatio()
        {
            yield return WaitForLayout();

            if (!HasLayout())
                Assert.Inconclusive("UI layout did not resolve in test environment");

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_4_3)
                this._behaviour.CycleAspectRatioForward();

            yield return null;

            var bars = GetBars();
            Assert.IsNotNull(bars);

            // Check style values set by UpdateBars (not resolvedStyle which is layout-dependent)
            Assert.Greater(StyleValue(bars.Value.left.style.width),  0f, "Left bar should have width for pillarbox");
            Assert.Greater(StyleValue(bars.Value.right.style.width), 0f, "Right bar should have width for pillarbox");
            Assert.AreEqual(0f, StyleValue(bars.Value.top.style.height), 0.5f, "Top bar should have no height for pillarbox");
        }

        // --- Letterbox (2.39:1 on 16:9 screen) ---

        [UnityTest]
        public IEnumerator UpdateBars__TopAndBottomBarsVisible__When__LetterboxRatio()
        {
            yield return WaitForLayout();

            if (!HasLayout())
                Assert.Inconclusive("UI layout did not resolve in test environment");

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_239_1)
                this._behaviour.CycleAspectRatioForward();

            yield return null;

            var bars = GetBars();
            Assert.IsNotNull(bars);

            Assert.Greater(StyleValue(bars.Value.top.style.height),    0f, "Top bar should have height for letterbox");
            Assert.Greater(StyleValue(bars.Value.bottom.style.height), 0f, "Bottom bar should have height for letterbox");
            Assert.AreEqual(0f, StyleValue(bars.Value.left.style.width), 0.5f, "Left bar should have no width for letterbox");
        }

        // --- Bars are centered ---

        [UnityTest]
        public IEnumerator UpdateBars__BarsAreCentered__When__PillarboxRatio()
        {
            yield return WaitForLayout();

            if (!HasLayout())
                Assert.Inconclusive("UI layout did not resolve in test environment");

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_4_3)
                this._behaviour.CycleAspectRatioForward();

            yield return null;

            var bars = GetBars();
            Assert.IsNotNull(bars);

            var leftWidth  = StyleValue(bars.Value.left.style.width);
            var rightWidth = StyleValue(bars.Value.right.style.width);
            Assert.AreEqual(leftWidth, rightWidth, 1f, "Pillarbox bars should be centered");
        }

        [UnityTest]
        public IEnumerator UpdateBars__BarsAreCentered__When__LetterboxRatio()
        {
            yield return WaitForLayout();

            if (!HasLayout())
                Assert.Inconclusive("UI layout did not resolve in test environment");

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_239_1)
                this._behaviour.CycleAspectRatioForward();

            yield return null;

            var bars = GetBars();
            Assert.IsNotNull(bars);

            var topHeight    = StyleValue(bars.Value.top.style.height);
            var bottomHeight = StyleValue(bars.Value.bottom.style.height);
            Assert.AreEqual(topHeight, bottomHeight, 1f, "Letterbox bars should be centered");
        }

        // --- Ratio switching (no stale bar state) ---

        [UnityTest]
        public IEnumerator UpdateBars__TransitionsCorrectly__When__SwitchingFromPillarboxToLetterbox()
        {
            yield return WaitForLayout();

            if (!HasLayout())
                Assert.Inconclusive("UI layout did not resolve in test environment");

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_4_3)
                this._behaviour.CycleAspectRatioForward();

            yield return null;

            var bars = GetBars();
            Assert.IsNotNull(bars);
            Assert.Greater(StyleValue(bars.Value.left.style.width), 0f, "Should start with pillarbox");

            // Switch to 2.39:1 → letterbox
            while (cam.ActiveAspectRatio != AspectRatio.RATIO_239_1)
                this._behaviour.CycleAspectRatioForward();

            yield return null;

            Assert.AreEqual(0f, StyleValue(bars.Value.left.style.width), 0.5f, "Left bar should be zero after switching to letterbox");
            Assert.Greater(StyleValue(bars.Value.top.style.height), 0f, "Top bar should have height after switching to letterbox");
        }

        // --- Helpers ---

        /// <summary>
        /// Extracts the float value from a StyleLength. Used instead of resolvedStyle
        /// because absolutely positioned bar elements don't reliably resolve in the
        /// test runner's layout engine.
        /// </summary>
        private static float StyleValue(StyleLength s) =>
            s.keyword == StyleKeyword.Undefined ? 0f : s.value.value;

        /// <summary>
        /// Waits until the overlay container has resolved to non-zero dimensions,
        /// or returns false after timeout. UI Toolkit layout needs a variable number
        /// of frames to resolve in the test runner.
        /// </summary>
        private IEnumerator WaitForLayout()
        {
            for (var i = 0; i < 30; i++)
            {
                yield return null;

                var root = this._uiDocument.rootVisualElement;

                if (root == null || root.childCount == 0)
                    continue;

                var container = root[0];
                var w         = container.resolvedStyle.width;
                var h         = container.resolvedStyle.height;

                if (!float.IsNaN(w) && w > 0 && !float.IsNaN(h) && h > 0)
                    yield break;
            }
        }

        private bool HasLayout()
        {
            var root = this._uiDocument.rootVisualElement;

            if (root == null || root.childCount == 0)
                return false;

            var w = root[0].resolvedStyle.width;
            return !float.IsNaN(w) && w > 0;
        }

        private (VisualElement top, VisualElement bottom, VisualElement left, VisualElement right)? GetBars()
        {
            var root = this._uiDocument.rootVisualElement;

            if (root == null || root.childCount == 0)
                return null;

            var container = root[0];

            if (container.childCount < 4)
                return null;

            return (container[0], container[1], container[2], container[3]);
        }
    }
}
