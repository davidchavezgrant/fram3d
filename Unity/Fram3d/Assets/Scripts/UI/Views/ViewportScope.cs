using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fram3d.UI.Views
{
    /// <summary>
    /// Positions a UI Toolkit container to match the Camera View viewport.
    /// In single-view, accounts for the properties panel and timeline insets.
    /// In multi-view, scopes to the Camera View slot's viewport rect.
    /// </summary>
    internal static class ViewportScope
    {
        public static void Apply(VisualElement container, VisualElement root,
                                  ViewCameraManager viewCameraManager, float rightInsetPixels)
        {
            var rootW = root.resolvedStyle.width;
            var rootH = root.resolvedStyle.height;

            if (float.IsNaN(rootW) || float.IsNaN(rootH) || rootW <= 0 || rootH <= 0)
            {
                return;
            }

            var bottomInsetPx = 0f;

            if (viewCameraManager != null && viewCameraManager.CameraBehaviour != null)
            {
                bottomInsetPx = viewCameraManager.CameraBehaviour.BottomInsetPixels;
            }

            var bottomCss = ComputeCssInset(root, bottomInsetPx);

            if (viewCameraManager == null || !viewCameraManager.IsMultiView)
            {
                var rightCss = ComputeCssInset(root, rightInsetPixels);
                container.style.left   = 0;
                container.style.top    = 0;
                container.style.right  = rightCss;
                container.style.bottom = bottomCss;
                container.style.width  = StyleKeyword.Auto;
                container.style.height = StyleKeyword.Auto;
                return;
            }

            // Multi-view: convert Camera.rect (normalized, bottom-up, full-screen)
            // to CSS (top-down, above the timeline). Camera rects include the
            // bottomNorm offset. We need to map from [bottomNorm..1.0] normalized
            // range to [0..availableCss] CSS range.
            var vpRect       = viewCameraManager.CameraViewRect;
            var availableCss = rootH - bottomCss;
            var bottomNorm   = bottomInsetPx > 0 && Screen.height > 0
                ? bottomInsetPx / Screen.height
                : 0f;
            var normRange    = 1f - bottomNorm;

            if (normRange <= 0)
            {
                normRange = 1f;
            }

            container.style.left   = vpRect.x * rootW;
            container.style.top    = (1f - vpRect.y - vpRect.height) / normRange * availableCss;
            container.style.width  = vpRect.width * rootW;
            container.style.height = vpRect.height / normRange * availableCss;
            container.style.right  = StyleKeyword.Auto;
            container.style.bottom = StyleKeyword.Auto;
        }

        private static float ComputeCssInset(VisualElement root, float screenPixels)
        {
            var w     = root.resolvedStyle.width;
            var scale = Screen.width > 0 && !float.IsNaN(w) && w > 0
                      ? (float)Screen.width / w
                      : 1f;

            return screenPixels / scale;
        }
    }
}
