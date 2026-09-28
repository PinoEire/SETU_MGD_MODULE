using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static MGD.Samples.Editor.SampleSceneBuild;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Builds the TouchDrag sample scene from a menu item. Every run assigns
    /// fresh object IDs, so run it only when this builder changes.
    /// </summary>
    public static class TouchDragSceneBuilder
    {
        const string ScenePath = "Assets/_Game/Scenes/TouchDrag/TouchDrag.unity";
        const string SpriteMaterialPath =
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
        const float Margin = 96f;

        // The Knob sprite is 0.2 world units across at its 200 pixels per unit;
        // scale 5 makes it 1 unit, about 190 px or 73 dp on a 1080-wide phone at
        // 420 dpi: comfortably above the 48 dp minimum touch target.
        const float SpriteScale = 5f;

        [MenuItem("MGD Samples/Build TouchDrag Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[TouchDragSceneBuilder] Cancelled: current scene has unsaved changes.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera camera = CreateCamera(new Color(0.08f, 0.09f, 0.12f));
            CreateEventSystem();

            var controllerObject = new GameObject("Touch Drag Controller", typeof(TouchDragController));
            var controller = controllerObject.GetComponent<TouchDragController>();
            SetField(controller, "worldCamera", camera);

            Draggable[] draggables =
            {
                CreateDraggable("Red", new Color(0.95f, 0.3f, 0.3f), new Vector2(-1.5f, 1.5f)),
                CreateDraggable("Blue", new Color(0.3f, 0.55f, 1f), new Vector2(1.5f, 1.5f)),
                CreateDraggable("Green", new Color(0.35f, 0.85f, 0.45f), new Vector2(0f, -0.5f))
            };

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateUiObject("Root", canvas.transform);
            Stretch(root, Margin);

            TextMeshProUGUI title = CreateLabel(root, "Title", "TouchDrag sample", 56f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 100f), stretchWidth: true);

            TextMeshProUGUI status = CreateLabel(root, "Status", "", 40f);
            Place(status.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 60f), stretchWidth: true);
            status.rectTransform.anchoredPosition = new Vector2(0f, -110f);

            TextMeshProUGUI hint = CreateLabel(root, "Hint",
                "Touch a circle and drag it. One finger per circle; two fingers move two.\nCircles stay on screen. Android back returns to the list.",
                32f);
            hint.color = new Color(0.75f, 0.78f, 0.85f);
            Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 120f), stretchWidth: true);

            var hud = root.gameObject.AddComponent<TouchDragHud>();
            SetField(hud, "controller", controller);
            SetField(hud, "status", status);
            var props = new SerializedObject(hud);
            SerializedProperty list = props.FindProperty("draggables");
            list.arraySize = draggables.Length;
            for (int i = 0; i < draggables.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = draggables[i];
            }

            props.ApplyModifiedPropertiesWithoutUndo();

            // Android back returns to the launcher; this sample does not use back itself.
            canvas.gameObject.AddComponent<BackToLauncher>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);

            Debug.Log($"[TouchDragSceneBuilder] Saved {ScenePath} and added it to Build Settings.");
        }

        static Draggable CreateDraggable(string label, Color colour, Vector2 position)
        {
            var go = new GameObject(label, typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Draggable));
            go.transform.position = position;
            go.transform.localScale = Vector3.one * SpriteScale;

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(SpriteMaterialPath);
            renderer.color = colour;

            var draggable = go.GetComponent<Draggable>();
            var props = new SerializedObject(draggable);
            props.FindProperty("label").stringValue = label;
            props.ApplyModifiedPropertiesWithoutUndo();
            return draggable;
        }
    }
}
