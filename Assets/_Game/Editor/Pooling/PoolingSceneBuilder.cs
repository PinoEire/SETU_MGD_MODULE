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
    /// Builds the Pooling sample: writes the item prefab, then the scene. Every run
    /// assigns fresh object IDs, so run it only when this builder changes.
    /// </summary>
    public static class PoolingSceneBuilder
    {
        const string BuilderName = "PoolingSceneBuilder";
        const string ScenePath = "Assets/_Game/Scenes/Pooling/Pooling.unity";
        const string PrefabPath = "Assets/_Game/Prefabs/Pooling/Pooled.prefab";
        const string SpriteMaterialPath =
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        // The Knob sprite is 0.2 world units across at 200 pixels per unit; scale 5
        // makes it 1 unit, about 73 dp on the reference phone and 90 dp on a
        // 1080 x 2400 one: above the 48 dp minimum touch target.
        const float ItemScale = 5f;

        static readonly Color ItemColour = new Color(0.3f, 0.8f, 0.75f);

        [MenuItem("MGD Samples/Build Pooling Scene")]
        public static void Build()
        {
            Scene? scene = BeginScene(BuilderName);
            if (scene == null)
            {
                return;
            }

            Pooled prefab = WritePrefab();

            var poolObject = new GameObject("Spawn Pool", typeof(SpawnPool), typeof(WaveTimer), typeof(TapToRelease));
            var pool = poolObject.GetComponent<SpawnPool>();
            SetField(pool, "prefab", prefab);
            var timer = poolObject.GetComponent<WaveTimer>();
            SetField(timer, "pool", pool);
            SetField(poolObject.GetComponent<TapToRelease>(), "worldCamera", Camera.main);

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateHudRoot(canvas);
            CreateHudCard(root, pool, timer);

            // Android back returns to the launcher; this sample does not use back
            // itself, so the pause panel's own back toggle is off.
            canvas.gameObject.AddComponent<BackToLauncher>();
            AddPauseMenu(canvas, backTogglesPause: false, addSamplesButton: false);

            FinishScene(scene.Value, ScenePath, BuilderName);
        }

        static Pooled WritePrefab()
        {
            var go = new GameObject("Pooled", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D),
                typeof(Pooled));
            go.transform.localScale = Vector3.one * ItemScale;

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(SpriteMaterialPath);
            renderer.color = ItemColour;

            // CircleCollider2D defaults to radius 0.5 local units, which scale 5 would
            // turn into a target five times the circle; match the sprite instead.
            go.GetComponent<CircleCollider2D>().radius = renderer.sprite.bounds.extents.x;

            // Kinematic: the item moves its own transform every frame, and a collider
            // moved without a body is "static" to Physics2D, which then rebuilds the
            // static world on every move. Kinematic bodies ignore gravity and forces.
            go.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

            var props = new SerializedObject(go.GetComponent<Pooled>());
            props.FindProperty("colour").colorValue = ItemColour;
            props.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            GameObject asset = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            return asset.GetComponent<Pooled>();
        }

        static void CreateHudCard(RectTransform root, SpawnPool pool, WaveTimer timer)
        {
            // A translucent card in the bottom third, where WaveTimer's bottomReserved
            // keeps spawns away, with the buttons in thumb reach.
            RectTransform card = CreateUiObject("HUD Card", root);
            var cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = new Color(0.08f, 0.09f, 0.12f, 0.85f);
            cardImage.raycastTarget = false;
            Place(card, new Vector2(0.5f, 0f), new Vector2(0f, 560f), stretchWidth: true);

            TextMeshProUGUI title = CreateLabel(card, "Title", "Pooling sample", 56f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 100f), stretchWidth: true);

            TextMeshProUGUI counters = CreateLabel(card, "Counters", "", 40f);
            Place(counters.rectTransform, new Vector2(0.5f, 0.7f), new Vector2(0f, 60f), stretchWidth: true);

            TextMeshProUGUI intervalLabel = CreateLabel(card, "Interval", "", 40f);
            Place(intervalLabel.rectTransform, new Vector2(0.5f, 0.56f), new Vector2(0f, 60f), stretchWidth: true);

            // 130 units tall: about 49 dp on the reference phone, above the 48 dp
            // minimum. Two across at 0.3 and 0.7 leave a gap on a 20:9 phone too.
            var buttonSize = new Vector2(260f, 130f);
            Button slower = CreateButton(card, "Button Slower", "Slower", buttonSize, 40f);
            Place(slower.GetComponent<RectTransform>(), new Vector2(0.3f, 0.24f), buttonSize);
            Button faster = CreateButton(card, "Button Faster", "Faster", buttonSize, 40f);
            Place(faster.GetComponent<RectTransform>(), new Vector2(0.7f, 0.24f), buttonSize);

            var hud = card.gameObject.AddComponent<PoolingDemoHud>();
            SetField(hud, "pool", pool);
            SetField(hud, "timer", timer);
            SetField(hud, "counters", counters);
            SetField(hud, "intervalLabel", intervalLabel);

            // Persistent listeners: what a student wires by hand in the Inspector.
            UnityEventTools.AddPersistentListener(slower.onClick, new UnityAction(hud.OnSlowerPressed));
            UnityEventTools.AddPersistentListener(faster.onClick, new UnityAction(hud.OnFasterPressed));
        }
    }
}
