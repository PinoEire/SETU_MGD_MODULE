using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Helpers shared by the sample scene builders: the scene preamble and
    /// epilogue, camera, event system, canvas and a few uGUI/TextMeshPro
    /// primitives. Editor-only; nothing here ships. Runtime scripts never
    /// reference each other across samples; builders may share this file.
    /// </summary>
    public static class SampleSceneBuild
    {
        /// <summary>Portrait reference resolution used by every sample Canvas.</summary>
        public static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);

        /// <summary>
        /// Inset of each sample's HUD root from the screen edge, in canvas units.
        /// Samples do not reference each other's scripts, so they cannot use the
        /// SafeArea panel; 96 units clears every notch and gesture bar seen so far.
        /// </summary>
        public const float Margin = 96f;

        /// <summary>The dark background every sample camera clears to.</summary>
        public static readonly Color Background = new Color(0.08f, 0.09f, 0.12f);

        /// <summary>
        /// Starts a fresh empty scene with the shared camera and event system, or
        /// returns null if the current scene has unsaved changes the user wants to
        /// keep. Every builder starts here.
        /// </summary>
        public static Scene? BeginScene(string builderName)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log($"[{builderName}] Cancelled: current scene has unsaved changes.");
                return null;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(Background);
            CreateEventSystem();
            return scene;
        }

        /// <summary>Saves the scene at <paramref name="path"/> and registers it in Build Settings.</summary>
        public static void FinishScene(Scene scene, string path, string builderName, bool first = false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            EditorSceneManager.SaveScene(scene, path);
            AddToBuildSettings(path, first);
            Debug.Log($"[{builderName}] Saved {path} and {(first ? "placed it first in" : "added it to")} Build Settings.");
        }

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

        /// <summary>A stretch/stretch root inset by <see cref="Margin"/>, the parent of every HUD element.</summary>
        public static RectTransform CreateHudRoot(Canvas canvas)
        {
            RectTransform root = CreateUiObject("Root", canvas.transform);
            Stretch(root, Margin);
            return root;
        }

        /// <summary>
        /// The pause handling every scene needs (decision 2026-10-04 Every scene
        /// pauses on focus loss): a <see cref="LifecycleGuard"/> on its own object, a
        /// hidden full-screen pause panel with a Resume button, and a
        /// <see cref="PauseMenu"/> on the Canvas. With <paramref name="backTogglesPause"/>
        /// the Android back gesture opens and closes the panel, for a scene that does
        /// not hand back to <see cref="BackToLauncher"/>. A scene that does that and
        /// still has a launcher to return to (Lifecycle) also passes
        /// <paramref name="addSamplesButton"/> and wires the button to
        /// <see cref="BackToLauncher.Go"/>; the Launcher itself leaves it off. Without
        /// the button the card is shorter. <paramref name="cardAnchorY"/> moves the card
        /// off a HUD region that must stay readable while paused. Returns the Back to
        /// samples button, or null when <paramref name="addSamplesButton"/> is off.
        /// </summary>
        public static Button AddPauseMenu(Canvas canvas, bool backTogglesPause, bool addSamplesButton, float cardAnchorY = 0.5f)
        {
            new GameObject("Lifecycle Guard", typeof(LifecycleGuard));

            // Full-screen dim that also blocks taps on the HUD underneath while paused.
            RectTransform panel = CreateUiObject("Pause Panel", canvas.transform);
            Stretch(panel);
            var dim = panel.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.5f);
            dim.raycastTarget = true;

            RectTransform card = CreateUiObject("Card", panel);
            var cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = new Color(0.1f, 0.11f, 0.14f, 1f);
            cardImage.raycastTarget = false;
            Place(card, new Vector2(0.5f, cardAnchorY), new Vector2(900f, addSamplesButton ? 580f : 440f));

            // Rows measured from the top of the card, in canvas units: title 12 to 122
            // (110 tall so a 72 pt title still fits at the Large text size, where
            // TextMeshPro blanks an Ellipsis label whose line does not fit), Resume 150
            // to 300, the optional Samples button 330 to 440, the hint in the bottom 100.
            TextMeshProUGUI title = CreateLabel(card, "Title", "Paused", 72f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 110f), stretchWidth: true, offset: new Vector2(0f, -12f));

            Button resume = CreateButton(card, "Button Resume", "Resume", new Vector2(520f, 150f), 56f);
            Place(resume.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(520f, 150f), offset: new Vector2(0f, -150f));

            Button samples = null;
            if (addSamplesButton)
            {
                samples = CreateButton(card, "Button Samples", "Back to samples", new Vector2(520f, 110f), 40f);
                Place(samples.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(520f, 110f), offset: new Vector2(0f, -330f));
            }

            string hintText = backTogglesPause
                ? "Android back also toggles pause.\nThere is no Quit button."
                : "Android back returns to the samples list.\nThere is no Quit button.";
            TextMeshProUGUI hint = CreateLabel(card, "Hint", hintText, 30f);
            Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 100f), stretchWidth: true, offset: new Vector2(0f, 12f));

            // PauseMenu lives on the Canvas, which stays active, and toggles only the panel.
            var menu = canvas.gameObject.AddComponent<PauseMenu>();
            SetField(menu, "panel", panel.gameObject);
            SetField(menu, "backTogglesPause", backTogglesPause);
            // Persistent listener: what a student wires by hand in the Inspector.
            UnityEventTools.AddPersistentListener(resume.onClick, new UnityAction(menu.OnResumePressed));

            panel.gameObject.SetActive(false);
            return samples;
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
        /// Anchors the rect at one normalised point of its parent, moved by
        /// <paramref name="offset"/> canvas units. With <paramref name="stretchWidth"/>
        /// the rect spans the parent's width and <paramref name="size"/>.x is
        /// ignored. An edge anchor (0 or 1) hugs that edge; an interior anchor
        /// centres the rect on the point.
        /// </summary>
        public static void Place(RectTransform rect, Vector2 anchor, Vector2 size, bool stretchWidth = false, Vector2 offset = default)
        {
            rect.anchorMin = stretchWidth ? new Vector2(0f, anchor.y) : anchor;
            rect.anchorMax = stretchWidth ? new Vector2(1f, anchor.y) : anchor;
            rect.pivot = new Vector2(EdgeOrCentre(anchor.x), EdgeOrCentre(anchor.y));
            rect.sizeDelta = stretchWidth ? new Vector2(0f, size.y) : size;
            rect.anchoredPosition = offset;
        }

        static float EdgeOrCentre(float anchor)
        {
            return anchor <= 0f || anchor >= 1f ? anchor : 0.5f;
        }

        /// <summary>Sets a private [SerializeField] on a freshly added component.</summary>
        public static void SetField(Component target, string field, Object value)
        {
            SerializedObject props = Open(target, field, out SerializedProperty property);
            property.objectReferenceValue = value;
            props.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetField(Component target, string field, bool value)
        {
            SerializedObject props = Open(target, field, out SerializedProperty property);
            property.boolValue = value;
            props.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetField(Component target, string field, string value)
        {
            SerializedObject props = Open(target, field, out SerializedProperty property);
            property.stringValue = value;
            props.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetField(Component target, string field, Object[] values)
        {
            SerializedObject props = Open(target, field, out SerializedProperty property);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            props.ApplyModifiedPropertiesWithoutUndo();
        }

        // A renamed [SerializeField] would otherwise surface as a bare
        // NullReferenceException inside the builder; name the field instead.
        static SerializedObject Open(Component target, string field, out SerializedProperty property)
        {
            var props = new SerializedObject(target);
            property = props.FindProperty(field);
            if (property == null)
            {
                throw new ArgumentException($"{target.GetType().Name} has no serialised field named '{field}'.", nameof(field));
            }

            return props;
        }

        /// <summary>
        /// A TextMeshPro toggle using Unity's built-in skin, resized for touch:
        /// the whole row is the tap target, at least 48 dp tall at the reference
        /// resolution, with a 64 unit box and the label to its right.
        /// </summary>
        public static Toggle CreateToggle(Transform parent, string name, string label, float width, float fontSize = 40f)
        {
            const float rowHeight = 130f;
            const float boxSize = 64f;

            // TMP_DefaultControls has no toggle factory, so the control is assembled
            // by hand: a full-row invisible Image is the tap target, a box Image is
            // the tinted graphic, a Checkmark Image is the on-state graphic.
            RectTransform row = CreateUiObject(name, parent);
            row.sizeDelta = new Vector2(width, rowHeight);
            var hitArea = row.gameObject.AddComponent<Image>();
            hitArea.color = Color.clear;
            hitArea.raycastTarget = true;

            RectTransform box = CreateUiObject("Background", row);
            box.anchorMin = new Vector2(0f, 0.5f);
            box.anchorMax = new Vector2(0f, 0.5f);
            box.pivot = new Vector2(0f, 0.5f);
            box.anchoredPosition = new Vector2(16f, 0f);
            box.sizeDelta = new Vector2(boxSize, boxSize);
            var boxImage = box.gameObject.AddComponent<Image>();
            boxImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            boxImage.type = Image.Type.Sliced;
            boxImage.raycastTarget = false;

            RectTransform check = CreateUiObject("Checkmark", box);
            Stretch(check, 10f);
            var checkImage = check.gameObject.AddComponent<Image>();
            checkImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
            checkImage.preserveAspect = true;
            checkImage.color = new Color(0.1f, 0.1f, 0.12f);
            checkImage.raycastTarget = false;

            TextMeshProUGUI text = CreateLabel(row, "Label", label, fontSize, TextAlignmentOptions.MidlineLeft);
            RectTransform labelRect = text.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(16f + boxSize + 24f, 0f);
            labelRect.offsetMax = Vector2.zero;

            var toggle = row.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = boxImage;
            toggle.graphic = checkImage;
            toggle.isOn = false;
            return toggle;
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
