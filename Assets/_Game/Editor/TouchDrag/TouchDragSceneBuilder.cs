using TMPro;
using UnityEditor;
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
        const string BuilderName = "TouchDragSceneBuilder";
        const string ScenePath = "Assets/_Game/Scenes/TouchDrag/TouchDrag.unity";
        const string SpriteMaterialPath =
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        // The Knob sprite is 0.2 world units across at its 200 pixels per unit;
        // scale 5 makes it 1 unit. The camera shows 10 units of height, so on a
        // 1080 x 2400 phone that is 240 px, about 90 dp at 420 dpi, and on the
        // 1080 x 1920 reference 192 px, about 73 dp: either way well above the
        // 48 dp minimum touch target.
        const float SpriteScale = 5f;

        [MenuItem("MGD Samples/Build TouchDrag Scene")]
        public static void Build()
        {
            Scene? scene = BeginScene(BuilderName);
            if (scene == null)
            {
                return;
            }

            var controllerObject = new GameObject("Touch Drag Controller", typeof(TouchDragController));
            var controller = controllerObject.GetComponent<TouchDragController>();
            SetField(controller, "worldCamera", Camera.main);

            Draggable[] draggables =
            {
                CreateDraggable("Red", new Color(0.95f, 0.3f, 0.3f), new Vector2(-1.5f, 1.5f)),
                CreateDraggable("Blue", new Color(0.3f, 0.55f, 1f), new Vector2(1.5f, 1.5f)),
                CreateDraggable("Green", new Color(0.35f, 0.85f, 0.45f), new Vector2(0f, -0.5f))
            };

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateHudRoot(canvas);

            TextMeshProUGUI title = CreateLabel(root, "Title", "TouchDrag sample", 56f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 100f), stretchWidth: true);

            TextMeshProUGUI status = CreateLabel(root, "Status", "", 40f);
            Place(status.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 60f), stretchWidth: true, offset: new Vector2(0f, -110f));

            TextMeshProUGUI hint = CreateLabel(root, "Hint",
                "Touch a circle and drag it. One finger per circle; two fingers move two.\nCircles stay on screen. Android back returns to the list.",
                32f);
            hint.color = new Color(0.75f, 0.78f, 0.85f);
            Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 120f), stretchWidth: true);

            var hud = root.gameObject.AddComponent<TouchDragHud>();
            SetField(hud, "controller", controller);
            SetField(hud, "status", status);
            SetField(hud, "draggables", draggables);

            // Android back returns to the launcher; this sample does not use back itself,
            // so the pause panel's own back toggle is off.
            canvas.gameObject.AddComponent<BackToLauncher>();
            AddPauseMenu(canvas, backTogglesPause: false, addSamplesButton: false);

            FinishScene(scene.Value, ScenePath, BuilderName);
        }

        static Draggable CreateDraggable(string label, Color colour, Vector2 position)
        {
            var go = new GameObject(label, typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D), typeof(Draggable));
            go.transform.position = position;
            go.transform.localScale = Vector3.one * SpriteScale;

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(SpriteMaterialPath);
            renderer.color = colour;

            // CircleCollider2D defaults to radius 0.5 in local units, which the
            // scale of 5 turns into a 5-unit-wide target around a 1-unit circle.
            // Match the sprite: its local extents are 0.1, so 0.5 world units.
            var collider = go.GetComponent<CircleCollider2D>();
            collider.radius = renderer.sprite.bounds.extents.x;

            // Kinematic: moved by the drag, never by physics (so gravity does not
            // apply), and cheap to move. Draggable warns at runtime if this is changed.
            go.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

            var draggable = go.GetComponent<Draggable>();
            SetField(draggable, "label", label);
            return draggable;
        }
    }
}
