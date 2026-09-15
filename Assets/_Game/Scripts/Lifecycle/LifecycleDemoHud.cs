using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MGD.Samples
{
    /// <summary>
    /// Demo scaffolding for the Lifecycle scene. Not something to copy into a game;
    /// it exists so the effects of <see cref="LifecycleGuard"/> can be seen and heard
    /// on the phone while running the five-row lifecycle test matrix:
    ///
    /// - a spinner and a game-time clock that stop at timeScale 0, next to a
    ///   real-time clock that does not;
    /// - a looping tone that goes silent while paused (AudioListener.pause);
    /// - a tap counter kept in PlayerPrefs, to check that force-stop and relaunch
    ///   restores what the guard saved;
    /// - a log of the last few pause and focus callbacks with wall-clock times.
    ///
    /// It receives OnApplicationPause and OnApplicationFocus itself (every
    /// MonoBehaviour does) rather than adding a logging hook to the guard, so the
    /// guard stays exactly what students copy.
    /// </summary>
    public sealed class LifecycleDemoHud : MonoBehaviour
    {
        const string CounterKey = "MGD.Lifecycle.TapCount";
        const int LogLines = 6;

        [SerializeField] RectTransform spinner;
        [SerializeField] TMP_Text clocks;
        [SerializeField] TMP_Text counter;
        [SerializeField] TMP_Text log;
        [SerializeField] AudioSource tone;

        int _taps;
        readonly string[] _lines = new string[LogLines];
        int _nextLine;
        readonly StringBuilder _logBuilder = new StringBuilder(256);

        void Awake()
        {
            _taps = PlayerPrefs.GetInt(CounterKey, 0);
            tone.clip = BuildTone(220f, 1f);
            tone.loop = true;
            tone.volume = 0.15f;
        }

        void OnEnable()
        {
            LifecycleGuard.PausedChanged += OnPausedChanged;
        }

        void OnDisable()
        {
            LifecycleGuard.PausedChanged -= OnPausedChanged;
        }

        void Start()
        {
            tone.Play();
            RefreshCounter();
            Append("Start");
        }

        void Update()
        {
            // Rotation uses deltaTime, so it stops at timeScale 0. Update itself keeps
            // running while paused, which is why gameplay must check IsPaused.
            spinner.Rotate(0f, 0f, -90f * Time.deltaTime);

            // SetText with format arguments does not allocate a string every frame.
            clocks.SetText("Game time {0:1} s\nReal time {1:1} s", Time.time, Time.realtimeSinceStartup);
        }

        // Same callbacks the guard receives, logged here so the guard stays clean.
        void OnApplicationPause(bool paused)
        {
            Append(paused ? "OnApplicationPause(true)" : "OnApplicationPause(false)");
        }

        void OnApplicationFocus(bool hasFocus)
        {
            Append(hasFocus ? "OnApplicationFocus(true)" : "OnApplicationFocus(false)");
        }

        void OnPausedChanged(bool paused)
        {
            Append(paused ? "IsPaused = true" : "IsPaused = false");
        }

        /// <summary>Wired to the Tap button's OnClick.</summary>
        public void OnTapPressed()
        {
            _taps++;
            // Written to memory now; LifecycleGuard flushes it to disk on pause.
            PlayerPrefs.SetInt(CounterKey, _taps);
            RefreshCounter();
        }

        void RefreshCounter()
        {
            counter.SetText("Taps saved: {0:0}", _taps);
        }

        void Append(string entry)
        {
            _lines[_nextLine] = DateTime.Now.ToString("HH:mm:ss") + "  " + entry;
            _nextLine = (_nextLine + 1) % LogLines;

            _logBuilder.Clear();
            for (int i = 0; i < LogLines; i++)
            {
                string line = _lines[(_nextLine + i) % LogLines];
                if (line != null)
                {
                    _logBuilder.AppendLine(line);
                }
            }
            log.text = _logBuilder.ToString();
        }

        static AudioClip BuildTone(float frequency, float seconds)
        {
            const int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * seconds);
            var samples = new float[sampleCount];
            float step = 2f * Mathf.PI * frequency / sampleRate;
            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = Mathf.Sin(step * i);
            }

            var clip = AudioClip.Create("Tone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
