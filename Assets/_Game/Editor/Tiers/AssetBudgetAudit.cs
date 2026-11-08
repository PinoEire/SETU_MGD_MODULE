using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Menu item <c>MGD Samples > Audit Asset Budgets</c>: checks every texture and
    /// audio clip under <c>Assets/_Game</c> against the Week 9 Lab A Part C import
    /// rules (<see cref="AssetBudgetRules"/>) and prints one <c>[Budget]</c>
    /// warning per broken rule, then a summary line. It reads the Android settings
    /// a release build uses; it changes nothing. Run it before measuring with the
    /// Memory Profiler: an over-budget texture is usually one of these lines.
    /// </summary>
    public static class AssetBudgetAudit
    {
        const string Root = "Assets/_Game";
        const string Android = "Android";

        [MenuItem("MGD Samples/Audit Asset Budgets")]
        public static void Audit()
        {
            int assets = 0;
            int problems = 0;

            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                {
                    continue;
                }

                assets++;
                TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings(Android);
                var facts = new TextureFacts
                {
                    HasAndroidOverride = android.overridden,
                    // Without an override, a build uses the default platform's settings.
                    Format = android.overridden ? android.format : importer.GetAutomaticFormat(Android),
                    MaxSize = android.overridden ? android.maxTextureSize : importer.maxTextureSize,
                    Mipmaps = importer.mipmapEnabled,
                    IsSprite = importer.textureType == TextureImporterType.Sprite,
                    IsNormalMap = importer.textureType == TextureImporterType.NormalMap
                };
                problems += Report(path, AssetBudgetRules.CheckTexture(facts));
            }

            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { Root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not AudioImporter importer)
                {
                    continue;
                }

                assets++;
                AudioImporterSampleSettings android = importer.ContainsSampleSettingsOverride(Android)
                    ? importer.GetOverrideSampleSettings(Android)
                    : importer.defaultSampleSettings;
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                var facts = new AudioFacts
                {
                    LengthSeconds = clip != null ? clip.length : 0f,
                    LoadType = android.loadType,
                    Format = android.compressionFormat,
                    ForceToMono = importer.forceToMono
                };
                problems += Report(path, AssetBudgetRules.CheckAudio(facts));
            }

            string summary = $"[Budget] {problems} problem(s) in {assets} texture(s) and audio clip(s) under {Root}.";
            if (problems > 0)
            {
                Debug.LogWarning(summary);
            }
            else
            {
                Debug.Log(summary);
            }
        }

        static int Report(string path, List<string> found)
        {
            foreach (string problem in found)
            {
                Debug.LogWarning($"[Budget] {path}: {problem}", AssetDatabase.LoadMainAssetAtPath(path));
            }

            return found.Count;
        }
    }
}
