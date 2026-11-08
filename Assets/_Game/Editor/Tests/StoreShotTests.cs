using NUnit.Framework;

namespace MGD.Samples.Editor
{
    public class StoreShotTests
    {
        [Test]
        public void FileName_NamesTheSceneTheNumberAndThePixelSize()
        {
            Assert.AreEqual("Tiers_03_1080x2400.png", StoreShot.FileName("Tiers", 3, 1080, 2400));
        }

        [Test]
        public void FileName_PadsTheNumberToTwoDigits()
        {
            Assert.AreEqual("About_00_2160x4800.png", StoreShot.FileName("About", 0, 2160, 4800));
        }
    }
}
