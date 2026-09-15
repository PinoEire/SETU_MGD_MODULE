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
    /// Builds the SafeArea sample scene from a menu item so the scene is reproducible
    /// and its construction is readable. Running it again overwrites the scene and
    /// keeps the generated background texture. Every run assigns fresh object IDs,
    /// so run it only when this builder changes.
    /// </summary>
    public static class SafeAreaSceneBuilder
    {
        const string ScenePath = "Assets/_Game/Scenes/SafeArea/SafeArea.unity";
        const string TexturePath = "Assets/_Game/Textures/SafeArea/GridBackground.png";

        // 200 px at the 1080-wide reference is roughly 70 dp on a typical 420 dpi
        // phone: comfortably above the 48 dp minimum touch target.
        const float ButtonSize = 200f;
        const float EdgePadding = 24f;

        [MenuItem("MGD Samples/Build SafeArea Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[SafeAreaSceneBuilder] Cancelled: current scene has unsaved changes.");
                return;
            }

            Sprite grid = GetOrCreateGridSprite();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera(new Color(0.08f, 0.09f, 0.12f));
            CreateEventSystem();

            Canvas canvas = CreateCanvas();
            CreateBackground(canvas.transform, grid);
            RectTransform safeArea = CreateSafeAreaPanel(canvas.transform);
            CreateHud(safeArea);

            // Android back returns to the launcher; this sample does not use back itself.
            canvas.gameObject.AddComponent<BackToLauncher>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);

            Debug.Log($"[SafeAreaSceneBuilder] Saved {ScenePath} and added it to Build Settings.");
        }

        /// <summary>
        /// Full-screen tiled grid that deliberately ignores the safe area, so on the
        /// phone it is visible running under the notch and behind the gesture bar.
        /// </summary>
        static void CreateBackground(Transform parent, Sprite grid)
        {
            RectTransform rect = CreateUiObject("Background (ignores safe area)", parent);
            Stretch(rect);

            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = grid;
            image.type = Image.Type.Tiled;
            image.color = new Color(0.55f, 0.6f, 0.7f);
            image.raycastTarget = false;
        }

        static RectTransform CreateSafeAreaPanel(Transform parent)
        {
            RectTransform rect = CreateUiObject("SafeArea", parent);
            Stretch(rect);

            // A faint tint shows the panel's bounds on device. Raycasts are off so it
            // never blocks a tap meant for the world underneath.
            var tint = rect.gameObject.AddComponent<Image>();
            tint.color = new Color(0.2f, 0.8f, 0.4f, 0.12f);
            tint.raycastTarget = false;

            rect.gameObject.AddComponent<MGD.Samples.SafeArea>();
            return rect;
        }

        static void CreateHud(RectTransform safeArea)
        {
            CreateCornerButton(safeArea, "TL", new Vector2(0f, 1f), new Vector2(EdgePadding, -EdgePadding));
            CreateCornerButton(safeArea, "TR", new Vector2(1f, 1f), new Vector2(-EdgePadding, -EdgePadding));
            CreateCornerButton(safeArea, "BL", new Vector2(0f, 0f), new Vector2(EdgePadding, EdgePadding));
            CreateCornerButton(safeArea, "BR", new Vector2(1f, 0f), new Vector2(-EdgePadding, EdgePadding));

            // Score label across the top, between the two top buttons.
            TextMeshProUGUI score = CreateLabel(safeArea, "Score", "SCORE 0000", 64f);
            RectTransform scoreRect = score.rectTransform;
            scoreRect.anchorMin = new Vector2(0f, 1f);
            scoreRect.anchorMax = new Vector2(1f, 1f);
            scoreRect.pivot = new Vector2(0.5f, 1f);
            scoreRect.offsetMin = new Vector2(ButtonSize + EdgePadding * 2f, -(ButtonSize + EdgePadding));
            scoreRect.offsetMax = new Vector2(-(ButtonSize + EdgePadding * 2f), -EdgePadding);

            // Live readout of the safe-area numbers, bottom centre.
            TextMeshProUGUI readout = CreateLabel(safeArea, "Readout", "", 36f, TextAlignmentOptions.Bottom);
            RectTransform readoutRect = readout.rectTransform;
            readoutRect.anchorMin = new Vector2(0f, 0f);
            readoutRect.anchorMax = new Vector2(1f, 0f);
            readoutRect.pivot = new Vector2(0.5f, 0f);
            readoutRect.offsetMin = new Vector2(ButtonSize + EdgePadding * 2f, EdgePadding);
            readoutRect.offsetMax = new Vector2(-(ButtonSize + EdgePadding * 2f), ButtonSize + EdgePadding);
            readout.gameObject.AddComponent<SafeAreaReadout>();
        }

        static void CreateCornerButton(RectTransform parent, string label, Vector2 anchor, Vector2 offset)
        {
            Button button = CreateButton(parent, $"Button {label}", label, new Vector2(ButtonSize, ButtonSize));
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = offset;
        }

        /// <summary>
        /// Generates a 256 px tileable grid PNG once and imports it as a full-rect
        /// sprite so the Image can tile it.
        /// </summary>
        static Sprite GetOrCreateGridSprite()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(TexturePath);
            if (existing != null)
            {
                return existing;
            }

            const int size = 256;
            const int cell = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            var fill = new Color32(255, 255, 255, 255);
            var line = new Color32(40, 40, 50, 255);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool onLine = x % cell == 0 || y % cell == 0;
                    pixels[y * size + x] = onLine ? line : fill;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            Directory.CreateDirectory(Path.GetDirectoryName(TexturePath));
            File.WriteAllBytes(TexturePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(TexturePath);

            var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.mipmapEnabled = false;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(TexturePath);
        }
    }
}
