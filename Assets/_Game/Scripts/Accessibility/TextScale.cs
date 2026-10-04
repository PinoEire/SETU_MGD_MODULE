using System;
using TMPro;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Player-chosen text size. Add to every TextMeshPro text that players read
    /// (HUD numbers, menu labels, dialogue) with Auto Size off, or the component's
    /// size is overridden. Each instance remembers its own base size and applies
    /// the shared <see cref="Factor"/> on top, so labels keep their relative sizes.
    ///
    /// Same shape as <see cref="Haptics"/> and <see cref="MotionSetting"/>: the
    /// value is read from PlayerPrefs once and cached, the setter writes through
    /// and raises <see cref="Changed"/> only when the value really changes.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    [DisallowMultipleComponent]
    public sealed class TextScale : MonoBehaviour
    {
        const string Key = "MGD.TextScale";

        public const float Small = 0.85f;
        public const float Normal = 1f;
        public const float Large = 1.25f;

        static bool _loaded;
        static float _factor = Normal;

        /// <summary>Raised after <see cref="Factor"/> changes; every instance re-applies.</summary>
        public static event Action Changed;

        /// <summary>Multiplier on each label's base size, persisted in PlayerPrefs.</summary>
        public static float Factor
        {
            get
            {
                if (!_loaded)
                {
                    _factor = PlayerPrefs.GetFloat(Key, Normal);
                    _loaded = true;
                }

                return _factor;
            }
            set
            {
                value = Mathf.Clamp(value, 0.5f, 2f);
                if (Mathf.Approximately(value, Factor))
                {
                    return;
                }

                _factor = value;
                PlayerPrefs.SetFloat(Key, value);
                PlayerPrefs.Save();
                Changed?.Invoke();
            }
        }

        TMP_Text _text;
        float _baseSize;

        void Awake()
        {
            // TMP_Text is abstract, so the attribute cannot add one for you: this
            // goes on an object that already has a TextMeshProUGUI or TextMeshPro.
            _text = GetComponent<TMP_Text>();
            _baseSize = _text.fontSize;
        }

        void OnEnable()
        {
            Changed += Apply;
            Apply();
        }

        void OnDisable()
        {
            Changed -= Apply;
        }

        void Apply()
        {
            _text.fontSize = _baseSize * Factor;
        }
    }
}
