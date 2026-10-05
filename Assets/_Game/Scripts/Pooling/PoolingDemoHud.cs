using TMPro;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Demo scaffolding for the Pooling scene, not something to copy into a game:
    /// shows the pool's counters and the spawn interval, and wires the Faster and
    /// Slower buttons. The counters line is rewritten only when a number changes,
    /// through SetText with numeric arguments, which formats into TextMeshPro's own
    /// buffer: no string is built in a player build. In the editor TextMeshPro also
    /// copies the text into a string for the Inspector, so the editor Profiler shows
    /// a small allocation here that the phone does not.
    /// </summary>
    public sealed class PoolingDemoHud : MonoBehaviour
    {
        [SerializeField] SpawnPool pool;
        [SerializeField] WaveTimer timer;
        [SerializeField] TMP_Text counters;
        [SerializeField] TMP_Text intervalLabel;

        int _active = -1;
        int _free = -1;
        int _created = -1;

        void Start()
        {
            RefreshInterval();
        }

        void Update()
        {
            int active = pool.ActiveCount;
            int free = pool.FreeCount;
            int created = pool.CreatedCount;
            if (active == _active && free == _free && created == _created)
            {
                return;
            }

            _active = active;
            _free = free;
            _created = created;
            counters.SetText("active {0} / free {1} / created {2}", active, free, created);
        }

        public void OnFasterPressed()
        {
            timer.Interval *= 0.5f;
            RefreshInterval();
        }

        public void OnSlowerPressed()
        {
            timer.Interval *= 2f;
            RefreshInterval();
        }

        void RefreshInterval()
        {
            intervalLabel.SetText("one every {0} ms", timer.Interval * 1000f);
        }
    }
}
