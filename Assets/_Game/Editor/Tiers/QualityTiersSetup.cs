using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Menu item <c>MGD Samples > Set Up Quality Tiers</c>: the Week 9 Lab A Part A
    /// set-up as a script, so it can be repeated and read. It replaces the
    /// template's quality levels with Low, Mid and High (in that order, the order
    /// <see cref="DeviceTier"/> relies on), gives each its own URP asset copied
    /// from the template's, and makes Mid the Android default. Running it again
    /// updates the same assets and levels.
    ///
    /// Per tier: render scale 0.75 / 0.9 / 1.0 and MSAA off / 2x / 4x on the URP
    /// asset; global mipmap limit half / full / full, texture streaming on, and
    /// anisotropic filtering off / per texture / forced on in the quality level.
    /// The lab table's Main Light Shadows row is a Universal (3D) Renderer setting
    /// and does nothing under the 2D Renderer this project uses. Post-processing
    /// and 2D shadows (Light2D with ShadowCaster2D) do exist in 2D, but no sample
    /// uses them, so the levels leave them out; a 2D game that uses them tiers
    /// the camera's post-processing and the lights' shadows instead.
    /// </summary>
    public static class QualityTiersSetup
    {
        const string TemplateAsset = "Assets/Settings/UniversalRP.asset";
        const string Folder = "Assets/_Game/Settings/Quality";
        const string QualitySettingsPath = "ProjectSettings/QualitySettings.asset";

        static readonly string[] Names = DeviceTier.LevelNames.ToArray();
        static readonly int[] Msaa = { 1, 2, 4 };
        static readonly int[] MipmapLimit = { 1, 0, 0 };

        // AnisotropicFiltering: Disable, Enable (per texture), ForceEnable.
        static readonly int[] Anisotropic = { 0, 1, 2 };

        [MenuItem("MGD Samples/Set Up Quality Tiers")]
        public static void SetUp()
        {
            Directory.CreateDirectory(Folder);
            var assets = new UniversalRenderPipelineAsset[Names.Length];
            for (int i = 0; i < Names.Length; i++)
            {
                assets[i] = CreateOrUpdateAsset((Tier)i);
                if (assets[i] == null)
                {
                    return;
                }
            }

            Object qualitySettings = AssetDatabase.LoadAllAssetsAtPath(QualitySettingsPath)[0];
            var settings = new SerializedObject(qualitySettings);
            SerializedProperty levels = settings.FindProperty("m_QualitySettings");
            levels.arraySize = Names.Length;
            for (int i = 0; i < Names.Length; i++)
            {
                SerializedProperty level = levels.GetArrayElementAtIndex(i);
                level.FindPropertyRelative("name").stringValue = Names[i];
                level.FindPropertyRelative("customRenderPipeline").objectReferenceValue = assets[i];
                level.FindPropertyRelative("globalTextureMipmapLimit").intValue = MipmapLimit[i];
                level.FindPropertyRelative("anisotropicTextures").intValue = Anisotropic[i];
                level.FindPropertyRelative("streamingMipmapsActive").boolValue = true;
                // URP keeps the built-in anti-aliasing field in step with the
                // asset's MSAA whenever the level is active (1 sample is written
                // as 0); set it the same way so the file does not change after a
                // Play session. Frame pacing is DeviceTier's targetFrameRate.
                level.FindPropertyRelative("antiAliasing").intValue = Msaa[i] > 1 ? Msaa[i] : 0;
                level.FindPropertyRelative("vSyncCount").intValue = 0;
                level.FindPropertyRelative("excludedTargetPlatforms").arraySize = 0;
            }

            // Every platform's default must point at a level that still exists;
            // Android starts on Mid, as the lab sheet asks.
            SerializedProperty defaults = settings.FindProperty("m_PerPlatformDefaultQuality");
            for (int i = 0; i < defaults.arraySize; i++)
            {
                SerializedProperty entry = defaults.GetArrayElementAtIndex(i);
                SerializedProperty value = entry.FindPropertyRelative("second");
                value.intValue = entry.FindPropertyRelative("first").stringValue == "Android"
                    ? (int)Tier.Mid
                    : Mathf.Min(value.intValue, (int)Tier.High);
            }

            settings.FindProperty("m_CurrentQuality").intValue = (int)Tier.High;
            settings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log($"[Tier] Quality levels are now {string.Join(", ", QualitySettings.names)}; Android default Mid; URP assets in {Folder}.");
        }

        static UniversalRenderPipelineAsset CreateOrUpdateAsset(Tier tier)
        {
            string path = $"{Folder}/URP-{Names[(int)tier]}.asset";
            if (AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path) == null &&
                !AssetDatabase.CopyAsset(TemplateAsset, path))
            {
                Debug.LogError($"[Tier] Could not copy {TemplateAsset} to {path}.");
                return null;
            }

            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            asset.renderScale = DeviceTier.RenderScaleFor(tier);
            asset.msaaSampleCount = Msaa[(int)tier];
            EditorUtility.SetDirty(asset);
            return asset;
        }
    }
}
