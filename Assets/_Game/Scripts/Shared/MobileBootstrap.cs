using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// App-wide settings that must be applied once, before anything else runs.
    /// Put it on a Bootstrap object in the first scene. Android runs a Unity game
    /// at 30 fps until <c>targetFrameRate</c> says otherwise. <c>vSyncCount</c> is
    /// ignored on Android and stays 0 so the editor paces the same way. The
    /// <c>[Boot]</c> line is the first thing to look for in Logcat: it names the
    /// device, OS, graphics API and resolution the numbers that follow came from.
    /// It logs <c>[Boot]</c> and <c>session_start</c> once per app run, although
    /// the first scene is loaded again every time the player goes back to it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MobileBootstrap : MonoBehaviour
    {
        /// <summary>The module's default. Turn-based and idle games may choose 30.</summary>
        public const int TargetFrameRate = 60;

        // The first scene is loaded again on every return to it; log the start of the
        // session once per run. Reset for editors with domain reloading off.
        static bool s_sessionLogged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            s_sessionLogged = false;
        }

        void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;
            QualitySettings.vSyncCount = 0;
            // Keep the screen on while the game is in the foreground.
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            if (s_sessionLogged)
            {
                return;
            }

            s_sessionLogged = true;
            Debug.Log($"[Boot] {SystemInfo.deviceModel} | {SystemInfo.operatingSystem} | " +
                      $"{SystemInfo.graphicsDeviceType} | {Screen.width}x{Screen.height} @ {Screen.dpi} dpi | " +
                      $"target {TargetFrameRate} fps");
            Telemetry.Log("session_start",
                ("app_version", Application.version),
                ("device_model", SystemInfo.deviceModel),
                ("android", SystemInfo.operatingSystem));
        }
    }
}
