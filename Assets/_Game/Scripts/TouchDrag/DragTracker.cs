using System.Collections.Generic;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Which finger holds which object, and where it grabbed it. Pure logic with
    /// no input code, so it is unit tested and the controller stays short.
    ///
    /// Drags are keyed by <c>touchId</c>, never by finger index: Enhanced Touch
    /// reuses a finger slot the moment a finger lifts, so a thumb replanted
    /// elsewhere would otherwise look like a continuation of the old drag.
    /// The grab offset is kept so the object does not jump to the fingertip.
    /// </summary>
    public sealed class DragTracker
    {
        struct Drag
        {
            public Transform Target;
            public Vector2 Offset;
        }

        readonly Dictionary<int, Drag> _drags = new Dictionary<int, Drag>();
        readonly HashSet<int> _attempted = new HashSet<int>();
        readonly List<int> _gone = new List<int>();

        /// <summary>Optional world-space rectangle the dragged centre is clamped to.</summary>
        public Rect? Bounds { get; set; }

        public int ActiveCount => _drags.Count;

        /// <summary>
        /// True the first time it is asked about a touch, false after that until the
        /// touch is forgotten or pruned. A finger gets one chance to grab, where it
        /// landed; a finger that missed must not keep trying and snatch an object
        /// that is dragged under it later, and a finger refused because the object
        /// was already held must not take it the moment the other finger lets go.
        /// </summary>
        public bool ShouldAttempt(int touchId)
        {
            return _attempted.Add(touchId);
        }

        /// <summary>
        /// Drops everything known about a touch the moment it ends. Android may give
        /// a new contact the same id straight away, before a frame without it has
        /// let <see cref="Prune"/> clear the attempt, so the end of a touch must
        /// clear it itself.
        /// </summary>
        public void Forget(int touchId)
        {
            _drags.Remove(touchId);
            _attempted.Remove(touchId);
        }

        /// <summary>Starts a drag. Refused if the touch or the target is already busy.</summary>
        public bool TryBegin(int touchId, Transform target, Vector2 worldPoint)
        {
            if (target == null || _drags.ContainsKey(touchId) || IsHeld(target))
            {
                return false;
            }

            _drags[touchId] = new Drag
            {
                Target = target,
                Offset = (Vector2)target.position - worldPoint
            };
            return true;
        }

        /// <summary>The centre the target should move to for this finger position.</summary>
        public bool TryMove(int touchId, Vector2 worldPoint, out Vector2 position)
        {
            if (!_drags.TryGetValue(touchId, out Drag drag))
            {
                position = default;
                return false;
            }

            position = worldPoint + drag.Offset;
            if (Bounds.HasValue)
            {
                Rect b = Bounds.Value;
                position.x = Mathf.Clamp(position.x, b.xMin, b.xMax);
                position.y = Mathf.Clamp(position.y, b.yMin, b.yMax);
            }

            return true;
        }

        public bool TryGetTarget(int touchId, out Transform target)
        {
            bool found = _drags.TryGetValue(touchId, out Drag drag);
            target = found ? drag.Target : null;
            return found;
        }

        public bool IsHeld(Transform target)
        {
            foreach (Drag drag in _drags.Values)
            {
                if (drag.Target == target)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Forgets every touch that is not in <paramref name="aliveTouchIds"/>: its
        /// drag is dropped and its grab attempt cleared. A finger that vanished
        /// without an Ended phase (the app went to the background mid-drag) must
        /// not leave its object stuck to nothing. Released targets are added to
        /// <paramref name="released"/> when given.
        /// </summary>
        public void Prune(IReadOnlyList<int> aliveTouchIds, List<Transform> released = null)
        {
            _gone.Clear();
            foreach (KeyValuePair<int, Drag> pair in _drags)
            {
                if (!Contains(aliveTouchIds, pair.Key))
                {
                    _gone.Add(pair.Key);
                    released?.Add(pair.Value.Target);
                }
            }

            for (int i = 0; i < _gone.Count; i++)
            {
                _drags.Remove(_gone[i]);
            }

            _gone.Clear();
            foreach (int touchId in _attempted)
            {
                if (!Contains(aliveTouchIds, touchId))
                {
                    _gone.Add(touchId);
                }
            }

            for (int i = 0; i < _gone.Count; i++)
            {
                _attempted.Remove(_gone[i]);
            }
        }

        static bool Contains(IReadOnlyList<int> ids, int id)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                if (ids[i] == id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
