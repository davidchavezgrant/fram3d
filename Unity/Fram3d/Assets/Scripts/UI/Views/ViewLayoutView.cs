using Fram3d.Core.Common;
using Fram3d.Engine.Integration;
using Fram3d.UI.Panels;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fram3d.UI.Views
{
    /// <summary>
    /// Renders the layout chooser buttons at the bottom-right of the screen.
    /// When a multi-view layout is selected, ViewCameraManager creates
    /// additional cameras and sets Camera.rect viewports. This class only
    /// manages the chooser UI — it does not overlay view panels or intercept
    /// input from the 3D scene.
    /// </summary>
    public sealed class ViewLayoutView : MonoBehaviour
    {
        private VisualElement     _layoutChooser;
        private VisualElement     _root;
        private ViewCameraManager _viewCameraManager;

        private void Start()
        {
            this._viewCameraManager = FindAnyObjectByType<ViewCameraManager>();

            if (this._viewCameraManager == null)
            {
                Debug.LogWarning("ViewLayoutView: No ViewCameraManager found.");
                return;
            }

            var uiDocument = this.GetComponent<UIDocument>();

            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogWarning("ViewLayoutView: UIDocument or rootVisualElement is null.");
                return;
            }

            this._root = uiDocument.rootVisualElement;
            StyleSheetLoader.Apply(this._root);
            this._root.pickingMode = PickingMode.Ignore;
            this.BuildLayoutChooser();
            this._viewCameraManager.ViewSlotModel.Changed += this.RefreshChooser;
        }

        private void OnDestroy()
        {
            if (this._viewCameraManager != null)
            {
                this._viewCameraManager.ViewSlotModel.Changed -= this.RefreshChooser;
            }
        }

        private void BuildLayoutChooser()
        {
            if (this._layoutChooser != null)
            {
                this._layoutChooser.RemoveFromHierarchy();
            }

            this._layoutChooser = new VisualElement();
            this._layoutChooser.AddToClassList("layout-chooser");

            var model = this._viewCameraManager.ViewSlotModel;

            this._layoutChooser.Add(this.CreateLayoutButton("\u25fb", ViewLayout.SINGLE,       model.Layout));
            this._layoutChooser.Add(this.CreateLayoutButton("\u25eb", ViewLayout.SIDE_BY_SIDE, model.Layout));
            this._layoutChooser.Add(this.CreateLayoutButton("\u229e", ViewLayout.ONE_PLUS_TWO, model.Layout));

            this._root.Add(this._layoutChooser);
        }

        private Button CreateLayoutButton(string label, ViewLayout layout, ViewLayout currentLayout)
        {
            var btn = new Button(() =>
            {
                this._viewCameraManager.ViewSlotModel.SetLayout(layout);
            });

            btn.text = label;
            btn.AddToClassList("layout-chooser__button");

            if (layout == currentLayout)
            {
                btn.AddToClassList("layout-chooser__button--active");
            }

            return btn;
        }

        private void RefreshChooser() => this.BuildLayoutChooser();
    }
}
