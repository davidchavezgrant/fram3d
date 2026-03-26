using UnityEngine;
using UnityEngine.InputSystem;

namespace Fram3d.Engine.Cursor
{
    internal sealed class SoftwareCursorOverlay : MonoBehaviour
    {
        private static SoftwareCursorOverlay _instance;

        private Vector2   _hotspot;
        private Texture2D _texture;

        public static SoftwareCursorOverlay EnsureCreated()
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindAnyObjectByType<SoftwareCursorOverlay>();

            if (_instance != null)
            {
                return _instance;
            }

            var overlayObject = new GameObject("SoftwareCursorOverlay");
            overlayObject.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(overlayObject);
            _instance = overlayObject.AddComponent<SoftwareCursorOverlay>();
            return _instance;
        }

        public void Hide()
        {
            this._texture = null;
        }

        public void Show(Texture2D texture, Vector2 hotspot)
        {
            this._texture = texture;
            this._hotspot = hotspot;
        }

        private void OnGUI()
        {
            if (this._texture == null || Mouse.current == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            GUI.depth = -1000;

            var mousePosition = Mouse.current.position.ReadValue();
            var rect          = new Rect(Mathf.Round(mousePosition.x - this._hotspot.x),
                                         Mathf.Round(Screen.height - mousePosition.y - this._hotspot.y),
                                         this._texture.width,
                                         this._texture.height);

            GUI.DrawTexture(rect, this._texture, ScaleMode.StretchToFill, true);
        }
    }
}
