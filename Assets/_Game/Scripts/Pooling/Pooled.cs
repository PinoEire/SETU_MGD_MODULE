using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// One thing a <see cref="SpawnPool"/> hands out: an enemy, a chunk, a tile, a
    /// particle. It is reused, not new, so everything that changes during its life
    /// is reset in <see cref="ResetState"/>, which runs every time the pool switches
    /// it on. It drifts, fades and returns itself to the pool when its lifetime runs
    /// out; anything else (a tap, a hit) can return it sooner with <see cref="Release"/>.
    /// Its collider needs a kinematic <see cref="Rigidbody2D"/> beside it: a collider
    /// moved by its transform without a body is "static" to Physics2D, which then
    /// rebuilds the static world every time it moves. The scene builder adds both.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Pooled : MonoBehaviour
    {
        [Tooltip("Seconds before it returns to the pool by itself.")]
        [SerializeField, Min(0.1f)] float lifetime = 4f;

        [Tooltip("World units per second, in a direction picked at each spawn.")]
        [SerializeField, Min(0f)] float driftSpeed = 0.3f;

        [SerializeField] Color colour = Color.white;

        SpawnPool _pool;
        SpriteRenderer _renderer;
        float _age;
        Vector2 _velocity;

        /// <summary>The pool this item came from and goes back to.</summary>
        public SpawnPool Pool => _pool;

        /// <summary>Seconds since the last spawn.</summary>
        public float Age => _age;

        /// <summary>Called once by the pool that created it.</summary>
        public Pooled Init(SpawnPool pool)
        {
            _pool = pool;
            _renderer = GetComponent<SpriteRenderer>();
            return this;
        }

        void OnEnable()
        {
            // OnEnable also runs inside Instantiate, before Init; the pool switches the
            // item off straight away, and every Spawn switches it on again, so the
            // reset that matters is that one.
            if (_pool != null)
            {
                ResetState();
            }
        }

        /// <summary>Everything that changed during the last life goes back to its start value.</summary>
        public void ResetState()
        {
            _age = 0f;
            _velocity = Random.insideUnitCircle * driftSpeed;
            _renderer.color = colour;
        }

        void Update()
        {
            // Time.deltaTime is 0 while LifecycleGuard has paused the game, so a
            // paused item neither ages nor drifts.
            Advance(Time.deltaTime);
        }

        /// <summary>Ages, moves and fades the item; past its lifetime it goes back to the pool.</summary>
        public void Advance(float deltaTime)
        {
            _age += deltaTime;
            if (_age >= lifetime)
            {
                Release();
                return;
            }

            transform.position += (Vector3)(_velocity * deltaTime);
            Color faded = colour;
            faded.a = colour.a * (1f - _age / lifetime);
            _renderer.color = faded;
        }

        /// <summary>Back to the pool: the pooled version of Destroy.</summary>
        public void Release()
        {
            _pool.Release(this);
        }
    }
}
