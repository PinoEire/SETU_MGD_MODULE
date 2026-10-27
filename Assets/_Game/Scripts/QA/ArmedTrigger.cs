namespace MGD.Samples
{
    /// <summary>
    /// A press that needs a second press within a time window: the first arms,
    /// the second fires. Guards a button whose action cannot be undone (here,
    /// crashing the app) against a stray tap. Pure logic, given the time, so it
    /// can be tested without a scene.
    /// </summary>
    public sealed class ArmedTrigger
    {
        readonly float _window;
        float _armedUntil = float.NegativeInfinity;

        public ArmedTrigger(float windowSeconds)
        {
            _window = windowSeconds;
        }

        /// <summary>True while a second press would fire.</summary>
        public bool IsArmed(float now)
        {
            return now <= _armedUntil;
        }

        /// <summary>Returns true when this press fires; otherwise it arms the trigger.</summary>
        public bool Press(float now)
        {
            if (IsArmed(now))
            {
                _armedUntil = float.NegativeInfinity;
                return true;
            }

            _armedUntil = now + _window;
            return false;
        }
    }
}
