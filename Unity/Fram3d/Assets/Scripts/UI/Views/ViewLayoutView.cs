using System;
using Fram3d.Core.Common;
using Fram3d.Core.Scene;
using Fram3d.Engine.Integration;
using Fram3d.UI.Panels;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Fram3d.UI.Views
{
    /// <summary>
    /// Renders the layout chooser and per-viewport header bars. Each viewport
    /// gets a header showing the view type name — clicking the name opens a
    /// dropdown to switch between Camera View and Director View. Headers
    /// appear in all modes including single-view.
    /// </summary>
    public sealed class ViewLayoutView : MonoBehaviour
    {
        private const int HEADER_HEIGHT = 26;

        private VisualElement     _activeOutline;
        private VisualElement     _headerContainer;
        private VisualElement     _layoutChooser;
        private VisualElement     _root;
        private ViewCameraManager _viewCameraManager;
        private ViewHeader[]      _viewHeaders = Array.Empty<ViewHeader>();

        public bool IsPointerOverUI
        {
            get
            {
                if (this._root == null || this._root.panel == null || Mouse.current == null)
                {
                    return false;
                }

                var mousePos  = Mouse.current.position.ReadValue();
                var screenPos = new Vector2(mousePos.x, Screen.height - mousePos.y);
                var panelPos  = RuntimePanelUtils.ScreenToPanel(this._root.panel, screenPos);

                if (this._layoutChooser != null && this._layoutChooser.worldBound.Contains(panelPos))
                {
                    return true;
                }

                foreach (var header in this._viewHeaders)
                {
                    if (header != null && header.Root.worldBound.Contains(panelPos))
                    {
                        return true;
                    }
                }

                return this._root.panel.Pick(panelPos) != null;
            }
        }

        private void Start()
        {
            this._viewCameraManager = FindAnyObjectByType<ViewCameraManager>();

            if (this._viewCameraManager == null)
            {
                Debug.LogWarning("ViewLayoutView: No ViewCameraManager found.");
                return;
            }

            this.InitializeUI();
        }

        private void InitializeUI()
        {
            if (this._root != null)
            {
                return;
            }

            var uiDocument = this.GetComponent<UIDocument>();

            if (uiDocument == null)
            {
                Debug.LogWarning("ViewLayoutView: No UIDocument component.");
                return;
            }

            if (uiDocument.panelSettings == null)
            {
                Debug.LogWarning("ViewLayoutView: UIDocument has no PanelSettings assigned.");
                return;
            }

            if (uiDocument.rootVisualElement == null)
            {
                return;
            }

            this._root = uiDocument.rootVisualElement;
            StyleSheetLoader.Apply(this._root);
            this._root.pickingMode = PickingMode.Ignore;

            this._headerContainer                = new VisualElement();
            this._headerContainer.name           = "view-headers";
            this._headerContainer.pickingMode    = PickingMode.Ignore;
            this._headerContainer.style.position = Position.Absolute;
            this._headerContainer.style.left     = 0;
            this._headerContainer.style.top      = 0;
            this._headerContainer.style.right    = 0;
            this._headerContainer.style.bottom   = 0;
            this._root.Add(this._headerContainer);

            this._activeOutline                = new VisualElement();
            this._activeOutline.name           = "active-panel-outline";
            this._activeOutline.pickingMode    = PickingMode.Ignore;
            this._activeOutline.style.position = Position.Absolute;
            this._activeOutline.style.display  = DisplayStyle.None;
            this._activeOutline.AddToClassList("view-active-outline");
            this._root.Add(this._activeOutline);

            this.Rebuild();
            this._viewCameraManager.ViewSlotModel.Changed += this.Rebuild;
        }

        private void Update()
        {
            if (this._viewCameraManager == null)
            {
                return;
            }

            // Retry UI init if rootVisualElement wasn't ready in Start
            if (this._root == null)
            {
                this.InitializeUI();
                return;
            }

            this.PositionHeaders();
            this.PositionActiveOutline();
        }

        private void OnDestroy()
        {
            if (this._viewCameraManager != null)
            {
                this._viewCameraManager.ViewSlotModel.Changed -= this.Rebuild;
            }
        }

        private void Rebuild()
        {
            this.BuildHeaders();

            if (this._layoutChooser != null)
            {
                this._layoutChooser.RemoveFromHierarchy();
            }

            this.BuildLayoutChooser();
        }

        // ── Layout chooser ─────────────────────────────────────────────

        private void BuildLayoutChooser()
        {
            this._layoutChooser = new VisualElement();
            this._layoutChooser.AddToClassList("layout-chooser");
            var model = this._viewCameraManager.ViewSlotModel;

            this._layoutChooser.Add(this.CreateLayoutButton("\u25fb", ViewLayout.SINGLE,     model.Layout));
            this._layoutChooser.Add(this.CreateLayoutButton("\u25eb", ViewLayout.HORIZONTAL, model.Layout));
            this._layoutChooser.Add(this.CreateLayoutButton("\u229f", ViewLayout.VERTICAL,   model.Layout));
            this._root.Add(this._layoutChooser);
        }

        private Button CreateLayoutButton(string label, ViewLayout layout, ViewLayout currentLayout)
        {
            var btn = new Button(() => this._viewCameraManager.ViewSlotModel.SetLayout(layout));
            btn.text = label;
            btn.AddToClassList("layout-chooser__button");

            if (layout == currentLayout)
            {
                btn.AddToClassList("layout-chooser__button--active");
            }

            return btn;
        }

        // ── Per-viewport headers ────────────────────────────────────────

        private void BuildHeaders()
        {
            this._headerContainer.Clear();
            var model = this._viewCameraManager.ViewSlotModel;
            var count = model.ActiveSlotCount;

            this._viewHeaders = new ViewHeader[count];

            for (var i = 0; i < count; i++)
            {
                var header = new ViewHeader(i, model.GetSlotType(i), this._viewCameraManager);
                this._headerContainer.Add(header.Root);
                this._viewHeaders[i] = header;
            }
        }

        private void PositionActiveOutline()
        {
            if (!this._viewCameraManager.IsMultiView)
            {
                this._activeOutline.style.display = DisplayStyle.None;
                return;
            }

            this._activeOutline.style.display = DisplayStyle.Flex;

            var rootW = this._root.resolvedStyle.width;
            var rootH = this._root.resolvedStyle.height;

            if (float.IsNaN(rootW) || float.IsNaN(rootH))
            {
                return;
            }

            var activeSlot = this._viewCameraManager.ActiveSlot;
            var vpRect     = this._viewCameraManager.GetViewportRect(activeSlot);

            this._activeOutline.style.left   = vpRect.x * rootW;
            this._activeOutline.style.top    = (1f - vpRect.y - vpRect.height) * rootH;
            this._activeOutline.style.width  = vpRect.width  * rootW;
            this._activeOutline.style.height = vpRect.height * rootH;
        }

        private void PositionHeaders()
        {
            var rootW = this._root.resolvedStyle.width;
            var rootH = this._root.resolvedStyle.height;

            if (float.IsNaN(rootW) || float.IsNaN(rootH))
            {
                return;
            }

            var model = this._viewCameraManager.ViewSlotModel;
            var count = model.ActiveSlotCount;

            if (model.Layout == ViewLayout.SINGLE)
            {
                if (this._viewHeaders.Length > 0)
                {
                    var insetPixels = this._viewCameraManager.CameraBehaviour.RightInsetPixels;
                    var scale       = Screen.width > 0 ? rootW / Screen.width : 1f;

                    this._viewHeaders[0].Root.style.left  = 0;
                    this._viewHeaders[0].Root.style.top   = 0;
                    this._viewHeaders[0].Root.style.width = rootW - insetPixels * scale;
                }

                return;
            }

            for (var i = 0; i < this._viewHeaders.Length && i < count; i++)
            {
                var vpRect = this._viewCameraManager.GetViewportRect(i);

                this._viewHeaders[i].Root.style.left  = vpRect.x * rootW;
                this._viewHeaders[i].Root.style.top   = (1f - vpRect.y - vpRect.height) * rootH;
                this._viewHeaders[i].Root.style.width = vpRect.width * rootW;
            }
        }

        // ── ViewHeader ──────────────────────────────────────────────────

        private sealed class ViewHeader
        {
            private readonly Label             _label;
            private readonly VisualElement      _root;
            private readonly int               _slotIndex;
            private readonly ViewCameraManager _viewCameraManager;
            private          ViewMode          _viewMode;

            public ViewHeader(int slotIndex, ViewMode viewMode, ViewCameraManager viewCameraManager)
            {
                this._slotIndex         = slotIndex;
                this._viewMode          = viewMode;
                this._viewCameraManager = viewCameraManager;

                this._root                = new VisualElement();
                this._root.name           = $"view-header-{slotIndex}";
                this._root.style.position = Position.Absolute;
                this._root.style.height   = HEADER_HEIGHT;
                this._root.pickingMode    = PickingMode.Ignore;
                this._root.AddToClassList("view-panel__header");

                this._label             = new Label(viewMode.Name);
                this._label.pickingMode = PickingMode.Position;
                this._label.AddToClassList("view-panel__header-label");
                this._label.RegisterCallback<ClickEvent>(_ => this.ShowViewTypeMenu());
                this._root.Add(this._label);
            }

            public VisualElement Root => this._root;

            public void UpdateLabel(ViewMode mode)
            {
                this._viewMode  = mode;
                this._label.text = mode.Name;
            }

            private void ShowViewTypeMenu()
            {
                var menu  = new GenericDropdownMenu();
                var types = new[] { ViewMode.CAMERA, ViewMode.DIRECTOR };

                foreach (var type in types)
                {
                    var captured = type;
                    menu.AddItem(type.Name, type == this._viewMode, () =>
                    {
                        if (captured == this._viewMode)
                        {
                            return;
                        }

                        this._viewCameraManager.ViewSlotModel.SetSlotType(this._slotIndex, captured);
                        this._viewMode   = captured;
                        this._label.text = captured.Name;
                    });
                }

#pragma warning disable CS0618 // GenericDropdownMenu.DropDown overload deprecated
                menu.DropDown(this._label.worldBound, this._label, false, false);
#pragma warning restore CS0618
            }
        }
    }
}
