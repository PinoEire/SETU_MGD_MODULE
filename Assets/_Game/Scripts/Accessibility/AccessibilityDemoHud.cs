using TMPro;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Demo scaffolding for the Accessibility scene. Not something to copy into a
    /// game; it makes the three settings and two of the accessibility checks
    /// visible on the phone:
    ///
    /// - a Hit button that fires <see cref="Haptics.Pulse"/> and reports the result
    ///   as text, so the state is readable with haptics off and in monochrome;
    /// - a square that shakes unless <see cref="MotionSetting.ReduceMotion"/> is on;
    /// - a reference square sized at runtime to exactly 48 dp, the minimum touch
    ///   target, with the dp-to-pixel maths printed beside it;
    /// - a label showing the current text-size factor.
    /// </summary>
    public sealed class AccessibilityDemoHud : MonoBehaviour
    {
        const float MinTouchTargetDp = 48f;
        const float ShakeAmplitude = 14f;
        const float ShakeFrequency = 18f;

        [SerializeField] TMP_Text status;
        [SerializeField] RectTransform shaker;
        [SerializeField] RectTransform dpSquare;
        [SerializeField] TMP_Text dpLabel;
        [SerializeField] TMP_Text sizeLabel;

        int _hits;
        Vector2 _shakerHome;
        Canvas _canvas;

        void Awake()
        {
            _shakerHome = shaker.anchoredPosition;
            _canvas = GetComponentInParent<Canvas>();
        }

        void OnEnable()
        {
            TextScale.Changed += ShowTextSize;
        }

        void OnDisable()
        {
            TextScale.Changed -= ShowTextSize;
        }

        // Start, not Awake: TextMeshPro on the labels may not have initialised yet.
        void Start()
        {
            status.text = "Tap Hit to test haptics";
            SizeDpSquare();
            ShowTextSize();
        }

        void Update()
        {
            // Motion effects check the setting every frame rather than caching it,
            // so the toggle takes effect immediately.
            if (MotionSetting.ReduceMotion)
            {
                shaker.anchoredPosition = _shakerHome;
                return;
            }

            float offset = Mathf.Sin(Time.time * ShakeFrequency) * ShakeAmplitude;
            shaker.anchoredPosition = _shakerHome + new Vector2(offset, 0f);
        }

        /// <summary>Wired to the Hit button's OnClick. The one meaningful event.</summary>
        public void OnHitPressed()
        {
            _hits++;
            Haptics.Pulse();

            // Words, not colour: readable in monochrome, with sound off and with
            // haptics off. SetText with an argument does not allocate a string.
            status.SetText(Haptics.Enabled ? "HIT {0:0}  (haptic pulse sent)" : "HIT {0:0}  (haptics are off)", _hits);
        }

        void SizeDpSquare()
        {
            // 1 dp is 1 px at 160 dpi. Screen.dpi can be 0 in the editor; assume a
            // 160 dpi screen then. The Canvas Scaler multiplies canvas units by
            // scaleFactor to get pixels, so divide it back out.
            float dpi = Screen.dpi > 0f ? Screen.dpi : 160f;
            float pixels = MinTouchTargetDp * dpi / 160f;
            float units = pixels / _canvas.scaleFactor;
            dpSquare.sizeDelta = new Vector2(units, units);

            dpLabel.SetText("48 dp = {0} px at {1} dpi", Mathf.Round(pixels), Mathf.Round(dpi));
        }

        void ShowTextSize()
        {
            float factor = TextScale.Factor;
            string name = Mathf.Approximately(factor, TextScale.Small) ? "Small"
                : Mathf.Approximately(factor, TextScale.Large) ? "Large"
                : "Normal";
            sizeLabel.text = $"Text size: {name} (x{factor:0.00})";
        }
    }
}
