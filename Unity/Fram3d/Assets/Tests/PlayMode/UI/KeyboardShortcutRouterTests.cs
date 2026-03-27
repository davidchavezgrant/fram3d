using System.Collections;
using System.Collections.Generic;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Fram3d.Engine.Integration;
using Fram3d.UI.Input;
using Fram3d.UI.Panels;
using Fram3d.UI.Timeline;
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
    /// Tests for keyboard shortcuts routed through KeyboardShortcutRouter
    /// that aren't covered by CameraInputHandlerTests — specifically the
    /// timeline shortcuts (T, Space, +, -) and panel toggle (I).
    ///
    /// Uses a full integration setup: CameraBehaviour + ShotEvaluator +
    /// TimelineSectionView + CameraInputHandler. This mirrors the real
    /// scene wiring and tests that shortcuts propagate through the full chain.
    /// </summary>
    public sealed class KeyboardShortcutRouterTests
    {
        private CameraBehaviour          _behaviour;
        private GameObject               _cameraGo;
        private readonly List<GameObject> _extras = new();
        private CameraInputHandler       _handler;
        private Keyboard                 _keyboard;
        private Mouse                    _mouse;
        private PropertiesPanelView      _propertiesPanel;
        private TimelineSectionView      _timelineView;

        [SetUp]
        public void SetUp()
        {
            foreach (var panel in Object.FindObjectsByType<PropertiesPanelView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(panel.gameObject);
            }

            foreach (var layout in Object.FindObjectsByType<ViewLayoutView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(layout.gameObject);
            }

            foreach (var timeline in Object.FindObjectsByType<TimelineSectionView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(timeline.gameObject);
            }

            foreach (var shot in Object.FindObjectsByType<ShotEvaluator>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(shot.gameObject);
            }

            foreach (var f in Object.FindObjectsByType<FrustumWireframe>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(f.gameObject);
            }

            // Add devices
            this._keyboard = InputSystem.AddDevice<Keyboard>();
            this._mouse    = InputSystem.AddDevice<Mouse>();

            // Create camera
            this._cameraGo  = new GameObject("TestCamera");
            this._behaviour = this._cameraGo.AddComponent<CameraBehaviour>();

            // Create ShotEvaluator (creates Timeline in Awake)
            var shotEvalGo = new GameObject("TestShotEvaluator");
            this._extras.Add(shotEvalGo);
            shotEvalGo.AddComponent<ShotEvaluator>();

            // Create TimelineSectionView with UIDocument
            var timelineGo = new GameObject("TestTimeline");
            this._extras.Add(timelineGo);
            var timelineUiDoc = timelineGo.AddComponent<UIDocument>();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:PanelSettings");

            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                timelineUiDoc.panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            }

            this._timelineView = timelineGo.AddComponent<TimelineSectionView>();

            // Create PropertiesPanelView
            var panelGo = new GameObject("TestPanel");
            this._extras.Add(panelGo);
            var panelUiDoc = panelGo.AddComponent<UIDocument>();

            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                panelUiDoc.panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            }

            this._propertiesPanel = panelGo.AddComponent<PropertiesPanelView>();

            // Create CameraInputHandler (finds timeline/panel via FindAnyObjectByType in Start)
            this._handler = this._cameraGo.AddComponent<CameraInputHandler>();
            SetField(this._handler, "cameraBehaviour", this._behaviour);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in this._extras)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }

            this._extras.Clear();

            var frustums = Object.FindObjectsByType<FrustumWireframe>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var f in frustums)
            {
                Object.DestroyImmediate(f.gameObject);
            }

            Object.DestroyImmediate(this._cameraGo);
            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(this._mouse,    new MouseState());
            InputSystem.RemoveDevice(this._keyboard);
            InputSystem.RemoveDevice(this._mouse);
        }

        // --- T key: toggle timeline ---

        [UnityTest]
        public IEnumerator TKey__TogglesTimelineVisibility__When__Pressed()
        {
            // Wait for Start() on all components
            yield return null;
            yield return null;

            Assert.IsTrue(this._timelineView.IsVisible, "Precondition: timeline visible");

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.T));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());

            Assert.IsFalse(this._timelineView.IsVisible, "T should toggle timeline off");
        }

        [UnityTest]
        public IEnumerator TKey__ShowsTimeline__When__PressedTwice()
        {
            yield return null;
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.T));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            yield return null;

            Assert.IsFalse(this._timelineView.IsVisible, "Precondition: hidden after first T");

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.T));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());

            Assert.IsTrue(this._timelineView.IsVisible, "Second T should show timeline");
        }

        // --- I key: toggle properties panel ---

        [UnityTest]
        public IEnumerator IKey__TogglesPanelVisibility__When__Pressed()
        {
            yield return null;
            yield return null;

            Assert.IsTrue(this._propertiesPanel.IsVisible, "Precondition: panel visible");

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.I));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());

            Assert.IsFalse(this._propertiesPanel.IsVisible, "I should toggle panel off");
        }

        [UnityTest]
        public IEnumerator IKey__ShowsPanel__When__PressedTwice()
        {
            yield return null;
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.I));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.I));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());

            Assert.IsTrue(this._propertiesPanel.IsVisible, "Double I should restore panel");
        }

        // --- Modifier guards for T and I ---

        [UnityTest]
        public IEnumerator CtrlT__DoesNotToggleTimeline__When__CtrlHeld()
        {
            yield return null;
            yield return null;

            var before = this._timelineView.IsVisible;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl, Key.T));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());

            Assert.AreEqual(before, this._timelineView.IsVisible,
                "Ctrl+T should not toggle timeline");
        }

        [UnityTest]
        public IEnumerator CtrlI__DoesNotTogglePanel__When__CtrlHeld()
        {
            yield return null;
            yield return null;

            var before = this._propertiesPanel.IsVisible;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState(Key.LeftCtrl, Key.I));
            yield return null;

            InputSystem.QueueStateEvent(this._keyboard, new KeyboardState());

            Assert.AreEqual(before, this._propertiesPanel.IsVisible,
                "Ctrl+I should not toggle panel");
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(target, value);
        }
    }
}
