using NUnit.Framework;

namespace MGD.Samples.Editor
{
    public class BuildStampTests
    {
        [Test]
        public void ShortId_KeepsTheFirstEightCharacters()
        {
            Assert.AreEqual("a1b2c3d4", BuildStamp.ShortId("a1b2c3d4e5f60718293a4b5c6d7e8f90"));
        }

        [Test]
        public void ShortId_OfAShortGuid_IsTheWholeGuid()
        {
            Assert.AreEqual("a1b2", BuildStamp.ShortId("a1b2"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("00000000000000000000000000000000")]
        public void ShortId_WithNoBuild_SaysEditor(string guid)
        {
            // The editor has no build: Play mode reports an all-zero buildGUID.
            Assert.AreEqual("editor", BuildStamp.ShortId(guid));
        }

        [Test]
        public void ShortId_OfARealGuidStartingWithZeros_KeepsThem()
        {
            Assert.AreEqual("00000000", BuildStamp.ShortId("00000000e5f60718293a4b5c6d7e8f90"));
        }

        [Test]
        public void Format_PutsVersionAndIdFirstThenDeviceOsAndScreen()
        {
            string text = BuildStamp.Format("0.5.0", "a1b2c3d4", "Pixel 7a", "Android OS 17 / API-37",
                8192, 1080, 2400, 60.0);

            Assert.AreEqual(
                "v0.5.0 (a1b2c3d4)\nPixel 7a\nAndroid OS 17 / API-37\n8192 MB RAM  1080x2400 @ 60 Hz",
                text);
        }

        [Test]
        public void Format_RoundsTheRefreshRateWhateverTheLocale()
        {
            var previous = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");

                string text = BuildStamp.Format("1.0", "id", "m", "os", 4096, 720, 1600, 89.9);

                StringAssert.EndsWith("4096 MB RAM  720x1600 @ 90 Hz", text);
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = previous;
            }
        }
    }
}
