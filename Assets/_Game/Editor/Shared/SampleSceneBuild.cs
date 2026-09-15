using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Helpers shared by the sample scene builders: camera, event system, canvas
    /// and a few uGUI/TextMeshPro primitives. Editor-only; nothing here ships.
    /// </summary>
    public static class SampleSceneBuild
    {
        /// <summary>Portrait reference resolution used by every sample Canvas.</summary>
        public static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);

        public static Camera CreateCamera(Color background)
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            var cam = go.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            go.transform.position = new Vector3(0f, 0f, -10f);

            // URP needs its per-camera data component; this call adds it if missing.
            cam.GetUniversalAdditionalCameraData();
            return cam;
        }

        public static void CreateEventSystem()
        {
            // The project uses the new Input System, so the UI module must be the
            // Input System one. The legacy StandaloneInputModule would throw at runtime.
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        public static Canvas CreateCanvas()
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

        public static RectTransform CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        /// <summary>Anchors the rect to all four parent edges with the given inset.</summary>
        public static void Stretch(RectTransform rect, float inset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        public static TextMeshProUGUI CreateLabel(Transform parent, string name, string content, float fontSize,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            RectTransform rect = CreateUiObject(name, parent);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        /// <summary>A TextMeshPro button using Unity's built-in UISprite skin.</summary>
        public static Button CreateButton(Transform parent, string name, string label, Vector2 size, float fontSize = 48f)
        {
            var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var resources = new TMP_DefaultControls.Resources { standard = uiSprite };
            GameObject go = TMP_DefaultControls.CreateButton(resources);
            go.name = name;
            go.transform.SetParent(parent, false);
            SetLayerRecursively(go, LayerMask.NameToLayer("UI"));

            go.GetComponent<RectTransform>().sizeDelta = size;

            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = fontSize;
            text.color = Color.black;

            return go.GetComponent<Button>();
        }

        /// <summary>
        /// Anchors the rect at one normalised point of its parent. With
        /// <paramref name="stretchWidth"/> the rect spans the parent's width and
        /// <paramref name="size"/>.x is ignored. An edge anchor (0 or 1) hugs that
        /// edge; an interior anchor centres the rect on the point.
        /// </summary>
        public static void Place(RectTransform rect, Vector2 anchor, Vector2 size, bool stretchWidth = false)
        {
            rect.anchorMin = stretchWidth ? new Vector2(0f, anchor.y) : anchor;
            rect.anchorMax = stretchWidth ? new Vector2(1f, anchor.y) : anchor;
            rect.pivot = new Vector2(EdgeOrCentre(anchor.x), EdgeOrCentre(anchor.y));
            rect.sizeDelta = stretchWidth ? new Vector2(0f, size.y) : size;
            rect.anchoredPosition = Vector2.zero;
        }

        static float EdgeOrCentre(float anchor)
        {
            return anchor <= 0f || anchor >= 1f ? anchor : 0.5f;
        }

        /// <summary>Sets a private [SerializeField] on a freshly added component.</summary>
        public static void SetField(Component target, string field, Object value)
        {
            var props = new SerializedObject(target);
            props.FindProperty(field).objectReferenceValue = value;
            props.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetField(Component target, string field, bool value)
        {
            var props = new SerializedObject(target);
            props.FindProperty(field).boolValue = value;
            props.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        /// <summary>
        /// Registers the scene in Build Settings if it is not there yet. With
        /// <paramref name="first"/> it is moved to index 0, which is where the
        /// launcher must be and what <c>BackToLauncher</c> loads.
        /// </summary>
        public static void AddToBuildSettings(string path, bool first = false)
        {
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int existing = list.FindIndex(s => s.path == path);

            if (existing >= 0 && !first)
            {
                return;
            }

            EditorBuildSettingsScene entry = existing >= 0 ? list[existing] : new EditorBuildSettingsScene(path, true);
            if (existing >= 0)
            {
                list.RemoveAt(existing);
            }

            if (first)
            {
                list.Insert(0, entry);
            }
            else
            {
                list.Add(entry);
            }

            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
