using FluentAssertions;
using Fram3d.Core.Cameras;
using Xunit;
namespace Fram3d.Core.Tests.Camera
{
    public sealed class CameraDatabaseParserTests
    {
        [Fact]
        public void LoadBodies__AddsBody__When__ValidCamera()
        {
            var db      = new CameraDatabase();
            var cameras = new[]
            {
                new RawCamera
                {
                    Name           = "TestCam",
                    Manufacturer   = "Acme",
                    Year           = 2024,
                    SensorWidthMm  = 36f,
                    SensorHeightMm = 24f,
                    Format         = "Full Frame",
                    Mount          = "PL"
                }
            };

            CameraDatabaseParser.LoadBodies(db, cameras);

            db.FindBody("TestCam").Should().NotBeNull();
            db.FindBody("TestCam").Manufacturer.Should().Be("Acme");
        }

        [Fact]
        public void LoadBodies__DoesNothing__When__NullArray()
        {
            var db         = new CameraDatabase();
            var bodyCount  = db.Bodies.Count;

            CameraDatabaseParser.LoadBodies(db, null);

            db.Bodies.Count.Should().Be(bodyCount);
        }

        [Fact]
        public void LoadLensSets__CreatesPrimeSet__When__PrimeLenses()
        {
            var db     = new CameraDatabase();
            var lenses = new[]
            {
                new RawLens { Set = "TestPrime", FocalLengthMm = 50f, MaxApertureTstop = 1.4f },
                new RawLens { Set = "TestPrime", FocalLengthMm = 85f, MaxApertureTstop = 1.4f }
            };

            CameraDatabaseParser.LoadLensSets(db, lenses);

            var set = db.FindLensSet("TestPrime");
            set.Should().NotBeNull();
            set.FocalLengths.Should().HaveCount(2);
            set.FocalLengths[0].Should().Be(50f);
            set.FocalLengths[1].Should().Be(85f);
        }

        [Fact]
        public void LoadLensSets__CreatesZoomSet__When__ZoomLens()
        {
            var db     = new CameraDatabase();
            var lenses = new[]
            {
                new RawLens
                {
                    Set          = "TestZoom",
                    Name         = "TestZoom 24-70",
                    Type         = "Zoom",
                    FocalRangeMm = new[] { 24f, 70f }
                }
            };

            CameraDatabaseParser.LoadLensSets(db, lenses);

            var set = db.FindLensSet("TestZoom 24-70");
            set.Should().NotBeNull();
            set.IsZoom.Should().BeTrue();
            set.MinFocalLength.Should().Be(24f);
            set.MaxFocalLength.Should().Be(70f);
        }

        [Fact]
        public void LoadLensSets__SkipsZoom__When__FocalRangeInvalid()
        {
            var db        = new CameraDatabase();
            var setCount  = db.LensSets.Count;
            var lenses    = new[]
            {
                new RawLens { Set = "Bad", Name = "Bad", Type = "Zoom", FocalRangeMm = new[] { 24f } }
            };

            CameraDatabaseParser.LoadLensSets(db, lenses);

            db.LensSets.Count.Should().Be(setCount);
        }

        [Fact]
        public void LoadLensSets__DoesNothing__When__NullArray()
        {
            var db        = new CameraDatabase();
            var setCount  = db.LensSets.Count;

            CameraDatabaseParser.LoadLensSets(db, null);

            db.LensSets.Count.Should().Be(setCount);
        }

        [Fact]
        public void LoadLensSets__OrdersSpecsByFocalLength__When__Unordered()
        {
            var db     = new CameraDatabase();
            var lenses = new[]
            {
                new RawLens { Set = "Ordered", FocalLengthMm = 135f },
                new RawLens { Set = "Ordered", FocalLengthMm = 24f },
                new RawLens { Set = "Ordered", FocalLengthMm = 50f }
            };

            CameraDatabaseParser.LoadLensSets(db, lenses);

            var set = db.FindLensSet("Ordered");
            set.FocalLengths.Should().BeInAscendingOrder();
        }

