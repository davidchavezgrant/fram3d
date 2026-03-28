using System;
using UnityEngine.UIElements;
namespace Fram3d.UI.Timeline
{
    /// <summary>
    /// Modal confirmation dialog shown when the user turns off a stopwatch
    /// on a track that has existing keyframes. Offers confirm/cancel and a
    /// "don't show again" toggle.
    /// </summary>
    public sealed class StopwatchConfirmDialog : VisualElement
    {
        private readonly Toggle _dontShowAgain;

        public StopwatchConfirmDialog(Action onConfirm, Action<bool> onDismiss)
        {
            this.AddToClassList("stopwatch-dialog-overlay");

            var panel = new VisualElement();
            panel.AddToClassList("stopwatch-dialog");

            var message = new Label(
                "Turning off the stopwatch will delete all keyframes on this track. Continue?");
            message.AddToClassList("stopwatch-dialog__message");
            panel.Add(message);

            this._dontShowAgain = new Toggle("Don't show again");
            this._dontShowAgain.AddToClassList("stopwatch-dialog__toggle");
            panel.Add(this._dontShowAgain);

            var buttons = new VisualElement();
            buttons.AddToClassList("stopwatch-dialog__buttons");

            var cancelBtn = new Button(() => onDismiss(this._dontShowAgain.value));
            cancelBtn.text = "Cancel";
            cancelBtn.AddToClassList("stopwatch-dialog__cancel");
            buttons.Add(cancelBtn);

            var confirmBtn = new Button(() =>
            {
                onConfirm();
                onDismiss(this._dontShowAgain.value);
            });
            confirmBtn.text = "Confirm";
            confirmBtn.AddToClassList("stopwatch-dialog__confirm");
            buttons.Add(confirmBtn);

            panel.Add(buttons);
            this.Add(panel);
        }
    }
}
