using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fram3d.UI.Panels
{
    /// <summary>
    /// A dropdown with an integrated search field that live-filters the options list.
    /// Built with UI Toolkit. Replaces PopupField + separate TextField pattern.
    /// </summary>
    public sealed class SearchableDropdown
    {
        private readonly VisualElement   _root;
        private readonly TextField       _searchField;
        private readonly ListView        _listView;
        private readonly VisualElement   _dropdownOverlay;
        private readonly List<string>    _allItems;
        private readonly List<string>    _filteredItems;
        private readonly Label           _selectedLabel;
        private          int             _selectedIndex;
        private          bool            _isOpen;

        public event Action<int> SelectionChanged;

        /// <summary>
        /// Whether the search field currently has keyboard focus.
        /// </summary>
        public bool HasFocus => this._searchField.focusController?.focusedElement == this._searchField;

        public int SelectedIndex => this._selectedIndex;

        public SearchableDropdown(List<string> items, int initialIndex, string placeholder)
        {
            this._allItems      = items;
            this._filteredItems = new List<string>(items);
            this._selectedIndex = Math.Clamp(initialIndex, 0, Math.Max(0, items.Count - 1));

            this._root = new VisualElement();

            // Selected value display (click to open)
            var selector = new VisualElement();
            selector.style.flexDirection   = FlexDirection.Row;
            selector.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
            selector.style.borderBottomWidth = 1;
            selector.style.borderTopWidth    = 1;
            selector.style.borderLeftWidth   = 1;
            selector.style.borderRightWidth  = 1;
            selector.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
            selector.style.borderTopColor    = new Color(0.3f, 0.3f, 0.3f);
            selector.style.borderLeftColor   = new Color(0.3f, 0.3f, 0.3f);
            selector.style.borderRightColor  = new Color(0.3f, 0.3f, 0.3f);
            selector.style.borderBottomLeftRadius  = 3;
            selector.style.borderBottomRightRadius = 3;
            selector.style.borderTopLeftRadius     = 3;
            selector.style.borderTopRightRadius    = 3;
            selector.style.paddingLeft   = 6;
            selector.style.paddingRight  = 6;
            selector.style.paddingTop    = 4;
            selector.style.paddingBottom = 4;

            this._selectedLabel = new Label(items.Count > 0 ? items[this._selectedIndex] : "—");
            this._selectedLabel.style.fontSize  = 11;
            this._selectedLabel.style.color     = new Color(0.8f, 0.8f, 0.8f);
            this._selectedLabel.style.flexGrow  = 1;
            this._selectedLabel.style.overflow  = Overflow.Hidden;

            var arrow = new Label("▾");
            arrow.style.fontSize = 10;
            arrow.style.color    = new Color(0.5f, 0.5f, 0.5f);

            selector.Add(this._selectedLabel);
            selector.Add(arrow);
            selector.RegisterCallback<ClickEvent>(_ => this.ToggleDropdown());

            this._root.Add(selector);

            // Dropdown overlay (search + list)
            this._dropdownOverlay = new VisualElement();
            this._dropdownOverlay.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
            this._dropdownOverlay.style.borderBottomWidth = 1;
            this._dropdownOverlay.style.borderLeftWidth   = 1;
            this._dropdownOverlay.style.borderRightWidth  = 1;
            this._dropdownOverlay.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
            this._dropdownOverlay.style.borderLeftColor   = new Color(0.3f, 0.3f, 0.3f);
            this._dropdownOverlay.style.borderRightColor  = new Color(0.3f, 0.3f, 0.3f);
            this._dropdownOverlay.style.borderBottomLeftRadius  = 3;
            this._dropdownOverlay.style.borderBottomRightRadius = 3;
            this._dropdownOverlay.style.display = DisplayStyle.None;

            this._searchField = new TextField();
            this._searchField.style.marginLeft   = 4;
            this._searchField.style.marginRight  = 4;
            this._searchField.style.marginTop    = 4;
            this._searchField.style.marginBottom = 2;
            this._searchField.style.fontSize     = 11;
            this._searchField.textEdition.placeholder = placeholder;
            this._searchField.RegisterValueChangedCallback(this.OnSearchChanged);

            this._dropdownOverlay.Add(this._searchField);

            this._listView = new ListView();
            this._listView.style.maxHeight = 200;
            this._listView.style.flexGrow  = 1;
            this._listView.itemsSource     = this._filteredItems;
            this._listView.fixedItemHeight = 22;

            this._listView.makeItem = () =>
            {
                var label = new Label();
                label.style.fontSize    = 11;
                label.style.color       = new Color(0.75f, 0.75f, 0.75f);
                label.style.paddingLeft = 6;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;

                return label;
            };

            this._listView.bindItem = (element, index) =>
            {
                ((Label)element).text = this._filteredItems[index];
            };

            this._listView.selectedIndicesChanged += _ => this.OnItemSelected();

            this._dropdownOverlay.Add(this._listView);
            this._root.Add(this._dropdownOverlay);
        }

        public VisualElement Root => this._root;

        private void ToggleDropdown()
        {
            this._isOpen = !this._isOpen;
            this._dropdownOverlay.style.display = this._isOpen ? DisplayStyle.Flex : DisplayStyle.None;

            if (this._isOpen)
                this._searchField.Focus();
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            var search = evt.newValue;
            this._filteredItems.Clear();

            if (string.IsNullOrEmpty(search))
            {
                this._filteredItems.AddRange(this._allItems);
            }
            else
            {
                this._filteredItems.AddRange(
                    this._allItems.Where(item => item.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            this._listView.RefreshItems();
        }

        private void OnItemSelected()
        {
            var listIndex = this._listView.selectedIndex;

            if (listIndex < 0 || listIndex >= this._filteredItems.Count)
                return;

            var selectedName = this._filteredItems[listIndex];
            this._selectedIndex = this._allItems.IndexOf(selectedName);
            this._selectedLabel.text = selectedName;

            // Close dropdown
            this._isOpen = false;
            this._dropdownOverlay.style.display = DisplayStyle.None;
            this._searchField.value = "";
            this._filteredItems.Clear();
            this._filteredItems.AddRange(this._allItems);
            this._listView.RefreshItems();

            this.SelectionChanged?.Invoke(this._selectedIndex);
        }
    }
}
