using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fram3d.UI.Panels
{
    /// <summary>
    /// A dropdown with an integrated search field that live-filters the options list.
    /// Arrow keys navigate the results, Enter confirms selection.
    /// Only one SearchableDropdown can be open at a time.
    /// </summary>
    public sealed class SearchableDropdown
    {
        private static SearchableDropdown _currentlyOpen;

        private List<string>  _allItems;
        private List<string>  _browseItems;
        private VisualElement _dropdownOverlay;
        private List<string>  _filteredItems;
        private int           _highlightedIndex = -1;
        private bool          _isOpen;
        private ScrollView    _scrollView;
        private TextField     _searchField;
        private int           _selectedIndex;
        private Label         _selectedLabel;
        private VisualElement _root;

        public event Action<int> SelectionChanged;

        public bool          HasFocus      => this._isOpen;
        public VisualElement Root          => this._root;
        public int           SelectedIndex => this._selectedIndex;

        public SearchableDropdown(List<string> items, int initialIndex, string placeholder)
        {
            this._allItems      = items;
            this._browseItems   = items;
            this._filteredItems = new List<string>(items);
            this._selectedIndex = Math.Clamp(initialIndex, 0, Math.Max(0, items.Count - 1));

            this._root = new VisualElement();
            this.BuildSelector();
            this.BuildOverlay(placeholder);
        }

        /// <summary>
        /// Sets which items are shown when the search field is empty (browse mode).
        /// Search always covers all items regardless of this filter.
        /// </summary>
        public void SetBrowseFilter(List<string> filteredItems)
        {
            this._browseItems = filteredItems;

            if (string.IsNullOrEmpty(this._searchField.value))
            {
                this._filteredItems.Clear();
                this._filteredItems.AddRange(this._browseItems);
                this.RebuildListItems();
            }
        }

        public void Close()
        {
            if (!this._isOpen)
                return;

            this._isOpen                        = false;
            this._dropdownOverlay.style.display = DisplayStyle.None;
            this._searchField.value             = "";
            this._highlightedIndex              = -1;

            this._filteredItems.Clear();
            this._filteredItems.AddRange(this._browseItems);

            if (_currentlyOpen == this)
                _currentlyOpen = null;
        }

        private void Open()
        {
            _currentlyOpen?.Close();

            this._isOpen                        = true;
            this._highlightedIndex              = -1;
            this._dropdownOverlay.style.display = DisplayStyle.Flex;

            this._filteredItems.Clear();
            this._filteredItems.AddRange(this._browseItems);
            this.RebuildListItems();

            this._searchField.Focus();
            _currentlyOpen = this;
        }

        private void BuildSelector()
        {
            var selector = new VisualElement();
            selector.style.flexDirection          = FlexDirection.Row;
            selector.style.backgroundColor        = new Color(0.18f, 0.18f, 0.18f);
            selector.style.borderBottomWidth      = 1;
            selector.style.borderTopWidth         = 1;
            selector.style.borderLeftWidth        = 1;
            selector.style.borderRightWidth       = 1;
            selector.style.borderBottomColor      = new Color(0.3f, 0.3f, 0.3f);
            selector.style.borderTopColor         = new Color(0.3f, 0.3f, 0.3f);
            selector.style.borderLeftColor        = new Color(0.3f, 0.3f, 0.3f);
            selector.style.borderRightColor       = new Color(0.3f, 0.3f, 0.3f);
            selector.style.borderBottomLeftRadius  = 3;
            selector.style.borderBottomRightRadius = 3;
            selector.style.borderTopLeftRadius     = 3;
            selector.style.borderTopRightRadius    = 3;
            selector.style.paddingLeft             = 6;
            selector.style.paddingRight            = 6;
            selector.style.paddingTop              = 4;
            selector.style.paddingBottom           = 4;

            var initialText = this._allItems.Count > 0 ? this._allItems[this._selectedIndex] : "—";
            this._selectedLabel = new Label(initialText);
            this._selectedLabel.style.fontSize = 11;
            this._selectedLabel.style.color    = new Color(0.8f, 0.8f, 0.8f);
            this._selectedLabel.style.flexGrow = 1;
            this._selectedLabel.style.overflow = Overflow.Hidden;

            var arrow = new Label("▾");
            arrow.style.fontSize = 10;
            arrow.style.color    = new Color(0.5f, 0.5f, 0.5f);

            selector.Add(this._selectedLabel);
            selector.Add(arrow);

            selector.RegisterCallback<ClickEvent>(_ =>
            {
                if (this._isOpen)
                    this.Close();
                else
                    this.Open();
            });

            this._root.Add(selector);
        }

        private void BuildOverlay(string placeholder)
        {
            this._dropdownOverlay = new VisualElement();
            this._dropdownOverlay.style.backgroundColor        = new Color(0.16f, 0.16f, 0.16f);
            this._dropdownOverlay.style.borderBottomWidth      = 1;
            this._dropdownOverlay.style.borderLeftWidth        = 1;
            this._dropdownOverlay.style.borderRightWidth       = 1;
            this._dropdownOverlay.style.borderBottomColor      = new Color(0.3f, 0.3f, 0.3f);
            this._dropdownOverlay.style.borderLeftColor        = new Color(0.3f, 0.3f, 0.3f);
            this._dropdownOverlay.style.borderRightColor       = new Color(0.3f, 0.3f, 0.3f);
            this._dropdownOverlay.style.borderBottomLeftRadius  = 3;
            this._dropdownOverlay.style.borderBottomRightRadius = 3;
            this._dropdownOverlay.style.display                = DisplayStyle.None;

            this._searchField = new TextField();
            this._searchField.style.marginLeft   = 4;
            this._searchField.style.marginRight  = 4;
            this._searchField.style.marginTop    = 4;
            this._searchField.style.marginBottom = 2;
            this._searchField.style.fontSize     = 11;
            this._searchField.textEdition.placeholder = placeholder;
            this._searchField.RegisterValueChangedCallback(this.OnSearchChanged);

            // Arrow keys + Enter — use TrickleDown so we capture before the text field consumes them
            this._searchField.RegisterCallback<KeyDownEvent>(this.OnKeyDown, TrickleDown.TrickleDown);

            // Close when focus leaves
            this._searchField.RegisterCallback<FocusOutEvent>(_ =>
                this._searchField.schedule.Execute(this.Close).ExecuteLater(150));

            this._dropdownOverlay.Add(this._searchField);

            this._scrollView = new ScrollView(ScrollViewMode.Vertical);
            this._scrollView.style.maxHeight = 200;
            this._dropdownOverlay.Add(this._scrollView);

            this._root.Add(this._dropdownOverlay);
        }

        /// <summary>
        /// Rebuilds the list items from _filteredItems. Uses simple VisualElements
        /// instead of ListView to avoid stale callback issues with bindItem.
        /// </summary>
        private void RebuildListItems()
        {
            this._scrollView.Clear();

            for (var i = 0; i < this._filteredItems.Count; i++)
            {
                var index = i;
                var row   = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems   = Align.Center;
                row.style.paddingLeft  = 6;
                row.style.paddingTop   = 3;
                row.style.paddingBottom = 3;

                var isHighlighted = i == this._highlightedIndex;

                row.style.backgroundColor = isHighlighted
                    ? new Color(0.2f, 0.4f, 0.7f, 0.6f)
                    : new StyleColor(StyleKeyword.Null);

                var label = new Label(this._filteredItems[i]);
                label.style.fontSize       = 11;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.flexGrow       = 1;

                // Highlighted row gets white text, others get gray
                label.style.color = isHighlighted
                    ? new Color(1f, 1f, 1f)
                    : new Color(0.75f, 0.75f, 0.75f);

                row.Add(label);

                row.RegisterCallback<PointerEnterEvent>(_ =>
                {
                    row.style.backgroundColor = new Color(0.2f, 0.4f, 0.7f, 0.4f);
                    label.style.color         = new Color(1f, 1f, 1f);
                });

                row.RegisterCallback<PointerLeaveEvent>(_ =>
                {
                    var highlighted = index == this._highlightedIndex;
                    row.style.backgroundColor = highlighted
                        ? new Color(0.2f, 0.4f, 0.7f, 0.6f)
                        : new StyleColor(StyleKeyword.Null);
                    label.style.color = highlighted
                        ? new Color(1f, 1f, 1f)
                        : new Color(0.75f, 0.75f, 0.75f);
                });

                row.RegisterCallback<ClickEvent>(_ => this.ConfirmSelection(index));
                this._scrollView.Add(row);
            }
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (this._filteredItems.Count == 0)
                return;

            switch (evt.keyCode)
            {
                case KeyCode.DownArrow:
                    this._highlightedIndex = Math.Min(this._highlightedIndex + 1, this._filteredItems.Count - 1);
                    this.RebuildListItems();
                    evt.StopPropagation();
                    evt.PreventDefault();

                    break;

                case KeyCode.UpArrow:
                    this._highlightedIndex = Math.Max(this._highlightedIndex - 1, 0);
                    this.RebuildListItems();
                    evt.StopPropagation();
                    evt.PreventDefault();

                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (this._highlightedIndex >= 0)
                    {
                        this.ConfirmSelection(this._highlightedIndex);
                        evt.StopPropagation();
                        evt.PreventDefault();
                    }

                    break;

                case KeyCode.Escape:
                    this.Close();
                    evt.StopPropagation();
                    evt.PreventDefault();

                    break;
            }
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            var search = evt.newValue;
            this._filteredItems.Clear();

            if (string.IsNullOrEmpty(search))
                this._filteredItems.AddRange(this._browseItems);
            else
                this._filteredItems.AddRange(
                    this._allItems.Where(item => item.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0));

            this._highlightedIndex = this._filteredItems.Count > 0 ? 0 : -1;
            this.RebuildListItems();
        }

        private void ConfirmSelection(int filteredIndex)
        {
            if (filteredIndex < 0 || filteredIndex >= this._filteredItems.Count)
                return;

            var selectedName = this._filteredItems[filteredIndex];
            this._selectedIndex      = this._allItems.IndexOf(selectedName);
            this._selectedLabel.text = selectedName;

            this.Close();
            this.SelectionChanged?.Invoke(this._selectedIndex);
        }
    }
}
