using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Fram3d.Core.Common;
using Fram3d.Core.Scenes;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Fram3d.Tests.Engine
{
    /// <summary>
    /// Play Mode tests for ViewCameraManager. Verifies multi-view lifecycle:
    /// Director camera creation/destruction, viewport rect computation, camera
    /// routing by slot, and slot activation by screen position.
    ///
    /// Each test creates a CameraBehaviour (which creates its own Camera and
    /// FrustumWireframe in Awake) and a ViewCameraManager wired to it.
    /// </summary>
    public sealed class ViewCameraManagerTests
    {
        private CameraBehaviour  _cameraBehaviour;
        private GameObject       _cameraGo;
        private List<GameObject> _extras;
        private ViewCameraManager _manager;

        // ── Awake ────────────────────────────────────────────────────────

        [Test]
        public void Awake__CreatesViewSlotModel__When__Created()
        {
            Assert.IsNotNull(this._manager.ViewSlotModel);
        }

        [Test]
        public void Awake__DefaultsToSingleView__When__Created()
        {
            Assert.IsFalse(this._manager.IsMultiView);
            Assert.AreEqual(ViewLayout.SINGLE, this._manager.ViewSlotModel.Layout);
        }

        // ── Single view ──────────────────────────────────────────────────

        [Test]
        public void ActiveCameraElement__ReturnsShotCamera__When__SingleView()
        {
            Assert.AreSame(this._cameraBehaviour.ActiveCamera, this._manager.ActiveCameraElement);
        }

        [UnityTest]
        public IEnumerator GetUnityCamera__ReturnsMainCamera__When__SingleView()
        {
            yield return null;

            var mainCam = this._cameraBehaviour.GetComponent<Camera>();
            var result  = this._manager.GetUnityCamera(0);
            Assert.AreSame(mainCam, result);
        }

        [UnityTest]
        public IEnumerator GetUnityCameraAtPosition__ReturnsMainCamera__When__SingleView()
        {
            yield return null;

            var mainCam = this._cameraBehaviour.GetComponent<Camera>();
            var center  = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var result  = this._manager.GetUnityCameraAtPosition(center);
            Assert.AreSame(mainCam, result);
        }

        [Test]
        public void CameraViewRect__ReturnsFullRect__When__SingleView()
        {
            var rect = this._manager.CameraViewRect;
            Assert.AreEqual(0f, rect.x, 0.001f);
            Assert.AreEqual(0f, rect.y, 0.001f);
            Assert.AreEqual(1f, rect.width, 0.001f);
            Assert.AreEqual(1f, rect.height, 0.001f);
        }

        // ── Entering multi-view ──────────────────────────────────────────

        [UnityTest]
        public IEnumerator SetLayout__CreatesDirectorCamera__When__SwitchingToHorizontal()
        {
            yield return null;

            this._manager.ViewSlotModel.SetLayout(ViewLayout.HORIZONTAL);
            yield return null;

            Assert.IsTrue(this._manager.IsMultiView);

            // Slot 1 is Director — it should have its own Unity Camera
            var directorCam = this._manager.GetUnityCamera(1);
            Assert.IsNotNull(directorCam, "Director slot should have a camera");
            Assert.AreNotSame(this._cameraBehaviour.GetComponent<Camera>(), directorCam,
                "Director camera should be separate from main camera");

            this._extras.Add(directorCam.gameObject);
        }

        [UnityTest]
        public IEnumerator SetLayout__MainCameraServesSlot0__When__Horizontal()
        {
            yield return null;

            this._manager.ViewSlotModel.SetLayout(ViewLayout.HORIZONTAL);
            yield return null;

            var mainCam = this._cameraBehaviour.GetComponent<Camera>();
            var slot0Cam = this._manager.GetUnityCamera(0);
            Assert.AreSame(mainCam, slot0Cam, "Slot 0 (Camera View) should use the main camera");

            // Cleanup director cameras
            var slot1Cam = this._manager.GetUnityCamera(1);

            if (slot1Cam != null)
            {
                this._extras.Add(slot1Cam.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator SetLayout__ForcesOutOfDirectorView__When__CameraBehaviourInDirectorView()
        {
            yield return null;

            this._cameraBehaviour.ToggleDirectorView();
            Assert.IsTrue(this._cameraBehaviour.IsDirectorView, "Precondition");

            this._manager.ViewSlotModel.SetLayout(ViewLayout.HORIZONTAL);
            yield return null;

            Assert.IsFalse(this._cameraBehaviour.IsDirectorView,
                "Entering multi-view should force CameraBehaviour out of Director View");

            var slot1Cam = this._manager.GetUnityCamera(1);

            if (slot1Cam != null)
            {
                this._extras.Add(slot1Cam.gameObject);
            }
        }

        // ── Returning to single view ─────────────────────────────────────

        [UnityTest]
        public IEnumerator SetLayout__DestroysDirectorCameras__When__ReturningToSingle()
        {
            yield return null;

            this._manager.ViewSlotModel.SetLayout(ViewLayout.HORIZONTAL);
            yield return null;

            var directorCam = this._manager.GetUnityCamera(1);
            Assert.IsNotNull(directorCam, "Precondition: director camera exists");

            this._manager.ViewSlotModel.SetLayout(ViewLayout.SINGLE);
            yield return null;
            yield return null; // Object.Destroy is deferred

            Assert.IsFalse(this._manager.IsMultiView);
            Assert.IsNull(this._manager.GetUnityCamera(1),
                "Director camera should be cleaned up after returning to single view");
        }

        [UnityTest]
        public IEnumerator SetLayout__RestoresFullViewport__When__ReturningToSingle()
        {
            yield return null;

            this._manager.ViewSlotModel.SetLayout(ViewLayout.HORIZONTAL);
            yield return null;

            this._manager.ViewSlotModel.SetLayout(ViewLayout.SINGLE);
            yield return null;

            var mainCam = this._cameraBehaviour.GetComponent<Camera>();
            Assert.AreEqual(1f, mainCam.rect.width, 0.01f,
                "Main camera viewport should be full width after returning to single");
        }

        // ── Viewport rects ───────────────────────────────────────────────

        [UnityTest]
        public IEnumerator GetViewportRect__ReturnsSplitRects__When__HorizontalLayout()
        {
            yield return null;

            this._manager.ViewSlotModel.SetLayout(ViewLayout.HORIZONTAL);
            yield return null;
            yield return null; // need LateUpdate to compute rects

            var rect0 = this._manager.GetViewportRect(0);
            var rect1 = this._manager.GetViewportRect(1);

            // Horizontal: side by side, each roughly half width
            Assert.AreEqual(0f, rect0.x, 0.01f, "Slot 0 starts at left edge");
            Assert.Greater(rect0.width, 0.3f, "Slot 0 should have meaningful width");
            Assert.Greater(rect1.x, 0.3f, "Slot 1 should start after slot 0");
            Assert.AreEqual(rect0.width, rect1.width, 0.01f, "Both slots should have equal width");

            var slot1Cam = this._manager.GetUnityCamera(1);

            if (slot1Cam != null)
            {
                this._extras.Add(slot1Cam.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator GetViewportRect__ReturnsSplitRects__When__VerticalLayout()
        {
            yield return null;

            this._manager.ViewSlotModel.SetLayout(ViewLayout.VERTICAL);
            yield return null;
            yield return null;

            var rect0 = this._manager.GetViewportRect(0);
            var rect1 = this._manager.GetViewportRect(1);

            // Vertical: top and bottom, each half height
            Assert.AreEqual(0.5f, rect0.height, 0.01f, "Slot 0 should be half height");
            Assert.AreEqual(0.5f, rect1.height, 0.01f, "Slot 1 should be half height");
            Assert.Greater(rect0.y, rect1.y, "Slot 0 (top) should have higher y than slot 1 (bottom)");

            var slot1Cam = this._manager.GetUnityCamera(1);

            if (slot1Cam != null)
            {
                this._extras.Add(slot1Cam.gameObject);
            }
        }

        // ── ActiveCameraElement routing ──────────────────────────────────

        [UnityTest]
        public IEnumerator ActiveCameraElement__ReturnsShotCamera__When__ActiveSlotIsCamera()
        {
            yield return null;

            this._manager.ViewSlotModel.SetLayout(ViewLayout.HORIZONTAL);
            yield return null;

            // Default active slot is 0 which is Camera View
            Assert.AreSame(this._cameraBehaviour.ShotCamera, this._manager.ActiveCameraElement);

            var slot1Cam = this._manager.GetUnityCamera(1);

            if (slot1Cam != null)
            {
                this._extras.Add(slot1Cam.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator ActiveCameraElement__ReturnsDirectorCamera__When__ActiveSlotIsDirector()
        {
            yield return null;

            this._manager.ViewSlotModel.SetLayout(ViewLayout.HORIZONTAL);
            yield return null;

            // Force active slot to 1 (Director) via reflection
            SetField(this._manager, "_activeSlot", 1);

            Assert.AreSame(this._cameraBehaviour.DirectorCamera, this._manager.ActiveCameraElement);

            var slot1Cam = this._manager.GetUnityCamera(1);

            if (slot1Cam != null)
            {
                this._extras.Add(slot1Cam.gameObject);
            }
        }

        // ── ActivateSlotAtPosition ───────────────────────────────────────

        [Test]
        public void ActivateSlotAtPosition__DoesNothing__When__SingleView()
        {
            Assert.AreEqual(0, this._manager.ActiveSlot);
            this._manager.ActivateSlotAtPosition(new Vector2(Screen.width - 10f, Screen.height / 2f));
            Assert.AreEqual(0, this._manager.ActiveSlot, "ActiveSlot should not change in single view");
        }

        // ── GetUnityCamera bounds ────────────────────────────────────────

        [Test]
        public void GetUnityCamera__ReturnsNull__When__NegativeIndex()
        {
            Assert.IsNull(this._manager.GetUnityCamera(-1));
        }

        [Test]
        public void GetUnityCamera__ReturnsNull__When__IndexExceedsSlotCount()
        {
            Assert.IsNull(this._manager.GetUnityCamera(5));
        }

        // ── GetViewportRect bounds ───────────────────────────────────────

        [Test]
        public void GetViewportRect__ReturnsFull__When__IndexOutOfRange()
        {
            var rect = this._manager.GetViewportRect(99);
            Assert.AreEqual(1f, rect.width, 0.001f);
            Assert.AreEqual(1f, rect.height, 0.001f);
        }

        // ── Stale slot state bug (FRA-52) ────────────────────────────────

        [UnityTest]
        public IEnumerator SetLayout__ClearsStaleState__When__DirectorSingleToSplit()
        {
            // Regression test: if single-view was in Director View (via slot type
            // change) and then expanded to split, the old Director state must not
            // persist. The split layout should force Camera+Director slots fresh.
            yield return null;

            this._manager.ViewSlotModel.SetSlotType(0, ViewMode.DIRECTOR);
            Assert.IsTrue(this._cameraBehaviour.IsDirectorView,
                "Precondition: single-view Director");

            this._manager.ViewSlotModel.SetLayout(ViewLayout.HORIZONTAL);
            yield return null;

            Assert.IsFalse(this._cameraBehaviour.IsDirectorView,
                "CameraBehaviour should be in Camera View in multi-view");
            Assert.AreEqual(ViewMode.CAMERA, this._manager.ViewSlotModel.GetSlotType(0));
            Assert.AreEqual(ViewMode.DIRECTOR, this._manager.ViewSlotModel.GetSlotType(1));

            var slot1Cam = this._manager.GetUnityCamera(1);

            if (slot1Cam != null)
            {
                this._extras.Add(slot1Cam.gameObject);
            }
        }

        // ── Frustum visibility ───────────────────────────────────────────

        [UnityTest]
        public IEnumerator SetLayout__ShowsFrustum__When__EnteringMultiView()
        {
            yield return null;

            this._manager.ViewSlotModel.SetLayout(ViewLayout.HORIZONTAL);
            yield return null;

            var frustum = this._cameraBehaviour.FrustumWireframe;
            Assert.IsNotNull(frustum, "FrustumWireframe should exist");
            Assert.IsTrue(frustum.gameObject.activeSelf,
                "Frustum should be visible in multi-view with Director slot");

            var slot1Cam = this._manager.GetUnityCamera(1);

            if (slot1Cam != null)
            {
                this._extras.Add(slot1Cam.gameObject);
            }
        }

        // ── Layout switching preserves camera count ──────────────────────

        [UnityTest]
        public IEnumerator SetLayout__SwitchesBetweenLayouts__When__HorizontalToVertical()
        {
            yield return null;

            this._manager.ViewSlotModel.SetLayout(ViewLayout.HORIZONTAL);
            yield return null;

            this._manager.ViewSlotModel.SetLayout(ViewLayout.VERTICAL);
            yield return null;

            Assert.IsTrue(this._manager.IsMultiView);
            Assert.IsNotNull(this._manager.GetUnityCamera(0), "Slot 0 camera should exist");
            Assert.IsNotNull(this._manager.GetUnityCamera(1), "Slot 1 camera should exist");

            var slot1Cam = this._manager.GetUnityCamera(1);

            if (slot1Cam != null)
            {
                this._extras.Add(slot1Cam.gameObject);
            }
        }

        // ── Setup / TearDown ─────────────────────────────────────────────

        [SetUp]
        public void SetUp()
        {
            this._extras   = new List<GameObject>();
            this._cameraGo = new GameObject("TestCamera");
            this._cameraBehaviour = this._cameraGo.AddComponent<CameraBehaviour>();
            this._manager         = this._cameraGo.AddComponent<ViewCameraManager>();
            SetField(this._manager, "cameraBehaviour", this._cameraBehaviour);
        }

        [TearDown]
        public void TearDown()
        {
            // Return to single view to trigger cleanup of director cameras
            if (this._manager != null && this._manager.ViewSlotModel != null)
            {
                this._manager.ViewSlotModel.SetLayout(ViewLayout.SINGLE);
            }

            foreach (var go in this._extras)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }

            var frustum = GameObject.Find("Shot Camera Frustum");

            if (frustum != null)
            {
                Object.DestroyImmediate(frustum);
            }

            Object.DestroyImmediate(this._cameraGo);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }
    }
}
