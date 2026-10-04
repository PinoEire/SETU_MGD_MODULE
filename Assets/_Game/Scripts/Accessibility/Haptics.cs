using System;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// One haptic pulse behind a player setting. Call <see cref="Pulse"/> from
    /// exactly one meaningful event (a hit landed, a level-up, a piece placed),
    /// never from Update. Unity adds the VIBRATE permission to the Android
    /// manifest automatically because <c>Handheld.Vibrate</c> is referenced.
    ///
    /// Same shape as <see cref="MotionSetting"/> and <see cref="TextScale"/>: the
    /// value is read from PlayerPrefs once and cached, the setter writes through
    /// and raises <see cref="Changed"/> only when the value really changes.
    /// </summary>
    public static class Haptics
    {
        const string Key = "MGD.Haptics";

        static bool _loaded;
        static bool _enabled;

        /// <summary>Raised after <see cref="Enabled"/> changes.</summary>
        public static event Action Changed;

        /// <summary>Player setting, on by default, persisted in PlayerPrefs.</summary>
        public static bool Enabled
        {
            get
            {
                if (!_loaded)
                {
                    _enabled = PlayerPrefs.GetInt(Key, 1) == 1;
                    _loaded = true;
                }

                return _enabled;
            }
            set
            {
                if (value == Enabled)
                {
                    return;
                }

                _enabled = value;
                PlayerPrefs.SetInt(Key, value ? 1 : 0);
                PlayerPrefs.Save();
                Changed?.Invoke();
            }
        }

        public static void Pulse()
        {
            if (!Enabled)
            {
                return;
            }

            // One fixed pulse; no duration or intensity control. Does nothing in
            // the editor or on desktop, so it is safe to call everywhere.
            Handheld.Vibrate();
        }
    }
}
