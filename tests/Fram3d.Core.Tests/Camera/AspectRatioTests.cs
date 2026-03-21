using FluentAssertions;
using Fram3d.Core.Camera;
using Xunit;

namespace Fram3d.Core.Tests.Camera
{
	public class AspectRatioTests
	{
		// --- Defaults ---

		[Fact]
		public void Default__Is16x9__When__Accessed()
		{
			AspectRatio.DEFAULT.Should().BeSameAs(AspectRatio.RATIO_16_9);
		}

		[Fact]
		public void ALL__ContainsTenRatios__When__Accessed()
		{
			AspectRatio.ALL.Should().HaveCount(10);
		}

		[Fact]
		public void ALL__StartsWithFullScreen__When__Accessed()
		{
			AspectRatio.ALL[0].Should().BeSameAs(AspectRatio.FULL_SCREEN);
		}

		[Fact]
		public void ALL__EndsWith9x16__When__Accessed()
		{
			AspectRatio.ALL[^1].Should().BeSameAs(AspectRatio.RATIO_9_16);
		}

		// --- Next cycling ---

		[Fact]
		public void Next__Returns16x10__When__CurrentIs16x9()
		{
			AspectRatio.RATIO_16_9.Next().Should().BeSameAs(AspectRatio.RATIO_16_10);
		}

		[Fact]
		public void Next__WrapsToFullScreen__When__CurrentIs9x16()
		{
			AspectRatio.RATIO_9_16.Next().Should().BeSameAs(AspectRatio.FULL_SCREEN);
		}

		[Fact]
		public void Next__Returns16x9__When__CurrentIsFullScreen()
		{
			AspectRatio.FULL_SCREEN.Next().Should().BeSameAs(AspectRatio.RATIO_16_9);
		}

		// --- Previous cycling ---

		[Fact]
		public void Previous__ReturnsFullScreen__When__CurrentIs16x9()
		{
			AspectRatio.RATIO_16_9.Previous().Should().BeSameAs(AspectRatio.FULL_SCREEN);
		}

		[Fact]
		public void Previous__WrapsTo9x16__When__CurrentIsFullScreen()
		{
			AspectRatio.FULL_SCREEN.Previous().Should().BeSameAs(AspectRatio.RATIO_9_16);
		}

		[Fact]
		public void Previous__Returns4x3__When__CurrentIs1x1()
		{
			AspectRatio.RATIO_1_1.Previous().Should().BeSameAs(AspectRatio.RATIO_4_3);
		}

		// --- Full cycle ---

		[Fact]
		public void Next__ReturnsToStart__When__CycledThroughAll()
		{
			var current = AspectRatio.FULL_SCREEN;

			for (var i = 0; i < AspectRatio.ALL.Length; i++)
				current = current.Next();

			current.Should().BeSameAs(AspectRatio.FULL_SCREEN);
		}

		[Fact]
		public void Previous__ReturnsToStart__When__CycledThroughAll()
		{
			var current = AspectRatio.FULL_SCREEN;

			for (var i = 0; i < AspectRatio.ALL.Length; i++)
				current = current.Previous();

			current.Should().BeSameAs(AspectRatio.FULL_SCREEN);
		}

		// --- ComputeUnmaskedRect: Full Screen ---

		[Fact]
		public void ComputeUnmaskedRect__ReturnsFullView__When__FullScreen()
		{
			var rect = AspectRatio.FULL_SCREEN.ComputeUnmaskedRect(1920f, 1080f);
			rect.X.Should().Be(0f);
			rect.Y.Should().Be(0f);
			rect.Width.Should().Be(1920f);
			rect.Height.Should().Be(1080f);
		}

		// --- ComputeUnmaskedRect: Letterbox (wider ratio than view) ---

		[Fact]
		public void ComputeUnmaskedRect__CreatesLetterbox__When__RatioWiderThanView()
		{
			// 2.39:1 on a 16:9 view (1920x1080). 16:9 ≈ 1.78, 2.39 > 1.78 → letterbox.
			var rect = AspectRatio.RATIO_239_1.ComputeUnmaskedRect(1920f, 1080f);
			rect.X.Should().Be(0f);
			rect.Width.Should().Be(1920f);
			rect.Height.Should().BeApproximately(1920f / 2.39f, 0.1f);
			rect.Y.Should().BeApproximately((1080f - rect.Height) / 2f, 0.1f);
		}

		// --- ComputeUnmaskedRect: Pillarbox (narrower ratio than view) ---

		[Fact]
		public void ComputeUnmaskedRect__CreatesPillarbox__When__RatioNarrowerThanView()
		{
			// 4:3 on a 16:9 view (1920x1080). 4:3 ≈ 1.33, 16:9 ≈ 1.78 → pillarbox.
			var rect = AspectRatio.RATIO_4_3.ComputeUnmaskedRect(1920f, 1080f);
			rect.Y.Should().Be(0f);
			rect.Height.Should().Be(1080f);
			rect.Width.Should().BeApproximately(1080f * (4f / 3f), 0.1f);
			rect.X.Should().BeApproximately((1920f - rect.Width) / 2f, 0.1f);
		}

