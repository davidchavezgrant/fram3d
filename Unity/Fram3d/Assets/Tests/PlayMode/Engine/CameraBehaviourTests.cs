using System.Collections;
using Fram3d.Core.Camera;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Fram3d.Tests.Engine
{
    /// <summary>
    /// Play Mode integration tests for CameraBehaviour. Verifies the wiring
    /// between Core domain state and Unity's Camera — does NOT re-test
    /// domain logic (covered by xUnit tests in Fram3d.Core.Tests).
    /// </summary>
    public sealed class CameraBehaviourTests
    {
        private CameraBehaviour _behaviour;
        private Camera          _camera;
        private GameObject      _go;

        [SetUp]
        public void SetUp()
        {
            this._go        = new GameObject("TestCamera");
            this._behaviour = this._go.AddComponent<CameraBehaviour>();
            this._camera    = this._go.GetComponent<Camera>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(this._go);
        }

        // --- Initialization ---

        [UnityTest]
        public IEnumerator Awake__SetsPhysicalCamera__When__Created()
        {
            yield return null;

            Assert.IsTrue(this._camera.usePhysicalProperties);
        }

        [UnityTest]
        public IEnumerator Awake__SetsGateFitOverscan__When__Created()
        {
            yield return null;

            Assert.AreEqual(Camera.GateFitMode.Overscan, this._camera.gateFit);
        }

        [UnityTest]
        public IEnumerator Awake__LoadsDatabase__When__Created()
        {
            yield return null;

            Assert.IsNotNull(this._behaviour.Database);
            Assert.Greater(this._behaviour.Database.Bodies.Count, 0);
            Assert.Greater(this._behaviour.Database.LensSets.Count, 0);
        }

        [UnityTest]
        public IEnumerator Awake__SetsDefaultBody__When__Created()
        {
            yield return null;

            Assert.IsNotNull(this._behaviour.CameraElement.Body);
        }

        [UnityTest]
        public IEnumerator Awake__SetsDefaultLensSet__When__Created()
        {
            yield return null;

            Assert.IsNotNull(this._behaviour.CameraElement.ActiveLensSet);
        }

        [UnityTest]
        public IEnumerator Awake__SetsSensorSize__When__Created()
        {
            yield return null;

            var cam    = this._behaviour.CameraElement;
            var sensor = this._camera.sensorSize;
            Assert.AreEqual(cam.SensorWidth,  sensor.x, 0.01f);
            Assert.AreEqual(cam.SensorHeight, sensor.y, 0.01f);
        }

        [UnityTest]
        public IEnumerator Awake__SetsFocalLength__When__Created()
        {
            yield return null;

            Assert.AreEqual(this._behaviour.CameraElement.FocalLength, this._camera.focalLength, 0.01f);
        }

        // --- Sync: sensor size ---

        [UnityTest]
        public IEnumerator Sync__UpdatesSensorSize__When__BodyChanged()
        {
            yield return null;

            var largeSensor = new CameraBody("Large Format", "Test", 2020,
                54.12f, 25.58f, "LF", "", new[] { 8192, 3840 }, new[] { 24 });
            this._behaviour.CameraElement.SetBody(largeSensor);
            yield return null;

            var sensor = this._camera.sensorSize;
            Assert.AreEqual(this._behaviour.CameraElement.SensorWidth,  sensor.x, 0.01f);
            Assert.AreEqual(this._behaviour.CameraElement.SensorHeight, sensor.y, 0.01f);
        }

        // --- Sync: focal length ---

        [UnityTest]
        public IEnumerator Sync__SnapsFocalLength__When__SnapFlagIsTrue()
        {
            yield return null;

            this._behaviour.CameraElement.SetFocalLengthPreset(85f);
            yield return null;

            Assert.AreEqual(85f, this._camera.focalLength, 0.01f);
        }

        // --- Sync: viewport rect ---

        [UnityTest]
        public IEnumerator Sync__UpdatesViewportRect__When__AspectRatioCycled()
        {
            yield return null;

            // Cycle until we hit a ratio that differs from screen aspect
            // 4:3 on a wider screen should produce a pillarbox (width < 1)
            var cam = this._behaviour.CameraElement;
            while (cam.ActiveAspectRatio != AspectRatio.RATIO_4_3)
                this._behaviour.CycleAspectRatioForward();

            yield return null;

            var rect = this._camera.rect;
            // 4:3 is narrower than most screens → pillarbox: x > 0, width < 1
            Assert.Less(rect.width, 1f);
            Assert.Greater(rect.x, 0f);
        }

        // --- Sync: position and rotation ---

        [UnityTest]
        public IEnumerator Sync__UpdatesTransformPosition__When__CameraMoved()
        {
            yield return null;

            this._behaviour.CameraElement.Dolly(3.0f);
            yield return null;

            // System.Numerics is right-handed (-Z forward), Unity is left-handed (+Z forward).
            // ToUnity() negates Z, so compare against the converted position.
            var expected = this._behaviour.CameraElement.Position;
            var actual   = this._go.transform.position;
            Assert.AreEqual(expected.X,  actual.x, 0.01f);
            Assert.AreEqual(expected.Y,  actual.y, 0.01f);
            Assert.AreEqual(-expected.Z, actual.z, 0.01f);
        }

        // --- Pass-through methods ---

        [UnityTest]
        public IEnumerator CycleAspectRatioForward__ChangesAspectRatio__When__Called()
        {
            yield return null;

            var before = this._behaviour.ActiveAspectRatio;
            this._behaviour.CycleAspectRatioForward();

            Assert.AreNotSame(before, this._behaviour.ActiveAspectRatio);
        }

        [UnityTest]
        public IEnumerator SetSensorMode__UpdatesActiveMode__When__Called()
        {
            yield return null;

            var mode = new SensorMode("Test", 4096, 2160, 24.89f, 13.12f, 60);
            this._behaviour.SetSensorMode(mode);

            Assert.AreSame(mode, this._behaviour.ActiveSensorMode);
        }
    }
}
