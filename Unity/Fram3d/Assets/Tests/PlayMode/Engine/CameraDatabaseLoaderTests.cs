using System.Collections;
using System.Linq;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Fram3d.Tests.Engine
{
    /// <summary>
    /// Verifies the camera-lens-database.json loads and parses correctly.
    /// Catches JSON schema mismatches, missing fields, and parse errors
    /// that only surface at runtime.
    /// </summary>
    public sealed class CameraDatabaseLoaderTests
    {
        [UnityTest]
        public IEnumerator Load__ReturnsDatabase__When__JsonExists()
        {
            yield return null;

            var db = CameraDatabaseLoader.Load();

            Assert.IsNotNull(db);
        }

        [UnityTest]
        public IEnumerator Load__ParsesBodies__When__JsonHasCameras()
        {
            yield return null;

            var db = CameraDatabaseLoader.Load();

            // Should have more than just the 5 generic defaults
            Assert.Greater(db.Bodies.Count, 5);
        }

        [UnityTest]
        public IEnumerator Load__ParsesLensSets__When__JsonHasLenses()
        {
            yield return null;

            var db = CameraDatabaseLoader.Load();

            // Should have more than just the 1 generic default
            Assert.Greater(db.LensSets.Count, 1);
        }

        [UnityTest]
        public IEnumerator Load__ParsesSensorModes__When__CameraHasModes()
        {
            yield return null;

            var db              = CameraDatabaseLoader.Load();
            var withSensorModes = db.Bodies.FirstOrDefault(b => b.HasSensorModes);

            Assert.IsNotNull(withSensorModes, "Expected at least one camera with sensor modes in the database");
            Assert.Greater(withSensorModes.SensorModes.Length, 0);
            Assert.IsNotEmpty(withSensorModes.SensorModes[0].Name);
        }

        [UnityTest]
        public IEnumerator Load__ParsesZoomLenses__When__JsonHasZooms()
        {
            yield return null;

            var db   = CameraDatabaseLoader.Load();
            var zoom = db.LensSets.FirstOrDefault(ls => ls.IsZoom);

            Assert.IsNotNull(zoom, "Expected at least one zoom lens set in the database");
            Assert.Greater(zoom.MaxFocalLength, zoom.MinFocalLength);
        }

        [UnityTest]
        public IEnumerator Load__AllBodiesHaveValidSensorSize__When__Loaded()
        {
            yield return null;

            var db = CameraDatabaseLoader.Load();

            foreach (var body in db.Bodies)
            {
                Assert.Greater(body.SensorWidthMm,  0f, $"{body.Name} has invalid sensor width");
                Assert.Greater(body.SensorHeightMm, 0f, $"{body.Name} has invalid sensor height");
            }
        }
    }
}
