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
        private int      _layerMask;
        private bool     _logNextReject = true;
        private bool     _hadHitLastFrame;
        private Vector2  _lastHitScreenPos;

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

            // Use the resolved screen-space viewport instead of inferring bounds
            // from Camera.rect so hover stays stable across split views and DPI.
            if (!this.targetCamera.pixelRect.Contains(screenPosition))
            {
                if (this._logNextReject)
                {
                    Debug.Log($"[Raycaster] Rejected: pos=({screenPosition.x},{screenPosition.y}), pixelRect={this.targetCamera.pixelRect}");
                    this._logNextReject = false;
                }

                return null;
            }

            this._logNextReject = true;

            var ray = this.targetCamera.ScreenPointToRay(screenPosition);

            if (!Physics.Raycast(ray,
                                 out var hit,
                                 MAX_RAYCAST_DISTANCE,
                                 this._layerMask))
            {
                if (this._hadHitLastFrame)
                {
                    // Ray missed after hitting last frame — log details
                    var anyHit = Physics.Raycast(ray, out var debugHit, MAX_RAYCAST_DISTANCE);
                    Debug.Log($"[Raycaster] Miss after hit! pos=({screenPosition.x},{screenPosition.y}) " +
                              $"lastPos=({this._lastHitScreenPos.x},{this._lastHitScreenPos.y}) " +
                              $"rayOrigin={ray.origin} rayDir={ray.direction} " +
                              $"anyHitNoMask={anyHit}" +
                              (anyHit ? $" hitObj={debugHit.collider.gameObject.name} hitLayer={debugHit.collider.gameObject.layer}" : ""));
                }

                this._hadHitLastFrame = false;
                return null;
            }

            var elementBehaviour = hit.collider.GetComponentInParent<ElementBehaviour>();

            if (elementBehaviour == null)
            {
                if (this._hadHitLastFrame)
                {
                    Debug.Log($"[Raycaster] Hit non-element after element! obj={hit.collider.gameObject.name} " +
                              $"layer={hit.collider.gameObject.layer} pos=({screenPosition.x},{screenPosition.y}) " +
                              $"hitPoint={hit.point} dist={hit.distance:F2}");
                }

                this._hadHitLastFrame = false;
                return null;
            }

            this._hadHitLastFrame  = true;
            this._lastHitScreenPos = screenPosition;

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
