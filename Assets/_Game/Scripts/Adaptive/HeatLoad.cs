using TMPro;
using UnityEngine;

namespace MGD.Samples
{
    public enum HeatLevel
    {
        Off,
        Warm,
        Hot
    }

    /// <summary>
    /// A steady GPU load to warm the phone for the soak: layers of faint (5% alpha)
    /// full-screen sprites, slowly turning, so every pixel is blended once per
    /// layer (overdraw). Render scale matters to this load, which is the point:
    /// High FPS at scale 0.8 shades about a third fewer pixels per frame, and the
    /// bottleneck line may change with it.
    ///
    /// Every layer is created once in Awake and switched on and off by
    /// <see cref="Cycle"/>, so the steady state never calls Instantiate or Destroy.
    /// Tune the layer counts per phone in the Inspector: Hot should hold the frame
    /// rate at first and lose it after some minutes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HeatLoad : MonoBehaviour
    {
        [SerializeField] Sprite sprite;
        [SerializeField] Material material;

        [Tooltip("Layers drawn at Warm.")]
        [SerializeField, Min(0)] int warmLayers = 8;

        [Tooltip("Layers drawn at Hot. This many are created at load.")]
        [SerializeField, Min(1)] int hotLayers = 24;

        [Tooltip("How fast the layers turn; it only shows the load is live.")]
        [SerializeField, Min(0f)] float degreesPerSecond = 12f;

        [Tooltip("The cycle button's label, which names the level.")]
        [SerializeField] TMP_Text buttonLabel;

        [Tooltip("The level the scene opens at.")]
        [SerializeField] HeatLevel startLevel = HeatLevel.Hot;

        GameObject[] _layers;
        Transform[] _transforms;
        int _active;

        public HeatLevel Level { get; private set; }

        void Awake()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[Heat] No camera tagged MainCamera in the scene; the layers are sized from it.", this);
                enabled = false;
                return;
            }

            // Big enough to cover the view while turning, in either orientation: the
            // diagonal of the view at the screen's longest-to-shortest ratio.
            float longRatio = Mathf.Max(Screen.width, Screen.height) / (float)Mathf.Max(1, Mathf.Min(Screen.width, Screen.height));
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * longRatio;
            float cover = 2f * Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight);
            float scale = cover / sprite.bounds.size.x;

            _layers = new GameObject[hotLayers];
            _transforms = new Transform[hotLayers];
            for (int i = 0; i < hotLayers; i++)
            {
                var go = new GameObject("Layer");
                go.transform.SetParent(transform, false);
                go.transform.localScale = new Vector3(scale, scale, 1f);
                go.transform.localRotation = Quaternion.Euler(0f, 0f, i * 360f / hotLayers);

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                if (material != null)
                {
                    renderer.sharedMaterial = material;
                }

                // Faint, so the HUD stays readable over the whole stack; a faint
                // layer costs the GPU the same blend as an opaque-looking one.
                Color colour = Color.HSVToRGB((float)i / hotLayers, 0.6f, 1f);
                colour.a = 0.05f;
                renderer.color = colour;
                renderer.sortingOrder = i;

                _layers[i] = go;
                _transforms[i] = go.transform;
                go.SetActive(false);
            }

            SetLevel(startLevel);
        }

        /// <summary>Wired to the Load button's OnClick: Off, Warm, Hot, Off.</summary>
        public void Cycle()
        {
            HeatLevel next = Level switch
            {
                HeatLevel.Off => HeatLevel.Warm,
                HeatLevel.Warm => HeatLevel.Hot,
                _ => HeatLevel.Off
            };
            SetLevel(next);
        }

        void SetLevel(HeatLevel level)
        {
            Level = level;
            int count = level switch
            {
                HeatLevel.Warm => Mathf.Min(warmLayers, hotLayers),
                HeatLevel.Hot => hotLayers,
                _ => 0
            };

            for (int i = 0; i < _layers.Length; i++)
            {
                bool on = i < count;
                if (_layers[i].activeSelf != on)
                {
                    _layers[i].SetActive(on);
                }
            }

            _active = count;
            Debug.Log($"[Heat] level={level} layers={count}");
            if (buttonLabel != null)
            {
                buttonLabel.text = $"Load: {level} ({count} layers)";
            }
        }

        void Update()
        {
            // Turning is only there to show the load is live; the drawing, and the
            // heat, carry on under the pause panel either way.
            if (LifecycleGuard.IsPaused)
            {
                return;
            }

            float step = degreesPerSecond * Time.deltaTime;
            for (int i = 0; i < _active; i++)
            {
                // Alternate directions so the overlap pattern keeps changing.
                _transforms[i].Rotate(0f, 0f, (i & 1) == 0 ? step : -step);
            }
        }
    }
}
