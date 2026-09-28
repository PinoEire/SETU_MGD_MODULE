using TMPro;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Demo scaffolding for the Performance scene. Not something to copy into a
    /// game; it wires the four buttons and keeps one status line current: load
    /// state and sprite count, the frame target, the panel refresh rate Android
    /// chose, and the render scale. The status is rebuilt only when a button is
    /// pressed, so nothing here allocates per frame. The Probe button is shown
    /// only in development builds, as the lab sheet asks.
    /// </summary>
    public sealed class PerformanceDemoHud : MonoBehaviour
    {
        [SerializeField] LoadGenerator load;
        [SerializeField] FrameTimeSampler sampler;
        [SerializeField] RenderScaleProbe probe;
        [SerializeField] TMP_Text status;
        [SerializeField] GameObject probeButton;

        // Start, not Awake: LoadGenerator.Awake must have run so ActiveCount is valid.
        void Start()
        {
            probeButton.SetActive(Debug.isDebugBuild);
            RefreshStatus();
        }

        public void OnIdlePressed()
        {
            SetLoad(LoadState.Idle);
        }

        public void OnSteadyPressed()
        {
            SetLoad(LoadState.Steady);
        }

        public void OnWorstPressed()
        {
            SetLoad(LoadState.Worst);
        }

        public void OnProbePressed()
        {
            probe.Toggle();
            RefreshStatus();
        }

        void SetLoad(LoadState state)
        {
            load.SetLoad(state);
            // The first [Baseline] line after a change should measure only the new state.
            sampler.Restart();
            RefreshStatus();
        }

        void RefreshStatus()
        {
            // Android picks the panel mode; most games get 60 Hz even on a 120 Hz
            // phone unless they ask for more (Week 7). targetFrameRate reads -1 if
            // the scene was opened directly: the bootstrap lives in the Launcher.
            double panelHz = Screen.currentResolution.refreshRateRatio.value;
            status.text =
                $"{load.State}, {load.ActiveCount} sprites  |  target {Application.targetFrameRate} fps\n" +
                $"panel {panelHz:0} Hz  |  render scale {probe.CurrentScale:0.00}";
        }
    }
}
