using Fram3d.Core.Shots;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// VisualElement representing a single shot block in the shot track strip.
    /// Displays the shot name and duration. Colored based on palette index.
    /// </summary>
    public sealed class ShotBlock : VisualElement
    {
        private readonly Color _baseColor;
        private readonly Label _durationLabel;
        private          bool  _isEditing;
        private readonly Label _nameLabel;

        public ShotBlock(Shot shot, int colorIndex)
        {
            this.Shot       = shot;
            this._baseColor = ShotColorPalette.GetColor(colorIndex);

            this.AddToClassList("shot-block");
            this.style.backgroundColor = this._baseColor;

            this._nameLabel = new Label(shot.Name);
            this._nameLabel.AddToClassList("shot-block__name");
            this.Add(this._nameLabel);

            this._durationLabel = new Label(FormatDuration(shot.Duration));
            this._durationLabel.AddToClassList("shot-block__duration");
            this._durationLabel.style.borderBottomWidth = 1;
            this._durationLabel.style.borderBottomColor = Color.clear;
            this._durationLabel.style.paddingLeft       = 2;
            this._durationLabel.style.paddingRight      = 2;
            this._durationLabel.RegisterCallback<PointerEnterEvent>(_ =>
            {
                this._durationLabel.style.borderBottomColor = new Color(1f, 1f, 1f, 0.5f);
                this._durationLabel.style.backgroundColor   = new Color(1f, 1f, 1f, 0.1f);
            });
            this._durationLabel.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                this._durationLabel.style.borderBottomColor = Color.clear;
                this._durationLabel.style.backgroundColor   = Color.clear;
            });
            this._durationLabel.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
            this._durationLabel.RegisterCallback<ClickEvent>(evt =>
            {
                this.DurationClicked?.Invoke(this);
                evt.StopPropagation();
            });
            this.Add(this._durationLabel);
        }

        /// <summary>
        /// Fired when the duration label is clicked (to trigger inline editing).
        /// </summary>
        public event System.Action<ShotBlock> DurationClicked;

        public bool IsEditing => this._isEditing;

        public Shot Shot { get; }

        public void BeginDurationEdit(System.Action<string> onCommit)
        {
            if (this._isEditing)
            {
                return;
            }

            this._isEditing = true;
            this._durationLabel.style.display = DisplayStyle.None;

            var field = new TextField();
            field.AddToClassList("shot-block__duration-field");
            field.value = this.Shot.Duration.ToString("F1");
            field.selectAllOnFocus = true;
            this.Add(field);

            // Focus the field next frame (UI Toolkit needs a frame to lay out)
            field.schedule.Execute(() => field.Focus()).StartingIn(0);

            var committed = false;

            void commit()
            {
                if (committed)
                {
                    return;
                }

                committed         = true;
                this._isEditing   = false;
                this._durationLabel.style.display = DisplayStyle.Flex;
                field.RemoveFromHierarchy();
                onCommit?.Invoke(field.value);
            }

            field.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    commit();
                    evt.StopPropagation();
                }
                else if (evt.keyCode == KeyCode.Escape)
                {
                    this._isEditing   = false;
                    this._durationLabel.style.display = DisplayStyle.Flex;
                    field.RemoveFromHierarchy();
                    committed = true;
                }
            });

            field.RegisterCallback<FocusOutEvent>(_ => commit());
        }

        public void Refresh()
        {
            this._nameLabel.text     = this.Shot.Name;
            this._durationLabel.text = FormatDuration(this.Shot.Duration);
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                this.AddToClassList("shot-block--active");
            }
            else
            {
                this.RemoveFromClassList("shot-block--active");
            }
        }

        private static string FormatDuration(double seconds) => $"{seconds:F1}s";
    }
}
