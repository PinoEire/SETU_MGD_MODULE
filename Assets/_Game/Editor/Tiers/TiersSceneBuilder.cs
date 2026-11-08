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
    /// Builds the Tiers sample scene from a menu item. Every run assigns fresh
    /// object IDs, so run it only when this builder changes. It also creates, once,
    /// the two textures the scene shows, with import settings inside the asset
    /// budgets: the checker (Android ASTC 6x6, 512, mipmaps on) and the bar sprite
    /// (4x4, Android ASTC 4x4, no mipmaps).
    /// </summary>
    public static class TiersSceneBuilder
    {
        const string BuilderName = "TiersSceneBuilder";
        const string ScenePath = "Assets/_Game/Scenes/Tiers/Tiers.unity";
        const string CheckerPath = "Assets/_Game/Textures/Tiers/Checker.png";
        const string BarPath = "Assets/_Game/Textures/Tiers/Bar.png";
        const string SpriteMaterialPath =
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
        const int CheckerSize = 512;

        static readonly string[] OverrideOptions = { "Auto", "Low", "Mid", "High" };

        [MenuItem("MGD Samples/Build Tiers Scene")]
        public static void Build()
        {
            Texture2D checker = CreateCheckerTexture();
            Sprite barSprite = CreateBarSprite();

            Scene? scene = BeginScene(BuilderName);
            if (scene == null)
            {
                return;
            }

            // A thin turning bar in world space: MSAA smooths only geometry edges,
            // so the bar is an opaque sprite on a full-rect quad and its visible
            // edge is the quad's own edge, stepped with MSAA off, smooth at 4x.
            var spinner = new GameObject("Spinner", typeof(SpriteRenderer));
            var bar = spinner.GetComponent<SpriteRenderer>();
            bar.sprite = barSprite;
            bar.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(SpriteMaterialPath);
            bar.color = new Color(0.4f, 0.8f, 1f);
            // In the portrait gap between the checker and the override controls.
            spinner.transform.position = new Vector3(0f, -1.33f, 0f);
            spinner.transform.localScale = new Vector3(2f, 0.06f, 1f);

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateHudRoot(canvas);

            TextMeshProUGUI title = CreateLabel(root, "Title", "Device tiers", 64f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 100f), stretchWidth: true);

            // Four lines at 36 units, about 14 sp on the reference phone.
            TextMeshProUGUI readout = CreateLabel(root, "Readout", "", 36f);
            Place(readout.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 220f), stretchWidth: true, offset: new Vector2(0f, -110f));

            // Shown a little larger than its 512 pixels, so the Low tier's
            // half-resolution mipmap (256) is plainly blurrier than the full one.
            RectTransform checkerRect = CreateUiObject("Checker", root);
            var image = checkerRect.gameObject.AddComponent<RawImage>();
            image.texture = checker;
            image.raycastTarget = false;
            Place(checkerRect, new Vector2(0.5f, 1f), new Vector2(560f, 560f), offset: new Vector2(0f, -350f));

            TextMeshProUGUI overrideLabel = CreateLabel(root, "Override Label", "Tier override (applies on the next launch)", 30f);
            overrideLabel.color = new Color(0.75f, 0.78f, 0.85f);
            Place(overrideLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 60f), stretchWidth: true, offset: new Vector2(0f, 340f));

            TMP_Dropdown dropdown = CreateDropdown(root, "Dropdown Override", OverrideOptions, 600f);
            Place(dropdown.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(600f, 130f), offset: new Vector2(0f, 200f));

            TextMeshProUGUI note = CreateLabel(root, "Override Note", "", 32f);
            Place(note.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 70f), stretchWidth: true, offset: new Vector2(0f, 110f));

            var tiers = root.gameObject.AddComponent<TierReadout>();
            SetField(tiers, "readout", readout);
            SetField(tiers, "overrideDropdown", dropdown);
            SetField(tiers, "overrideNote", note);
            SetField(tiers, "spinner", spinner.transform);
            // Persistent listener of the dynamic int kind: the chosen option is passed through.
            UnityEventTools.AddPersistentListener(dropdown.onValueChanged, new UnityAction<int>(tiers.OnOverrideChanged));

            // Android back returns to the launcher; this sample does not use back
            // itself, so the pause panel's own back toggle is off.
            canvas.gameObject.AddComponent<BackToLauncher>();
            AddPauseMenu(canvas, backTogglesPause: false, addSamplesButton: false);

            FinishScene(scene.Value, ScenePath, BuilderName);
        }

        // A 4x4 opaque white square, imported as a full-rect sprite of one world
        // unit, within the budget rules for sprites. Written once; later runs reuse it.
        static Sprite CreateBarSprite()
        {
            if (!File.Exists(BarPath))
            {
                var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                var pixels = new Color32[16];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(255, 255, 255, 255);
                }

                texture.SetPixels32(pixels);
                Directory.CreateDirectory(Path.GetDirectoryName(BarPath));
                File.WriteAllBytes(BarPath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(BarPath);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(BarPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 4f;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            var spriteSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(spriteSettings);
            spriteSettings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(spriteSettings);
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = 32,
                format = TextureImporterFormat.ASTC_4x4
            });
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(BarPath);
        }

        // Fine 4-pixel squares in dark and light grey with a 1-pixel orange grid
        // every 32: detail that a half-resolution mipmap visibly loses. Written
        // once; later runs reuse it.
        static Texture2D CreateCheckerTexture()
        {
            if (!File.Exists(CheckerPath))
            {
                var texture = new Texture2D(CheckerSize, CheckerSize, TextureFormat.RGBA32, false);
                var pixels = new Color32[CheckerSize * CheckerSize];
                for (int y = 0; y < CheckerSize; y++)
                {
                    for (int x = 0; x < CheckerSize; x++)
                    {
                        bool grid = x % 32 == 0 || y % 32 == 0;
                        bool dark = ((x / 4) + (y / 4)) % 2 == 0;
                        byte grey = dark ? (byte)40 : (byte)200;
                        pixels[y * CheckerSize + x] = grid ? new Color32(255, 140, 0, 255) : new Color32(grey, grey, grey, 255);
                    }
                }

                texture.SetPixels32(pixels);
                Directory.CreateDirectory(Path.GetDirectoryName(CheckerPath));
                File.WriteAllBytes(CheckerPath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(CheckerPath);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(CheckerPath);
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = CheckerSize;
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = CheckerSize,
                format = TextureImporterFormat.ASTC_6x6
            });
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(CheckerPath);
        }
    }
}
