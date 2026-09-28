using System;

namespace MGD.Samples
{
    /// <summary>
    /// Average and 99th-percentile of a buffer of frame times. Kept apart from
    /// the MonoBehaviour so the maths can be unit tested and the sampler's
    /// Update stays a few lines. Sorts a copy in <c>scratch</c>, never the
    /// samples themselves, and allocates nothing.
    /// </summary>
    public static class FrameStats
    {
        public static void Compute(float[] samples, float[] scratch, out float average, out float p99)
        {
            if (samples == null)
            {
                throw new ArgumentNullException(nameof(samples));
            }

            if (samples.Length == 0)
            {
                throw new ArgumentException("samples must not be empty", nameof(samples));
            }

            if (scratch == null || scratch.Length < samples.Length)
            {
                throw new ArgumentException("scratch must be at least as long as samples", nameof(scratch));
            }

            int count = samples.Length;
            Array.Copy(samples, scratch, count);
            Array.Sort(scratch, 0, count);

            float sum = 0f;
            for (int i = 0; i < count; i++)
            {
                sum += scratch[i];
            }

            average = sum / count;
            p99 = scratch[PercentileIndex(count, 0.99f)];
        }

        /// <summary>
        /// Index of the given percentile in a sorted buffer of <paramref name="count"/>
        /// values, nearest rank rounded down: 593 for 600 samples at 0.99.
        /// </summary>
        public static int PercentileIndex(int count, float percentile)
        {
            return (int)Math.Floor(percentile * (count - 1));
        }
    }
}
