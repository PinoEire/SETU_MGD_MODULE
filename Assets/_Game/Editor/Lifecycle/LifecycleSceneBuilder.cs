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
    /// Builds the Lifecycle sample scene from a menu item so the scene is
    /// reproducible and its construction is readable. Every run assigns fresh
    /// object IDs, so run it only when this builder changes.
    /// </summary>
    public static class LifecycleSceneBuilder
    {
        const string BuilderName = "LifecycleSceneBuilder";
        const string ScenePath = "Assets/_Game/Scenes/Lifecycle/Lifecycle.unity";

        [MenuItem("MGD Samples/Build Lifecycle Scene")]
        public static void Build()
        {
            Scene? scene = BeginScene(BuilderName);
            if (scene == null)
            {
                return;
            }

            Canvas canvas = CreateCanvas();
            CreateHud(CreateHudRoot(canvas));

            // The same guard and pause menu every scene gets, but here back toggles
            // pause (the thing this sample demonstrates), so the way out is the
            // Samples button on the pause card, not the back gesture. The card sits
            // in the band the HUD leaves free, so the clocks stay readable while paused.
            Button samples = AddPauseMenu(canvas, backTogglesPause: true, addSamplesButton: true, cardAnchorY: 0.32f);
            var back = canvas.gameObject.AddComponent<BackToLauncher>();
            SetField(back, "listenForBack", false);
            UnityEventTools.AddPersistentListener(samples.onClick, new UnityAction(back.Go));

            FinishScene(scene.Value, ScenePath, BuilderName);
        }

        static void CreateHud(RectTransform hud)
        {
            TextMeshProUGUI title = CreateLabel(hud, "Title", "Lifecycle sample", 56f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 120f), stretchWidth: true);

            // The upper half holds the live readouts; the band between the Tap
            // button and the log stays free for the pause card, so the clocks remain
            // readable while paused (that is what the sample demonstrates).
            RectTransform spinner = CreateUiObject("Spinner", hud);
            var spinnerImage = spinner.gameObject.AddComponent<Image>();
            spinnerImage.color = new Color(0.4f, 0.8f, 1f);
            spinnerImage.raycastTarget = false;
            Place(spinner, new Vector2(0.5f, 0.85f), new Vector2(160f, 160f));

            TextMeshProUGUI clocks = CreateLabel(hud, "Clocks", "", 40f);
            Place(clocks.rectTransform, new Vector2(0.5f, 0.72f), new Vector2(0f, 140f), stretchWidth: true);

            TextMeshProUGUI counter = CreateLabel(hud, "Counter", "", 44f);
            Place(counter.rectTransform, new Vector2(0.5f, 0.63f), new Vector2(0f, 80f), stretchWidth: true);

            Button tap = CreateButton(hud, "Button Tap", "Tap to count", new Vector2(520f, 160f));
            Place(tap.GetComponent<RectTransform>(), new Vector2(0.5f, 0.55f), new Vector2(520f, 160f));

            TextMeshProUGUI log = CreateLabel(hud, "Log", "", 30f, TextAlignmentOptions.BottomLeft);
            Place(log.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 300f), stretchWidth: true);

            var tone = hud.gameObject.AddComponent<AudioSource>();
            tone.playOnAwake = false;

            var demo = hud.gameObject.AddComponent<LifecycleDemoHud>();
            SetField(demo, "spinner", spinner);
            SetField(demo, "clocks", clocks);
            SetField(demo, "counter", counter);
            SetField(demo, "log", log);
            SetField(demo, "tone", tone);

            UnityEventTools.AddPersistentListener(tap.onClick, new UnityAction(demo.OnTapPressed));
        }
    }
}
