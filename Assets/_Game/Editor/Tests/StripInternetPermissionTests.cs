using NUnit.Framework;

namespace MGD.Samples.Editor
{
    public class StripInternetPermissionTests
    {
        const string UnityLibraryManifest =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
            "<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" package=\"com.unity3d.player\">\n" +
            "  <uses-permission android:name=\"android.permission.INTERNET\" />\n" +
            "  <uses-permission android:name=\"android.permission.VIBRATE\" />\n" +
            "  <application />\n" +
            "</manifest>\n";

        [Test]
        public void Strip_RemovesOnlyTheInternetLine()
        {
            string result = StripInternetPermission.Strip(UnityLibraryManifest);

            StringAssert.DoesNotContain("android.permission.INTERNET", result);
            StringAssert.Contains("android.permission.VIBRATE", result);
            StringAssert.Contains("<application />", result);
        }

        [Test]
        public void Strip_LeavesLineCountTidy()
        {
            string result = StripInternetPermission.Strip(UnityLibraryManifest);

            Assert.AreEqual(UnityLibraryManifest.Split('\n').Length - 1, result.Split('\n').Length);
        }

        [Test]
        public void Strip_ManifestWithoutInternet_Unchanged()
        {
            string manifest = UnityLibraryManifest.Replace("  <uses-permission android:name=\"android.permission.INTERNET\" />\n", "");

            Assert.AreEqual(manifest, StripInternetPermission.Strip(manifest));
        }

        [Test]
        public void Strip_ToleratesSpacingAndNoSpaceBeforeTheSlash()
        {
            string manifest = "<manifest>\n<uses-permission   android:name=\"android.permission.INTERNET\"/>\n</manifest>\n";

            Assert.AreEqual("<manifest>\n</manifest>\n", StripInternetPermission.Strip(manifest));
        }

        [Test]
        public void Strip_ToleratesExtraAttributesInAnyOrder()
        {
            string manifest =
                "<manifest>\n" +
                "  <uses-permission tools:node=\"merge\" android:name=\"android.permission.INTERNET\" android:maxSdkVersion=\"30\" />\n" +
                "</manifest>\n";

            Assert.AreEqual("<manifest>\n</manifest>\n", StripInternetPermission.Strip(manifest));
        }

        [Test]
        public void Strip_ToleratesExplicitClosingTag()
        {
            string manifest = "<manifest>\n  <uses-permission android:name=\"android.permission.INTERNET\"></uses-permission>\n</manifest>\n";

            Assert.AreEqual("<manifest>\n</manifest>\n", StripInternetPermission.Strip(manifest));
        }

        [Test]
        public void Strip_ToleratesWindowsLineEndings()
        {
            string manifest = UnityLibraryManifest.Replace("\n", "\r\n");

            string result = StripInternetPermission.Strip(manifest);

            StringAssert.DoesNotContain("android.permission.INTERNET", result);
            Assert.AreEqual(manifest.Split('\n').Length - 1, result.Split('\n').Length);
        }

        [Test]
        public void RequestsInternet_DetectsThePermissionWhateverTheShape()
        {
            Assert.IsTrue(StripInternetPermission.RequestsInternet(UnityLibraryManifest));
            Assert.IsTrue(StripInternetPermission.RequestsInternet("<uses-permission\n  android:name='android.permission.INTERNET'/>"));
            Assert.IsFalse(StripInternetPermission.RequestsInternet(UnityLibraryManifest.Replace("INTERNET", "VIBRATE")));
        }
    }
}
