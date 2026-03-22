using FluentAssertions;
using Fram3d.Core.Camera;
using Xunit;

namespace Fram3d.Core.Tests.Camera
{
	public class CameraDatabaseTests
	{
		[Fact]
		public void DefaultBody__IsGeneric35mm__When__NewDatabase()
		{
			var db = new CameraDatabase();
			db.DefaultBody.Should().NotBeNull();
			db.DefaultBody.Name.Should().Be("Generic 35mm");
			db.DefaultBody.SensorWidthMm.Should().Be(36.0f);
			db.DefaultBody.SensorHeightMm.Should().Be(24.0f);
		}

		[Fact]
		public void DefaultLensSet__IsGenericPrime__When__NewDatabase()
		{
			var db = new CameraDatabase();
			db.DefaultLensSet.Should().NotBeNull();
			db.DefaultLensSet.Name.Should().Be("Generic Prime");
			db.DefaultLensSet.FocalLengths.Should().HaveCount(16);
		}

		[Fact]
		public void FindBody__ReturnsBody__When__NameExists()
		{
			var db = new CameraDatabase();
			db.AddBody(new CameraBody("ARRI Alexa 35", "ARRI", 0, 27.99f, 19.22f, "S35", "LPL", new[] { 4608, 3164 }, new[] { 24, 25, 30 }));

			var body = db.FindBody("ARRI Alexa 35");

			body.Should().NotBeNull();
			body.SensorHeightMm.Should().Be(19.22f);
		}

		[Fact]
		public void FindBody__ReturnsNull__When__NameNotFound()
		{
			var db = new CameraDatabase();
			db.FindBody("Nonexistent Camera").Should().BeNull();
		}

		[Fact]
		public void FindLensSet__ReturnsLensSet__When__NameExists()
		{
			var db = new CameraDatabase();
			db.AddLensSet(new LensSet("Cooke S4/i", new float[] { 14, 18, 21, 25, 27, 32, 35, 40, 50, 65, 75, 100, 135 }, false, 1.0f));

			var ls = db.FindLensSet("Cooke S4/i");

			ls.Should().NotBeNull();
			ls.FocalLengths.Should().HaveCount(13);
		}

		[Fact]
		public void Bodies__IncludesGenericDefaults__When__NewDatabase()
		{
			var db = new CameraDatabase();
			db.Bodies.Should().Contain(b => b.Name == "Generic 35mm");
			db.Bodies.Should().Contain(b => b.Name == "Generic Super 35");
			db.Bodies.Should().Contain(b => b.Name == "Generic 16mm");
			db.Bodies.Should().Contain(b => b.Name == "Generic Super 16");
			db.Bodies.Should().Contain(b => b.Name == "Generic 8mm");
		}

		[Fact]
		public void Bodies__HaveCorrectFormats__When__NewDatabase()
		{
			var db = new CameraDatabase();
			db.FindBody("Generic 35mm").Format.Should().Be("FF");
			db.FindBody("Generic Super 35").Format.Should().Be("S35");
			db.FindBody("Generic 16mm").Format.Should().Be("16mm");
			db.FindBody("Generic Super 16").Format.Should().Be("S16");
			db.FindBody("Generic 8mm").Format.Should().Be("8mm");
		}

		[Fact]
		public void Bodies__HaveGenericManufacturer__When__NewDatabase()
		{
			var db = new CameraDatabase();

			foreach (var body in db.Bodies)
				body.Manufacturer.Should().Be("Generic");
		}

		[Fact]
		public void DefaultLensSet__IsNotAnamorphic__When__NewDatabase()
		{
			var db = new CameraDatabase();
			db.DefaultLensSet.IsAnamorphic.Should().BeFalse();
		}

		[Fact]
		public void FindLensSet__ReturnsNull__When__NameNotFound()
		{
			var db = new CameraDatabase();
			db.FindLensSet("Nonexistent").Should().BeNull();
		}
	}
}
