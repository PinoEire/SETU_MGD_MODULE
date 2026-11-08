using System;
using System.Globalization;
using NUnit.Framework;
using UnityEngine;

namespace MGD.Samples.Editor
{
    public class BuildInfoTests
    {
        [Test]
        public void Format_DateAndHash()
        {
            Assert.AreEqual("Built 2026-11-30 from 3669742", BuildInfoLabel.Format("2026-11-30", "3669742"));
        }

        [Test]
        public void Format_NoHash_ShowsTheDateOnly()
        {
            Assert.AreEqual("Built 2026-11-30", BuildInfoLabel.Format("2026-11-30", ""));
        }

        [Test]
        public void Format_NeverStamped_SaysSo()
        {
            StringAssert.StartsWith("Not built yet", BuildInfoLabel.Format("", ""));
        }

        [Test]
        public void Stamp_WritesAnIsoDateWhateverTheLocale()
        {
            CultureInfo before = CultureInfo.CurrentCulture;
            var info = ScriptableObject.CreateInstance<BuildInfo>();
            try
            {
                // The Thai culture uses the Buddhist calendar: 2026 would print as 2569.
                CultureInfo.CurrentCulture = new CultureInfo("th-TH");
                Assume.That(new DateTime(2026, 1, 1).ToString("yyyy", CultureInfo.CurrentCulture), Is.EqualTo("2569"),
                    "this runtime's Thai culture does not use the Buddhist calendar, so the test proves nothing here");
                BuildInfoStamper.Stamp(info, new DateTime(2026, 11, 30, 23, 0, 0, DateTimeKind.Utc), "3669742");
                Assert.AreEqual("2026-11-30", info.buildDate);
                Assert.AreEqual("3669742", info.gitHash);
            }
            finally
            {
                CultureInfo.CurrentCulture = before;
                UnityEngine.Object.DestroyImmediate(info);
            }
        }
    }
}