        [Fact]
        public void ParseSensorModes__ReturnsNull__When__NullInput()
        {
            CameraDatabaseParser.ParseSensorModes(null).Should().BeNull();
        }

        [Fact]
        public void ParseSensorModes__ReturnsNull__When__EmptyArray()
        {
            CameraDatabaseParser.ParseSensorModes(new RawSensorMode[0]).Should().BeNull();
        }

        [Fact]
        public void ParseSensorModes__ParsesResolution__When__ValidMode()
        {
            var raw = new[]
            {
                new RawSensorMode
                {
                    Name       = "4K DCI",
                    Resolution = new[] { 4096, 2160 },
                    MaxFps     = 60
                }
            };

            var modes = CameraDatabaseParser.ParseSensorModes(raw);

            modes.Should().HaveCount(1);
            modes[0].Name.Should().Be("4K DCI");
            modes[0].ResolutionWidth.Should().Be(4096);
            modes[0].ResolutionHeight.Should().Be(2160);
            modes[0].MaxFps.Should().Be(60);
        }

        [Fact]
        public void ParseSensorModes__DefaultsToZero__When__ResolutionMissing()
        {
            var raw = new[] { new RawSensorMode { Name = "NoRes" } };

            var modes = CameraDatabaseParser.ParseSensorModes(raw);

            modes[0].ResolutionWidth.Should().Be(0);
            modes[0].ResolutionHeight.Should().Be(0);
        }

        [Fact]
        public void ParseSensorModes__DefaultsSensorAreaToZero__When__SensorAreaMissing()
        {
            var raw = new[]
            {
                new RawSensorMode
                {
                    Name       = "NoSensor",
                    Resolution = new[] { 1920, 1080 },
                    MaxFps     = 30
                }
            };

            var modes = CameraDatabaseParser.ParseSensorModes(raw);

            modes[0].SensorAreaWidthMm.Should().Be(0f);
            modes[0].SensorAreaHeightMm.Should().Be(0f);
        }

        [Fact]
        public void ParseSensorModes__ParsesSensorArea__When__ValidSensorArea()
        {
            var raw = new[]
            {
                new RawSensorMode
                {
                    Name        = "WithSensor",
                    Resolution  = new[] { 4096, 2160 },
                    SensorAreaMm = new[] { 23.76f, 13.365f },
                    MaxFps      = 60
                }
            };

            var modes = CameraDatabaseParser.ParseSensorModes(raw);

            modes[0].SensorAreaWidthMm.Should().Be(23.76f);
            modes[0].SensorAreaHeightMm.Should().Be(13.365f);
        }

        [Fact]
        public void LoadLensSets__SkipsPrime__When__AllFocalLengthsInvalid()
        {
            var db       = new CameraDatabase();
            var setCount = db.LensSets.Count;
            var lenses   = new[]
            {
                new RawLens { Set = "BadPrime", FocalLengthMm = 0f },
                new RawLens { Set = "BadPrime", FocalLengthMm = -5f }
            };

            CameraDatabaseParser.LoadLensSets(db, lenses);

            db.LensSets.Count.Should().Be(setCount);
        }

        [Fact]
        public void LoadLensSets__SetsZoomAperture__When__ApertureProvided()
        {
            var db     = new CameraDatabase();
            var lenses = new[]
            {
                new RawLens
                {
                    Set             = "TestZoom",
                    Name            = "TestZoom 24-70 T2.8",
                    Type            = "Zoom",
                    FocalRangeMm    = new[] { 24f, 70f },
                    MaxApertureTstop = 2.8f,
                    CloseFocusM     = 0.38f
                }
            };

            CameraDatabaseParser.LoadLensSets(db, lenses);

            var set = db.FindLensSet("TestZoom 24-70 T2.8");
            set.Should().NotBeNull();
            set.MaxAperture.Should().Be(2.8f);
            set.CloseFocusM.Should().Be(0.38f);
        }
    }
}
