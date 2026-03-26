using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fram3d.UI.Views
{
    /// <summary>
    /// Positions a UI Toolkit container to match the Camera View viewport.
    /// In single-view, accounts for the properties panel inset. In multi-view,
    /// scopes to the Camera View slot's viewport rect.
    /// </summary>
    internal static class ViewportScope
    {
        public static void Apply(VisualElement container, VisualElement root,
                                  ViewCameraManager viewCameraManager, float rightInsetPixels)
        {
            var rootW = root.resolvedStyle.width;
            var rootH = root.resolvedStyle.height;

            if (viewCameraManager == null || !viewCameraManager.IsMultiView)
            {
                var cssInset = ComputeCssInset(root, rightInsetPixels);
                container.style.left   = 0;
                container.style.top    = 0;
                container.style.right  = cssInset;
                container.style.bottom = 0;
                container.style.width  = StyleKeyword.Auto;
                container.style.height = StyleKeyword.Auto;
                return;
            }

            if (float.IsNaN(rootW) || float.IsNaN(rootH))
            {
                return;
            }

            var vpRect = viewCameraManager.CameraViewRect;

            container.style.left   = vpRect.x * rootW;
            container.style.top    = (1f - vpRect.y - vpRect.height) * rootH;
            container.style.width  = vpRect.width  * rootW;
            container.style.height = vpRect.height * rootH;
            container.style.right  = StyleKeyword.Auto;
            container.style.bottom = StyleKeyword.Auto;
        }

        private static float ComputeCssInset(VisualElement root, float rightInsetPixels)
        {
            var w     = root.resolvedStyle.width;
            var scale = Screen.width > 0 && !float.IsNaN(w) && w > 0
                      ? (float)Screen.width / w
                      : 1f;

            return rightInsetPixels / scale;
        }
    }
}
