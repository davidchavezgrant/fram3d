using Fram3d.Engine.Integration;
using Fram3d.UI.Input;
using Fram3d.UI.Panels;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Fram3d.Editor
{
    /// <summary>
    /// One-time scene setup for development. Sets up the camera with CameraBehaviour
    /// and CameraInputHandler, plus reference geometry for spatial context.
    /// Run via menu: Fram3d > Bootstrap Scene.
    /// </summary>
    public static class SceneBootstrap
    {
        [MenuItem("Fram3d/Bootstrap Scene")]
        public static void Bootstrap()
        {
            SetupCamera();
            SetupPropertiesPanel();
            SetupGroundPlane();
            SetupReferenceObjects();
            Debug.Log("Fram3d scene bootstrapped.");
        }

        private static void CreateReferenceObject(string        name,
                                                  PrimitiveType type,
                                                  Vector3       position,
                                                  Color         color)
        {
            if (GameObject.Find(name) != null)
                return;

            var gameObject = GameObject.CreatePrimitive(type);
            gameObject.name               = name;
            gameObject.transform.position = position;
            var renderer = gameObject.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.material.color = color;
            }

            EditorUtility.SetDirty(gameObject);
        }

        private static void SetupCamera()
        {
            var cameraGameObject = GameObject.Find("Main Camera");

            if (cameraGameObject == null)
            {
                cameraGameObject = new GameObject("Main Camera");
                cameraGameObject.AddComponent<Camera>();
                cameraGameObject.tag = "MainCamera";
            }

            if (cameraGameObject.GetComponent<CameraBehaviour>() == null)
                cameraGameObject.AddComponent<CameraBehaviour>();

            var inputHandler = cameraGameObject.GetComponent<CameraInputHandler>();

            if (inputHandler == null)
                inputHandler = cameraGameObject.AddComponent<CameraInputHandler>();

            // Wire the serialized reference
            var serializedObject = new SerializedObject(inputHandler);
            var prop             = serializedObject.FindProperty("cameraBehaviour");
            prop.objectReferenceValue = cameraGameObject.GetComponent<CameraBehaviour>();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(cameraGameObject);
        }

        private static void SetupPropertiesPanel()
        {
            var panelGo = GameObject.Find("Properties Panel");

            if (panelGo == null)
            {
                panelGo = new GameObject("Properties Panel");
            }

            if (panelGo.GetComponent<UIDocument>() == null)
            {
                var uiDoc = panelGo.AddComponent<UIDocument>();
                uiDoc.panelSettings = GetOrCreatePanelSettings();
            }

            if (panelGo.GetComponent<PropertiesPanelView>() == null)
                panelGo.AddComponent<PropertiesPanelView>();

            // Wire properties panel reference on the input handler
            var cameraGo     = GameObject.Find("Main Camera");
            var inputHandler = cameraGo.GetComponent<CameraInputHandler>();

            if (inputHandler != null)
            {
                var so   = new SerializedObject(inputHandler);
                var prop = so.FindProperty("propertiesPanel");
                prop.objectReferenceValue = panelGo.GetComponent<PropertiesPanelView>();
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(panelGo);
        }

        private static PanelSettings GetOrCreatePanelSettings()
        {
            var guids = AssetDatabase.FindAssets("t:PanelSettings");

            if (guids.Length > 0)
                return AssetDatabase.LoadAssetAtPath<PanelSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));

            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ConstantPixelSize;
            AssetDatabase.CreateAsset(settings, "Assets/Settings/PanelSettings.asset");

            return settings;
        }

        private static void SetupGroundPlane()
        {
            if (GameObject.Find("Ground Plane") != null)
                return;

            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name                 = "Ground Plane";
            plane.transform.position   = Vector3.zero;
            plane.transform.localScale = new Vector3(10f, 1f, 10f);
            var renderer = plane.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.material.color = new Color(0.3f, 0.3f, 0.3f);
            }

            EditorUtility.SetDirty(plane);
        }

        private static void SetupReferenceObjects()
        {
            CreateReferenceObject("Ref Cube A",
                                  PrimitiveType.Cube,
                                  new Vector3(0f, 0.5f, 3f),
                                  Color.red);

            CreateReferenceObject("Ref Cube B",
                                  PrimitiveType.Cube,
                                  new Vector3(3f, 0.5f, 0f),
                                  Color.blue);

            CreateReferenceObject("Ref Sphere",
                                  PrimitiveType.Sphere,
                                  new Vector3(-2f, 1f, 5f),
                                  Color.green);
        }
    }
}