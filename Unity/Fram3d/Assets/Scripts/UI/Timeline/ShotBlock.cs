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
            this._durationLabel.RegisterCallback<PointerEnterEvent>(_ =>
            {
                CursorService.SetCursor(CursorType.IBeam);
                this.DurationHoverStarted?.Invoke();
            });
            this._durationLabel.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                CursorService.ResetCursor();
                this.DurationHoverEnded?.Invoke();
            });
            this._durationLabel.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
            this._durationLabel.RegisterCallback<ClickEvent>(evt =>
            {
                this.DurationClicked?.Invoke(this);
                evt.StopPropagation();
            });
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
                if (!int.TryParse(parts[0], out var ss) || !int.TryParse(parts[1], out var ff))
                {
                    return -1;
                }

                return ss + (double)ff / fps;
            }

            if (parts.Length == 3)
            {
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

            var initialTimecode = FormatDurationTimecode(this.Shot.Duration);
            var field           = new TextField();
            field.value            = initialTimecode;
            field.selectAllOnFocus = true;
            field.AddToClassList("shot-block__duration-edit");
            this.Add(field);

            field.schedule.Execute(() => field.Focus()).StartingIn(0);

            field.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    this.CommitEdit(field, onCommit);
                    evt.StopPropagation();
                    return;
                }

                if (evt.keyCode == KeyCode.Escape)
                {
                    this.CancelEdit(field);
                    evt.StopPropagation();
                    return;
                }
            });

            field.RegisterValueChangedCallback(evt =>
            {
                var filtered = FilterTimecodeChars(evt.newValue);

                if (filtered != evt.newValue)
                {
                    field.SetValueWithoutNotify(filtered);
                }
            });

            field.RegisterCallback<FocusOutEvent>(_ => this.CommitEdit(field, onCommit));
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

        private void CancelEdit(TextField field)
        {
            if (!this._isEditing)
            {
                return;
            }

            this._isEditing = false;
            this._durationLabel.style.display = DisplayStyle.Flex;
            field.RemoveFromHierarchy();
        }

        private void CommitEdit(TextField field, Action<string> onCommit)
        {
            if (!this._isEditing)
            {
                return;
            }

            this._isEditing = false;
            this._durationLabel.style.display = DisplayStyle.Flex;
            var value = field.value;
            field.RemoveFromHierarchy();
            onCommit?.Invoke(value);
        }
    }
}
