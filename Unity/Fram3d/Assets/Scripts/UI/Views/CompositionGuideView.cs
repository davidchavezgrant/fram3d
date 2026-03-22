using Fram3d.Core.Camera;
using Fram3d.Engine.Integration;
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
        private const float CENTER_CROSS_ARM   = 20f;
        private const float CENTER_CROSS_ALPHA = 0.25f;
        private const float CENTER_CROSS_WIDTH = 1.5f;
        private const float GUIDE_LINE_ALPHA   = 0.15f;
        private const float GUIDE_LINE_WIDTH   = 1f;

        private CameraBehaviour _cameraBehaviour;
        private VisualElement   _container;

        // Rule of thirds
        private VisualElement _thirdsH1;
        private VisualElement _thirdsH2;
        private VisualElement _thirdsV1;
        private VisualElement _thirdsV2;

        // Center cross
        private VisualElement _crossH;
        private VisualElement _crossV;

        // Safe zones (each is a single element with border)
        private VisualElement _titleSafe;
        private VisualElement _actionSafe;

        public CompositionGuideSettings Settings { get; } = new();

        private static VisualElement CreateLine(float alpha, float thickness)
        {
            var line = new VisualElement();
            line.style.position        = Position.Absolute;
            line.style.backgroundColor = new Color(1f, 1f, 1f, alpha);
            line.pickingMode           = PickingMode.Ignore;

            return line;
        }

        private static VisualElement CreateSafeZone(float alpha)
        {
            var zone = new VisualElement();
            zone.style.position          = Position.Absolute;
            zone.style.backgroundColor   = new Color(0, 0, 0, 0);
            zone.style.borderTopWidth    = 1;
            zone.style.borderBottomWidth = 1;
            zone.style.borderLeftWidth   = 1;
            zone.style.borderRightWidth  = 1;
            var borderColor              = new Color(1f, 1f, 1f, alpha);
            zone.style.borderTopColor    = borderColor;
            zone.style.borderBottomColor = borderColor;
            zone.style.borderLeftColor   = borderColor;
            zone.style.borderRightColor  = borderColor;
            zone.pickingMode             = PickingMode.Ignore;

            return zone;
        }

        private void BuildOverlay()
        {
            this._container                = new VisualElement();
            this._container.style.position = Position.Absolute;
            this._container.style.left     = 0;
            this._container.style.top      = 0;
            this._container.style.right    = 0;
            this._container.style.bottom   = 0;
            this._container.pickingMode    = PickingMode.Ignore;

            // Rule of thirds
            this._thirdsH1 = CreateLine(GUIDE_LINE_ALPHA, GUIDE_LINE_WIDTH);
            this._thirdsH2 = CreateLine(GUIDE_LINE_ALPHA, GUIDE_LINE_WIDTH);
            this._thirdsV1 = CreateLine(GUIDE_LINE_ALPHA, GUIDE_LINE_WIDTH);
            this._thirdsV2 = CreateLine(GUIDE_LINE_ALPHA, GUIDE_LINE_WIDTH);
            this._container.Add(this._thirdsH1);
            this._container.Add(this._thirdsH2);
            this._container.Add(this._thirdsV1);
            this._container.Add(this._thirdsV2);

            // Center cross
            this._crossH = CreateLine(CENTER_CROSS_ALPHA, CENTER_CROSS_WIDTH);
            this._crossV = CreateLine(CENTER_CROSS_ALPHA, CENTER_CROSS_WIDTH);
            this._container.Add(this._crossH);
            this._container.Add(this._crossV);

            // Safe zones
            this._titleSafe  = CreateSafeZone(0.12f);
            this._actionSafe = CreateSafeZone(0.08f);
            this._container.Add(this._titleSafe);
            this._container.Add(this._actionSafe);

            var uiDocument = this.GetComponent<UIDocument>();
            uiDocument.rootVisualElement.Add(this._container);
            this._container.RegisterCallback<GeometryChangedEvent>(_ => this.UpdateGuides());
        }

        private void UpdateGuides()
        {
            if (this._container == null || this._cameraBehaviour == null)
                return;

            var viewWidth  = this._container.resolvedStyle.width;
            var viewHeight = this._container.resolvedStyle.height;

            if (float.IsNaN(viewWidth) || float.IsNaN(viewHeight))
                return;

            var rect = this._cameraBehaviour.ActiveAspectRatio.ComputeUnmaskedRect(
                viewWidth, viewHeight, this._cameraBehaviour.ActiveSensorMode);

            this.UpdateThirds(rect);
            this.UpdateCenterCross(rect);
            this.UpdateSafeZones(rect);
        }

        private void UpdateThirds(UnmaskedRect rect)
        {
            var visible = this.Settings.ThirdsVisible;
            var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            this._thirdsH1.style.display = display;
            this._thirdsH2.style.display = display;
            this._thirdsV1.style.display = display;
            this._thirdsV2.style.display = display;

            if (!visible)
                return;

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

        private void UpdateCenterCross(UnmaskedRect rect)
        {
            var visible = this.Settings.CenterCrossVisible;
            var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            this._crossH.style.display = display;
            this._crossV.style.display = display;

            if (!visible)
                return;

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

        private void UpdateSafeZones(UnmaskedRect rect)
        {
            var visible = this.Settings.SafeZonesVisible;
            var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            this._titleSafe.style.display  = display;
            this._actionSafe.style.display = display;

            if (!visible)
                return;

            PositionSafeZone(this._titleSafe,  rect, this.Settings.TitleSafePercent);
            PositionSafeZone(this._actionSafe, rect, this.Settings.ActionSafePercent);
        }

        private static void PositionSafeZone(VisualElement zone, UnmaskedRect rect, float percent)
        {
            var insetX = rect.Width  * (1f - percent) / 2f;
            var insetY = rect.Height * (1f - percent) / 2f;

            zone.style.left   = rect.X + insetX;
            zone.style.top    = rect.Y + insetY;
            zone.style.width  = rect.Width  - insetX * 2f;
            zone.style.height = rect.Height - insetY * 2f;
        }

        private void Start()
        {
            this._cameraBehaviour = FindAnyObjectByType<CameraBehaviour>();

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

            this.BuildOverlay();
        }

        private void Update()
        {
            this.UpdateGuides();
        }
    }
}
