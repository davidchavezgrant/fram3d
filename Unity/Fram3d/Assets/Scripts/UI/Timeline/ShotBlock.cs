using System;
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
        private static readonly Color EDIT_BG  = new(0f, 0f, 0f, 0.35f);
        private static readonly Color HOVER_BG = new(1f, 1f, 1f, 0.12f);

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
            this.style.alignItems      = Align.FlexStart;

            this._nameLabel = new Label(shot.Name);
            this._nameLabel.AddToClassList("shot-block__name");
            this.Add(this._nameLabel);

            this._durationLabel = new Label(FormatDuration(shot.Duration));
            this._durationLabel.AddToClassList("shot-block__duration");
            this.StyleDurationLabel(this._durationLabel);
            this.Add(this._durationLabel);
        }

        public event Action<ShotBlock> DurationClicked;
        public event Action            DurationHoverEnded;
        public event Action            DurationHoverStarted;

        public bool IsEditing => this._isEditing;

        public Shot Shot { get; }

        /// <summary>
        /// Parses flexible timecode input into seconds at 24fps.
        /// Accepts: "5" → 5 frames, "500" → 5s 0f, "5;00" → 5s 0f,
        /// "1;30;12" → 1m 30s 12f, bare frames up to 99.
        /// </summary>
        public static double ParseTimecodeInput(string input, int fps = 24)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return -1;
            }

            input = input.Trim();
            var separator = input.Contains(";") ? ';' : ':';
            var parts     = input.Split(separator);

            if (parts.Length == 1)
            {
                if (!int.TryParse(parts[0], out var num))
                {
                    return -1;
                }

                // Bare number: <=99 → frames, >=100 → treat as SSF pattern (e.g. 500 → 5s 00f)
                if (num < 100)
                {
                    return (double)num / fps;
                }

                var frames  = num % 100;
                var seconds = num / 100;
                return seconds + (double)frames / fps;
            }

            if (parts.Length == 2)
            {
                // SS;FF
                if (!int.TryParse(parts[0], out var ss) || !int.TryParse(parts[1], out var ff))
                {
                    return -1;
                }

                return ss + (double)ff / fps;
            }

            if (parts.Length == 3)
            {
                // MM;SS;FF
                if (!int.TryParse(parts[0], out var mm) || !int.TryParse(parts[1], out var ss) || !int.TryParse(parts[2], out var ff))
                {
                    return -1;
                }

                return mm * 60 + ss + (double)ff / fps;
            }

            return -1;
        }

        public void BeginDurationEdit(Action<string> onCommit)
        {
            if (this._isEditing)
            {
                return;
            }

            this._isEditing = true;
            this._durationLabel.style.display = DisplayStyle.None;

            // Invisible TextField captures keyboard input
            var field = new TextField();
            field.style.position = Position.Absolute;
            field.style.opacity  = 0;
            field.style.width    = 1;
            field.style.height   = 1;
            field.style.overflow = Overflow.Hidden;
            var initialTimecode = FormatDurationTimecode(this.Shot.Duration);
            field.value            = initialTimecode;
            field.selectAllOnFocus = true;
            this.Add(field);

            // Visible pill — same size/style as hover state
            var pill = new Label(initialTimecode);
            StylePill(pill);
            pill.style.backgroundColor = EDIT_BG;
            pill.pickingMode           = PickingMode.Ignore;
            this.Add(pill);

            field.schedule.Execute(() => field.Focus()).StartingIn(0);

            // Filter: only allow digits, semicolons, colons
            field.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    this.CommitEdit(field, pill, onCommit);
                    evt.StopPropagation();
                    return;
                }

                if (evt.keyCode == KeyCode.Escape)
                {
                    this.CancelEdit(field, pill);
                    evt.StopPropagation();
                    return;
                }
            });

            // Update pill text as user types (display only, no live resize)
            field.RegisterValueChangedCallback(evt =>
            {
                // Strip any characters that aren't digits, semicolons, or colons
                var filtered = FilterTimecodeChars(evt.newValue);

                if (filtered != evt.newValue)
                {
                    field.SetValueWithoutNotify(filtered);
                }

                pill.text = filtered;
            });

            field.RegisterCallback<FocusOutEvent>(_ => this.CommitEdit(field, pill, onCommit));
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

        private static string FilterTimecodeChars(string input)
        {
            var sb = new System.Text.StringBuilder(input.Length);

            foreach (var c in input)
            {
                if (c >= '0' && c <= '9' || c == ';' || c == ':')
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static string FormatDuration(double seconds) => $"{seconds:F1}s";

        private static string FormatDurationTimecode(double seconds, int fps = 24)
        {
            var totalFrames = (int)(seconds * fps);
            var s           = totalFrames   / fps;
            var f           = totalFrames   % fps;
            return $"{s};{f:D2}";
        }

        private static void StylePill(VisualElement el)
        {
            el.style.borderTopLeftRadius     = 3;
            el.style.borderTopRightRadius    = 3;
            el.style.borderBottomLeftRadius  = 3;
            el.style.borderBottomRightRadius = 3;
            el.style.color                   = Color.white;
            el.style.paddingLeft             = 4;
            el.style.paddingRight            = 8;
            el.style.overflow                = Overflow.Visible;
            el.style.alignSelf               = Align.FlexStart;
            el.style.unityTextAlign          = TextAnchor.MiddleLeft;
        }

        private void CancelEdit(TextField field, Label pill)
        {
            if (!this._isEditing)
            {
                return;
            }

            this._isEditing = false;
            this._durationLabel.style.display = DisplayStyle.Flex;
            field.RemoveFromHierarchy();
            pill.RemoveFromHierarchy();
        }

        private void CommitEdit(TextField field, Label pill, Action<string> onCommit)
        {
            if (!this._isEditing)
            {
                return;
            }

            this._isEditing = false;
            this._durationLabel.style.display = DisplayStyle.Flex;
            var value = field.value;
            field.RemoveFromHierarchy();
            pill.RemoveFromHierarchy();
            onCommit?.Invoke(value);
        }

        private void StyleDurationLabel(Label label)
        {
            StylePill(label);
            label.style.backgroundColor = Color.clear;

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
