using Fram3d.Core.Camera;
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
        private       CameraBodySection  _bodySection;
        private       CameraBehaviour    _cameraBehaviour;
        private       CameraInfoSection  _infoSection;
        private       LensSetSection     _lensSetSection;
        private       VisualElement      _panel;
        private       SensorModeSection  _sensorModeSection;
        private       ShakeSection       _shakeSection;
        private       VisualElement      _root;
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

        public void Toggle()
        {
            this._visible             = !this._visible;
            this._panel.style.display = this._visible? DisplayStyle.Flex : DisplayStyle.None;
        }

        private VisualElement BuildContent()
        {
            var cam     = this._cameraBehaviour.CameraElement;
            var db      = this._cameraBehaviour.Database;
            var content = new VisualElement();
            content.style.flexGrow     = 1;
            content.style.paddingTop   = 8;
            content.style.paddingLeft  = 10;
            content.style.paddingRight = 10;
            this._infoSection          = new CameraInfoSection();
            this._infoSection.UpdateValues(cam, this._cameraBehaviour.ActiveAspectRatio);
            content.Add(this._infoSection);
            content.Add(Theme.CreateSeparator());
            this._bodySection             =  new CameraBodySection(db.Bodies, cam.Body);
            this._bodySection.BodyChanged += this.OnBodyChanged;
            content.Add(this._bodySection);
            content.Add(Theme.CreateSeparator());
            var initialModes   = cam.Body?.HasSensorModes == true? cam.Body.SensorModes : null;
            var initialMode    = this._cameraBehaviour.ActiveSensorMode;
            this._sensorModeSection              =  new SensorModeSection(initialModes, initialMode);
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
            this._panel                       = new VisualElement();
            this._panel.style.position        = Position.Absolute;
            this._panel.style.right           = 0;
            this._panel.style.top             = 0;
            this._panel.style.bottom          = 0;
            this._panel.style.width           = PANEL_WIDTH;
            this._panel.style.backgroundColor = Theme.PANEL_BACKGROUND;
            this._panel.style.borderLeftWidth = 1;
            this._panel.style.borderLeftColor = Theme.BORDER;
            this._panel.style.overflow        = Overflow.Hidden;
            this._panel.Add(BuildHeader());
            this._panel.Add(this.BuildContent());
            this._root.Add(this._panel);
        }

        private static VisualElement BuildHeader()
        {
            var header = new VisualElement();
            header.style.height            = Theme.HEADER_HEIGHT;
            header.style.backgroundColor   = Theme.HEADER_BACKGROUND;
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = Theme.BORDER;
            header.style.alignItems        = Align.Center;
            header.style.justifyContent    = Justify.FlexStart;
            header.style.flexDirection     = FlexDirection.Row;
            header.style.paddingLeft       = 10;
            header.style.flexShrink        = 0;
            var title = new Label("PROPERTIES");
            title.style.fontSize      = Theme.FONT_HEADER;
            title.style.color         = Theme.LABEL_DIM;
            title.style.letterSpacing = 1;
            header.Add(title);
            return header;
        }

        private void OnBodyChanged(CameraBody body)
        {
            this._cameraBehaviour.CameraElement.SetBody(body);
            var firstMode = body.HasSensorModes? body.SensorModes[0] : null;
            this._cameraBehaviour.SetSensorMode(firstMode);
            this._sensorModeSection?.SetModes(
                body.HasSensorModes? body.SensorModes : null,
                firstMode);
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
            this.BuildPanel();
        }

        private void Update()
        {
            if (!this._visible || this._cameraBehaviour == null)
                return;

            var cam = this._cameraBehaviour.CameraElement;
            this._infoSection?.UpdateValues(cam, this._cameraBehaviour.ActiveAspectRatio);
            this._shakeSection?.UpdateValues(cam);
        }
    }
}