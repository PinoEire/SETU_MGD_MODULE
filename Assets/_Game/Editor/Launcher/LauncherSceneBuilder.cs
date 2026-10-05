using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MGD.Samples.Editor.SampleSceneBuild;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Builds the launcher scene, the first scene in the build. The list of samples
    /// is not baked in: <see cref="LauncherMenu"/> reads Build Settings at runtime.
    /// Every run assigns fresh object IDs, so run it only when this builder changes.
    /// </summary>
    public static class LauncherSceneBuilder
    {
        const string BuilderName = "LauncherSceneBuilder";
        const string ScenePath = "Assets/_Game/Scenes/Launcher/Launcher.unity";

        [MenuItem("MGD Samples/Build Launcher Scene")]
        public static void Build()
        {
            Scene? scene = BeginScene(BuilderName);
            if (scene == null)
            {
                return;
            }

            // App-wide settings live in the first scene, once. See MobileBootstrap.
            new GameObject("Bootstrap", typeof(MobileBootstrap));
            // Logs time to interactive on the first frame the menu can be used.
            new GameObject("Menu Ready", typeof(MenuReady));

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateHudRoot(canvas);

            TextMeshProUGUI title = CreateLabel(root, "Title", "MGD Samples", 72f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 110f), stretchWidth: true);

            TextMeshProUGUI subtitle = CreateLabel(root, "Subtitle", "Mobile Game Development, SETU 2026/27\nTap a sample. Android back returns here.", 32f);
            Place(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 110f), stretchWidth: true, offset: new Vector2(0f, -120f));

            // The list fills the space between the subtitle and the footer.
            RectTransform list = CreateUiObject("List", root);
            list.anchorMin = new Vector2(0f, 0f);
            list.anchorMax = new Vector2(1f, 1f);
            list.pivot = new Vector2(0.5f, 1f);
            list.offsetMin = new Vector2(0f, 200f);
            list.offsetMax = new Vector2(0f, -260f);
            var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Inactive template; LauncherMenu clones it once per scene. 130 units tall
            // is about 49 dp on the reference phone, just above the 48 dp minimum, so
            // eight samples (8 x 130 + 7 x 16 = 1152) fit the list's 1268 units on the
            // 1080 x 1920 reference without reaching the footer.
            Button template = CreateButton(list, "Button Template", "Sample", new Vector2(720f, 130f), 48f);
            template.gameObject.SetActive(false);

            TextMeshProUGUI footer = CreateLabel(root, "Footer", "", 26f, TextAlignmentOptions.Bottom);
            footer.color = new Color(0.75f, 0.78f, 0.85f);
            Place(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 120f), stretchWidth: true);

            // The first scene pauses like every other; back toggles the panel here
            // because there is no launcher to return to.
            AddPauseMenu(canvas, backTogglesPause: true, addSamplesButton: false);

            var menu = canvas.gameObject.AddComponent<LauncherMenu>();
            SetField(menu, "listRoot", list);
            SetField(menu, "buttonTemplate", template);
            SetField(menu, "footer", footer);

            CreateSceneLoader();

            FinishScene(scene.Value, ScenePath, BuilderName, first: true);
        }

        // Above every scene's canvases, the pause panel included.
        const int LoadingSortOrder = 100;

        static void CreateSceneLoader()
        {
            // Its own canvas under its own root, because DontDestroyOnLoad keeps a
            // root object and everything under it: the loading screen must survive
            // the scene change it is covering.
            var loaderObject = new GameObject("Scene Loader", typeof(SceneLoader));
            Canvas canvas = CreateCanvas();
            canvas.name = "Loading Canvas";
            canvas.sortingOrder = LoadingSortOrder;
            canvas.transform.SetParent(loaderObject.transform, false);

            RectTransform screen = CreateUiObject("Loading Screen", canvas.transform);
            Stretch(screen);
            var dim = screen.gameObject.AddComponent<Image>();
            dim.color = new Color(0.08f, 0.09f, 0.12f, 0.96f);
            dim.raycastTarget = true; // swallows taps while loading

            TextMeshProUGUI label = CreateLabel(screen, "Label", "Loading", 56f);
            Place(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(800f, 100f), offset: new Vector2(0f, 110f));

            Slider bar = CreateBar(screen, "Bar", new Vector2(800f, 48f));
            Place(bar.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(800f, 48f));

            TextMeshProUGUI percent = CreateLabel(screen, "Percent", "0%", 40f);
            Place(percent.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(400f, 70f), offset: new Vector2(0f, -90f));

            var loader = loaderObject.GetComponent<SceneLoader>();
            SetField(loader, "loadingScreen", screen.gameObject);
            SetField(loader, "bar", bar);
            SetField(loader, "percent", percent);
            screen.gameObject.SetActive(false);
        }
    }
}
