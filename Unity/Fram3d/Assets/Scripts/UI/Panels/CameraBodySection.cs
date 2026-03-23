using System;
using System.Collections.Generic;
using System.Linq;
using Fram3d.Core.Camera;
using UnityEngine.UIElements;
namespace Fram3d.UI.Panels
{
    /// <summary>
    /// Camera body picker: searchable dropdown + "Show all cameras" toggle.
    /// Generics appear first, then cameras sorted by year descending.
    /// Browse mode shows 2019+ cameras; search always covers all cameras.
    /// </summary>
    public sealed class CameraBodySection: VisualElement
    {
        private const    int                MIN_BODY_YEAR = 2019;
        private readonly List<string>       _allNames;
        private readonly List<CameraBody>   _bodyList;
        private          SearchableDropdown _dropdown;
        private          bool               _showAll;

        public CameraBodySection(IReadOnlyList<CameraBody> allBodies, CameraBody currentBody)
        {
            // Generics first, then sorted by year descending
            this._bodyList = allBodies.OrderByDescending(b => b.Manufacturer == "Generic").ThenByDescending(b => b.Year).ToList();
            this._allNames = this._bodyList.Select(b => $"{b.Manufacturer} — {b.Name}").ToList();
            this.Add(Theme.CreateSectionLabel("Camera Body"));
            this.BuildDropdown(currentBody);
            this.BuildShowAllToggle();
        }

        public event Action<CameraBody> BodyChanged;

        /// <summary>
        /// Whether the dropdown's search field has focus.
        /// </summary>
        public bool HasFocus => this._dropdown != null && this._dropdown.HasFocus;

        private void ApplyBrowseFilter()
        {
            if (this._showAll)
            {
                this._dropdown.SetBrowseFilter(this._allNames);
                return;
            }

            var browseNames = new List<string>();

            for (var i = 0; i < this._bodyList.Count; i++)
            {
                var body = this._bodyList[i];

                if (body.Manufacturer == "Generic" || body.Year >= MIN_BODY_YEAR)
                    browseNames.Add(this._allNames[i]);
            }

            this._dropdown.SetBrowseFilter(browseNames);
        }

        private void BuildDropdown(CameraBody currentBody)
        {
            var currentIndex = currentBody != null? this._bodyList.IndexOf(currentBody) : 0;
            this._dropdown                  =  new SearchableDropdown(this._allNames, Math.Max(currentIndex, 0), "Search cameras...");
            this._dropdown.SelectionChanged += this.OnSelectionChanged;
            this.Add(this._dropdown.Root);
            this.ApplyBrowseFilter();
        }

        private void BuildShowAllToggle()
        {
            var labelColor = Theme.TEXT_DEFAULT;
            var toggle     = new Toggle("Show all cameras");
            toggle.AddToClassList("section-toggle");
            var toggleLabel = toggle.Q<Label>();

            if (toggleLabel != null)
            {
                toggleLabel.style.color = labelColor;
            }

            toggle.RegisterValueChangedCallback(evt =>
                                                {
                                                    this._showAll = evt.newValue;
                                                    this.ApplyBrowseFilter();

                                                    if (toggleLabel != null)
                                                    {
                                                        toggleLabel.style.color = labelColor;
                                                    }
                                                });

            this.Add(toggle);
        }

        private void OnSelectionChanged(int index)
        {
            if (index >= 0 && index < this._bodyList.Count)
                this.BodyChanged?.Invoke(this._bodyList[index]);
        }
    }
}