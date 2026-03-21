using System.Collections.Generic;
using System.Linq;
using Fram3d.Core.Camera;
using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Panels
{
    /// <summary>
    /// Runtime properties panel built with UI Toolkit.
    /// Docked to the right side. Shows camera body, lens set, focal length, sensor, FOV.
    /// Toggled with I key. Matches the Premiere-style dark theme from the UI mockup.
    /// Will expand to show element/light/character properties when selection arrives (2.1.1).
    /// </summary>
    public sealed class PropertiesPanelView: MonoBehaviour
    {
        private const float              PANEL_WIDTH = 320f;
        private       PopupField<string> _bodyDropdown;
        private       Label              _bodyLabel;
        private       List<CameraBody>   _bodyList;
        private       CameraBehaviour    _cameraBehaviour;
        private       Label              _focalLengthLabel;
        private       Label              _fovLabel;
        private       PopupField<string> _lensSetDropdown;
        private       List<LensSet>      _lensSetList;
        private       VisualElement      _panel;
        private       VisualElement      _root;
        private       Label              _sensorLabel;
        private       bool               _visible = true;

        /// <summary>
        /// True when the mouse is over any UI Toolkit element (panel, dropdowns, popup menus).
        /// Used by CameraInputHandler to block scroll/drag passthrough.
        /// </summary>
        public bool IsPointerOverUI
        {
            get
            {
                if (!this._visible || this._root == null)
                    return false;

                var mousePos  = UnityEngine.Input.mousePosition;
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

        private void BuildBody()
        {
            var body = new VisualElement();
            body.style.flexGrow     = 1;
            body.style.paddingTop   = 8;
            body.style.paddingLeft  = 10;
            body.style.paddingRight = 10;
            this.BuildInfoRows(body);
            AddSeparator(body);
            this.BuildBodyDropdown(body);
            AddSeparator(body);
            this.BuildLensSetDropdown(body);
            this._panel.Add(body);
        }

        private void BuildBodyDropdown(VisualElement parent)
        {
            var db = this._cameraBehaviour.Database;
            this._bodyList = db.Bodies.ToList();
            var names   = this._bodyList.Select(b => $"{b.Manufacturer} — {b.Name}").ToList();
            var cam     = this._cameraBehaviour.CameraElement;
            var current = cam.Body != null? this._bodyList.IndexOf(cam.Body) : 0;
            AddSectionLabel(parent, "Camera Body");
            this._bodyDropdown = new PopupField<string>(names, current >= 0? current : 0);
            this._bodyDropdown.RegisterValueChangedCallback(this.OnBodyChanged);
            parent.Add(this._bodyDropdown);
        }

        private void BuildHeader()
        {
            var header = new VisualElement();
            header.style.height = 28;

            header.style.backgroundColor = new Color(0.118f,
                                                     0.118f,
                                                     0.118f,
                                                     1f); // #1e1e1e

            header.style.borderBottomWidth = 1;

            header.style.borderBottomColor = new Color(0.235f,
                                                       0.235f,
                                                       0.235f,
                                                       1f); // #3c3c3c

            header.style.alignItems     = Align.Center;
            header.style.justifyContent = Justify.FlexStart;
            header.style.flexDirection  = FlexDirection.Row;
            header.style.paddingLeft    = 10;
            header.style.flexShrink     = 0;
            var title = new Label("PROPERTIES");
            title.style.fontSize                = 10;
            title.style.color                   = new Color(0.4f, 0.4f, 0.4f); // #666
            title.style.letterSpacing           = 1;
            title.style.unityFontStyleAndWeight = FontStyle.Normal;
            header.Add(title);
            this._panel.Add(header);
        }

        private void BuildInfoRows(VisualElement parent)
        {
            this._bodyLabel        = CreateInfoRow(parent, "Body");
            this._sensorLabel      = CreateInfoRow(parent, "Sensor");
            this._focalLengthLabel = CreateInfoRow(parent, "Focal Length");
            this._fovLabel         = CreateInfoRow(parent, "FOV");
        }

        private void BuildLensSetDropdown(VisualElement parent)
        {
            var db = this._cameraBehaviour.Database;
            this._lensSetList = db.LensSets.ToList();
            var names   = this._lensSetList.Select(ls => ls.Name).ToList();
            var cam     = this._cameraBehaviour.CameraElement;
            var current = cam.ActiveLensSet != null? this._lensSetList.IndexOf(cam.ActiveLensSet) : 0;
            AddSectionLabel(parent, "Lens Set");
            this._lensSetDropdown = new PopupField<string>(names, current >= 0? current : 0);
            this._lensSetDropdown.RegisterValueChangedCallback(this.OnLensSetChanged);
            parent.Add(this._lensSetDropdown);
        }

        private void BuildPanel()
        {
            this._panel                = new VisualElement();
            this._panel.style.position = Position.Absolute;
            this._panel.style.right    = 0;
            this._panel.style.top      = 0;
            this._panel.style.bottom   = 0;
            this._panel.style.width    = PANEL_WIDTH;

            this._panel.style.backgroundColor = new Color(0.145f,
                                                          0.145f,
                                                          0.145f,
                                                          1f); // #252525

            this._panel.style.borderLeftWidth = 1;

            this._panel.style.borderLeftColor = new Color(0.235f,
                                                          0.235f,
                                                          0.235f,
                                                          1f); // #3c3c3c

            this._panel.style.overflow = Overflow.Hidden;
            this.BuildHeader();
            this.BuildBody();
            this._root.Add(this._panel);
        }

        private void OnBodyChanged(ChangeEvent<string> evt)
        {
            var index = this._bodyDropdown.index;

            if (index >= 0 && index < this._bodyList.Count)
                this._cameraBehaviour.CameraElement.SetBody(this._bodyList[index]);
        }

        private void OnLensSetChanged(ChangeEvent<string> evt)
        {
            var index = this._lensSetDropdown.index;

            if (index >= 0 && index < this._lensSetList.Count)
                this._cameraBehaviour.CameraElement.SetLensSet(this._lensSetList[index]);
        }

        private void UpdateLabels()
        {
            var cam = this._cameraBehaviour.CameraElement;
            this._bodyLabel.text        = cam.Body?.Name ?? "—";
            this._sensorLabel.text      = $"{cam.SensorWidth:F1} × {cam.SensorHeight:F1} mm";
            this._focalLengthLabel.text = $"{cam.FocalLength:F0} mm";
            this._fovLabel.text         = $"{cam.VerticalFov * Mathf.Rad2Deg:F1}°";
        }

        private static void AddSectionLabel(VisualElement parent, string text)
        {
            var label = new Label(text);
            label.style.fontSize      = 10;
            label.style.color         = new Color(0.4f, 0.4f, 0.4f); // #666
            label.style.letterSpacing = 1;
            label.style.marginBottom  = 4;
            parent.Add(label);
        }

        private static void AddSeparator(VisualElement parent)
        {
            var separator = new VisualElement();
            separator.style.height          = 1;
            separator.style.backgroundColor = new Color(0.235f, 0.235f, 0.235f); // #3c3c3c
            separator.style.marginTop       = 8;
            separator.style.marginBottom    = 8;
            parent.Add(separator);
        }

        private static Label CreateInfoRow(VisualElement parent, string labelText)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom  = 3;
            var nameLabel = new Label(labelText);
            nameLabel.style.width    = 80;
            nameLabel.style.fontSize = 11;
            nameLabel.style.color    = new Color(0.4f, 0.4f, 0.4f); // #666
            var valueLabel = new Label("—");
            valueLabel.style.fontSize = 11;
            valueLabel.style.color    = new Color(0.733f, 0.733f, 0.733f); // #bbb
            row.Add(nameLabel);
            row.Add(valueLabel);
            parent.Add(row);
            return valueLabel;
        }

        private void Start()
        {
            this._cameraBehaviour = FindObjectOfType<CameraBehaviour>();

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

            this.UpdateLabels();
        }
    }
}