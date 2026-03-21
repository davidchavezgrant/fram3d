using System;
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
        private const float PANEL_WIDTH    = 440f;
        private const int   MIN_BODY_YEAR  = 2019;

        private VisualElement      _bodyContainer;
        private PopupField<string> _bodyDropdown;
        private Label              _bodyLabel;
        private List<CameraBody>   _bodyList;
        private string             _bodySearch = "";
        private CameraBehaviour    _cameraBehaviour;
        private Label              _focalLengthLabel;
        private Label              _fovLabel;
        private VisualElement      _lensSetContainer;
        private PopupField<string> _lensSetDropdown;
        private List<LensSet>      _lensSetList;
        private string             _lensSetSearch = "";
        private VisualElement      _panel;
        private VisualElement      _root;
        private Label              _sensorLabel;
        private bool               _showAllBodies;
        private bool               _visible = true;

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

                var mousePos  = Input.mousePosition;
                var screenPos = new Vector2(mousePos.x, Screen.height - mousePos.y);
                var panelPos  = RuntimePanelUtils.ScreenToPanel(this._root.panel, screenPos);

                return this._root.panel.Pick(panelPos) != null;
            }
        }

        public void Toggle()
        {
            this._visible             = !this._visible;
            this._panel.style.display = this._visible ? DisplayStyle.Flex : DisplayStyle.None;
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

        private void BuildPanel()
        {
            this._panel                       = new VisualElement();
            this._panel.style.position        = Position.Absolute;
            this._panel.style.right           = 0;
            this._panel.style.top             = 0;
            this._panel.style.bottom          = 0;
            this._panel.style.width           = PANEL_WIDTH;
            this._panel.style.backgroundColor = new Color(0.145f, 0.145f, 0.145f, 1f);
            this._panel.style.borderLeftWidth = 1;
            this._panel.style.borderLeftColor = new Color(0.235f, 0.235f, 0.235f, 1f);
            this._panel.style.overflow        = Overflow.Hidden;

            this.BuildHeader();
            this.BuildBodyContent();
            this._root.Add(this._panel);
        }

        private void BuildHeader()
        {
            var header = new VisualElement();
            header.style.height            = 28;
            header.style.backgroundColor   = new Color(0.118f, 0.118f, 0.118f, 1f);
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = new Color(0.235f, 0.235f, 0.235f, 1f);
            header.style.alignItems        = Align.Center;
            header.style.justifyContent    = Justify.FlexStart;
            header.style.flexDirection     = FlexDirection.Row;
            header.style.paddingLeft       = 10;
            header.style.flexShrink        = 0;

            var title = new Label("PROPERTIES");
            title.style.fontSize                = 10;
            title.style.color                   = new Color(0.4f, 0.4f, 0.4f);
            title.style.letterSpacing           = 1;
            title.style.unityFontStyleAndWeight = FontStyle.Normal;
            header.Add(title);
            this._panel.Add(header);
        }

        private void BuildBodyContent()
        {
            var body = new VisualElement();
            body.style.flexGrow     = 1;
            body.style.paddingTop   = 8;
            body.style.paddingLeft  = 10;
            body.style.paddingRight = 10;

            this.BuildInfoRows(body);
            AddSeparator(body);
            this.BuildBodySection(body);
            AddSeparator(body);
            this.BuildLensSetSection(body);

            this._panel.Add(body);
        }

        private void BuildInfoRows(VisualElement parent)
        {
            this._bodyLabel        = CreateInfoRow(parent, "Body");
            this._sensorLabel      = CreateInfoRow(parent, "Sensor");
            this._focalLengthLabel = CreateInfoRow(parent, "Focal Length");
            this._fovLabel         = CreateInfoRow(parent, "FOV");
        }

        // --- Camera Body ---

        private void BuildBodySection(VisualElement parent)
        {
            AddSectionLabel(parent, "Camera Body");
            parent.Add(CreateSearchField("Search cameras...", value =>
            {
                this._bodySearch = value;
                this.RebuildBodyDropdown();
            }));

            this._bodyContainer = new VisualElement();
            parent.Add(this._bodyContainer);
            this.RebuildBodyDropdown();

            var showAllToggle = new Toggle("Show all cameras");
            showAllToggle.style.fontSize  = 10;
            showAllToggle.style.marginTop = 4;
            showAllToggle.value           = this._showAllBodies;

            showAllToggle.RegisterValueChangedCallback(evt =>
            {
                this._showAllBodies = evt.newValue;
                this.RebuildBodyDropdown();
            });

            parent.Add(showAllToggle);
        }

        private void RebuildBodyDropdown()
        {
            this._bodyContainer.Clear();

            var db  = this._cameraBehaviour.Database;
            var cam = this._cameraBehaviour.CameraElement;

            IEnumerable<CameraBody> filtered = this._showAllBodies
                ? db.Bodies
                : db.Bodies.Where(b => b.Manufacturer == "Generic" || b.Year >= MIN_BODY_YEAR);

            if (!string.IsNullOrEmpty(this._bodySearch))
            {
                var search = this._bodySearch;
                filtered = filtered.Where(b => MatchesSearch(b.Name, search) || MatchesSearch(b.Manufacturer, search));
            }

            this._bodyList = filtered.ToList();
            var names   = this._bodyList.Select(b => $"{b.Manufacturer} — {b.Name}").ToList();
            var current = cam.Body != null ? this._bodyList.IndexOf(cam.Body) : 0;

            if (current < 0 && cam.Body != null)
            {
                this._bodyList.Insert(0, cam.Body);
                names.Insert(0, $"{cam.Body.Manufacturer} — {cam.Body.Name}");
                current = 0;
            }

            if (names.Count == 0)
            {
                this._bodyContainer.Add(new Label("No matches") { style = { fontSize = 10, color = new Color(0.4f, 0.4f, 0.4f) } });

                return;
            }

            this._bodyDropdown = new PopupField<string>(names, current >= 0 ? current : 0);
            this._bodyDropdown.RegisterValueChangedCallback(this.OnBodyChanged);
            this._bodyContainer.Add(this._bodyDropdown);
        }

        private void OnBodyChanged(ChangeEvent<string> evt)
        {
            var index = this._bodyDropdown.index;

            if (index >= 0 && index < this._bodyList.Count)
                this._cameraBehaviour.CameraElement.SetBody(this._bodyList[index]);
        }

        // --- Lens Set ---

        private void BuildLensSetSection(VisualElement parent)
        {
            AddSectionLabel(parent, "Lens Set");
            parent.Add(CreateSearchField("Search lenses...", value =>
            {
                this._lensSetSearch = value;
                this.RebuildLensSetDropdown();
            }));

            this._lensSetContainer = new VisualElement();
            parent.Add(this._lensSetContainer);
            this.RebuildLensSetDropdown();
        }

        private void RebuildLensSetDropdown()
        {
            this._lensSetContainer.Clear();

            var db  = this._cameraBehaviour.Database;
            var cam = this._cameraBehaviour.CameraElement;

            IEnumerable<LensSet> filtered = db.LensSets;

            if (!string.IsNullOrEmpty(this._lensSetSearch))
            {
                var search = this._lensSetSearch;
                filtered = filtered.Where(ls => MatchesSearch(ls.Name, search));
            }

            this._lensSetList = filtered.ToList();
            var names   = this._lensSetList.Select(ls => ls.Name).ToList();
            var current = cam.ActiveLensSet != null ? this._lensSetList.IndexOf(cam.ActiveLensSet) : 0;

            if (current < 0 && cam.ActiveLensSet != null)
            {
                this._lensSetList.Insert(0, cam.ActiveLensSet);
                names.Insert(0, cam.ActiveLensSet.Name);
                current = 0;
            }

            if (names.Count == 0)
            {
                this._lensSetContainer.Add(new Label("No matches") { style = { fontSize = 10, color = new Color(0.4f, 0.4f, 0.4f) } });

                return;
            }

            this._lensSetDropdown = new PopupField<string>(names, current >= 0 ? current : 0);
            this._lensSetDropdown.RegisterValueChangedCallback(this.OnLensSetChanged);
            this._lensSetContainer.Add(this._lensSetDropdown);
        }

        private void OnLensSetChanged(ChangeEvent<string> evt)
        {
            var index = this._lensSetDropdown.index;

            if (index >= 0 && index < this._lensSetList.Count)
                this._cameraBehaviour.CameraElement.SetLensSet(this._lensSetList[index]);
        }

        // --- Labels ---

        private void UpdateLabels()
        {
            var cam = this._cameraBehaviour.CameraElement;

            this._bodyLabel.text        = cam.Body?.Name ?? "—";
            this._sensorLabel.text      = $"{cam.SensorWidth:F1} × {cam.SensorHeight:F1} mm";
            this._focalLengthLabel.text = $"{cam.FocalLength:F0} mm";
            this._fovLabel.text         = $"{cam.VerticalFov * Mathf.Rad2Deg:F1}°";
        }

        // --- UI Helpers ---

        private static bool MatchesSearch(string text, string search) =>
            text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

        private static TextField CreateSearchField(string placeholder, Action<string> onChanged)
        {
            var field = new TextField();
            field.style.marginBottom       = 4;
            field.style.fontSize           = 11;
            field.textEdition.placeholder  = placeholder;

            field.RegisterValueChangedCallback(evt => onChanged(evt.newValue));

            return field;
        }

        private static Label CreateInfoRow(VisualElement parent, string labelText)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom  = 3;

            var nameLabel = new Label(labelText);
            nameLabel.style.width    = 80;
            nameLabel.style.fontSize = 11;
            nameLabel.style.color    = new Color(0.4f, 0.4f, 0.4f);

            var valueLabel = new Label("—");
            valueLabel.style.fontSize = 11;
            valueLabel.style.color    = new Color(0.733f, 0.733f, 0.733f);

            row.Add(nameLabel);
            row.Add(valueLabel);
            parent.Add(row);

            return valueLabel;
        }

        private static void AddSectionLabel(VisualElement parent, string text)
        {
            var label = new Label(text);
            label.style.fontSize      = 10;
            label.style.color         = new Color(0.4f, 0.4f, 0.4f);
            label.style.letterSpacing = 1;
            label.style.marginBottom  = 4;
            parent.Add(label);
        }

        private static void AddSeparator(VisualElement parent)
        {
            var separator = new VisualElement();
            separator.style.height          = 1;
            separator.style.backgroundColor = new Color(0.235f, 0.235f, 0.235f);
            separator.style.marginTop       = 8;
            separator.style.marginBottom    = 8;
            parent.Add(separator);
        }
    }
}
