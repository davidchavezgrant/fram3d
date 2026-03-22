using FluentAssertions;
using Fram3d.Core.Camera;
using Xunit;

namespace Fram3d.Core.Tests.Camera
{
	public class CameraBodyTests
	{
		private static readonly SensorMode OPEN_GATE = new("Open Gate", 5120, 2700, 29.90f, 15.77f, 60);
		private static readonly SensorMode C4K       = new("C4K", 4096, 2160, 24.89f, 13.12f, 60);
		private static readonly SensorMode CROP      = new("2.5K Crop", 2560, 1350, 0f, 0f, 120);

		private static CameraBody CreateBodyWithModes() =>
			new("RED DSMC2", "RED", 2016, 29.90f, 15.77f, "S35", "RF",
				new[] { 5120, 2700 }, new[] { 24, 60, 120 },
				new[] { OPEN_GATE, C4K, CROP });

		private static CameraBody CreateBodyWithoutModes() =>
			new("Canon R5", "Canon", 2020, 36.0f, 24.0f, "FF", "RF",
				new[] { 8192, 5464 }, new[] { 24, 30, 60 });

		// --- ComputeGateWidth ---

		[Fact]
		public void ComputeGateWidth__ReturnsModeWidth__When__ModeHasExplicitSensorArea()
		{
			var body = CreateBodyWithModes();

			body.ComputeGateWidth(C4K).Should().Be(24.89f);
		}

		[Fact]
		public void ComputeGateWidth__ReturnsBodyWidth__When__ModeIsNull()
		{
			var body = CreateBodyWithModes();

			body.ComputeGateWidth(null).Should().Be(29.90f);
		}

		[Fact]
		public void ComputeGateWidth__DerivesFromOpenGate__When__ModeHasNoSensorArea()
		{
			var body = CreateBodyWithModes();

			// Open gate is 5120 wide at 29.90mm. Crop is 2560 wide → 29.90 * (2560/5120) = 14.95
			body.ComputeGateWidth(CROP).Should().BeApproximately(14.95f, 0.01f);
		}

		[Fact]
		public void ComputeGateWidth__ReturnsBodyWidth__When__BodyHasNoSensorModes()
		{
			var body = CreateBodyWithoutModes();
			var externalMode = new SensorMode("HD", 1920, 1080, 0f, 0f, 60);

			body.ComputeGateWidth(externalMode).Should().Be(36.0f);
		}

		[Fact]
		public void ComputeGateWidth__ReturnsOpenGateWidth__When__OpenGateMode()
		{
			var body = CreateBodyWithModes();

			body.ComputeGateWidth(OPEN_GATE).Should().Be(29.90f);
		}
	}
}
