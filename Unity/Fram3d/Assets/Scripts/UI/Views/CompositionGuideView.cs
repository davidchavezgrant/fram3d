using Fram3d.Core.Cameras;
using Fram3d.Core.Viewports;
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
    /// Delegates rendering to <see cref="ThirdsGuide"/>, <see cref="CenterCrossGuide"/>,
    /// and <see cref="SafeZoneGuide"/>.
    /// </summary>
    public sealed class CompositionGuideView: MonoBehaviour
    {
        private CameraBehaviour   _cameraBehaviour;
        private CenterCrossGuide  _centerCross;
        private VisualElement     _container;
        private VisualElement     _root;
        private SafeZoneGuide     _safeZones;
        private ThirdsGuide       _thirds;
        private ViewCameraManager _viewCameraManager;

        public CompositionGuideSettings Settings { get; } = new();

        private void BuildOverlay()
        {
            this._container             = new VisualElement();
            this._container.pickingMode = PickingMode.Ignore;
            this._container.AddToClassList("guide-container");

            this._thirds      = new ThirdsGuide();
            this._centerCross = new CenterCrossGuide();
            this._safeZones   = new SafeZoneGuide();
            this._container.Add(this._thirds);
            this._container.Add(this._centerCross);
            this._container.Add(this._safeZones);

            var uiDocument = this.GetComponent<UIDocument>();
            uiDocument.rootVisualElement.Add(this._container);
            this._container.RegisterCallback<GeometryChangedEvent>(_ => this.UpdateGuides());
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

        private void UpdateGuides()
        {
            if (this._container == null || this._cameraBehaviour == null)
            {
                return;
            }

            ViewportScope.Apply(this._container, this._root,
                                this._viewCameraManager, this._cameraBehaviour.RightInsetPixels);
            var viewWidth  = this._container.resolvedStyle.width;
            var viewHeight = this._container.resolvedStyle.height;

            if (float.IsNaN(viewWidth) || float.IsNaN(viewHeight))
            {
                return;
            }

            var rect = this._cameraBehaviour.ActiveAspectRatio.ComputeUnmaskedRect(
                viewWidth, viewHeight, this._cameraBehaviour.ActiveSensorMode);

            this._thirds.Update(rect, this.Settings.ThirdsVisible);
            this._centerCross.Update(rect, this.Settings.CenterCrossVisible);
            this._safeZones.Update(rect, this.Settings.SafeZonesVisible,
                this.Settings.TitleSafePercent, this.Settings.ActionSafePercent);
        }
    }
}
