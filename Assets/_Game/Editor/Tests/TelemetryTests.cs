using NUnit.Framework;

namespace MGD.Samples.Editor
{
    public class TelemetryTests
    {
        [Test]
        public void Format_WritesTimeSessionEventThenPairsInOrder()
        {
            string line = Telemetry.Format(1.234f, "abcd1234", "level_start", ("level_id", 1), ("attempt", 2));

            Assert.AreEqual("1.23 abcd1234 level_start level_id=1 attempt=2", line);
        }

        [Test]
        public void Format_WithNoParameters_IsJustTheHeader()
        {
            Assert.AreEqual("0.00 abcd1234 session_start", Telemetry.Format(0f, "abcd1234", "session_start"));
        }

        [Test]
        public void Format_UsesADotForDecimalsWhateverTheLocale()
        {
            var previous = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");

                string line = Telemetry.Format(12.5f, "abcd1234", "level_fail", ("time_s", 12.5f));

                Assert.AreEqual("12.50 abcd1234 level_fail time_s=12.5", line);
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = previous;
            }
        }

        [Test]
        public void Format_ReplacesSpacesInValues_SoEachPairStaysOneToken()
        {
            string line = Telemetry.Format(0f, "abcd1234", "session_start", ("device_model", "Pixel 7a"));

            Assert.AreEqual("0.00 abcd1234 session_start device_model=Pixel_7a", line);
        }

        [Test]
        public void Format_ReplacesAnyWhitespaceInValues()
        {
            string line = Telemetry.Format(0f, "abcd1234", "level_fail", ("cause", "line\nbreak\there"));

            Assert.AreEqual("0.00 abcd1234 level_fail cause=line_break_here", line);
        }

        [Test]
        public void Format_WritesANullValueAsEmpty()
        {
            Assert.AreEqual("0.00 abcd1234 ev k=", Telemetry.Format(0f, "abcd1234", "ev", ("k", (object)null)));
        }

        [Test]
        public void Session_IsEightCharactersAndStableWithinARun()
        {
            string first = Telemetry.Session;

            Assert.AreEqual(8, first.Length);
            Assert.AreEqual(first, Telemetry.Session);
        }
    }
}
