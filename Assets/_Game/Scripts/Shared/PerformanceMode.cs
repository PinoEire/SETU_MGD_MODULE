using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MGD.Samples
{
    public enum PerfMode
    {
        Balanced = 0,
        HighFps = 1
    }

    /// <summary>
    /// The Balanced / High FPS setting, applied on top of the <see cref="DeviceTier"/>.
    /// Balanced runs at the tier's own frame cap and render scale (Low 30 fps at
    /// 0.75, Mid 60 at 0.9, High 60 at 1.0). High FPS asks for 120 and drops the
    /// URP render scale to 0.8 to buy the GPU headroom the higher rate needs, and
    /// only on the High tier: a Mid or Low phone has no headroom to spend, so there
    /// it runs as Balanced. A 60 Hz panel clamps the request to 60, which is fine.
    /// The player's choice is saved in PlayerPrefs whatever the tier, so it takes
    /// effect if the tier later becomes High; <see cref="Current"/> is the mode
    /// actually running. <see cref="DeviceTier"/> applies it at start-up, after
    /// the tier, so every scene starts in the player's mode.
    ///
    /// It is static, not a component, because everything it changes is already
    /// global: <c>Application.targetFrameRate</c>, the active URP asset and the
    /// saved key. A static method cannot be picked in a UnityEvent, so the pause
    /// menu's High FPS toggle calls <c>PauseMenu.OnHighFpsChanged</c>, which
    /// forwards to <see cref="SetHighFps"/>.
    /// In the editor the URP asset is the project asset, so the render scale is put
    /// back when Play mode ends and never reaches git.
    /// </summary>
    public static class PerformanceMode
    {
        public const string Key = "perf_mode";

        public const int HighFrameRate = 120;
        public const float HighRenderScale = 0.8f;

        /// <summary>The mode running now: High FPS only when the tier allows it.</summary>
        public static PerfMode Current { get; private set; }

        public static bool IsHighFps => Current == PerfMode.HighFps;

        /// <summary>True when the current tier lets High FPS take effect.</summary>
        public static bool IsHighFpsAllowed => HighFpsAllowedOn(DeviceTier.Current);

        /// <summary>The render scale the current mode and tier call for.</summary>
        public static float CurrentRenderScale => RenderScaleFor(Current, DeviceTier.Current);

        public static bool HighFpsAllowedOn(Tier tier)
        {
            return tier == Tier.High;
        }

        /// <summary>The mode that runs when <paramref name="requested"/> is chosen on <paramref name="tier"/>.</summary>
        public static PerfMode Effective(PerfMode requested, Tier tier)
        {
            return requested == PerfMode.HighFps && HighFpsAllowedOn(tier) ? PerfMode.HighFps : PerfMode.Balanced;
        }

        public static int FrameRateFor(PerfMode requested, Tier tier)
        {
            return Effective(requested, tier) == PerfMode.HighFps ? HighFrameRate : DeviceTier.FrameRateFor(tier);
        }

        public static float RenderScaleFor(PerfMode requested, Tier tier)
        {
            return Effective(requested, tier) == PerfMode.HighFps ? HighRenderScale : DeviceTier.RenderScaleFor(tier);
        }

        /// <summary>A saved value from an older or edited build that is not a known mode reads as Balanced.</summary>
        public static PerfMode FromSaved(int value)
        {
            return value == (int)PerfMode.HighFps ? PerfMode.HighFps : PerfMode.Balanced;
        }

        /// <summary>
        /// Applies the saved choice. <see cref="DeviceTier"/> calls it before the
        /// first scene loads, after setting the tier, so the bootstrap's [Boot] line
        /// already reports the target, and a scene opened directly in the editor
        /// runs in the saved mode too.
        /// </summary>
        public static void ApplySaved()
        {
            ApplyWithoutSaving(FromSaved(PlayerPrefs.GetInt(Key, (int)PerfMode.Balanced)));
        }

        /// <summary>
        /// Saves the player's choice, applies what the tier allows of it and logs a
        /// <c>perf_mode=</c> line for Logcat with the mode that actually runs.
        /// </summary>
        public static void Apply(PerfMode mode)
        {
            ApplyWithoutSaving(mode);
            PlayerPrefs.SetInt(Key, (int)mode);
            PlayerPrefs.Save();
            Tier tier = DeviceTier.Current;
            Debug.Log(FormattableString.Invariant(
                $"perf_mode={Current} high={Current == PerfMode.HighFps} target={FrameRateFor(Current, tier)} scale={RenderScaleFor(Current, tier):0.00} tier={tier}"));
        }

        /// <summary>The toggle-friendly form: on is High FPS, off is Balanced.</summary>
        public static void SetHighFps(bool on)
        {
            Apply(on ? PerfMode.HighFps : PerfMode.Balanced);
        }

        static void ApplyWithoutSaving(PerfMode requested)
        {
            Tier tier = DeviceTier.Current;
            Current = Effective(requested, tier);
            Application.targetFrameRate = FrameRateFor(Current, tier);

            // currentRenderPipeline is the active quality level's URP asset, which
            // is the tier's: each tier has its own asset.
            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)
            {
                RememberEditorScale(urp);
                urp.renderScale = RenderScaleFor(Current, tier);
            }
        }

#if UNITY_EDITOR
        static UniversalRenderPipelineAsset s_editedAsset;
        static float s_editorScale;

        // The asset outlives Play mode in the editor. Restore it once everything in
        // the scene has been destroyed, so a component that also puts a scale back
        // on destroy (the Performance sample's probe) cannot undo the restore. A
        // script recompile during Play drops this hook; stop Play first.
        static void RememberEditorScale(UniversalRenderPipelineAsset urp)
        {
            if (s_editedAsset != null || !Application.isPlaying)
            {
                return;
            }

            s_editedAsset = urp;
            s_editorScale = urp.renderScale;
            UnityEditor.EditorApplication.playModeStateChanged += RestoreEditorScale;
        }

        static void RestoreEditorScale(UnityEditor.PlayModeStateChange state)
        {
            if (state != UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            UnityEditor.EditorApplication.playModeStateChanged -= RestoreEditorScale;
            if (s_editedAsset != null)
            {
                s_editedAsset.renderScale = s_editorScale;
            }

            s_editedAsset = null;
        }
#else
        static void RememberEditorScale(UniversalRenderPipelineAsset urp)
        {
        }
#endif
    }
}
