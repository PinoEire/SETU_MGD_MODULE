using System;
using TMPro;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

namespace MGD.Samples
{
    /// <summary>
    /// The clock for the Week 7 Lab B thermal soak. It shows the time since the
    /// soak started, so the minute 0, 5 and 10 screenshots are easy to take, and
    /// the moment the warning level first left <c>NoWarning</c>. Every whole
    /// minute it logs a <c>[Soak]</c> line with the thermal metrics, so a Logcat
    /// recording of the soak carries its own timeline.
    ///
    /// It counts real time, not game time: pausing does not stop a phone warming
    /// up. Leaving the app does not stop the clock either, which is right, because
    /// a soak with a break in it is not a soak; restart it instead.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SoakTimer : MonoBehaviour
    {
        const float RefreshInterval = 0.25f;

        [SerializeField] TMP_Text label;

        float _start;
        float _previous;
        float _firstWarning;
        float _nextRefresh;

        void Start()
        {
            Restart();
        }

        /// <summary>Wired to the Restart soak button's OnClick.</summary>
        public void Restart()
        {
            _start = Time.realtimeSinceStartup;
            _previous = 0f;
            _firstWarning = -1f;
            _nextRefresh = 0f;
            Log("start");
        }

        void Update()
        {
            float elapsed = Time.realtimeSinceStartup - _start;
            if (MinuteCrossed(_previous, elapsed, out int minute))
            {
                Log($"minute {minute}");
            }

            _previous = elapsed;

            IAdaptivePerformance ap = Holder.Instance;
            if (_firstWarning < 0f && ap != null && ap.Active &&
                ap.ThermalStatus.ThermalMetrics.WarningLevel != WarningLevel.NoWarning)
            {
                _firstWarning = elapsed;
                Split(elapsed, out float wm, out float ws);
                Log(FormattableString.Invariant($"first warning at {wm:0} min {ws:0} s"));
            }

            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            Split(elapsed, out float m, out float s);
            if (_firstWarning < 0f)
            {
                label.SetText("Soak {0:0} min {1:0} s   no warning yet", m, s);
            }
            else
            {
                Split(_firstWarning, out float fm, out float fs);
                label.SetText("Soak {0:0} min {1:0} s   first warning {2:0} min {3:0} s", m, s, fm, fs);
            }
        }

        /// <summary>
        /// True when <paramref name="now"/> is in a later whole minute than
        /// <paramref name="previous"/>; <paramref name="minute"/> is the minute
        /// reached. A long stall that skips minutes reports only the latest one.
        /// </summary>
        public static bool MinuteCrossed(float previous, float now, out int minute)
        {
            minute = Mathf.FloorToInt(now / 60f);
            return minute > Mathf.FloorToInt(previous / 60f);
        }

        /// <summary>Whole minutes and whole seconds, as floats for <c>SetText</c>.</summary>
        public static void Split(float seconds, out float minutes, out float rest)
        {
            minutes = Mathf.Floor(seconds / 60f);
            rest = Mathf.Floor(seconds - minutes * 60f);
        }

        // One line per event: the few allocations here happen once a minute at most.
        static void Log(string what)
        {
            IAdaptivePerformance ap = Holder.Instance;
            if (ap == null || !ap.Active)
            {
                Debug.Log($"[Soak] {what} (AP inactive) mode={PerformanceMode.Current}");
                return;
            }

            ThermalMetrics t = ap.ThermalStatus.ThermalMetrics;
            Debug.Log(FormattableString.Invariant(
                $"[Soak] {what} warning={t.WarningLevel} level={t.TemperatureLevel:0.00} trend={t.TemperatureTrend:0.00} mode={PerformanceMode.Current}"));
        }
    }
}
