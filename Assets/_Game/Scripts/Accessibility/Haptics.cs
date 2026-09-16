using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// One haptic pulse behind a player setting. Call <see cref="Pulse"/> from
    /// exactly one meaningful event (a hit landed, a level-up, a piece placed),
    /// never from Update. Unity adds the VIBRATE permission to the Android
    /// manifest automatically because <c>Handheld.Vibrate</c> is referenced.
    /// </summary>
    public static class Haptics
    {
        const string Key = "MGD.Haptics";

        /// <summary>Player setting, on by default, persisted in PlayerPrefs.</summary>
        public static bool Enabled
        {
            get => PlayerPrefs.GetInt(Key, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(Key, value ? 1 : 0);
                PlayerPrefs.Save();
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
