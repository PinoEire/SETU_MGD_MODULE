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
        bool _skipNext;

        void Awake()
        {
            // The attribute only guards the Inspector; a value set from code or an
            // old prefab reaches here unclamped, and 0 would index an empty array.
            windowSize = Mathf.Max(windowSize, 2);
            _ms = new float[windowSize];
            _scratch = new float[windowSize];
            Restart();
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
            if (_skipNext)
            {
                // The first frame after a load or a restart carries the load time
                // (or the previous state's last frame) in its delta; it would skew
                // the window it lands in.
                _skipNext = false;
                return;
            }

            _ms[_count++] = Time.unscaledDeltaTime * 1000f;
            if (_count < windowSize)
            {
                return;
            }

            _count = 0;
            FrameStats.Compute(_ms, _scratch, out float average, out float p99);

            // The one deliberate allocation: a string every windowSize frames.
            Debug.Log($"[Baseline] avg {average:F2} ms  p99 {p99:F2} ms");

            if (label != null)
            {
                // SetText with arguments formats without allocating a string.
                label.SetText("[Baseline] avg {0:2} ms  p99 {1:2} ms", average, p99);
            }
        }

        /// <summary>
        /// Discards the partial window and the next frame's delta, so the first
        /// line after a change of load state measures only the new state.
        /// </summary>
        public void Restart()
        {
            _count = 0;
            _skipNext = true;
        }
    }
}
