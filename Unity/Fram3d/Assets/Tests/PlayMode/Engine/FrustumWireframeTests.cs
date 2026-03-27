using System;
using System.Collections;
using Fram3d.Core.Cameras;
using Fram3d.Core.Common;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Fram3d.Tests.Engine
{
    /// <summary>
    /// Play Mode tests for FrustumWireframe. Verifies that the procedurally
    /// generated wireframe mesh has correct geometry (8 vertices for a frustum,
    /// 24 line indices for 12 edges), that the BoxCollider is sized to contain
    /// the frustum, and that the mesh rebuilds when camera FOV changes.
    /// </summary>
    public sealed class FrustumWireframeTests
    {
        private const float FRUSTUM_FAR  = 3f;
        private const float FRUSTUM_NEAR = 0.3f;

        private CameraElement    _camera;
        private GameObject       _go;
        private Material         _material;
        private FrustumWireframe _wireframe;

        // ── Initialize ─────────────────────────────────────────────────────

        [Test]
        public void Initialize__CreatesMeshFilter__When__Called()
        {
            var filter = this._go.GetComponent<MeshFilter>();

            Assert.IsNotNull(filter, "MeshFilter should be added by Initialize");
            Assert.IsNotNull(filter.mesh, "Mesh should be assigned");
        }

        [Test]
        public void Initialize__CreatesMeshRenderer__When__Called()
        {
            var renderer = this._go.GetComponent<MeshRenderer>();

            Assert.IsNotNull(renderer, "MeshRenderer should be added by Initialize");
            Assert.AreSame(this._material, renderer.sharedMaterial);
        }

        [Test]
        public void Initialize__CreatesBoxCollider__When__Called()
        {
            var collider = this._go.GetComponent<BoxCollider>();

            Assert.IsNotNull(collider, "BoxCollider should be added by Initialize");
        }

        [Test]
        public void Initialize__MeshHas8Vertices__When__Created()
        {
            var mesh = this._go.GetComponent<MeshFilter>().mesh;

            Assert.AreEqual(8, mesh.vertexCount,
                "Frustum wireframe should have 8 vertices (4 near + 4 far)");
        }

        [Test]
        public void Initialize__MeshHas24LineIndices__When__Created()
        {
            var mesh    = this._go.GetComponent<MeshFilter>().mesh;
            var indices = mesh.GetIndices(0);

            Assert.AreEqual(24, indices.Length,
                "Frustum wireframe should have 24 indices (12 edges x 2 per line)");
        }

        [Test]
        public void Initialize__MeshTopologyIsLines__When__Created()
        {
            var mesh = this._go.GetComponent<MeshFilter>().mesh;

            Assert.AreEqual(MeshTopology.Lines, mesh.GetTopology(0));
        }

        [Test]
        public void Initialize__NearVerticesAtNearPlane__When__Created()
        {
            var mesh     = this._go.GetComponent<MeshFilter>().mesh;
            var vertices = mesh.vertices;

            // First 4 vertices are near plane corners (z = FRUSTUM_NEAR)
            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(FRUSTUM_NEAR, vertices[i].z, 0.001f,
                    $"Near vertex {i} Z should be at near plane");
            }
        }

        [Test]
        public void Initialize__FarVerticesAtFarPlane__When__Created()
        {
            var mesh     = this._go.GetComponent<MeshFilter>().mesh;
            var vertices = mesh.vertices;

            // Last 4 vertices are far plane corners (z = FRUSTUM_FAR)
            for (var i = 4; i < 8; i++)
            {
                Assert.AreEqual(FRUSTUM_FAR, vertices[i].z, 0.001f,
                    $"Far vertex {i} Z should be at far plane");
            }
        }

        [Test]
        public void Initialize__ColliderCenteredInFrustum__When__Created()
        {
            var collider   = this._go.GetComponent<BoxCollider>();
            var expectedZ = (FRUSTUM_NEAR + FRUSTUM_FAR) * 0.5f;

            Assert.AreEqual(0f, collider.center.x, 0.001f);
            Assert.AreEqual(0f, collider.center.y, 0.001f);
            Assert.AreEqual(expectedZ, collider.center.z, 0.001f);
        }

        [Test]
        public void Initialize__ColliderDepthMatchesFrustumRange__When__Created()
        {
            var collider  = this._go.GetComponent<BoxCollider>();
            var expectedZ = FRUSTUM_FAR - FRUSTUM_NEAR;

            Assert.AreEqual(expectedZ, collider.size.z, 0.001f,
                "Collider depth should span from near to far plane");
        }

        [Test]
        public void Initialize__FarPlaneWiderThanNear__When__Created()
        {
            var mesh     = this._go.GetComponent<MeshFilter>().mesh;
            var vertices = mesh.vertices;

            // Near plane X range (first 4 vertices)
            var nearMaxX = Mathf.Max(Mathf.Abs(vertices[0].x), Mathf.Abs(vertices[1].x));
            // Far plane X range (last 4 vertices)
            var farMaxX = Mathf.Max(Mathf.Abs(vertices[4].x), Mathf.Abs(vertices[5].x));

            Assert.Greater(farMaxX, nearMaxX,
                "Far plane should be wider than near plane (perspective frustum)");
        }

        // ── LateUpdate ─────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator LateUpdate__UpdatesMesh__When__FocalLengthChanges()
        {
            var mesh          = this._go.GetComponent<MeshFilter>().mesh;
            var verticesBefore = mesh.vertices;
            var farWidthBefore = Mathf.Abs(verticesBefore[4].x);

            // Change focal length to something wider (shorter focal = wider FOV)
            this._camera.FocalLength = 18f;
            yield return null; // LateUpdate triggers RebuildMesh

            var verticesAfter = this._go.GetComponent<MeshFilter>().mesh.vertices;
            var farWidthAfter = Mathf.Abs(verticesAfter[4].x);

            Assert.Greater(farWidthAfter, farWidthBefore,
                "Shorter focal length should produce wider frustum");
        }

        // ── LateUpdate null guard ──────────────────────────────────────────

        [UnityTest]
        public IEnumerator LateUpdate__DoesNotThrow__When__CameraIsNull()
        {
            // Re-create wireframe without initializing (no camera set)
            UnityEngine.Object.DestroyImmediate(this._go);
            this._go       = new GameObject("NullCamFrustum");
            this._wireframe = this._go.AddComponent<FrustumWireframe>();

            // Don't call Initialize — _shotCamera remains null
            yield return null; // LateUpdate runs

            Assert.Pass("No exception when camera is null");
        }

        // ── Setup / TearDown ───────────────────────────────────────────────

        [SetUp]
        public void SetUp()
        {
            this._go       = new GameObject("TestFrustum");
            this._camera   = new CameraElement(new ElementId(Guid.NewGuid()), "TestCam");
            this._material = new Material(Shader.Find("Unlit/Color"));
            this._wireframe = this._go.AddComponent<FrustumWireframe>();
            this._wireframe.Initialize(this._camera, this._material);
        }

        [TearDown]
        public void TearDown()
        {
            if (this._material != null)
            {
                UnityEngine.Object.DestroyImmediate(this._material);
            }

            if (this._go != null)
            {
                UnityEngine.Object.DestroyImmediate(this._go);
            }
        }
    }
}
