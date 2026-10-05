using System.Collections.Generic;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// An object pool: every item is created in <see cref="Prewarm"/>, during
    /// loading, then handed out by <see cref="Spawn"/> and taken back by
    /// <see cref="Release"/>, so play never calls Instantiate or Destroy, and Spawn
    /// and Release make no garbage while the pool has free items. When every item
    /// is in use, Spawn still works by creating one more and warns once: that is
    /// the sign <c>size</c> is too small. Items are never destroyed, so the pool
    /// only grows, and once the extra items come back the stack grows once to hold
    /// them.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SpawnPool : MonoBehaviour
    {
        [SerializeField] Pooled prefab;

        [Tooltip("Items created at load. Set it to the most ever alive at once, plus a margin.")]
        [SerializeField, Min(1)] int size = 32;

        readonly Stack<Pooled> _free = new Stack<Pooled>();
        bool _warnedEmpty;

        public int Size => size;

        /// <summary>Items this pool has ever made. Stays at <see cref="Size"/> unless the pool ran dry.</summary>
        public int CreatedCount { get; private set; }

        public int FreeCount => _free.Count;

        public int ActiveCount => CreatedCount - _free.Count;

        void Awake()
        {
            Prewarm();
        }

        /// <summary>Creates items until there are <see cref="Size"/>. Safe to call twice.</summary>
        public void Prewarm()
        {
            if (prefab == null)
            {
                Debug.LogError("[SpawnPool] No prefab assigned, so nothing will spawn.", this);
                return;
            }

            while (CreatedCount < size)
            {
                _free.Push(Create());
            }
        }

        /// <summary>
        /// Switches on a free item at <paramref name="position"/>, creating one only
        /// if none is free. Returns null when no prefab is assigned.
        /// </summary>
        public Pooled Spawn(Vector2 position)
        {
            if (prefab == null)
            {
                return null;
            }

            Pooled item;
            if (_free.Count > 0)
            {
                item = _free.Pop();
            }
            else
            {
                item = Create();
                if (!_warnedEmpty)
                {
                    _warnedEmpty = true;
                    Debug.LogWarning($"[SpawnPool] All {size} items were in use, so one was created during play. " +
                                     "Raise size to the most ever alive at once.", this);
                }
            }

            item.transform.position = new Vector3(position.x, position.y, 0f);
            // OnEnable resets the item's state.
            item.gameObject.SetActive(true);
            return item;
        }

        /// <summary>Switches the item off and makes it free again. The pooled version of Destroy.</summary>
        public void Release(Pooled item)
        {
            if (item.Pool != this)
            {
                Debug.LogWarning($"[SpawnPool] {item.name} belongs to another pool; not taken.", this);
                return;
            }

            // A free item is always inactive, so an inactive one has already been
            // released. Pushing it twice would hand it to two owners later.
            if (!item.gameObject.activeSelf)
            {
                Debug.LogWarning($"[SpawnPool] {item.name} was already released; ignored.", this);
                return;
            }

            item.gameObject.SetActive(false);
            _free.Push(item);
        }

        Pooled Create()
        {
            Pooled item = Instantiate(prefab, transform);
            item.gameObject.SetActive(false);
            CreatedCount++;
            return item.Init(this);
        }
    }
}
