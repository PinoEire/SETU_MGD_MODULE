using System.IO;
using NUnit.Framework;

namespace MGD.Samples.Editor
{
    public class ReleaseChecklistTests
    {
        static ReleaseSettings Good()
        {
            return new ReleaseSettings
            {
                Il2Cpp = true,
                Arm64Only = true,
                TargetApiAuto = true,
                BuildAppBundle = false,
                UseCustomKeystore = true,
                KeystorePasswordPresent = true,
                KeyPasswordPresent = true,
                PackageName = "com.dftgames.mgdsamples"
            };
        }

        [Test]
        public void Check_AllSettingsCorrect_NoProblems()
        {
            Assert.IsEmpty(ReleaseChecklist.Check(Good()));
        }

        [Test]
        public void Check_MonoBackend_NamesScriptingBackend()
        {
            ReleaseSettings s = Good();
            s.Il2Cpp = false;

            string[] problems = ReleaseChecklist.Check(s);

            Assert.AreEqual(1, problems.Length);
            StringAssert.Contains("IL2CPP", problems[0]);
        }

        [Test]
        public void Check_Armv7Ticked_NamesArchitecture()
        {
            ReleaseSettings s = Good();
            s.Arm64Only = false;

            string[] problems = ReleaseChecklist.Check(s);

            Assert.AreEqual(1, problems.Length);
            StringAssert.Contains("ARM64", problems[0]);
        }

        [Test]
        public void Check_FixedTargetApi_NamesTargetApiLevel()
        {
            ReleaseSettings s = Good();
            s.TargetApiAuto = false;

            string[] problems = ReleaseChecklist.Check(s);

            Assert.AreEqual(1, problems.Length);
            StringAssert.Contains("Target API Level", problems[0]);
        }

        [Test]
        public void Check_MissingPasswords_OneProblemEach()
        {
            ReleaseSettings s = Good();
            s.KeystorePasswordPresent = false;
            s.KeyPasswordPresent = false;

            string[] problems = ReleaseChecklist.Check(s);

            Assert.AreEqual(2, problems.Length);
            StringAssert.Contains("Keystore password", problems[0]);
            StringAssert.Contains("Key password", problems[1]);
        }

        [Test]
        public void Check_NoCustomKeystore_NamesKeystore()
        {
            ReleaseSettings s = Good();
            s.UseCustomKeystore = false;

            string[] problems = ReleaseChecklist.Check(s);

            Assert.AreEqual(1, problems.Length);
            StringAssert.Contains("Custom Keystore", problems[0]);
        }

        [Test]
        public void Check_AppBundleOn_NamesAppBundle()
        {
            ReleaseSettings s = Good();
            s.BuildAppBundle = true;

            string[] problems = ReleaseChecklist.Check(s);

            Assert.AreEqual(1, problems.Length);
            StringAssert.Contains("App Bundle", problems[0]);
        }

        [TestCase("com.DefaultCompany.2D-URP", TestName = "Check_PackageName_UnityDefaultWithUpperCaseAndHyphen_Rejected")]
        [TestCase("game", TestName = "Check_PackageName_SingleSegment_Rejected")]
        [TestCase("com.pino.my-game", TestName = "Check_PackageName_Hyphen_Rejected")]
        [TestCase("com.1abc.game", TestName = "Check_PackageName_DigitLeadingSegment_Rejected")]
        [TestCase("Com.pino.game", TestName = "Check_PackageName_UpperCase_Rejected")]
        [TestCase("", TestName = "Check_PackageName_Empty_Rejected")]
        public void Check_BadPackageName_Rejected(string packageName)
        {
            ReleaseSettings s = Good();
            s.PackageName = packageName;

            string[] problems = ReleaseChecklist.Check(s);

            Assert.AreEqual(1, problems.Length);
            StringAssert.Contains("Package Name", problems[0]);
        }

        [TestCase("com.pino.runner")]
        [TestCase("ie.setu.mgd_samples2")]
        public void Check_GoodPackageName_Accepted(string packageName)
        {
            ReleaseSettings s = Good();
            s.PackageName = packageName;

            Assert.IsEmpty(ReleaseChecklist.Check(s));
        }

        [Test]
        public void ManifestRow_FormatsFixedColumns()
        {
            string row = ReleaseManifest.Row("0.2.0", 2, "2026-09-28", "a121f41", 41_943_040L,
                "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef");

            Assert.AreEqual(
                "| 0.2.0 | 2 | 2026-09-28 | a121f41 | 40.0 MB | 0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef | |",
                row);
        }

        [Test]
        public void ManifestAppend_FreshFile_WritesHeaderOnceAndRowsOnOwnLines()
        {
            string path = Path.Combine(Path.GetTempPath(), "mgd-manifest-" + Path.GetRandomFileName() + ".md");
            try
            {
                ReleaseManifest.Append(path, "| row one |");
                ReleaseManifest.Append(path, "| row two |");

                string[] lines = File.ReadAllLines(path);
                Assert.AreEqual(1, System.Array.FindAll(lines, l => l.StartsWith("# Release manifest")).Length);
                Assert.AreEqual("| row one |", lines[lines.Length - 2]);
                Assert.AreEqual("| row two |", lines[lines.Length - 1]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void ManifestAppend_FileWithoutTrailingNewline_StartsRowOnNewLine()
        {
            string path = Path.Combine(Path.GetTempPath(), "mgd-manifest-" + Path.GetRandomFileName() + ".md");
            try
            {
                File.WriteAllText(path, "| edited by hand |");

                ReleaseManifest.Append(path, "| appended |");

                string[] lines = File.ReadAllLines(path);
                Assert.AreEqual("| edited by hand |", lines[0]);
                Assert.AreEqual("| appended |", lines[1]);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
