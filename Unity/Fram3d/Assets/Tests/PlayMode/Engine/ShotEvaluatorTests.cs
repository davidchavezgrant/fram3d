using System.Collections;
using System.Collections.Generic;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Fram3d.Tests.Engine
{
    /// <summary>
    /// Play Mode tests for ShotEvaluator. Verifies Timeline creation,
    /// subscription lifecycle, default shot seeding from the camera,
    /// and bottom inset pixel tracking.
    /// </summary>
    public sealed class ShotEvaluatorTests
    {
        private CameraBehaviour  _cameraBehaviour;
        private GameObject       _cameraGo;
        private ShotEvaluator    _evaluator;
        private GameObject       _evaluatorGo;
        private List<GameObject> _extras;

        // ── Awake ──────────────────────────────────────────────────────────

        [Test]
        public void Awake__CreatesTimeline__When__Created()
        {
            Assert.IsNotNull(this._evaluator.Controller);
        }

        [Test]
        public void Awake__TimelineHasNoShots__When__BeforeStart()
        {
            Assert.AreEqual(0, this._evaluator.Controller.Shots.Count,
                "Timeline should have no shots before Start adds one");
        }

        // ── SetBottomInset ─────────────────────────────────────────────────

        [Test]
        public void BottomInsetPixels__ReturnsZero__When__NeverSet()
        {
            Assert.AreEqual(0f, this._evaluator.BottomInsetPixels, 0.001f);
        }

        [Test]
        public void SetBottomInset__StoresValue__When__Called()
        {
            this._evaluator.SetBottomInset(120f);

            Assert.AreEqual(120f, this._evaluator.BottomInsetPixels, 0.001f);
        }

        [Test]
        public void SetBottomInset__OverwritesPreviousValue__When__CalledTwice()
        {
            this._evaluator.SetBottomInset(100f);
            this._evaluator.SetBottomInset(200f);

            Assert.AreEqual(200f, this._evaluator.BottomInsetPixels, 0.001f);
        }

        // ── Start with CameraBehaviour ─────────────────────────────────────

        [UnityTest]
        public IEnumerator Start__AddsDefaultShot__When__CameraBehaviourExists()
        {
            yield return null; // Start runs

            Assert.AreEqual(1, this._evaluator.Controller.Shots.Count,
                "Start should add one default shot from the camera position");
        }

        [UnityTest]
        public IEnumerator Start__SetsCurrentShot__When__CameraBehaviourExists()
        {
            yield return null;

            Assert.IsNotNull(this._evaluator.Controller.CurrentShot,
                "Current shot should be set after Start");
        }

        // ── Start without CameraBehaviour ──────────────────────────────────

        [UnityTest]
        public IEnumerator Start__LogsWarning__When__NoCameraBehaviourExists()
        {
            // Destroy everything from SetUp — we need a clean scene with no CameraBehaviour
            Object.DestroyImmediate(this._evaluatorGo);
            this._evaluatorGo = null;
            this._evaluator   = null;

            var frustums = Object.FindObjectsByType<FrustumWireframe>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var f in frustums)
            {
                Object.DestroyImmediate(f.gameObject);
            }

            Object.DestroyImmediate(this._cameraGo);
            this._cameraGo       = null;
            this._cameraBehaviour = null;

            var isolatedGo = new GameObject("IsolatedEvaluator");
            this._extras.Add(isolatedGo);
            var isolatedEval = isolatedGo.AddComponent<ShotEvaluator>();

            LogAssert.Expect(LogType.Warning, "ShotEvaluator: No CameraBehaviour found.");
            yield return null;

            Assert.AreEqual(0, isolatedEval.Controller.Shots.Count,
                "Should not add a shot when no camera is found");
        }

        // ── OnDestroy ──────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator OnDestroy__DisposesCleanly__When__DestroyedAfterStart()
        {
            yield return null; // Let Start subscribe

            // Destroying should not throw
            Object.DestroyImmediate(this._evaluatorGo);
            this._evaluatorGo = null;
            this._evaluator   = null;
            yield return null;

            // If subscriptions weren't disposed, timeline events would
            // invoke callbacks on a destroyed object and throw
            Assert.Pass("No exception on destroy");
        }

        // ── Setup / TearDown ───────────────────────────────────────────────

        [SetUp]
        public void SetUp()
        {
            this._extras      = new List<GameObject>();
            this._cameraGo    = new GameObject("TestCamera");
            this._cameraBehaviour = this._cameraGo.AddComponent<CameraBehaviour>();
            this._evaluatorGo = new GameObject("TestEvaluator");
            this._evaluator   = this._evaluatorGo.AddComponent<ShotEvaluator>();
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

            if (this._evaluatorGo != null)
            {
                Object.DestroyImmediate(this._evaluatorGo);
            }

            var frustums = Object.FindObjectsByType<FrustumWireframe>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var f in frustums)
            {
                Object.DestroyImmediate(f.gameObject);
            }

            if (this._cameraGo != null)
            {
                Object.DestroyImmediate(this._cameraGo);
            }
        }
    }
}
