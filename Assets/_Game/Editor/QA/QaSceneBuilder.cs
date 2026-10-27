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
    /// Builds the QA sample scene from a menu item. Every run assigns fresh
    /// object IDs, so run it only when this builder changes.
    /// </summary>
    public static class QaSceneBuilder
    {
        const string BuilderName = "QaSceneBuilder";
        const string ScenePath = "Assets/_Game/Scenes/QA/QA.unity";

        // 140 units is about 53 dp on the reference phone, above the 48 dp minimum.
        static readonly Vector2 ButtonSize = new Vector2(640f, 140f);

        [MenuItem("MGD Samples/Build QA Scene")]
        public static void Build()
        {
            Scene? scene = BeginScene(BuilderName);
            if (scene == null)
            {
                return;
            }

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateHudRoot(canvas);

            TextMeshProUGUI title = CreateLabel(root, "Title", "QA signals", 64f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 100f), stretchWidth: true);

            TextMeshProUGUI intro = CreateLabel(root, "Intro",
                "Start a logcat recording first, then make each failure\nonce and find its signature in the log.", 30f);
            intro.color = new Color(0.75f, 0.78f, 0.85f);
            Place(intro.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 110f), stretchWidth: true, offset: new Vector2(0f, -110f));

            Button exception = CreateButton(root, "Button Exception", "Throw exception", ButtonSize, 40f);
            Place(exception.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), ButtonSize, offset: new Vector2(0f, -270f));
            Button anr = CreateButton(root, "Button ANR", "Freeze UI thread (ANR)", ButtonSize, 40f);
            Place(anr.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), ButtonSize, offset: new Vector2(0f, -440f));
            Button crash = CreateButton(root, "Button Crash", "Native crash", ButtonSize, 40f);
            Place(crash.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), ButtonSize, offset: new Vector2(0f, -610f));

            TextMeshProUGUI status = CreateLabel(root, "Status", "", 34f);
            Place(status.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 200f), stretchWidth: true, offset: new Vector2(0f, -790f));

            TextMeshProUGUI hint = CreateLabel(root, "Hint",
                "Low memory: adb shell am send-trim-memory\ncom.dftgames.mgdsamples RUNNING_CRITICAL", 28f, TextAlignmentOptions.Bottom);
            hint.color = new Color(0.75f, 0.78f, 0.85f);
            Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 100f), stretchWidth: true);

            var triggers = root.gameObject.AddComponent<QaTriggers>();
            SetField(triggers, "exceptionLabel", exception.GetComponentInChildren<TextMeshProUGUI>());
            SetField(triggers, "anrLabel", anr.GetComponentInChildren<TextMeshProUGUI>());
            SetField(triggers, "crashLabel", crash.GetComponentInChildren<TextMeshProUGUI>());
            SetField(triggers, "status", status);

            // Persistent listeners: what a student wires by hand in the Inspector.
            UnityEventTools.AddPersistentListener(exception.onClick, new UnityAction(triggers.OnExceptionPressed));
            UnityEventTools.AddPersistentListener(anr.onClick, new UnityAction(triggers.OnAnrPressed));
            UnityEventTools.AddPersistentListener(crash.onClick, new UnityAction(triggers.OnCrashPressed));

            // Android back returns to the launcher; this sample does not use back
            // itself, so the pause panel's own back toggle is off.
            canvas.gameObject.AddComponent<BackToLauncher>();
            AddPauseMenu(canvas, backTogglesPause: false, addSamplesButton: false);

            FinishScene(scene.Value, ScenePath, BuilderName);
        }
    }
}
