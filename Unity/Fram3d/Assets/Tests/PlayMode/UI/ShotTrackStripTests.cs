using System.Collections;
using Fram3d.Core.Common;
using Fram3d.Core.Timelines;
using Fram3d.UI.Timeline;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Fram3d.Tests.UI
{
    /// <summary>
    /// Tests for ShotTrackStrip — the shot track area showing shot blocks,
    /// playhead, out-of-range overlay, and drop indicator.
    /// Wires a real Timeline Core controller to verify block creation,
    /// active states, and rebuild behavior.
    /// </summary>
    public sealed class ShotTrackStripTests
    {
        private Timeline   _controller;
        private UIDocument _uiDocument;
        private GameObject _uiGo;

        [SetUp]
        public void SetUp()
        {
            this._controller = new Timeline(FrameRate.FPS_24);
            this._controller.InitializeViewRange(800);

            this._uiGo       = new GameObject("TestUI");
            this._uiDocument = this._uiGo.AddComponent<UIDocument>();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:PanelSettings");

            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                this._uiDocument.panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            }
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(this._uiGo);
        }

        [Test]
        public void Constructor__CreatesLabelColumnAndTrackArea__When__Created()
        {
            var strip = new ShotTrackStrip();

            Assert.GreaterOrEqual(strip.childCount, 2,
                "Strip should have label column and track area");
        }

        [Test]
        public void Constructor__HasDropIndicator__When__Created()
        {
            var strip = new ShotTrackStrip();
            var found = false;

            for (var i = 0; i < strip.TrackArea.childCount; i++)
            {
                if (strip.TrackArea[i].ClassListContains("shot-track__drop-indicator"))
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, "Track area should contain a drop indicator");
        }

        [Test]
        public void Constructor__HasPlayhead__When__Created()
        {
            var strip = new ShotTrackStrip();
            var found = false;

            for (var i = 0; i < strip.TrackArea.childCount; i++)
            {
                if (strip.TrackArea[i].ClassListContains("timeline-playhead"))
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, "Track area should contain a playhead");
        }

        [Test]
        public void Constructor__HasOutOfRange__When__Created()
        {
            var strip = new ShotTrackStrip();
            var found = false;

            for (var i = 0; i < strip.TrackArea.childCount; i++)
            {
                if (strip.TrackArea[i].ClassListContains("timeline-out-of-range"))
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, "Track area should contain an out-of-range overlay");
        }

        [Test]
        public void Constructor__DropIndicatorHidden__When__Created()
        {
            var strip = new ShotTrackStrip();

            for (var i = 0; i < strip.TrackArea.childCount; i++)
            {
                if (strip.TrackArea[i].ClassListContains("shot-track__drop-indicator"))
                {
                    Assert.AreEqual(DisplayStyle.None, strip.TrackArea[i].style.display.value,
                        "Drop indicator should be hidden initially");
                    break;
                }
            }
        }

        [Test]
        public void RebuildBlocks__CreatesBlockPerShot__When__ShotsExist()
        {
            this._controller.AddShot(System.Numerics.Vector3.Zero, System.Numerics.Quaternion.Identity);
            this._controller.AddShot(System.Numerics.Vector3.Zero, System.Numerics.Quaternion.Identity);
            this._controller.AddShot(System.Numerics.Vector3.Zero, System.Numerics.Quaternion.Identity);

            var strip = new ShotTrackStrip();
            strip.Bind(this._controller);
            this._uiDocument.rootVisualElement.Add(strip);
            strip.RebuildBlocks();

            var blockCount = 0;

            for (var i = 0; i < strip.TrackArea.childCount; i++)
            {
                if (strip.TrackArea[i] is ShotBlock)
                {
                    blockCount++;
                }
            }

            Assert.AreEqual(3, blockCount, "Should create one ShotBlock per shot");
        }

        [Test]
        public void RebuildBlocks__ClearsPreviousBlocks__When__Rebuilt()
        {
            this._controller.AddShot(System.Numerics.Vector3.Zero, System.Numerics.Quaternion.Identity);

            var strip = new ShotTrackStrip();
            strip.Bind(this._controller);
            this._uiDocument.rootVisualElement.Add(strip);
            strip.RebuildBlocks();

            // Add another shot and rebuild
            this._controller.AddShot(System.Numerics.Vector3.Zero, System.Numerics.Quaternion.Identity);
            strip.RebuildBlocks();

            var blockCount = 0;

            for (var i = 0; i < strip.TrackArea.childCount; i++)
            {
                if (strip.TrackArea[i] is ShotBlock)
                {
                    blockCount++;
                }
            }

            Assert.AreEqual(2, blockCount, "Rebuild should clear old blocks and create new ones");
        }

        [Test]
        public void RebuildBlocks__PreservesInfrastructureElements__When__Rebuilt()
        {
            this._controller.AddShot(System.Numerics.Vector3.Zero, System.Numerics.Quaternion.Identity);

            var strip = new ShotTrackStrip();
            strip.Bind(this._controller);
            this._uiDocument.rootVisualElement.Add(strip);
            strip.RebuildBlocks();

            // Infrastructure elements (drop indicator, playhead, out-of-range) should survive
            var hasDropIndicator = false;
            var hasPlayhead      = false;
            var hasOutOfRange    = false;

            for (var i = 0; i < strip.TrackArea.childCount; i++)
            {
                var child = strip.TrackArea[i];

                if (child.ClassListContains("shot-track__drop-indicator"))
                {
                    hasDropIndicator = true;
                }

                if (child.ClassListContains("timeline-playhead"))
                {
                    hasPlayhead = true;
                }

                if (child.ClassListContains("timeline-out-of-range"))
                {
                    hasOutOfRange = true;
                }
            }

            Assert.IsTrue(hasDropIndicator, "Drop indicator should survive rebuild");
            Assert.IsTrue(hasPlayhead, "Playhead should survive rebuild");
            Assert.IsTrue(hasOutOfRange, "Out-of-range should survive rebuild");
        }

        [Test]
        public void UpdateActiveStates__MarksCurrentShot__When__ShotIsCurrent()
        {
            this._controller.AddShot(System.Numerics.Vector3.Zero, System.Numerics.Quaternion.Identity);
            this._controller.AddShot(System.Numerics.Vector3.Zero, System.Numerics.Quaternion.Identity);

            var strip = new ShotTrackStrip();
            strip.Bind(this._controller);
            this._uiDocument.rootVisualElement.Add(strip);
            strip.RebuildBlocks();

            // Move current to second shot
            this._controller.SetCurrentShot(this._controller.Shots[1].Id);
            strip.UpdateActiveStates();

            for (var i = 0; i < strip.TrackArea.childCount; i++)
            {
                if (strip.TrackArea[i] is ShotBlock block)
                {
                    var isCurrentShot = block.Shot == this._controller.CurrentShot;

                    if (isCurrentShot)
                    {
                        Assert.IsTrue(block.ClassListContains("shot-block--active"),
                            "Current shot block should have active class");
                    }
                    else
                    {
                        Assert.IsFalse(block.ClassListContains("shot-block--active"),
                            "Non-current shot block should not have active class");
                    }
                }
            }
        }

        [Test]
        public void RebuildBlocks__CreatesZeroBlocks__When__NoShots()
        {
            var strip = new ShotTrackStrip();
            strip.Bind(this._controller);
            this._uiDocument.rootVisualElement.Add(strip);
            strip.RebuildBlocks();

            var blockCount = 0;

            for (var i = 0; i < strip.TrackArea.childCount; i++)
            {
                if (strip.TrackArea[i] is ShotBlock)
                {
                    blockCount++;
                }
            }

            Assert.AreEqual(0, blockCount, "Should have no blocks when no shots");
        }

        [Test]
        public void Constructor__HasAddButton__When__Created()
        {
            var strip    = new ShotTrackStrip();
            var labelCol = strip[0]; // first child is label column
            Button addBtn = null;

            foreach (var child in labelCol.Children())
            {
                foreach (var grandchild in child.Children())
                {
                    if (grandchild is Button btn && btn.text == "+")
                    {
                        addBtn = btn;
                        break;
                    }
                }

                if (addBtn != null)
                {
                    break;
                }
            }

            Assert.IsNotNull(addBtn, "Label column should contain a + add button");
        }

        [Test]
        public void SyncVisuals__DoesNotThrow__When__ControllerNotBound()
        {
            var strip = new ShotTrackStrip();

            Assert.DoesNotThrow(() => strip.SyncVisuals(),
                "SyncVisuals should be safe to call without binding");
        }

        [Test]
        public void TrackArea__IsAccessible__When__Created()
        {
            var strip = new ShotTrackStrip();

            Assert.IsNotNull(strip.TrackArea);
            Assert.IsTrue(strip.TrackArea.ClassListContains("timeline-shot-strip"));
        }
    }
}
