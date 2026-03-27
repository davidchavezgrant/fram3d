using System;
using Fram3d.Core.Scenes;

namespace Fram3d.Core.Common
{
    /// <summary>
    /// Manages which ViewMode is assigned to each view slot. Two slots:
    /// slot 0 defaults to Camera View, slot 1 defaults to Director View.
    /// Enforces the invariant that exactly one slot holds Camera View.
    /// Changing a slot to Camera View triggers a smart swap.
    /// </summary>
    public sealed class ViewSlotModel
    {
        private readonly ViewMode[] _slots = { ViewMode.CAMERA, ViewMode.DIRECTOR };
        private          ViewLayout _layout = ViewLayout.SINGLE;

        /// <summary>
        /// Fires when layout or any slot type changes.
        /// </summary>
        public event Action Changed;

        public ViewLayout Layout         => this._layout;
        public int        ActiveSlotCount => this._layout.ViewCount;

        /// <summary>
        /// Which slot index currently holds Camera View.
        /// </summary>
        public int CameraViewSlotIndex => this._slots[0] == ViewMode.CAMERA ? 0 : 1;

        public ViewMode GetSlotType(int index)
        {
            if (index < 0 || index >= this.ActiveSlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return this._slots[index];
        }

        /// <summary>
        /// Sets the view type for a slot. Only Camera View and Director View
        /// are valid types. Setting a slot to Camera View triggers a smart
        /// swap. The Camera View slot cannot be changed to a different type
        /// directly — it can only move via smart swap.
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
                // Smart swap: move Camera View here, old Camera slot gets this type
                var oldCameraSlot          = this.CameraViewSlotIndex;
                var oldType                = this._slots[index];
                this._slots[index]         = ViewMode.CAMERA;
                this._slots[oldCameraSlot] = oldType;
            }
            else if (this._slots[index] == ViewMode.CAMERA && this.ActiveSlotCount > 1)
            {
                // Multi-view: swap Camera View to the other slot
                for (var i = 0; i < this.ActiveSlotCount; i++)
                {
                    if (i != index)
                    {
                        this._slots[i] = ViewMode.CAMERA;
                        break;
                    }
                }

                this._slots[index] = type;
            }
            else
            {
                // Single-view or non-Camera slot: just change it
                this._slots[index] = type;
            }

            this.Changed?.Invoke();
        }

        /// <summary>
        /// Changes the layout. Preserves existing slot types. If switching
        /// from split to single and Camera View was in slot 1, it moves to
        /// slot 0.
        /// </summary>
        public void SetLayout(ViewLayout layout)
        {
            if (this._layout == layout)
            {
                return;
            }

            this._layout = layout;

            if (layout == ViewLayout.SINGLE)
            {
                // Shrinking: ensure Camera View is in slot 0
                if (this._slots[0] != ViewMode.CAMERA)
                {
                    this._slots[0] = ViewMode.CAMERA;
                }
            }
            else
            {
                // Expanding: ensure exactly one Camera and one Director
                this._slots[0] = ViewMode.CAMERA;
                this._slots[1] = ViewMode.DIRECTOR;
            }

            this.Changed?.Invoke();
        }
    }
}
