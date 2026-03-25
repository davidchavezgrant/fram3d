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
    /// Renders the layout chooser and per-viewport header bars. In multi-view,
    /// each viewport gets a small header showing the view type with a dropdown
    /// to change it. Headers are positioned to match Camera.rect viewports.
    /// All elements use pickingMode = Ignore except interactive controls.
    /// </summary>
    public sealed class ViewLayoutView : MonoBehaviour
    {
        private const int HEADER_HEIGHT = 22;

        private VisualElement     _headerContainer;
        private VisualElement     _layoutChooser;
        private VisualElement     _root;
        private ViewCameraManager _viewCameraManager;
        private ViewHeader[]      _viewHeaders = Array.Empty<ViewHeader>();

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

            this._headerContainer             = new VisualElement();
            this._headerContainer.name        = "view-headers";
            this._headerContainer.pickingMode = PickingMode.Ignore;
            this._headerContainer.style.position = Position.Absolute;
            this._headerContainer.style.left     = 0;
            this._headerContainer.style.top      = 0;
            this._headerContainer.style.right    = 0;
            this._headerContainer.style.bottom   = 0;
            this._root.Add(this._headerContainer);

            this.BuildLayoutChooser();
            this._viewCameraManager.ViewSlotModel.Changed += this.Rebuild;
        }

        private void Update()
        {
            if (this._viewCameraManager == null || !this._viewCameraManager.IsMultiView)
            {
                return;
            }

            this.PositionHeaders();
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

            this._layoutChooser.Add(this.CreateLayoutButton("\u25fb", ViewLayout.SINGLE,       model.Layout));
            this._layoutChooser.Add(this.CreateLayoutButton("\u25eb", ViewLayout.SIDE_BY_SIDE, model.Layout));
            this._layoutChooser.Add(this.CreateLayoutButton("\u229e", ViewLayout.ONE_PLUS_TWO, model.Layout));
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

            if (model.Layout == ViewLayout.SINGLE)
            {
                this._viewHeaders = Array.Empty<ViewHeader>();
                return;
            }

            var count = model.ActiveSlotCount;
            this._viewHeaders = new ViewHeader[count];

            for (var i = 0; i < count; i++)
            {
                var header = new ViewHeader(i, model.GetSlotType(i), this._viewCameraManager);
                this._headerContainer.Add(header.Root);
                this._viewHeaders[i] = header;
            }
        }

        private void PositionHeaders()
        {
            var screenWidth  = (float)Screen.width;
            var screenHeight = (float)Screen.height;
            var count        = this._viewCameraManager.ViewSlotModel.ActiveSlotCount;

            for (var i = 0; i < this._viewHeaders.Length && i < count; i++)
            {
                var vpRect = this._viewCameraManager.GetViewportRect(i);
                var left   = vpRect.x * screenWidth;
                var top    = (1f - vpRect.y - vpRect.height) * screenHeight;
                var w      = vpRect.width * screenWidth;

                this._viewHeaders[i].Root.style.left  = left;
                this._viewHeaders[i].Root.style.top   = top;
                this._viewHeaders[i].Root.style.width = w;
            }
        }

        // ── ViewHeader ──────────────────────────────────────────────────

        private sealed class ViewHeader
        {
            private readonly Label              _label;
            private readonly VisualElement       _root;
            private readonly int                _slotIndex;
            private readonly ViewCameraManager  _viewCameraManager;
            private          ViewMode           _viewMode;

            public ViewHeader(int slotIndex, ViewMode viewMode, ViewCameraManager viewCameraManager)
            {
                this._slotIndex         = slotIndex;
                this._viewMode          = viewMode;
                this._viewCameraManager = viewCameraManager;

                this._root                  = new VisualElement();
                this._root.name             = $"view-header-{slotIndex}";
                this._root.style.position   = Position.Absolute;
                this._root.style.height     = HEADER_HEIGHT;
                this._root.pickingMode      = PickingMode.Ignore;
                this._root.AddToClassList("view-panel__header");

                this._label = new Label(viewMode.Name);
                this._label.AddToClassList("view-panel__header-label");
                this._label.pickingMode = PickingMode.Ignore;
                this._root.Add(this._label);

                var dropdownBtn = new Button();
                dropdownBtn.text = "\u25be";
                dropdownBtn.AddToClassList("view-panel__dropdown-btn");
                dropdownBtn.clicked += () => this.ShowViewTypeMenu(dropdownBtn);
                this._root.Add(dropdownBtn);
            }

            public VisualElement Root => this._root;

            private void ShowViewTypeMenu(VisualElement anchor)
            {
                var menu  = new GenericDropdownMenu();
                var types = new[] { ViewMode.CAMERA, ViewMode.DIRECTOR, ViewMode.DESIGNER };

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
                        this._viewMode  = captured;
                        this._label.text = captured.Name;
                    });
                }

                menu.DropDown(anchor.worldBound, anchor, false);
            }
        }
    }
}
