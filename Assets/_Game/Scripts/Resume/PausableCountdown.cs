using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// The bookkeeping of a one-shot timer that can be paused: how long is left,
    /// whether it is running, and whether it has fired. It holds no clock; the
    /// caller passes the time (the same clock as the wait, here <c>Time.time</c>),
    /// so it is plain C# and testable in EditMode.
    /// </summary>
    public sealed class PausableCountdown
    {
        readonly float _duration;
        float _remaining;
        float _startedAt;

        public PausableCountdown(float duration)
        {
            _duration = duration;
            _remaining = duration;
        }

        public bool Running { get; private set; }
        public bool Fired { get; private set; }

        /// <summary>Seconds left at <paramref name="now"/>; never negative.</summary>
        public float Remaining(float now)
        {
            return Running ? Mathf.Max(0f, _remaining - (now - _startedAt)) : _remaining;
        }

        /// <summary>Starts counting the time that is left. Returns false, doing nothing, once fired.</summary>
        // A plain method, not Unity's Start message: this class is not a MonoBehaviour.
        public bool Start(float now)
        {
            if (Fired)
            {
                return false;
            }
            _startedAt = now;
            Running = true;
            return true;
        }

        /// <summary>Stops counting and keeps the time that is left for the next Start.</summary>
        public void Stop(float now)
        {
            if (Running)
            {
                _remaining = Remaining(now);
                Running = false;
            }
        }

        /// <summary>Marks the countdown as done; it will not start again until Reset.</summary>
        public void Fire()
        {
            Running = false;
            _remaining = 0f;
            Fired = true;
        }

        /// <summary>Back to the full duration, not fired.</summary>
        public void Reset()
        {
            Running = false;
            Fired = false;
            _remaining = _duration;
        }
    }
}
