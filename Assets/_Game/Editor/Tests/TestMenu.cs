using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Menu item <c>MGD Samples > Run EditMode Tests</c>: runs every EditMode test
    /// without opening the Test Runner window and prints one summary line to the
    /// console, <c>[Tests] N passed, N failed, N inconclusive, N skipped in N s</c>,
    /// as an error when the run failed, plus one error line per failure with its
    /// message. It refuses to start during Play mode, while scripts compile, with
    /// compile errors (the run would test stale code) or while a run is going on.
    /// The <c>[Tests]</c> prefix is shared by every line so the console
    /// search box finds the whole report at once.
    /// </summary>
    public static class TestMenu
    {
        // True from Execute until RunFinished. Test callbacks are global, so a second
        // run started on top of the first would have both reports hear both runs.
        // A domain reload resets it, which is right: the reload ends the run too.
        static bool s_Running;

        [MenuItem("MGD Samples/Run EditMode Tests")]
        public static void RunEditModeTests()
        {
            if (s_Running)
            {
                Debug.LogWarning("[Tests] A run is already in progress; wait for its summary line.");
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Tests] Leave Play mode first.");
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                Debug.LogWarning("[Tests] Scripts or assets are still updating; run the tests when that finishes.");
                return;
            }

            // With compile errors the editor still holds the last good assemblies, so a
            // run would test old code and could report a pass for code that does not build.
            if (EditorUtility.scriptCompilationFailed)
            {
                Debug.LogError("[Tests] Scripts have compile errors; fix them first. Not running stale tests.");
                return;
            }

            s_Running = true;
            TestRunnerApi api = null;
            try
            {
                api = ScriptableObject.CreateInstance<TestRunnerApi>();
                api.hideFlags = HideFlags.HideAndDontSave;
                api.RegisterCallbacks(new Reporter(api));
                api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode }));
            }
            catch
            {
                // The run never started, so RunFinished will not clear the flag.
                s_Running = false;
                if (api != null)
                {
                    Object.DestroyImmediate(api);
                }

                throw;
            }
        }

        sealed class Reporter : ICallbacks
        {
            readonly TestRunnerApi _api;

            // Every failure reported below, tests and fixtures alike, so the summary
            // agrees with the FAILED lines above it.
            int _failures;

            public Reporter(TestRunnerApi api)
            {
                _api = api;
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
                Debug.Log($"[Tests] Running {testsToRun.TestCaseCount} EditMode tests.");
            }

            // Required by ICallbacks; nothing to report when a test starts.
            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.TestStatus != TestStatus.Failed)
                {
                    return;
                }

                // A test that failed is reported once. A fixture or namespace marked
                // failed only because a child failed ("Failed(Child)") would repeat
                // it, but a fixture that failed in its own right (a OneTimeTearDown
                // that throws after every test passed) has no failed child to report
                // it and must be shown here.
                if (!result.HasChildren || !result.ResultState.Contains("Child"))
                {
                    _failures++;
                    Debug.LogError($"[Tests] FAILED {result.FullName}: {result.Message}");
                }
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                // First, so nothing below can leave the menu refusing every run.
                s_Running = false;
                _api.UnregisterCallbacks(this);
                Object.DestroyImmediate(_api);

                string summary = $"[Tests] {result.PassCount} passed, {_failures} failed, " +
                                 $"{result.InconclusiveCount} inconclusive, {result.SkipCount} skipped " +
                                 $"in {result.Duration:0.0} s";
                // The run's own status, not the test count: a fixture-level failure
                // leaves every test passed but still fails the run.
                if (result.TestStatus == TestStatus.Failed || _failures > 0)
                {
                    Debug.LogError(summary);
                }
                else
                {
                    Debug.Log(summary);
                }
            }
        }
    }
}
