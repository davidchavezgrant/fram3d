using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.UI.Panels
{
    /// <summary>
    /// Loads the fram3d.uss stylesheet from Resources and applies it
    /// to a root VisualElement. Caches the loaded asset.
    /// </summary>
    public static class StyleSheetLoader
    {
        private static StyleSheet _cached;

        public static void Apply(VisualElement root)
        {
            if (_cached == null)
            {
                _cached = Resources.Load<StyleSheet>("fram3d");
            }

            if (_cached != null && !root.styleSheets.Contains(_cached))
            {
                root.styleSheets.Add(_cached);
            }
        }
    }
}
