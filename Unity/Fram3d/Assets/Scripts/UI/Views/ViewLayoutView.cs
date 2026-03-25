using System;
using Fram3d.Core.Common;
using Fram3d.Core.Scene;
using Fram3d.Engine.Integration;
using Fram3d.UI.Panels;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fram3d.UI.Views
{
    /// <summary>
    /// Manages the view layout UI. Creates 1-3 view panels based on the
    /// ViewSlotModel, each displaying a RenderTexture from ViewCameraManager.
    /// Includes a layout chooser (bottom-right) and per-view type dropdowns.
    /// Tracks which view the mouse is over for input routing.
    /// </summary>
    public sealed class ViewLayoutView : MonoBehaviour
    {
        private const int    HEADER_HEIGHT    = 22;
        private const int    VIEW_GAP         = 2;
        private const string VIEW_PANEL_CLASS = "view-panel";

        private int               _activeSlot = 0;
        private VisualElement     _layoutChooser;
        private VisualElement     _root;
        private ViewCameraManager _viewCameraManager;
        private VisualElement     _viewContainer;
        private ViewPanel[]       _viewPanels = Array.Empty<ViewPanel>();

        /// <summary>
        /// Which slot the mouse is currently over. -1 if not over any view.
        /// Used by input handlers to route to the correct camera.
        /// </summary>
        public int ActiveSlot => this._activeSlot;

        /// <summary>
        /// Fires when the active (mouse-hovered) slot changes. Input handlers
        /// subscribe to update their camera references.
        /// </summary>
        public event Action<int> ActiveSlotChanged;

        private void Start()
        {
            this._viewCameraManager = FindAnyObjectByType<ViewCameraManager>();

            if (this._viewCameraManager == null)
            {
                Debug.LogWarning("ViewLayoutView: No ViewCameraManager found.");
                return;
            }

            var uiDocument = this.GetComponent<UIDocument>();

            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogWarning("ViewLayoutView: UIDocument or rootVisualElement is null.");
                return;
            }

            this._root = uiDocument.rootVisualElement;
            StyleSheetLoader.Apply(this._root);
            this._root.pickingMode = PickingMode.Ignore;
            this.BuildLayout();
            this._viewCameraManager.ViewSlotModel.Changed += this.RebuildLayout;
        }

        private void Update()
        {
            if (this._viewCameraManager == null)
            {
                return;
            }

            this.UpdateRenderTextures();
        }

        private void OnDestroy()
        {
            if (this._viewCameraManager != null)
            {
                this._viewCameraManager.ViewSlotModel.Changed -= this.RebuildLayout;
            }
        }

        private void BuildLayout()
        {
            this._root.Clear();
            this._viewContainer             = new VisualElement();
            this._viewContainer.name        = "view-container";
            this._viewContainer.pickingMode = PickingMode.Ignore;
            this._viewContainer.AddToClassList("view-container");
            this._root.Add(this._viewContainer);
            this.BuildViewPanels();
            this.BuildLayoutChooser();
        }

        private void BuildLayoutChooser()
        {
            this._layoutChooser = new VisualElement();
            this._layoutChooser.AddToClassList("layout-chooser");
            var model = this._viewCameraManager.ViewSlotModel;

            var singleBtn    = this.CreateLayoutButton("◻",  ViewLayout.SINGLE,       model.Layout);
            var sideBySideBtn = this.CreateLayoutButton("◫", ViewLayout.SIDE_BY_SIDE, model.Layout);
            var onePlusTwoBtn = this.CreateLayoutButton("⊞", ViewLayout.ONE_PLUS_TWO, model.Layout);

            this._layoutChooser.Add(singleBtn);
            this._layoutChooser.Add(sideBySideBtn);
            this._layoutChooser.Add(onePlusTwoBtn);
            this._root.Add(this._layoutChooser);
        }

        private void BuildViewPanels()
        {
            this._viewContainer.Clear();
            var model = this._viewCameraManager.ViewSlotModel;
            var count = model.ActiveSlotCount;
            this._viewPanels = new ViewPanel[count];

            if (count == 1)
            {
                this.BuildSingleLayout(model);
            }
            else if (count == 2)
            {
                this.BuildSideBySideLayout(model);
            }
            else
            {
                this.BuildOnePlusTwoLayout(model);
            }

            // Clamp active slot to new count
            var newCount = model.ActiveSlotCount;

            if (this._activeSlot >= newCount)
            {
                this._activeSlot = 0;
                this._viewCameraManager.SetActiveSlot(0);
                this.ActiveSlotChanged?.Invoke(0);
            }
        }

        private void BuildSingleLayout(ViewSlotModel model)
        {
            var panel = this.CreateViewPanel(0, model.GetSlotType(0));
            panel.Root.style.flexGrow = 1;
            this._viewContainer.style.flexDirection = FlexDirection.Row;
            this._viewContainer.Add(panel.Root);
            this._viewPanels[0] = panel;
        }

        private void BuildSideBySideLayout(ViewSlotModel model)
        {
            this._viewContainer.style.flexDirection = FlexDirection.Row;

            for (var i = 0; i < 2; i++)
            {
                var panel = this.CreateViewPanel(i, model.GetSlotType(i));
                panel.Root.style.flexGrow  = 1;
                panel.Root.style.flexBasis = new StyleLength(new Length(50, LengthUnit.Percent));

                if (i == 0)
                {
                    panel.Root.style.marginRight = VIEW_GAP;
                }

                this._viewContainer.Add(panel.Root);
                this._viewPanels[i] = panel;
            }
        }

        private void BuildOnePlusTwoLayout(ViewSlotModel model)
        {
            this._viewContainer.style.flexDirection = FlexDirection.Column;

            // Top row: large view
            var topPanel = this.CreateViewPanel(0, model.GetSlotType(0));
            topPanel.Root.style.flexGrow   = 1;
            topPanel.Root.style.flexBasis  = new StyleLength(new Length(60, LengthUnit.Percent));
            topPanel.Root.style.marginBottom = VIEW_GAP;
            this._viewContainer.Add(topPanel.Root);
            this._viewPanels[0] = topPanel;

            // Bottom row: two smaller views
            var bottomRow = new VisualElement();
            bottomRow.name                  = "bottom-row";
            bottomRow.pickingMode           = PickingMode.Ignore;
            bottomRow.style.flexDirection   = FlexDirection.Row;
            bottomRow.style.flexGrow        = 1;
            bottomRow.style.flexBasis       = new StyleLength(new Length(40, LengthUnit.Percent));

            for (var i = 1; i < 3; i++)
            {
                var panel = this.CreateViewPanel(i, model.GetSlotType(i));
                panel.Root.style.flexGrow  = 1;
                panel.Root.style.flexBasis = new StyleLength(new Length(50, LengthUnit.Percent));

                if (i == 1)
                {
                    panel.Root.style.marginRight = VIEW_GAP;
                }

                bottomRow.Add(panel.Root);
                this._viewPanels[i] = panel;
            }

            this._viewContainer.Add(bottomRow);
        }

        private ViewPanel CreateViewPanel(int slotIndex, ViewMode viewMode)
        {
            var panel = new ViewPanel(slotIndex, viewMode);

            panel.Root.RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (this._activeSlot != slotIndex)
                {
                    this._activeSlot = slotIndex;
                    this._viewCameraManager.SetActiveSlot(slotIndex);
                    this.ActiveSlotChanged?.Invoke(slotIndex);
                }
            });

            panel.ViewTypeChanged += (slot, newType) =>
            {
                this._viewCameraManager.ViewSlotModel.SetSlotType(slot, newType);
            };

            return panel;
        }

        private Button CreateLayoutButton(string label, ViewLayout layout, ViewLayout currentLayout)
        {
            var btn = new Button(() =>
            {
                this._viewCameraManager.ViewSlotModel.SetLayout(layout);
            });

            btn.text = label;
            btn.AddToClassList("layout-chooser__button");

            if (layout == currentLayout)
            {
                btn.AddToClassList("layout-chooser__button--active");
            }

            return btn;
        }

        private void RebuildLayout()
        {
            this.BuildViewPanels();

            // Rebuild layout chooser to update active state
            if (this._layoutChooser != null)
            {
                this._layoutChooser.RemoveFromHierarchy();
            }

            this.BuildLayoutChooser();

            // Trigger RT resize on next frame
            this._root.schedule.Execute(() => this.UpdateRenderTextures()).ExecuteLater(50);
        }

        private void UpdateRenderTextures()
        {
            for (var i = 0; i < this._viewPanels.Length; i++)
            {
                var panel = this._viewPanels[i];

                if (panel == null)
                {
                    continue;
                }

                var mode = this._viewCameraManager.ViewSlotModel.GetSlotType(i);

                if (mode == ViewMode.DESIGNER)
                {
                    continue;
                }

                var contentWidth  = (int)panel.ContentArea.resolvedStyle.width;
                var contentHeight = (int)panel.ContentArea.resolvedStyle.height;

                if (contentWidth <= 0 || contentHeight <= 0 || float.IsNaN(panel.ContentArea.resolvedStyle.width))
                {
                    continue;
                }

                this._viewCameraManager.ResizeSlot(i, contentWidth, contentHeight);
                var rt = this._viewCameraManager.GetRenderTexture(i);

                if (rt != null)
                {
                    panel.ContentArea.style.backgroundImage = Background.FromRenderTexture(rt);
                }
            }
        }

        /// <summary>
        /// Represents a single view panel in the layout — header bar with
        /// view type dropdown + content area showing the RenderTexture.
        /// </summary>
        private sealed class ViewPanel
        {
            private readonly VisualElement _contentArea;
            private readonly VisualElement _header;
            private readonly Label         _headerLabel;
            private readonly VisualElement _placeholder;
            private readonly VisualElement _root;
            private readonly int           _slotIndex;
            private          ViewMode      _viewMode;

            public ViewPanel(int slotIndex, ViewMode viewMode)
            {
                this._slotIndex = slotIndex;
                this._viewMode  = viewMode;

                this._root             = new VisualElement();
                this._root.name        = $"view-panel-{slotIndex}";
                this._root.pickingMode = PickingMode.Position;
                this._root.AddToClassList(VIEW_PANEL_CLASS);

                // Header bar with view type selector
                this._header = new VisualElement();
                this._header.AddToClassList("view-panel__header");

                this._headerLabel = new Label(viewMode.Name);
                this._headerLabel.AddToClassList("view-panel__header-label");
                this._header.Add(this._headerLabel);
                this.BuildDropdownButton();
                this._root.Add(this._header);

                // Content area
                this._contentArea             = new VisualElement();
                this._contentArea.name        = $"view-content-{slotIndex}";
                this._contentArea.pickingMode = PickingMode.Ignore;
                this._contentArea.AddToClassList("view-panel__content");
                this._root.Add(this._contentArea);

                // Placeholder for Designer View
                this._placeholder = new VisualElement();
                this._placeholder.AddToClassList("view-panel__placeholder");
                var placeholderLabel = new Label("Designer View");
                placeholderLabel.AddToClassList("view-panel__placeholder-title");
                var placeholderHint = new Label("Top-down scene layout (coming soon)");
                placeholderHint.AddToClassList("view-panel__placeholder-hint");
                this._placeholder.Add(placeholderLabel);
                this._placeholder.Add(placeholderHint);
                this._contentArea.Add(this._placeholder);
                this.UpdatePlaceholderVisibility();
            }

            public VisualElement ContentArea => this._contentArea;
            public VisualElement Root        => this._root;

            public event Action<int, ViewMode> ViewTypeChanged;

            private void BuildDropdownButton()
            {
                var dropdownBtn = new Button();
                dropdownBtn.text = "▾";
                dropdownBtn.AddToClassList("view-panel__dropdown-btn");
                dropdownBtn.clicked += () => this.ShowViewTypeMenu(dropdownBtn);
                this._header.Add(dropdownBtn);
            }

            private void ShowViewTypeMenu(VisualElement anchor)
            {
                var menu = new GenericDropdownMenu();
                var types = new[] { ViewMode.CAMERA, ViewMode.DIRECTOR, ViewMode.DESIGNER };

                foreach (var type in types)
                {
                    var captured = type;
                    menu.AddItem(type.Name, type == this._viewMode, () =>
                    {
                        if (captured != this._viewMode)
                        {
                            this._viewMode        = captured;
                            this._headerLabel.text = captured.Name;
                            this.UpdatePlaceholderVisibility();
                            this.ViewTypeChanged?.Invoke(this._slotIndex, captured);
                        }
                    });
                }

                menu.DropDown(anchor.worldBound, anchor, false);
            }

            private void UpdatePlaceholderVisibility()
            {
                this._placeholder.style.display = this._viewMode == ViewMode.DESIGNER
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                this._contentArea.style.backgroundImage = this._viewMode == ViewMode.DESIGNER
                    ? new StyleBackground(StyleKeyword.None)
                    : this._contentArea.style.backgroundImage;
            }
        }
    }
}
