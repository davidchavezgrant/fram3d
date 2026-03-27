using UnityEngine;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Creates an infinite ground plane with a visible grid at Y=0.
    /// The plane uses an analytical grid shader — no tiling textures.
    /// A flat BoxCollider enables selection raycasts: clicking the ground
    /// plane hits the collider but finds no ElementBehaviour, so the
    /// existing ElementPicker returns null and triggers deselect.
    /// </summary>
    public sealed class GroundPlane: MonoBehaviour
    {
        private const float COLLIDER_DEPTH  = 0.001f;
        private const float COLLIDER_OFFSET = 0.01f;
        private const float PLANE_SIZE      = 1000f;

        private void Awake()
        {
            this.gameObject.name              = "GroundPlane";
            this.transform.position           = Vector3.zero;
            this.transform.localScale         = Vector3.one;

            this.CreateMesh();
            this.CreateCollider();
            this.CreateMaterial();
        }

        private void CreateCollider()
        {
            var col  = this.gameObject.AddComponent<BoxCollider>();
            col.size   = new Vector3(PLANE_SIZE, COLLIDER_DEPTH, PLANE_SIZE);
            col.center = new Vector3(0f, -(COLLIDER_OFFSET + COLLIDER_DEPTH / 2f), 0f);
        }

        private void CreateMaterial()
        {
            var shader   = Shader.Find("Fram3d/InfiniteGrid");
            var material = new Material(shader);
            this.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private void CreateMesh()
        {
            var filter   = this.gameObject.AddComponent<MeshFilter>();
            var renderer = this.gameObject.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode    = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows       = false;

            var half = PLANE_SIZE / 2f;
            var mesh = new Mesh { name = "GroundPlane" };

            mesh.vertices = new[]
            {
                new Vector3(-half, 0f, -half),
                new Vector3(-half, 0f,  half),
                new Vector3( half, 0f,  half),
                new Vector3( half, 0f, -half)
            };

            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.normals   = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            filter.mesh    = mesh;
        }
    }
}
