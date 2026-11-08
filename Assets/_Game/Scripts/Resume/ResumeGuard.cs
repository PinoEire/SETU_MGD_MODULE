using System;
using System.Threading;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Timed Awaitables that stop on pause and restart on resume. Every timed await
    /// takes its token from one source: pausing cancels it, resuming makes a new
    /// one, and StartTimers always cancels any old one first, so no loop can run
    /// twice. A one-shot timer keeps its remaining time and never restarts once it
    /// has fired.
    ///
    /// Restarting hangs off <see cref="LifecycleGuard.PausedChanged"/> (in
    /// <c>Scripts/Shared/</c>) because this game resumes when the player taps
    /// Resume, not when Android returns the app; see the vault decision
    /// "2026-11-08 Timers restart on the game's resume, not on OnApplicationPause(false)".
    /// </summary>
    public sealed class ResumeGuard : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] float bossDelay = 5f;
        [SerializeField, Min(0.1f)] float waveInterval = 2f;

        CancellationTokenSource _timerSource;
        PausableCountdown _boss;
        bool _started;

        public bool BossSpawned => _boss.Fired;
        public float BossRemaining => _boss.Remaining(Time.time);
        public int WavesSpawned { get; private set; }

        void Awake()
        {
            _boss = new PausableCountdown(bossDelay);
        }

        void OnEnable()
        {
            LifecycleGuard.PausedChanged += OnPausedChanged;
            // Re-enabled after Start (a toggled or pooled object): Start will not run again.
            if (_started && !LifecycleGuard.IsPaused)
            {
                StartTimers();
            }
        }

        void OnDisable()
        {
            LifecycleGuard.PausedChanged -= OnPausedChanged;
            StopTimers();
        }

        void Start()
        {
            _started = true;
            // A scene that opens behind the pause panel starts its timers on Resume.
            if (!LifecycleGuard.IsPaused)
            {
                StartTimers();
            }
        }

        void OnPausedChanged(bool paused)
        {
            if (paused)
            {
                StopTimers();
            }
            else
            {
                StartTimers();
            }
        }

        /// <summary>Wired to the Restart button: a fresh boss timer and wave count, to repeat a test.</summary>
        public void Restart()
        {
            StopTimers();
            _boss.Reset();
            WavesSpawned = 0;
            if (!LifecycleGuard.IsPaused)
            {
                StartTimers();
            }
        }

        void StartTimers()
        {
            StopTimers(); // Never two sources, so never two copies of a loop.

            // Linked to the object's own lifetime and to the application quitting, so
            // nothing outlives the scene or the app even if OnDisable never runs.
            _timerSource = CancellationTokenSource.CreateLinkedTokenSource(
                destroyCancellationToken, Application.exitCancellationToken);
            CancellationToken token = _timerSource.Token;

            if (_boss.Start(Time.time))
            {
                _ = BossTimerAsync(_boss.Remaining(Time.time), token);
            }
            _ = WaveLoopAsync(token);
        }

        void StopTimers()
        {
            if (_timerSource == null)
            {
                return;
            }

            // Keep how far the boss got before its await is cancelled.
            _boss.Stop(Time.time);
            _timerSource.Cancel();
            _timerSource.Dispose();
            _timerSource = null;
        }

        async Awaitable BossTimerAsync(float seconds, CancellationToken token)
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(seconds, token);
            }
            catch (OperationCanceledException)
            {
                return; // The expected path on pause: StopTimers kept the remaining time.
            }

            _boss.Fire();
            Debug.Log("[Resume] Boss spawned");
        }

        async Awaitable WaveLoopAsync(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    await Awaitable.WaitForSecondsAsync(waveInterval, token);
                    WavesSpawned++;
                    Debug.Log($"[Resume] Wave {WavesSpawned} spawned");
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on pause and on destroy.
            }
        }
    }
}
