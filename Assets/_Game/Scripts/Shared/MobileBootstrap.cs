using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// App-wide settings applied once, in the first scene. Put it on a Bootstrap
    /// object there. Android runs a Unity game at 30 fps until
    /// <c>targetFrameRate</c> says otherwise; that target comes from
    /// <see cref="PerformanceMode"/> (60, or 120 in High FPS), applied before the
    /// first scene loads, so this only reports it. <c>vSyncCount</c> is ignored on
    /// Android and stays 0 so the editor paces the same way. The <c>[Boot]</c>
    /// line is the first thing to look for in Logcat: it names the device, OS,
    /// graphics API and resolution the numbers that follow came from.
    /// It logs <c>[Boot]</c> and <c>session_start</c> once per app run, although
    /// the first scene is loaded again every time the player goes back to it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MobileBootstrap : MonoBehaviour
    {
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
                      $"target {Application.targetFrameRate} fps ({PerformanceMode.Current})");
            // The build ID ties every session in a pulled log to one binary.
            Telemetry.Log("session_start",
                ("app_version", Application.version),
                ("build", BuildStamp.BuildId),
                ("device_model", SystemInfo.deviceModel),
                ("android", SystemInfo.operatingSystem));
        }
    }
}
