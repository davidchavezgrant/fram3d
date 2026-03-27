using System.Collections;
using System.Reflection;
using Fram3d.Core.Cameras;
using Fram3d.Core.Scenes;
using Fram3d.Core.Viewports;
using Fram3d.Engine.Integration;
using Fram3d.UI.Input;
using Fram3d.UI.Panels;
using Fram3d.UI.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Play Mode tests for CameraInputHandler keyboard shortcuts and input routing.
    ///
    /// Extends InputTestFixture to isolate the Input System — all Editor devices
    /// are removed, so test devices are the only Keyboard.current / Mouse.current.
    /// This prevents the .current identity race that caused persistent flaking.
    ///
    /// With isolation, Update() → Tick(Keyboard.current, Mouse.current) uses the
    /// test devices directly. No explicit Tick() calls needed.
    /// </summary>
    public sealed class CameraInputHandlerTests: InputTestFixture
    {
        private CameraBehaviour      _behaviour;
        private CameraElement        _cam;
        private GizmoBehaviour      _gizmoController;
        private GameObject           _go;
        private GameObject           _guideGo;
        private CompositionGuideView _guideView;
        private CameraInputHandler   _handler;
        private Keyboard             _keyboard;
        private Mouse                _mouse;

        // --- A key: cycle aspect ratio ---

        [UnityTest]
        public IEnumerator AKey__CyclesAspectRatioForward__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.ActiveAspectRatio;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.A));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreNotSame(before, this._cam.ActiveAspectRatio);
        }

        [UnityTest]
        public IEnumerator AltG__TogglesSafeZones__When__Pressed()
        {
            yield return null;
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftAlt, Key.G));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.IsFalse(this._guideView.Settings.ThirdsVisible);
            Assert.IsFalse(this._guideView.Settings.CenterCrossVisible);
            Assert.IsTrue(this._guideView.Settings.SafeZonesVisible);
        }

        // --- Drag: Alt+left mouse → Orbit ---

        [UnityTest]
        public IEnumerator AltLeftMouseDrag__Orbits__When__Dragged()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var posBefore = this._cam.Position;

            // Left button = bit 0 (value 1)
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftAlt));

            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            delta   = new Vector2(50f, 0f),
                                            buttons = 1
                                        });

            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse,    new MouseState());
            Assert.AreNotEqual(posBefore, this._cam.Position);
        }

        // --- Scroll: Alt+scroll Y → Crane ---

        [UnityTest]
        public IEnumerator AltScrollY__Cranes__When__ScrolledVertically()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var yBefore = this._cam.Position.Y;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftAlt));
            InputSystem.QueueStateEvent(this._mouse,    new MouseState { scroll = new Vector2(0, 10f) });
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse,    new MouseState());
            Assert.That(Mathf.Abs(yBefore - this._cam.Position.Y), Is.GreaterThan(0.001f));
        }

        // --- Drag: Cmd+left mouse → Pan/Tilt (trackpad-friendly) ---

        [UnityTest]
        public IEnumerator CmdLeftMouseDrag__PansAndTilts__When__Dragged()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var rotBefore = this._cam.Rotation;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCommand));

            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            delta   = new Vector2(50f, 30f),
                                            buttons = 1
                                        });

            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse,    new MouseState());
            Assert.AreNotEqual(rotBefore, this._cam.Rotation);
        }

        // --- Modifier keys should NOT trigger unmodified shortcuts ---

        [UnityTest]
        public IEnumerator CtrlA__DoesNotCycleAspectRatio__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.ActiveAspectRatio;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl, Key.A));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreSame(before, this._cam.ActiveAspectRatio);
        }

        [UnityTest]
        public IEnumerator CtrlG__TogglesCenterCross__When__Pressed()
        {
            yield return null;
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl, Key.G));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.IsFalse(this._guideView.Settings.ThirdsVisible);
            Assert.IsTrue(this._guideView.Settings.CenterCrossVisible);
            Assert.IsFalse(this._guideView.Settings.SafeZonesVisible);
        }

        [UnityTest]
        public IEnumerator CtrlR__DoesNotSwitchToScale__When__CtrlHeld()
        {
            yield return null;

            this._gizmoController.SetActiveTool(ActiveTool.TRANSLATE);
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl, Key.R));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreSame(ActiveTool.TRANSLATE, this._gizmoController.ActiveTool, "Ctrl+R should not switch tool (reserved for reset)");
        }

        // --- Ctrl+R: reset ---

        [UnityTest]
        public IEnumerator CtrlR__ResetsCameraPosition__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            this._cam.Dolly(5.0f);
            this._cam.Pan(1.0f);
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl, Key.R));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            var defaultPos = new System.Numerics.Vector3(0f, 1.6f, 5f);
            Assert.AreEqual(defaultPos.X, this._cam.Position.X, 0.01f);
            Assert.AreEqual(defaultPos.Y, this._cam.Position.Y, 0.01f);
            Assert.AreEqual(defaultPos.Z, this._cam.Position.Z, 0.01f);
        }

        // --- Scroll: Ctrl+scroll X → Truck ---

        [UnityTest]
        public IEnumerator CtrlScrollX__Trucks__When__ScrolledHorizontally()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var posBefore = this._cam.Position;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl));
            InputSystem.QueueStateEvent(this._mouse,    new MouseState { scroll = new Vector2(10f, 0) });
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse,    new MouseState());
            Assert.That(Mathf.Abs(posBefore.X - this._cam.Position.X), Is.GreaterThan(0.001f));
        }

        // --- Scroll: Ctrl+scroll Y → Dolly ---

        [UnityTest]
        public IEnumerator CtrlScrollY__Dollies__When__ScrolledVertically()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var posBefore = this._cam.Position;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl));
            InputSystem.QueueStateEvent(this._mouse,    new MouseState { scroll = new Vector2(0, 10f) });
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse,    new MouseState());
            Assert.That(Mathf.Abs(posBefore.Z - this._cam.Position.Z), Is.GreaterThan(0.001f));
        }

        // --- FRA-126: Delta spike rejection (orbit throw) ---

        [UnityTest]
        public IEnumerator DeltaSpike__DoesNotOrbit__When__DeltaExceedsThreshold()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var posBefore = this._cam.Position;
            var rotBefore = this._cam.Rotation;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftAlt));

            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            delta   = new Vector2(500f, 300f),
                                            buttons = 1
                                        });

            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse,    new MouseState());

            Assert.AreEqual(posBefore.X,
                            this._cam.Position.X,
                            0.001f,
                            "Camera should not move from a delta spike");

            Assert.AreEqual(posBefore.Z,
                            this._cam.Position.Z,
                            0.001f,
                            "Camera should not move from a delta spike");
        }

        // --- Number keys: focal length presets ---

        [UnityTest]
        public IEnumerator Digit1__SetsFocalLengthPreset__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.FocalLength;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.Digit1));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreNotEqual(before, this._cam.FocalLength);
        }

        // --- D key: toggle Director View ---

        [UnityTest]
        public IEnumerator DKey__TogglesDirectorView__When__Pressed()
        {
            yield return null;

            Assert.IsFalse(this._behaviour.IsDirectorView, "Precondition");
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.D));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.IsTrue(this._behaviour.IsDirectorView, "D should toggle Director View");
        }

        [UnityTest]
        public IEnumerator DKey__RoutesInputToDirectorCamera__When__InDirectorView()
        {
            yield return null;

            // Enter Director View
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.D));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            yield return null;

            // The handler's _camera should now be the director camera
            var shotPos = this._behaviour.CameraElement.Position;

            // Scroll to change focal length on the director camera
            InputSystem.QueueStateEvent(this._mouse, new MouseState { scroll = new Vector2(0, 10f) });
            yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            yield return null;

            // Shot camera should not have changed
            Assert.AreEqual(shotPos.X, this._behaviour.CameraElement.Position.X, 0.001f, "Shot camera position should not change from director input");
        }

        // --- Shift+D: toggle DOF ---

        [UnityTest]
        public IEnumerator ShiftD__TogglesDof__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.DofEnabled;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftShift, Key.D));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreNotEqual(before, this._cam.DofEnabled, "Shift+D should toggle DOF");
        }

        [UnityTest]
        public IEnumerator EKey__SwitchesToRotate__When__Pressed()
        {
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.E));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreSame(ActiveTool.ROTATE, this._gizmoController.ActiveTool);
        }

        // --- Composition guide shortcuts ---

        [UnityTest]
        public IEnumerator GKey__TogglesAllGuides__When__Pressed()
        {
            yield return null;
            yield return null;

            Assert.IsFalse(this._guideView.Settings.AnyVisible);
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.G));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.IsTrue(this._guideView.Settings.ThirdsVisible);
            Assert.IsTrue(this._guideView.Settings.CenterCrossVisible);
            Assert.IsTrue(this._guideView.Settings.SafeZonesVisible);
        }

        // --- HasFocusedTextField guard (typing in search shouldn't trigger shortcuts) ---

        [UnityTest]
        public IEnumerator HasFocusedTextField__BlocksKeyboardShortcuts__When__SearchFieldOpen()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;

            // Create a PropertiesPanelView and wire it to the handler
            var panelGo    = new GameObject("TestPanel");
            var uiDocument = panelGo.AddComponent<UIDocument>();
            var guids      = UnityEditor.AssetDatabase.FindAssets("t:PanelSettings");

            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                uiDocument.panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            }

            var panel = panelGo.AddComponent<PropertiesPanelView>();
            SetField(this._handler, "propertiesPanel", panel);

            // Wait for panel to initialize
            yield return null;
            yield return null;

            // Simulate a dropdown being open (HasFocusedTextField = true)
            var bodySectionField = typeof(PropertiesPanelView).GetField("_bodySection", BindingFlags.NonPublic | BindingFlags.Instance);
            var bodySection      = bodySectionField?.GetValue(panel);

            if (bodySection == null)
            {
                Object.DestroyImmediate(panelGo);
                Assert.Inconclusive("Could not access body section for focus test");
                yield break;
            }

            var dropdownField = bodySection.GetType().GetField("_dropdown", BindingFlags.NonPublic | BindingFlags.Instance);
            var dropdown      = dropdownField?.GetValue(bodySection);

            if (dropdown == null)
            {
                Object.DestroyImmediate(panelGo);
                Assert.Inconclusive("Could not access dropdown for focus test");
                yield break;
            }

            // Open the dropdown (sets HasFocus = true)
            var openMethod = dropdown.GetType().GetMethod("Open", BindingFlags.NonPublic | BindingFlags.Instance);
            openMethod.Invoke(dropdown, null);
            yield return null;

            var before = this._cam.ActiveAspectRatio;

            // Press A — should NOT cycle aspect ratio because search field has focus
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.A));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreSame(before, this._cam.ActiveAspectRatio, "A key should not cycle aspect ratio when search field has focus");

            // Close dropdown and verify A key works again
            var closeMethod = dropdown.GetType().GetMethod("Close", BindingFlags.Public | BindingFlags.Instance);
            closeMethod.Invoke(dropdown, null);
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.A));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreNotSame(before, this._cam.ActiveAspectRatio, "A key should cycle aspect ratio after search field closes");
            Object.DestroyImmediate(panelGo);
        }

        // --- Bracket keys: aperture ---

        [UnityTest]
        public IEnumerator LeftBracket__StepsApertureWider__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.Aperture;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftBracket));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.Less(this._cam.Aperture, before);
        }

        // --- Drag: middle mouse → Pan/Tilt (external mouse fallback) ---

        [UnityTest]
        public IEnumerator MiddleMouseDrag__PansAndTilts__When__Dragged()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var rotBefore = this._cam.Rotation;

            // Middle button = bit 2 (value 4) in MouseState.buttons
            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            delta   = new Vector2(50f, 30f),
                                            buttons = 4
                                        });

            yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            Assert.AreNotEqual(rotBefore, this._cam.Rotation);
        }

        [UnityTest]
        public IEnumerator NormalDelta__DoesOrbit__When__DeltaWithinThreshold()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var posBefore = this._cam.Position;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftAlt));

            InputSystem.QueueStateEvent(this._mouse,
                                        new MouseState
                                        {
                                            delta   = new Vector2(50f, 0f),
                                            buttons = 1
                                        });

            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse,    new MouseState());
            Assert.AreNotEqual(posBefore, this._cam.Position, "Camera should orbit with a normal delta");
        }

        [UnityTest]
        public IEnumerator OnApplicationFocus__ClearsStaleScrollSamples__When__FocusRegained()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;

            this._cam.SetLensSet(new LensSet("Zoom",
                                             24f,
                                             200f,
                                             false,
                                             1.0f));

            this._cam.SetFocalLengthPreset(50f);
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl));
            InputSystem.QueueStateEvent(this._mouse,    new MouseState { scroll = new Vector2(0, 10f) });
            this._handler.SendMessage("OnApplicationFocus", true);
            yield return null;

            var posBefore = this._cam.Position;

            // Wait for cooldown
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(posBefore.Z,
                            this._cam.Position.Z,
                            0.01f,
                            "Stale scroll samples should be cleared on focus regain");
        }

        // --- FRA-127: Modifier state resync on focus regain ---

        [UnityTest]
        public IEnumerator OnApplicationFocus__ResyncsModifiers__When__CtrlStuckAfterFocusLoss()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;

            this._cam.SetLensSet(new LensSet("Zoom",
                                             24f,
                                             200f,
                                             false,
                                             1.0f));

            this._cam.SetFocalLengthPreset(50f);

            // Wait for any stale momentum
            yield return new WaitForSeconds(0.3f);

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            yield return null;

            this._handler.SendMessage("OnApplicationFocus", true);
            yield return null;

            var flBefore = this._cam.FocalLength;

            // Wait for cooldown
            yield return new WaitForSeconds(0.3f);

            InputSystem.QueueStateEvent(this._mouse, new MouseState { scroll = new Vector2(0, 5f) });
            yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            Assert.AreNotEqual(flBefore, this._cam.FocalLength, "After focus resync, unmodified scroll should change focal length, not dolly");
        }

        // --- Q/W/E/R: tool switching ---

        [UnityTest]
        public IEnumerator QKey__SwitchesToSelect__When__Pressed()
        {
            yield return null;

            this._gizmoController.SetActiveTool(ActiveTool.TRANSLATE);
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.Q));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreSame(ActiveTool.SELECT, this._gizmoController.ActiveTool);
        }

        [UnityTest]
        public IEnumerator RightBracket__StepsApertureNarrower__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.Aperture;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.RightBracket));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.Greater(this._cam.Aperture, before);
        }

        [UnityTest]
        public IEnumerator RKey__SwitchesToScale__When__Pressed()
        {
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.R));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreSame(ActiveTool.SCALE, this._gizmoController.ActiveTool);
        }

        // --- Scroll: unmodified → focal length ---

        [UnityTest]
        public IEnumerator Scroll__ChangesFocalLength__When__Unmodified()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;

            this._cam.SetLensSet(new LensSet("Zoom",
                                             24f,
                                             200f,
                                             false,
                                             1.0f));

            this._cam.SetFocalLengthPreset(50f);
            var before = this._cam.FocalLength;

            // Need to wait long enough for momentum guard to expire
            for (var i = 0; i < 30; i++)
                yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState { scroll = new Vector2(0, 5f) });
            yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            Assert.AreNotEqual(before, this._cam.FocalLength);
        }

        [UnityTest]
        public IEnumerator ScrollBleed__AllowsScroll__When__AfterCooldownExpires()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;

            this._cam.SetLensSet(new LensSet("Zoom",
                                             24f,
                                             200f,
                                             false,
                                             1.0f));

            this._cam.SetFocalLengthPreset(50f);

            // Wait for any stale momentum
            yield return new WaitForSeconds(0.3f);

            // Modifier+scroll to set _lastModifierScrollTime
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl));
            InputSystem.QueueStateEvent(this._mouse,    new MouseState { scroll = new Vector2(0, 5f) });
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse,    new MouseState());
            yield return null;

            var flBefore = this._cam.FocalLength;

            // Wait for cooldown to expire (150ms + 50ms margin)
            yield return new WaitForSeconds(0.2f);

            // Now unmodified scroll should pass through
            InputSystem.QueueStateEvent(this._mouse, new MouseState { scroll = new Vector2(0, 5f) });
            yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState());
            Assert.AreNotEqual(flBefore, this._cam.FocalLength, "Unmodified scroll should work after cooldown expires");
        }

        // --- Scroll bleed momentum guard (150ms cooldown) ---

        [UnityTest]
        public IEnumerator ScrollBleed__BlocksMomentumScroll__When__WithinCooldownPeriod()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;

            this._cam.SetLensSet(new LensSet("Zoom",
                                             24f,
                                             200f,
                                             false,
                                             1.0f));

            this._cam.SetFocalLengthPreset(50f);

            // Wait for any stale momentum from previous tests to expire
            yield return new WaitForSeconds(0.3f);

            // Modifier+scroll to set _lastModifierScrollTime
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl));
            InputSystem.QueueStateEvent(this._mouse,    new MouseState { scroll = new Vector2(0, 5f) });
            yield return null;

            // Release modifier, clear scroll
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse,    new MouseState());
            yield return null;

            var flAfterModifier = this._cam.FocalLength;

            // Immediately send unmodified scroll — should be BLOCKED (within 150ms)
            InputSystem.QueueStateEvent(this._mouse, new MouseState { scroll = new Vector2(0, 5f) });
            yield return null;

            InputSystem.QueueStateEvent(this._mouse, new MouseState());

            Assert.AreEqual(flAfterModifier,
                            this._cam.FocalLength,
                            0.001f,
                            "Unmodified scroll should be blocked within momentum cooldown");
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            // Add devices FIRST — they become the only .current devices
            this._keyboard = InputSystem.AddDevice<Keyboard>();
            this._mouse    = InputSystem.AddDevice<Mouse>();

            // Create GameObjects AFTER input isolation — OnEnable registers
            // InputSystem.onEvent on the isolated system
            this._go        = new GameObject("TestCamera");
            this._behaviour = this._go.AddComponent<CameraBehaviour>();
            this._handler   = this._go.AddComponent<CameraInputHandler>();
            SetField(this._handler, "cameraBehaviour", this._behaviour);

            // Wire composition guides
            this._guideGo = new GameObject("TestGuides");
            var uiDoc = this._guideGo.AddComponent<UIDocument>();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:PanelSettings");

            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                uiDoc.panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            }

            this._guideView = this._guideGo.AddComponent<CompositionGuideView>();
            SetField(this._handler, "compositionGuides", this._guideView);

            // Wire GizmoBehaviour for Q/W/E/R tests
            var highlighter = this._go.AddComponent<SelectionDisplay>();
            this._gizmoController = this._go.AddComponent<GizmoBehaviour>();
            SetField(this._gizmoController, "selectionDisplay", highlighter);
            SetField(this._gizmoController, "targetCamera",         this._go.GetComponent<Camera>());
            SetField(this._handler,         "gizmoBehaviour",      this._gizmoController);
        }

        [UnityTest]
        public IEnumerator ShiftA__CyclesAspectRatioBackward__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.ActiveAspectRatio;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftShift, Key.A));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreNotSame(before, this._cam.ActiveAspectRatio);
        }

        [UnityTest]
        public IEnumerator ShiftG__TogglesThirds__When__Pressed()
        {
            yield return null;
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftShift, Key.G));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.IsTrue(this._guideView.Settings.ThirdsVisible);
            Assert.IsFalse(this._guideView.Settings.CenterCrossVisible);
            Assert.IsFalse(this._guideView.Settings.SafeZonesVisible);
        }

        // --- Scroll: Shift+scroll X → Roll ---

        [UnityTest]
        public IEnumerator ShiftScrollX__Rolls__When__ScrolledHorizontally()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var rotBefore = this._cam.Rotation;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftShift));
            InputSystem.QueueStateEvent(this._mouse,    new MouseState { scroll = new Vector2(10f, 0) });
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse,    new MouseState());
            Assert.AreNotEqual(rotBefore, this._cam.Rotation);
        }

        // --- S key: toggle shake ---

        [UnityTest]
        public IEnumerator SKey__TogglesShake__When__Pressed()
        {
            yield return null;

            this._cam = this._behaviour.CameraElement;
            var before = this._cam.ShakeEnabled;
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.S));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreNotEqual(before, this._cam.ShakeEnabled);
        }

        [TearDown]
        public override void TearDown()
        {
            var gizmoRoot = GameObject.Find("GizmoRoot");

            if (gizmoRoot != null)
            {
                Object.DestroyImmediate(gizmoRoot);
            }

            var frustum = GameObject.Find("Shot Camera Frustum");

            if (frustum != null)
            {
                Object.DestroyImmediate(frustum);
            }

            Object.DestroyImmediate(this._guideGo);
            Object.DestroyImmediate(this._go);
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator WKey__SwitchesToTranslate__When__Pressed()
        {
            yield return null;

            this._gizmoController.SetActiveTool(ActiveTool.SELECT);
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.W));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            Assert.AreSame(ActiveTool.TRANSLATE, this._gizmoController.ActiveTool);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }
    }
}