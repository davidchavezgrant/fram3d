using Fram3d.Core.Common;
namespace Fram3d.Core.Timelines
{
    /// <summary>
    /// Emitted when elements should evaluate at a global time.
    /// Camera evaluation uses per-shot local time; element evaluation
    /// uses absolute global time because element keyframes are global.
    /// </summary>
    public sealed class ElementEvaluation
    {
        public ElementEvaluation(TimePosition globalTime)
        {
            this.GlobalTime = globalTime;
        }

        public TimePosition GlobalTime { get; }
    }
}