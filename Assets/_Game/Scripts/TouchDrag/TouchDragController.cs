using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MGD.Samples
{
    /// <summary>
    /// Drags <see cref="Draggable"/> sprites with fingers, one finger per sprite,
    /// through the Input System's Enhanced Touch API. Each frame it walks
    /// <c>Touch.activeTouches</c>: a touch seen for the first time gets one
    /// chance to grab whatever its start position landed on, a tracked touch
    /// moves its sprite, an Ended or Canceled touch drops it, and anything the
    /// tracker still holds for a touch that is no longer listed is dropped too.
    /// Reading state every frame instead of reacting to events is what makes it
    /// survive a trip to the background mid-drag. Nothing here allocates per
    /// frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TouchDragController : MonoBehaviour
    {
        [Tooltip("Camera the sprites are seen through. Main Camera when empty.")]
        [SerializeField] Camera worldCamera;

        [Tooltip("World units kept between a dragged centre and the screen edge, so a sprite stays fully visible.")]
        [SerializeField, Min(0f)] float edgeInset = 0.5f;

        readonly DragTracker _tracker = new DragTracker();
        readonly List<int> _alive = new List<int>(10);
        readonly List<Transform> _released = new List<Transform>(10);
        int _boundsWidth;
        int _boundsHeight;

        public DragTracker Tracker => _tracker;

        /// <summary>Fingers on the screen this frame, held or not. Zero while this component is disabled.</summary>
        public int FingerCount => EnhancedTouchSupport.enabled ? Touch.activeTouches.Count : 0;

        void OnEnable()
        {
            // Touch.activeTouches is empty until Enhanced Touch is switched on.
            EnhancedTouchSupport.Enable();
#if UNITY_EDITOR
            // The mouse acts as one finger in Play mode; never shipped.
            TouchSimulation.Enable();
#endif
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            if (worldCamera == null)
            {
                Debug.LogError("[TouchDragController] No camera: assign World Camera or tag one MainCamera.", this);
                enabled = false;
                return;
            }

            UpdateBounds();
            LifecycleGuard.PausedChanged += OnPausedChanged;
        }

        void OnDisable()
        {
            LifecycleGuard.PausedChanged -= OnPausedChanged;
            // No touches are alive once disabled, so every drag is released.
            ReleaseEverything();
#if UNITY_EDITOR
            TouchSimulation.Disable();
#endif
            EnhancedTouchSupport.Disable();
        }

        void Update()
        {
            // Enhanced Touch bypasses the UI raycast, so the pause panel's dim does not
            // stop a finger reaching the sprites: gameplay gates itself on the guard.
            // Skipping frames is not enough on its own, because touches that end and
            // begin while paused go unseen and Android reuses their ids; the drags
            // are released when the pause lands (OnPausedChanged), so a finger still
            // down after Resume is a fresh touch that must lift and grab again.
            if (LifecycleGuard.IsPaused)
            {
                return;
            }

            // Rotation changes the visible rectangle; two int compares per frame.
            if (Screen.width != _boundsWidth || Screen.height != _boundsHeight)
            {
                UpdateBounds();
            }

            _alive.Clear();
            var touches = Touch.activeTouches;
            for (int i = 0; i < touches.Count; i++)
            {
                Touch touch = touches[i];
                _alive.Add(touch.touchId);

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    Release(touch.touchId);
                }
                else if (_tracker.TryGetTarget(touch.touchId, out _))
                {
                    Move(touch);
                }
                else if (_tracker.ShouldAttempt(touch.touchId))
                {
                    // Once per touch, the first time it is seen, whatever its phase.
                    // The grab uses where the finger landed, and a finger that
                    // missed does not get a second try, so nothing dragged under it
                    // later can be snatched.
                    TryGrab(touch);
                }
            }

            // _released already holds this frame's Ended targets; Prune adds any
            // whose touch vanished without an Ended, then all of them reset.
            _tracker.Prune(_alive, _released);
            ReleaseVisuals();
        }

        void TryGrab(Touch touch)
        {
            Vector2 world = worldCamera.ScreenToWorldPoint(touch.startScreenPosition);
            Collider2D hit = Physics2D.OverlapPoint(world);
            if (hit != null && hit.TryGetComponent(out Draggable draggable) &&
                _tracker.TryBegin(touch.touchId, draggable.transform, world))
            {
                draggable.SetHeld(true);
            }
        }

        void Move(Touch touch)
        {
            Vector2 world = worldCamera.ScreenToWorldPoint(touch.screenPosition);
            if (_tracker.TryMove(touch.touchId, world, out Vector2 position) &&
                _tracker.TryGetTarget(touch.touchId, out Transform target))
            {
                target.position = new Vector3(position.x, position.y, target.position.z);
            }
        }

        void Release(int touchId)
        {
            if (_tracker.TryGetTarget(touchId, out Transform target))
            {
                _released.Add(target);
            }

            // Forget the attempt as well as the drag: Android can reuse this id for
            // a new contact in the very next frame.
            _tracker.Forget(touchId);
        }

        void ReleaseVisuals()
        {
            for (int i = 0; i < _released.Count; i++)
            {
                // A sprite can change hands within one frame: finger A's Ended is
                // listed before finger B's Began on the same spot. B holds it now,
                // so it must stay drawn as held.
                if (_released[i] != null && !_tracker.IsHeld(_released[i]) &&
                    _released[i].TryGetComponent(out Draggable draggable))
                {
                    draggable.SetHeld(false);
                }
            }

            _released.Clear();
        }

        void OnPausedChanged(bool paused)
        {
            if (paused)
            {
                ReleaseEverything();
            }
        }

        void ReleaseEverything()
        {
            _alive.Clear();
            _tracker.Prune(_alive, _released);
            ReleaseVisuals();
        }

        void UpdateBounds()
        {
            _boundsWidth = Screen.width;
            _boundsHeight = Screen.height;
            Rect visible = VisibleWorldRect(worldCamera);
            _tracker.Bounds = Rect.MinMaxRect(
                visible.xMin + edgeInset, visible.yMin + edgeInset,
                visible.xMax - edgeInset, visible.yMax - edgeInset);
        }

        /// <summary>The world-space rectangle an orthographic camera shows.</summary>
        public static Rect VisibleWorldRect(Camera cam)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            Vector3 c = cam.transform.position;
            return new Rect(c.x - halfWidth, c.y - halfHeight, halfWidth * 2f, halfHeight * 2f);
        }
    }
}
