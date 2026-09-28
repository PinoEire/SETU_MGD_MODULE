using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace MGD.Samples.Editor
{
    public class DragTrackerTests
    {
        readonly List<GameObject> _objects = new List<GameObject>();

        Transform NewTarget(string name, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            _objects.Add(go);
            return go.transform;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in _objects)
            {
                Object.DestroyImmediate(go);
            }

            _objects.Clear();
        }

        [Test]
        public void Move_KeepsTheGrabbedPointUnderTheFinger()
        {
            var tracker = new DragTracker();
            Transform target = NewTarget("Red", new Vector2(1f, 1f));
            Assert.IsTrue(tracker.TryBegin(1, target, new Vector2(1.25f, 1.5f))); // grabbed off centre

            Assert.IsTrue(tracker.TryMove(1, new Vector2(3.25f, 0.5f), out Vector2 position));

            Assert.AreEqual(new Vector2(3f, 0f), position); // centre follows, offset preserved
        }

        [Test]
        public void Begin_SecondFingerOnHeldTarget_Refused()
        {
            var tracker = new DragTracker();
            Transform target = NewTarget("Red", Vector2.zero);
            tracker.TryBegin(1, target, Vector2.zero);

            Assert.IsFalse(tracker.TryBegin(2, target, Vector2.zero));
            Assert.AreEqual(1, tracker.ActiveCount);
        }

        [Test]
        public void Begin_SameTouchIdTwice_Refused()
        {
            var tracker = new DragTracker();
            Transform red = NewTarget("Red", Vector2.zero);
            Transform blue = NewTarget("Blue", Vector2.one);
            tracker.TryBegin(1, red, Vector2.zero);

            Assert.IsFalse(tracker.TryBegin(1, blue, Vector2.one));
            Assert.IsTrue(tracker.IsHeld(red));
            Assert.IsFalse(tracker.IsHeld(blue));
        }

        [Test]
        public void Move_UnknownTouch_ReturnsFalse()
        {
            var tracker = new DragTracker();

            Assert.IsFalse(tracker.TryMove(7, Vector2.one, out _));
        }

        [Test]
        public void End_ReleasesTarget_SoAnotherFingerCanGrabIt()
        {
            var tracker = new DragTracker();
            Transform target = NewTarget("Red", Vector2.zero);
            tracker.TryBegin(1, target, Vector2.zero);

            Assert.IsTrue(tracker.End(1));
            Assert.IsFalse(tracker.End(1)); // already gone
            Assert.IsFalse(tracker.IsHeld(target));
            Assert.IsTrue(tracker.TryBegin(2, target, Vector2.zero));
        }

        [Test]
        public void Move_WithBounds_ClampsTheCentre()
        {
            var tracker = new DragTracker { Bounds = new Rect(-2f, -2f, 4f, 4f) };
            Transform target = NewTarget("Red", Vector2.zero);
            tracker.TryBegin(1, target, Vector2.zero);

            tracker.TryMove(1, new Vector2(10f, -10f), out Vector2 position);

            Assert.AreEqual(new Vector2(2f, -2f), position);
        }

        [Test]
        public void Prune_DropsDragsWhoseTouchIsGone()
        {
            var tracker = new DragTracker();
            Transform red = NewTarget("Red", Vector2.zero);
            Transform blue = NewTarget("Blue", Vector2.one);
            tracker.TryBegin(1, red, Vector2.zero);
            tracker.TryBegin(2, blue, Vector2.one);

            var alive = new List<int> { 2 };
            tracker.Prune(alive);

            Assert.IsFalse(tracker.IsHeld(red));
            Assert.IsTrue(tracker.IsHeld(blue));
            Assert.AreEqual(1, tracker.ActiveCount);
        }
    }
}
