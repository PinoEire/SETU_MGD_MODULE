using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Demo scaffolding for the Resume scene, not something to copy into a game. It
    /// shows what <see cref="ResumeGuard"/> and <see cref="OfflineProgress"/> are
    /// doing, so the six-case resume matrix from Week 11 Lab A can be checked on the
    /// phone: the boss countdown, the wave count, the last offline reward, and a log
    /// of the boss spawn, pause and resume, the offline reward, and lifecycle
    /// callbacks with wall-clock times.
    ///
    /// It watches the guard rather than the guard calling it, so the guard stays
    /// exactly what students copy.
    /// </summary>
    public sealed class ResumeDemoHud : MonoBehaviour
    {
        const int LogLines = 8;

        [SerializeField] ResumeGuard guard;
        [SerializeField] OfflineProgress offline;
        [SerializeField] TMP_Text boss;
        [SerializeField] TMP_Text waves;
        [SerializeField] TMP_Text away;
        [SerializeField] TMP_Text log;

        readonly string[] _lines = new string[LogLines];
        int _nextLine;
        readonly StringBuilder _logBuilder = new StringBuilder(512);

        // Last values shown, so the labels are only rebuilt when something changes.
        int _shownTenths = -1;
        int _shownWaves = -1;
        bool _shownSpawned;

        void OnEnable()
        {
            LifecycleGuard.PausedChanged += OnPausedChanged;
            offline.Returned += OnReturned;
        }

        void OnDisable()
        {
            LifecycleGuard.PausedChanged -= OnPausedChanged;
            offline.Returned -= OnReturned;
        }

        void Update()
        {
            if (guard.BossSpawned != _shownSpawned)
            {
                _shownSpawned = guard.BossSpawned;
                _shownTenths = -1;
                if (_shownSpawned)
                {
                    boss.SetText("Boss spawned");
                    Append("Boss spawned");
                }
            }

            if (!_shownSpawned)
            {
                int tenths = Mathf.CeilToInt(guard.BossRemaining * 10f);
                if (tenths != _shownTenths)
                {
                    _shownTenths = tenths;
                    // SetText with format arguments does not allocate a string.
                    boss.SetText("Boss in {0:1} s", tenths / 10f);
                }
            }

            if (guard.WavesSpawned != _shownWaves)
            {
                _shownWaves = guard.WavesSpawned;
                waves.SetText("Waves spawned: {0:0}", _shownWaves);
            }
        }

        void OnPausedChanged(bool paused)
        {
            Append(paused ? "Paused: timers stopped" : "Resumed: timers started");
        }

        void OnApplicationPause(bool paused)
        {
            Append(paused ? "OnApplicationPause(true)" : "OnApplicationPause(false)");
        }

        void OnApplicationFocus(bool hasFocus)
        {
            Append(hasFocus ? "OnApplicationFocus(true)" : "OnApplicationFocus(false)");
        }

        void OnReturned(double seconds, int coins)
        {
            away.SetText("Away {0:0} s: +{1:0} coins", (float)seconds, coins);
            Append("Offline reward");
        }

        // The same ring buffer as LifecycleDemoHud, kept local so the sample stays self-contained.
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
    }
}
