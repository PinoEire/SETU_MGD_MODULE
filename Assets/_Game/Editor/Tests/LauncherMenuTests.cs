using NUnit.Framework;

namespace MGD.Samples.Editor
{
    public class LauncherMenuTests
    {
        [TestCase(0f, 10, 10)]      // Screen.dpi unknown: keep what the EventSystem has
        [TestCase(96f, 10, 10)]     // a desktop monitor: 6 px would be smaller, keep 10
        [TestCase(420f, 10, 26)]    // a typical phone: 10 px at 160 dpi, scaled
        [TestCase(420f, 30, 30)]    // a larger value set elsewhere is kept
        [TestCase(5000f, 10, 60)]   // a bogus density is clamped
        public void DragThresholdFor_ScalesWithDensityWithinLimits(float dpi, int current, int expected)
        {
            Assert.AreEqual(expected, LauncherMenu.DragThresholdFor(dpi, current));
        }
    }
}
