using System;
using TMPro;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MGD.Samples
{
    /// <summary>
    /// The on-phone readout for the Week 7 screenshots, in three lines: smoothed
    /// frame time with the Adaptive Performance temperature level and trend; the
    /// warning level and bottleneck; the performance mode, target and render
    /// scale. Keep it in the build until CA3: the showcase markers need it.
    ///
    /// It runs every frame in a release build, so it allocates nothing in the
    /// steady state, which is why it has two labels where the deck has one. The
    /// numbers change every refresh and go through <c>SetText</c> with arguments,
    /// which formats without a string; the status lines hold names, which
    /// <c>SetText</c> cannot format, so they are a string rebuilt only when one of
    /// their values changes. The labels refresh four times a second: readable,
    /// and cheaper than every frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ThermalOverlay : MonoBehaviour
    {
        [SerializeField] TMP_Text numbers;
        [SerializeField] TMP_Text status;

        [Tooltip("Seconds between label refreshes.")]
        [SerializeField, Min(0.05f)] float refreshInterval = 0.25f;

        float _ms;
        float _nextRefresh;
        bool _skippedFirst;

        // What the status line shows now; it is rebuilt when any of these differ.
        bool _statusShown;
        bool _shownActive;
        WarningLevel _shownWarning;
        PerformanceBottleneck _shownBottleneck;
        PerfMode _shownMode;
        int _shownFps;
        float _shownScale;

        UniversalRenderPipelineAsset _urp;

        void Awake()
        {
            _urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        }

        void Update()
        {
            // The first frame's delta carries the scene load; it would inflate the
            // reading for the next few dozen frames.
            if (!_skippedFirst)
            {
                _skippedFirst = true;
                return;
            }

            // Unscaled, so the pause (timeScale 0) does not freeze the reading.
            float now = Time.unscaledDeltaTime * 1000f;
            _ms = _ms > 0f ? Mathf.Lerp(_ms, now, 0.1f) : now;

            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + refreshInterval;

            IAdaptivePerformance ap = Holder.Instance;
            bool active = ap != null && ap.Active;
            WarningLevel warning = WarningLevel.NoWarning;
            PerformanceBottleneck bottleneck = PerformanceBottleneck.Unknown;

            if (active)
            {
                ThermalMetrics t = ap.ThermalStatus.ThermalMetrics;
                warning = t.WarningLevel;
                bottleneck = ap.PerformanceStatus.PerformanceMetrics.PerformanceBottleneck;
                numbers.SetText("{0:1} ms   temp {1:2}   trend {2:2}", _ms, t.TemperatureLevel, t.TemperatureTrend);
            }
            else
            {
                numbers.SetText("{0:1} ms", _ms);
            }

            PerfMode mode = PerformanceMode.Current;
            int fps = Application.targetFrameRate;
            float scale = _urp != null ? _urp.renderScale : 1f;

            if (_statusShown && active == _shownActive && warning == _shownWarning &&
                bottleneck == _shownBottleneck && mode == _shownMode && fps == _shownFps &&
                Mathf.Approximately(scale, _shownScale))
            {
                return;
            }

            _statusShown = true;
            _shownActive = active;
            _shownWarning = warning;
            _shownBottleneck = bottleneck;
            _shownMode = mode;
            _shownFps = fps;
            _shownScale = scale;
            status.text = StatusLine(active, warning, bottleneck, mode, fps, scale);
        }

        /// <summary>The two-line status text. Allocates, so call it only on a change.</summary>
        public static string StatusLine(bool active, WarningLevel warning, PerformanceBottleneck bottleneck,
            PerfMode mode, int fps, float scale)
        {
            string thermal = active
                ? $"{warning}  |  bottleneck {bottleneck}"
                : "(AP inactive)";
            string modeName = mode == PerfMode.HighFps ? "High FPS" : "Balanced";
            return FormattableString.Invariant($"{thermal}\n{modeName}  {fps} fps  scale {scale:0.00}");
        }
    }
}
