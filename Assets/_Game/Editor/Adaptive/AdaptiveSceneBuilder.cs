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
    /// Builds the Adaptive sample scene from a menu item. Every run assigns fresh
    /// object IDs, so run it only when this builder changes.
    /// </summary>
    public static class AdaptiveSceneBuilder
    {
        const string BuilderName = "AdaptiveSceneBuilder";
        const string ScenePath = "Assets/_Game/Scenes/Adaptive/Adaptive.unity";
        const string SpriteMaterialPath =
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        // 140 units is about 53 dp on the reference phone, above the 48 dp minimum.
        static readonly Vector2 ButtonSize = new Vector2(600f, 140f);

        [MenuItem("MGD Samples/Build Adaptive Scene")]
        public static void Build()
        {
            Scene? scene = BeginScene(BuilderName);
            if (scene == null)
            {
                return;
            }

            // The layers sit behind the Canvas in world space.
            var heatObject = new GameObject("Heat Load", typeof(HeatLoad));
            var heat = heatObject.GetComponent<HeatLoad>();
            SetField(heat, "sprite", AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"));
            SetField(heat, "material", AssetDatabase.LoadAssetAtPath<Material>(SpriteMaterialPath));

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateHudRoot(canvas);

            SoakTimer soak = CreateReadout(root);

            Button load = CreateButton(root, "Button Load", "Load", ButtonSize, 40f);
            Place(load.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), ButtonSize, offset: new Vector2(0f, 170f));
            SetField(heat, "buttonLabel", load.GetComponentInChildren<TextMeshProUGUI>());

            Button restart = CreateButton(root, "Button Restart Soak", "Restart soak", ButtonSize, 40f);
            Place(restart.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), ButtonSize);

            // Persistent listeners: what a student wires by hand in the Inspector.
            UnityEventTools.AddPersistentListener(load.onClick, new UnityAction(heat.Cycle));
            UnityEventTools.AddPersistentListener(restart.onClick, new UnityAction(soak.Restart));

            // Back opens the pause menu here, because the High FPS toggle lives on
            // it; the way out is the Samples button on the pause card, as in the
            // Lifecycle sample. The card sits below the readout, so the mode change
            // shows on the overlay while the menu is still open.
            Button samples = AddPauseMenu(canvas, backTogglesPause: true, addSamplesButton: true, cardAnchorY: 0.42f);
            var back = canvas.gameObject.AddComponent<BackToLauncher>();
            SetField(back, "listenForBack", false);
            UnityEventTools.AddPersistentListener(samples.onClick, new UnityAction(back.Go));

            FinishScene(scene.Value, ScenePath, BuilderName);
        }

        static SoakTimer CreateReadout(RectTransform root)
        {
            // A dark card keeps the readout legible over the coloured layers.
            RectTransform card = CreateUiObject("Readout Card", root);
            var cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = new Color(0.08f, 0.09f, 0.12f, 0.85f);
            cardImage.raycastTarget = false;
            Place(card, new Vector2(0.5f, 1f), new Vector2(0f, 500f), stretchWidth: true);

            TextMeshProUGUI title = CreateLabel(card, "Title", "Adaptive Performance", 56f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 100f), stretchWidth: true);

            TextMeshProUGUI numbers = CreateLabel(card, "Numbers", "", 44f);
            Place(numbers.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 70f), stretchWidth: true, offset: new Vector2(0f, -110f));

            TextMeshProUGUI status = CreateLabel(card, "Status", "", 36f);
            Place(status.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 110f), stretchWidth: true, offset: new Vector2(0f, -185f));

            // 32 units so the longest soak line fits a 20:9 phone, where the card is
            // only about 774 units wide.
            TextMeshProUGUI soakLabel = CreateLabel(card, "Soak", "", 32f);
            Place(soakLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 60f), stretchWidth: true, offset: new Vector2(0f, -310f));

            TextMeshProUGUI hint = CreateLabel(card, "Hint", "Android back opens the pause menu:\nthe High FPS toggle is there.", 30f);
            hint.color = new Color(0.75f, 0.78f, 0.85f);
            Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 100f), stretchWidth: true, offset: new Vector2(0f, 16f));

            var overlay = card.gameObject.AddComponent<ThermalOverlay>();
            SetField(overlay, "numbers", numbers);
            SetField(overlay, "status", status);

            var soak = card.gameObject.AddComponent<SoakTimer>();
            SetField(soak, "label", soakLabel);
            return soak;
        }
    }
}
