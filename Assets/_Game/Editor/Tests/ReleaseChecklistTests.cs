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
                KeystoreFileExists = true,
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
        public void Check_KeystoreFileMissing_NamesTheFile()
        {
            ReleaseSettings s = Good();
            s.KeystoreFileExists = false;

            string[] problems = ReleaseChecklist.Check(s);

            Assert.AreEqual(1, problems.Length);
            StringAssert.Contains("Keystore file", problems[0]);
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
        [TestCase("com.pino.game.", TestName = "Check_PackageName_TrailingDot_Rejected")]
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
        [TestCase("com.pino.game_2")]
        public void Check_GoodPackageName_Accepted(string packageName)
        {
            ReleaseSettings s = Good();
            s.PackageName = packageName;

            Assert.IsEmpty(ReleaseChecklist.Check(s));
        }

        [Test]
        public void ResolveKeystorePath_InProjectPrefix_IsRelativeToTheProjectRoot()
        {
            string path = ReleaseSettings.ResolveKeystorePath("{inproject}: Keystore/user.keystore", @"D:\project", @"C:\dedicated");

            Assert.AreEqual(Path.Combine(@"D:\project", "Keystore/user.keystore"), path);
        }

        [Test]
        public void ResolveKeystorePath_DedicatedPrefix_IsRelativeToTheDedicatedFolder()
        {
            string path = ReleaseSettings.ResolveKeystorePath("{dedicated}: user.keystore", @"D:\project", @"C:\dedicated");

            Assert.AreEqual(Path.Combine(@"C:\dedicated", "user.keystore"), path);
        }

        [Test]
        public void ResolveKeystorePath_AbsolutePath_IsUsedAsGiven()
        {
            Assert.AreEqual(@"E:\keys\user.keystore", ReleaseSettings.ResolveKeystorePath(@"E:\keys\user.keystore", @"D:\project", @"C:\dedicated"));
        }

        [Test]
        public void ResolveKeystorePath_RelativePathWithoutPrefix_IsRelativeToTheProjectRoot()
        {
            // What PlayerSettings.Android.keystoreName returns for an in-project keystore in Unity 6.
            Assert.AreEqual(Path.Combine(@"D:\project", "Keystore/user.keystore"), ReleaseSettings.ResolveKeystorePath("Keystore/user.keystore", @"D:\project", @"C:\dedicated"));
        }

        [Test]
        public void ResolveKeystorePath_Empty_StaysEmpty()
        {
            Assert.AreEqual("", ReleaseSettings.ResolveKeystorePath("", @"D:\project", @"C:\dedicated"));
        }

        [Test]
        public void ApkFileName_CarriesVersionAndCode_SoBuildsNeverOverwriteEachOther()
        {
            Assert.AreEqual("SETU_MGD_MODULE-0.2.0-9-arm64.apk", ReleaseBuild.ApkFileName("SETU_MGD_MODULE", "0.2.0", 9));
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
        public void ManifestAppend_ExistingEmptyFile_GetsTheHeader()
        {
            string path = Path.Combine(Path.GetTempPath(), "mgd-manifest-" + Path.GetRandomFileName() + ".md");
            try
            {
                File.WriteAllText(path, "");

                ReleaseManifest.Append(path, "| row |");

                string[] lines = File.ReadAllLines(path);
                StringAssert.StartsWith("# Release manifest", lines[0]);
                Assert.AreEqual("| row |", lines[lines.Length - 1]);
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
