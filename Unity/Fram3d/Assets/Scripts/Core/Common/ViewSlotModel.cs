using System;
using Fram3d.Core.Scene;

namespace Fram3d.Core.Common
{
    /// <summary>
    /// Manages which ViewMode is assigned to each view slot. Enforces the
    /// invariant that exactly one slot holds Camera View at all times.
    /// Changing a slot to Camera View triggers a smart swap — the slot that
    /// previously held Camera View receives the requesting slot's old type.
    /// </summary>
    public sealed class ViewSlotModel
    {
        private readonly ViewMode[] _slots = { ViewMode.CAMERA, ViewMode.DIRECTOR, ViewMode.DESIGNER };
        private          ViewLayout _layout = ViewLayout.SINGLE;

        /// <summary>
        /// Fires when layout or any slot type changes. The subscriber should
        /// rebuild the view structure from scratch.
        /// </summary>
        public event Action Changed;

        public ViewLayout Layout         => this._layout;
        public int        ActiveSlotCount => this._layout.ViewCount;

        /// <summary>
        /// Which slot index currently holds Camera View.
        /// </summary>
        public int CameraViewSlotIndex
        {
            get
            {
                for (var i = 0; i < this._slots.Length; i++)
                {
                    if (this._slots[i] == ViewMode.CAMERA)
                    {
                        return i;
                    }
                }

                return 0;
            }
        }

        public ViewMode GetSlotType(int index)
        {
            if (index < 0 || index >= this.ActiveSlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return this._slots[index];
        }

        /// <summary>
        /// Sets the view type for a slot. If the requested type is Camera View,
        /// performs a smart swap: the slot that currently holds Camera View
        /// receives this slot's old type. Camera View always exists in exactly
        /// one slot.
        /// </summary>
        public void SetSlotType(int index, ViewMode type)
        {
            if (index < 0 || index >= this.ActiveSlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (this._slots[index] == type)
            {
                return;
            }

            if (type == ViewMode.CAMERA)
            {
                var oldCameraSlot = this.CameraViewSlotIndex;
                var oldType       = this._slots[index];
                this._slots[index]         = ViewMode.CAMERA;
                this._slots[oldCameraSlot] = oldType;
            }
            else if (this._slots[index] == ViewMode.CAMERA)
            {
                // Cannot remove Camera View — it must always exist in one slot.
                // Ignore the request silently.
                return;
            }
            else
            {
                this._slots[index] = type;
            }

            this.Changed?.Invoke();
        }

        /// <summary>
        /// Changes the layout. Preserves existing slot types where possible.
        /// New slots receive defaults: slot 1 → Director View, slot 2 → Designer View.
        /// If Camera View was in a slot beyond the new layout's count, it moves
        /// to slot 0.
        /// </summary>
        public void SetLayout(ViewLayout layout)
        {
            if (this._layout == layout)
            {
                return;
            }

            var oldCount = this._layout.ViewCount;
            this._layout = layout;
            var newCount = layout.ViewCount;

            // If we're expanding, fill new slots with defaults
            if (newCount > oldCount)
            {
                for (var i = oldCount; i < newCount; i++)
                {
                    this._slots[i] = DefaultForSlot(i);
                }

                // Resolve duplicates: if a new slot's default matches an
                // existing slot's type (and it's not Camera View), reassign
                for (var i = oldCount; i < newCount; i++)
                {
                    if (this._slots[i] == ViewMode.CAMERA)
                    {
                        continue;
                    }

                    for (var j = 0; j < i; j++)
                    {
                        if (this._slots[j] == this._slots[i])
                        {
                            this._slots[i] = FindUnusedType(newCount);
                            break;
                        }
                    }
                }
            }

            // If Camera View is now beyond visible slots, move it to slot 0
            if (this.CameraViewSlotIndex >= newCount)
            {
                this._slots[this.CameraViewSlotIndex] = DefaultForSlot(this.CameraViewSlotIndex);
                this._slots[0]                         = ViewMode.CAMERA;
            }

            this.Changed?.Invoke();
        }

        private static ViewMode DefaultForSlot(int index)
        {
            if (index == 0)
            {
                return ViewMode.CAMERA;
            }

            if (index == 1)
            {
                return ViewMode.DIRECTOR;
            }

            return ViewMode.DESIGNER;
        }

        private ViewMode FindUnusedType(int slotCount)
        {
            var types = new[] { ViewMode.DIRECTOR, ViewMode.DESIGNER };

            foreach (var type in types)
            {
                var used = false;

                for (var i = 0; i < slotCount; i++)
                {
                    if (this._slots[i] == type)
                    {
                        used = true;
                        break;
                    }
                }

                if (!used)
                {
                    return type;
                }
            }

            // All types in use — fallback to Director
            return ViewMode.DIRECTOR;
        }
    }
}
