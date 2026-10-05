using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MGD.Samples
{
    /// <summary>
    /// A finger landing on a <see cref="Pooled"/> item sends it back to its pool:
    /// the stand-in for whatever your core verb does to an enemy, a coin or a tile.
    /// Reads Enhanced Touch (the mouse is one finger in the editor) and hit-tests
    /// with <c>Physics2D.OverlapPoint</c>, which allocates nothing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TapToRelease : MonoBehaviour
    {
        [Tooltip("Camera the items are seen through. Main Camera when empty.")]
        [SerializeField] Camera worldCamera;

        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
#if UNITY_EDITOR
            TouchSimulation.Enable();
#endif
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            if (worldCamera == null)
            {
                Debug.LogError("[TapToRelease] No camera: assign World Camera or tag one MainCamera.", this);
                enabled = false;
            }
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            TouchSimulation.Disable();
#endif
            EnhancedTouchSupport.Disable();
        }

        void Update()
        {
            // Enhanced Touch bypasses the UI, so the pause panel does not block a
            // finger from reaching the items: gameplay checks the guard itself.
            if (LifecycleGuard.IsPaused)
            {
                return;
            }

            var touches = Touch.activeTouches;
            for (int i = 0; i < touches.Count; i++)
            {
                Touch touch = touches[i];
                if (touch.phase != TouchPhase.Began)
                {
                    continue;
                }

                Vector2 world = worldCamera.ScreenToWorldPoint(touch.screenPosition);
                Collider2D hit = Physics2D.OverlapPoint(world);
                if (hit != null && hit.TryGetComponent(out Pooled item))
                {
                    item.Release();
                }
            }
        }
    }
}
