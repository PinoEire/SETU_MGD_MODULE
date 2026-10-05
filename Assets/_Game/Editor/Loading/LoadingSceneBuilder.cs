using System.IO;
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
    /// Builds the Loading sample scene and LoadingHeavy, the scene it loads. The
    /// weight constants decide how long the heavy load takes on a phone; the
    /// scene file must stay under 5 MB (SceneLoaderTests checks it). Every run
    /// assigns fresh object IDs, so run it only when this builder changes.
    /// </summary>
    public static class LoadingSceneBuilder
    {
        const string BuilderName = "LoadingSceneBuilder";
        const string ScenePath = "Assets/_Game/Scenes/Loading/Loading.unity";
        const string HeavyScenePath = "Assets/_Game/Scenes/Loading/LoadingHeavy.unity";
        const string TexturePath = "Assets/_Game/Textures/Loading/HeavyTexture.png";
        const string SpriteMaterialPath =
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        // Weight: one uncompressed 2048 x 2048 texture (16 MB in memory, a smooth
        // pattern so the PNG and the APK stay small) and this many static sprites.
        const int TextureSize = 2048;
        const int SpriteCount = 1000;

        [MenuItem("MGD Samples/Build Loading Scene")]
        public static void Build()
        {
            Sprite heavySprite = WriteTexture();

            Scene? scene = BeginScene(BuilderName);
            if (scene == null)
            {
                return;
            }

            BuildLoadingScene();
            FinishScene(scene.Value, ScenePath, BuilderName);

            Scene? heavy = BeginScene(BuilderName);
            if (heavy == null)
            {
                return;
            }

            BuildHeavyScene(heavySprite);
            FinishScene(heavy.Value, HeavyScenePath, BuilderName);
        }

        static void BuildLoadingScene()
        {
            Canvas canvas = CreateCanvas();
            RectTransform root = CreateHudRoot(canvas);

            TextMeshProUGUI title = CreateLabel(root, "Title", "Loading sample", 56f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 100f), stretchWidth: true);

            TextMeshProUGUI body = CreateLabel(root, "Explanation",
                "Every scene change in this app goes through one SceneLoader.\n" +
                "Load heavy scene: the bar follows the real load.\n" +
                "Load with the bug: the classic loop that never ends, stopped by a watchdog.",
                34f);
            Place(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 260f), stretchWidth: true, offset: new Vector2(0f, -120f));

            var buttonSize = new Vector2(640f, 150f);
            Button heavyButton = CreateButton(root, "Button Heavy", "Load heavy scene", buttonSize, 44f);
            Place(heavyButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.55f), buttonSize);
            Button bugButton = CreateButton(root, "Button Bug", "Load with the bug", buttonSize, 44f);
            Place(bugButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.42f), buttonSize);

            RectTransform panel = CreateUiObject("Demo Panel", root);
            Place(panel, new Vector2(0.5f, 0.2f), new Vector2(0f, 300f), stretchWidth: true);
            Slider demoBar = CreateBar(panel, "Demo Bar", new Vector2(800f, 48f));
            Place(demoBar.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(800f, 48f));
            TextMeshProUGUI demoLabel = CreateLabel(panel, "Demo Label", "", 36f);
            Place(demoLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 200f), stretchWidth: true, offset: new Vector2(0f, -70f));

            var back = canvas.gameObject.AddComponent<BackToLauncher>();
            AddPauseMenu(canvas, backTogglesPause: false, addSamplesButton: false);

            var hud = root.gameObject.AddComponent<LoadingDemoHud>();
            SetField(hud, "heavyButton", heavyButton);
            SetField(hud, "bugButton", bugButton);
            SetField(hud, "demoPanel", panel.gameObject);
            SetField(hud, "demoBar", demoBar);
            SetField(hud, "demoLabel", demoLabel);
            SetField(hud, "back", back);

            // Persistent listeners: what a student wires by hand in the Inspector.
            UnityEventTools.AddPersistentListener(heavyButton.onClick, new UnityAction(hud.OnHeavyPressed));
            UnityEventTools.AddPersistentListener(bugButton.onClick, new UnityAction(hud.OnBugPressed));
        }

        static void BuildHeavyScene(Sprite heavySprite)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(SpriteMaterialPath);

            // The texture fills the view, so the load cannot skip it.
            var background = new GameObject("Heavy Background", typeof(SpriteRenderer));
            var backgroundRenderer = background.GetComponent<SpriteRenderer>();
            backgroundRenderer.sprite = heavySprite;
            backgroundRenderer.sharedMaterial = material;
            backgroundRenderer.sortingOrder = -1;
            // The sprite is TextureSize / 100 units wide; scale it to cover 10 units.
            background.transform.localScale = Vector3.one * (1000f / TextureSize);

            // Static sprites with no scripts: pure scene data for the load to read.
            var parent = new GameObject("Weight").transform;
            var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            int columns = Mathf.CeilToInt(Mathf.Sqrt(SpriteCount));
            for (int i = 0; i < SpriteCount; i++)
            {
                var go = new GameObject($"Weight {i}", typeof(SpriteRenderer));
                go.transform.SetParent(parent, false);
                go.transform.localPosition = new Vector3(
                    -2.5f + 5f * (i % columns) / columns,
                    -4.5f + 9f * (i / columns) / columns, 0f);
                var renderer = go.GetComponent<SpriteRenderer>();
                renderer.sprite = knob;
                renderer.sharedMaterial = material;
                renderer.color = new Color(1f, 1f, 1f, 0.25f);
            }

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateHudRoot(canvas);
            TextMeshProUGUI title = CreateLabel(root, "Title", "LoadingHeavy", 56f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 100f), stretchWidth: true);
            TextMeshProUGUI hint = CreateLabel(root, "Hint", $"{SpriteCount} static sprites and a {TextureSize} x {TextureSize} texture.\nCheck the [perf] line in the console.", 32f);
            Place(hint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 120f), stretchWidth: true, offset: new Vector2(0f, -110f));

            var back = canvas.gameObject.AddComponent<BackToLauncher>();
            var buttonSize = new Vector2(520f, 150f);
            Button backButton = CreateButton(root, "Button Back", "Back to samples", buttonSize, 44f);
            Place(backButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), buttonSize);
            UnityEventTools.AddPersistentListener(backButton.onClick, new UnityAction(back.Go));
            AddPauseMenu(canvas, backTogglesPause: false, addSamplesButton: false);
        }

        static Sprite WriteTexture()
        {
            // A smooth two-axis gradient: compresses to almost nothing as a PNG and in
            // the APK, but is still 16 MB of pixels to read at load.
            var tex = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            var pixels = new Color32[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    pixels[y * TextureSize + x] = new Color32(
                        (byte)(x * 255 / TextureSize), (byte)(y * 255 / TextureSize), 96, 255);
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
            importer.mipmapEnabled = false;
            importer.maxTextureSize = TextureSize;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(TexturePath);
        }
    }
}
