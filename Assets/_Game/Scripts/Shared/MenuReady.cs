using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Time to interactive: put it in the menu scene. Its first Update is the first
    /// frame the player can use the menu, so it logs the time since the process
    /// started, once per app run, then switches itself off. Read it with
    /// <c>adb logcat -s Unity | grep perf</c>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MenuReady : MonoBehaviour
    {
        static bool _logged;

        // With domain reloading off in the editor, statics survive between Play sessions.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _logged = false;
        }

        void Update()
        {
            if (!_logged)
            {
                _logged = true;
                Debug.Log($"[perf] interactive {Time.realtimeSinceStartup:F2}s");
            }

            enabled = false;
        }
    }
}
