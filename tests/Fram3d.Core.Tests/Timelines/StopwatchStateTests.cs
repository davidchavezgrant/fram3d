using FluentAssertions;
using Fram3d.Core.Timelines;
using Xunit;

namespace Fram3d.Core.Tests.Timelines
{
    public sealed class StopwatchStateTests
    {
        [Fact]
        public void IsRecording__ReturnsFalse__When__Default()
        {
            var state = new StopwatchState(3);

            state.IsRecording(0).Should().BeFalse();
            state.IsRecording(1).Should().BeFalse();
            state.IsRecording(2).Should().BeFalse();
        }

        [Fact]
        public void AnyRecording__ReturnsFalse__When__Default()
        {
            var state = new StopwatchState(3);

            state.AnyRecording.Should().BeFalse();
        }

        [Fact]
        public void AllRecording__ReturnsFalse__When__Default()
        {
            var state = new StopwatchState(3);

            state.AllRecording.Should().BeFalse();
        }

        [Fact]
        public void SetAll__EnablesAll__When__CalledWithTrue()
        {
            var state = new StopwatchState(3);

            state.SetAll(true);

            state.IsRecording(0).Should().BeTrue();
            state.IsRecording(1).Should().BeTrue();
            state.IsRecording(2).Should().BeTrue();
        }

        [Fact]
        public void AllRecording__ReturnsTrue__When__AllEnabled()
        {
            var state = new StopwatchState(3);

            state.SetAll(true);

            state.AllRecording.Should().BeTrue();
        }

        [Fact]
        public void AllRecording__ReturnsFalse__When__OnlyOneEnabled()
        {
            var state = new StopwatchState(3);

            state.Set(1, true);

            state.AllRecording.Should().BeFalse();
        }

        [Fact]
        public void AnyRecording__ReturnsTrue__When__OneEnabled()
        {
            var state = new StopwatchState(3);

            state.Set(0, true);

            state.AnyRecording.Should().BeTrue();
        }

        [Fact]
        public void Set__EnablesSingleProperty__When__Called()
        {
            var state = new StopwatchState(3);

            state.Set(1, true);

            state.IsRecording(0).Should().BeFalse();
            state.IsRecording(1).Should().BeTrue();
            state.IsRecording(2).Should().BeFalse();
        }

        [Fact]
        public void SetAll__DisablesAll__When__CalledWithFalse()
        {
            var state = new StopwatchState(3);
            state.SetAll(true);

            state.SetAll(false);

            state.IsRecording(0).Should().BeFalse();
            state.IsRecording(1).Should().BeFalse();
            state.IsRecording(2).Should().BeFalse();
        }

        [Fact]
        public void Toggle__FlipsSingle__When__Called()
        {
            var state = new StopwatchState(3);

            state.Toggle(2);

            state.IsRecording(0).Should().BeFalse();
            state.IsRecording(1).Should().BeFalse();
            state.IsRecording(2).Should().BeTrue();

            state.Toggle(2);

            state.IsRecording(2).Should().BeFalse();
        }

        [Fact]
        public void ToggleAll__DisablesAll__When__AnyEnabled()
        {
            var state = new StopwatchState(3);
            state.Set(0, true);

            state.ToggleAll();

            state.IsRecording(0).Should().BeFalse();
            state.IsRecording(1).Should().BeFalse();
            state.IsRecording(2).Should().BeFalse();
        }

        [Fact]
        public void ToggleAll__EnablesAll__When__NoneEnabled()
        {
            var state = new StopwatchState(3);

            state.ToggleAll();

            state.IsRecording(0).Should().BeTrue();
            state.IsRecording(1).Should().BeTrue();
            state.IsRecording(2).Should().BeTrue();
        }
    }
}
