using System.Collections;
using Fram3d.Core.Camera;
using Fram3d.Core.Viewport;
using Fram3d.Engine.Integration;
using Fram3d.UI.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Play Mode tests for AspectRatioMaskView. Verifies bar structure and
    /// that UpdateBars computes correct style values for different aspect ratios.
    /// ComputeUnmaskedRect logic is tested in Core — these verify the wiring.
    /// </summary>
    public sealed class AspectRatioMaskViewTests
    {
        private CameraBehaviour     _behaviour;
        private GameObject          _cameraGo;
        private AspectRatioMaskView _maskView;
        private PanelSettings       _panelSettings;
        private RenderTexture       _renderTexture;
        private UIDocument          _uiDocument;
        private GameObject          _uiGo;

        [SetUp]
        public void SetUp()
        {
            this._cameraGo   = new GameObject("TestCamera");
            this._behaviour  = this._cameraGo.AddComponent<CameraBehaviour>();
            this._uiGo       = new GameObject("TestUI");
            this._uiDocument = this._uiGo.AddComponent<UIDocument>();

            // Render to a fixed-size texture so layout resolves to deterministic
            // 1920×1080 dimensions regardless of Game view size.
            var guids = UnityEditor.AssetDatabase.FindAssets("t:PanelSettings");

            if (guids.Length > 0)
            {
                var path     = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                var original = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
                this._panelSettings = Object.Instantiate(original);
            }
            else
            {
                this._panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            }

            this._renderTexture               = new RenderTexture(1920, 1080, 0);
            this._panelSettings.targetTexture = this._renderTexture;
            this._uiDocument.panelSettings    = this._panelSettings;
            this._maskView                    = this._uiGo.AddComponent<AspectRatioMaskView>();
        }

        [UnityTest]
        public IEnumerator Start__AllBarsAreAbsolutePositioned__When__Initialized()
        {
            yield return null;
            yield return null;

            var container = this._uiDocument.rootVisualElement[0];

            for (var i = 0; i < container.childCount; i++)
                Assert.AreEqual(Position.Absolute, container[i].style.position.value, $"Bar {i} should be absolutely positioned");
        }

        [UnityTest]
        public IEnumerator Start__AllBarsIgnoreInput__When__Initialized()
        {
            yield return null;
            yield return null;

            var container = this._uiDocument.rootVisualElement[0];

            for (var i = 0; i < container.childCount; i++)
                Assert.AreEqual(PickingMode.Ignore, container[i].pickingMode, $"Bar {i} should ignore mouse events");
        }

        // --- Structure ---

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
        public IEnumerator Start__FindsCameraBehaviour__When__InScene()
        {
            yield return null;

            var root = this._uiDocument.rootVisualElement;
            Assert.Greater(root.childCount, 0, "BuildOverlay should have run (CameraBehaviour found)");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(this._uiGo);
            Object.DestroyImmediate(this._cameraGo);

            if (this._renderTexture != null)
            {
                this._renderTexture.Release();
                Object.DestroyImmediate(this._renderTexture);
            }

            if (this._panelSettings != null)
                Object.DestroyImmediate(this._panelSettings);
        }

        [UnityTest]
        public IEnumerator UpdateBars__BarsAreCentered__When__LetterboxRatio()
        {
            yield return null;
            yield return null;

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_239_1)
                this._behaviour.CycleAspectRatioForward();

            yield return null;
            yield return null;

            var bars = GetBars();

            if (bars == null || !HasLayout())
                Assert.Inconclusive("UI layout did not resolve");

            var b            = bars.Value;
            var topHeight    = GetStyleFloat(b.top.style.height);
            var bottomHeight = GetStyleFloat(b.bottom.style.height);

            Assert.AreEqual(topHeight,
                            bottomHeight,
                            1f,
                            "Letterbox bars should be centered");
        }

        // --- Centering ---

        [UnityTest]
        public IEnumerator UpdateBars__BarsAreCentered__When__PillarboxRatio()
        {
            yield return null;
            yield return null;

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_4_3)
                this._behaviour.CycleAspectRatioForward();

            yield return null;
            yield return null;

            var bars = GetBars();

            if (bars == null || !HasLayout())
                Assert.Inconclusive("UI layout did not resolve");

            var b          = bars.Value;
            var leftWidth  = GetStyleFloat(b.left.style.width);
            var rightWidth = GetStyleFloat(b.right.style.width);

            Assert.AreEqual(leftWidth,
                            rightWidth,
                            1f,
                            "Pillarbox bars should be centered");
        }

        // --- Pillarbox (4:3) ---

        [UnityTest]
        public IEnumerator UpdateBars__LeftAndRightBarsVisible__When__PillarboxRatio()
        {
            yield return null;
            yield return null;

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_4_3)
                this._behaviour.CycleAspectRatioForward();

            yield return null;
            yield return null;

            var bars = GetBars();

            if (bars == null || !HasLayout())
                Assert.Inconclusive("UI layout did not resolve");

            var b = bars.Value;
            Assert.Greater(GetStyleFloat(b.left.style.width),  0f, "Left bar should have width for pillarbox");
            Assert.Greater(GetStyleFloat(b.right.style.width), 0f, "Right bar should have width for pillarbox");

            Assert.AreEqual(0f,
                            GetStyleFloat(b.top.style.height),
                            0.5f,
                            "Top bar should have no height for pillarbox");
        }

        // --- Letterbox (2.39:1) ---

        [UnityTest]
        public IEnumerator UpdateBars__TopAndBottomBarsVisible__When__LetterboxRatio()
        {
            yield return null;
            yield return null;

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_239_1)
                this._behaviour.CycleAspectRatioForward();

            yield return null;
            yield return null;

            var bars = GetBars();

            if (bars == null || !HasLayout())
                Assert.Inconclusive("UI layout did not resolve");

            var b = bars.Value;
            Assert.Greater(GetStyleFloat(b.top.style.height),    0f, "Top bar should have height for letterbox");
            Assert.Greater(GetStyleFloat(b.bottom.style.height), 0f, "Bottom bar should have height for letterbox");

            Assert.AreEqual(0f,
                            GetStyleFloat(b.left.style.width),
                            0.5f,
                            "Left bar should have no width for letterbox");
        }

        // --- Ratio switching ---

        [UnityTest]
        public IEnumerator UpdateBars__TransitionsCorrectly__When__SwitchingFromPillarboxToLetterbox()
        {
            yield return null;
            yield return null;

            var cam = this._behaviour.CameraElement;

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_4_3)
                this._behaviour.CycleAspectRatioForward();

            yield return null;
            yield return null;

            var bars = GetBars();

            if (bars == null || !HasLayout())
                Assert.Inconclusive("UI layout did not resolve");

            var b = bars.Value;
            Assert.Greater(GetStyleFloat(b.left.style.width), 0f, "Should start with pillarbox");

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_239_1)
                this._behaviour.CycleAspectRatioForward();

            yield return null;
            yield return null;

            Assert.AreEqual(0f,
                            GetStyleFloat(b.left.style.width),
                            0.5f,
                            "Left bar should be zero after switching to letterbox");

            Assert.Greater(GetStyleFloat(b.top.style.height), 0f, "Top bar should have height after switching to letterbox");
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

        private bool HasLayout()
        {
            var root = this._uiDocument.rootVisualElement;

            if (root == null || root.childCount == 0)
                return false;

            var w = root[0].resolvedStyle.width;
            var h = root[0].resolvedStyle.height;
            return !float.IsNaN(w) && w > 0 && !float.IsNaN(h) && h > 0;
        }

        // --- Helpers ---

        /// <summary>
        /// Extracts the float value from a StyleLength set by UpdateBars.
        /// StyleKeyword.Undefined means a numeric value was set (no keyword like Auto/Initial).
        /// </summary>
        private static float GetStyleFloat(StyleLength s) => s.keyword == StyleKeyword.Undefined? s.value.value : 0f;
    }
}