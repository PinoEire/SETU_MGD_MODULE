using System;
using TMPro;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// The build stamp for an About screen: version, build ID, device, Android
    /// version, RAM, resolution and panel refresh rate. The build ID is the first
    /// eight characters of <c>Application.buildGUID</c>, which is new for every
    /// build, so a tester who quotes it names exactly one binary; the release
    /// manifest records the same ID per build. The stamp is written once in
    /// <c>Start</c> and logs a <c>build=</c> line for logcat.
    ///
    /// <see cref="BuildId"/> is static so other code can tag its output with it:
    /// <c>MobileBootstrap</c> puts it in the <c>session_start</c> telemetry event.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BuildStamp : MonoBehaviour
    {
        [SerializeField] TMP_Text label;

        /// <summary>The short build ID of the running build, or "editor" in Play mode.</summary>
        public static string BuildId => ShortId(Application.buildGUID);

        // Start, not Awake: the TextMeshPro label may not have initialised yet.
        void Start()
        {
            label.text = Format(Application.version, BuildId, SystemInfo.deviceModel,
                SystemInfo.operatingSystem, SystemInfo.systemMemorySize,
                Screen.width, Screen.height, Screen.currentResolution.refreshRateRatio.value);
            Debug.Log($"build={Application.version}/{BuildId}");
        }

        /// <summary>
        /// The first eight characters of a build GUID; "editor" when there is no
        /// build, which in Play mode means an empty or all-zero GUID.
        /// </summary>
        public static string ShortId(string guid)
        {
            if (string.IsNullOrEmpty(guid) || guid.Trim('0').Length == 0)
            {
                return "editor";
            }

            return guid.Length >= 8 ? guid.Substring(0, 8) : guid;
        }

        /// <summary>The four-line stamp text, with the refresh rate rounded to whole hertz.</summary>
        public static string Format(string version, string buildId, string deviceModel, string operatingSystem,
            int ramMb, int width, int height, double refreshHz)
        {
            return FormattableString.Invariant(
                $"v{version} ({buildId})\n{deviceModel}\n{operatingSystem}\n{ramMb} MB RAM  {width}x{height} @ {refreshHz:0} Hz");
        }
    }
}
