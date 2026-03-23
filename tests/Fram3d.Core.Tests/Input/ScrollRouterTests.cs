using FluentAssertions;
using Fram3d.Core.Camera;
using Fram3d.Core.Input;
using Xunit;
namespace Fram3d.Core.Tests.Input
{
    public sealed class ScrollRouterTests
    {
        [Fact]
        public void Route__ReturnsDollyZoom__When__CmdAltScroll()
        {
            var router = new ScrollRouter();

            var action = router.Route(0, 5f, ctrl: false, alt: true, shift: false, cmd: true, 1.0f);

            action.Kind.Should().Be(ScrollActionKind.DOLLY_ZOOM);
            action.Y.Should().Be(5f);
        }

        [Fact]
        public void Route__ReturnsFocusDistance__When__CmdOnlyScroll()
        {
            var router = new ScrollRouter();

            var action = router.Route(0, 5f, ctrl: false, alt: false, shift: false, cmd: true, 1.0f);

            action.Kind.Should().Be(ScrollActionKind.FOCUS_DISTANCE);
        }

        [Fact]
        public void Route__ReturnsDollyTruck__When__CtrlScroll()
        {
            var router = new ScrollRouter();

            var action = router.Route(3f, 5f, ctrl: true, alt: false, shift: false, cmd: false, 1.0f);

            action.Kind.Should().Be(ScrollActionKind.DOLLY_TRUCK);
            action.X.Should().Be(3f);
            action.Y.Should().Be(5f);
        }

        [Fact]
        public void Route__ReturnsCrane__When__AltScroll()
        {
            var router = new ScrollRouter();

            var action = router.Route(0, 5f, ctrl: false, alt: true, shift: false, cmd: false, 1.0f);

            action.Kind.Should().Be(ScrollActionKind.CRANE);
        }

        [Fact]
        public void Route__ReturnsRoll__When__ShiftScrollX()
        {
            var router = new ScrollRouter();

            var action = router.Route(5f, 0, ctrl: false, alt: false, shift: true, cmd: false, 1.0f);

            action.Kind.Should().Be(ScrollActionKind.ROLL);
            action.X.Should().Be(5f);
        }

        [Fact]
        public void Route__ReturnsFocalLength__When__UnmodifiedScroll()
        {
            var router = new ScrollRouter();

            var action = router.Route(0, 5f, ctrl: false, alt: false, shift: false, cmd: false, 1.0f);

            action.Kind.Should().Be(ScrollActionKind.FOCAL_LENGTH);
        }

        // --- Scroll bleed cooldown ---

        [Fact]
        public void Route__BlocksMomentum__When__WithinCooldown()
        {
            var router = new ScrollRouter();

            // Modifier scroll at t=1.0
            router.Route(0, 5f, ctrl: true, alt: false, shift: false, cmd: false, 1.0f);

            // Unmodified scroll at t=1.1 (within 150ms cooldown)
            var action = router.Route(0, 5f, ctrl: false, alt: false, shift: false, cmd: false, 1.1f);

            action.Kind.Should().Be(ScrollActionKind.BLOCKED);
        }

        [Fact]
        public void Route__AllowsScroll__When__AfterCooldownExpires()
        {
            var router = new ScrollRouter();

            // Modifier scroll at t=1.0
            router.Route(0, 5f, ctrl: true, alt: false, shift: false, cmd: false, 1.0f);

            // Unmodified scroll at t=1.2 (after 150ms + margin)
            var action = router.Route(0, 5f, ctrl: false, alt: false, shift: false, cmd: false, 1.2f);

            action.Kind.Should().Be(ScrollActionKind.FOCAL_LENGTH);
        }

        [Fact]
        public void Route__ExtendsCooldown__When__BlockedMomentumEvent()
        {
            var router = new ScrollRouter();

            // Modifier scroll at t=1.0
            router.Route(0, 5f, ctrl: true, alt: false, shift: false, cmd: false, 1.0f);

            // Momentum at t=1.1 — blocked, timer resets to 1.1
            router.Route(0, 5f, ctrl: false, alt: false, shift: false, cmd: false, 1.1f);

            // Another momentum at t=1.2 — still within 150ms of the RESET timer
            var action = router.Route(0, 5f, ctrl: false, alt: false, shift: false, cmd: false, 1.2f);

            action.Kind.Should().Be(ScrollActionKind.BLOCKED);
        }

        [Fact]
        public void Route__AllowsScroll__When__CooldownExpiresAfterExtension()
        {
            var router = new ScrollRouter();

            // Modifier scroll at t=1.0
            router.Route(0, 5f, ctrl: true, alt: false, shift: false, cmd: false, 1.0f);

            // Momentum at t=1.1 — blocked, timer resets to 1.1
            router.Route(0, 5f, ctrl: false, alt: false, shift: false, cmd: false, 1.1f);

            // After extended cooldown: 1.1 + 0.15 = 1.25, try at 1.3
            var action = router.Route(0, 5f, ctrl: false, alt: false, shift: false, cmd: false, 1.3f);

            action.Kind.Should().Be(ScrollActionKind.FOCAL_LENGTH);
        }

        // --- Modifier priority ---

        [Fact]
        public void Route__PrefersDollyZoom__When__CmdAndAltBothHeld()
        {
            var router = new ScrollRouter();

            var action = router.Route(5f, 5f, ctrl: false, alt: true, shift: false, cmd: true, 1.0f);

            action.Kind.Should().Be(ScrollActionKind.DOLLY_ZOOM);
        }

        [Fact]
        public void Route__PrefersCtrl__When__CtrlAndShiftBothHeld()
        {
            var router = new ScrollRouter();

            var action = router.Route(5f, 5f, ctrl: true, alt: false, shift: true, cmd: false, 1.0f);

            action.Kind.Should().Be(ScrollActionKind.DOLLY_TRUCK);
        }
    }
}
