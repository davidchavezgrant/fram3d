using System.Collections;
using System.Collections.Generic;
using Fram3d.Core.Scene;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Fram3d.Tests.Engine
{
    /// <summary>
    /// Play Mode tests for ElementDuplicator. Verifies clone creation,
    /// position offset, property copying, selection transfer, and name
    /// incrementing. Each test creates elements via GameObjects with
    /// ElementBehaviour, then calls the static TryDuplicate method.
    /// </summary>
    public sealed class ElementDuplicatorTests
    {
        private List<GameObject>     _extras;
        private Selection            _selection;
        private SelectionHighlighter _highlighter;
        private GameObject           _cameraGo;
        private GameObject           _cube;

        [Test]
        public void TryDuplicate__ReturnsFalse__When__NothingSelected()
        {
            var result = ElementDuplicator.TryDuplicate(this._selection);

            Assert.IsFalse(result);
        }

        [Test]
        public void TryDuplicate__ReturnsFalse__When__SelectionIsNull()
        {
            var result = ElementDuplicator.TryDuplicate(null);

            Assert.IsFalse(result);
        }

        [UnityTest]
        public IEnumerator TryDuplicate__CreatesClone__When__ElementSelected()
        {
            yield return null;

            var source = this._cube.GetComponent<ElementBehaviour>().Element;
            this._selection.Select(source.Id);

            var result = ElementDuplicator.TryDuplicate(this._selection);
            this.TrackDuplicates();
            yield return null;

            Assert.IsTrue(result);
            var behaviours = Object.FindObjectsByType<ElementBehaviour>(FindObjectsSortMode.None);
            Assert.AreEqual(2, behaviours.Length, "Should have original + duplicate");
        }

        [UnityTest]
        public IEnumerator TryDuplicate__AppliesOffset__When__Duplicated()
        {
            yield return null;

            var source = this._cube.GetComponent<ElementBehaviour>().Element;
            this._selection.Select(source.Id);
            var originalPos = source.Position;

            ElementDuplicator.TryDuplicate(this._selection);
            this.TrackDuplicates();
            yield return null;

            var duplicate = this.FindDuplicateBehaviour();
            Assert.IsNotNull(duplicate, "Duplicate should exist");

            // Offset is (1, 0, -1) in System.Numerics coords = (+X, +Z in Unity)
            var dupPos = duplicate.Element.Position;
            Assert.AreEqual(originalPos.X + 1f, dupPos.X, 0.01f, "X should be offset by +1");
            Assert.AreEqual(originalPos.Z - 1f, dupPos.Z, 0.01f, "Z should be offset by -1 (System.Numerics)");
        }

        [UnityTest]
        public IEnumerator TryDuplicate__CopiesScale__When__SourceScaled()
        {
            yield return null;

            var source = this._cube.GetComponent<ElementBehaviour>().Element;
            source.Scale = 2.5f;
            this._selection.Select(source.Id);

            ElementDuplicator.TryDuplicate(this._selection);
            this.TrackDuplicates();
            yield return null;

            var duplicate = this.FindDuplicateBehaviour();
            Assert.AreEqual(2.5f, duplicate.Element.Scale, 0.001f, "Duplicate should inherit source scale");
        }

        [UnityTest]
        public IEnumerator TryDuplicate__CopiesRotation__When__SourceRotated()
        {
            yield return null;

            var source = this._cube.GetComponent<ElementBehaviour>().Element;
            var rot    = System.Numerics.Quaternion.CreateFromAxisAngle(System.Numerics.Vector3.UnitY, 1.0f);
            source.Rotation = rot;

            // Yield so LateUpdate syncs Core rotation → Unity transform.
            // Instantiate clones the transform, and the clone's Awake reads
            // rotation from its transform — if we don't yield, the clone
            // gets the stale identity rotation from the un-synced transform.
            yield return null;

            this._selection.Select(source.Id);
            ElementDuplicator.TryDuplicate(this._selection);
            this.TrackDuplicates();
            yield return null;

            var duplicate = this.FindDuplicateBehaviour();
            Assert.AreNotEqual(System.Numerics.Quaternion.Identity, duplicate.Element.Rotation, "Duplicate should not have identity rotation");
        }

        [UnityTest]
        public IEnumerator TryDuplicate__SelectsDuplicate__When__Created()
        {
            yield return null;

            var source = this._cube.GetComponent<ElementBehaviour>().Element;
            this._selection.Select(source.Id);

            ElementDuplicator.TryDuplicate(this._selection);
            this.TrackDuplicates();
            yield return null;

            var duplicate = this.FindDuplicateBehaviour();
            Assert.AreEqual(duplicate.Element.Id, this._selection.SelectedId, "Duplicate should be selected");
            Assert.AreNotEqual(source.Id, this._selection.SelectedId, "Original should not be selected");
        }

        [UnityTest]
        public IEnumerator TryDuplicate__AssignsIncrementedName__When__Duplicated()
        {
            yield return null;

            var source = this._cube.GetComponent<ElementBehaviour>().Element;
            this._selection.Select(source.Id);

            ElementDuplicator.TryDuplicate(this._selection);
            this.TrackDuplicates();
            yield return null;

            var duplicate = this.FindDuplicateBehaviour();
            Assert.AreEqual("TestCube_1", duplicate.Element.Name, "First duplicate should be named _1");
        }

        [UnityTest]
        public IEnumerator TryDuplicate__IncrementsName__When__DuplicatedSequentially()
        {
            yield return null;

            var source = this._cube.GetComponent<ElementBehaviour>().Element;
            this._selection.Select(source.Id);

            // First duplicate
            ElementDuplicator.TryDuplicate(this._selection);
            this.TrackDuplicates();

            // Second duplicate (selection is now on the first duplicate)
            ElementDuplicator.TryDuplicate(this._selection);
            this.TrackDuplicates();
            yield return null;

            var behaviours = Object.FindObjectsByType<ElementBehaviour>(FindObjectsSortMode.None);
            var names      = new List<string>();

            foreach (var b in behaviours)
            {
                names.Add(b.Element.Name);
            }

            Assert.Contains("TestCube",   names, "Original should exist");
            Assert.Contains("TestCube_1", names, "First duplicate should exist");
            Assert.Contains("TestCube_2", names, "Second duplicate should exist");
        }

        [UnityTest]
        public IEnumerator TryDuplicate__CreatesIndependentElement__When__Duplicated()
        {
            yield return null;

            var source    = this._cube.GetComponent<ElementBehaviour>().Element;
            var sourcePos = source.Position;
            this._selection.Select(source.Id);

            ElementDuplicator.TryDuplicate(this._selection);
            this.TrackDuplicates();
            yield return null;

            // Move the duplicate
            var duplicate = this.FindDuplicateBehaviour();
            duplicate.Element.Position = new System.Numerics.Vector3(99f, 0f, 99f);

            Assert.AreEqual(sourcePos.X, source.Position.X, 0.001f, "Moving duplicate should not affect original");
            Assert.AreEqual(sourcePos.Z, source.Position.Z, 0.001f, "Moving duplicate should not affect original");
        }

        [SetUp]
        public void SetUp()
        {
            this._extras   = new List<GameObject>();
            this._cameraGo = new GameObject("TestCamera");
            this._highlighter = this._cameraGo.AddComponent<SelectionHighlighter>();
            this._selection   = this._highlighter.Selection;
            this._cube                    = GameObject.CreatePrimitive(PrimitiveType.Cube);
            this._cube.name               = "TestCube";
            this._cube.transform.position = new Vector3(0f, 0f, 5f);
            this._cube.AddComponent<ElementBehaviour>();
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

            Object.DestroyImmediate(this._cube);
            Object.DestroyImmediate(this._cameraGo);
        }

        /// <summary>
        /// Finds any ElementBehaviour in the scene that was created by duplication
        /// (not the original _cube). Adds its GameObject to _extras for cleanup.
        /// </summary>
        private ElementBehaviour FindDuplicateBehaviour()
        {
            var behaviours = Object.FindObjectsByType<ElementBehaviour>(FindObjectsSortMode.None);

            foreach (var b in behaviours)
            {
                if (b.gameObject != this._cube)
                {
                    return b;
                }
            }

            return null;
        }

        /// <summary>
        /// Tracks all duplicated GameObjects for TearDown cleanup.
        /// Must be called after each TryDuplicate to prevent leaks.
        /// </summary>
        private void TrackDuplicates()
        {
            var behaviours = Object.FindObjectsByType<ElementBehaviour>(FindObjectsSortMode.None);

            foreach (var b in behaviours)
            {
                if (b.gameObject != this._cube && !this._extras.Contains(b.gameObject))
                {
                    this._extras.Add(b.gameObject);
                }
            }
        }
    }
}
