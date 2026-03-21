using Fram3d.Core.Camera;
using UnityEngine.UIElements;
namespace Fram3d.UI.Panels
{
    /// <summary>
    /// Shake controls: enable toggle, amplitude slider, frequency slider.
    /// Call UpdateValues() each frame to sync toggle state from external changes (keyboard).
    /// </summary>
    public sealed class ShakeSection: VisualElement
    {
        private const float MAX_AMPLITUDE = 1f;
        private const float MAX_FREQUENCY = 5f;
        private const float MIN_AMPLITUDE = 0.01f;
        private const float MIN_FREQUENCY = 0.1f;

        private readonly Slider _amplitudeSlider;
        private readonly Slider _frequencySlider;
        private readonly Toggle _toggle;

        public ShakeSection(CameraElement camera)
        {
            this.Add(Theme.CreateSectionLabel("SHAKE"));
            this._toggle                          = new Toggle("Enabled");
            this._toggle.value                    = camera.ShakeEnabled;
            this._toggle.style.marginBottom       = 4;
            this._toggle.RegisterValueChangedCallback(e => camera.ShakeEnabled = e.newValue);
            this.Add(this._toggle);
            this._amplitudeSlider                 = CreateSlider("Amplitude", MIN_AMPLITUDE, MAX_AMPLITUDE, camera.ShakeAmplitude);
            this._amplitudeSlider.RegisterValueChangedCallback(e => camera.ShakeAmplitude = e.newValue);
            this.Add(this._amplitudeSlider);
            this._frequencySlider                 = CreateSlider("Frequency", MIN_FREQUENCY, MAX_FREQUENCY, camera.ShakeFrequency);
            this._frequencySlider.RegisterValueChangedCallback(e => camera.ShakeFrequency = e.newValue);
            this.Add(this._frequencySlider);
        }

        public void UpdateValues(CameraElement camera)
        {
            if (this._toggle.value != camera.ShakeEnabled)
                this._toggle.SetValueWithoutNotify(camera.ShakeEnabled);

            if (!this._amplitudeSlider.focusController?.focusedElement?.Equals(this._amplitudeSlider) ?? true)
                this._amplitudeSlider.SetValueWithoutNotify(camera.ShakeAmplitude);

            if (!this._frequencySlider.focusController?.focusedElement?.Equals(this._frequencySlider) ?? true)
                this._frequencySlider.SetValueWithoutNotify(camera.ShakeFrequency);
        }

        private static Slider CreateSlider(string label, float min, float max, float value)
        {
            var slider                  = new Slider(label, min, max);
            slider.value                = value;
            slider.showInputField       = true;
            slider.style.marginBottom   = 4;
            slider.labelElement.style.minWidth = 70;
            slider.labelElement.style.fontSize = Theme.FONT_BODY;
            slider.labelElement.style.color    = Theme.LABEL_DIM;
            return slider;
        }
    }
}
