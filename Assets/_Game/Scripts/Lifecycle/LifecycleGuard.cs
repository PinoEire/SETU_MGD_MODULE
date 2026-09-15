using System;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Pauses the game whenever Android takes it away from the player: Home, app
    /// switch, incoming call, screen off, the notification shade, a permission
    /// dialog or the on-screen keyboard. Saves before pausing, because this may be
    /// the last code that runs before the OS kills the process.
    ///
    /// Resuming is never automatic. The player chooses to resume from the pause
    /// menu, so coming back from a call does not drop them straight into play.
    ///
    /// Put it on a Bootstrap object in the first scene. Anything that must stop
    /// while paused checks <see cref="IsPaused"/>, because <c>Time.timeScale = 0</c>
    /// stops physics and animation but not <c>Update</c>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LifecycleGuard : MonoBehaviour
    {
        public static bool IsPaused { get; private set; }

        /// <summary>Raised after the pause state changes. UI subscribes to show or hide the menu.</summary>
        public static event Action<bool> PausedChanged;

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

        /// <summary>The Resume button calls this with <c>false</c>.</summary>
        public void SetPaused(bool value)
        {
            // Pause and focus loss usually both fire on Home; only act once.
            if (IsPaused == value)
            {
                return;
            }

            IsPaused = value;
            Time.timeScale = value ? 0f : 1f;
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
