using System;
using Fram3d.Core.Cameras;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Renders a wireframe frustum showing the shot camera's position,
    /// orientation, and field of view. Visible only in Director View.
    /// The parent GameObject has an ElementBehaviour wrapping the shot
    /// CameraElement, so the frustum is selectable and gizmo-draggable.
    /// </summary>
    public sealed class FrustumWireframe: MonoBehaviour
    {
        private const float FRUSTUM_FAR  = 3f;
        private const float FRUSTUM_NEAR = 0.3f;

        private static readonly Color WIREFRAME_COLOR = new(0.9f, 0.9f, 0.9f, 1f);

        private BoxCollider   _collider;
        private CameraElement _shotCamera;
        private MeshFilter    _meshFilter;
        private MeshRenderer  _meshRenderer;

        public void Initialize(CameraElement shotCamera, Material material)
        {
            this._shotCamera = shotCamera;

            this._meshFilter   = this.gameObject.AddComponent<MeshFilter>();
            this._meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            this._collider     = this.gameObject.AddComponent<BoxCollider>();

            this._meshRenderer.sharedMaterial = material;
            this.RebuildMesh();
        }

        private void LateUpdate()
        {
            if (this._shotCamera == null)
            {
                return;
            }

            this.RebuildMesh();
        }

        private void RebuildMesh()
        {
            var vFov   = this._shotCamera.VerticalFov;
            var aspect = this._shotCamera.SensorWidth / this._shotCamera.SensorHeight;

            var nearHalfH = MathF.Tan(vFov * 0.5f) * FRUSTUM_NEAR;
            var nearHalfW = nearHalfH * aspect;
            var farHalfH  = MathF.Tan(vFov * 0.5f) * FRUSTUM_FAR;
            var farHalfW  = farHalfH * aspect;

            // Frustum corners in local space (camera looks along +Z in local)
            // Near plane
            var n0 = new Vector3(-nearHalfW,  nearHalfH, FRUSTUM_NEAR);
            var n1 = new Vector3( nearHalfW,  nearHalfH, FRUSTUM_NEAR);
            var n2 = new Vector3( nearHalfW, -nearHalfH, FRUSTUM_NEAR);
            var n3 = new Vector3(-nearHalfW, -nearHalfH, FRUSTUM_NEAR);

            // Far plane
            var f0 = new Vector3(-farHalfW,  farHalfH, FRUSTUM_FAR);
            var f1 = new Vector3( farHalfW,  farHalfH, FRUSTUM_FAR);
            var f2 = new Vector3( farHalfW, -farHalfH, FRUSTUM_FAR);
            var f3 = new Vector3(-farHalfW, -farHalfH, FRUSTUM_FAR);

            var vertices = new[] { n0, n1, n2, n3, f0, f1, f2, f3 };

            // 12 edges: 4 near + 4 far + 4 connecting
            var indices = new[]
            {
                0, 1,  1, 2,  2, 3,  3, 0,   // near plane
                4, 5,  5, 6,  6, 7,  7, 4,   // far plane
                0, 4,  1, 5,  2, 6,  3, 7    // connecting edges
            };

            var mesh = this._meshFilter.mesh;

            if (mesh == null)
            {
                mesh = new Mesh { name = "FrustumWireframe" };
                this._meshFilter.mesh = mesh;
            }

            mesh.Clear();
            mesh.vertices  = vertices;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);

            // Size the box collider to cover the frustum volume
            this._collider.center = new Vector3(0f, 0f, (FRUSTUM_NEAR + FRUSTUM_FAR) * 0.5f);
            this._collider.size   = new Vector3(farHalfW * 2f, farHalfH * 2f, FRUSTUM_FAR - FRUSTUM_NEAR);
        }
    }
}
