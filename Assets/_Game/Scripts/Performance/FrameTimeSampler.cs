using TMPro;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Logs average and 99th-percentile frame time every <c>windowSize</c> frames,
    /// in milliseconds from <c>Time.unscaledDeltaTime</c> so a paused game
    /// (timeScale 0) cannot hide a stutter. It runs in the release build, so
    /// Update allocates nothing: both buffers are made once in Awake and the
    /// <c>[Baseline]</c> log line is the only garbage, once per window. Put it on
    /// any object in the scene being measured; give it a label to read the
    /// numbers on the phone without Logcat.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FrameTimeSampler : MonoBehaviour
    {
        [Tooltip("Frames per window. 600 is 10 s at 60 fps, the lab sheet's value.")]
        [SerializeField, Min(2)] int windowSize = 600;

        [Tooltip("Optional on-screen copy of the log line.")]
        [SerializeField] TMP_Text label;

        float[] _ms;
        float[] _scratch;
        int _count;

        /// <summary>Last completed window's average in ms; NaN before the first window.</summary>
        public float LastAverageMs { get; private set; } = float.NaN;

        /// <summary>Last completed window's 99th percentile in ms; NaN before the first window.</summary>
        public float LastP99Ms { get; private set; } = float.NaN;

        void Awake()
        {
            _ms = new float[windowSize];
            _scratch = new float[windowSize];
        }

        // Start, not Awake: the TextMeshPro label may not have initialised yet.
        void Start()
        {
            if (label != null)
            {
                label.text = "[Baseline] waiting for the first window";
            }
        }

        void Update()
        {
            _ms[_count++] = Time.unscaledDeltaTime * 1000f;
            if (_count < windowSize)
            {
                return;
            }

            _count = 0;
            FrameStats.Compute(_ms, _scratch, out float average, out float p99);
            LastAverageMs = average;
            LastP99Ms = p99;

            // The one deliberate allocation: a string every windowSize frames.
            Debug.Log($"[Baseline] avg {average:F2} ms  p99 {p99:F2} ms");

            if (label != null)
            {
                // SetText with arguments formats without allocating a string.
                label.SetText("[Baseline] avg {0:2} ms  p99 {1:2} ms", average, p99);
            }
        }

        /// <summary>
        /// Discards the partial window, so the first line after a change of load
        /// state measures only the new state.
        /// </summary>
        public void Restart()
        {
            _count = 0;
        }
    }
}
