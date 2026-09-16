using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// The "reduce motion" player setting: screen shake, camera bob, parallax
    /// wobble and similar effects check <see cref="ReduceMotion"/> before moving
    /// anything. The accessibility pass asks for the toggle to exist from Week 2
    /// even if the game has no motion effects yet; storing the value is the whole
    /// job until then.
    /// </summary>
    public static class MotionSetting
    {
        const string Key = "MGD.ReduceMotion";

        /// <summary>Off by default, persisted in PlayerPrefs.</summary>
        public static bool ReduceMotion
        {
            get => PlayerPrefs.GetInt(Key, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(Key, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}
