using NUnit.Framework;
using UnityEngine.AdaptivePerformance;

namespace MGD.Samples.Editor
{
    public class ThermalOverlayTests
    {
        [Test]
        public void StatusLine_WhenActive_NamesWarningBottleneckAndMode()
        {
            string line = ThermalOverlay.StatusLine(true, WarningLevel.ThrottlingImminent,
                PerformanceBottleneck.GPU, PerfMode.HighFps, 120, 0.8f);

            Assert.AreEqual("ThrottlingImminent  |  bottleneck GPU\nHigh FPS  120 fps  scale 0.80", line);
        }

        [Test]
        public void StatusLine_WhenInactive_SaysSoAndStillShowsTheMode()
        {
            string line = ThermalOverlay.StatusLine(false, WarningLevel.NoWarning,
                PerformanceBottleneck.Unknown, PerfMode.Balanced, 60, 1f);

            Assert.AreEqual("(AP inactive)\nBalanced  60 fps  scale 1.00", line);
        }
    }
}
