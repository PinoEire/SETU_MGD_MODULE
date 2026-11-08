using System;
using System.Globalization;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Writes today's UTC date and the current commit's short hash into the
    /// <see cref="BuildInfo"/> asset before every player build, so the About
    /// screen of each build says when and from what it was made without anyone
    /// remembering to update it (Week 12 Lab A, Part B step 5).
    ///
    /// The asset is tracked, so every build leaves it modified, a failed one too:
    /// commit it with the release, as the versionCode bump is.
    /// </summary>
    public sealed class BuildInfoStamper : IPreprocessBuildWithReport
    {
        public const string AssetPath = "Assets/_Game/Settings/BuildInfo.asset";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var info = AssetDatabase.LoadAssetAtPath<BuildInfo>(AssetPath);
            if (info == null)
            {
                Debug.LogWarning($"[BuildInfo] No asset at {AssetPath}; the About screen will say it was not built. Run MGD Samples > Build About Scene to create it.");
                return;
            }

            Stamp(info, DateTime.UtcNow, GitInfo.Run("rev-parse --short HEAD"));
            EditorUtility.SetDirty(info);
            AssetDatabase.SaveAssetIfDirty(info);
            Debug.Log($"[BuildInfo] Stamped {info.buildDate} {info.gitHash}");
        }

        /// <summary>Writes the date (yyyy-MM-dd, UTC) and hash into the asset.</summary>
        public static void Stamp(BuildInfo info, DateTime utcNow, string gitHash)
        {
            info.buildDate = utcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            info.gitHash = gitHash;
        }
    }
}
