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
    /// The Balanced / High FPS setting. Balanced is the module default: 60 fps at
    /// full render scale. High FPS asks for 120 and drops the URP render scale to
    /// 0.8 to buy the GPU headroom the higher rate needs; a 60 Hz panel clamps the
    /// request to 60, which is fine. The choice is saved in PlayerPrefs and applied
    /// before the first scene loads, so every scene starts in the player's mode.
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

        public const int BalancedFrameRate = 60;
        public const int HighFrameRate = 120;
        public const float BalancedRenderScale = 1f;
        public const float HighRenderScale = 0.8f;

        /// <summary>The mode applied last.</summary>
        public static PerfMode Current { get; private set; }

        public static bool IsHighFps => Current == PerfMode.HighFps;

        public static int FrameRateFor(PerfMode mode)
        {
            return mode == PerfMode.HighFps ? HighFrameRate : BalancedFrameRate;
        }

        public static float RenderScaleFor(PerfMode mode)
        {
            return mode == PerfMode.HighFps ? HighRenderScale : BalancedRenderScale;
        }

        /// <summary>A saved value from an older or edited build that is not a known mode reads as Balanced.</summary>
        public static PerfMode FromSaved(int value)
        {
            return value == (int)PerfMode.HighFps ? PerfMode.HighFps : PerfMode.Balanced;
        }

        // Before the first scene's Awake, so the bootstrap's [Boot] line already
        // reports the player's target, and a scene opened directly in the editor
        // runs in the saved mode too.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ApplySaved()
        {
            ApplyWithoutSaving(FromSaved(PlayerPrefs.GetInt(Key, (int)PerfMode.Balanced)));
        }

        /// <summary>Applies the mode, saves it and logs a <c>perf_mode=</c> line for Logcat.</summary>
        public static void Apply(PerfMode mode)
        {
            ApplyWithoutSaving(mode);
            PlayerPrefs.SetInt(Key, (int)mode);
            PlayerPrefs.Save();
            Debug.Log(FormattableString.Invariant(
                $"perf_mode={mode} high={mode == PerfMode.HighFps} target={FrameRateFor(mode)} scale={RenderScaleFor(mode):0.00}"));
        }

        /// <summary>The toggle-friendly form: on is High FPS, off is Balanced.</summary>
        public static void SetHighFps(bool on)
        {
            Apply(on ? PerfMode.HighFps : PerfMode.Balanced);
        }

        static void ApplyWithoutSaving(PerfMode mode)
        {
            Current = mode;
            Application.targetFrameRate = FrameRateFor(mode);

            // currentRenderPipeline honours a Quality-level override, which is where
            // an Android quality level may point at a different asset.
            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)
            {
                RememberEditorScale(urp);
                urp.renderScale = RenderScaleFor(mode);
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
