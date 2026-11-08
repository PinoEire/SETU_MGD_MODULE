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
    /// Builds the Resume sample scene from a menu item so the scene is
    /// reproducible and its construction is readable. Every run assigns fresh
    /// object IDs, so run it only when this builder changes.
    /// </summary>
    public static class ResumeSceneBuilder
    {
        const string BuilderName = "ResumeSceneBuilder";
        const string ScenePath = "Assets/_Game/Scenes/Resume/Resume.unity";

        [MenuItem("MGD Samples/Build Resume Scene")]
        public static void Build()
        {
            Scene? scene = BeginScene(BuilderName);
            if (scene == null)
            {
                return;
            }

            // The gameplay object: the timers and the offline clock.
            var guardObject = new GameObject("Resume Guard");
            var guard = guardObject.AddComponent<ResumeGuard>();
            var offline = guardObject.AddComponent<OfflineProgress>();

            Canvas canvas = CreateCanvas();
            CreateHud(CreateHudRoot(canvas), guard, offline);

            // Back is not used by this sample, so it returns to the launcher.
            AddPauseMenu(canvas, backTogglesPause: false, addSamplesButton: false);
            canvas.gameObject.AddComponent<BackToLauncher>();

            FinishScene(scene.Value, ScenePath, BuilderName);
        }

        static void CreateHud(RectTransform hud, ResumeGuard guard, OfflineProgress offline)
        {
            TextMeshProUGUI title = CreateLabel(hud, "Title", "Resume sample", 56f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 120f), stretchWidth: true);

            TextMeshProUGUI boss = CreateLabel(hud, "Boss", "", 56f);
            Place(boss.rectTransform, new Vector2(0.5f, 0.82f), new Vector2(0f, 100f), stretchWidth: true);

            TextMeshProUGUI waves = CreateLabel(hud, "Waves", "", 44f);
            Place(waves.rectTransform, new Vector2(0.5f, 0.74f), new Vector2(0f, 80f), stretchWidth: true);

            TextMeshProUGUI away = CreateLabel(hud, "Away", "No time away yet", 40f);
            Place(away.rectTransform, new Vector2(0.5f, 0.67f), new Vector2(0f, 80f), stretchWidth: true);

            Button restart = CreateButton(hud, "Button Restart", "Restart timers", new Vector2(520f, 160f));
            Place(restart.GetComponent<RectTransform>(), new Vector2(0.5f, 0.57f), new Vector2(520f, 160f));

            TextMeshProUGUI log = CreateLabel(hud, "Log", "", 30f, TextAlignmentOptions.BottomLeft);
            Place(log.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 400f), stretchWidth: true);

            var demo = hud.gameObject.AddComponent<ResumeDemoHud>();
            SetField(demo, "guard", guard);
            SetField(demo, "offline", offline);
            SetField(demo, "boss", boss);
            SetField(demo, "waves", waves);
            SetField(demo, "away", away);
            SetField(demo, "log", log);

            UnityEventTools.AddPersistentListener(restart.onClick, new UnityAction(guard.Restart));
        }
    }
}
