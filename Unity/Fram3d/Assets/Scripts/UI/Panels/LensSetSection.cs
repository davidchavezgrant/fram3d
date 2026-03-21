using System;
using System.Collections.Generic;
using System.Linq;
using Fram3d.Core.Camera;
using UnityEngine.UIElements;
namespace Fram3d.UI.Panels
{
    /// <summary>
    /// Lens set picker: searchable dropdown over all lens sets.
    /// </summary>
    public sealed class LensSetSection: VisualElement
    {
        private readonly List<LensSet>      _lensSetList;
        private          SearchableDropdown _dropdown;

        public LensSetSection(IReadOnlyList<LensSet> allLensSets, LensSet currentLensSet)
        {
            this._lensSetList = allLensSets.ToList();
            var names        = this._lensSetList.Select(ls => ls.Name).ToList();
            var currentIndex = currentLensSet != null? this._lensSetList.IndexOf(currentLensSet) : 0;
            this.Add(Theme.CreateSectionLabel("Lens Set"));
            this._dropdown                  =  new SearchableDropdown(names, Math.Max(currentIndex, 0), "Search lenses...");
            this._dropdown.SelectionChanged += this.OnSelectionChanged;
            this.Add(this._dropdown.Root);
        }

        public event Action<LensSet> LensSetChanged;

        /// <summary>
        /// Whether the dropdown's search field has focus.
        /// </summary>
        public bool HasFocus => this._dropdown != null && this._dropdown.HasFocus;

        private void OnSelectionChanged(int index)
        {
            if (index >= 0 && index < this._lensSetList.Count)
                this.LensSetChanged?.Invoke(this._lensSetList[index]);
        }
    }
}