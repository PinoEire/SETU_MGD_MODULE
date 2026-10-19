using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MGD.Samples
{
    /// <summary>
    /// Drops the URP render scale to <c>lowScale</c> (0.5) on demand. Frame time
    /// drops by a third or more: the GPU was the bottleneck. It barely moves: the
    /// CPU was. Now you know where to look. The change is made on the live
    /// pipeline asset, which in the editor is the project asset itself, so
    /// <c>OnDestroy</c> puts the scale back.
    /// "Back" means the current <see cref="PerformanceMode"/>'s scale, read when it
    /// is needed rather than remembered in Awake: the High FPS toggle on the pause
    /// card can change the mode while this scene is open.
    /// Bind <see cref="Toggle"/> to a debug button.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RenderScaleProbe : MonoBehaviour
    {
        [SerializeField, Range(0.3f, 1f)] float lowScale = 0.5f;

        UniversalRenderPipelineAsset _urp;
        bool _low;

        /// <summary>True while the probe holds the scale at <c>lowScale</c>.</summary>
        public bool IsLow => _low;

        /// <summary>The live render scale, or 1 when no URP asset is active.</summary>
        public float CurrentScale => _urp != null ? _urp.renderScale : 1f;

        void Awake()
        {
            // currentRenderPipeline honours a Quality-level override, which is where
            // an Android quality level may point at a different asset.
            _urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (_urp == null)
            {
                Debug.LogWarning("[Probe] No URP asset is active; the probe is disabled. " +
                                 "Check Project Settings > Quality > Render Pipeline Asset.");
                enabled = false;
            }
        }

        /// <summary>Wired to the Probe button's OnClick.</summary>
        public void Toggle()
        {
            if (_urp == null)
            {
                return;
            }

            _low = !_low;
            _urp.renderScale = _low ? lowScale : PerformanceMode.RenderScaleFor(PerformanceMode.Current);
            Debug.Log($"[Probe] renderScale = {_urp.renderScale}");
        }

        void OnDestroy()
        {
            // The asset outlives this scene (and, in the editor, Play mode).
            if (_low)
            {
                _urp.renderScale = PerformanceMode.RenderScaleFor(PerformanceMode.Current);
                _low = false;
            }
        }
    }
}
