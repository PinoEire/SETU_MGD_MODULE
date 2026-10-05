using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// A local telemetry stub: each event becomes one line,
    /// <c>&lt;time&gt; &lt;session&gt; &lt;event&gt; key=value ...</c>, written to the
    /// console (logcat on the phone, filter with <c>grep telemetry</c>) and appended
    /// to <c>telemetry.log</c> in <c>Application.persistentDataPath</c>, which never
    /// leaves the device. Event names are snake_case; parameters are numbers and
    /// ids. Call it per event, never per frame: building the line allocates. Call
    /// it from the main thread: it reads <c>Time</c> and <c>Application</c>. In the
    /// editor the time counts from when the editor started, not from Play; on a
    /// phone it counts from app launch.
    /// </summary>
    public static class Telemetry
    {
        /// <summary>The log file's name in <c>Application.persistentDataPath</c>.</summary>
        public const string FileName = "telemetry.log";

        static string s_session;
        static string s_path;
        static bool s_fileFailed;

        // With domain reloading off in the editor, statics survive between Play
        // sessions; each Play is a new session with a new id.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            s_session = null;
            s_path = null;
            s_fileFailed = false;
        }

        /// <summary>Eight characters that tag every line of this run, so a pulled log splits into sessions.</summary>
        public static string Session => s_session ??= Guid.NewGuid().ToString("N").Substring(0, 8);

        /// <summary>Writes one event, for example <c>Telemetry.Log("level_start", ("level_id", 1))</c>.</summary>
        public static void Log(string ev, params (string k, object v)[] p)
        {
            string line = Format(Time.realtimeSinceStartup, Session, ev, p);
            Debug.Log("[telemetry] " + line);

            if (s_fileFailed)
            {
                return;
            }

            try
            {
                s_path ??= Path.Combine(Application.persistentDataPath, FileName);
                File.AppendAllText(s_path, line + "\n");
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                // Telemetry must never break the game: warn once, then log to the console only.
                s_fileFailed = true;
                Debug.LogWarning($"[telemetry] Cannot write {FileName}, console only from now on: {e.Message}");
            }
        }

        /// <summary>
        /// The line for one event. Numbers use the invariant culture (a dot, whatever
        /// the phone's language), and any whitespace in values (spaces, tabs, line
        /// breaks) becomes an underscore so every event stays one line and every
        /// <c>key=value</c> pair one token. Pure, so it is tested.
        /// </summary>
        public static string Format(float time, string session, string ev, params (string k, object v)[] p)
        {
            var line = new StringBuilder(128);
            line.Append(time.ToString("F2", CultureInfo.InvariantCulture))
                .Append(' ').Append(session)
                .Append(' ').Append(ev);
            foreach ((string k, object v) in p)
            {
                string value = Convert.ToString(v, CultureInfo.InvariantCulture) ?? "";
                line.Append(' ').Append(k).Append('=');
                foreach (char c in value)
                {
                    line.Append(char.IsWhiteSpace(c) ? '_' : c);
                }
            }

            return line.ToString();
        }
    }
}
