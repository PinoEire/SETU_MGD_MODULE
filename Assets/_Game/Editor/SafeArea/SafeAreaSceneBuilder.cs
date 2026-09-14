using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Builds the SafeArea sample scene from a menu item so the scene is reproducible
    /// and its construction is readable. Running it again overwrites the scene and
    /// keeps the generated background texture.
    /// </summary>
    public static class SafeAreaSceneBuilder
    {
        const string ScenePath = "Assets/_Game/Scenes/SafeArea/SafeArea.unity";
        const string TexturePath = "Assets/_Game/Textures/SafeArea/GridBackground.png";

        // Portrait reference resolution for the Canvas Scaler. 1080 x 1920 matches the
        // lab sheet; real phones scale from it.
        static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);

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
            Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();

            Canvas canvas = CreateCanvas();
            CreateBackground(canvas.transform, grid);
            RectTransform safeArea = CreateSafeAreaPanel(canvas.transform);
            CreateHud(safeArea, uiSprite);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);

            Debug.Log($"[SafeAreaSceneBuilder] Saved {ScenePath} and added it to Build Settings.");
        }

        static void CreateCamera()
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            var cam = go.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            go.transform.position = new Vector3(0f, 0f, -10f);

            // URP needs its per-camera data component; this call adds it if missing.
            cam.GetUniversalAdditionalCameraData();
        }

        static void CreateEventSystem()
        {
            // The project uses the new Input System, so the UI module must be the
            // Input System one. The legacy StandaloneInputModule would throw at runtime.
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.layer = LayerMask.NameToLayer("UI");

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
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

        static void CreateHud(RectTransform safeArea, Sprite uiSprite)
        {
            CreateCornerButton(safeArea, uiSprite, "TL", new Vector2(0f, 1f), new Vector2(EdgePadding, -EdgePadding));
            CreateCornerButton(safeArea, uiSprite, "TR", new Vector2(1f, 1f), new Vector2(-EdgePadding, -EdgePadding));
            CreateCornerButton(safeArea, uiSprite, "BL", new Vector2(0f, 0f), new Vector2(EdgePadding, EdgePadding));
            CreateCornerButton(safeArea, uiSprite, "BR", new Vector2(1f, 0f), new Vector2(-EdgePadding, EdgePadding));

            // Score label across the top, between the two top buttons.
            TextMeshProUGUI score = CreateLabel(safeArea, "Score", "SCORE 0000", 64f);
            RectTransform scoreRect = score.rectTransform;
            scoreRect.anchorMin = new Vector2(0f, 1f);
            scoreRect.anchorMax = new Vector2(1f, 1f);
            scoreRect.pivot = new Vector2(0.5f, 1f);
            scoreRect.offsetMin = new Vector2(ButtonSize + EdgePadding * 2f, -(ButtonSize + EdgePadding));
            scoreRect.offsetMax = new Vector2(-(ButtonSize + EdgePadding * 2f), -EdgePadding);

            // Live readout of the safe-area numbers, bottom centre.
            TextMeshProUGUI readout = CreateLabel(safeArea, "Readout", "", 36f);
            readout.alignment = TextAlignmentOptions.Bottom;
            RectTransform readoutRect = readout.rectTransform;
            readoutRect.anchorMin = new Vector2(0f, 0f);
            readoutRect.anchorMax = new Vector2(1f, 0f);
            readoutRect.pivot = new Vector2(0.5f, 0f);
            readoutRect.offsetMin = new Vector2(ButtonSize + EdgePadding * 2f, EdgePadding);
            readoutRect.offsetMax = new Vector2(-(ButtonSize + EdgePadding * 2f), ButtonSize + EdgePadding);
            readout.gameObject.AddComponent<SafeAreaReadout>();
        }

        static void CreateCornerButton(RectTransform parent, Sprite uiSprite, string label, Vector2 anchor, Vector2 offset)
        {
            var resources = new TMP_DefaultControls.Resources { standard = uiSprite };
            GameObject go = TMP_DefaultControls.CreateButton(resources);
            go.name = $"Button {label}";
            go.transform.SetParent(parent, false);
            SetLayerRecursively(go, LayerMask.NameToLayer("UI"));

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = new Vector2(ButtonSize, ButtonSize);
            rect.anchoredPosition = offset;

            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 48f;
            text.color = Color.black;
        }

        static TextMeshProUGUI CreateLabel(RectTransform parent, string name, string content, float fontSize)
        {
            RectTransform rect = CreateUiObject(name, parent);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        static RectTransform CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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

        static void AddToBuildSettings(string path)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene s in scenes)
            {
                if (s.path == path)
                {
                    return;
                }
            }

            var list = new EditorBuildSettingsScene[scenes.Length + 1];
            scenes.CopyTo(list, 0);
            list[scenes.Length] = new EditorBuildSettingsScene(path, true);
            EditorBuildSettings.scenes = list;
        }
    }
}
