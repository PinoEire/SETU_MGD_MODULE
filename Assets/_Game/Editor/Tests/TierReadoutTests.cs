using NUnit.Framework;

namespace MGD.Samples.Editor
{
    public class TierReadoutTests
    {
        [Test]
        public void Describe_Detected_NamesTheHardwareAndTheSettings()
        {
            string text = TierReadout.Describe(current: Tier.Mid, detected: Tier.Mid, overridden: false,
                ramMb: 6144, cores: 8, qualityLevel: "Mid", renderScale: 0.9f, msaa: 2, targetFps: 60, mipmapLimit: 0);

            Assert.AreEqual(
                "Tier Mid (detected: 6144 MB RAM, 8 cores)\n" +
                "Quality level Mid\n" +
                "Render scale 0.90  MSAA 2x  60 fps\n" +
                "Textures at full resolution",
                text);
        }

        [Test]
        public void Describe_Overridden_SaysWhatTheHardwarePicked()
        {
            string text = TierReadout.Describe(current: Tier.Low, detected: Tier.High, overridden: true,
                ramMb: 8192, cores: 8, qualityLevel: "Low", renderScale: 0.75f, msaa: 1, targetFps: 30, mipmapLimit: 1);

            StringAssert.StartsWith("Tier Low (override; detected High: 8192 MB RAM, 8 cores)\n", text);
            StringAssert.Contains("MSAA off", text);
            StringAssert.EndsWith("Textures at half resolution", text);
        }

        [Test]
        public void Describe_FormatsTheScaleWhateverTheLocale()
        {
            var previous = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");

                string text = TierReadout.Describe(current: Tier.Low, detected: Tier.Low, overridden: false,
                    ramMb: 3072, cores: 4, qualityLevel: "Low", renderScale: 0.75f, msaa: 1, targetFps: 30, mipmapLimit: 1);

                StringAssert.Contains("Render scale 0.75", text);
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = previous;
            }
        }

        [TestCase(0, Tier.Mid, "Auto: the next launch detects the tier again.")]
        [TestCase(1, Tier.Mid, "Low from the next launch.")]
        [TestCase(2, Tier.Mid, "Mid: already running.")]
        [TestCase(3, Tier.Low, "High from the next launch.")]
        public void OverrideNote_SaysWhenTheChoiceApplies(int option, Tier current, string expected)
        {
            Assert.AreEqual(expected, TierReadout.OverrideNote(option, current));
        }
    }
}
