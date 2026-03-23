using FluentAssertions;
using Fram3d.Core.Input;
using Xunit;
namespace Fram3d.Core.Tests.Input
{
    public sealed class DragRouterTests
    {
        [Fact]
        public void Route__ReturnsOrbit__When__AltAndLeftButton()
        {
            var action = DragRouter.Route(50f, 30f, altHeld: true, cmdHeld: false, leftButton: true, middleButton: false);

            action.Kind.Should().BeSameAs(DragActionKind.ORBIT);
            action.DeltaX.Should().Be(50f);
            action.DeltaY.Should().Be(30f);
        }

        [Fact]
        public void Route__ReturnsPanTilt__When__CmdAndLeftButton()
        {
            var action = DragRouter.Route(50f, 30f, altHeld: false, cmdHeld: true, leftButton: true, middleButton: false);

            action.Kind.Should().BeSameAs(DragActionKind.PAN_TILT);
        }

        [Fact]
        public void Route__ReturnsPanTilt__When__MiddleButton()
        {
            var action = DragRouter.Route(50f, 30f, altHeld: false, cmdHeld: false, leftButton: false, middleButton: true);

            action.Kind.Should().BeSameAs(DragActionKind.PAN_TILT);
        }

        [Fact]
        public void Route__ReturnsNone__When__NoModifiersOrButtons()
        {
            var action = DragRouter.Route(50f, 30f, altHeld: false, cmdHeld: false, leftButton: false, middleButton: false);

            action.Kind.Should().BeSameAs(DragActionKind.NONE);
        }

        [Fact]
        public void Route__ReturnsNone__When__ZeroDelta()
        {
            var action = DragRouter.Route(0f, 0f, altHeld: true, cmdHeld: false, leftButton: true, middleButton: false);

            action.Kind.Should().BeSameAs(DragActionKind.NONE);
        }

        [Fact]
        public void Route__RejectsSpike__When__DeltaExceedsThreshold()
        {
            var action = DragRouter.Route(500f, 300f, altHeld: true, cmdHeld: false, leftButton: true, middleButton: false);

            action.Kind.Should().BeSameAs(DragActionKind.NONE);
        }

        [Fact]
        public void Route__PrefersOrbit__When__AltAndCmdBothHeld()
        {
            var action = DragRouter.Route(50f, 30f, altHeld: true, cmdHeld: true, leftButton: true, middleButton: false);

            action.Kind.Should().BeSameAs(DragActionKind.ORBIT);
        }
    }
}
