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
    /// connection are sockets that need it. So does a project that asks for the
    /// permission on purpose (Internet Access set to Require).
    ///
    /// Runs after Unity writes the Gradle project and before Gradle merges the
    /// manifests, which is the last moment the permission is one line in one
    /// file. If the permission is still there after the strip (a Unity upgrade
    /// changed the manifest), the build fails rather than shipping a statement
    /// that has silently become false.
    /// </summary>
    public sealed class StripInternetPermission : IPreprocessBuildWithReport, IPostGenerateGradleAndroidProject
    {
        // Any <uses-permission> element naming INTERNET, whatever the attribute
        // order, quoting, spacing or closing style, plus the line break after it.
        static readonly Regex InternetPermissionElement = new Regex(
            @"[ \t]*<uses-permission\b[^>]*android:name\s*=\s*[""']android\.permission\.INTERNET[""'][^>]*?(/>|>\s*</uses-permission\s*>)[ \t]*(\r?\n)?");

        static bool _developmentBuild;

        public int callbackOrder => 0;

        // The Gradle callback gets no report, so remember the build kind here.
        public void OnPreprocessBuild(BuildReport report)
        {
            _developmentBuild = (report.summary.options & BuildOptions.Development) != 0;
        }

        /// <summary>The manifest text without the INTERNET element. Pure, so it is tested.</summary>
        public static string Strip(string manifestXml)
        {
            return InternetPermissionElement.Replace(manifestXml, "");
        }

        /// <summary>True if the manifest still asks for INTERNET in any form.</summary>
        public static bool RequestsInternet(string manifestXml)
        {
            return manifestXml.Contains("android.permission.INTERNET");
        }

        public void OnPostGenerateGradleAndroidProject(string unityLibraryPath)
        {
            if (_developmentBuild)
            {
                Debug.Log("[StripInternetPermission] Development build: INTERNET kept for the Profiler connection.");
                return;
            }

            if (PlayerSettings.Android.forceInternetPermission)
            {
                Debug.Log("[StripInternetPermission] Internet Access is Require: INTERNET kept on purpose.");
                return;
            }

            // Unity writes the permission into the unityLibrary module; the launcher
            // module's manifest is checked too, so a plugin or a later Unity version
            // declaring it there cannot slip through.
            string libraryManifest = Path.Combine(unityLibraryPath, "src", "main", "AndroidManifest.xml");
            string launcherManifest = Path.Combine(unityLibraryPath, "..", "launcher", "src", "main", "AndroidManifest.xml");
            if (!File.Exists(libraryManifest))
            {
                throw new BuildFailedException($"[StripInternetPermission] No manifest at {libraryManifest}; cannot prove INTERNET is absent.");
            }

            StripFile(libraryManifest);
            if (File.Exists(launcherManifest))
            {
                StripFile(launcherManifest);
            }
        }

        static void StripFile(string manifestPath)
        {
            string original = File.ReadAllText(manifestPath);
            if (!RequestsInternet(original))
            {
                Debug.Log($"[StripInternetPermission] INTERNET not requested in {manifestPath}; nothing to remove.");
                return;
            }

            string stripped = Strip(original);
            if (RequestsInternet(stripped))
            {
                throw new BuildFailedException(
                    $"[StripInternetPermission] INTERNET is still in {manifestPath} after the strip: the element's " +
                    "shape changed. Update the pattern (and its tests) or the privacy statement is wrong.");
            }

            File.WriteAllText(manifestPath, stripped);
            Debug.Log($"[StripInternetPermission] Removed android.permission.INTERNET from {manifestPath}.");
        }
    }
}
