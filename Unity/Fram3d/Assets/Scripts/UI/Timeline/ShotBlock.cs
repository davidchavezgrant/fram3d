using Fram3d.Core.Shots;
using Fram3d.Engine.Cursor;
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
        private static readonly Color HOVER_BG = new(1f, 1f, 1f, 0.12f);
        private static readonly Color EDIT_BG  = new(0f, 0f, 0f, 0.35f);

        private readonly Color _baseColor;
        private readonly Label _durationLabel;
        private readonly Label _nameLabel;
        private          bool  _isEditing;

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
            this.StyleDurationLabel(this._durationLabel);
            this.Add(this._durationLabel);
        }

        /// <summary>
        /// Fired when the duration label is clicked (to trigger inline editing).
        /// </summary>
        public event System.Action<ShotBlock> DurationClicked;
        public event System.Action           DurationHoverEnded;
        public event System.Action           DurationHoverStarted;

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

            // Invisible TextField for input capture — never visually shown
            var field = new TextField();
            field.style.position = Position.Absolute;
            field.style.opacity  = 0;
            field.style.width    = 0;
            field.style.height   = 0;
            field.value          = this.Shot.Duration.ToString("F1");
            field.selectAllOnFocus = true;
            this.Add(field);

            // Visible pill that mirrors the hover style
            var pill = new Label(field.value);
            pill.style.borderTopLeftRadius     = 3;
            pill.style.borderTopRightRadius    = 3;
            pill.style.borderBottomLeftRadius  = 3;
            pill.style.borderBottomRightRadius = 3;
            pill.style.backgroundColor         = EDIT_BG;
            pill.style.color                   = Color.white;
            pill.style.paddingLeft             = 4;
            pill.style.paddingRight            = 8;
            pill.style.overflow                = Overflow.Visible;
            pill.pickingMode                   = PickingMode.Ignore;
            this.Add(pill);

            field.schedule.Execute(() => field.Focus()).StartingIn(0);

            // Sync visible pill text as user types
            field.RegisterValueChangedCallback(evt => pill.text = evt.newValue);

            var committed = false;

            void commit()
            {
                if (committed)
                {
                    return;
                }

                committed       = true;
                this._isEditing = false;
                this._durationLabel.style.display = DisplayStyle.Flex;
                field.RemoveFromHierarchy();
                pill.RemoveFromHierarchy();
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
                    this._isEditing = false;
                    this._durationLabel.style.display = DisplayStyle.Flex;
                    field.RemoveFromHierarchy();
                    pill.RemoveFromHierarchy();
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

        private void StyleDurationLabel(Label label)
        {
            label.style.borderTopLeftRadius     = 3;
            label.style.borderTopRightRadius    = 3;
            label.style.borderBottomLeftRadius  = 3;
            label.style.borderBottomRightRadius = 3;
            label.style.backgroundColor         = Color.clear;
            label.style.paddingLeft             = 4;
            label.style.paddingRight            = 8;
            label.style.overflow                = Overflow.Visible;

            label.RegisterCallback<PointerEnterEvent>(_ =>
            {
                label.style.backgroundColor = HOVER_BG;
                CursorService.SetCursor(CursorType.IBeam);
                this.DurationHoverStarted?.Invoke();
            });

            label.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                label.style.backgroundColor = Color.clear;
                CursorService.ResetCursor();
                this.DurationHoverEnded?.Invoke();
            });

            label.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());

            label.RegisterCallback<ClickEvent>(evt =>
            {
                this.DurationClicked?.Invoke(this);
                evt.StopPropagation();
            });
        }
    }
}
