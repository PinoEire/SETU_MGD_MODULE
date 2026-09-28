using UnityEngine;

namespace MGD.Samples
{
    public enum LoadState
    {
        Idle,
        Steady,
        Worst
    }

    /// <summary>
    /// A field of bouncing sprites: the scene's worst case, with a knob to make it
    /// as bad as a given phone needs. Every sprite is created once in Awake and
    /// switched on and off by <see cref="SetLoad"/>, so the steady state never calls
    /// Instantiate or Destroy: the crude form of the pooling Week 5 does properly.
    /// Positions and velocities live in arrays and one loop moves the active ones.
    /// Worst also scales the sprites up so they overlap, which adds overdraw (a
    /// GPU cost) on top of the transform work (a CPU cost).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LoadGenerator : MonoBehaviour
    {
        [SerializeField] Sprite sprite;
        [SerializeField] Material material;

        [Tooltip("Sprites active in the Steady state.")]
        [SerializeField, Min(0)] int steadyCount = 200;

        [Tooltip("Sprites active in the Worst state. This many are created at load.")]
        [SerializeField, Min(1)] int worstCount = 1500;

        [SerializeField, Min(0.1f)] float steadyScale = 1f;
        [SerializeField, Min(0.1f)] float worstScale = 4f;

        [Tooltip("World units per second.")]
        [SerializeField, Min(0f)] float speed = 3f;

        Transform[] _transforms;
        GameObject[] _objects;
        Vector2[] _positions;
        Vector2[] _velocities;
        Vector2 _halfExtents;
        int _active;

        public LoadState State { get; private set; } = LoadState.Idle;

        /// <summary>How many sprites are moving right now.</summary>
        public int ActiveCount => _active;

        void Awake()
        {
            Camera cam = Camera.main;
            float halfHeight = cam.orthographicSize;
            _halfExtents = new Vector2(halfHeight * cam.aspect, halfHeight);

            _transforms = new Transform[worstCount];
            _objects = new GameObject[worstCount];
            _positions = new Vector2[worstCount];
            _velocities = new Vector2[worstCount];

            for (int i = 0; i < worstCount; i++)
            {
                var go = new GameObject("Sprite");
                go.transform.SetParent(transform, false);

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                if (material != null)
                {
                    renderer.sharedMaterial = material;
                }

                // Half transparent so overlapping sprites all get drawn: overdraw.
                renderer.color = new Color(Random.value, Random.value, Random.value, 0.5f);

                _positions[i] = new Vector2(
                    Random.Range(-_halfExtents.x, _halfExtents.x),
                    Random.Range(-_halfExtents.y, _halfExtents.y));
                _velocities[i] = Random.insideUnitCircle.normalized * speed;

                _transforms[i] = go.transform;
                _objects[i] = go;
                go.SetActive(false);
            }

            SetLoad(LoadState.Idle);
        }

        /// <summary>Wired to the Idle, Steady and Worst buttons through the HUD.</summary>
        public void SetLoad(LoadState state)
        {
            State = state;

            // Steady can never exceed what was created, whatever the Inspector says.
            int count = state switch
            {
                LoadState.Steady => Mathf.Min(steadyCount, worstCount),
                LoadState.Worst => worstCount,
                _ => 0
            };
            float scale = state == LoadState.Worst ? worstScale : steadyScale;

            for (int i = 0; i < worstCount; i++)
            {
                bool on = i < count;
                if (_objects[i].activeSelf != on)
                {
                    _objects[i].SetActive(on);
                }

                if (on)
                {
                    _transforms[i].localScale = new Vector3(scale, scale, 1f);
                    _transforms[i].localPosition = _positions[i];
                }
            }

            _active = count;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < _active; i++)
            {
                Vector2 p = _positions[i] + _velocities[i] * dt;

                if (p.x < -_halfExtents.x || p.x > _halfExtents.x)
                {
                    _velocities[i].x = -_velocities[i].x;
                    p.x = Mathf.Clamp(p.x, -_halfExtents.x, _halfExtents.x);
                }

                if (p.y < -_halfExtents.y || p.y > _halfExtents.y)
                {
                    _velocities[i].y = -_velocities[i].y;
                    p.y = Mathf.Clamp(p.y, -_halfExtents.y, _halfExtents.y);
                }

                _positions[i] = p;
                _transforms[i].localPosition = p;
            }
        }
    }
}
