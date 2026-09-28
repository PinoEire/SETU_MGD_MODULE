using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Removes <c>android.permission.INTERNET</c> from release builds. Unity 6
    /// adds it to every Android build, even with Internet Access on Auto and
    /// every Unity service off, because the engine's analytics common module is
    /// never stripped; this app never opens a connection and its data-use
    /// statement says so, so the build must agree. Development builds keep the
    /// permission: the Profiler's Wi-Fi autoconnect and the editor's log
    /// connection are sockets that need it.
    ///
    /// Runs after Unity writes the Gradle project and before Gradle merges the
    /// manifests, which is the last moment the permission is one line in one
    /// file. Removing it here needs no custom manifest in Assets/Plugins.
    /// </summary>
    public sealed class StripInternetPermission : IPreprocessBuildWithReport, IPostGenerateGradleAndroidProject
    {
        static readonly Regex InternetPermissionLine = new Regex(
            @"[ \t]*<uses-permission\s+android:name=""android\.permission\.INTERNET""\s*/>[ \t]*(\r?\n)?");

        static bool _developmentBuild;

        public int callbackOrder => 0;

        // The Gradle callback gets no report, so remember the build kind here.
        public void OnPreprocessBuild(BuildReport report)
        {
            _developmentBuild = (report.summary.options & BuildOptions.Development) != 0;
        }

        /// <summary>The manifest text without the INTERNET line. Pure, so it is tested.</summary>
        public static string Strip(string manifestXml)
        {
            return InternetPermissionLine.Replace(manifestXml, "");
        }

        public void OnPostGenerateGradleAndroidProject(string unityLibraryPath)
        {
            if (_developmentBuild)
            {
                Debug.Log("[StripInternetPermission] Development build: INTERNET kept for the Profiler connection.");
                return;
            }

            string manifestPath = Path.Combine(unityLibraryPath, "src", "main", "AndroidManifest.xml");
            if (!File.Exists(manifestPath))
            {
                Debug.LogWarning($"[StripInternetPermission] No manifest at {manifestPath}; nothing removed.");
                return;
            }

            string original = File.ReadAllText(manifestPath);
            string stripped = Strip(original);
            if (stripped == original)
            {
                Debug.Log("[StripInternetPermission] INTERNET was not requested; nothing to remove.");
                return;
            }

            File.WriteAllText(manifestPath, stripped);
            Debug.Log("[StripInternetPermission] Removed android.permission.INTERNET from the release build.");
        }
    }
}
