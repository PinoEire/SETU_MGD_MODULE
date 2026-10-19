using NUnit.Framework;

namespace MGD.Samples.Editor
{
    public class SoakTimerTests
    {
        [TestCase(0f, 0.5f)]
        [TestCase(59f, 59.99f)]
        [TestCase(60f, 61f)]
        public void MinuteCrossed_WithinOneMinute_IsFalse(float previous, float now)
        {
            Assert.IsFalse(SoakTimer.MinuteCrossed(previous, now, out _));
        }

        [TestCase(59.9f, 60.1f, 1)]
        [TestCase(299.99f, 300f, 5)]
        [TestCase(599f, 601f, 10)]
        public void MinuteCrossed_IntoANewMinute_ReportsIt(float previous, float now, int expected)
        {
            Assert.IsTrue(SoakTimer.MinuteCrossed(previous, now, out int minute));
            Assert.AreEqual(expected, minute);
        }

        [Test]
        public void MinuteCrossed_AfterALongStall_ReportsTheLatestMinute()
        {
            // A frame that took two minutes (the app was in the background) logs once.
            Assert.IsTrue(SoakTimer.MinuteCrossed(30f, 170f, out int minute));
            Assert.AreEqual(2, minute);
        }

        [TestCase(0f, 0f, 0f)]
        [TestCase(59.99f, 0f, 59f)]
        [TestCase(363.4f, 6f, 3f)]
        [TestCase(600f, 10f, 0f)]
        public void Split_GivesWholeMinutesAndSeconds(float seconds, float minutes, float rest)
        {
            SoakTimer.Split(seconds, out float m, out float s);

            Assert.AreEqual(minutes, m);
            Assert.AreEqual(rest, s);
        }
    }
}
