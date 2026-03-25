using System;
using Fram3d.Core.Common;
using Fram3d.Engine.Conversion;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Thin MonoBehaviour wrapper for a domain Element. Placed on the root
    /// GameObject of each interactive scene element. SelectionRaycaster walks
    /// up the transform hierarchy looking for this component to resolve
    /// compound elements (multi-mesh models) as single selectable units.
    ///
    /// Self-initializes in Awake — creates the domain Element from the
    /// GameObject's name and syncs Core transform state to Unity each frame.
    ///
    /// For elements that wrap an existing Core object (e.g., the frustum
    /// wireframe wrapping the shot CameraElement), set Element before Awake
    /// via the internal setter. Awake skips auto-creation when Element is
    /// already set.
    /// </summary>
    public sealed class ElementBehaviour: MonoBehaviour
    {
        public Element Element { get; internal set; }

        private void Awake()
        {
            if (this.Element != null)
            {
                return;
            }

            this.Element = new Element(new ElementId(Guid.NewGuid()), this.gameObject.name);
            this.Element.GroundOffset = this.ComputeGroundOffset();

            // Initialize Core position from the scene's editor-time placement
            this.Element.Position = this.transform.position.ToSystem();
            this.Element.Rotation = this.transform.rotation.ToSystem();
        }

        /// <summary>
        /// Computes the distance from the element's origin to its lowest
        /// geometry point, so the Position setter can prevent the visible
        /// mesh from clipping through Y=0.
        /// </summary>
        private float ComputeGroundOffset()
        {
            var renderer = this.GetComponentInChildren<Renderer>();

            if (renderer == null)
            {
                return 0f;
            }

            return this.transform.position.y - renderer.bounds.min.y;
        }

        /// <summary>
        /// Syncs Core domain state → Unity Transform each frame.
        /// During gizmo drag, the gizmo writes directly to Element.Position;
        /// this sync propagates that to the visible scene immediately.
        /// </summary>
        private void LateUpdate()
        {
            this.transform.position   = this.Element.Position.ToUnity();
            this.transform.rotation   = this.Element.Rotation.ToUnity();
            this.transform.localScale = Vector3.one * this.Element.Scale;
        }
    }
}