using System;
using Fram3d.Core.Common;
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
    /// GameObject's name. Pure C# objects don't survive Unity's
    /// editor→play mode serialization boundary, so the Element must be
    /// created at runtime.
    /// </summary>
    public sealed class ElementBehaviour: MonoBehaviour
    {
        public Element Element { get; private set; }

        private void Awake()
        {
            this.Element = new Element(new ElementId(Guid.NewGuid()), this.gameObject.name);
        }
    }
}
