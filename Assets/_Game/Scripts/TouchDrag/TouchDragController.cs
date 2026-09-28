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
    /// <c>Touch.activeTouches</c>: a touch not yet tracked tries to grab whatever
    /// its start position landed on, a tracked touch moves its sprite, an Ended
    /// or Canceled touch drops it, and anything the tracker still holds for a
    /// touch that is no longer listed is dropped too. Reading state every frame instead of reacting to events is
    /// what makes it survive a trip to the background mid-drag.
    /// Nothing here allocates per frame.
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

        public DragTracker Tracker => _tracker;

        /// <summary>Fingers on the screen this frame, held or not.</summary>
        public int FingerCount => Touch.activeTouches.Count;

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

            Rect visible = VisibleWorldRect(worldCamera);
            _tracker.Bounds = Rect.MinMaxRect(
                visible.xMin + edgeInset, visible.yMin + edgeInset,
                visible.xMax - edgeInset, visible.yMax - edgeInset);
        }

        void OnDisable()
        {
            // No touches are alive once disabled, so every drag is released.
            _alive.Clear();
            _tracker.Prune(_alive, _released);
            ReleaseVisuals();
#if UNITY_EDITOR
            TouchSimulation.Disable();
#endif
            EnhancedTouchSupport.Disable();
        }

        void Update()
        {
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
                else
                {
                    // Not only on Began: touch is sampled faster than the game
                    // runs, so a fast finger can be first seen already Moved. The
                    // grab still uses where the finger landed, so a finger that
                    // started on empty space never picks anything up on the way.
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
            // startScreenPosition, not screenPosition: a fast finger may already
            // have moved by the time its first frame is read.
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
                _tracker.End(touchId);
            }
        }

        void ReleaseVisuals()
        {
            for (int i = 0; i < _released.Count; i++)
            {
                if (_released[i] != null && _released[i].TryGetComponent(out Draggable draggable))
                {
                    draggable.SetHeld(false);
                }
            }

            _released.Clear();
        }

        static Rect VisibleWorldRect(Camera cam)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            Vector3 c = cam.transform.position;
            return new Rect(c.x - halfWidth, c.y - halfHeight, halfWidth * 2f, halfHeight * 2f);
        }
    }
}
