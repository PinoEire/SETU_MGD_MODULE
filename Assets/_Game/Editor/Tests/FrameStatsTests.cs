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
        public void PercentileIndex_SixHundredSamples_Is593()
        {
            Assert.AreEqual(593, FrameStats.PercentileIndex(600, 0.99f));
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
