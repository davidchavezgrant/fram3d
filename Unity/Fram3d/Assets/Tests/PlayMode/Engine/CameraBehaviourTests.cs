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

        [UnityTest]
        public IEnumerator Awake__LoadsDatabase__When__Created()
        {
            yield return null;

            Assert.IsNotNull(this._behaviour.Database);
            Assert.Greater(this._behaviour.Database.Bodies.Count,   0);
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
        public IEnumerator Awake__SetsFocalLength__When__Created()
        {
            yield return null;

            Assert.AreEqual(this._behaviour.CameraElement.FocalLength, this._camera.focalLength, 0.01f);
        }

        [UnityTest]
        public IEnumerator Awake__SetsGateFitOverscan__When__Created()
        {
            yield return null;

            Assert.AreEqual(Camera.GateFitMode.Overscan, this._camera.gateFit);
        }

        // --- Initialization ---

        [UnityTest]
        public IEnumerator Awake__SetsPhysicalCamera__When__Created()
        {
            yield return null;

            Assert.IsTrue(this._camera.usePhysicalProperties);
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

        // --- Pass-through methods ---

        [UnityTest]
        public IEnumerator CycleAspectRatioForward__ChangesAspectRatio__When__Called()
        {
            yield return null;

            var before = this._behaviour.ActiveAspectRatio;
            this._behaviour.CycleAspectRatioForward();
            Assert.AreNotSame(before, this._behaviour.ActiveAspectRatio);
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

        // --- Reset integration ---

        [UnityTest]
        public IEnumerator Reset__PreservesSensorSize__When__BodyWasSet()
        {
            yield return null;

            var body = new CameraBody("Large",
                                      "Test",
                                      2020,
                                      54.12f,
                                      25.58f,
                                      "LF",
                                      "",
                                      new[] { 8192, 3840 },
                                      new[] { 24 });

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

        [UnityTest]
        public IEnumerator SetSensorMode__UpdatesActiveMode__When__Called()
        {
            yield return null;

            var mode = new SensorMode("Test",
                                      4096,
                                      2160,
                                      24.89f,
                                      13.12f,
                                      60);

            this._behaviour.SetSensorMode(mode);
            Assert.AreSame(mode, this._behaviour.ActiveSensorMode);
        }

        [SetUp]
        public void SetUp()
        {
            this._go        = new GameObject("TestCamera");
            this._behaviour = this._go.AddComponent<CameraBehaviour>();
            this._camera    = this._go.GetComponent<Camera>();
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

        // --- Compound workflow ---

        [UnityTest]
        public IEnumerator Sync__AllPropertiesConsistent__When__BodyModeAndRatioChanged()
        {
            yield return null;

            var cam = this._behaviour.CameraElement;

            var openGate = new SensorMode("Open Gate",
                                          5120,
                                          2700,
                                          29.90f,
                                          15.77f,
                                          60);

            var body = new CameraBody("RED DSMC2",
                                      "RED",
                                      2016,
                                      29.90f,
                                      15.77f,
                                      "S35",
                                      "RF",
                                      new[] { 5120, 2700 },
                                      new[] { 24 },
                                      new[] { openGate });

            // Change all three inputs
            cam.SetBody(body);
            cam.SetSensorMode(openGate);

            while (cam.ActiveAspectRatio != AspectRatio.RATIO_239_1)
                this._behaviour.CycleAspectRatioForward();

            yield return null;

            // Unity Camera should reflect all changes
            var sensor = this._camera.sensorSize;
            Assert.AreEqual(cam.SensorWidth,  sensor.x, 0.01f);
            Assert.AreEqual(cam.SensorHeight, sensor.y, 0.01f);

            // Viewport rect should be constrained (2.39:1 on ~1.9:1 gate → letterbox)
            var rect = this._camera.rect;
            Assert.Less(rect.height, 1f);
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

        // --- Tilt rotation conversion ---

        [UnityTest]
        public IEnumerator Sync__ConvertsRotation__When__CameraTilted()
        {
            yield return null;

            this._behaviour.CameraElement.Tilt(0.5f);
            yield return null;

            // Tilt rotates around the local X axis. In the conversion, X is negated.
            var rot = this._go.transform.rotation;
            Assert.AreNotEqual(Quaternion.identity, rot);

            // X component should be non-zero (tilt axis)
            Assert.AreNotEqual(0f, rot.x);
        }

        // --- Full Screen viewport ---

        [UnityTest]
        public IEnumerator Sync__WiderViewport__When__FullScreenVsNamedRatio()
        {
            yield return null;

            var cam = this._behaviour.CameraElement;

            // Set 4:3 first — produces a pillarbox
            while (cam.ActiveAspectRatio != AspectRatio.RATIO_4_3)
                this._behaviour.CycleAspectRatioForward();

            yield return null;

            var rectNarrow = this._camera.rect;

            // Switch to Full Screen — should show more of the sensor
            while (cam.ActiveAspectRatio != AspectRatio.FULL_SCREEN)
                this._behaviour.CycleAspectRatioBackward();

            yield return null;

            var rectFull = this._camera.rect;

            // Full Screen should use at least as much viewport width as 4:3
            Assert.GreaterOrEqual(rectFull.width, rectNarrow.width);
        }

        // --- Shake disabled → exact rotation match ---

        [UnityTest]
        public IEnumerator Sync__RotationMatchesCore__When__ShakeDisabled()
        {
            yield return null;

            var cam = this._behaviour.CameraElement;
            cam.Pan(0.4f);
            cam.Tilt(0.2f);
            cam.ShakeEnabled = false;
            yield return null;

            // With shake off, Unity rotation should exactly match converted Core rotation
            var coreRot  = cam.Rotation;
            var unityRot = this._go.transform.rotation;

            // Conversion: X negated, Y negated, Z preserved, W preserved
            Assert.AreEqual(-coreRot.X, unityRot.x, 0.001f);
            Assert.AreEqual(-coreRot.Y, unityRot.y, 0.001f);
            Assert.AreEqual(coreRot.Z,  unityRot.z, 0.001f);
            Assert.AreEqual(coreRot.W,  unityRot.w, 0.001f);
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

        // --- Lens set switch → focal length ---

        [UnityTest]
        public IEnumerator Sync__UpdatesFocalLength__When__LensSetSwitched()
        {
            yield return null;

            var cam = this._behaviour.CameraElement;
            cam.SetFocalLengthPreset(200f);
            yield return null;

            // Switch to a zoom lens that maxes out at 70mm — should clamp
            cam.SetLensSet(new LensSet("Canon 24-70mm",
                                       24f,
                                       70f,
                                       false,
                                       1.0f));

            yield return null;

            Assert.AreEqual(70f, this._camera.focalLength, 0.01f);
        }

        // --- Sync: sensor size ---

        [UnityTest]
        public IEnumerator Sync__UpdatesSensorSize__When__BodyChanged()
        {
            yield return null;

            var largeSensor = new CameraBody("Large Format",
                                             "Test",
                                             2020,
                                             54.12f,
                                             25.58f,
                                             "LF",
                                             "",
                                             new[] { 8192, 3840 },
                                             new[] { 24 });

            this._behaviour.CameraElement.SetBody(largeSensor);
            yield return null;

            var sensor = this._camera.sensorSize;
            Assert.AreEqual(this._behaviour.CameraElement.SensorWidth,  sensor.x, 0.01f);
            Assert.AreEqual(this._behaviour.CameraElement.SensorHeight, sensor.y, 0.01f);
        }

        // --- Sensor mode → Unity Camera sync ---

        [UnityTest]
        public IEnumerator Sync__UpdatesSensorSize__When__SensorModeChanged()
        {
            yield return null;

            var cam = this._behaviour.CameraElement;

            // Use Full Screen so mode change is visible — with 16:9 delivery,
            // both open gate and 16:9 crop produce identical effective heights
            while (cam.ActiveAspectRatio != AspectRatio.FULL_SCREEN)
                this._behaviour.CycleAspectRatioBackward();

            var openGate = new SensorMode("Open Gate",
                                          4448,
                                          3096,
                                          36.70f,
                                          25.54f,
                                          40);

            var crop = new SensorMode("16:9",
                                      4448,
                                      2502,
                                      36.70f,
                                      20.64f,
                                      60);

            var body = new CameraBody("Test LF",
                                      "Test",
                                      2020,
                                      36.70f,
                                      25.54f,
                                      "LF",
                                      "",
                                      new[] { 4448, 3096 },
                                      new[] { 24 },
                                      new[] { openGate, crop });

            cam.SetBody(body);
            cam.SetSensorMode(openGate);
            yield return null;

            var sensorBefore = this._camera.sensorSize;
            cam.SetSensorMode(crop);
            yield return null;

            var sensorAfter = this._camera.sensorSize;

            // On Full Screen, open gate (≈1.44:1) and crop (≈1.78:1) produce different heights
            Assert.That(Mathf.Abs(sensorBefore.y - sensorAfter.y), Is.GreaterThan(0.1f));

            // And should match Core's effective dims
            Assert.AreEqual(cam.SensorWidth,  sensorAfter.x, 0.01f);
            Assert.AreEqual(cam.SensorHeight, sensorAfter.y, 0.01f);
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

        [UnityTest]
        public IEnumerator Sync__UpdatesViewportRect__When__SensorModeChanged()
        {
            yield return null;

            var cam = this._behaviour.CameraElement;

            // Use Full Screen so viewport rect is driven by sensor mode, not delivery ratio
            while (cam.ActiveAspectRatio != AspectRatio.FULL_SCREEN)
                this._behaviour.CycleAspectRatioBackward();

            var openGate = new SensorMode("Open Gate",
                                          4448,
                                          3096,
                                          36.70f,
                                          25.54f,
                                          40);

            var body = new CameraBody("Test LF",
                                      "Test",
                                      2020,
                                      36.70f,
                                      25.54f,
                                      "LF",
                                      "",
                                      new[] { 4448, 3096 },
                                      new[] { 24 },
                                      new[] { openGate });

            cam.SetBody(body);
            cam.SetSensorMode(openGate);
            yield return null;

            // Open gate is ~1.44:1, most screens are 16:9 → pillarbox
            var rect = this._camera.rect;
            Assert.Less(rect.width, 1f);
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

            this._behaviour.CameraElement.DofEnabled    = true;
            this._behaviour.CameraElement.FocusDistance = 3.5f;
            yield return null;

            var dof = GetDof();
            Assert.AreEqual(3.5f, dof.focusDistance.value, 0.01f);
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

        // --- FRA-38: Focal length lerp convergence ---

        [UnityTest]
        public IEnumerator SyncFocalLength__ConvergesToTarget__When__LerpingOverFrames()
        {
            yield return null;

            // Need a zoom lens — prime lens blocks continuous FocalLength writes
            var cam = this._behaviour.CameraElement;

            cam.SetLensSet(new LensSet("Test Zoom",
                                       24f,
                                       200f,
                                       false,
                                       1.0f));

            cam.SnapFocalLength = false;
            cam.FocalLength     = 135f;

            // Wait until convergence or timeout — lerp rate depends on framerate
            for (var i = 0; i < 600; i++)
            {
                yield return null;

                if (Mathf.Abs(this._camera.focalLength - 135f) < 0.01f)
                {
                    Assert.AreEqual(135f, this._camera.focalLength, 0.01f);
                    yield break;
                }
            }

            Assert.Fail($"Focal length did not converge after 600 frames (stuck at {this._camera.focalLength:F2}mm)");
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(this._go);
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