using System;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Offline progress for an idle game, measured on the wall clock: the UTC time is
    /// saved when the app goes to the background and compared when it comes back,
    /// including after Android has killed the process. <c>Time.time</c> cannot do
    /// this: it does not advance while the app is suspended and starts again from
    /// zero in a new process.
    ///
    /// The time away is clamped: never negative (the player moved the phone's clock
    /// back) and never more than <see cref="maxHoursAway"/> (a cap is a design
    /// choice, and it also limits what moving the clock forward can earn).
    /// </summary>
    public sealed class OfflineProgress : MonoBehaviour
    {
        const string PausedAtKey = "MGD.Resume.PausedAtUtc";

        [SerializeField, Min(0f)] float coinsPerSecond = 1f;
        [SerializeField, Min(0f)] float maxHoursAway = 8f;

        /// <summary>Raised with the seconds away and the coins earned when a return is counted.</summary>
        public event Action<double, int> Returned;

        // Start, not Awake, so a listener added in another component's Awake or OnEnable hears it.
        void Start()
        {
            // A stamp still here at launch means the last session went to the
            // background and never came back: the process was killed.
            Collect();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                PlayerPrefs.SetString(PausedAtKey, DateTime.UtcNow.Ticks.ToString());
                PlayerPrefs.Save(); // The process may be killed from here on.
            }
            else
            {
                // Also fires once at launch on Android. Whichever of this and Start
                // runs first collects and clears the stamp; the other finds nothing.
                Collect();
            }
        }

        void Collect()
        {
            if (!long.TryParse(PlayerPrefs.GetString(PausedAtKey, ""), out long pausedAt))
            {
                return;
            }

            PlayerPrefs.DeleteKey(PausedAtKey);
            // Whole seconds, so the reward and the time shown agree.
            double seconds = Math.Floor(SecondsAway(pausedAt, DateTime.UtcNow.Ticks, maxHoursAway * 3600.0));
            int coins = (int)(seconds * coinsPerSecond);
            Debug.Log($"[Resume] Away {seconds:0} s, offline reward {coins} coins");
            Returned?.Invoke(seconds, coins);
        }

        /// <summary>
        /// Seconds between two UTC tick stamps, clamped to 0 to <paramref name="maxSeconds"/>.
        /// </summary>
        public static double SecondsAway(long pausedAtTicks, long nowTicks, double maxSeconds)
        {
            double seconds = TimeSpan.FromTicks(nowTicks - pausedAtTicks).TotalSeconds;
            return Math.Clamp(seconds, 0.0, maxSeconds);
        }
    }
}
