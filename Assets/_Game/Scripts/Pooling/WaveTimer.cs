using System;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MGD.Samples
{
    /// <summary>
    /// Spawns from a <see cref="SpawnPool"/> every <see cref="Interval"/> seconds at
    /// a random point in the upper part of the screen. The loop is an async
    /// Awaitable method, not a coroutine: <c>yield return new WaitForSeconds(s)</c>
    /// becomes <c>await Awaitable.WaitForSecondsAsync(s, ct)</c> and StopCoroutine
    /// becomes <c>Cancel()</c> on the token source. The token is linked to
    /// <c>Application.exitCancellationToken</c>, so the loop also ends when Play mode
    /// stops or the app quits. Passing the token costs a small allocation per wait
    /// (about 48 B, measured in the editor): Unity registers a callback on it so the
    /// wait can end the moment it is cancelled. That is the price of stopping at
    /// once, and it is once per wait, not once per frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WaveTimer : MonoBehaviour
    {
        public const float MinInterval = 0.0625f;
        public const float MaxInterval = 2f;

        [SerializeField] SpawnPool pool;

        [Tooltip("Seconds between spawns.")]
        [SerializeField, Range(MinInterval, MaxInterval)] float interval = 0.5f;

        [Tooltip("Share of the screen height kept clear at the bottom, where the HUD is.")]
        [SerializeField, Range(0f, 0.9f)] float bottomReserved = 0.4f;

        [Tooltip("World units kept between a spawn point and the screen edge.")]
        [SerializeField, Min(0f)] float edgeInset = 0.75f;

        CancellationTokenSource _cts;
        Camera _camera;

        /// <summary>Seconds between spawns, clamped to [<see cref="MinInterval"/>, <see cref="MaxInterval"/>].</summary>
        public float Interval
        {
            get => interval;
            set => interval = ClampInterval(value);
        }

        public static float ClampInterval(float seconds)
        {
            return Mathf.Clamp(seconds, MinInterval, MaxInterval);
        }

        void OnEnable()
        {
            _camera = Camera.main;
            if (pool == null || _camera == null)
            {
                Debug.LogError("[WaveTimer] Needs a Spawn Pool and a camera tagged MainCamera.", this);
                enabled = false;
                return;
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(Application.exitCancellationToken);
            // Fire and forget, like StartCoroutine was. Cancellation is caught inside;
            // any other exception still reaches the console.
            _ = RunAsync(_cts.Token);
        }

        void OnDisable()
        {
            // Null when OnEnable bailed out above.
            if (_cts == null)
            {
                return;
            }

            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        async Awaitable RunAsync(CancellationToken ct)
        {
            try
            {
                while (true)
                {
                    // Wait first. An async method runs synchronously up to its first
                    // await, so spawning before it would run inside OnEnable, possibly
                    // before the pool's Awake has prewarmed it.
                    await Awaitable.WaitForSecondsAsync(interval, ct);

                    // The wait runs on scaled time, so it stalls while LifecycleGuard
                    // holds timeScale at 0; the check also covers a wait that ends on
                    // the frame the pause starts.
                    if (!LifecycleGuard.IsPaused)
                    {
                        Rect area = SpawnArea(_camera, bottomReserved, edgeInset);
                        pool.Spawn(new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax)));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // The normal way out: disabled, destroyed, Play stopped or app quit.
            }
        }

        /// <summary>
        /// The world rectangle an orthographic camera shows, less
        /// <paramref name="bottomReserved"/> of its height at the bottom and
        /// <paramref name="edgeInset"/> world units on every side.
        /// </summary>
        public static Rect SpawnArea(Camera cam, float bottomReserved, float edgeInset)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            Vector3 c = cam.transform.position;
            float bottom = c.y - halfHeight + 2f * halfHeight * bottomReserved;
            return Rect.MinMaxRect(
                c.x - halfWidth + edgeInset, bottom + edgeInset,
                c.x + halfWidth - edgeInset, c.y + halfHeight - edgeInset);
        }
    }
}
