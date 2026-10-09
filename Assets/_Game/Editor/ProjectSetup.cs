using System.IO;
using System.Linq;
using MathPirates.AR;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.SceneManagement;
using UnityEditor.XR.ARCore;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace MathPirates.Editor
{
    public static class ProjectSetup
    {
        public const string ScenePath = "Assets/_Game/Scenes/Bootstrap.unity";
        private const string Settings = "Assets/_Game/Settings";

        [MenuItem("Math Pirates/Prepare project")]
        public static void Prepare()
        {
            Directory.CreateDirectory(Settings);
            Directory.CreateDirectory("Assets/_Game/Scenes");
            AssetDatabase.Refresh();
            ConfigureAndroid();
            ConfigureRendering();
            ConfigureXR();
            ConfigurePanel();
            LauncherIcons.Configure();
            if (!File.Exists(ScenePath)) CreateScene();
            else
            {
                var scene = EditorSceneManager.OpenScene(ScenePath);
                var arCamera = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Camera>(true)).First(camera => camera.name == "AR Camera");
                ARPresentation.ConfigureCamera(arCamera);
                var preview = GameObject.Find("Preview Camera");
                preview.GetComponent<Camera>().GetUniversalAdditionalCameraData().allowXRRendering = false;
                preview.GetComponent<Camera>().backgroundColor = Color.white;
                var controller = new SerializedObject(GameObject.Find("Game").GetComponent<GameController>());
                controller.FindProperty("waterShader").objectReferenceValue = Shader.Find("MathPirates/Low Poly Water");
                controller.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            if (!BuildProfile.GetAllBuildProfiles().Any())
            {
                var platforms = BuildProfile.GetInstalledPlatformModules().ToArray();
                Debug.Log("Installed build platforms: " + string.Join(", ", platforms.Select(x => x.displayName)));
                var android = platforms.FirstOrDefault(x => x.displayName.Contains("Android") && !x.displayName.Contains("XR"));
                if (string.IsNullOrEmpty(android.displayName))
                {
                    AssetDatabase.SaveAssets();
                    Debug.LogWarning("Run Prepare with -buildTarget Android to create the Android build profiles.");
                    return;
                }
                foreach (string name in new[] { "Android Development", "Android Release" })
                {
                    var profile = BuildProfile.CreateBuildProfile(android.platformGuid, name);
                    profile.overrideGlobalScenes = true;
                    profile.scenes = EditorBuildSettings.scenes;
                    EditorUtility.SetDirty(profile);
                }
            }
            foreach (var profile in BuildProfile.GetAllBuildProfiles())
            {
                if (!profile.name.StartsWith("Android ")) continue;
                var serializedProfile = new SerializedObject(profile);
                var platform = serializedProfile.FindProperty("m_PlatformBuildProfile");
                platform.FindPropertyRelative("m_Development").boolValue = profile.name == "Android Development";
                platform.FindPropertyRelative("m_BuildAppBundle").boolValue = profile.name == "Android Release";
                serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Math Pirates prepared: Unity 6000.6.5f1, Android min 29 / target 37 / compile 37.");
        }

        public static void ConfigureAndroid()
        {
            PlayerSettings.companyName = "CarapacikSpace";
            PlayerSettings.productName = "MathPiratesAR";
            PlayerSettings.bundleVersion = "2.0.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.carapacik.mathpiratesar");
            PlayerSettings.Android.bundleVersionCode = 6;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel37;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Android, ApiCompatibilityLevel.NET_Standard);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Minimal);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            var player = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            player.FindProperty("activeInputHandler").intValue = 1;
            player.ApplyModifiedPropertiesWithoutUndo();
            QualitySettings.vSyncCount = 0;
            QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
            EditorSettings.serializationMode = SerializationMode.ForceText;
        }

        private static T Asset<T>(string name) where T : ScriptableObject
        {
            string path = $"{Settings}/{name}.asset";
            var value = AssetDatabase.LoadAssetAtPath<T>(path);
            if (value != null) return value;
            value = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(value, path);
            return value;
        }

        private static void ConfigureRendering()
        {
            var renderer = Asset<UniversalRendererData>("Mobile Renderer");
            if (!renderer.rendererFeatures.OfType<ARBackgroundRendererFeature>().Any())
            {
                var background = ScriptableObject.CreateInstance<ARBackgroundRendererFeature>();
                background.name = "AR Camera Background";
                AssetDatabase.AddObjectToAsset(background, renderer);
                renderer.rendererFeatures.Add(background);
                EditorUtility.SetDirty(renderer);
            }
            var pipeline = Asset<UniversalRenderPipelineAsset>("Mobile URP");
            var serialized = new SerializedObject(pipeline);
            var renderers = serialized.FindProperty("m_RendererDataList");
            renderers.arraySize = 1;
            renderers.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
            serialized.FindProperty("m_MainLightRenderingMode").intValue = (int)LightRenderingMode.PerPixel;
            serialized.FindProperty("m_AdditionalLightsRenderingMode").intValue = (int)LightRenderingMode.Disabled;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            pipeline.supportsHDR = false;
            pipeline.msaaSampleCount = 2;
            pipeline.renderScale = 1;
            pipeline.supportsCameraDepthTexture = false;
            pipeline.supportsCameraOpaqueTexture = false;
            pipeline.shadowDistance = 0;
            GraphicsSettings.defaultRenderPipeline = pipeline;
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i);
                QualitySettings.renderPipeline = pipeline;
                QualitySettings.vSyncCount = 0;
            }
            EditorUtility.SetDirty(pipeline);
        }

        private static void ConfigureXR()
        {
            var perTarget = Asset<XRGeneralSettingsPerBuildTarget>("XR Targets");
            var general = Asset<XRGeneralSettings>("Android XR");
            var manager = Asset<XRManagerSettings>("Android XR Manager");
            general.Manager = manager;
            general.InitManagerOnStart = false;
            manager.automaticLoading = false;
            manager.automaticRunning = false;
            perTarget.SetSettingsForBuildTarget(BuildTargetGroup.Android, general);
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.settingsKey, perTarget, true);
            if (!XRPackageMetadataStore.AssignLoader(manager, "UnityEngine.XR.ARCore.ARCoreLoader", BuildTargetGroup.Android))
                throw new System.InvalidOperationException("Could not assign ARCore loader.");
            var arcore = Asset<ARCoreSettings>("ARCore");
            arcore.requirement = ARCoreSettings.Requirement.Optional;
            arcore.depth = ARCoreSettings.Requirement.Optional;
            ARCoreSettings.currentSettings = arcore;
            EditorUtility.SetDirty(general); EditorUtility.SetDirty(manager); EditorUtility.SetDirty(perTarget);
        }

        private static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var light = new GameObject("Sun", typeof(Light)).GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.4f;
            light.shadows = LightShadows.None;
            light.transform.rotation = Quaternion.Euler(48, -30, 0);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.65f, .75f, .78f);
            var camera = new GameObject("Preview Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.white;
            camera.nearClipPlane = .05f;
            camera.farClipPlane = 50;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            camera.GetUniversalAdditionalCameraData().allowXRRendering = false;

            var xr = new GameObject("XR Origin");
            xr.SetActive(false);
            var origin = xr.AddComponent<XROrigin>();
            var offset = new GameObject("Camera Offset");
            offset.transform.SetParent(xr.transform, false);
            origin.CameraFloorOffsetObject = offset;
            origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
            origin.CameraYOffset = 0;
            var arCamera = new GameObject("AR Camera", typeof(Camera)).GetComponent<Camera>();
            arCamera.transform.SetParent(offset.transform, false);
            arCamera.nearClipPlane = .01f;
            arCamera.farClipPlane = 30;
            arCamera.clearFlags = CameraClearFlags.SolidColor;
            arCamera.gameObject.AddComponent<ARCameraManager>();
            arCamera.gameObject.AddComponent<ARCameraBackground>();
            ARPresentation.ConfigureCamera(arCamera);
            origin.Camera = arCamera;
            var planes = xr.AddComponent<ARPlaneManager>();
            planes.requestedDetectionMode = PlaneDetectionMode.Horizontal;
            var raycasts = xr.AddComponent<ARRaycastManager>();
            var anchors = xr.AddComponent<ARAnchorManager>();
            var sessionObject = new GameObject("AR Session");
            sessionObject.SetActive(false);
            var session = sessionObject.AddComponent<ARSession>();
            sessionObject.AddComponent<ARInputManager>();
            var placement = new GameObject("Surface Placement").AddComponent<SurfacePlacement>();
            placement.Configure(xr, session, raycasts, planes, anchors, arCamera);

            var panel = ConfigurePanel();
            var document = new GameObject("Game UI").AddComponent<UIDocument>();
            document.panelSettings = panel;
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Game/UI/Game.uxml");
            var game = new GameObject("Game").AddComponent<GameController>();
            game.Configure(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Game/Content/Campaign.json"), document, Shader.Find("Universal Render Pipeline/Simple Lit"), Shader.Find("MathPirates/Low Poly Water"), camera, placement);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static PanelSettings ConfigurePanel()
        {
            var panel = Asset<PanelSettings>("Game Panel");
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(1280, 720);
            panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panel.match = 1;
            panel.scale = 1;
            panel.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Assets/_Game/UI/GameTheme.tss");
            // Persist asset setters; otherwise the player uses the default DPI-based scaling.
            EditorUtility.SetDirty(panel);
            return panel;
        }
    }
}

