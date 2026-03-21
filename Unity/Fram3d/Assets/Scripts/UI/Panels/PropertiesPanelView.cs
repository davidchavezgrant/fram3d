using System.Collections.Generic;
using System.Linq;
using Fram3d.Core.Camera;
using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.InputSystem;
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
        private const float PANEL_WIDTH   = 440f;
        private const int   MIN_BODY_YEAR = 2019;

        private SearchableDropdown _bodyDropdown;
        private Label              _bodyLabel;
        private List<CameraBody>   _bodyList;
        private CameraBehaviour    _cameraBehaviour;
        private Label              _focalLengthLabel;
        private Label              _fovLabel;
        private SearchableDropdown _lensSetDropdown;
        private List<LensSet>      _lensSetList;
        private VisualElement      _panel;
        private VisualElement      _root;
        private Label              _sensorLabel;
        private bool               _showAllBodies;
        private VisualElement      _bodySectionContainer;
        private bool               _visible = true;

        /// <summary>
        /// True when the mouse is over any UI Toolkit element (panel, dropdowns, popup menus).
        /// </summary>
        public bool IsPointerOverUI
        {
            get
            {
                if (!this._visible || this._root == null)
                    return false;

                var mousePos  = Mouse.current.position.ReadValue();
                var screenPos = new Vector2(mousePos.x, Screen.height - mousePos.y);
                var panelPos  = RuntimePanelUtils.ScreenToPanel(this._root.panel, screenPos);

                return this._root.panel.Pick(panelPos) != null;
            }
        }

        /// <summary>
        /// True when a search field in the panel has keyboard focus.
        /// Used to suppress keyboard shortcuts while typing.
        /// </summary>
        public bool HasFocusedTextField
        {
            get
            {
                if (this._bodyDropdown != null && this._bodyDropdown.HasFocus)
                    return true;

                if (this._lensSetDropdown != null && this._lensSetDropdown.HasFocus)
                    return true;

                return false;
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
            this.BuildContent();
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

        private void BuildContent()
        {
            var content = new VisualElement();
            content.style.flexGrow     = 1;
            content.style.paddingTop   = 8;
            content.style.paddingLeft  = 10;
            content.style.paddingRight = 10;

            this.BuildInfoRows(content);
            AddSeparator(content);
            this.BuildBodySection(content);
            AddSeparator(content);
            this.BuildLensSetSection(content);

            this._panel.Add(content);
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

            this._bodySectionContainer = new VisualElement();
            parent.Add(this._bodySectionContainer);

            this.RebuildBodyDropdown();

            var labelColor    = new Color(0.75f, 0.75f, 0.75f);
            var showAllToggle = new Toggle("Show all cameras");
            showAllToggle.style.fontSize  = 9;
            showAllToggle.style.marginTop = 4;

            // Force the label color so Unity's default checked-blue doesn't override it
            var toggleLabel = showAllToggle.Q<Label>();

            if (toggleLabel != null)
                toggleLabel.style.color = labelColor;

            showAllToggle.RegisterValueChangedCallback(_ =>
            {
                if (toggleLabel != null)
                    toggleLabel.style.color = labelColor;
            });

            // Shrink the checkbox itself
            var checkmark = showAllToggle.Q(className: "unity-toggle__checkmark");

            if (checkmark != null)
            {
                checkmark.style.width  = 12;
                checkmark.style.height = 12;
            }
            showAllToggle.value           = this._showAllBodies;

            showAllToggle.RegisterValueChangedCallback(evt =>
            {
                this._showAllBodies = evt.newValue;
                var names = this._bodyList.Select(b => $"{b.Manufacturer} — {b.Name}").ToList();
                this.ApplyBodyBrowseFilter(names);
            });

            parent.Add(showAllToggle);
        }

        private void RebuildBodyDropdown()
        {
            this._bodySectionContainer.Clear();

            var db  = this._cameraBehaviour.Database;
            var cam = this._cameraBehaviour.CameraElement;

            // Generics first, then sorted by year descending (most recent cameras first)
            this._bodyList = db.Bodies
                .OrderByDescending(b => b.Manufacturer == "Generic")
                .ThenByDescending(b => b.Year)
                .ToList();
            var names   = this._bodyList.Select(b => $"{b.Manufacturer} — {b.Name}").ToList();
            var current = cam.Body != null ? this._bodyList.IndexOf(cam.Body) : 0;

            this._bodyDropdown = new SearchableDropdown(names, current >= 0 ? current : 0, "Search cameras...");
            this._bodyDropdown.SelectionChanged += this.OnBodySelected;
            this._bodySectionContainer.Add(this._bodyDropdown.Root);

            this.ApplyBodyBrowseFilter(names);
        }

        private void ApplyBodyBrowseFilter(List<string> allNames)
        {
            if (this._showAllBodies)
            {
                this._bodyDropdown.SetBrowseFilter(allNames);

                return;
            }

            // When browsing (no search text), show only generics + 2019+ cameras
            var browseNames = new List<string>();

            for (var i = 0; i < this._bodyList.Count; i++)
            {
                var body = this._bodyList[i];

                if (body.Manufacturer == "Generic" || body.Year >= MIN_BODY_YEAR)
                    browseNames.Add(allNames[i]);
            }

            this._bodyDropdown.SetBrowseFilter(browseNames);
        }

        private void OnBodySelected(int index)
        {
            if (index >= 0 && index < this._bodyList.Count)
                this._cameraBehaviour.CameraElement.SetBody(this._bodyList[index]);
        }

        // --- Lens Set ---

        private void BuildLensSetSection(VisualElement parent)
        {
            AddSectionLabel(parent, "Lens Set");

            var db  = this._cameraBehaviour.Database;
            var cam = this._cameraBehaviour.CameraElement;

            this._lensSetList = db.LensSets.ToList();
            var names   = this._lensSetList.Select(ls => ls.Name).ToList();
            var current = cam.ActiveLensSet != null ? this._lensSetList.IndexOf(cam.ActiveLensSet) : 0;

            this._lensSetDropdown = new SearchableDropdown(names, current >= 0 ? current : 0, "Search lenses...");
            this._lensSetDropdown.SelectionChanged += this.OnLensSetSelected;
            parent.Add(this._lensSetDropdown.Root);
        }

        private void OnLensSetSelected(int index)
        {
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
