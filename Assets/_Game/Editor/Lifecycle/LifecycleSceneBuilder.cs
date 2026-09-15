using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
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
        const string ScenePath = "Assets/_Game/Scenes/Lifecycle/Lifecycle.unity";

        // Generous margin instead of a SafeArea panel: samples do not reference
        // each other's scripts, and 96 px clears every notch and gesture bar.
        const float Margin = 96f;

        [MenuItem("MGD Samples/Build Lifecycle Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[LifecycleSceneBuilder] Cancelled: current scene has unsaved changes.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera(new Color(0.08f, 0.09f, 0.12f));
            CreateEventSystem();

            var bootstrap = new GameObject("Bootstrap", typeof(LifecycleGuard));
            var guard = bootstrap.GetComponent<LifecycleGuard>();

            Canvas canvas = CreateCanvas();
            CreateHud(canvas.transform);
            GameObject panel = CreatePausePanel(canvas.transform, out Button resume, out Button samples);

            // PauseMenu lives on the Canvas, which stays active, and toggles only the panel.
            var menu = canvas.gameObject.AddComponent<PauseMenu>();
            SetField(menu, "panel", panel);
            SetField(menu, "guard", guard);
            UnityEventTools.AddPersistentListener(resume.onClick, new UnityAction(menu.OnResumePressed));

            // Back is taken by the pause toggle here, so the way out is the Samples
            // button on the pause card, not the back gesture.
            var back = canvas.gameObject.AddComponent<BackToLauncher>();
            SetField(back, "listenForBack", false);
            UnityEventTools.AddPersistentListener(samples.onClick, new UnityAction(back.Go));

            panel.SetActive(false);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);

            Debug.Log($"[LifecycleSceneBuilder] Saved {ScenePath} and added it to Build Settings.");
        }

        static void CreateHud(Transform parent)
        {
            RectTransform hud = CreateUiObject("HUD", parent);
            Stretch(hud, Margin);

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

        static GameObject CreatePausePanel(Transform parent, out Button resume, out Button samples)
        {
            // Full-screen dim that also blocks taps on the HUD underneath while paused.
            RectTransform panel = CreateUiObject("Pause Panel", parent);
            Stretch(panel);
            var dim = panel.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.5f);
            dim.raycastTarget = true;

            // Opaque card in the band the HUD leaves free, so the clocks above stay readable.
            RectTransform card = CreateUiObject("Card", panel);
            var cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = new Color(0.1f, 0.11f, 0.14f, 1f);
            cardImage.raycastTarget = false;
            Place(card, new Vector2(0.5f, 0.32f), new Vector2(900f, 580f));

            TextMeshProUGUI title = CreateLabel(card, "Title", "Paused", 72f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 100f), stretchWidth: true);
            title.rectTransform.anchoredPosition = new Vector2(0f, -12f);

            resume = CreateButton(card, "Button Resume", "Resume", new Vector2(520f, 150f), 56f);
            Place(resume.GetComponent<RectTransform>(), new Vector2(0.5f, 0.60f), new Vector2(520f, 150f));

            samples = CreateButton(card, "Button Samples", "Back to samples", new Vector2(520f, 110f), 40f);
            Place(samples.GetComponent<RectTransform>(), new Vector2(0.5f, 0.34f), new Vector2(520f, 110f));

            TextMeshProUGUI hint = CreateLabel(card, "Hint", "Android back also toggles pause.\nThere is no Quit button.", 30f);
            Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 100f), stretchWidth: true);
            hint.rectTransform.anchoredPosition = new Vector2(0f, 12f);

            return panel.gameObject;
        }

    }
}
