---
type: sample
status: done
lab: W08-B
unity: 6000.6.6f1
scene: Assets/_Game/Scenes/QA/QA.unity
updated: 2026-10-27
tags: [sample, qa, logcat, anr, w08]
---

# QA

## Goal

Let students see each failure signature Week 8 Lab B asks them to find (a managed exception, an [[Glossary#ANR|ANR]], a native crash, a low-memory notice) once, on purpose, before it turns up in their own test run; and give them a worked six-scenario test plan and a Known Issues file written against this project's real code.

## Lab it supports

Week 8 Lab B, *QA evidence: logcat, ANRs and a test plan*: Part B.3 (the three signatures), Part C (the six scenarios, trigger 5's low-memory notice) and Part D (Known Issues) as worked examples, with Part A's warm-start and PSS row added to the baseline sheet.

## What the student learns

- What each failure looks like in logcat, and what it does to the app: an exception is logged and the app carries on, an ANR freezes it until Android offers to close it, a native crash ends it with `libc : Fatal signal 11 (SIGSEGV)` and a [[Glossary#Tombstone|tombstone]], a low-memory kill ends it with no crash line at all, only an Info-level `has died` line.
- Unity runs the game on its own thread, not on Android's UI thread: blocking Unity's thread freezes the game, but an ANR comes from the UI thread Android watches, which is why the ANR button blocks that one (`AndroidApplication.InvokeOnUIThread`).
- A button whose action cannot be undone needs a guard against a stray tap; a second tap within a few seconds is the simplest.
- A memory notice is not guaranteed: Android 14 and later no longer send apps the running trim levels, so a game cannot rely on `Application.lowMemory` and must keep to its own memory budget.
- A test plan row is only done when it has evidence: a logcat line or a screenshot, and the build ID it was run on.
- A Known Issue is written for a stranger: steps, expected, actual, the log line, the build, the device.

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/QA/QaTriggers.cs` | Three buttons, each behind an `ArmedTrigger` with a 3 s window: throws `InvalidOperationException`; blocks the Android UI thread for 10 s through `AndroidApplication.InvokeOnUIThread`; `Utils.ForceCrash(ForcedCrashCategory.AccessViolation)`. In the editor the last two only log. Labels read *Tap again to confirm* while armed (swapped only on a change, no per-frame allocation). Presses of the ANR button during the freeze, delivered when the UI thread wakes, are ignored. Counts `Application.lowMemory` and logs `[QA] lowMemory #n`. Teaching scaffolding: a student's own game should not ship these buttons. |
| `Assets/_Game/Scripts/QA/ArmedTrigger.cs` | Pure: `Press(now)` arms, a second press inside the window fires, firing disarms; `IsArmed(now)`. What students copy for any irreversible button. |
| `Assets/_Game/Scripts/QA/README.md` | The three signatures and how to make them, for students who open the folder without this vault. |
| `Assets/_Game/Editor/QA/QaSceneBuilder.cs` | Menu item *MGD Samples > Build QA Scene*: title, intro, the three 140-unit buttons, a status line, the trim-memory command as a hint; `BackToLauncher` listening and the shared pause card. Laid out for portrait (see Known limits). |
| `Assets/_Game/Editor/Tests/ArmedTriggerTests.cs` | Six EditMode tests: first press arms only, second inside the window fires, a press exactly at the end of the window still fires, after the window it re-arms, firing disarms, arming runs out. |
| `Assets/_Game/Scenes/QA/QA.unity` | Generated; not hand-edited. |
| `docs/qa/test-plan.md` | The six scenarios for this app with what the code is written to do and which script does it, the signature table and the live filter; Pass/Fail and Evidence are filled on the phone. |
| `KnownIssues.md` | Four real open issues from code review, in the lab's format. |
| `docs/baseline-sheet.md` | Gains the Week 8 row (warm start, PSS after five minutes). |

## How to test

1. **Editor:** Play the Launcher, tap QA. Tap *Throw exception* once: the label reads *Tap again to confirm* and turns back after 3 s. Tap twice: the console shows `InvalidOperationException: [QA] Deliberate exception from the QA scene.` and the status line says where to look. Twice on *Freeze UI thread (ANR)* or *Native crash*: the status line says each needs a phone, and the editor keeps running. On a phone, ANR presses in the 13 s after a freeze show *Ignored* instead.
2. **Phone, exception:** `adb logcat -c`, then `adb logcat -v time > qa.txt`; two taps on *Throw exception*: the `Unity` exception and its stack, and the app keeps running.
3. **Phone, ANR:** two taps on *Freeze UI thread (ANR)*, then straight away tap an empty part of the screen: 5 s after that tap, `ActivityManager: ANR in com.dftgames.mgdsamples` and the "isn't responding" dialog; choose *Wait*: when the 10 s freeze ends the app is back with the pause panel up (the dialog took focus); press Resume.
4. **Phone, crash:** two taps on *Native crash*: the app closes and logcat shows Unity's `CRASH` lines, `libc : Fatal signal 11 (SIGSEGV)` and the tombstone under `DEBUG`.
5. **Phone, low memory:** `adb shell am send-trim-memory com.dftgames.mgdsamples RUNNING_CRITICAL` with QA open: on Android 14 and later the count stays at 0 (the running levels are no longer sent to apps); on an older phone it should rise. Record which.
6. **Test plan:** run the six rows of `docs/qa/test-plan.md` on the release build and fill Pass/Fail and Evidence.
7. **EditMode tests:** *MGD Samples > Run EditMode Tests*: 179 passed.

## Known limits

- The phone behaviour (the ANR dialog after a tap during the freeze, the SIGSEGV tombstone, the trim-memory count on Android 17) is what the Android and Unity documentation describe; it is confirmed on a phone before the lab, and this note is updated with what the Pixel 7a showed.
- The layout is for portrait: in landscape the status line runs into the bottom hint (Known Issue 4).
- The buttons work in release builds, behind the two-tap guard, so the lecturer's release APK can demonstrate them; a student's game should not ship such buttons.
- No low-memory kill is produced here: Android decides when to kill a background process; scenario 5 sets up the conditions.
- Verified 2026-10-27 in editor Play mode.

## Cuts list

1. The low-memory counter: the lab's scenario 5 works without it; kept because it shows whether the notice reaches the game at all.
2. The status line: logcat alone is the evidence; kept so a student without a terminal open still sees what to look for.

## Decisions

- [[2026-10-27 The QA crash buttons work in release builds behind a two-tap guard]]
- [[2026-09-15 Back navigation returns to the launcher]]
- [[2026-10-04 Every scene pauses on focus loss]]

## References

- Lab sheet: `Week 08/Lab B/W08-LabB-LabSheet.md` in the curriculum folder.
- ANRs: https://developer.android.com/topic/performance/vitals/anr
- `logcat`: https://developer.android.com/tools/logcat
- App start-up time: https://developer.android.com/topic/performance/vitals/launch-time
- `Utils.ForceCrash`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Diagnostics.Utils.ForceCrash.html
- `Application.lowMemory`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Application-lowMemory.html
