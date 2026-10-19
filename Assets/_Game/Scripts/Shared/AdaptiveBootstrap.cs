using System;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

namespace MGD.Samples
{
    /// <summary>
    /// Logs what Adaptive Performance reports, as <c>[AP]</c> lines for Logcat: one
    /// line at start-up with the current thermal state, then one per thermal
    /// change (<c>ThermalEvent</c>) and one per bottleneck change
    /// (<c>PerformanceBottleneckChangeEvent</c>). Both events fire on change only,
    /// so a cool phone can go minutes without a thermal line; poll
    /// <c>ThermalStatus.ThermalMetrics</c> for a value now, as the overlay does.
    ///
    /// The first scene creates it on a DontDestroyOnLoad object, so it keeps
    /// listening whichever scene is open; the copy that comes back with the first
    /// scene destroys itself. Needs the Adaptive Performance Android provider
    /// ticked and Initialize on Startup on; without them, or on Android below 11,
    /// it logs that the provider is inactive and does nothing else.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AdaptiveBootstrap : MonoBehaviour
    {
        static AdaptiveBootstrap s_instance;

        IAdaptivePerformance _ap;

        // With domain reloading off in the editor, statics survive between Play
        // sessions; this clears the stale reference before the first scene loads.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            s_instance = null;
        }

        void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            // Destroy in Awake is deferred to the end of the frame; a duplicate's
            // Start can still run, and must not subscribe a second time.
            if (s_instance != this)
            {
                return;
            }

            IAdaptivePerformance ap = Holder.Instance;
            if (ap == null || !ap.Active)
            {
                Debug.Log("[AP] provider inactive on this device");
                return;
            }

            _ap = ap;
            _ap.ThermalStatus.ThermalEvent += OnThermal;
            _ap.PerformanceStatus.PerformanceBottleneckChangeEvent += OnBottleneck;

            ThermalMetrics m = _ap.ThermalStatus.ThermalMetrics;
            // Invariant: a phone set to a comma-decimal language must not change the
            // line a findstr or a spreadsheet reads.
            Debug.Log(FormattableString.Invariant(
                $"[AP] active warning={m.WarningLevel} level={m.TemperatureLevel:0.00}"));
        }

        void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
            }

            if (_ap == null)
            {
                return;
            }

            _ap.ThermalStatus.ThermalEvent -= OnThermal;
            _ap.PerformanceStatus.PerformanceBottleneckChangeEvent -= OnBottleneck;
        }

        static void OnThermal(ThermalMetrics m)
        {
            Debug.Log(FormattableString.Invariant(
                $"[AP] thermal warning={m.WarningLevel} level={m.TemperatureLevel:0.00} trend={m.TemperatureTrend:0.00}"));
        }

        static void OnBottleneck(PerformanceBottleneckChangeEventArgs e)
        {
            Debug.Log($"[AP] bottleneck={e.PerformanceBottleneck}");
        }
    }
}
