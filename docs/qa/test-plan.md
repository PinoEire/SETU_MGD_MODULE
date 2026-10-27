# Test plan

Worked example of the Week 8 Lab B, Part C table for this project. Submit yours as `/docs/CA3/qa/test-plan.md`, with the whole run recorded in `/docs/CA3/qa/logcat-w08-testplan.txt` and one screenshot per scenario that needs one in `/docs/CA3/qa/screens/`. Package `com.dftgames.mgdsamples`; the build ID comes from the About scene. The Expected column is what this project's code is written to do, with the script that does it; Pass/Fail and Evidence are filled on the phone, and nothing is guessed.

Build: (fill in: versionName, versionCode, build ID). Device: (fill in: model, Android version). Date: (fill in). Commands in the Trigger column run on the phone, so prefix them with `adb shell` from the PC.

| # | Scenario | Trigger | Expected | Pass/Fail | Evidence |
|---|----------|---------|----------|-----------|----------|
| 1 | Resume / background | Home, wait 30 s, relaunch, ten times; then `am force-stop` and relaunch | Pause panel up on return, audio silent, Resume continues in place (`LifecycleGuard`); after force-stop the Economy wallet loads from PlayerPrefs (`WalletPersistence`) and the High FPS choice is applied again (`PerformanceMode`) | | |
| 2 | No network | Airplane mode on before launch, and again mid-play | Nothing changes: the app has no network code and no `INTERNET` permission; when a form link is set, *Open feedback form* still hands it to the browser, which reports no connection | | |
| 3 | Rotation | Auto-rotate on, rotate mid-play in SafeArea and Adaptive | Auto-rotation is allowed: the SafeArea HUD re-fits, no stretched UI, no restart; the Adaptive pause card covers part of the readout in landscape (Known Issue) | | |
| 4 | Battery saver | `settings put global low_power 1` (or Quick Settings), two minutes in Adaptive | Playable; the overlay shows any change in frame time and the `[AP]` lines any bottleneck change; note whether the system cap held the frame rate below the target | | |
| 5 | Low RAM | In QA, `am send-trim-memory com.dftgames.mgdsamples RUNNING_CRITICAL`, then background, open the camera and a browser for a minute, return | The QA count of `Application.lowMemory` stays at 0 on Android 14 and later (see the note under the table) and may rise on older phones; returns without a black screen; if Android killed the process, relaunch opens the Launcher and the saves are intact | | |
| 6 | Cold and warm start | `am force-stop`, then `am start -W com.dftgames.mgdsamples/com.unity3d.player.UnityPlayerGameActivity`; Home, 10 s, the same line again | `TotalTime` recorded for both, warm well below cold (`am` reports the second as `LaunchState: HOT`, the lab's warm start); the loading screen never hangs | | |

The activity name above is for GameActivity, this project's entry point; confirm it with `adb shell cmd package resolve-activity --brief com.dftgames.mgdsamples`.

Row 5: Android 14 (API 34) and later no longer send apps the `RUNNING_*` trim levels, so the QA count staying at 0 there is the expected result, not a bug. A game cannot rely on the notice and must keep to its own memory budget.

## Signatures to find in the recording

Make each one once in the QA scene before the run, so it is recognisable when it turns up for real:

| Failure | Logcat signature | What happens to the app |
|---------|------------------|-------------------------|
| Managed exception | A `Unity` line with `InvalidOperationException: ...`, then `at ...` stack lines (logcat pads the tag, so search for the exception name) | Carries on; its state may be wrong |
| ANR | `ActivityManager: ANR in com.dftgames.mgdsamples`, reason `Input dispatching timed out` | Android offers to close it |
| Native crash | Unity's `CRASH` lines, then `libc : Fatal signal 11 (SIGSEGV)`, then the tombstone under `DEBUG` | Gone |
| Low-memory kill | No crash line; `ActivityManager: Process com.dftgames.mgdsamples (pid N) has died` at Info level | Gone; the next launch is a cold start |

Live filter while the recording runs: `adb logcat -s ActivityManager:E AndroidRuntime:E libc:F CRASH:E DEBUG:F Unity:E`. It shows errors only, so look for a low-memory kill in the full recording: `findstr "has died" docs\CA3\qa\logcat-w08-testplan.txt`.

## Failures found

The worst failure of each run is fixed and its row re-run, or filed in `KnownIssues.md` at the repo root with steps, expected versus actual, the logcat line, the build ID and the device.
