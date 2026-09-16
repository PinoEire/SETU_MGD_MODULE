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
    /// Builds the Accessibility sample scene from a menu item. Every run assigns
    /// fresh object IDs, so run it only when this builder changes.
    /// </summary>
    public static class AccessibilitySceneBuilder
    {
        const string ScenePath = "Assets/_Game/Scenes/Accessibility/Accessibility.unity";
        const float Margin = 96f;

        // Canvas units at the 1080 x 1920 reference. On a 1080-wide 420 dpi phone
        // one unit is one pixel and 1 sp is about 2.6 px, so 42 units is roughly
        // 16 sp: the smallest size body text should ever render at Normal.
        const float BodyFontSize = 42f;

        [MenuItem("MGD Samples/Build Accessibility Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[AccessibilitySceneBuilder] Cancelled: current scene has unsaved changes.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera(new Color(0.08f, 0.09f, 0.12f));
            CreateEventSystem();

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateUiObject("Root", canvas.transform);
            Stretch(root, Margin);

            CreateDemo(root);
            CreateSettingsCard(root);

            // Every text players read gets TextScale, with Auto Size off so the
            // component's size is not overridden.
            foreach (TMP_Text text in canvas.GetComponentsInChildren<TMP_Text>(true))
            {
                text.enableAutoSizing = false;
                text.gameObject.AddComponent<TextScale>();
            }

            // Android back returns to the launcher; this sample does not use back itself.
            canvas.gameObject.AddComponent<BackToLauncher>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);

            Debug.Log($"[AccessibilitySceneBuilder] Saved {ScenePath} and added it to Build Settings.");
        }

        static void CreateDemo(RectTransform root)
        {
            TextMeshProUGUI title = CreateLabel(root, "Title", "Accessibility sample", 56f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 100f), stretchWidth: true);

            TextMeshProUGUI status = CreateLabel(root, "Status", "", 44f);
            Place(status.rectTransform, new Vector2(0.5f, 0.90f), new Vector2(0f, 80f), stretchWidth: true);

            Button hit = CreateButton(root, "Button Hit", "Hit", new Vector2(520f, 150f), 48f);
            Place(hit.GetComponent<RectTransform>(), new Vector2(0.5f, 0.82f), new Vector2(520f, 150f));

            TextMeshProUGUI shakeLabel = CreateLabel(root, "Shake Label", "Motion", 30f);
            Place(shakeLabel.rectTransform, new Vector2(0.3f, 0.745f), new Vector2(300f, 50f));

            RectTransform shaker = CreateUiObject("Shaker", root);
            var shakerImage = shaker.gameObject.AddComponent<Image>();
            shakerImage.color = new Color(1f, 0.6f, 0.2f);
            shakerImage.raycastTarget = false;
            Place(shaker, new Vector2(0.3f, 0.68f), new Vector2(120f, 120f));

            TextMeshProUGUI dpTitle = CreateLabel(root, "Dp Label Title", "48 dp minimum target", 30f);
            Place(dpTitle.rectTransform, new Vector2(0.7f, 0.745f), new Vector2(400f, 50f));

            RectTransform dpSquare = CreateUiObject("Dp Square", root);
            var dpImage = dpSquare.gameObject.AddComponent<Image>();
            dpImage.color = new Color(0.4f, 0.8f, 1f);
            dpImage.raycastTarget = false;
            Place(dpSquare, new Vector2(0.7f, 0.68f), new Vector2(126f, 126f));

            TextMeshProUGUI dpLabel = CreateLabel(root, "Dp Label", "", 30f);
            Place(dpLabel.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(0f, 50f), stretchWidth: true);

            TextMeshProUGUI body = CreateLabel(root, "Body",
                "Body text at Normal is about 16 sp on a 1080-wide phone. Set Large and check that nothing overflows or is cut off. Labels use ellipsis; paragraphs wrap.",
                BodyFontSize, TextAlignmentOptions.TopLeft);
            body.textWrappingMode = TextWrappingModes.Normal;
            // Tall enough for four wrapped lines at Large; the paragraph must never
            // be shrunk to fit, so the box gives it room instead.
            Place(body.rectTransform, new Vector2(0.5f, 0.52f), new Vector2(0f, 260f), stretchWidth: true);

            TextMeshProUGUI sizeLabel = CreateLabel(root, "Size Label", "", 36f);
            Place(sizeLabel.rectTransform, new Vector2(0.5f, 0.42f), new Vector2(0f, 60f), stretchWidth: true);

            var demo = root.gameObject.AddComponent<AccessibilityDemoHud>();
            SetField(demo, "status", status);
            SetField(demo, "shaker", shaker);
            SetField(demo, "dpSquare", dpSquare);
            SetField(demo, "dpLabel", dpLabel);
            SetField(demo, "sizeLabel", sizeLabel);
            UnityEventTools.AddPersistentListener(hit.onClick, new UnityAction(demo.OnHitPressed));
        }

        static void CreateSettingsCard(RectTransform root)
        {
            RectTransform card = CreateUiObject("Settings Card", root);
            var cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = new Color(0.12f, 0.13f, 0.17f, 1f);
            cardImage.raycastTarget = false;
            Place(card, new Vector2(0.5f, 0.19f), new Vector2(0f, 600f), stretchWidth: true);

            TextMeshProUGUI title = CreateLabel(card, "Title", "Settings", 48f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 90f), stretchWidth: true);

            Toggle haptics = CreateToggle(card, "Toggle Haptics", "Haptics", 760f);
            Place(haptics.GetComponent<RectTransform>(), new Vector2(0.5f, 0.70f), new Vector2(760f, 130f));

            Toggle reduceMotion = CreateToggle(card, "Toggle Reduce Motion", "Reduce motion", 760f);
            Place(reduceMotion.GetComponent<RectTransform>(), new Vector2(0.5f, 0.47f), new Vector2(760f, 130f));

            TextMeshProUGUI sizeTitle = CreateLabel(card, "Text Size Title", "Text size", 36f, TextAlignmentOptions.MidlineLeft);
            Place(sizeTitle.rectTransform, new Vector2(0.5f, 0.27f), new Vector2(0f, 50f), stretchWidth: true);
            sizeTitle.rectTransform.offsetMin = new Vector2(64f, sizeTitle.rectTransform.offsetMin.y);

            var buttonSize = new Vector2(250f, 130f);
            Button small = CreateButton(card, "Button Small", "Small", buttonSize, 40f);
            Place(small.GetComponent<RectTransform>(), new Vector2(0.2f, 0.12f), buttonSize);
            Button normal = CreateButton(card, "Button Normal", "Normal", buttonSize, 40f);
            Place(normal.GetComponent<RectTransform>(), new Vector2(0.5f, 0.12f), buttonSize);
            Button large = CreateButton(card, "Button Large", "Large", buttonSize, 40f);
            Place(large.GetComponent<RectTransform>(), new Vector2(0.8f, 0.12f), buttonSize);

            var panel = card.gameObject.AddComponent<SettingsPanel>();
            SetField(panel, "hapticsToggle", haptics);
            SetField(panel, "reduceMotionToggle", reduceMotion);
            SetField(panel, "smallButton", small);
            SetField(panel, "normalButton", normal);
            SetField(panel, "largeButton", large);
        }
    }
}
