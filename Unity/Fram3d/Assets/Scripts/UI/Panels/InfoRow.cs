using UnityEngine.UIElements;

namespace Fram3d.UI.Panels
{
    /// <summary>
    /// A key-value row: dim label on the left, bright value on the right.
    /// Used for live-updating camera info (body, sensor, focal length, FOV).
    /// </summary>
    public sealed class InfoRow: VisualElement
    {
        private readonly Label _valueLabel;

        public string Value
        {
            get => this._valueLabel.text;
            set => this._valueLabel.text = value;
        }

        public InfoRow(string labelText)
        {
            this.style.flexDirection = FlexDirection.Row;
            this.style.marginBottom  = 3;

            var nameLabel = new Label(labelText);
            nameLabel.style.width    = Theme.INFO_LABEL_WIDTH;
            nameLabel.style.fontSize = Theme.FONT_BODY;
            nameLabel.style.color    = Theme.LABEL_DIM;

            this._valueLabel = new Label("—");
            this._valueLabel.style.fontSize = Theme.FONT_BODY;
            this._valueLabel.style.color    = Theme.LABEL_BRIGHT;

            this.Add(nameLabel);
            this.Add(this._valueLabel);
        }
    }
}
