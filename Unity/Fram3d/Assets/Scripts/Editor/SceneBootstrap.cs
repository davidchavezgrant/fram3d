using Fram3d.Engine.Integration;
using Fram3d.UI.Input;
using Fram3d.UI.Panels;
using Fram3d.UI.Timeline;
using Fram3d.UI.Views;
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
            SetupSelection();
            SetupShotController();
            SetupShotTrack();
            SetupViewLayout();
            SetupAspectRatioMask();
            SetupCompositionGuides();
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
            {
                return;
            }

            var gameObject = GameObject.CreatePrimitive(type);
            gameObject.name               = name;
            gameObject.transform.position = position;
            var renderer = gameObject.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.material.color = color;
            }

            // Make the object a selectable scene element.
            // ElementBehaviour self-initializes its Element in Awake().
            if (gameObject.GetComponent<ElementBehaviour>() == null)
            {
                gameObject.AddComponent<ElementBehaviour>();
            }

            EditorUtility.SetDirty(gameObject);
        }

        private static PanelSettings GetOrCreatePanelSettings()
        {
            var guids = AssetDatabase.FindAssets("t:PanelSettings");

            if (guids.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<PanelSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ConstantPixelSize;
            settings.scale     = 1.25f;
            AssetDatabase.CreateAsset(settings, "Assets/Settings/PanelSettings.asset");
            return settings;
        }

        private static void SetupAspectRatioMask()
        {
            var maskGo = GameObject.Find("Aspect Ratio Mask");

            if (maskGo == null)
            {
                maskGo = new GameObject("Aspect Ratio Mask");
            }

            var uiDoc = maskGo.GetComponent<UIDocument>();

            if (uiDoc == null)
            {
                uiDoc               = maskGo.AddComponent<UIDocument>();
                uiDoc.panelSettings = GetOrCreatePanelSettings();
            }

            uiDoc.sortingOrder = 0;

            if (maskGo.GetComponent<AspectRatioMaskView>() == null)
            {
                maskGo.AddComponent<AspectRatioMaskView>();
            }

            EditorUtility.SetDirty(maskGo);
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
            {
                cameraGameObject.AddComponent<CameraBehaviour>();
            }

            var inputHandler = cameraGameObject.GetComponent<CameraInputHandler>();

            if (inputHandler == null)
            {
                inputHandler = cameraGameObject.AddComponent<CameraInputHandler>();
            }

            // Wire the serialized references
            var serializedObject = new SerializedObject(inputHandler);
            var prop             = serializedObject.FindProperty("cameraBehaviour");
            prop.objectReferenceValue = cameraGameObject.GetComponent<CameraBehaviour>();
            serializedObject.ApplyModifiedProperties();

            // Wire the wireframe shader on CameraBehaviour
            var camBehaviour = cameraGameObject.GetComponent<CameraBehaviour>();
            var camSo        = new SerializedObject(camBehaviour);
            var shaderProp   = camSo.FindProperty("wireframeShader");
            shaderProp.objectReferenceValue = Shader.Find("Unlit/Color");
            camSo.ApplyModifiedProperties();

            EditorUtility.SetDirty(cameraGameObject);
        }

        private static void SetupCompositionGuides()
        {
            var guidesGo = GameObject.Find("Composition Guides");

            if (guidesGo == null)
            {
                guidesGo = new GameObject("Composition Guides");
            }

            var uiDoc = guidesGo.GetComponent<UIDocument>();

            if (uiDoc == null)
            {
                uiDoc               = guidesGo.AddComponent<UIDocument>();
                uiDoc.panelSettings = GetOrCreatePanelSettings();
            }

            uiDoc.sortingOrder = 1;

            if (guidesGo.GetComponent<CompositionGuideView>() == null)
            {
                guidesGo.AddComponent<CompositionGuideView>();
            }

            // Wire composition guides reference on the input handler
            var cameraGo     = GameObject.Find("Main Camera");
            var inputHandler = cameraGo.GetComponent<CameraInputHandler>();

            if (inputHandler != null)
            {
                var so   = new SerializedObject(inputHandler);
                var prop = so.FindProperty("compositionGuides");
                prop.objectReferenceValue = guidesGo.GetComponent<CompositionGuideView>();
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(guidesGo);
        }

        private static void SetupGroundPlane()
        {
            if (GameObject.Find("Ground Plane") != null)
            {
                return;
            }

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

        private static void SetupPropertiesPanel()
        {
            var panelGo = GameObject.Find("Properties Panel");

            if (panelGo == null)
            {
                panelGo = new GameObject("Properties Panel");
            }

            var panelUiDoc = panelGo.GetComponent<UIDocument>();

            if (panelUiDoc == null)
            {
                panelUiDoc               = panelGo.AddComponent<UIDocument>();
                panelUiDoc.panelSettings = GetOrCreatePanelSettings();
            }

            panelUiDoc.sortingOrder = 2;

            if (panelGo.GetComponent<PropertiesPanelView>() == null)
            {
                panelGo.AddComponent<PropertiesPanelView>();
            }

            var cameraGo  = GameObject.Find("Main Camera");
            var panelView = panelGo.GetComponent<PropertiesPanelView>();

            // Wire properties panel reference on the input handler
            var inputHandler = cameraGo.GetComponent<CameraInputHandler>();

            if (inputHandler != null)
            {
                var so   = new SerializedObject(inputHandler);
                var prop = so.FindProperty("propertiesPanel");
                prop.objectReferenceValue = panelView;
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(panelGo);
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

        private static void SetupViewLayout()
        {
            var cameraGo = GameObject.Find("Main Camera");

            // ViewCameraManager on the camera object
            var viewCameraManager = cameraGo.GetComponent<ViewCameraManager>();

            if (viewCameraManager == null)
            {
                viewCameraManager = cameraGo.AddComponent<ViewCameraManager>();
            }

            var vcmSo = new SerializedObject(viewCameraManager);
            vcmSo.FindProperty("cameraBehaviour").objectReferenceValue = cameraGo.GetComponent<CameraBehaviour>();
            vcmSo.ApplyModifiedProperties();

            // Wire viewCameraManager on input handlers
            var cameraInput = cameraGo.GetComponent<CameraInputHandler>();

            if (cameraInput != null)
            {
                var so = new SerializedObject(cameraInput);
                so.FindProperty("viewCameraManager").objectReferenceValue = viewCameraManager;
                so.ApplyModifiedProperties();
            }

            var selectionInput = cameraGo.GetComponent<SelectionInputHandler>();

            if (selectionInput != null)
            {
                var so = new SerializedObject(selectionInput);
                so.FindProperty("viewCameraManager").objectReferenceValue = viewCameraManager;
                so.ApplyModifiedProperties();
            }

            // View Layout UI (layout chooser and viewport headers)
            var layoutGo = GameObject.Find("View Layout");

            if (layoutGo == null)
            {
                layoutGo = new GameObject("View Layout");
            }

            var uiDoc = layoutGo.GetComponent<UIDocument>();

            if (uiDoc == null)
            {
                uiDoc               = layoutGo.AddComponent<UIDocument>();
                uiDoc.panelSettings = GetOrCreatePanelSettings();
            }

            uiDoc.sortingOrder = 3;

            if (layoutGo.GetComponent<ViewLayoutView>() == null)
            {
                layoutGo.AddComponent<ViewLayoutView>();
            }

            EditorUtility.SetDirty(cameraGo);
            EditorUtility.SetDirty(layoutGo);
        }

        private static void SetupShotController()
        {
            var cameraGo = GameObject.Find("Main Camera");

            if (cameraGo == null)
            {
                return;
            }

            if (cameraGo.GetComponent<ShotController>() == null)
            {
                cameraGo.AddComponent<ShotController>();
            }

            EditorUtility.SetDirty(cameraGo);
        }

        private static void SetupShotTrack()
        {
            var shotTrackGo = GameObject.Find("Shot Track");

            if (shotTrackGo == null)
            {
                shotTrackGo = new GameObject("Shot Track");
            }

            var uiDoc = shotTrackGo.GetComponent<UIDocument>();

            if (uiDoc == null)
            {
                uiDoc               = shotTrackGo.AddComponent<UIDocument>();
                uiDoc.panelSettings = GetOrCreatePanelSettings();
            }

            // Timeline renders on top of all views and overlays
            uiDoc.sortingOrder = 10;

            if (shotTrackGo.GetComponent<TimelineSectionView>() == null)
            {
                shotTrackGo.AddComponent<TimelineSectionView>();
            }

            EditorUtility.SetDirty(shotTrackGo);
        }

        private static void SetupSelection()
        {
            var cameraGo = GameObject.Find("Main Camera");

            // SelectionRaycaster on the camera
            var raycaster = cameraGo.GetComponent<SelectionRaycaster>();

            if (raycaster == null)
            {
                raycaster = cameraGo.AddComponent<SelectionRaycaster>();
            }

            var raycasterSo = new SerializedObject(raycaster);
            raycasterSo.FindProperty("targetCamera").objectReferenceValue = cameraGo.GetComponent<Camera>();
            raycasterSo.ApplyModifiedProperties();

            // SelectionHighlighter on the camera
            var highlighter = cameraGo.GetComponent<SelectionHighlighter>();

            if (highlighter == null)
            {
                highlighter = cameraGo.AddComponent<SelectionHighlighter>();
            }

            // GizmoController on the camera
            var gizmoController = cameraGo.GetComponent<GizmoController>();

            if (gizmoController == null)
            {
                gizmoController = cameraGo.AddComponent<GizmoController>();
            }

            var gizmoSo = new SerializedObject(gizmoController);
            gizmoSo.FindProperty("selectionHighlighter").objectReferenceValue = highlighter;
            gizmoSo.FindProperty("targetCamera").objectReferenceValue         = cameraGo.GetComponent<Camera>();
            gizmoSo.ApplyModifiedProperties();

            // SelectionInputHandler on the camera
            var selectionInput = cameraGo.GetComponent<SelectionInputHandler>();

            if (selectionInput == null)
            {
                selectionInput = cameraGo.AddComponent<SelectionInputHandler>();
            }

            var selectionInputSo = new SerializedObject(selectionInput);
            selectionInputSo.FindProperty("selectionHighlighter").objectReferenceValue = highlighter;
            selectionInputSo.FindProperty("raycaster").objectReferenceValue            = raycaster;
            selectionInputSo.FindProperty("gizmoController").objectReferenceValue      = gizmoController;
            selectionInputSo.ApplyModifiedProperties();

            // Wire gizmo controller on camera input handler
            var cameraInput   = cameraGo.GetComponent<CameraInputHandler>();
            var cameraInputSo = new SerializedObject(cameraInput);
            cameraInputSo.FindProperty("gizmoController").objectReferenceValue = gizmoController;
            cameraInputSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(cameraGo);
        }
    }
}