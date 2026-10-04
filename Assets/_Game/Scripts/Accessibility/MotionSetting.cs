using System;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// The "reduce motion" player setting: screen shake, camera bob, parallax
    /// wobble and similar effects check <see cref="ReduceMotion"/> before moving
    /// anything. The accessibility pass asks for the toggle to exist from Week 2
    /// even if the game has no motion effects yet; storing the value is the whole
    /// job until then.
    ///
    /// The value is read from PlayerPrefs once and cached, so effects can check
    /// it every frame for free; the setter writes through and raises
    /// <see cref="Changed"/>, so anything that wants to react at once can.
    /// </summary>
    public static class MotionSetting
    {
        const string Key = "MGD.ReduceMotion";

        static bool _loaded;
        static bool _reduceMotion;

        /// <summary>Raised after <see cref="ReduceMotion"/> changes.</summary>
        public static event Action Changed;

        /// <summary>Off by default, persisted in PlayerPrefs.</summary>
        public static bool ReduceMotion
        {
            get
            {
                if (!_loaded)
                {
                    _reduceMotion = PlayerPrefs.GetInt(Key, 0) == 1;
                    _loaded = true;
                }

                return _reduceMotion;
            }
            set
            {
                if (value == ReduceMotion)
                {
                    return;
                }

                _reduceMotion = value;
                PlayerPrefs.SetInt(Key, value ? 1 : 0);
                PlayerPrefs.Save();
                Changed?.Invoke();
            }
        }
    }
}
