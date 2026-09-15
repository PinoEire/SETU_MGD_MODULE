using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MGD.Samples.Editor.SampleSceneBuild;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Builds the launcher scene, the first scene in the build. The list of samples
    /// is not baked in: <see cref="LauncherMenu"/> reads Build Settings at runtime.
    /// </summary>
    public static class LauncherSceneBuilder
    {
        const string ScenePath = "Assets/_Game/Scenes/Launcher/Launcher.unity";
        const float Margin = 96f;

        [MenuItem("MGD Samples/Build Launcher Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[LauncherSceneBuilder] Cancelled: current scene has unsaved changes.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera(new Color(0.08f, 0.09f, 0.12f));
            CreateEventSystem();

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateUiObject("Root", canvas.transform);
            Stretch(root, Margin);

            TextMeshProUGUI title = CreateLabel(root, "Title", "MGD Samples", 72f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 110f), stretchWidth: true);

            TextMeshProUGUI subtitle = CreateLabel(root, "Subtitle", "Mobile Game Development, SETU 2026/27\nTap a sample. Android back returns here.", 32f);
            Place(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 110f), stretchWidth: true);
            subtitle.rectTransform.anchoredPosition = new Vector2(0f, -120f);

            // The list fills the space between the subtitle and the footer.
            RectTransform list = CreateUiObject("List", root);
            list.anchorMin = new Vector2(0f, 0f);
            list.anchorMax = new Vector2(1f, 1f);
            list.pivot = new Vector2(0.5f, 1f);
            list.offsetMin = new Vector2(0f, 200f);
            list.offsetMax = new Vector2(0f, -260f);
            var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Inactive template; LauncherMenu clones it once per scene.
            Button template = CreateButton(list, "Button Template", "Sample", new Vector2(720f, 150f), 48f);
            template.gameObject.SetActive(false);

            TextMeshProUGUI footer = CreateLabel(root, "Footer", "", 26f, TextAlignmentOptions.Bottom);
            footer.color = new Color(0.75f, 0.78f, 0.85f);
            Place(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 120f), stretchWidth: true);

            var menu = canvas.gameObject.AddComponent<LauncherMenu>();
            SetField(menu, "listRoot", list);
            SetField(menu, "buttonTemplate", template);
            SetField(menu, "footer", footer);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath, first: true);

            Debug.Log($"[LauncherSceneBuilder] Saved {ScenePath} and placed it first in Build Settings.");
        }
    }
}
