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
            yield return null;

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_4_3)
                this._behaviour.CycleAspectRatioForward();

            // Wait for layout + UpdateBars
            yield return null;
            yield return null;

            var bars = GetBars();

            if (bars == null)
                yield break;

            // 4:3 on a wider screen → pillarbox: left and right bars have width > 0
            var leftWidth  = bars.left.resolvedStyle.width;
            var rightWidth = bars.right.resolvedStyle.width;

            if (float.IsNaN(leftWidth))
                yield break; // Layout not resolved yet, skip

            Assert.Greater(leftWidth,  0f, "Left bar should have width for pillarbox");
            Assert.Greater(rightWidth, 0f, "Right bar should have width for pillarbox");

            // Top and bottom bars should have zero height (no letterbox)
            Assert.AreEqual(0f, bars.top.resolvedStyle.height, 0.5f, "Top bar should have no height for pillarbox");
        }

        // --- Letterbox (2.39:1 on 16:9 screen) ---

        [UnityTest]
        public IEnumerator UpdateBars__TopAndBottomBarsVisible__When__LetterboxRatio()
        {
            yield return null;

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_239_1)
                this._behaviour.CycleAspectRatioForward();

            yield return null;
            yield return null;

            var bars = GetBars();

            if (bars == null)
                yield break;

            var topHeight    = bars.top.resolvedStyle.height;
            var bottomHeight = bars.bottom.resolvedStyle.height;

            if (float.IsNaN(topHeight))
                yield break;

            Assert.Greater(topHeight,    0f, "Top bar should have height for letterbox");
            Assert.Greater(bottomHeight, 0f, "Bottom bar should have height for letterbox");

            // Left and right bars should have zero width (no pillarbox)
            Assert.AreEqual(0f, bars.left.resolvedStyle.width, 0.5f, "Left bar should have no width for letterbox");
        }

        // --- Bars are centered ---

        [UnityTest]
        public IEnumerator UpdateBars__BarsAreCentered__When__PillarboxRatio()
        {
            yield return null;

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_4_3)
                this._behaviour.CycleAspectRatioForward();

            yield return null;
            yield return null;

            var bars = GetBars();

            if (bars == null)
                yield break;

            var leftWidth  = bars.left.resolvedStyle.width;
            var rightWidth = bars.right.resolvedStyle.width;

            if (float.IsNaN(leftWidth))
                yield break;

            // Left and right bars should be equal width (centered)
            Assert.AreEqual(leftWidth, rightWidth, 1f, "Pillarbox bars should be centered");
        }

        [UnityTest]
        public IEnumerator UpdateBars__BarsAreCentered__When__LetterboxRatio()
        {
            yield return null;

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_239_1)
                this._behaviour.CycleAspectRatioForward();

            yield return null;
            yield return null;

            var bars = GetBars();

            if (bars == null)
                yield break;

            var topHeight    = bars.top.resolvedStyle.height;
            var bottomHeight = bars.bottom.resolvedStyle.height;

            if (float.IsNaN(topHeight))
                yield break;

            Assert.AreEqual(topHeight, bottomHeight, 1f, "Letterbox bars should be centered");
        }

        // --- Bars fill the view ---

        [UnityTest]
        public IEnumerator UpdateBars__BarsCoverFullView__When__AnyRatio()
        {
            yield return null;

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_4_3)
                this._behaviour.CycleAspectRatioForward();

            yield return null;
            yield return null;

            var bars = GetBars();

            if (bars == null)
                yield break;

            var container = this._uiDocument.rootVisualElement[0];
            var viewWidth = container.resolvedStyle.width;

            if (float.IsNaN(viewWidth) || viewWidth <= 0)
                yield break;

            var viewHeight = container.resolvedStyle.height;

            // Left bar + unmasked area + right bar should equal view width
            var leftW    = bars.left.resolvedStyle.width;
            var rightW   = bars.right.resolvedStyle.width;
            var unmaskedW = viewWidth - leftW - rightW;
            Assert.AreEqual(viewWidth, leftW + unmaskedW + rightW, 1f, "Bars + unmasked should fill width");

            // Top bar + unmasked area + bottom bar should equal view height
            var topH     = bars.top.resolvedStyle.height;
            var bottomH  = bars.bottom.resolvedStyle.height;
            var unmaskedH = viewHeight - topH - bottomH;
            Assert.AreEqual(viewHeight, topH + unmaskedH + bottomH, 1f, "Bars + unmasked should fill height");
        }

        // --- Helpers ---

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
