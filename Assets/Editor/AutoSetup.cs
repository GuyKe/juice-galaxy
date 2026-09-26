using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;
using UnityEngine.XR.OpenXR.Features.OculusQuestSupport;

public static class AutoSetup
{
    public static void Run()
    {
        Debug.Log("[AutoSetup] Ensuring required runtime-created shaders survive build stripping...");
        var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("ProjectSettings/GraphicsSettings.asset");
        var gso = new SerializedObject(graphicsSettingsObj);
        var alwaysIncluded = gso.FindProperty("m_AlwaysIncludedShaders");
        var tooExpensiveShader = Shader.Find("Universal Render Pipeline/Lit");
        for (int i = alwaysIncluded.arraySize - 1; i >= 0; i--)
        {
            if (alwaysIncluded.GetArrayElementAtIndex(i).objectReferenceValue == tooExpensiveShader)
            {
                alwaysIncluded.DeleteArrayElementAtIndex(i);
                Debug.Log("[AutoSetup] Removed Universal Render Pipeline/Lit from always-included shaders (too many variants)");
            }
        }
        string[] requiredShaders = {
            "Universal Render Pipeline/Unlit",
            "Unlit/Color",
            "Standard",
        };
        foreach (var shaderName in requiredShaders)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null) { Debug.LogWarning($"[AutoSetup] Could not find shader: {shaderName}"); continue; }
            bool already = false;
            for (int i = 0; i < alwaysIncluded.arraySize; i++)
            {
                if (alwaysIncluded.GetArrayElementAtIndex(i).objectReferenceValue == shader) { already = true; break; }
            }
            if (!already)
            {
                alwaysIncluded.InsertArrayElementAtIndex(alwaysIncluded.arraySize);
                alwaysIncluded.GetArrayElementAtIndex(alwaysIncluded.arraySize - 1).objectReferenceValue = shader;
                Debug.Log($"[AutoSetup] Added always-included shader: {shaderName}");
            }
        }
        gso.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();

        Debug.Log("[AutoSetup] Creating real Lit material assets so URP keeps their exact variant instead of stripping or exploding into every variant...");
        System.IO.Directory.CreateDirectory("Assets/RuntimeShaderKeepAlive");
        var litShaderForAsset = Shader.Find("Universal Render Pipeline/Lit");
        var preloaded = new System.Collections.Generic.List<UnityEngine.Object>(PlayerSettings.GetPreloadedAssets());
        void EnsureKeepAliveMaterial(string path, bool withTexture)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(litShaderForAsset);
                if (withTexture) mat.SetTexture("_BaseMap", Texture2D.whiteTexture);
                AssetDatabase.CreateAsset(mat, path);
            }
            if (!preloaded.Contains(mat)) preloaded.Add(mat);
        }
        EnsureKeepAliveMaterial("Assets/RuntimeShaderKeepAlive/LitKeepAlive_NoTexture.mat", false);
        EnsureKeepAliveMaterial("Assets/RuntimeShaderKeepAlive/LitKeepAlive_WithTexture.mat", true);
        PlayerSettings.SetPreloadedAssets(preloaded.ToArray());
        AssetDatabase.SaveAssets();

        Debug.Log("[AutoSetup] Switching active build target to Android...");
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        Debug.Log("[AutoSetup] Configuring Player Settings for Quest...");
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.guyke.juicegalaxy");
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.colorSpace = ColorSpace.Linear;

        Debug.Log("[AutoSetup] Enabling OpenXR loader for Android via XR Plug-in Management...");
        XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
        foreach (var guid in AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget"))
        {
            buildTargetSettings = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(AssetDatabase.GUIDToAssetPath(guid));
            if (buildTargetSettings != null) break;
        }
        if (buildTargetSettings == null)
        {
            buildTargetSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            System.IO.Directory.CreateDirectory("Assets/XR/Settings");
            AssetDatabase.CreateAsset(buildTargetSettings, "Assets/XR/Settings/XRGeneralSettingsPerBuildTarget.asset");
            AssetDatabase.SaveAssets();
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, buildTargetSettings, true);
        }

        if (!buildTargetSettings.HasSettingsForBuildTarget(BuildTargetGroup.Android))
            buildTargetSettings.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);

        if (!buildTargetSettings.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))
            buildTargetSettings.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);

        var androidXrSettings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
        bool assigned = XRPackageMetadataStore.AssignLoader(androidXrSettings.Manager, "UnityEngine.XR.OpenXR.OpenXRLoader", BuildTargetGroup.Android);
        Debug.Log($"[AutoSetup] OpenXR loader assigned for Android: {assigned}");
        androidXrSettings.InitManagerOnStart = true;
        EditorUtility.SetDirty(androidXrSettings.Manager);
        EditorUtility.SetDirty(androidXrSettings);
        EditorUtility.SetDirty(buildTargetSettings);

        Debug.Log("[AutoSetup] Enabling Meta Quest Support + Oculus Touch Controller Profile OpenXR features...");
        var androidOpenXrSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
        if (androidOpenXrSettings == null)
        {
            Debug.LogError("[AutoSetup] No OpenXR settings found for Android build target group.");
        }
        else
        {
            var metaQuestFeature = androidOpenXrSettings.GetFeature<MetaQuestFeature>();
            if (metaQuestFeature != null) { metaQuestFeature.enabled = true; Debug.Log("[AutoSetup] Enabled MetaQuestFeature"); }
            else Debug.LogError("[AutoSetup] MetaQuestFeature not found");

            var oculusQuestFeature = androidOpenXrSettings.GetFeature<OculusQuestFeature>();
            if (oculusQuestFeature != null) { oculusQuestFeature.enabled = false; Debug.Log("[AutoSetup] Left OculusQuestFeature disabled (deprecated, conflicts with MetaQuestFeature's runtime loader)"); }

            var touchProfile = androidOpenXrSettings.GetFeature<OculusTouchControllerProfile>();
            if (touchProfile != null) { touchProfile.enabled = true; Debug.Log("[AutoSetup] Enabled OculusTouchControllerProfile"); }
            else Debug.LogError("[AutoSetup] OculusTouchControllerProfile not found");

            EditorUtility.SetDirty(androidOpenXrSettings);
        }

        Debug.Log("[AutoSetup] Ensuring a URP asset is assigned...");
        if (GraphicsSettings.currentRenderPipeline == null)
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>("Assets");
            EditorApplication.ExecuteMenuItem("Assets/Create/Rendering/URP Asset (with Universal Renderer)");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var urpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/New Universal Render Pipeline Asset.asset");
            if (urpAsset == null)
            {
                foreach (var guid in AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset"))
                {
                    urpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(guid));
                    if (urpAsset != null) break;
                }
            }

            if (urpAsset != null)
            {
                GraphicsSettings.defaultRenderPipeline = urpAsset;
                QualitySettings.renderPipeline = urpAsset;
                Debug.Log($"[AutoSetup] Assigned URP asset: {AssetDatabase.GetAssetPath(urpAsset)}");
            }
            else
            {
                Debug.LogError("[AutoSetup] Failed to create/locate a URP asset.");
            }
        }
        else
        {
            Debug.Log("[AutoSetup] URP asset already assigned, skipping.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[AutoSetup] DONE");
    }

    public static void BuildApk()
    {
        Debug.Log("[AutoSetup] Starting Android build...");
        var scenes = EditorBuildSettings.scenes;
        var buildPath = "Builds/Android/JuiceGalaxy.apk";
        System.IO.Directory.CreateDirectory("Builds/Android");

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = System.Array.ConvertAll(scenes, s => s.path),
            locationPathName = buildPath,
            target = BuildTarget.Android,
            options = BuildOptions.None,
        });

        Debug.Log($"[AutoSetup] Build result: {report.summary.result}, total errors: {report.summary.totalErrors}, size: {report.summary.totalSize} bytes, output: {buildPath}");
    }
}
