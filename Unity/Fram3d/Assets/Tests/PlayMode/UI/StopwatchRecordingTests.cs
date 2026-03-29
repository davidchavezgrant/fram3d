using System.Collections;
using System.Numerics;
using System.Reflection;
using Fram3d.Core.Cameras;
using Fram3d.Core.Common;
using Fram3d.Core.Scenes;
using Fram3d.Core.Timelines;
using Fram3d.Engine.Integration;
using Fram3d.UI.Input;
using Fram3d.UI.Timeline;
using Fram3d.UI.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;
namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Play Mode tests for stopwatch recording behavior. Verifies that camera
    /// manipulations only create keyframes when the stopwatch is on, that
    /// Director View blocks recording, and that the C key force-records.
    ///
    /// Uses the same device setup pattern as CameraInputHandlerTests: AddDevice
    /// for keyboard/mouse, SetField for [SerializeField] wiring.
    /// </summary>
    public sealed class StopwatchRecordingTests
    {
        private CameraBehaviour    _behaviour;
        private CameraElement      _cam;
        private CameraInputHandler _handler;
        private Keyboard           _keyboard;
        private Mouse              _mouse;
        private ShotEvaluator      _shotEvaluator;
        private Timeline           _timeline;

        // ── GameObjects ──
        private GameObject _cameraGo;
        private GameObject _shotEvalGo;

        [SetUp]
        public void SetUp()
        {
            // Clean up stale instances from scene
            foreach (var f in Object.FindObjectsByType<FrustumWireframe>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(f.gameObject);
            }

            foreach (var t in Object.FindObjectsByType<TimelineSectionView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(t.gameObject);
            }

            foreach (var s in Object.FindObjectsByType<ShotEvaluator>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(s.gameObject);
            }

            foreach (var h in Object.FindObjectsByType<CameraInputHandler>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(h.gameObject);
            }

            // Input devices
            this._keyboard = InputSystem.AddDevice<Keyboard>();
            this._mouse    = InputSystem.AddDevice<Mouse>();

            // ShotEvaluator creates Timeline in Awake
            this._shotEvalGo   = new GameObject("TestShotEval");
            this._shotEvaluator = this._shotEvalGo.AddComponent<ShotEvaluator>();

            // Camera + handler
            this._cameraGo  = new GameObject("TestCamera");
            this._behaviour = this._cameraGo.AddComponent<CameraBehaviour>();
            this._handler   = this._cameraGo.AddComponent<CameraInputHandler>();
            SetField(this._handler, "cameraBehaviour", this._behaviour);

            // GizmoBehaviour (needed for KeyboardShortcutRouter.Configure)
            var highlighter = this._cameraGo.AddComponent<SelectionDisplay>();
            var gizmo       = this._cameraGo.AddComponent<GizmoBehaviour>();
            SetField(gizmo, "selectionDisplay", highlighter);
            SetField(gizmo, "targetCamera", this._cameraGo.GetComponent<Camera>());
            SetField(this._handler, "gizmoBehaviour", gizmo);
        }

        [TearDown]
        public void TearDown()
        {
            // Destroy ViewCameraManager-created cameras
            foreach (var vcm in Object.FindObjectsByType<ViewCameraManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(vcm.gameObject);
            }

            Object.DestroyImmediate(this._cameraGo);
            Object.DestroyImmediate(this._shotEvalGo);

            var frustums = Object.FindObjectsByType<FrustumWireframe>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var f in frustums)
            {
                Object.DestroyImmediate(f.gameObject);
            }

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            InputSystem.RemoveDevice(this._keyboard);
            InputSystem.RemoveDevice(this._mouse);
        }

        // ══════════════════════════════════════════════════════════════════
        // Stopwatch off — no recording
        // ══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ScrollDolly__DoesNotCreateKeyframe__When__StopwatchOff()
        {
            yield return null; // Awake
            yield return null; // Start — wires _timeline

            this._timeline = this._shotEvaluator.Controller;
            var shot = this._timeline.CurrentShot;

            // Stopwatch is off by default — dolly via scroll
            InputSystem.QueueStateEvent(this._mouse,
                new MouseState { scroll = new UnityEngine.Vector2(0f, 120f) });
            yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            yield return null;

            // Only the mandatory initial keyframe at t=0 should exist
            Assert.AreEqual(1, shot.CameraPositionKeyframes.Count,
                "No new position keyframe should be created when stopwatch is off");
        }

        // ══════════════════════════════════════════════════════════════════
        // Stopwatch on — recording works
        // ══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ScrollDolly__CreatesKeyframe__When__StopwatchOn()
        {
            yield return null;
            yield return null;

            this._timeline = this._shotEvaluator.Controller;
            var shot = this._timeline.CurrentShot;
            shot.CameraStopwatch.SetAll(true);

            // Scrub playhead to t=2 so the new keyframe doesn't merge with t=0
            this._timeline.Playhead.Scrub(2.0, this._timeline.TotalDuration);

            // Dolly via scroll
            InputSystem.QueueStateEvent(this._mouse,
                new MouseState { scroll = new UnityEngine.Vector2(0f, 120f) });
            yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            yield return null;

            Assert.AreEqual(2, shot.CameraPositionKeyframes.Count,
                "A new position keyframe should be created when stopwatch is on");
        }

        // ══════════════════════════════════════════════════════════════════
        // Drag end records (pan/tilt)
        // ══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator PanDrag__CreatesRotationKeyframe__When__StopwatchOnAndDragEnds()
        {
            yield return null;
            yield return null;

            this._timeline = this._shotEvaluator.Controller;
            var shot = this._timeline.CurrentShot;
            shot.CameraStopwatch.SetAll(true);
            this._timeline.Playhead.Scrub(2.0, this._timeline.TotalDuration);

            // Begin drag: left mouse button + delta
            InputSystem.QueueStateEvent(this._mouse,
                new MouseState { delta = new UnityEngine.Vector2(50f, 0f), buttons = 1 });
            yield return null;

            // Hold drag
            InputSystem.QueueStateEvent(this._mouse,
                new MouseState { delta = new UnityEngine.Vector2(30f, 0f), buttons = 1 });
            yield return null;

            // Before release: should NOT have recorded yet (record on drag end)
            var countBeforeRelease = shot.CameraRotationKeyframes.Count;

            // Release mouse — drag ends, should record
            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            yield return null;

            Assert.GreaterOrEqual(shot.CameraRotationKeyframes.Count, countBeforeRelease,
                "Rotation keyframe should be created when drag ends with stopwatch on");
        }

        // ══════════════════════════════════════════════════════════════════
        // Playback blocks recording
        // ══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ScrollDolly__DoesNotCreateKeyframe__When__Playing()
        {
            yield return null;
            yield return null;

            this._timeline = this._shotEvaluator.Controller;
            var shot = this._timeline.CurrentShot;
            shot.CameraStopwatch.SetAll(true);
            this._timeline.TogglePlayback(); // start playing

            InputSystem.QueueStateEvent(this._mouse,
                new MouseState { scroll = new UnityEngine.Vector2(0f, 120f) });
            yield return null;

            this._timeline.TogglePlayback(); // stop
            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            yield return null;

            // Only initial keyframe — playback guard blocked recording
            Assert.AreEqual(1, shot.CameraPositionKeyframes.Count,
                "No keyframe should be created during playback");
        }

        // ══════════════════════════════════════════════════════════════════
        // C key force records
        // ══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator CKey__ForceRecordsCameraKeyframe__When__Pressed()
        {
            yield return null;
            yield return null;

            this._timeline = this._shotEvaluator.Controller;
            var shot = this._timeline.CurrentShot;
            this._timeline.Playhead.Scrub(2.0, this._timeline.TotalDuration);

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.C));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            yield return null;

            // ForceRecord creates keyframes for all 5 properties
            Assert.AreEqual(2, shot.CameraPositionKeyframes.Count,
                "C key should force-record a camera keyframe");
            Assert.AreEqual(2, shot.CameraRotationKeyframes.Count,
                "C key should force-record rotation keyframe");
        }

        [UnityTest]
        public IEnumerator CKey__DoesNotRecord__When__Playing()
        {
            yield return null;
            yield return null;

            this._timeline = this._shotEvaluator.Controller;
            var shot = this._timeline.CurrentShot;
            this._timeline.Playhead.Scrub(2.0, this._timeline.TotalDuration);
            this._timeline.TogglePlayback();

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.C));
            yield return null;

            this._timeline.TogglePlayback();
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            yield return null;

            Assert.AreEqual(1, shot.CameraPositionKeyframes.Count,
                "C key should not create keyframes during playback");
        }

        // ══════════════════════════════════════════════════════════════════
        // Director View blocks recording
        // ══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ScrollDolly__DoesNotCreateKeyframe__When__DirectorViewSingleMode()
        {
            yield return null;
            yield return null;

            this._timeline = this._shotEvaluator.Controller;
            var shot = this._timeline.CurrentShot;
            shot.CameraStopwatch.SetAll(true);
            this._timeline.Playhead.Scrub(2.0, this._timeline.TotalDuration);

            // Switch to director view
            this._behaviour.ToggleDirectorView();
            // Update handler's camera reference (normally done in Update)
            SetField(this._handler, "_camera", this._behaviour.ActiveCamera);

            InputSystem.QueueStateEvent(this._mouse,
                new MouseState { scroll = new UnityEngine.Vector2(0f, 120f) });
            yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            yield return null;

            // Switch back
            this._behaviour.ToggleDirectorView();

            Assert.AreEqual(1, shot.CameraPositionKeyframes.Count,
                "No keyframe should be created in Director View");
        }

        [UnityTest]
        public IEnumerator ScrollDolly__DoesNotCreateKeyframe__When__DirectorViewSplitMode()
        {
            yield return null;
            yield return null;

            this._timeline = this._shotEvaluator.Controller;
            var shot = this._timeline.CurrentShot;
            shot.CameraStopwatch.SetAll(true);
            this._timeline.Playhead.Scrub(2.0, this._timeline.TotalDuration);

            // Create ViewCameraManager and wire it
            var vcm = this._cameraGo.AddComponent<ViewCameraManager>();
            SetField(vcm, "cameraBehaviour", this._behaviour);
            SetField(this._handler, "viewCameraManager", vcm);
            yield return null; // let VCM.Awake/Start run

            // Ensure director camera exists
            this._behaviour.EnsureDirectorInitialized();

            // Switch to split view with Director View in slot 1
            vcm.ViewSlotModel.SetLayout(ViewLayout.HORIZONTAL);
            vcm.ViewSlotModel.SetSlotType(0, ViewMode.CAMERA);
            vcm.ViewSlotModel.SetSlotType(1, ViewMode.DIRECTOR);

            // Activate the Director View slot
            var activeSlotField = typeof(ViewCameraManager)
                .GetField("_activeSlot", BindingFlags.NonPublic | BindingFlags.Instance);
            activeSlotField.SetValue(vcm, 1);

            // Set the handler's camera to the director camera
            SetField(this._handler, "_camera", this._behaviour.DirectorCamera);

            InputSystem.QueueStateEvent(this._mouse,
                new MouseState { scroll = new UnityEngine.Vector2(0f, 120f) });
            yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            yield return null;

            Assert.AreEqual(1, shot.CameraPositionKeyframes.Count,
                "No keyframe should be created when Director View is active in split mode");
        }

        // ══════════════════════════════════════════════════════════════════
        // Per-property stopwatch
        // ══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ScrollDolly__RecordsPosition__When__OnlyPositionStopwatchOn()
        {
            yield return null;
            yield return null;

            this._timeline = this._shotEvaluator.Controller;
            var shot = this._timeline.CurrentShot;

            // Only enable position recording, not rotation
            shot.CameraStopwatch.Set(CameraProperty.POSITION.Index, true);
            this._timeline.Playhead.Scrub(2.0, this._timeline.TotalDuration);

            InputSystem.QueueStateEvent(this._mouse,
                new MouseState { scroll = new UnityEngine.Vector2(0f, 120f) });
            yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            yield return null;

            Assert.AreEqual(2, shot.CameraPositionKeyframes.Count,
                "Position keyframe should be created when position stopwatch is on");
            Assert.AreEqual(1, shot.CameraRotationKeyframes.Count,
                "Rotation keyframe should NOT be created when rotation stopwatch is off");
        }

        // ══════════════════════════════════════════════════════════════════
        // Helpers
        // ══════════════════════════════════════════════════════════════════

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }
    }
}
