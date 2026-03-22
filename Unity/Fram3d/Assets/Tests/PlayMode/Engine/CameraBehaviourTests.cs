using System.Collections;
using Fram3d.Core.Camera;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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

        // --- FRA-37: Dolly zoom snap (prevents jitter from lerp/position desync) ---

        [UnityTest]
        public IEnumerator DollyZoom__FocalLengthMatchesTarget__When__Applied()
        {
            yield return null;

            var cam = this._behaviour.CameraElement;
            cam.FocalLength = 50f;
            cam.DollyZoom(2.0f);
            yield return null;

            // DollyZoom sets SnapFocalLength — focal length should match Core value
            // exactly after one frame, not be mid-lerp (the jitter bug from FRA-37)
            Assert.AreEqual(cam.FocalLength, this._camera.focalLength, 0.01f);
        }

        [UnityTest]
        public IEnumerator DollyZoom__SnapFlagConsumed__When__SyncRuns()
        {
            yield return null;

            this._behaviour.CameraElement.DollyZoom(1.0f);
            yield return null;

            // Snap flag should be cleared after Sync consumes it
            Assert.IsFalse(this._behaviour.CameraElement.SnapFocalLength);
        }

        // --- FRA-38: Focal length lerp convergence ---

        [UnityTest]
        public IEnumerator SyncFocalLength__ConvergesToTarget__When__LerpingOverFrames()
        {
            yield return null;

            // Change focal length without snap — should lerp
            this._behaviour.CameraElement.FocalLength = 135f;
            // Wait several frames for lerp to converge
            for (var i = 0; i < 60; i++)
                yield return null;

            // Should have converged within 0.01mm (the snap threshold in SyncFocalLength)
            Assert.AreEqual(135f, this._camera.focalLength, 0.01f);
        }

        // --- DOF wiring ---

        [UnityTest]
        public IEnumerator SyncDof__SetsBokehMode__When__DofEnabled()
        {
            yield return null;

            this._behaviour.CameraElement.DofEnabled = true;
            yield return null;

            var dof = GetDof();
            Assert.IsNotNull(dof, "DepthOfField override not found on camera Volume");
            Assert.AreEqual(DepthOfFieldMode.Bokeh, dof.mode.value);
        }

        [UnityTest]
        public IEnumerator SyncDof__SetsOffMode__When__DofDisabled()
        {
            yield return null;

            this._behaviour.CameraElement.DofEnabled = false;
            yield return null;

            var dof = GetDof();
            Assert.IsNotNull(dof);
            Assert.AreEqual(DepthOfFieldMode.Off, dof.mode.value);
        }

        [UnityTest]
        public IEnumerator SyncDof__PropagatesAperture__When__ApertureChanged()
        {
            yield return null;

            this._behaviour.CameraElement.DofEnabled = true;
            this._behaviour.CameraElement.StepApertureWider(); // f/5.6 → f/4
            yield return null;

            var dof = GetDof();
            Assert.AreEqual(this._behaviour.CameraElement.Aperture, dof.aperture.value, 0.01f);
        }

        [UnityTest]
        public IEnumerator SyncDof__PropagatesFocusDistance__When__FocusChanged()
        {
            yield return null;

            this._behaviour.CameraElement.DofEnabled = true;
            this._behaviour.CameraElement.FocusDistance = 3.5f;
            yield return null;

            var dof = GetDof();
            Assert.AreEqual(3.5f, dof.focusDistance.value, 0.01f);
        }

        // --- Shake: no drift (prior-codebase-lessons anti-pattern) ---

        [UnityTest]
        public IEnumerator Shake__DoesNotDriftBaseRotation__When__EnabledOverManyFrames()
        {
            yield return null;

            var cam = this._behaviour.CameraElement;
            cam.Pan(0.3f); // set a known rotation
            var baseRotation = cam.Rotation;

            cam.ShakeEnabled   = true;
            cam.ShakeAmplitude = 0.5f;
            cam.ShakeFrequency = 2.0f;

            // Run for many frames with shake
            for (var i = 0; i < 120; i++)
                yield return null;

            // Core rotation should be unchanged — shake is applied in Engine only
            Assert.AreEqual(baseRotation.X, cam.Rotation.X, 0.0001f);
            Assert.AreEqual(baseRotation.Y, cam.Rotation.Y, 0.0001f);
            Assert.AreEqual(baseRotation.Z, cam.Rotation.Z, 0.0001f);
            Assert.AreEqual(baseRotation.W, cam.Rotation.W, 0.0001f);
        }

        // --- Reset integration ---

        [UnityTest]
        public IEnumerator Reset__PreservesSensorSize__When__BodyWasSet()
        {
            yield return null;

            var body = new CameraBody("Large", "Test", 2020,
                54.12f, 25.58f, "LF", "", new[] { 8192, 3840 }, new[] { 24 });
            this._behaviour.CameraElement.SetBody(body);
            yield return null;

            this._behaviour.CameraElement.Reset();
            yield return null;

            // Reset reframes, doesn't change equipment — sensor should match body
            var sensor = this._camera.sensorSize;
            Assert.AreEqual(this._behaviour.CameraElement.SensorWidth,  sensor.x, 0.01f);
            Assert.AreEqual(this._behaviour.CameraElement.SensorHeight, sensor.y, 0.01f);
        }

        [UnityTest]
        public IEnumerator Reset__RestoresDefaultFocalLength__When__FocalLengthChanged()
        {
            yield return null;

            this._behaviour.CameraElement.SetFocalLengthPreset(200f);
            yield return null;

            this._behaviour.CameraElement.Reset();
            yield return null;

            Assert.AreEqual(50f, this._camera.focalLength, 0.01f);
        }

        // --- Rotation coordinate conversion ---

        [UnityTest]
        public IEnumerator Sync__ConvertsRotation__When__CameraPanned()
        {
            yield return null;

            this._behaviour.CameraElement.Pan(0.5f);
            yield return null;

            // After pan, Unity transform rotation should differ from identity
            var rot = this._go.transform.rotation;
            Assert.AreNotEqual(Quaternion.identity, rot);

            // The Y component should be non-zero (pan rotates around Y)
            // In the conversion, X and Y are negated, Z is preserved, W is preserved
            Assert.AreNotEqual(0f, rot.y);
        }

        // --- Helpers ---

        private DepthOfField GetDof()
        {
            var volume = this._go.GetComponent<Volume>();

            if (volume == null || volume.profile == null)
                return null;

            volume.profile.TryGet(out DepthOfField dof);
            return dof;
        }
    }
}
