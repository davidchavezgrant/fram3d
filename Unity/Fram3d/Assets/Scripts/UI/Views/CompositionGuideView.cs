using Fram3d.Core.Camera;
using Fram3d.Core.Viewport;
using Fram3d.Engine.Integration;
using Fram3d.UI.Panels;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Views
{
    /// <summary>
    /// Renders composition guide overlays within the unmasked area: rule of thirds
    /// grid, center cross, and safe zone rectangles. All guides start hidden and
    /// are toggled independently. Recalculates positions every frame.
    /// </summary>
    public sealed class CompositionGuideView: MonoBehaviour
    {
        private const float           CENTER_CROSS_ARM   = 20f;
        private const float           CENTER_CROSS_WIDTH = 1.5f;
        private const float           GUIDE_LINE_WIDTH   = 1f;
        private       VisualElement   _actionSafe;
        private       CameraBehaviour _cameraBehaviour;
        private       VisualElement   _container;

        // Center cross
        private VisualElement _crossH;
        private VisualElement _crossV;

        // Rule of thirds
        private VisualElement _thirdsH1;
        private VisualElement _thirdsH2;
        private VisualElement _thirdsV1;
        private VisualElement _thirdsV2;

        private VisualElement            _root;

        // Safe zones (each is a single element with border)
        private VisualElement            _titleSafe;
        private ViewCameraManager        _viewCameraManager;
        public  CompositionGuideSettings Settings { get; } = new();

        private void BuildOverlay()
        {
            this._container             = new VisualElement();
            this._container.pickingMode = PickingMode.Ignore;
            this._container.AddToClassList("guide-container");

            // Rule of thirds
            this._thirdsH1 = CreateLine("guide-line--thirds");
            this._thirdsH2 = CreateLine("guide-line--thirds");
            this._thirdsV1 = CreateLine("guide-line--thirds");
            this._thirdsV2 = CreateLine("guide-line--thirds");
            this._container.Add(this._thirdsH1);
            this._container.Add(this._thirdsH2);
            this._container.Add(this._thirdsV1);
            this._container.Add(this._thirdsV2);

            // Center cross
            this._crossH = CreateLine("guide-line--center");
            this._crossV = CreateLine("guide-line--center");
            this._container.Add(this._crossH);
            this._container.Add(this._crossV);

            // Safe zones
            this._titleSafe  = CreateSafeZone("guide-safe-zone--title");
            this._actionSafe = CreateSafeZone("guide-safe-zone--action");
            this._container.Add(this._titleSafe);
            this._container.Add(this._actionSafe);
            var uiDocument = this.GetComponent<UIDocument>();
            uiDocument.rootVisualElement.Add(this._container);
            this._container.RegisterCallback<GeometryChangedEvent>(_ => this.UpdateGuides());
        }

        private void UpdateCenterCross(UnmaskedRect rect)
        {
            var visible = this.Settings.CenterCrossVisible;
            var display = visible? DisplayStyle.Flex : DisplayStyle.None;
            this._crossH.style.display = display;
            this._crossV.style.display = display;

            if (!visible)
            {
                return;
            }

            var cx = rect.X + rect.Width  / 2f;
            var cy = rect.Y + rect.Height / 2f;

            // Horizontal arm
            this._crossH.style.left   = cx - CENTER_CROSS_ARM;
            this._crossH.style.top    = cy - CENTER_CROSS_WIDTH / 2f;
            this._crossH.style.width  = CENTER_CROSS_ARM * 2f;
            this._crossH.style.height = CENTER_CROSS_WIDTH;

            // Vertical arm
            this._crossV.style.left   = cx - CENTER_CROSS_WIDTH / 2f;
            this._crossV.style.top    = cy - CENTER_CROSS_ARM;
            this._crossV.style.width  = CENTER_CROSS_WIDTH;
            this._crossV.style.height = CENTER_CROSS_ARM * 2f;
        }

        private void ScopeToViewport()
        {
            if (this._viewCameraManager == null || !this._viewCameraManager.IsMultiView)
            {
                this._container.style.left   = 0;
                this._container.style.top    = 0;
                this._container.style.right  = this._cameraBehaviour.RightInsetPixels;
                this._container.style.bottom = 0;
                this._container.style.width  = StyleKeyword.Auto;
                this._container.style.height = StyleKeyword.Auto;
                return;
            }

            var vpRect = this._viewCameraManager.CameraViewRect;
            var rootW  = this._root.resolvedStyle.width;
            var rootH  = this._root.resolvedStyle.height;

            if (float.IsNaN(rootW) || float.IsNaN(rootH))
            {
                return;
            }

            this._container.style.left   = vpRect.x * rootW;
            this._container.style.top    = (1f - vpRect.y - vpRect.height) * rootH;
            this._container.style.width  = vpRect.width  * rootW;
            this._container.style.height = vpRect.height * rootH;
            this._container.style.right  = StyleKeyword.Auto;
            this._container.style.bottom = StyleKeyword.Auto;
        }

        private void UpdateGuides()
        {
            if (this._container == null || this._cameraBehaviour == null)
            {
                return;
            }

            this.ScopeToViewport();
            var viewWidth  = this._container.resolvedStyle.width;
            var viewHeight = this._container.resolvedStyle.height;

            if (float.IsNaN(viewWidth) || float.IsNaN(viewHeight))
            {
                return;
            }

            var rect = this._cameraBehaviour.ActiveAspectRatio.ComputeUnmaskedRect(viewWidth, viewHeight, this._cameraBehaviour.ActiveSensorMode);
            this.UpdateThirds(rect);
            this.UpdateCenterCross(rect);
            this.UpdateSafeZones(rect);
        }

        private void UpdateSafeZones(UnmaskedRect rect)
        {
            var visible = this.Settings.SafeZonesVisible;
            var display = visible? DisplayStyle.Flex : DisplayStyle.None;
            this._titleSafe.style.display  = display;
            this._actionSafe.style.display = display;

            if (!visible)
            {
                return;
            }

            PositionSafeZone(this._titleSafe,  rect, this.Settings.TitleSafePercent);
            PositionSafeZone(this._actionSafe, rect, this.Settings.ActionSafePercent);
        }

        private void UpdateThirds(UnmaskedRect rect)
        {
            var visible = this.Settings.ThirdsVisible;
            var display = visible? DisplayStyle.Flex : DisplayStyle.None;
            this._thirdsH1.style.display = display;
            this._thirdsH2.style.display = display;
            this._thirdsV1.style.display = display;
            this._thirdsV2.style.display = display;

            if (!visible)
            {
                return;
            }

            var thirdW = rect.Width  / 3f;
            var thirdH = rect.Height / 3f;

            // Horizontal lines
            this._thirdsH1.style.left   = rect.X;
            this._thirdsH1.style.top    = rect.Y + thirdH;
            this._thirdsH1.style.width  = rect.Width;
            this._thirdsH1.style.height = GUIDE_LINE_WIDTH;
            this._thirdsH2.style.left   = rect.X;
            this._thirdsH2.style.top    = rect.Y + thirdH * 2f;
            this._thirdsH2.style.width  = rect.Width;
            this._thirdsH2.style.height = GUIDE_LINE_WIDTH;

            // Vertical lines
            this._thirdsV1.style.left   = rect.X + thirdW;
            this._thirdsV1.style.top    = rect.Y;
            this._thirdsV1.style.width  = GUIDE_LINE_WIDTH;
            this._thirdsV1.style.height = rect.Height;
            this._thirdsV2.style.left   = rect.X + thirdW * 2f;
            this._thirdsV2.style.top    = rect.Y;
            this._thirdsV2.style.width  = GUIDE_LINE_WIDTH;
            this._thirdsV2.style.height = rect.Height;
        }

        private static VisualElement CreateLine(string cssClass)
        {
            var line = new VisualElement();
            line.pickingMode = PickingMode.Ignore;
            line.AddToClassList("guide-line");
            line.AddToClassList(cssClass);
            return line;
        }

        private static VisualElement CreateSafeZone(string cssClass)
        {
            var zone = new VisualElement();
            zone.pickingMode = PickingMode.Ignore;
            zone.AddToClassList("guide-safe-zone");
            zone.AddToClassList(cssClass);
            return zone;
        }

        private static void PositionSafeZone(VisualElement zone, UnmaskedRect rect, float percent)
        {
            var insetX = rect.Width  * (1f - percent) / 2f;
            var insetY = rect.Height * (1f - percent) / 2f;
            zone.style.left   = rect.X      + insetX;
            zone.style.top    = rect.Y      + insetY;
            zone.style.width  = rect.Width  - insetX * 2f;
            zone.style.height = rect.Height - insetY * 2f;
        }

        private void Start()
        {
            this._cameraBehaviour   = FindAnyObjectByType<CameraBehaviour>();
            this._viewCameraManager = FindAnyObjectByType<ViewCameraManager>();

            if (this._cameraBehaviour == null)
            {
                Debug.LogWarning("CompositionGuideView: No CameraBehaviour found.");
                return;
            }

            var uiDocument = this.GetComponent<UIDocument>();

            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogWarning("CompositionGuideView: UIDocument or rootVisualElement is null.");
                return;
            }

            this._root = uiDocument.rootVisualElement;
            StyleSheetLoader.Apply(this._root);
            this.BuildOverlay();
        }

        private void Update()
        {
            if (this._cameraBehaviour != null && this._cameraBehaviour.IsDirectorView)
            {
                this._container.style.display = DisplayStyle.None;
                return;
            }

            if (this._container != null)
            {
                this._container.style.display = DisplayStyle.Flex;
            }

            this.UpdateGuides();
        }
    }
}