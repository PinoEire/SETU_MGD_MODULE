using System.Collections.Generic;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>The device tiers, numbered as the quality levels are: Low is level 0.</summary>
    public enum Tier
    {
        Low = 0,
        Mid = 1,
        High = 2
    }

    /// <summary>
    /// Picks a quality level for this phone once, before the first scene loads:
    /// High for 8 GB of RAM and 8 cores or more, Mid for 4 GB and 6 cores, Low
    /// below that. A tier saved by the Tiers sample's override wins over the
    /// detected one, and applies from the next launch. Each tier has its own frame
    /// cap (Low 30, Mid and High 60) and its own URP asset with the tier's render
    /// scale; <see cref="PerformanceMode"/> then applies the player's Balanced or
    /// High FPS choice on top, and High FPS only takes effect on the High tier.
    ///
    /// The lab sheet sets the order with Script Execution Order (DeviceTier at
    /// -100, PerformanceMode at -50). Here both are static, so the order is
    /// written down instead: this applies the tier, then calls
    /// <see cref="PerformanceMode.ApplySaved"/>. It needs the three quality levels
    /// named Low, Mid and High in that order (menu MGD Samples > Set Up Quality
    /// Tiers); with any other set it logs a warning and leaves the level alone.
    /// In the editor the quality level is put back when Play mode ends.
    /// </summary>
    public static class DeviceTier
    {
        public const string OverrideKey = "tier_override";

        /// <summary>The quality level names, in tier order; the set-up menu creates exactly these.</summary>
        public static readonly IReadOnlyList<string> LevelNames = new[] { "Low", "Mid", "High" };

        /// <summary>
        /// The tier in use. High until start-up has first run; with domain reloading
        /// off, the editor then keeps the last Play session's tier, so EditMode tests
        /// work with whatever tier this is.
        /// </summary>
        public static Tier Current { get; private set; } = Tier.High;

        /// <summary>What the hardware alone would pick, whatever the override says.</summary>
        public static Tier Detected { get; private set; } = Tier.High;

        /// <summary>True when the saved override, not the hardware, chose <see cref="Current"/>.</summary>
        public static bool IsOverridden { get; private set; }

        /// <summary>The deck's rule: RAM in MB and logical cores to a tier.</summary>
        public static Tier Detect(int ramMb, int cores)
        {
            if (ramMb >= 8192 && cores >= 8)
            {
                return Tier.High;
            }

            if (ramMb >= 4096 && cores >= 6)
            {
                return Tier.Mid;
            }

            return Tier.Low;
        }

        /// <summary>True when <paramref name="value"/> is a tier's number, 0 to 2.</summary>
        public static bool IsTier(int value)
        {
            return value >= (int)Tier.Low && value <= (int)Tier.High;
        }

        /// <summary>The saved override when it names a tier; otherwise the detected tier.</summary>
        public static Tier Resolve(Tier detected, bool hasOverride, int overrideValue)
        {
            return hasOverride && IsTier(overrideValue) ? (Tier)overrideValue : detected;
        }

        public static int FrameRateFor(Tier tier)
        {
            return tier == Tier.Low ? 30 : 60;
        }

        /// <summary>The render scale each tier's URP asset is set up with.</summary>
        public static float RenderScaleFor(Tier tier)
        {
            switch (tier)
            {
                case Tier.Low:
                    return 0.75f;
                case Tier.Mid:
                    return 0.9f;
                default:
                    return 1f;
            }
        }

        /// <summary>True when the quality levels are exactly Low, Mid, High, in that order.</summary>
        public static bool LevelsMatch(string[] names)
        {
            if (names == null || names.Length != LevelNames.Count)
            {
                return false;
            }

            for (int i = 0; i < LevelNames.Count; i++)
            {
                if (names[i] != LevelNames[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>The override dropdown's option for a saved key: 0 is Auto, 1 to 3 are Low to High.</summary>
        public static int OverrideOption(bool hasOverride, int saved)
        {
            return hasOverride && IsTier(saved) ? saved + 1 : 0;
        }

        /// <summary>The dropdown's current option, read from PlayerPrefs.</summary>
        public static int SavedOverrideOption()
        {
            return OverrideOption(PlayerPrefs.HasKey(OverrideKey), PlayerPrefs.GetInt(OverrideKey));
        }

        /// <summary>
        /// Saves the override from the dropdown (0 Auto, 1 to 3 Low to High). It
        /// takes effect on the next launch: switching quality level mid-session
        /// would reload the pipeline asset under the running scene.
        /// </summary>
        public static void SetOverride(int option)
        {
            if (option <= 0)
            {
                PlayerPrefs.DeleteKey(OverrideKey);
            }
            else
            {
                // Option 1 is Low (tier 0); anything past High clamps to High.
                PlayerPrefs.SetInt(OverrideKey, Mathf.Min(option - 1, (int)Tier.High));
            }

            PlayerPrefs.Save();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ApplyAtStartup()
        {
            Detected = Detect(SystemInfo.systemMemorySize, SystemInfo.processorCount);
            bool hasOverride = PlayerPrefs.HasKey(OverrideKey);
            int saved = PlayerPrefs.GetInt(OverrideKey);
            Current = Resolve(Detected, hasOverride, saved);
            IsOverridden = hasOverride && IsTier(saved);

            if (LevelsMatch(QualitySettings.names))
            {
                RememberEditorLevel();
                QualitySettings.SetQualityLevel((int)Current, true);
            }
            else
            {
                Debug.LogWarning("[Tier] The quality levels are not Low, Mid, High; run MGD Samples > Set Up Quality Tiers. Keeping the current level.");
            }

            Debug.Log($"tier={Current} detected={Detected} override={IsOverridden} ram={SystemInfo.systemMemorySize}MB cores={SystemInfo.processorCount}");

            // The tier first, then the player's mode on top of it.
            PerformanceMode.ApplySaved();
        }

#if UNITY_EDITOR
        static bool s_restoreQueued;
        static int s_editorLevel;

        // The quality level is a project setting in the editor; put it back after
        // Play so a Low phone's tier is not left selected in Project Settings.
        static void RememberEditorLevel()
        {
            if (s_restoreQueued || !Application.isPlaying)
            {
                return;
            }

            s_restoreQueued = true;
            s_editorLevel = QualitySettings.GetQualityLevel();
            UnityEditor.EditorApplication.playModeStateChanged += RestoreEditorLevel;
        }

        static void RestoreEditorLevel(UnityEditor.PlayModeStateChange state)
        {
            if (state != UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            UnityEditor.EditorApplication.playModeStateChanged -= RestoreEditorLevel;
            s_restoreQueued = false;
            QualitySettings.SetQualityLevel(s_editorLevel, true);
        }
#else
        static void RememberEditorLevel()
        {
        }
#endif
    }
}
