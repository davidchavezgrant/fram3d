using Fram3d.Core.Common;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Casts a ray from the camera through a screen position and resolves
    /// the hit to a domain Element. Compound elements (multi-mesh models)
    /// are resolved by walking up the transform hierarchy to find the
    /// nearest ElementBehaviour.
    /// </summary>
    public sealed class SelectionRaycaster: MonoBehaviour
    {
        private const float MAX_RAYCAST_DISTANCE = 1000f;

        /// <summary>
        /// Layer mask excluding gizmos and UI. Set during Awake from the
        /// camera's culling mask, minus any layers we want to ignore.
        /// If a dedicated "Gizmo" layer exists, it is excluded automatically.
        /// </summary>
        private int _layerMask;

        [SerializeField]
        private Camera targetCamera;

        public void SetCamera(Camera camera)
        {
            if (camera != null)
            {
                this.targetCamera = camera;
            }
        }

        /// <summary>
        /// Casts a ray from the camera through the given screen position.
        /// Returns the Element under the cursor, or null if no element was hit.
        /// </summary>
        public Element Raycast(Vector2 screenPosition)
        {
            if (this.targetCamera == null)
            {
                return null;
            }

            // Skip if the screen position is outside this camera's viewport
            var vpRect = this.targetCamera.rect;
            var normX  = screenPosition.x / Screen.width;
            var normY  = screenPosition.y / Screen.height;

            if (normX < vpRect.x || normX > vpRect.x + vpRect.width
             || normY < vpRect.y || normY > vpRect.y + vpRect.height)
            {
                return null;
            }

            var ray = this.targetCamera.ScreenPointToRay(screenPosition);

            if (!Physics.Raycast(ray,
                                 out var hit,
                                 MAX_RAYCAST_DISTANCE,
                                 this._layerMask))
            {
                return null;
            }

            var elementBehaviour = hit.collider.GetComponentInParent<ElementBehaviour>();

            if (elementBehaviour == null)
            {
                return null;
            }

            return elementBehaviour.Element;
        }

        private void Awake()
        {
            if (this.targetCamera == null)
            {
                this.targetCamera = this.GetComponent<Camera>();
            }

            // Start with the Default layer. Exclude UI and IgnoreRaycast.
            this._layerMask = ~(LayerMask.GetMask("UI", "Ignore Raycast"));

            // Exclude a "Gizmo" layer if it exists (for future gizmo support).
            var gizmoLayer = LayerMask.NameToLayer("Gizmo");

            if (gizmoLayer >= 0)
            {
                this._layerMask &= ~(1 << gizmoLayer);
            }
        }
    }
}