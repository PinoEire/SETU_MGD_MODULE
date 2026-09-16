using UnityEngine;
using UnityEngine.UI;

namespace MGD.Samples
{
    /// <summary>
    /// Binds a Settings card to the three accessibility settings: the Haptics and
    /// Reduce-motion toggles and the Small / Normal / Large text-size buttons.
    /// Reads the saved values into the controls on Start, so the panel always
    /// shows what is actually in effect. The button for the current size is
    /// disabled, which greys it and also stops a pointless re-apply.
    /// </summary>
    public sealed class SettingsPanel : MonoBehaviour
    {
        [SerializeField] Toggle hapticsToggle;
        [SerializeField] Toggle reduceMotionToggle;
        [SerializeField] Button smallButton;
        [SerializeField] Button normalButton;
        [SerializeField] Button largeButton;

        void Start()
        {
            // SetIsOnWithoutNotify: initialising the toggle must not write the
            // value straight back to PlayerPrefs.
            hapticsToggle.SetIsOnWithoutNotify(Haptics.Enabled);
            reduceMotionToggle.SetIsOnWithoutNotify(MotionSetting.ReduceMotion);

            hapticsToggle.onValueChanged.AddListener(value => Haptics.Enabled = value);
            reduceMotionToggle.onValueChanged.AddListener(value => MotionSetting.ReduceMotion = value);

            smallButton.onClick.AddListener(() => TextScale.Factor = TextScale.Small);
            normalButton.onClick.AddListener(() => TextScale.Factor = TextScale.Normal);
            largeButton.onClick.AddListener(() => TextScale.Factor = TextScale.Large);

            ShowCurrentSize();
        }

        void OnEnable()
        {
            TextScale.Changed += ShowCurrentSize;
        }

        void OnDisable()
        {
            TextScale.Changed -= ShowCurrentSize;
        }

        void ShowCurrentSize()
        {
            float factor = TextScale.Factor;
            smallButton.interactable = !Mathf.Approximately(factor, TextScale.Small);
            normalButton.interactable = !Mathf.Approximately(factor, TextScale.Normal);
            largeButton.interactable = !Mathf.Approximately(factor, TextScale.Large);
        }
    }
}
