using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// When and from which commit the running build was made: the Week 12 Lab A
    /// Build Info asset. A phone has no git and no build date, so both are written
    /// into this asset in the editor just before every build (by
    /// <c>BuildInfoStamper</c>, in <c>Editor/About/</c>) and travel inside the APK.
    /// </summary>
    [CreateAssetMenu(menuName = "MGD Samples/Build Info")]
    public sealed class BuildInfo : ScriptableObject
    {
        [Tooltip("UTC date of the last build, yyyy-MM-dd. Written by the build step.")]
        public string buildDate = "";

        [Tooltip("Short hash of the commit the last build was made from. Written by the build step.")]
        public string gitHash = "";
    }
}
