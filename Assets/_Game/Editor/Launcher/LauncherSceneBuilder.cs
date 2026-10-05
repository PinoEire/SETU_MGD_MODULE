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

        // The list's distance from the bottom (above the footer) and the top (below
        // the subtitle) of the HUD root, shared by the viewport and the scrollbar.
        const float ListBottom = 200f;
        const float ListTop = 260f;
        const float ScrollbarWidth = 16f;
        const float ScrollbarGap = 24f;

        // Above every scene's canvases, the pause panel included.
        const int LoadingSortOrder = 100;

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

            RectTransform list = CreateScrollList(root);
            var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Inactive template; LauncherMenu clones it once per scene. 150 units is
            // about 57 dp on the reference phone, comfortably above the 48 dp minimum.
            Button template = CreateButton(list, "Button Template", "Sample", new Vector2(720f, 150f), 48f);
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

        /// <summary>
        /// A vertical scroll view filling the space between the subtitle and the
        /// footer: a viewport that clips with RectMask2D (so buttons never draw over
        /// the footer) and a slim scrollbar that appears only when there are more
        /// scenes than fit, because a list that happens to end on a whole button gives
        /// no other hint. Returns the content rect, anchored to the viewport's top,
        /// which grows to fit however many buttons LauncherMenu adds.
        /// </summary>
        static RectTransform CreateScrollList(RectTransform root)
        {
            RectTransform viewport = CreateUiObject("List Viewport", root);
            viewport.anchorMin = new Vector2(0f, 0f);
            viewport.anchorMax = new Vector2(1f, 1f);
            viewport.offsetMin = new Vector2(0f, ListBottom);
            viewport.offsetMax = new Vector2(0f, -ListTop);
            viewport.gameObject.AddComponent<RectMask2D>();
            // An invisible Image so a drag on the gaps between buttons still scrolls.
            var hitArea = viewport.gameObject.AddComponent<Image>();
            hitArea.color = Color.clear;

            RectTransform content = CreateUiObject("List", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = true;

            // In the root's right-hand margin, outside the viewport so the mask does
            // not clip it and clear of the buttons however narrow the phone is. A
            // visual cue, not a touch target.
            var resources = new DefaultControls.Resources
            {
                standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
                background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd")
            };
            GameObject barObject = DefaultControls.CreateScrollbar(resources);
            barObject.name = "List Scrollbar";
            barObject.transform.SetParent(root, false);
            SetLayerRecursively(barObject, LayerMask.NameToLayer("UI"));
            var bar = barObject.GetComponent<Scrollbar>();
            bar.direction = Scrollbar.Direction.BottomToTop;
            var barRect = barObject.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(1f, 0f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.pivot = new Vector2(1f, 0.5f);
            barRect.offsetMin = new Vector2(ScrollbarGap, ListBottom);
            barRect.offsetMax = new Vector2(ScrollbarGap + ScrollbarWidth, -ListTop);

            scroll.verticalScrollbar = bar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            return content;
        }

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
