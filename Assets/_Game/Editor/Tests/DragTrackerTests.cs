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
        public void Begin_NullTarget_Refused()
        {
            var tracker = new DragTracker();

            Assert.IsFalse(tracker.TryBegin(1, null, Vector2.zero));
            Assert.AreEqual(0, tracker.ActiveCount);
        }

        [Test]
        public void Move_UnknownTouch_ReturnsFalse()
        {
            var tracker = new DragTracker();

            Assert.IsFalse(tracker.TryMove(7, Vector2.one, out _));
        }

        [Test]
        public void TryGetTarget_UnknownTouch_ReturnsFalseAndNull()
        {
            var tracker = new DragTracker();

            Assert.IsFalse(tracker.TryGetTarget(7, out Transform target));
            Assert.IsNull(target);
        }

        [Test]
        public void Forget_ReleasesTarget_SoAnotherFingerCanGrabIt()
        {
            var tracker = new DragTracker();
            Transform target = NewTarget("Red", Vector2.zero);
            tracker.TryBegin(1, target, Vector2.zero);

            tracker.Forget(1);
            tracker.Forget(1); // forgetting twice is harmless

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
        public void Move_BoundsCleared_NoLongerClamps()
        {
            var tracker = new DragTracker { Bounds = new Rect(-2f, -2f, 4f, 4f) };
            Transform target = NewTarget("Red", Vector2.zero);
            tracker.TryBegin(1, target, Vector2.zero);
            tracker.Bounds = null;

            tracker.TryMove(1, new Vector2(10f, -10f), out Vector2 position);

            Assert.AreEqual(new Vector2(10f, -10f), position);
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

        [Test]
        public void Prune_ReportsReleasedTargets()
        {
            var tracker = new DragTracker();
            Transform red = NewTarget("Red", Vector2.zero);
            Transform blue = NewTarget("Blue", Vector2.one);
            tracker.TryBegin(1, red, Vector2.zero);
            tracker.TryBegin(2, blue, Vector2.one);
            var released = new List<Transform>();

            tracker.Prune(new List<int>(), released);

            CollectionAssert.AreEquivalent(new[] { red, blue }, released);
            Assert.AreEqual(0, tracker.ActiveCount);
        }

        [Test]
        public void ShouldAttempt_OncePerTouch_UntilPruned()
        {
            var tracker = new DragTracker();

            Assert.IsTrue(tracker.ShouldAttempt(5));
            Assert.IsFalse(tracker.ShouldAttempt(5)); // a finger that missed does not retry every frame

            tracker.Prune(new List<int>()); // touch 5 is gone
            Assert.IsTrue(tracker.ShouldAttempt(5)); // the id can come back as a new touch
        }

        [Test]
        public void Forget_ClearsTheDragAndTheAttempt_SoAReusedTouchIdStartsFresh()
        {
            // Android may hand a new contact the id of a touch that ended this frame,
            // before Prune has seen a frame without it.
            var tracker = new DragTracker();
            Transform red = NewTarget("Red", Vector2.zero);
            tracker.ShouldAttempt(5);
            tracker.TryBegin(5, red, Vector2.zero);

            tracker.Forget(5);

            Assert.IsFalse(tracker.IsHeld(red));
            Assert.AreEqual(0, tracker.ActiveCount);
            Assert.IsTrue(tracker.ShouldAttempt(5));
        }

        [Test]
        public void ShouldAttempt_AliveTouch_StaysMarkedAcrossPrune()
        {
            var tracker = new DragTracker();
            tracker.ShouldAttempt(5);

            tracker.Prune(new List<int> { 5 });

            Assert.IsFalse(tracker.ShouldAttempt(5));
        }
    }
}
