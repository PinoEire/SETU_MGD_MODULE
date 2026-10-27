using System;
using TMPro;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// The closed-test feedback form on the About screen: the link as text, so a
    /// tester can read it out, and a button that opens it in the browser. Paste
    /// the form's short link into <see cref="url"/> and rebuild; every build
    /// should ask for feedback on itself, which is why the form asks for the
    /// build ID shown above it. With no link set, a build hides both, and the
    /// editor shows a reminder instead. Opening a link hands it to the browser,
    /// so the app itself needs no <c>INTERNET</c> permission.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FeedbackLink : MonoBehaviour
    {
        [Tooltip("The feedback form's short link, for example https://forms.office.com/r/...")]
        [SerializeField] string url;
        [SerializeField] TMP_Text label;
        [SerializeField] GameObject openButton;

        // Start, not Awake: the TextMeshPro label may not have initialised yet.
        void Start()
        {
            // A link copied from a form often brings a space with it.
            url = url == null ? "" : url.Trim();
            bool hasLink = url.Length > 0;
            openButton.SetActive(hasLink);

            if (hasLink)
            {
                label.text = $"Feedback: {url}";
                // Without a scheme Android finds no app to open it, and the tap does nothing.
                if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning($"[About] The feedback link '{url}' should start with https://", this);
                }
            }
            else if (Application.isEditor)
            {
                label.text = "Feedback: set the form link on FeedbackLink";
            }
            else
            {
                label.gameObject.SetActive(false);
            }
        }

        /// <summary>Wired to the Open feedback form button's OnClick.</summary>
        public void OnOpenPressed()
        {
            Application.OpenURL(url);
        }
    }
}
