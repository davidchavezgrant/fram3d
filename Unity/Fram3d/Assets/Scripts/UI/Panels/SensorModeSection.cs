using System;
using System.Collections.Generic;
using System.Linq;
using Fram3d.Core.Camera;
using UnityEngine.UIElements;
namespace Fram3d.UI.Panels
{
    /// <summary>
    /// Sensor mode picker: simple dropdown showing the available recording modes
    /// for the current camera body. Updates when the body changes.
    /// </summary>
    public sealed class SensorModeSection: VisualElement
    {
        private List<SensorMode> _modes = new();
        private DropdownField    _dropdown;

        public SensorModeSection(SensorMode[] modes, SensorMode currentMode)
        {
            this.Add(Theme.CreateSectionLabel("Sensor Mode"));
            this._dropdown = new DropdownField();
            this._dropdown.style.fontSize        = Theme.FONT_BODY;
            this._dropdown.style.backgroundColor = Theme.SELECTOR_BG;
            this._dropdown.style.borderTopWidth   = 1;
            this._dropdown.style.borderBottomWidth = 1;
            this._dropdown.style.borderLeftWidth  = 1;
            this._dropdown.style.borderRightWidth = 1;
            this._dropdown.style.borderTopColor   = Theme.SELECTOR_BORDER;
            this._dropdown.style.borderBottomColor = Theme.SELECTOR_BORDER;
            this._dropdown.style.borderLeftColor  = Theme.SELECTOR_BORDER;
            this._dropdown.style.borderRightColor = Theme.SELECTOR_BORDER;
            this._dropdown.RegisterValueChangedCallback(this.OnSelectionChanged);
            this.Add(this._dropdown);
            this.SetModes(modes, currentMode);
        }

        public event Action<SensorMode> ModeChanged;

        public void SetModes(SensorMode[] modes, SensorMode currentMode)
        {
            this._modes = modes != null? modes.ToList() : new List<SensorMode>();
            var names = this._modes.Select(m => m.ToString()).ToList();
            this._dropdown.choices = names;

            if (this._modes.Count == 0)
            {
                this._dropdown.SetEnabled(false);
                this._dropdown.value = "—";
                return;
            }

            this._dropdown.SetEnabled(true);
            var currentIndex = currentMode != null? this._modes.IndexOf(currentMode) : 0;

            if (currentIndex < 0)
                currentIndex = 0;

            this._dropdown.SetValueWithoutNotify(names[currentIndex]);
        }

        private void OnSelectionChanged(ChangeEvent<string> evt)
        {
            var index = this._dropdown.choices.IndexOf(evt.newValue);

            if (index >= 0 && index < this._modes.Count)
                this.ModeChanged?.Invoke(this._modes[index]);
        }
    }
}
