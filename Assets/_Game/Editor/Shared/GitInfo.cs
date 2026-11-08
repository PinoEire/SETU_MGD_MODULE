using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Runs one git command in the project's root folder and returns what it
    /// printed. Editor-only: a phone has no git, so anything a build needs from
    /// git is written into an asset before the build (see <see cref="BuildInfoStamper"/>).
    /// </summary>
    public static class GitInfo
    {
        const int TimeoutMilliseconds = 5000;

        /// <summary>
        /// The command's trimmed output, or "unknown" when git is not installed,
        /// the command fails (for example outside a repository), it printed
        /// nothing, or git or its output took longer than five seconds each.
        /// </summary>
        public static string Run(string arguments)
        {
            try
            {
                var git = new ProcessStartInfo("git", arguments)
                {
                    WorkingDirectory = Path.GetDirectoryName(UnityEngine.Application.dataPath),
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using Process process = Process.Start(git);

                // Read in the background: a plain ReadToEnd would wait for git to
                // finish, however long it took, and the timeout below would never apply.
                Task<string> output = process.StandardOutput.ReadToEndAsync();
                if (!process.WaitForExit(TimeoutMilliseconds))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (InvalidOperationException)
                    {
                        // It exited between the timeout and the kill: nothing to stop.
                    }
                    // The background read is dropped: it ends when the pipe closes or the process is disposed.
                    return "unknown"; // Never hold up a build waiting for git.
                }

                // A helper git started can keep the pipe open after git exits, so the
                // read gets the same limit.
                if (!output.Wait(TimeoutMilliseconds))
                {
                    return "unknown";
                }

                string text = output.Result.Trim();
                return process.ExitCode == 0 && text.Length > 0 ? text : "unknown";
            }
            catch (Exception)
            {
                return "unknown";
            }
        }
    }
}
