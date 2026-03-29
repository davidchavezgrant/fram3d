namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Tracks per-property recording state. Each property slot is independently
    /// toggled on/off. Used by Shot (camera properties) and ElementTrack (element properties).
    /// </summary>
    public sealed class StopwatchState
    {
        private readonly bool[] _recording;

        public StopwatchState(int propertyCount)
        {
            this._recording = new bool[propertyCount];
        }

        public bool AllRecording
        {
            get
            {
                foreach (var r in this._recording)
                {
                    if (!r)
                    {
                        return false;
                    }
                }

                return this._recording.Length > 0;
            }
        }

        public bool AnyRecording
        {
            get
            {
                foreach (var r in this._recording)
                {
                    if (r)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsRecording(int propertyIndex) => this._recording[propertyIndex];

        public void Set(int propertyIndex, bool recording) =>
            this._recording[propertyIndex] = recording;

        public void SetAll(bool recording)
        {
            for (var i = 0; i < this._recording.Length; i++)
            {
                this._recording[i] = recording;
            }
        }

        public void Toggle(int propertyIndex) =>
            this._recording[propertyIndex] = !this._recording[propertyIndex];

        public void ToggleAll()
        {
            var newValue = !this.AnyRecording;
            this.SetAll(newValue);
        }
    }
}
