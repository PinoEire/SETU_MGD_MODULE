using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MGD.Samples
{
    /// <summary>
    /// The Tiers sample's screen: what <see cref="DeviceTier"/> picked and why,
    /// what that quality level changes, and the override dropdown (Auto, Low,
    /// Mid, High) that Week 9 Lab A puts in the settings menu. The override is
    /// saved at once and applies from the next launch, which the note under the
    /// dropdown says. The readout is written in <c>Start</c> and again on each
    /// resume, because the pause card's High FPS toggle can change the frame cap
    /// and render scale. Pausing closes an open dropdown list, which would
    /// otherwise sit above the pause card. The turning bar shows MSAA on its
    /// edges; the checker texture shows the Low tier's half-resolution textures.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TierReadout : MonoBehaviour
    {
        [SerializeField] TMP_Text readout;
        [SerializeField] TMP_Dropdown overrideDropdown;
        [SerializeField] TMP_Text overrideNote;
        [SerializeField] Transform spinner;
        [SerializeField, Min(0f)] float degreesPerSecond = 20f;

        void OnEnable()
        {
            LifecycleGuard.PausedChanged += OnPausedChanged;
        }

        void OnDisable()
        {
            LifecycleGuard.PausedChanged -= OnPausedChanged;
        }

        // Start, not Awake: the TextMeshPro labels may not have initialised yet.
        void Start()
        {
            Refresh();

            // Without notifying: the dropdown shows the saved choice, it does not make one.
            overrideDropdown.SetValueWithoutNotify(DeviceTier.SavedOverrideOption());
            overrideNote.text = OverrideNote(overrideDropdown.value, DeviceTier.Current);
        }

        void OnPausedChanged(bool paused)
        {
            if (paused)
            {
                overrideDropdown.Hide();
            }
            else
            {
                Refresh();
            }
        }

        void Refresh()
        {
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            readout.text = Describe(DeviceTier.Current, DeviceTier.Detected, DeviceTier.IsOverridden,
                SystemInfo.systemMemorySize, SystemInfo.processorCount,
                QualitySettings.names[QualitySettings.GetQualityLevel()],
                urp != null ? urp.renderScale : 1f, urp != null ? urp.msaaSampleCount : 1,
                Application.targetFrameRate, QualitySettings.globalTextureMipmapLimit);
        }

        void Update()
        {
            if (!LifecycleGuard.IsPaused)
            {
                spinner.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime);
            }
        }

        /// <summary>Wired to the override dropdown's On Value Changed (dynamic int).</summary>
        public void OnOverrideChanged(int option)
        {
            DeviceTier.SetOverride(option);
            overrideNote.text = OverrideNote(option, DeviceTier.Current);
            Debug.Log($"[Tier] override option={overrideDropdown.options[option].text}");
        }

        /// <summary>The four readout lines; allocates, so it is called at start and on resume only.</summary>
        public static string Describe(Tier current, Tier detected, bool overridden, int ramMb, int cores,
            string qualityLevel, float renderScale, int msaa, int targetFps, int mipmapLimit)
        {
            string why = overridden
                ? $"override; detected {detected}: {ramMb} MB RAM, {cores} cores"
                : $"detected: {ramMb} MB RAM, {cores} cores";
            string aa = msaa > 1 ? $"{msaa}x" : "off";
            string textures = mipmapLimit == 0 ? "full" : mipmapLimit == 1 ? "half" : $"1/{1 << mipmapLimit}";
            return FormattableString.Invariant(
                $"Tier {current} ({why})\nQuality level {qualityLevel}\nRender scale {renderScale:0.00}  MSAA {aa}  {targetFps} fps\nTextures at {textures} resolution");
        }

        /// <summary>What the dropdown's choice will do, given the tier running now.</summary>
        public static string OverrideNote(int option, Tier current)
        {
            if (option <= 0)
            {
                return "Auto: the next launch detects the tier again.";
            }

            var chosen = (Tier)(option - 1);
            return chosen == current ? $"{chosen}: already running." : $"{chosen} from the next launch.";
        }
    }
}
