using Fram3d.Core.Scene;
using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Views
{
    /// <summary>
    /// "DIRECTOR VIEW" badge displayed at top-center when Director View is
    /// active. Red/pink accent to clearly signal the user is NOT looking
    /// through the shot camera.
    /// </summary>
    public sealed class DirectorViewBadge: MonoBehaviour
    {
        private CameraBehaviour   _cameraBehaviour;
        private VisualElement     _badge;
        private ViewCameraManager _viewCameraManager;

        private void BuildBadge()
        {
            this._badge             = new VisualElement();
            this._badge.pickingMode = PickingMode.Ignore;
            this._badge.AddToClassList("director-badge");

            var label = new Label("DIRECTOR VIEW");
            label.pickingMode = PickingMode.Ignore;
            label.AddToClassList("director-badge__label");
            this._badge.Add(label);

            var uiDocument = this.GetComponent<UIDocument>();
            uiDocument.rootVisualElement.Add(this._badge);
            this._badge.style.display = DisplayStyle.None;
        }

        private bool IsDirectorViewVisible()
        {
            if (this._viewCameraManager == null)
            {
                return this._cameraBehaviour.IsDirectorView;
            }

            // In multi-view, show the badge when the active (hovered) slot is Director View
            var model = this._viewCameraManager.ViewSlotModel;
            var slot  = this._viewCameraManager.ActiveSlot;

            if (slot < 0 || slot >= model.ActiveSlotCount)
            {
                return false;
            }

            return model.GetSlotType(slot) == ViewMode.DIRECTOR;
        }

        private void Start()
        {
            this._cameraBehaviour   = FindAnyObjectByType<CameraBehaviour>();
            this._viewCameraManager = FindAnyObjectByType<ViewCameraManager>();
            this.BuildBadge();
        }

        private void Update()
        {
            if (this._badge == null || this._cameraBehaviour == null)
            {
                return;
            }

            this._badge.style.display = this.IsDirectorViewVisible()
                                      ? DisplayStyle.Flex
                                      : DisplayStyle.None;
        }
    }
}
