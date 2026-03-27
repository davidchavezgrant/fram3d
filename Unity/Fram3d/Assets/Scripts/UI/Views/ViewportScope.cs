using Fram3d.Engine.Integration;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fram3d.UI.Views
{
    /// <summary>
    /// Positions a UI Toolkit container to match the Camera View viewport.
    /// In single-view, accounts for the properties panel and timeline insets.
    /// In multi-view, scopes to the Camera View slot's viewport rect.
    /// Also exposes shared screen-to-CSS conversion helpers used by multiple views.
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

            if (viewCameraManager == null || !viewCameraManager.IsMultiView)
            {
                var rightCss  = ScreenToCss(root, rightInsetPixels);
                var bottomCss = ScreenToCss(root, bottomInsetPx);
                container.style.left   = 0;
                container.style.top    = 0;
                container.style.right  = rightCss;
                container.style.bottom = bottomCss;
                container.style.width  = StyleKeyword.Auto;
                container.style.height = StyleKeyword.Auto;
                return;
            }

            var vpRect = viewCameraManager.CameraViewRect;
            ViewportRectToCss(root, vpRect, bottomInsetPx,
                out var left, out var top, out var width, out var height);
            container.style.left   = left;
            container.style.top    = top;
            container.style.width  = width;
            container.style.height = height;
            container.style.right  = StyleKeyword.Auto;
            container.style.bottom = StyleKeyword.Auto;
        }

        /// <summary>
        /// Converts CSS pixels to screen pixels based on the root element's width.
        /// </summary>
        public static float CssToScreen(VisualElement root, float cssPixels)
        {
            var w     = root.resolvedStyle.width;
            var scale = Screen.width > 0 && !float.IsNaN(w) && w > 0
                      ? (float)Screen.width / w
                      : 1f;

            return cssPixels * scale;
        }

        /// <summary>
        /// Converts screen pixels to CSS pixels based on the root element's width.
        /// </summary>
        public static float ScreenToCss(VisualElement root, float screenPixels)
        {
            var w     = root.resolvedStyle.width;
            var scale = Screen.width > 0 && !float.IsNaN(w) && w > 0
                      ? (float)Screen.width / w
                      : 1f;

            return screenPixels / scale;
        }

        /// <summary>
        /// Converts a Camera.rect viewport rectangle to CSS pixel coordinates,
        /// accounting for the bottom inset (timeline section).
        /// </summary>
        public static void ViewportRectToCss(VisualElement root, Rect vpRect,
                                              float bottomInsetPixels,
                                              out float left, out float top,
                                              out float width, out float height)
        {
            var rootW = root.resolvedStyle.width;
            var rootH = root.resolvedStyle.height;

            if (float.IsNaN(rootW) || float.IsNaN(rootH))
            {
                left = top = width = height = 0f;
                return;
            }

            var bottomCss    = ScreenToCss(root, bottomInsetPixels);
            var availableCss = rootH - bottomCss;
            var bottomNorm   = Screen.height > 0 ? bottomInsetPixels / Screen.height : 0f;
            var normRange    = 1f - bottomNorm;

            if (normRange <= 0)
            {
                normRange = 1f;
            }

            left   = vpRect.x * rootW;
            top    = (1f - vpRect.y - vpRect.height) / normRange * availableCss;
            width  = vpRect.width * rootW;
            height = vpRect.height / normRange * availableCss;
        }
    }
}