		// --- ComputeUnmaskedRect: Exact match ---

		[Fact]
		public void ComputeUnmaskedRect__ReturnsFullView__When__RatioMatchesView()
		{
			// 16:9 on a 16:9 view — no bars.
			var rect = AspectRatio.RATIO_16_9.ComputeUnmaskedRect(1920f, 1080f);
			rect.X.Should().BeApproximately(0f, 0.1f);
			rect.Y.Should().BeApproximately(0f, 0.1f);
			rect.Width.Should().BeApproximately(1920f, 0.1f);
			rect.Height.Should().BeApproximately(1080f, 0.1f);
		}

		// --- ComputeUnmaskedRect: Portrait window ---

		[Fact]
		public void ComputeUnmaskedRect__CreatesLetterboxInPortrait__When__16x9OnTallWindow()
		{
			// 16:9 on a 1080x1920 portrait window. 16:9 ≈ 1.78 > 1080/1920 ≈ 0.56 → letterbox.
			var rect = AspectRatio.RATIO_16_9.ComputeUnmaskedRect(1080f, 1920f);
			rect.X.Should().Be(0f);
			rect.Width.Should().Be(1080f);
			rect.Height.Should().BeApproximately(1080f / (16f / 9f), 0.1f);
		}

		[Fact]
		public void ComputeUnmaskedRect__CreatesPillarboxInPortrait__When__9x16OnWideWindow()
		{
			// 9:16 on a 1920x1080 landscape window. 9/16 ≈ 0.56 < 1920/1080 ≈ 1.78 → pillarbox.
			var rect = AspectRatio.RATIO_9_16.ComputeUnmaskedRect(1920f, 1080f);
			rect.Y.Should().Be(0f);
			rect.Height.Should().Be(1080f);
			rect.Width.Should().BeApproximately(1080f * (9f / 16f), 0.1f);
		}

		// --- ComputeUnmaskedRect: Centered ---

		[Fact]
		public void ComputeUnmaskedRect__IsCenteredVertically__When__Letterbox()
		{
			var rect = AspectRatio.RATIO_235_1.ComputeUnmaskedRect(1920f, 1080f);
			var topBar    = rect.Y;
			var bottomBar = 1080f - (rect.Y + rect.Height);
			topBar.Should().BeApproximately(bottomBar, 0.1f);
		}

		[Fact]
		public void ComputeUnmaskedRect__IsCenteredHorizontally__When__Pillarbox()
		{
			var rect = AspectRatio.RATIO_4_3.ComputeUnmaskedRect(1920f, 1080f);
			var leftBar  = rect.X;
			var rightBar = 1920f - (rect.X + rect.Width);
			leftBar.Should().BeApproximately(rightBar, 0.1f);
		}

		// --- ComputeUnmaskedRect: Degenerate inputs ---

		[Fact]
		public void ComputeUnmaskedRect__ReturnsFullView__When__ZeroWidth()
		{
			var rect = AspectRatio.RATIO_16_9.ComputeUnmaskedRect(0f, 1080f);
			rect.X.Should().Be(0f);
			rect.Y.Should().Be(0f);
			rect.Width.Should().Be(0f);
			rect.Height.Should().Be(1080f);
		}

		[Fact]
		public void ComputeUnmaskedRect__ReturnsFullView__When__ZeroHeight()
		{
			var rect = AspectRatio.RATIO_16_9.ComputeUnmaskedRect(1920f, 0f);
			rect.X.Should().Be(0f);
			rect.Y.Should().Be(0f);
			rect.Width.Should().Be(1920f);
			rect.Height.Should().Be(0f);
		}

		// --- DisplayName ---

		[Fact]
		public void DisplayName__ReturnsHumanReadable__When__Accessed()
		{
			AspectRatio.FULL_SCREEN.DisplayName.Should().Be("Full Screen");
			AspectRatio.RATIO_16_9.DisplayName.Should().Be("16:9");
			AspectRatio.RATIO_239_1.DisplayName.Should().Be("2.39:1");
		}

		// --- Value ---

		[Fact]
		public void Value__IsNull__When__FullScreen()
		{
			AspectRatio.FULL_SCREEN.Value.Should().BeNull();
		}

		[Fact]
		public void Value__IsCorrectRatio__When__16x9()
		{
			AspectRatio.RATIO_16_9.Value.Should().BeApproximately(16f / 9f, 0.001f);
		}

		[Fact]
		public void Value__IsLessThanOne__When__9x16()
		{
			AspectRatio.RATIO_9_16.Value.Should().BeApproximately(9f / 16f, 0.001f);
		}
	}
}
