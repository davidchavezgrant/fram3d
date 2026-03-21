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

        private readonly List<string>    _allItems;
        private          List<string>    _browseItems;
        private readonly VisualElement   _dropdownOverlay;
        private readonly List<string>    _filteredItems;
        private readonly ListView        _listView;
        private readonly VisualElement   _root;
        private readonly TextField       _searchField;
        private readonly Label           _selectedLabel;
        private          int             _highlightedIndex = -1;
        private          bool            _isOpen;
        private          int             _selectedIndex;

        public event Action<int> SelectionChanged;

        public bool            HasFocus      => this._isOpen;
        public VisualElement   Root          => this._root;
        public int             SelectedIndex => this._selectedIndex;

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
                this._listView.RefreshItems();
            }
        }

        public SearchableDropdown(List<string> items, int initialIndex, string placeholder)
        {
            this._allItems      = items;
            this._browseItems   = items;
            this._filteredItems = new List<string>(items);
            this._selectedIndex = Math.Clamp(initialIndex, 0, Math.Max(0, items.Count - 1));

            this._root = new VisualElement();
            this.BuildSelector(items);
            this.BuildOverlay(placeholder);
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
            this._listView.RefreshItems();

            if (_currentlyOpen == this)
                _currentlyOpen = null;
        }

        private void Open()
        {
            // Close any other open dropdown instantly
            _currentlyOpen?.Close();

            this._isOpen                        = true;
            this._dropdownOverlay.style.display = DisplayStyle.Flex;
            this._searchField.Focus();
            _currentlyOpen = this;
        }

        private void BuildSelector(List<string> items)
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

            this._selectedLabel = new Label(items.Count > 0 ? items[this._selectedIndex] : "—");
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

            // Keyboard navigation: arrow keys + enter
            this._searchField.RegisterCallback<KeyDownEvent>(this.OnKeyDown);

            // Close when focus leaves the entire dropdown
            this._searchField.RegisterCallback<FocusOutEvent>(_ =>
                this._searchField.schedule.Execute(this.Close).ExecuteLater(150));

            this._dropdownOverlay.Add(this._searchField);
            this.BuildListView();
            this._dropdownOverlay.Add(this._listView);
            this._root.Add(this._dropdownOverlay);
        }

        private void BuildListView()
        {
            this._listView = new ListView();
            this._listView.style.maxHeight = 200;
            this._listView.style.flexGrow  = 1;
            this._listView.itemsSource     = this._filteredItems;
            this._listView.fixedItemHeight = 22;
            this._listView.selectionType   = SelectionType.None;

            this._listView.makeItem = () =>
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems   = Align.Center;
                row.style.paddingLeft  = 6;

                var label = new Label();
                label.style.fontSize       = 11;
                label.style.color          = new Color(0.75f, 0.75f, 0.75f);
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.flexGrow       = 1;

                row.Add(label);
                return row;
            };

            this._listView.bindItem = (element, index) =>
            {
                var label       = element.Q<Label>();
                label.text      = this._filteredItems[index];
                var isHighlighted = index == this._highlightedIndex;

                element.style.backgroundColor = isHighlighted
                    ? new Color(0.2f, 0.4f, 0.7f, 0.4f)
                    : new StyleColor(StyleKeyword.Null);

                // Click to select
                element.RegisterCallback<ClickEvent>(_ => this.ConfirmSelection(index));
            };
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (this._filteredItems.Count == 0)
                return;

            switch (evt.keyCode)
            {
                case KeyCode.DownArrow:
                    this._highlightedIndex = Math.Min(this._highlightedIndex + 1, this._filteredItems.Count - 1);
                    this._listView.RefreshItems();
                    this._listView.ScrollToItem(this._highlightedIndex);
                    evt.StopPropagation();
                    evt.PreventDefault();

                    break;

                case KeyCode.UpArrow:
                    this._highlightedIndex = Math.Max(this._highlightedIndex - 1, 0);
                    this._listView.RefreshItems();
                    this._listView.ScrollToItem(this._highlightedIndex);
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
                // Search always covers ALL items, not just the browse-filtered subset
                this._filteredItems.AddRange(
                    this._allItems.Where(item => item.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0));

            this._highlightedIndex = this._filteredItems.Count > 0 ? 0 : -1;
            this._listView.RefreshItems();
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
