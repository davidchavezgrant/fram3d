using Fram3d.Core.Camera;
using Fram3d.Core.Viewport;
using Fram3d.Engine.Integration;
using Fram3d.UI.Panels;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Views
{
    /// <summary>
    /// Renders opaque black letterbox/pillarbox bars over the camera view based on
    /// the active aspect ratio. Four absolutely positioned bars surround the unmasked
    /// area. All bars have pickingMode = Ignore so mouse events pass through to the
    /// 3D scene. Recalculates on geometry change and each frame (to track ratio changes).
    /// In multi-view, constrains itself to the Camera View viewport rect.
    /// </summary>
    public sealed class AspectRatioMaskView: MonoBehaviour
    {
        private VisualElement     _barBottom;
        private VisualElement     _barLeft;
        private VisualElement     _barRight;
        private VisualElement     _barTop;
        private CameraBehaviour   _cameraBehaviour;
        private VisualElement     _container;
        private VisualElement     _root;
        private ViewCameraManager _viewCameraManager;

        private void BuildOverlay()
        {
            this._container             = new VisualElement();
            this._container.pickingMode = PickingMode.Ignore;
            this._container.AddToClassList("aspect-mask-container");
            this._barTop    = CreateBar();
            this._barBottom = CreateBar();
            this._barLeft   = CreateBar();
            this._barRight  = CreateBar();
            this._container.Add(this._barTop);
            this._container.Add(this._barBottom);
            this._container.Add(this._barLeft);
            this._container.Add(this._barRight);
            this._root.Insert(0, this._container);
            this._container.RegisterCallback<GeometryChangedEvent>(_ => this.UpdateBars());
        }

        private float CssInset()
        {
            var w     = this._root.resolvedStyle.width;
            var scale = Screen.width > 0 && !float.IsNaN(w) && w > 0
                      ? (float)Screen.width / w
                      : 1f;

            return this._cameraBehaviour.RightInsetPixels / scale;
        }

        private void ScopeToViewport()
        {
            var rootW = this._root.resolvedStyle.width;
            var rootH = this._root.resolvedStyle.height;

            if (this._viewCameraManager == null || !this._viewCameraManager.IsMultiView)
            {
                this._container.style.left   = 0;
                this._container.style.top    = 0;
                this._container.style.right  = this.CssInset();
                this._container.style.bottom = 0;
                this._container.style.width  = StyleKeyword.Auto;
                this._container.style.height = StyleKeyword.Auto;
                return;
            }

            if (float.IsNaN(rootW) || float.IsNaN(rootH))
            {
                return;
            }

            var vpRect = this._viewCameraManager.CameraViewRect;

            this._container.style.left   = vpRect.x * rootW;
            this._container.style.top    = (1f - vpRect.y - vpRect.height) * rootH;
            this._container.style.width  = vpRect.width  * rootW;
            this._container.style.height = vpRect.height * rootH;
            this._container.style.right  = StyleKeyword.Auto;
            this._container.style.bottom = StyleKeyword.Auto;
        }

        private void UpdateBars()
        {
            if (this._container == null || this._cameraBehaviour == null)
            {
                return;
            }

            this.ScopeToViewport();

            var viewWidth  = this._container.resolvedStyle.width;
            var viewHeight = this._container.resolvedStyle.height;

            if (float.IsNaN(viewWidth) || float.IsNaN(viewHeight) || viewWidth <= 0 || viewHeight <= 0)
            {
                return;
            }

            var rect = this._cameraBehaviour.ActiveAspectRatio.ComputeUnmaskedRect(viewWidth, viewHeight, this._cameraBehaviour.ActiveSensorMode);

            // Top bar
            this._barTop.style.left   = 0;
            this._barTop.style.top    = 0;
            this._barTop.style.width  = viewWidth;
            this._barTop.style.height = rect.Y;

            // Bottom bar
            var bottomY      = rect.Y     + rect.Height;
            var bottomHeight = viewHeight - bottomY;
            this._barBottom.style.left   = 0;
            this._barBottom.style.top    = bottomY;
            this._barBottom.style.width  = viewWidth;
            this._barBottom.style.height = bottomHeight;

            // Left bar
            this._barLeft.style.left   = 0;
            this._barLeft.style.top    = rect.Y;
            this._barLeft.style.width  = rect.X;
            this._barLeft.style.height = rect.Height;

            // Right bar
            var rightX     = rect.X    + rect.Width;
            var rightWidth = viewWidth - rightX;
            this._barRight.style.left   = rightX;
            this._barRight.style.top    = rect.Y;
            this._barRight.style.width  = rightWidth;
            this._barRight.style.height = rect.Height;
        }

        private static VisualElement CreateBar()
        {
            var bar = new VisualElement();
            bar.pickingMode = PickingMode.Ignore;
            bar.AddToClassList("aspect-mask-bar");
            return bar;
        }

        private void Start()
        {
            this._cameraBehaviour   = FindAnyObjectByType<CameraBehaviour>();
            this._viewCameraManager = FindAnyObjectByType<ViewCameraManager>();

            if (this._cameraBehaviour == null)
            {
                Debug.LogWarning("AspectRatioMaskView: No CameraBehaviour found.");
                return;
            }

            var uiDocument = this.GetComponent<UIDocument>();

            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogWarning("AspectRatioMaskView: UIDocument or rootVisualElement is null.");
                return;
            }

            this._root = uiDocument.rootVisualElement;
            StyleSheetLoader.Apply(this._root);
            this.BuildOverlay();
        }

        private void Update()
        {
            if (this._cameraBehaviour != null && this._cameraBehaviour.IsDirectorView)
            {
                this._container.style.display = DisplayStyle.None;
                return;
            }

            if (this._container != null)
            {
                this._container.style.display = DisplayStyle.Flex;
            }

            this.UpdateBars();
        }
    }
}
