using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Fram3d.UI.Panels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Tests for SearchableDropdown search filtering, keyboard navigation,
    /// and selection. Uses reflection to access private members for test setup
    /// (Open, searchField, scrollView) but asserts on public behavior
    /// (SelectedIndex, SelectionChanged, item count).
    /// </summary>
    public sealed class SearchableDropdownTests
    {
        private static readonly List<string> TEST_ITEMS = new()
        {
            "ARRI Alexa 35",
            "ARRI Alexa Mini LF",
            "RED V-RAPTOR",
            "RED KOMODO",
            "Sony VENICE 2",
            "Sony FX6",
            "Canon EOS R5",
            "Canon EOS C70",
            "Blackmagic URSA Mini Pro",
            "Phantom Flex4K"
        };

        private SearchableDropdown _dropdown;
        private ScrollView         _scrollView;
        private TextField          _searchField;
        private GameObject         _uiGo;
        private UIDocument         _uiDocument;

        [SetUp]
        public void SetUp()
        {
            this._dropdown = new SearchableDropdown(new List<string>(TEST_ITEMS), 0, "Search cameras...");

            this._uiGo      = new GameObject("TestUI");
            this._uiDocument = this._uiGo.AddComponent<UIDocument>();

            var guids = UnityEditor.AssetDatabase.FindAssets("t:PanelSettings");

            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                this._uiDocument.panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            }

            this._uiDocument.rootVisualElement.Add(this._dropdown.Root);

            // Access private members for test setup
            this._searchField = GetPrivateField<TextField>("_searchField");
            this._scrollView  = GetPrivateField<ScrollView>("_scrollView");
        }

        [TearDown]
        public void TearDown()
        {
            this._dropdown.Close();
            UnityEngine.Object.DestroyImmediate(this._uiGo);
        }

        // --- Search filtering ---

        [UnityTest]
        public IEnumerator Search__ShowsAllBrowseItems__When__SearchIsEmpty()
        {
            yield return null;

            CallPrivateMethod("Open");
            yield return null;

            Assert.AreEqual(TEST_ITEMS.Count, this._scrollView.childCount);
        }

        [UnityTest]
        public IEnumerator Search__FiltersToMatchingItems__When__SingleWord()
        {
            yield return null;

            CallPrivateMethod("Open");
            this._searchField.value = "ARRI";
            yield return null;

            // "ARRI Alexa 35" and "ARRI Alexa Mini LF"
            Assert.AreEqual(2, this._scrollView.childCount);
        }

        [UnityTest]
        public IEnumerator Search__FiltersWithAndLogic__When__MultipleWords()
        {
            yield return null;

            CallPrivateMethod("Open");
            this._searchField.value = "ARRI Mini";
            yield return null;

            // Only "ARRI Alexa Mini LF" matches both words
            Assert.AreEqual(1, this._scrollView.childCount);
        }

        [UnityTest]
        public IEnumerator Search__IsCaseInsensitive__When__LowercaseQuery()
        {
            yield return null;

            CallPrivateMethod("Open");
            this._searchField.value = "red";
            yield return null;

            // "RED V-RAPTOR" and "RED KOMODO"
            Assert.AreEqual(2, this._scrollView.childCount);
        }

        [UnityTest]
        public IEnumerator Search__ReturnsNoResults__When__NoMatch()
        {
            yield return null;

            CallPrivateMethod("Open");
            this._searchField.value = "Panavision";
            yield return null;

            Assert.AreEqual(0, this._scrollView.childCount);
        }

        [UnityTest]
        public IEnumerator Search__SearchesAllItems__When__BrowseFilterIsSet()
        {
            yield return null;

            // Set browse filter to only show ARRI cameras
            this._dropdown.SetBrowseFilter(new List<string> { "ARRI Alexa 35", "ARRI Alexa Mini LF" });

            CallPrivateMethod("Open");
            yield return null;

            // Browse mode: only 2 items
            Assert.AreEqual(2, this._scrollView.childCount);

            // Search should cover ALL items, not just browse items
            this._searchField.value = "Canon";
            yield return null;

            // "Canon EOS R5" and "Canon EOS C70" — found even though not in browse filter
            Assert.AreEqual(2, this._scrollView.childCount);
        }

        [UnityTest]
        public IEnumerator Search__ResetsToBrowseItems__When__SearchCleared()
        {
            yield return null;

            this._dropdown.SetBrowseFilter(new List<string> { "ARRI Alexa 35" });

            CallPrivateMethod("Open");
            this._searchField.value = "RED";
            yield return null;

            Assert.AreEqual(2, this._scrollView.childCount);

            this._searchField.value = "";
            yield return null;

            // Back to browse items (just 1)
            Assert.AreEqual(1, this._scrollView.childCount);
        }

        // --- Selection ---

        [UnityTest]
        public IEnumerator Selection__FiresEvent__When__Confirmed()
        {
            yield return null;

            var selectedIndex = -1;
            this._dropdown.SelectionChanged += i => selectedIndex = i;

            CallPrivateMethod("Open");
            this._searchField.value = "Sony VENICE";
            yield return null;

            // Highlight first result and confirm
            SetPrivateField("_highlightedIndex", 0);
            CallPrivateMethod("ConfirmSelection", 0);

            // "Sony VENICE 2" is at index 4 in the ALL items list
            Assert.AreEqual(4, selectedIndex);
        }

        [UnityTest]
        public IEnumerator Selection__UpdatesSelectedIndex__When__Confirmed()
        {
            yield return null;

            CallPrivateMethod("Open");
            this._searchField.value = "Canon EOS R5";
            yield return null;

            SetPrivateField("_highlightedIndex", 0);
            CallPrivateMethod("ConfirmSelection", 0);

            // "Canon EOS R5" is at index 6
            Assert.AreEqual(6, this._dropdown.SelectedIndex);
        }

        [UnityTest]
        public IEnumerator Selection__ClosesDropdown__When__Confirmed()
        {
            yield return null;

            CallPrivateMethod("Open");
            Assert.IsTrue(this._dropdown.HasFocus);

            SetPrivateField("_highlightedIndex", 0);
            CallPrivateMethod("ConfirmSelection", 0);

            Assert.IsFalse(this._dropdown.HasFocus);
        }

        // --- Open / Close ---

        [UnityTest]
        public IEnumerator Close__ResetsSearchField__When__Called()
        {
            yield return null;

            CallPrivateMethod("Open");
            this._searchField.value = "test query";
            yield return null;

            this._dropdown.Close();

            Assert.AreEqual("", this._searchField.value);
        }

        [UnityTest]
        public IEnumerator Close__ResetsHighlight__When__Called()
        {
            yield return null;

            CallPrivateMethod("Open");
            SetPrivateField("_highlightedIndex", 3);

            this._dropdown.Close();

            Assert.AreEqual(-1, GetPrivateField<int>("_highlightedIndex"));
        }

        [UnityTest]
        public IEnumerator Open__SetsHasFocus__When__Called()
        {
            yield return null;

            Assert.IsFalse(this._dropdown.HasFocus);

            CallPrivateMethod("Open");

            Assert.IsTrue(this._dropdown.HasFocus);
        }

        // --- Initial state ---

        [UnityTest]
        public IEnumerator Constructor__SetsInitialIndex__When__Provided()
        {
            yield return null;

            var dropdown = new SearchableDropdown(new List<string>(TEST_ITEMS), 3, "Search...");

            Assert.AreEqual(3, dropdown.SelectedIndex);
        }

        [UnityTest]
        public IEnumerator Constructor__ClampsIndex__When__OutOfRange()
        {
            yield return null;

            var dropdown = new SearchableDropdown(new List<string>(TEST_ITEMS), 999, "Search...");

            Assert.AreEqual(TEST_ITEMS.Count - 1, dropdown.SelectedIndex);
        }

        // --- Helpers ---

        private T GetPrivateField<T>(string fieldName)
        {
            var field = typeof(SearchableDropdown).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(this._dropdown);
        }

        private void SetPrivateField<T>(string fieldName, T value)
        {
            var field = typeof(SearchableDropdown).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(this._dropdown, value);
        }

        private void CallPrivateMethod(string methodName, params object[] args)
        {
            var method = typeof(SearchableDropdown).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            method.Invoke(this._dropdown, args);
        }
    }
}
