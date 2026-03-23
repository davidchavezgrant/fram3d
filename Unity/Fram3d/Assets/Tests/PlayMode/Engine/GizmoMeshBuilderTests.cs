using System.Collections;
using Fram3d.Engine.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Fram3d.Tests.Engine
{
    /// <summary>
    /// Play Mode tests for GizmoMeshBuilder. Validates that procedurally
    /// generated meshes have valid geometry — catches bugs like the
    /// triangle array copy issue (mesh.triangles returns a copy).
    /// </summary>
    public sealed class GizmoMeshBuilderTests
    {
        [UnityTest]
        public IEnumerator CreateArrow__HasVertices__When__Created()
        {
            yield return null;

            var mesh = GizmoMeshBuilder.CreateArrow();

            Assert.IsNotNull(mesh);
            Assert.Greater(mesh.vertexCount, 0, "Arrow mesh should have vertices");
            Object.DestroyImmediate(mesh);
        }

        [UnityTest]
        public IEnumerator CreateArrow__HasTriangles__When__Created()
        {
            yield return null;

            var mesh = GizmoMeshBuilder.CreateArrow();

            Assert.Greater(mesh.triangles.Length, 0, "Arrow mesh should have triangles");
            Assert.AreEqual(0, mesh.triangles.Length % 3, "Triangle count should be multiple of 3");
            Object.DestroyImmediate(mesh);
        }

        [UnityTest]
        public IEnumerator CreateArrow__TriangleIndicesInRange__When__Created()
        {
            yield return null;

            var mesh = GizmoMeshBuilder.CreateArrow();

            foreach (var index in mesh.triangles)
            {
                Assert.GreaterOrEqual(index, 0, "Triangle index should be >= 0");
                Assert.Less(index, mesh.vertexCount, "Triangle index should be < vertex count");
            }

            Object.DestroyImmediate(mesh);
        }

        [UnityTest]
        public IEnumerator CreateRing__HasVertices__When__Created()
        {
            yield return null;

            var mesh = GizmoMeshBuilder.CreateRing();

            Assert.IsNotNull(mesh);
            Assert.Greater(mesh.vertexCount, 0, "Ring mesh should have vertices");
            Object.DestroyImmediate(mesh);
        }

        [UnityTest]
        public IEnumerator CreateRing__HasTriangles__When__Created()
        {
            yield return null;

            var mesh = GizmoMeshBuilder.CreateRing();

            Assert.Greater(mesh.triangles.Length, 0, "Ring mesh should have triangles");
            Assert.AreEqual(0, mesh.triangles.Length % 3, "Triangle count should be multiple of 3");
            Object.DestroyImmediate(mesh);
        }

        [UnityTest]
        public IEnumerator CreateRing__TriangleIndicesInRange__When__Created()
        {
            yield return null;

            var mesh = GizmoMeshBuilder.CreateRing();

            foreach (var index in mesh.triangles)
            {
                Assert.GreaterOrEqual(index, 0);
                Assert.Less(index, mesh.vertexCount);
            }

            Object.DestroyImmediate(mesh);
        }

        [UnityTest]
        public IEnumerator CreateDiamond__HasVertices__When__Created()
        {
            yield return null;

            var mesh = GizmoMeshBuilder.CreateDiamond();

            Assert.IsNotNull(mesh);
            Assert.AreEqual(6, mesh.vertexCount, "Diamond (octahedron) should have 6 vertices");
            Object.DestroyImmediate(mesh);
        }

        [UnityTest]
        public IEnumerator CreateDiamond__HasTriangles__When__Created()
        {
            yield return null;

            var mesh = GizmoMeshBuilder.CreateDiamond();

            Assert.AreEqual(24, mesh.triangles.Length, "Diamond should have 8 faces × 3 indices = 24");
            Object.DestroyImmediate(mesh);
        }

        [UnityTest]
        public IEnumerator CreateDiamond__TriangleIndicesInRange__When__Created()
        {
            yield return null;

            var mesh = GizmoMeshBuilder.CreateDiamond();

            foreach (var index in mesh.triangles)
            {
                Assert.GreaterOrEqual(index, 0);
                Assert.Less(index, mesh.vertexCount);
            }

            Object.DestroyImmediate(mesh);
        }
    }
}
