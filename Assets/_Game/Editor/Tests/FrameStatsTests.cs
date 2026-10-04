using System;
using NUnit.Framework;

namespace MGD.Samples.Editor
{
    public class FrameStatsTests
    {
        static float[] OneTo(int count)
        {
            var values = new float[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = i + 1;
            }

            return values;
        }

        [Test]
        public void Compute_OneToHundred_AverageIsFiftyPointFiveAndP99IsNinetyNine()
        {
            float[] samples = OneTo(100);
            var scratch = new float[100];

            FrameStats.Compute(samples, scratch, out float average, out float p99);

            Assert.AreEqual(50.5f, average, 0.001f);
            Assert.AreEqual(99f, p99, 0.001f);
        }

        [Test]
        public void Compute_LeavesSamplesUnsorted()
        {
            float[] samples = { 5f, 1f, 4f, 2f, 3f };
            var scratch = new float[5];

            FrameStats.Compute(samples, scratch, out _, out float p99);

            CollectionAssert.AreEqual(new[] { 5f, 1f, 4f, 2f, 3f }, samples);
            Assert.AreEqual(4f, p99, 0.001f); // floor(0.99 * 4) = 3 -> sorted[3] = 4
        }

        [Test]
        public void Compute_AllEqual_AverageEqualsP99()
        {
            var samples = new float[600];
            Array.Fill(samples, 16.7f);
            var scratch = new float[600];

            FrameStats.Compute(samples, scratch, out float average, out float p99);

            Assert.AreEqual(16.7f, average, 0.001f);
            Assert.AreEqual(16.7f, p99, 0.001f);
        }

        [Test]
        public void Compute_FiveBadFramesInSixHundred_DoNotMoveP99()
        {
            // The lesson of p99: a stutter has to happen in more than 1 frame in
            // 100 before this number sees it. Five spikes in 600 sit above index 593.
            var samples = new float[600];
            Array.Fill(samples, 16.7f);
            for (int i = 0; i < 5; i++)
            {
                samples[i * 100] = 100f;
            }

            FrameStats.Compute(samples, new float[600], out _, out float p99);

            Assert.AreEqual(16.7f, p99, 0.001f);
        }

        [Test]
        public void Compute_SevenBadFramesInSixHundred_MoveP99()
        {
            var samples = new float[600];
            Array.Fill(samples, 16.7f);
            for (int i = 0; i < 7; i++)
            {
                samples[i * 80] = 100f;
            }

            FrameStats.Compute(samples, new float[600], out _, out float p99);

            Assert.AreEqual(100f, p99, 0.001f);
        }

        [Test]
        public void PercentileIndex_SixHundredSamples_Is593()
        {
            Assert.AreEqual(593, FrameStats.PercentileIndex(600, 0.99f));
        }

        [TestCase(1, 0)]
        [TestCase(2, 0)]
        [TestCase(100, 98)]
        public void PercentileIndex_SmallBuffers_StayInRange(int count, int expected)
        {
            Assert.AreEqual(expected, FrameStats.PercentileIndex(count, 0.99f));
        }

        [Test]
        public void PercentileIndex_PercentileOutsideZeroToOne_IsClamped()
        {
            Assert.AreEqual(0, FrameStats.PercentileIndex(600, -1f));
            Assert.AreEqual(599, FrameStats.PercentileIndex(600, 2f));
        }

        [Test]
        public void Compute_EmptySamples_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                FrameStats.Compute(new float[0], new float[0], out _, out _));
        }

        [Test]
        public void Compute_ScratchShorterThanSamples_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                FrameStats.Compute(new float[10], new float[5], out _, out _));
        }
    }
}
