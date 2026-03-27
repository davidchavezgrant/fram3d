using System.Collections.Generic;
using Fram3d.Core.Common;
using Fram3d.Core.Scenes;
using UnityEngine;
using SysVector3 = System.Numerics.Vector3;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Duplicates scene elements. Creates a clone of the source GameObject,
    /// assigns an incremented name, applies a world-space offset, and
    /// selects the new element.
    /// </summary>
    public static class ElementDuplicator
    {
        /// <summary>
        /// World-space offset applied to duplicates: +X and +Z in Unity
        /// coordinates (right and forward). In System.Numerics right-handed
        /// coordinates, +Z Unity maps to -Z.
        /// </summary>
        private static readonly SysVector3 OFFSET = new(1f, 0f, -1f);

        /// <summary>
        /// Duplicates the currently selected element. Returns true if a
        /// duplicate was created, false if nothing was selected.
        /// </summary>
        public static bool TryDuplicate(Selection selection)
        {
            if (selection?.SelectedId == null)
            {
                return false;
            }

            var source = FindBehaviour(selection.SelectedId);

            if (source == null)
            {
                return false;
            }

            var existingNames = CollectElementNames();
            var newName       = ElementNaming.GenerateDuplicateName(source.Element.Name, existingNames);
            var clone         = Object.Instantiate(source.gameObject);

            // Awake() already fired on the clone, creating a new Element with
            // a fresh ElementId and the cloned GO name. Overwrite the name and
            // copy properties that Awake doesn't initialize.
            var cloneBehaviour = clone.GetComponent<ElementBehaviour>();
            clone.name                   = newName;
            cloneBehaviour.Element.Name  = newName;
            cloneBehaviour.Element.Scale = source.Element.Scale;

            // Position with offset — Element.Position setter clamps Y to GroundOffset
            cloneBehaviour.Element.Position = source.Element.Position + OFFSET;

            selection.Select(cloneBehaviour.Element.Id);
            return true;
        }

        private static List<string> CollectElementNames()
        {
            var behaviours = Object.FindObjectsByType<ElementBehaviour>(FindObjectsSortMode.None);
            var names      = new List<string>(behaviours.Length);

            foreach (var behaviour in behaviours)
            {
                if (behaviour.Element != null)
                {
                    names.Add(behaviour.Element.Name);
                }
            }

            return names;
        }

        private static ElementBehaviour FindBehaviour(ElementId id)
        {
            var behaviours = Object.FindObjectsByType<ElementBehaviour>(FindObjectsSortMode.None);

            foreach (var behaviour in behaviours)
            {
                if (behaviour.Element != null && behaviour.Element.Id == id)
                {
                    return behaviour;
                }
            }

            return null;
        }
    }
}
