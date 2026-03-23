using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Panels
{
    /// <summary>
    /// A dropdown with an integrated search field that live-filters the options list.
    /// Arrow keys navigate the results, Enter confirms selection, Escape closes.
    /// Only one SearchableDropdown can be open at a time.
    /// </summary>
    public sealed class SearchableDropdown
    {
        private static SearchableDropdown _currentlyOpen;
        private        List<string>       _allItems;
        private        List<string>       _browseItems;
        private        VisualElement      _dropdownOverlay;
        private        List<string>       _filteredItems;
        private        int                _highlightedIndex = -1;
        private        bool               _isOpen;
        private        VisualElement      _root;
        private        ScrollView         _scrollView;
        private        TextField          _searchField;
        private        int                _selectedIndex;
        private        Label              _selectedLabel;

        public SearchableDropdown(List<string> items, int initialIndex, string placeholder)
        {
            this._allItems      = items;
            this._browseItems   = items;
            this._filteredItems = new List<string>(items);
            this._selectedIndex = Math.Clamp(initialIndex, 0, Math.Max(0, items.Count - 1));
            this._root          = new VisualElement();
            this.BuildSelector();
            this.BuildOverlay(placeholder);
        }

        public event Action<int> SelectionChanged;
        public bool              HasFocus      => this._isOpen;
        public VisualElement     Root          => this._root;
        public int               SelectedIndex => this._selectedIndex;

        public void Close()
        {
            if (!this._isOpen)
            {
                return;
            }

            this._isOpen                        = false;
            this._dropdownOverlay.style.display = DisplayStyle.None;
            this._searchField.value             = "";
            this._highlightedIndex              = -1;
            this._filteredItems.Clear();
            this._filteredItems.AddRange(this._browseItems);

            if (_currentlyOpen == this)
                _currentlyOpen = null;
        }

        /// <summary>
        /// Sets which items are shown when the search field is empty (browse mode).
        /// Search always covers all items regardless of this filter.
        /// </summary>
        public void SetBrowseFilter(List<string> filteredItems)
        {
            this._browseItems = filteredItems;

            if (!string.IsNullOrEmpty(this._searchField.value))
            {
                return;
            }

            this._filteredItems.Clear();
            this._filteredItems.AddRange(this._browseItems);
            this.RebuildListItems();
        }

        // --- Dropdown overlay (search + scrollable list) ---

        private void BuildOverlay(string placeholder)
        {
            this._dropdownOverlay                               = new VisualElement();
            this._dropdownOverlay.style.backgroundColor         = Theme.DROPDOWN_BG;
            this._dropdownOverlay.style.borderBottomWidth       = 1;
            this._dropdownOverlay.style.borderLeftWidth         = 1;
            this._dropdownOverlay.style.borderRightWidth        = 1;
            this._dropdownOverlay.style.borderBottomColor       = Theme.SELECTOR_BORDER;
            this._dropdownOverlay.style.borderLeftColor         = Theme.SELECTOR_BORDER;
            this._dropdownOverlay.style.borderRightColor        = Theme.SELECTOR_BORDER;
            this._dropdownOverlay.style.borderBottomLeftRadius  = 3;
            this._dropdownOverlay.style.borderBottomRightRadius = 3;
            this._dropdownOverlay.style.display                 = DisplayStyle.None;
            this._searchField                                   = new TextField();
            this._searchField.style.marginLeft                  = 4;
            this._searchField.style.marginRight                 = 4;
            this._searchField.style.marginTop                   = 4;
            this._searchField.style.marginBottom                = 2;
            this._searchField.style.fontSize                    = Theme.FONT_BODY;
            this._searchField.textEdition.placeholder           = placeholder;
            this._searchField.RegisterValueChangedCallback(this.OnSearchChanged);
            this._searchField.RegisterCallback<KeyDownEvent>(this.OnKeyDown, TrickleDown.TrickleDown);
            this._searchField.RegisterCallback<FocusOutEvent>(_ => this._searchField.schedule.Execute(this.Close).ExecuteLater(150));
            this._dropdownOverlay.Add(this._searchField);
            this._scrollView                 = new ScrollView(ScrollViewMode.Vertical);
            this._scrollView.style.maxHeight = 200;
            this._dropdownOverlay.Add(this._scrollView);
            this._root.Add(this._dropdownOverlay);
        }

        // --- Selector bar (click to open) ---

        private void BuildSelector()
        {
            var selector = new VisualElement();
            StyleSelector(selector);
            var initialText = this._allItems.Count > 0? this._allItems[this._selectedIndex] : "—";
            this._selectedLabel                = new Label(initialText);
            this._selectedLabel.style.fontSize = Theme.FONT_BODY;
            this._selectedLabel.style.color    = Theme.TEXT_LIGHT;
            this._selectedLabel.style.flexGrow = 1;
            this._selectedLabel.style.overflow = Overflow.Hidden;
            var arrow = new Label("▾");
            arrow.style.fontSize = Theme.FONT_HEADER;
            arrow.style.color    = Theme.LABEL_MID;
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

        // --- Selection ---

        private void ConfirmSelection(int filteredIndex)
        {
            if (filteredIndex < 0 || filteredIndex >= this._filteredItems.Count)
            {
                return;
            }

            var selectedName = this._filteredItems[filteredIndex];
            this._selectedIndex      = this._allItems.IndexOf(selectedName);
            this._selectedLabel.text = selectedName;
            this.Close();
            this.SelectionChanged?.Invoke(this._selectedIndex);
        }

        private VisualElement CreateListRow(string text, int index, bool isHighlighted)
        {
            var row = new VisualElement();
            row.style.flexDirection   = FlexDirection.Row;
            row.style.alignItems      = Align.Center;
            row.style.paddingLeft     = 6;
            row.style.paddingTop      = 3;
            row.style.paddingBottom   = 3;
            row.style.backgroundColor = isHighlighted? Theme.HIGHLIGHT_STRONG : new StyleColor(StyleKeyword.Null);
            var label = new Label(text);
            label.style.fontSize       = Theme.FONT_BODY;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.flexGrow       = 1;
            label.style.color          = isHighlighted? Theme.TEXT_WHITE : Theme.TEXT_DEFAULT;
            row.Add(label);

            row.RegisterCallback<PointerEnterEvent>(_ =>
                                                    {
                                                        row.style.backgroundColor = Theme.HIGHLIGHT;
                                                        label.style.color         = Theme.TEXT_WHITE;
                                                    });

            row.RegisterCallback<PointerLeaveEvent>(_ =>
                                                    {
                                                        var highlighted = index == this._highlightedIndex;

                                                        row.style.backgroundColor =
                                                            highlighted? Theme.HIGHLIGHT_STRONG : new StyleColor(StyleKeyword.Null);

                                                        label.style.color = highlighted? Theme.TEXT_WHITE : Theme.TEXT_DEFAULT;
                                                    });

            row.RegisterCallback<ClickEvent>(_ => this.ConfirmSelection(index));
            return row;
        }

        // --- Keyboard navigation ---

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
                    break;

                case KeyCode.UpArrow:
                    this._highlightedIndex = Math.Max(this._highlightedIndex - 1, 0);
                    this.RebuildListItems();
                    evt.StopPropagation();
                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (this._highlightedIndex >= 0)
                    {
                        this.ConfirmSelection(this._highlightedIndex);
                        evt.StopPropagation();
                    }

                    break;

                case KeyCode.Escape:
                    this.Close();
                    evt.StopPropagation();
                    break;
            }
        }

        // --- Search ---

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            var search = evt.newValue;
            this._filteredItems.Clear();

            if (string.IsNullOrEmpty(search))
            {
                this._filteredItems.AddRange(this._browseItems);
            }
            else
            {
                var words = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                this._filteredItems.AddRange(this._allItems.Where(item => words.All(word => item.IndexOf(word, StringComparison.OrdinalIgnoreCase)
                                                                                         >= 0)));
            }

            this._highlightedIndex = this._filteredItems.Count > 0? 0 : -1;
            this.RebuildListItems();
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

        // --- List items ---

        private void RebuildListItems()
        {
            this._scrollView.Clear();

            for (var i = 0; i < this._filteredItems.Count; i++)
            {
                var index         = i;
                var isHighlighted = i == this._highlightedIndex;
                var row           = this.CreateListRow(this._filteredItems[i], index, isHighlighted);
                this._scrollView.Add(row);
            }
        }

        private static void StyleSelector(VisualElement selector)
        {
            selector.style.flexDirection           = FlexDirection.Row;
            selector.style.backgroundColor         = Theme.SELECTOR_BG;
            selector.style.borderBottomWidth       = 1;
            selector.style.borderTopWidth          = 1;
            selector.style.borderLeftWidth         = 1;
            selector.style.borderRightWidth        = 1;
            selector.style.borderBottomColor       = Theme.SELECTOR_BORDER;
            selector.style.borderTopColor          = Theme.SELECTOR_BORDER;
            selector.style.borderLeftColor         = Theme.SELECTOR_BORDER;
            selector.style.borderRightColor        = Theme.SELECTOR_BORDER;
            selector.style.borderBottomLeftRadius  = 3;
            selector.style.borderBottomRightRadius = 3;
            selector.style.borderTopLeftRadius     = 3;
            selector.style.borderTopRightRadius    = 3;
            selector.style.paddingLeft             = 6;
            selector.style.paddingRight            = 6;
            selector.style.paddingTop              = 4;
            selector.style.paddingBottom           = 4;
        }
    }
}