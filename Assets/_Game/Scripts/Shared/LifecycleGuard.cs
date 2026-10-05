using System;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Pauses the game whenever Android takes it away from the player: Home, app
    /// switch, incoming call, screen off, the notification shade, a permission
    /// dialog or the on-screen keyboard. Saves before pausing on
    /// <c>OnApplicationPause(true)</c>, because that may be the last code that runs
    /// before the OS kills the process. Focus loss alone (the shade, a dialog, the
    /// keyboard) only pauses: the process is not at risk, and saving on every
    /// shade pull would be wasted disk writes.
    ///
    /// Resuming is never automatic. The player chooses to resume from the pause
    /// menu, so coming back from a call does not drop them straight into play.
    ///
    /// Put it in every scene that can pause, next to the <see cref="PauseMenu"/>
    /// (the pause state itself is static, so the two need no reference to each
    /// other). Anything that must stop while paused checks <see cref="IsPaused"/>,
    /// because <c>Time.timeScale = 0</c> stops physics and animation but not
    /// <c>Update</c>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LifecycleGuard : MonoBehaviour
    {
        public static bool IsPaused { get; private set; }

        /// <summary>Raised after the pause state changes. UI subscribes to show or hide the menu.</summary>
        public static event Action<bool> PausedChanged;

        // The time scale the game was running at before the pause, so a game that
        // plays at 0.5 (slow motion) does not come back at 1.
        static float _resumeTimeScale = 1f;

        // With domain reloading off in the editor, a handler left subscribed when
        // Play stopped mid-load would survive into the next session; start clean.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            PausedChanged = null;
        }

        // Home, app switch, incoming call, screen off.
        void OnApplicationPause(bool paused)
        {
            // OnApplicationPause(false) also fires once at launch on Android, and
            // resuming is a player choice anyway, so only the true case acts.
            if (!paused)
            {
                return;
            }

            PlayerPrefs.Save(); // Swap for your own save system.
            SetPaused(true);
        }

        // Notification shade, permission dialog, on-screen keyboard.
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SetPaused(true);
            }
        }

        /// <summary>Pauses or resumes the game. The pause menu calls it from Resume and from the back toggle.</summary>
        public static void SetPaused(bool value)
        {
            // Pause and focus loss usually both fire on Home; only act once.
            if (IsPaused == value)
            {
                return;
            }

            // Only a running scale is worth restoring: a game that is already frozen
            // by its own code (a tutorial pop-up) must not resume to 0 with no panel.
            if (value && Time.timeScale > 0f)
            {
                _resumeTimeScale = Time.timeScale;
            }

            IsPaused = value;
            Time.timeScale = value ? 0f : _resumeTimeScale;
            AudioListener.pause = value;
            PausedChanged?.Invoke(value);
        }

        void OnDestroy()
        {
            // IsPaused and timeScale are global and outlive this scene. Leaving them
            // set would freeze the next scene, and in the editor timeScale even
            // survives exiting Play mode.
            SetPaused(false);
        }
    }
}
