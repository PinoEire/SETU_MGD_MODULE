using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MGD.Samples.Editor.SampleSceneBuild;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Builds the About sample scene from a menu item. Every run assigns fresh
    /// object IDs, so run it only when this builder changes.
    /// </summary>
    public static class AboutSceneBuilder
    {
        const string BuilderName = "AboutSceneBuilder";
        const string ScenePath = "Assets/_Game/Scenes/About/About.unity";

        // 140 units is about 53 dp on the reference phone, above the 48 dp minimum.
        static readonly Vector2 ButtonSize = new Vector2(600f, 140f);

        [MenuItem("MGD Samples/Build About Scene")]
        public static void Build()
        {
            Scene? scene = BeginScene(BuilderName);
            if (scene == null)
            {
                return;
            }

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateHudRoot(canvas);

            TextMeshProUGUI title = CreateLabel(root, "Title", "About", 72f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 110f), stretchWidth: true);

            // Four lines at 40 units, about 15 sp on the reference phone.
            TextMeshProUGUI stamp = CreateLabel(root, "BuildLabel", "", 40f);
            Place(stamp.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 260f), stretchWidth: true, offset: new Vector2(0f, -140f));
            var buildStamp = root.gameObject.AddComponent<BuildStamp>();
            SetField(buildStamp, "label", stamp);

            TextMeshProUGUI hint = CreateLabel(root, "Hint",
                "Quote the build ID in your feedback:\nit names exactly one build.", 30f);
            hint.color = new Color(0.75f, 0.78f, 0.85f);
            Place(hint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 110f), stretchWidth: true, offset: new Vector2(0f, -430f));

            TextMeshProUGUI feedback = CreateLabel(root, "Feedback", "", 32f);
            Place(feedback.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 110f), stretchWidth: true, offset: new Vector2(0f, -580f));

            Button open = CreateButton(root, "Button Feedback", "Open feedback form", ButtonSize, 40f);
            Place(open.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), ButtonSize, offset: new Vector2(0f, -710f));

            var link = root.gameObject.AddComponent<FeedbackLink>();
            SetField(link, "label", feedback);
            SetField(link, "openButton", open.gameObject);
            // Persistent listener: what a student wires by hand in the Inspector.
            UnityEventTools.AddPersistentListener(open.onClick, new UnityAction(link.OnOpenPressed));

            TextMeshProUGUI footer = CreateLabel(root, "Footer",
                "Android back returns to the samples list.\nThere is no Quit button.", 28f, TextAlignmentOptions.Bottom);
            footer.color = new Color(0.75f, 0.78f, 0.85f);
            Place(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 100f), stretchWidth: true);

            // Android back returns to the launcher; this sample does not use back
            // itself, so the pause panel's own back toggle is off.
            canvas.gameObject.AddComponent<BackToLauncher>();
            AddPauseMenu(canvas, backTogglesPause: false, addSamplesButton: false);

            FinishScene(scene.Value, ScenePath, BuilderName);
        }
    }
}
