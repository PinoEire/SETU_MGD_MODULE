using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace MGD.Samples
{
    /// <summary>
    /// Produces, on purpose, the three failures Week 8 Lab B teaches students to
    /// recognise in logcat, so each signature can be seen once before it turns up
    /// for real:
    /// <list type="bullet">
    /// <item>a managed exception: a <c>Unity</c> line with
    /// <c>InvalidOperationException</c> and its stack (search for the exception
    /// name: logcat pads the tag); the app carries on;</item>
    /// <item>an ANR: Android's UI thread is blocked for 10 s; tap an empty part
    /// of the screen straight away (within 5 s) and, 5 s after that tap,
    /// <c>ActivityManager: ANR in ...</c> (<c>Input dispatching timed out</c>)
    /// and the "isn't responding" dialog;</item>
    /// <item>a native crash: <c>libc : Fatal signal 11 (SIGSEGV)</c>, then the
    /// tombstone under <c>DEBUG</c>; the app is gone.</item>
    /// </list>
    /// Unity runs the game on its own thread, not on Android's UI thread, which
    /// is why the ANR button blocks the UI thread through
    /// <c>AndroidApplication.InvokeOnUIThread</c>: sleeping in a button handler
    /// would only freeze the game. Each button needs a second tap within 3 s
    /// (<see cref="ArmedTrigger"/>), because the app really freezes or dies. In the
    /// editor the ANR and the crash only log, so they cannot take the editor down.
    /// It also counts <c>Application.lowMemory</c>, which Unity raises for
    /// <c>onLowMemory</c> and <c>TRIM_MEMORY_RUNNING_CRITICAL</c>; Android 14 and
    /// later no longer send apps the running levels, so on most current phones
    /// Lab B's <c>send-trim-memory ... RUNNING_CRITICAL</c> leaves the count at 0.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class QaTriggers : MonoBehaviour
    {
        const float ArmWindow = 3f;
        const int FreezeMilliseconds = 10000;
        const string ArmedText = "Tap again to confirm";
        const string AnrIgnoredText = "Ignored: the last freeze has only just ended. Wait a few seconds.";

        [SerializeField] TMP_Text exceptionLabel;
        [SerializeField] TMP_Text anrLabel;
        [SerializeField] TMP_Text crashLabel;
        [SerializeField] TMP_Text status;

        readonly ArmedTrigger _exception = new ArmedTrigger(ArmWindow);
        readonly ArmedTrigger _anr = new ArmedTrigger(ArmWindow);
        readonly ArmedTrigger _crash = new ArmedTrigger(ArmWindow);

        // Labels are swapped only when the armed state changes; the strings are
        // made once here, so Update allocates nothing.
        string _exceptionIdle;
        string _anrIdle;
        string _crashIdle;
        bool _exceptionShownArmed;
        bool _anrShownArmed;
        bool _crashShownArmed;

        int _lowMemoryCount;

        // Taps made during the freeze are delivered when the UI thread wakes; two
        // of them on the ANR button would start a second freeze straight away. The
        // window is the freeze plus one arm window, for a tap that arms the button
        // just as the thread wakes. Only a phone sets it: the editor does not freeze.
        float _anrIgnoredUntil = float.NegativeInfinity;

        void Awake()
        {
            Application.lowMemory += OnLowMemory;
        }

        void OnDestroy()
        {
            Application.lowMemory -= OnLowMemory;
        }

        // Start, not Awake: the TextMeshPro labels may not have initialised yet.
        void Start()
        {
            _exceptionIdle = exceptionLabel.text;
            _anrIdle = anrLabel.text;
            _crashIdle = crashLabel.text;
            ShowStatus("Ready. Each button needs two taps.");
        }

        void Update()
        {
            // Unscaled: the pause sets timeScale to 0, and the window must still run out.
            float now = Time.unscaledTime;
            Refresh(exceptionLabel, _exceptionIdle, _exception.IsArmed(now), ref _exceptionShownArmed);
            Refresh(anrLabel, _anrIdle, _anr.IsArmed(now), ref _anrShownArmed);
            Refresh(crashLabel, _crashIdle, _crash.IsArmed(now), ref _crashShownArmed);
        }

        static void Refresh(TMP_Text label, string idle, bool armed, ref bool shownArmed)
        {
            if (armed == shownArmed)
            {
                return;
            }

            shownArmed = armed;
            label.text = armed ? ArmedText : idle;
        }

        /// <summary>Wired to the Exception button's OnClick.</summary>
        public void OnExceptionPressed()
        {
            if (!_exception.Press(Time.unscaledTime))
            {
                return;
            }

            ShowStatus("Threw an exception: look for 'Unity' and 'InvalidOperationException' in logcat.");
            // Unity catches an exception thrown from a UI event, logs it with its stack
            // and carries on: exactly what an unhandled bug in a handler looks like.
            throw new InvalidOperationException("[QA] Deliberate exception from the QA scene.");
        }

        /// <summary>Wired to the ANR button's OnClick.</summary>
        public void OnAnrPressed()
        {
            float now = Time.unscaledTime;
            if (now < _anrIgnoredUntil)
            {
                ShowStatus(AnrIgnoredText);
                return;
            }

            if (!_anr.Press(now))
            {
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            _anrIgnoredUntil = now + FreezeMilliseconds / 1000f + ArmWindow;
            ShowStatus("UI thread blocked for 10 s: tap an empty spot now, then watch logcat for 'ANR in'.");
            Debug.Log("[QA] Blocking the Android UI thread for 10 s");
            UnityEngine.Android.AndroidApplication.InvokeOnUIThread(() => Thread.Sleep(FreezeMilliseconds));
#else
            ShowStatus("An ANR needs a phone: the editor has no Android UI thread to block.");
            Debug.Log("[QA] ANR skipped in the editor");
#endif
        }

        /// <summary>Wired to the Native crash button's OnClick.</summary>
        public void OnCrashPressed()
        {
            if (!_crash.Press(Time.unscaledTime))
            {
                return;
            }

#if UNITY_EDITOR
            ShowStatus("A native crash needs a phone: in the editor it would close the editor.");
            Debug.Log("[QA] Native crash skipped in the editor");
#else
            Debug.Log("[QA] Forcing a native crash (SIGSEGV)");
            Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
#endif
        }

        void OnLowMemory()
        {
            _lowMemoryCount++;
            Debug.Log($"[QA] lowMemory #{_lowMemoryCount}");
            ShowStatus($"Application.lowMemory raised {_lowMemoryCount} time(s).");
        }

        void ShowStatus(string text)
        {
            status.text = text;
        }
    }
}
