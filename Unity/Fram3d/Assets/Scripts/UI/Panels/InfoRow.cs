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

        public InfoRow(string labelText)
        {
            this.AddToClassList("info-row");
            var nameLabel = new Label(labelText);
            nameLabel.AddToClassList("info-row__key");
            this._valueLabel = new Label("\u2014");
            this._valueLabel.AddToClassList("info-row__value");
            this.Add(nameLabel);
            this.Add(this._valueLabel);
        }

        public string Value
        {
            get => this._valueLabel.text;
            set => this._valueLabel.text = value;
        }
    }
}