using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MGD.Samples
{
    public enum RoundOutcome
    {
        Playing,
        Won,
        Lost
    }

    /// <summary>
    /// Demo scaffolding: a 20 s round of tapping targets, standing in for your
    /// core verb. Each tap earns <see cref="Wallet.CoinsPerTap"/> and moves the
    /// target (repositioned, never instantiated). Reaching the goal is
    /// <c>level_complete</c>, running out of time is <c>level_fail</c> (and so is
    /// leaving the scene mid-round, with <c>cause=quit</c>); the start of each round
    /// is <c>level_start</c>. The timer is an Awaitable loop on scaled
    /// time, so it stops while the game is paused.
    /// </summary>
    public sealed class TapRound : MonoBehaviour
    {
        const int LevelId = 1;

        [SerializeField] Wallet wallet;
        [SerializeField] RectTransform playArea;
        [SerializeField] Button[] targets;
        [SerializeField] GameObject betweenRounds;
        [SerializeField] GameObject resetButton;
        [SerializeField] TMP_Text status;

        [Tooltip("Seconds in a round.")]
        [SerializeField, Min(1f)] float roundSeconds = 20f;

        [Tooltip("Taps needed to win the round.")]
        [SerializeField, Min(1)] int goal = 15;

        // Attempts count across the whole app run, not one visit to this scene: the
        // lab's level_start reports how often the player retried the lane this session.
        static int s_attempt;

        CancellationTokenSource _round;
        bool _playing;
        int _score;
        int _coinsEarned;
        float _secondsLeft;
        float _startTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            s_attempt = 0;
        }

        /// <summary>The round's rule: the goal wins, even on the last second; no time left loses.</summary>
        public static RoundOutcome Outcome(int score, int goal, float secondsLeft)
        {
            if (score >= goal)
            {
                return RoundOutcome.Won;
            }

            return secondsLeft <= 0f ? RoundOutcome.Lost : RoundOutcome.Playing;
        }

        void Start()
        {
            // The pacing pass resets the wallet on the phone; players never see this.
            resetButton.SetActive(Debug.isDebugBuild);
            SetTargets(false);
            status.text = $"Tap Start round, then tap the targets: {goal} taps in {roundSeconds:0} s.";
        }

        void OnDestroy()
        {
            // Leaving mid-round (back to the samples) still ends the round in the
            // log, so every level_start has an outcome. A process killed by Android
            // gets no chance to log, which the event map notes. The timer stops
            // first, so nothing the log does can leave it running.
            StopTimer();
            if (_playing)
            {
                _playing = false;
                Telemetry.Log("level_fail", ("level_id", LevelId), ("time_s", RoundSeconds()),
                    ("cause", "quit"), ("coins_earned", _coinsEarned));
            }
        }

        /// <summary>Wired to the Start button.</summary>
        public void OnStartPressed()
        {
            if (_playing)
            {
                return;
            }

            _playing = true;
            s_attempt++;
            _score = 0;
            _coinsEarned = 0;
            _secondsLeft = roundSeconds;
            _startTime = Time.time;
            Telemetry.Log("level_start", ("level_id", LevelId), ("attempt", s_attempt), ("coins", wallet.Coins));

            betweenRounds.SetActive(false);
            SetTargets(true);
            foreach (Button target in targets)
            {
                Place(target);
            }

            ShowProgress();
            _round = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            _ = RunTimerAsync(_round.Token);
        }

        /// <summary>Wired to each target button, with that button as the argument.</summary>
        public void OnTargetPressed(Button target)
        {
            if (!_playing || LifecycleGuard.IsPaused)
            {
                return;
            }

            _score++;
            int coins = Wallet.CoinsPerTap(wallet.Level);
            _coinsEarned += coins; // counted here, so an upgrade bought mid-round does not distort it
            wallet.Earn(coins);
            Place(target);

            if (Outcome(_score, goal, _secondsLeft) == RoundOutcome.Won)
            {
                End(RoundOutcome.Won);
            }
            else
            {
                ShowProgress();
            }
        }

        /// <summary>Wired to the Reset wallet button (development builds only).</summary>
        public void OnResetPressed()
        {
            wallet.Restore(0, 0);
        }

        async Awaitable RunTimerAsync(CancellationToken ct)
        {
            try
            {
                while (true)
                {
                    // Scaled time: a paused game (timeScale 0) does not lose seconds.
                    await Awaitable.WaitForSecondsAsync(1f, ct);
                    _secondsLeft -= 1f;
                    if (Outcome(_score, goal, _secondsLeft) == RoundOutcome.Lost)
                    {
                        End(RoundOutcome.Lost);
                        return;
                    }

                    ShowProgress();
                }
            }
            catch (OperationCanceledException)
            {
                // The round was won (End cancels the timer) or the scene was left.
            }
        }

        void End(RoundOutcome outcome)
        {
            _playing = false;
            StopTimer();
            SetTargets(false);
            betweenRounds.SetActive(true);

            float seconds = RoundSeconds();
            if (outcome == RoundOutcome.Won)
            {
                Telemetry.Log("level_complete", ("level_id", LevelId), ("time_s", seconds),
                    ("coins_earned", _coinsEarned), ("score", _score));
                status.text = $"Won in {seconds:0.0} s, +{_coinsEarned} coins. Start another round?";
            }
            else
            {
                Telemetry.Log("level_fail", ("level_id", LevelId), ("time_s", seconds),
                    ("cause", "timeout"), ("coins_earned", _coinsEarned));
                status.text = $"Time up at {_score} of {goal}, +{_coinsEarned} coins. Try again?";
            }
        }

        // Seconds since the round started, to a tenth; paused time does not count.
        float RoundSeconds()
        {
            return Mathf.Round((Time.time - _startTime) * 10f) / 10f;
        }

        void StopTimer()
        {
            if (_round == null)
            {
                return;
            }

            _round.Cancel();
            _round.Dispose();
            _round = null;
        }

        void ShowProgress()
        {
            status.SetText("{0} of {1} taps   {2} s left", (float)_score, (float)goal, _secondsLeft);
        }

        void SetTargets(bool visible)
        {
            foreach (Button target in targets)
            {
                target.gameObject.SetActive(visible);
            }
        }

        // A random spot in this target's own column of the play area, so targets
        // never overlap and the whole target stays inside.
        void Place(Button target)
        {
            var rect = (RectTransform)target.transform;
            Rect area = playArea.rect;
            Vector2 half = rect.rect.size * 0.5f;
            float column = area.width / targets.Length;
            float left = area.xMin + column * Array.IndexOf(targets, target);
            if (column < rect.rect.width)
            {
                // Too narrow for a column each: use the whole width (targets may overlap).
                column = area.width;
                left = area.xMin;
            }

            // Mathf.Max keeps each range valid when the area is smaller than a target
            // (a very short landscape screen): the target then sits at the edge.
            rect.anchoredPosition = new Vector2(
                Random.Range(left + half.x, Mathf.Max(left + half.x, left + column - half.x)),
                Random.Range(area.yMin + half.y, Mathf.Max(area.yMin + half.y, area.yMax - half.y)));
        }
    }
}
