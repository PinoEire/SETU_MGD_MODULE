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
    /// Builds the Performance sample scene from a menu item. Every run assigns
    /// fresh object IDs, so run it only when this builder changes.
    /// </summary>
    public static class PerformanceSceneBuilder
    {
        const string ScenePath = "Assets/_Game/Scenes/Performance/Performance.unity";
        const string SpriteMaterialPath =
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
        const float Margin = 96f;

        [MenuItem("MGD Samples/Build Performance Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[PerformanceSceneBuilder] Cancelled: current scene has unsaved changes.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera(new Color(0.08f, 0.09f, 0.12f));
            CreateEventSystem();

            // The sprite field sits behind the Canvas in world space.
            var loadObject = new GameObject("Load Generator", typeof(LoadGenerator));
            var load = loadObject.GetComponent<LoadGenerator>();
            SetField(load, "sprite", AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"));
            SetField(load, "material", AssetDatabase.LoadAssetAtPath<Material>(SpriteMaterialPath));

            var tools = new GameObject("Tools", typeof(FrameTimeSampler), typeof(RenderScaleProbe));
            var sampler = tools.GetComponent<FrameTimeSampler>();
            var probe = tools.GetComponent<RenderScaleProbe>();

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateUiObject("Root", canvas.transform);
            Stretch(root, Margin);

            CreateHudCard(root, load, sampler, probe);

            // Android back returns to the launcher; this sample does not use back itself.
            canvas.gameObject.AddComponent<BackToLauncher>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);

            Debug.Log($"[PerformanceSceneBuilder] Saved {ScenePath} and added it to Build Settings.");
        }

        static void CreateHudCard(RectTransform root, LoadGenerator load, FrameTimeSampler sampler, RenderScaleProbe probe)
        {
            // A translucent card in the bottom third keeps the text readable over
            // the moving sprites and the buttons in thumb reach.
            RectTransform card = CreateUiObject("HUD Card", root);
            var cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = new Color(0.08f, 0.09f, 0.12f, 0.85f);
            cardImage.raycastTarget = false;
            Place(card, new Vector2(0.5f, 0f), new Vector2(0f, 720f), stretchWidth: true);

            TextMeshProUGUI title = CreateLabel(card, "Title", "Performance sample", 56f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 100f), stretchWidth: true);

            // Two lines at 40 units: the body-text floor from the Accessibility
            // sample (about 16 sp on a 1080-wide phone) applies to a HUD too.
            TextMeshProUGUI status = CreateLabel(card, "Status", "", 40f);
            Place(status.rectTransform, new Vector2(0.5f, 0.76f), new Vector2(0f, 110f), stretchWidth: true);

            TextMeshProUGUI samplerLabel = CreateLabel(card, "Sampler", "", 40f);
            Place(samplerLabel.rectTransform, new Vector2(0.5f, 0.60f), new Vector2(0f, 60f), stretchWidth: true);
            SetField(sampler, "label", samplerLabel);

            // Every button is 130 units tall: 48 dp on the reference phone, the minimum tap target.
            var buttonSize = new Vector2(270f, 130f);
            Button idle = CreateButton(card, "Button Idle", "Idle", buttonSize, 40f);
            Place(idle.GetComponent<RectTransform>(), new Vector2(0.2f, 0.41f), buttonSize);
            Button steady = CreateButton(card, "Button Steady", "Steady", buttonSize, 40f);
            Place(steady.GetComponent<RectTransform>(), new Vector2(0.5f, 0.41f), buttonSize);
            Button worst = CreateButton(card, "Button Worst", "Worst", buttonSize, 40f);
            Place(worst.GetComponent<RectTransform>(), new Vector2(0.8f, 0.41f), buttonSize);

            var probeSize = new Vector2(560f, 130f);
            Button probeButton = CreateButton(card, "Button Probe", "Probe: toggle render scale", probeSize, 36f);
            Place(probeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.15f), probeSize);

            var hud = card.gameObject.AddComponent<PerformanceDemoHud>();
            SetField(hud, "load", load);
            SetField(hud, "sampler", sampler);
            SetField(hud, "probe", probe);
            SetField(hud, "status", status);
            SetField(hud, "probeButton", probeButton.gameObject);

            UnityEventTools.AddPersistentListener(idle.onClick, new UnityAction(hud.OnIdlePressed));
            UnityEventTools.AddPersistentListener(steady.onClick, new UnityAction(hud.OnSteadyPressed));
            UnityEventTools.AddPersistentListener(worst.onClick, new UnityAction(hud.OnWorstPressed));
            UnityEventTools.AddPersistentListener(probeButton.onClick, new UnityAction(hud.OnProbePressed));
        }
    }
}
