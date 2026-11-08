using NUnit.Framework;

namespace MGD.Samples.Editor
{
    public class DeviceTierTests
    {
        [TestCase(8192, 8, Tier.High)]
        [TestCase(12288, 9, Tier.High)]
        [TestCase(8192, 7, Tier.Mid)]
        [TestCase(7800, 8, Tier.Mid)]
        [TestCase(4096, 6, Tier.Mid)]
        [TestCase(4095, 8, Tier.Low)]
        [TestCase(6144, 4, Tier.Low)]
        [TestCase(2048, 4, Tier.Low)]
        public void Detect_UsesTheDeckThresholds(int ramMb, int cores, Tier expected)
        {
            Assert.AreEqual(expected, DeviceTier.Detect(ramMb, cores));
        }

        [Test]
        public void Resolve_WithNoOverride_IsTheDetectedTier()
        {
            Assert.AreEqual(Tier.Mid, DeviceTier.Resolve(Tier.Mid, false, 0));
        }

        [TestCase(0, Tier.Low)]
        [TestCase(1, Tier.Mid)]
        [TestCase(2, Tier.High)]
        public void Resolve_WithAnOverride_UsesIt(int saved, Tier expected)
        {
            Assert.AreEqual(expected, DeviceTier.Resolve(Tier.High, true, saved));
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void Resolve_IgnoresAnOverrideThatIsNotATier(int saved)
        {
            Assert.AreEqual(Tier.Mid, DeviceTier.Resolve(Tier.Mid, true, saved));
        }

        [TestCase(Tier.Low, 30, 0.75f)]
        [TestCase(Tier.Mid, 60, 0.9f)]
        [TestCase(Tier.High, 60, 1f)]
        public void EachTier_HasTheLabTableFrameCapAndScale(Tier tier, int fps, float scale)
        {
            Assert.AreEqual(fps, DeviceTier.FrameRateFor(tier));
            Assert.AreEqual(scale, DeviceTier.RenderScaleFor(tier), 1e-6f);
        }

        [Test]
        public void LevelsMatch_NeedsLowMidHighInOrder()
        {
            Assert.IsTrue(DeviceTier.LevelsMatch(new[] { "Low", "Mid", "High" }));
            Assert.IsFalse(DeviceTier.LevelsMatch(new[] { "Very Low", "Low", "Medium", "High", "Very High", "Ultra" }));
            Assert.IsFalse(DeviceTier.LevelsMatch(new[] { "Low", "High", "Mid" }));
            Assert.IsFalse(DeviceTier.LevelsMatch(new[] { "Low", "Mid" }));
        }

        [TestCase(false, 0, 0)]
        [TestCase(true, 0, 1)]
        [TestCase(true, 2, 3)]
        [TestCase(true, 7, 0)]
        public void OverrideOption_MapsTheSavedKeyToTheDropdown(bool hasOverride, int saved, int option)
        {
            // 0 = Auto, 1..3 = Low..High; a key that is not a tier shows as Auto.
            Assert.AreEqual(option, DeviceTier.OverrideOption(hasOverride, saved));
        }
    }
}
