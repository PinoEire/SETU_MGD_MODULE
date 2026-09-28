using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// The Player Settings that a release build depends on, copied into plain
    /// fields so the checklist can be tested without touching the editor.
    /// </summary>
    public struct ReleaseSettings
    {
        public bool Il2Cpp;
        public bool Arm64Only;
        public bool TargetApiAuto;
        public bool BuildAppBundle;
        public bool UseCustomKeystore;
        public bool KeystorePasswordPresent;
        public bool KeyPasswordPresent;
        public string PackageName;

        /// <summary>Reads the live Player Settings for Android.</summary>
        public static ReleaseSettings FromPlayerSettings()
        {
            return new ReleaseSettings
            {
                Il2Cpp = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP,
                Arm64Only = PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64,
                TargetApiAuto = PlayerSettings.Android.targetSdkVersion == AndroidSdkVersions.AndroidApiLevelAuto,
                // Reads the active build profile's flag. The build uses the Android
                // profile asset; with more than one Android profile, activate the
                // one you are building with first.
                BuildAppBundle = EditorUserBuildSettings.buildAppBundle,
                UseCustomKeystore = PlayerSettings.Android.useCustomKeystore,
                // Both passwords are session-only: Unity never writes them to disk.
                KeystorePasswordPresent = !string.IsNullOrEmpty(PlayerSettings.Android.keystorePass),
                KeyPasswordPresent = !string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass),
                PackageName = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android)
            };
        }
    }

    /// <summary>
    /// The Week 4 release checklist as code. Every rule is one line a student
    /// can read, and every failure names the Player Settings page to fix.
    /// </summary>
    public static class ReleaseChecklist
    {
        // Lower case segments, at least two of them, as Android and Play require.
        static readonly Regex PackageNamePattern = new Regex(@"^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]*)+$");

        public static string[] Check(ReleaseSettings s)
        {
            var problems = new List<string>();

            if (!s.Il2Cpp)
            {
                problems.Add("Scripting Backend must be IL2CPP (Other Settings > Configuration).");
            }

            if (!s.Arm64Only)
            {
                problems.Add("Target Architectures must be ARM64 only (Other Settings > Configuration): every supported phone is 64-bit and ARMv7 only adds size.");
            }

            if (!s.TargetApiAuto)
            {
                problems.Add("Target API Level must be Automatic (highest installed) (Other Settings > Identification).");
            }

            if (s.BuildAppBundle)
            {
                problems.Add("Build App Bundle (Google Play) must be off (Build Profiles > Android > Platform Settings): submissions are APKs.");
            }

            if (!s.UseCustomKeystore)
            {
                problems.Add("Custom Keystore must be selected (Publishing Settings > Project Keystore).");
            }

            if (!s.KeystorePasswordPresent)
            {
                problems.Add("Keystore password is empty: type it in Publishing Settings (passwords are session-only).");
            }

            if (!s.KeyPasswordPresent)
            {
                problems.Add("Key password is empty: type it in Publishing Settings (passwords are session-only).");
            }

            // Unity's default is derived from the company name, so it usually has
            // upper-case letters or a hyphen and fails here; a clean lower-case
            // default such as com.pino.game is fine and passes.
            if (string.IsNullOrEmpty(s.PackageName) || !PackageNamePattern.IsMatch(s.PackageName))
            {
                problems.Add("Package Name must be lower case com.<name>.<title> with no hyphens (Other Settings > Identification).");
            }

            return problems.ToArray();
        }
    }

    /// <summary>
    /// One Markdown row per release in <c>releases/manifest.md</c>. The APK is
    /// not committed; the row is what proves which build went out.
    /// </summary>
    public static class ReleaseManifest
    {
        const string Header =
            "# Release manifest\n\n" +
            "One row per release build made with *MGD Samples > Build Release APK*. The APK itself is attached to the GitHub Release for the tag; paste its link in the Download column. Anyone can check a downloaded file against the SHA-256 here.\n\n" +
            "| Version | versionCode | Date | Commit | Size | SHA-256 | Download |\n" +
            "|---------|-------------|------|--------|------|---------|----------|\n";

        public static string Row(string version, int versionCode, string date, string commit, long bytes, string sha256)
        {
            string size = (bytes / 1048576.0).ToString("F1", CultureInfo.InvariantCulture) + " MB";
            return $"| {version} | {versionCode} | {date} | {commit} | {size} | {sha256} | |";
        }

        public static void Append(string path, string row)
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, Header);
            }

            // A hand-edited manifest (the Download column is pasted in) may have
            // lost its final newline; never glue a row onto the previous one.
            string existing = File.ReadAllText(path);
            string separator = existing.Length == 0 || existing.EndsWith("\n") ? "" : "\n";
            File.AppendAllText(path, separator + row + "\n");
        }
    }
}
