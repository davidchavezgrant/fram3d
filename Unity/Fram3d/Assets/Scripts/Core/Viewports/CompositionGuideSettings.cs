namespace Fram3d.Core.Viewports
{
    /// <summary>
    /// Visibility state and configuration for composition guides (rule of thirds,
    /// center cross, safe zones). Global state — not per-shot.
    /// </summary>
    public sealed class CompositionGuideSettings
    {
        private const float DEFAULT_ACTION_SAFE_PERCENT = 0.93f;
        private const float DEFAULT_TITLE_SAFE_PERCENT  = 0.90f;
        private       bool  _rememberedCenterCross;
        private       bool  _rememberedSafeZones;
        private       bool  _rememberedThirds;
        public        float ActionSafePercent  { get; set; } = DEFAULT_ACTION_SAFE_PERCENT;
        public        bool  AnyVisible         => this.ThirdsVisible || this.CenterCrossVisible || this.SafeZonesVisible;
        public        bool  CenterCrossVisible { get; private set; }
        public        bool  SafeZonesVisible   { get; private set; }
        public        bool  ThirdsVisible      { get; private set; }
        public        float TitleSafePercent   { get; set; } = DEFAULT_TITLE_SAFE_PERCENT;

        /// <summary>
        /// If any guide is visible, hide all and remember which were on.
        /// If none are visible, restore the previously remembered set
        /// (or show all if nothing was previously enabled).
        /// </summary>
        public void ToggleAll()
        {
            if (this.AnyVisible)
            {
                this._rememberedThirds      = this.ThirdsVisible;
                this._rememberedCenterCross = this.CenterCrossVisible;
                this._rememberedSafeZones   = this.SafeZonesVisible;
                this.ThirdsVisible          = false;
                this.CenterCrossVisible     = false;
                this.SafeZonesVisible       = false;
                return;
            }

            var anyRemembered = this._rememberedThirds || this._rememberedCenterCross || this._rememberedSafeZones;

            if (anyRemembered)
            {
                this.ThirdsVisible      = this._rememberedThirds;
                this.CenterCrossVisible = this._rememberedCenterCross;
                this.SafeZonesVisible   = this._rememberedSafeZones;
            }
            else
            {
                this.ThirdsVisible      = true;
                this.CenterCrossVisible = true;
                this.SafeZonesVisible   = true;
            }
        }

        public void ToggleCenterCross() => this.CenterCrossVisible = !this.CenterCrossVisible;
        public void ToggleSafeZones()   => this.SafeZonesVisible = !this.SafeZonesVisible;
        public void ToggleThirds()      => this.ThirdsVisible = !this.ThirdsVisible;
    }
}