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

            if (!this.targetCamera.pixelRect.Contains(screenPosition))
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

            this._layerMask = ~(LayerMask.GetMask("UI", "Ignore Raycast"));

            var gizmoLayer = LayerMask.NameToLayer("Gizmo");

            if (gizmoLayer >= 0)
            {
                this._layerMask &= ~(1 << gizmoLayer);
            }
        }
    }
}
