using Fram3d.Core.Camera;
using Fram3d.Core.Viewport;
using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
namespace Fram3d.UI.Panels
{
    /// <summary>
    /// Properties panel MonoBehaviour. Creates a docked side panel with camera info,
    /// body picker, and lens set picker. Toggled with I key.
    /// Thin orchestrator — delegates to CameraInfoSection, CameraBodySection, LensSetSection.
    /// </summary>
    public sealed class PropertiesPanelView: MonoBehaviour
    {
        private const float             PANEL_WIDTH = 440f;
        private       CameraBodySection _bodySection;
        private       CameraBehaviour   _cameraBehaviour;
        private       CameraInfoSection _infoSection;
        private       LensSetSection    _lensSetSection;
        private       VisualElement     _panel;
        private       VisualElement     _root;
        private       SensorModeSection _sensorModeSection;
        private       ShakeSection      _shakeSection;
        private       bool              _visible = true;

        /// <summary>
        /// True when any search field in the panel has keyboard focus.
        /// </summary>
        public bool HasFocusedTextField => (this._bodySection    != null && this._bodySection.HasFocus)
                                        || (this._lensSetSection != null && this._lensSetSection.HasFocus);

        /// <summary>
        /// True when the mouse is over any UI Toolkit element (panel, dropdowns, popup menus).
        /// </summary>
        public bool IsPointerOverUI
        {
            get
            {
                if (!this._visible || this._root == null || Mouse.current == null)
                    return false;

                var mousePos  = Mouse.current.position.ReadValue();
                var screenPos = new Vector2(mousePos.x, Screen.height - mousePos.y);
                var panelPos  = RuntimePanelUtils.ScreenToPanel(this._root.panel, screenPos);
                return this._root.panel.Pick(panelPos) != null;
            }
        }

        public bool  IsVisible  => this._visible;
        public float PanelWidth => PANEL_WIDTH;

        public void Toggle()
        {
            this._visible             = !this._visible;
            this._panel.style.display = this._visible? DisplayStyle.Flex : DisplayStyle.None;
            this._cameraBehaviour?.SetRightInset(this._visible? PANEL_WIDTH : 0f);
        }

        private VisualElement BuildContent()
        {
            var cam     = this._cameraBehaviour.CameraElement;
            var db      = this._cameraBehaviour.Database;
            var content = new VisualElement();
            content.AddToClassList("panel-content");
            this._infoSection = new CameraInfoSection();
            this._infoSection.UpdateValues(cam, this._cameraBehaviour.ActiveAspectRatio);
            content.Add(this._infoSection);
            content.Add(Theme.CreateSeparator());
            this._bodySection             =  new CameraBodySection(db.Bodies, cam.Body);
            this._bodySection.BodyChanged += this.OnBodyChanged;
            content.Add(this._bodySection);
            content.Add(Theme.CreateSeparator());
            var initialModes = cam.Body?.HasSensorModes == true? cam.Body.SensorModes : null;
            var initialMode  = this._cameraBehaviour.ActiveSensorMode;
            this._sensorModeSection             =  new SensorModeSection(initialModes, initialMode);
            this._sensorModeSection.ModeChanged += mode => this._cameraBehaviour.SetSensorMode(mode);
            content.Add(this._sensorModeSection);
            content.Add(Theme.CreateSeparator());
            this._lensSetSection                =  new LensSetSection(db.LensSets, cam.ActiveLensSet);
            this._lensSetSection.LensSetChanged += lensSet => cam.SetLensSet(lensSet);
            content.Add(this._lensSetSection);
            content.Add(Theme.CreateSeparator());
            this._shakeSection = new ShakeSection(cam);
            content.Add(this._shakeSection);
            return content;
        }

        private void BuildPanel()
        {
            this._panel             = new VisualElement();
            this._panel.style.width = PANEL_WIDTH;
            this._panel.AddToClassList("properties-panel");
            this._panel.Add(BuildHeader());
            this._panel.Add(this.BuildContent());
            this._root.Add(this._panel);
        }

        private void OnBodyChanged(CameraBody body)
        {
            this._cameraBehaviour.CameraElement.SetBody(body);
            var firstMode = body.HasSensorModes? body.SensorModes[0] : null;
            this._cameraBehaviour.SetSensorMode(firstMode);
            this._sensorModeSection?.SetModes(body.HasSensorModes? body.SensorModes : null, firstMode);
        }

        private static VisualElement BuildHeader()
        {
            var header = new VisualElement();
            header.AddToClassList("panel-header");
            var title = new Label("PROPERTIES");
            title.AddToClassList("panel-header-title");
            header.Add(title);
            return header;
        }

        private void Start()
        {
            this._cameraBehaviour = FindAnyObjectByType<CameraBehaviour>();

            if (this._cameraBehaviour == null)
            {
                Debug.LogWarning("PropertiesPanelView: No CameraBehaviour found.");
                return;
            }

            var uiDocument = this.GetComponent<UIDocument>();

            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogWarning("PropertiesPanelView: UIDocument or rootVisualElement is null.");
                return;
            }

            this._root = uiDocument.rootVisualElement;
            StyleSheetLoader.Apply(this._root);
            this.BuildPanel();
            this._cameraBehaviour.SetRightInset(this._visible? PANEL_WIDTH : 0f);
        }

        private void Update()
        {
            if (this._cameraBehaviour == null)
            {
                return;
            }

            // Keep the viewport inset in screen pixels (CSS pixels × scale).
            // PanelSettings scale means PANEL_WIDTH CSS px ≠ PANEL_WIDTH screen px.
            this.UpdateRightInset();

            if (!this._visible)
            {
                return;
            }

            var cam = this._cameraBehaviour.CameraElement;
            this._infoSection?.UpdateValues(cam, this._cameraBehaviour.ActiveAspectRatio);
            this._shakeSection?.UpdateValues(cam);
        }

        private void UpdateRightInset()
        {
            if (!this._visible)
            {
                this._cameraBehaviour.SetRightInset(0f);
                return;
            }

            var rootW = this._root?.resolvedStyle.width ?? 0f;

            if (float.IsNaN(rootW) || rootW <= 0)
            {
                return;
            }

            var scale = Screen.width / rootW;
            this._cameraBehaviour.SetRightInset(PANEL_WIDTH * scale);
        }
    }
}