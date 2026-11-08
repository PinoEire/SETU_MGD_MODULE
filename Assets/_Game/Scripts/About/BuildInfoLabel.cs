using TMPro;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// The About screen's build date and commit line, from the <see cref="BuildInfo"/>
    /// asset. It sits under <see cref="BuildStamp"/>, which already shows what the
    /// lab's AboutScreen shows apart from the date: title, version, build ID and device.
    /// </summary>
    public sealed class BuildInfoLabel : MonoBehaviour
    {
        [SerializeField] BuildInfo info;
        [SerializeField] TMP_Text label;

        // OnEnable, as in the lab: an About panel that is opened and closed refreshes
        // its line each time it opens. BuildStamp writes once in Start because its
        // values never change while the app runs.
        void OnEnable()
        {
            if (info == null || label == null)
            {
                Debug.LogError("[BuildInfo] Assign the BuildInfo asset and the label on BuildInfoLabel.", this);
                return;
            }
            label.text = Format(info.buildDate, info.gitHash);
        }

        /// <summary>"Built 2026-11-30 from 3669742", or a note that nothing has been built yet.</summary>
        public static string Format(string buildDate, string gitHash)
        {
            if (string.IsNullOrEmpty(buildDate))
            {
                return "Not built yet: the build step fills this in";
            }
            return string.IsNullOrEmpty(gitHash) ? $"Built {buildDate}" : $"Built {buildDate} from {gitHash}";
        }
    }
}
