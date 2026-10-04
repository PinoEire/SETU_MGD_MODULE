using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Menu item <c>MGD Samples > Build Release APK</c>: the Week 4 release
    /// pipeline as one button. It runs the <see cref="ReleaseChecklist"/> and
    /// refuses to build until every line passes, bumps versionCode, builds with
    /// the Android build profile, and records the result in the manifest.
    /// The keystore passwords are the ones typed into Player Settings this
    /// session, so this runs from the editor only, not from batch mode.
    /// Commit before building: the manifest row records the current commit, and
    /// a tree with uncommitted changes is recorded as "-dirty".
    /// </summary>
    public static class ReleaseBuild
    {
        const string ProfilePath = "Assets/Settings/Build Profiles/Android™.asset";
        const string ReleasesFolder = "releases";
        const string ManifestPath = "releases/manifest.md";

        [MenuItem("MGD Samples/Build Release APK")]
        public static void Build()
        {
            string[] problems = ReleaseChecklist.Check(ReleaseSettings.FromPlayerSettings());
            if (problems.Length > 0)
            {
                foreach (string problem in problems)
                {
                    Debug.LogError($"[ReleaseBuild] {problem}");
                }

                Debug.LogError("[ReleaseBuild] Not built. Fix the items above; each line names its settings page.");
                return;
            }

            var profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(ProfilePath);
            if (profile == null)
            {
                Debug.LogError($"[ReleaseBuild] Build profile not found at {ProfilePath}.");
                return;
            }

            // Read the commit before anything is written: bumping versionCode
            // changes ProjectSettings.asset, which would mark every build dirty.
            string commit = GitDescribe();

            // Android refuses to install a lower versionCode over a higher one, so
            // every build gets a new code. Bumped before the build because the APK
            // carries it; put back if the build fails.
            int previousCode = PlayerSettings.Android.bundleVersionCode;
            int code = previousCode + 1;
            PlayerSettings.Android.bundleVersionCode = code;

            string version = PlayerSettings.bundleVersion;
            string apkPath = $"{ReleasesFolder}/{ApkFileName(PlayerSettings.productName, version, code)}";
            Directory.CreateDirectory(ReleasesFolder);

            BuildReport report;
            try
            {
                report = BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions
                {
                    buildProfile = profile,
                    locationPathName = apkPath,
                    options = BuildOptions.None
                });
            }
            catch (Exception)
            {
                // BuildPlayer throws (not just reports) when the editor is still
                // compiling or importing; the bump must not survive that either.
                PlayerSettings.Android.bundleVersionCode = previousCode;
                throw;
            }

            if (report.summary.result != BuildResult.Succeeded || !File.Exists(apkPath))
            {
                PlayerSettings.Android.bundleVersionCode = previousCode;
                Debug.LogError($"[ReleaseBuild] Build {report.summary.result}, APK present: {File.Exists(apkPath)}. versionCode stays {previousCode}. See the errors above.");
                return;
            }

            AssetDatabase.SaveAssets(); // persists the bumped versionCode

            var apk = new FileInfo(apkPath);
            string row = ReleaseManifest.Row(version, code, DateTime.Now.ToString("yyyy-MM-dd"), commit, apk.Length, Sha256(apkPath));
            ReleaseManifest.Append(ManifestPath, row);

            Debug.Log($"[ReleaseBuild] Built {apkPath} ({apk.Length / 1048576.0:F1} MB), versionCode {code}, commit {commit}. " +
                      $"Row added to {ManifestPath}. Install with: adb install -r {apkPath}. " +
                      "Attach the APK to the GitHub Release and paste the link in the Download column.");
        }

        /// <summary>
        /// Version and versionCode both in the name, so two builds of the same
        /// version never overwrite each other and every manifest row keeps a file.
        /// </summary>
        public static string ApkFileName(string productName, string version, int versionCode)
        {
            return $"{productName}-{version}-{versionCode}-arm64.apk";
        }

        static string Sha256(string path)
        {
            using FileStream stream = File.OpenRead(path);
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }

        // "a121f41" when the tree is clean, "a121f41-dirty" when the build
        // includes uncommitted changes: commit first if the row is to be evidence.
        static string GitDescribe()
        {
            try
            {
                var git = new ProcessStartInfo("git", "describe --always --dirty")
                {
                    WorkingDirectory = Path.GetDirectoryName(UnityEngine.Application.dataPath),
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using Process process = Process.Start(git);
                string hash = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                return string.IsNullOrEmpty(hash) ? "unknown" : hash;
            }
            catch (Exception)
            {
                return "unknown";
            }
        }
    }
}
