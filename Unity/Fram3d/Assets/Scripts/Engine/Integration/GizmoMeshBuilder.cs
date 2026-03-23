using System;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Creates low-poly meshes for gizmo handles at runtime.
    /// All meshes are oriented along +Y by default — the caller rotates
    /// the parent GameObject to align with the desired axis.
    /// </summary>
    public static class GizmoMeshBuilder
    {
        private const int   ARROW_SEGMENTS     = 12;
        private const float CONE_HEIGHT        = 0.15f;
        private const float CONE_RADIUS        = 0.045f;
        private const int   RING_SEGMENTS      = 48;
        private const float RING_THICKNESS     = 0.015f;
        private const float RING_RADIUS        = 0.5f;
        private const float SHAFT_LENGTH       = 0.5f;
        private const float SHAFT_RADIUS       = 0.012f;

        /// <summary>
        /// Creates an arrow mesh: thin cylinder shaft + cone tip, oriented along +Y.
        /// Total length = SHAFT_LENGTH + CONE_HEIGHT.
        /// </summary>
        public static Mesh CreateArrow()
        {
            var shaftVerts = (ARROW_SEGMENTS + 1) * 2;
            var coneVerts  = ARROW_SEGMENTS + 2;
            var totalVerts = shaftVerts + coneVerts;
            var vertices   = new Vector3[totalVerts];
            var triangles  = new int[(ARROW_SEGMENTS * 6) + (ARROW_SEGMENTS * 3)];
            var vi         = 0;
            var ti         = 0;

            // Shaft — open cylinder along Y
            for (var i = 0; i <= ARROW_SEGMENTS; i++)
            {
                var angle = i * Mathf.PI * 2f / ARROW_SEGMENTS;
                var x     = Mathf.Cos(angle) * SHAFT_RADIUS;
                var z     = Mathf.Sin(angle) * SHAFT_RADIUS;
                vertices[vi]     = new Vector3(x, 0f, z);
                vertices[vi + 1] = new Vector3(x, SHAFT_LENGTH, z);
                vi += 2;
            }

            for (var i = 0; i < ARROW_SEGMENTS; i++)
            {
                var b = i * 2;
                triangles[ti++] = b;
                triangles[ti++] = b + 1;
                triangles[ti++] = b + 2;
                triangles[ti++] = b + 2;
                triangles[ti++] = b + 1;
                triangles[ti++] = b + 3;
            }

            // Cone — fan from tip to base circle
            var coneBase  = vi;
            var tipIndex  = vi;
            vertices[vi++] = new Vector3(0f, SHAFT_LENGTH + CONE_HEIGHT, 0f);

            for (var i = 0; i <= ARROW_SEGMENTS; i++)
            {
                var angle = i * Mathf.PI * 2f / ARROW_SEGMENTS;
                var x     = Mathf.Cos(angle) * CONE_RADIUS;
                var z     = Mathf.Sin(angle) * CONE_RADIUS;
                vertices[vi++] = new Vector3(x, SHAFT_LENGTH, z);
            }

            for (var i = 0; i < ARROW_SEGMENTS; i++)
            {
                triangles[ti++] = tipIndex;
                triangles[ti++] = coneBase + 1 + i + 1;
                triangles[ti++] = coneBase + 1 + i;
            }

            var mesh = new Mesh();
            mesh.vertices  = vertices;
            mesh.triangles = new int[ti];
            Array.Copy(triangles, mesh.triangles, ti);
            mesh.RecalculateNormals();
            mesh.name = "GizmoArrow";
            return mesh;
        }

        /// <summary>
        /// Creates a torus mesh around the Y axis for rotation handles.
        /// </summary>
        public static Mesh CreateRing()
        {
            var tubeSegments = 8;
            var totalVerts   = RING_SEGMENTS * tubeSegments;
            var totalTris    = RING_SEGMENTS * tubeSegments * 6;
            var vertices     = new Vector3[totalVerts];
            var triangles    = new int[totalTris];
            var vi           = 0;
            var ti           = 0;

            for (var i = 0; i < RING_SEGMENTS; i++)
            {
                var ringAngle = i * Mathf.PI * 2f / RING_SEGMENTS;
                var center    = new Vector3(Mathf.Cos(ringAngle) * RING_RADIUS, 0f, Mathf.Sin(ringAngle) * RING_RADIUS);
                var radial    = new Vector3(Mathf.Cos(ringAngle), 0f, Mathf.Sin(ringAngle));

                for (var j = 0; j < tubeSegments; j++)
                {
                    var tubeAngle = j * Mathf.PI * 2f / tubeSegments;
                    var offset    = radial * (Mathf.Cos(tubeAngle) * RING_THICKNESS)
                                  + Vector3.up * (Mathf.Sin(tubeAngle) * RING_THICKNESS);

                    vertices[vi++] = center + offset;
                }
            }

            for (var i = 0; i < RING_SEGMENTS; i++)
            {
                var nextI = (i + 1) % RING_SEGMENTS;

                for (var j = 0; j < tubeSegments; j++)
                {
                    var nextJ = (j + 1) % tubeSegments;
                    var a     = i * tubeSegments + j;
                    var b     = i * tubeSegments + nextJ;
                    var c     = nextI * tubeSegments + j;
                    var d     = nextI * tubeSegments + nextJ;
                    triangles[ti++] = a;
                    triangles[ti++] = c;
                    triangles[ti++] = b;
                    triangles[ti++] = b;
                    triangles[ti++] = c;
                    triangles[ti++] = d;
                }
            }

            var mesh = new Mesh();
            mesh.vertices  = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.name = "GizmoRing";
            return mesh;
        }

        /// <summary>
        /// Creates a small cube mesh for the uniform scale handle.
        /// </summary>
        public static Mesh CreateCube(float size = 0.06f)
        {
            var h = size / 2f;

            var vertices = new[]
            {
                // Front
                new Vector3(-h, -h, h), new Vector3(h, -h, h), new Vector3(h, h, h), new Vector3(-h, h, h),
                // Back
                new Vector3(h, -h, -h), new Vector3(-h, -h, -h), new Vector3(-h, h, -h), new Vector3(h, h, -h),
                // Top
                new Vector3(-h, h, h), new Vector3(h, h, h), new Vector3(h, h, -h), new Vector3(-h, h, -h),
                // Bottom
                new Vector3(-h, -h, -h), new Vector3(h, -h, -h), new Vector3(h, -h, h), new Vector3(-h, -h, h),
                // Right
                new Vector3(h, -h, h), new Vector3(h, -h, -h), new Vector3(h, h, -h), new Vector3(h, h, h),
                // Left
                new Vector3(-h, -h, -h), new Vector3(-h, -h, h), new Vector3(-h, h, h), new Vector3(-h, h, -h)
            };

            var triangles = new[]
            {
                0, 2, 1, 0, 3, 2,
                4, 6, 5, 4, 7, 6,
                8, 10, 9, 8, 11, 10,
                12, 14, 13, 12, 15, 14,
                16, 18, 17, 16, 19, 18,
                20, 22, 21, 20, 23, 22
            };

            var mesh = new Mesh();
            mesh.vertices  = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.name = "GizmoCube";
            return mesh;
        }
    }
}
