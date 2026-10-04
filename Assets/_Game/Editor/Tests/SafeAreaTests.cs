using NUnit.Framework;
using UnityEngine;

namespace MGD.Samples.Editor
{
    public class SafeAreaTests
    {
        [Test]
        public void ToAnchors_NotchedPhone_InsetsBecomeNormalisedAnchors()
        {
            // 1080 x 2400 phone, 84 px notch at the top, 60 px gesture bar at the bottom.
            var safe = new Rect(0f, 60f, 1080f, 2400f - 60f - 84f);

            bool ok = SafeArea.ToAnchors(safe, 1080f, 2400f, out Vector2 min, out Vector2 max);

            Assert.IsTrue(ok);
            Assert.AreEqual(0f, min.x, 0.0001f);
            Assert.AreEqual(60f / 2400f, min.y, 0.0001f);
            Assert.AreEqual(1f, max.x, 0.0001f);
            Assert.AreEqual((2400f - 84f) / 2400f, max.y, 0.0001f);
        }

        [Test]
        public void ToAnchors_FullScreen_IsZeroToOne()
        {
            bool ok = SafeArea.ToAnchors(new Rect(0f, 0f, 1080f, 1920f), 1080f, 1920f, out Vector2 min, out Vector2 max);

            Assert.IsTrue(ok);
            Assert.AreEqual(Vector2.zero, min);
            Assert.AreEqual(Vector2.one, max);
        }

        [Test]
        public void ToAnchors_ZeroSizedScreen_RefusesInsteadOfDividingByZero()
        {
            bool ok = SafeArea.ToAnchors(new Rect(0f, 0f, 0f, 0f), 0f, 0f, out _, out _);

            Assert.IsFalse(ok);
        }
    }
}
