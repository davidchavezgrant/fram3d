using System.Collections.Generic;
using UnityEngine;
namespace Fram3d.Engine.Integration
{
    /// <summary>
    /// Builds the gizmo handle GameObject tree — translate arrows, rotate
    /// rings, and scale diamond — parented under a single root. Runs once
    /// during <see cref="GizmoController"/> Awake.
    /// </summary>
    internal static class GizmoHandleFactory
    {
        private static readonly Color AXIS_X = new(0.9f,
                                                   0.2f,
                                                   0.2f,
                                                   1f);

        private static readonly Color AXIS_Y = new(0.4f,
                                                   0.85f,
                                                   0.2f,
                                                   1f);

        private static readonly Color AXIS_Z = new(0.2f,
                                                   0.5f,
                                                   0.95f,
                                                   1f);

        private static readonly Color SCALE_COLOR = new(0.85f,
                                                        0.85f,
                                                        0.85f,
                                                        1f);

        public static GizmoHandles Build(Transform parent, Material material)
        {
            var axisColors     = new Dictionary<Renderer, Color>();
            var root           = BuildRoot(parent);
            var translateGroup = BuildTranslateHandles(root.transform, material, axisColors);
            var rotateGroup    = BuildRotateHandles(root.transform, material, axisColors);
            var scaleGroup     = BuildScaleHandle(root.transform, material, axisColors);

            return new GizmoHandles(axisColors,
                                    root,
                                    rotateGroup,
                                    scaleGroup,
                                    translateGroup);
        }

        private static GameObject BuildRoot(Transform parent)
        {
            var root = new GameObject("GizmoRoot");
            root.transform.SetParent(parent, false);
            SetLayerRecursive(root, GizmoController.GIZMO_LAYER_INDEX);
            return root;
        }

        private static GameObject BuildRotateHandles(Transform rootTransform, Material material, Dictionary<Renderer, Color> axisColors)
        {
            var group = new GameObject("RotateGroup");
            group.transform.SetParent(rootTransform, false);
            var ringMesh = GizmoMeshBuilder.CreateRing();

            void addHandle(string name, Quaternion rotation, Color color) => CreateHandle(name,
                                                                                          ringMesh,
                                                                                          rotation,
                                                                                          color,
                                                                                          group,
                                                                                          material,
                                                                                          axisColors);

            addHandle("RotateY", Quaternion.identity,            AXIS_Y);
            addHandle("RotateX", Quaternion.Euler(0f,  0f, 90f), AXIS_X);
            addHandle("RotateZ", Quaternion.Euler(90f, 0f, 0f),  AXIS_Z);
            return group;
        }

        private static GameObject BuildScaleHandle(Transform rootTransform, Material material, Dictionary<Renderer, Color> axisColors)
        {
            var group = new GameObject("ScaleGroup");
            group.transform.SetParent(rootTransform, false);
            var diamondMesh = GizmoMeshBuilder.CreateDiamond();

            CreateHandle("ScaleUniform",
                         diamondMesh,
                         Quaternion.identity,
                         SCALE_COLOR,
                         group,
                         material,
                         axisColors);

            return group;
        }

        private static GameObject BuildTranslateHandles(Transform rootTransform, Material material, Dictionary<Renderer, Color> axisColors)
        {
            var group = new GameObject("TranslateGroup");
            group.transform.SetParent(rootTransform, false);
            var arrowMesh = GizmoMeshBuilder.CreateArrow();

            void addHandle(string name, Quaternion rotation, Color color) => CreateHandle(name,
                                                                                          arrowMesh,
                                                                                          rotation,
                                                                                          color,
                                                                                          group,
                                                                                          material,
                                                                                          axisColors);

            addHandle("TranslateY", Quaternion.identity,             AXIS_Y);
            addHandle("TranslateX", Quaternion.Euler(0f,  0f, -90f), AXIS_X);
            addHandle("TranslateZ", Quaternion.Euler(90f, 0f, 0f),   AXIS_Z);
            return group;
        }

        /// <summary>
        /// Adds a collider with hover padding appropriate to the handle type.
        /// Arrows get a CapsuleCollider wider than the visual shaft.
        /// Diamond gets a SphereCollider larger than the visual radius.
        /// Rings keep a convex MeshCollider (convex hull is already a thick disk).
        /// </summary>
        private static void AddCollider(GameObject go, Mesh mesh, string handleName)
        {
            if (handleName.StartsWith("Translate"))
            {
                // Arrow: shaft 0→0.5 + cone 0.5→0.65, visual radius 0.012
                // CapsuleCollider with 4x visual radius for easier grabbing
                var cc      = go.AddComponent<CapsuleCollider>();
                cc.center    = new Vector3(0f, 0.325f, 0f);
                cc.height    = 0.65f;
                cc.radius    = 0.05f;
                cc.direction = 1;
            }
            else if (handleName.StartsWith("Scale"))
            {
                // Diamond: visual radius 0.06, collider ~67% larger
                var sc    = go.AddComponent<SphereCollider>();
                sc.radius = 0.1f;
            }
            else
            {
                // Rings: convex hull of torus is already a thick disk
                var mc        = go.AddComponent<MeshCollider>();
                mc.sharedMesh = mesh;
                mc.convex     = true;
            }
        }

        private static void CreateHandle(string                      handleName,
                                         Mesh                        mesh,
                                         Quaternion                  rotation,
                                         Color                       color,
                                         GameObject                  parent,
                                         Material                    material,
                                         Dictionary<Renderer, Color> axisColors)
        {
            var go = new GameObject(handleName);
            go.transform.SetParent(parent.transform, false);
            go.transform.localRotation = rotation;
            go.layer                   = GizmoController.GIZMO_LAYER_INDEX;
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr  = go.AddComponent<MeshRenderer>();
            var mat = new Material(material);
            mat.SetColor(GizmoHighlighter.SHADER_COLOR, color);
            mr.sharedMaterial = mat;
            AddCollider(go, mesh, handleName);
            axisColors[mr] = color;
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;

            foreach (Transform child in go.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }
    }
}