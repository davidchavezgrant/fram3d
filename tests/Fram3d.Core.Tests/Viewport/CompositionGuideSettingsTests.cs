using FluentAssertions;
using Fram3d.Core.Cameras;
using Fram3d.Core.Viewports;
using Xunit;

namespace Fram3d.Core.Tests.Viewport
{
	public class CompositionGuideSettingsTests
	{
		// --- Defaults ---

		[Fact]
		public void Constructor__AllHidden__When__Created()
		{
			var settings = new CompositionGuideSettings();

			settings.ThirdsVisible.Should().BeFalse();
			settings.CenterCrossVisible.Should().BeFalse();
			settings.SafeZonesVisible.Should().BeFalse();
			settings.AnyVisible.Should().BeFalse();
		}

		[Fact]
		public void Constructor__DefaultSafeZonePercentages__When__Created()
		{
			var settings = new CompositionGuideSettings();

			settings.TitleSafePercent.Should().Be(0.90f);
			settings.ActionSafePercent.Should().Be(0.93f);
		}

		// --- Individual toggles ---

		[Fact]
		public void ToggleThirds__ShowsThirds__When__Hidden()
		{
			var settings = new CompositionGuideSettings();

			settings.ToggleThirds();

			settings.ThirdsVisible.Should().BeTrue();
			settings.CenterCrossVisible.Should().BeFalse();
			settings.SafeZonesVisible.Should().BeFalse();
		}

		[Fact]
		public void ToggleThirds__HidesThirds__When__Visible()
		{
			var settings = new CompositionGuideSettings();
			settings.ToggleThirds();

			settings.ToggleThirds();

			settings.ThirdsVisible.Should().BeFalse();
		}

		[Fact]
		public void ToggleCenterCross__ShowsCenterCross__When__Hidden()
		{
			var settings = new CompositionGuideSettings();

			settings.ToggleCenterCross();

			settings.CenterCrossVisible.Should().BeTrue();
			settings.ThirdsVisible.Should().BeFalse();
		}

		[Fact]
		public void ToggleSafeZones__ShowsSafeZones__When__Hidden()
		{
			var settings = new CompositionGuideSettings();

			settings.ToggleSafeZones();

			settings.SafeZonesVisible.Should().BeTrue();
			settings.ThirdsVisible.Should().BeFalse();
		}

		[Fact]
		public void AnyVisible__ReturnsTrue__When__OneGuideEnabled()
		{
			var settings = new CompositionGuideSettings();
			settings.ToggleCenterCross();

			settings.AnyVisible.Should().BeTrue();
		}

		// --- ToggleAll: hide all ---

		[Fact]
		public void ToggleAll__HidesAll__When__AnyVisible()
		{
			var settings = new CompositionGuideSettings();
			settings.ToggleThirds();
			settings.ToggleSafeZones();

			settings.ToggleAll();

			settings.ThirdsVisible.Should().BeFalse();
			settings.CenterCrossVisible.Should().BeFalse();
			settings.SafeZonesVisible.Should().BeFalse();
		}

		// --- ToggleAll: restore remembered ---

		[Fact]
		public void ToggleAll__RestoresRemembered__When__ToggledBackOn()
		{
			var settings = new CompositionGuideSettings();
			settings.ToggleThirds();
			settings.ToggleSafeZones();
			// Thirds and SafeZones are on, CenterCross is off

			settings.ToggleAll(); // hide all, remember {thirds, safeZones}
			settings.ToggleAll(); // restore

			settings.ThirdsVisible.Should().BeTrue();
			settings.CenterCrossVisible.Should().BeFalse();
			settings.SafeZonesVisible.Should().BeTrue();
		}

		// --- ToggleAll: show all when nothing remembered ---

		[Fact]
		public void ToggleAll__ShowsAll__When__NothingPreviouslyEnabled()
		{
			var settings = new CompositionGuideSettings();

			settings.ToggleAll();

			settings.ThirdsVisible.Should().BeTrue();
			settings.CenterCrossVisible.Should().BeTrue();
			settings.SafeZonesVisible.Should().BeTrue();
		}

		// --- ToggleAll: only one was on ---

		[Fact]
		public void ToggleAll__RestoresOnlyOne__When__OnlyOneWasEnabled()
		{
			var settings = new CompositionGuideSettings();
			settings.ToggleCenterCross();

			settings.ToggleAll(); // hide all, remember {centerCross}
			settings.ToggleAll(); // restore

			settings.ThirdsVisible.Should().BeFalse();
			settings.CenterCrossVisible.Should().BeTrue();
			settings.SafeZonesVisible.Should().BeFalse();
		}

		// --- Safe zone configuration ---

		[Fact]
		public void TitleSafePercent__CanBeChanged__When__SetDirectly()
		{
			var settings = new CompositionGuideSettings();

			settings.TitleSafePercent = 0.85f;

			settings.TitleSafePercent.Should().Be(0.85f);
		}

		[Fact]
		public void ActionSafePercent__CanBeChanged__When__SetDirectly()
		{
			var settings = new CompositionGuideSettings();

			settings.ActionSafePercent = 0.95f;

			settings.ActionSafePercent.Should().Be(0.95f);
		}
	}
}
