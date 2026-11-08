# Resume tests

Worked example of the Week 11 Lab A, Part D matrix for this project's Resume scene. Submit yours as `docs/CA3/qa/resume-tests.md`. Each case checks three things: state intact, timers (and audio) resume once, no exception in Logcat.

Build: 0.7.0 code plus the Resume sample, release settings (not a Development Build), built to `Temp/` for the test. Device: Google Pixel 7a, Android 17 (API 37). Date: 2026-11-08. Logcat: `adb logcat -s Unity`, plus the on-screen log in the scene.

| # | Case | Expected | Observed | Pass or fail | Logcat line if failed |
|---|------|----------|----------|--------------|-----------------------|
| 1 | Home at second 9 (boss already spawned), reopen from Recents after 10 s | Pause panel up; timers stopped while away; offline reward for the time away; after Resume, waves continue and the boss does not spawn again | Panel up; the log shows the timers stopping on focus loss; `Away 10 s, offline reward 9 coins`; after Resume, waves continued, one `[Resume] Boss spawned` in the whole run | Pass | |
| 2 | Incoming call | As case 1 (a call fires `OnApplicationPause(true)` like Home) | Not run: needs a second phone | Not run | |
| 3 | Notification shade pulled down and released | Focus loss only: timers stop and the game pauses, no save, no offline reward | `OnApplicationFocus(false)`, the timers stopping, then `OnApplicationFocus(true)`; no `OnApplicationPause`; panel up | Pass | |
| 4 | Screen lock with the power button, unlock after 48 s | As case 1 | `OnApplicationPause(true)` then `(false)` 48 s later; `Away 48 s, offline reward 47 coins`; panel up; timers started only on Resume | Pass | |
| 5 | Rotation | The UI stays inside the safe area, no drift | Not run: rotating through `settings put system user_rotation` over adb does not turn a sensor-orientation app; rotate the phone by hand | Not run | |
| 6 | Process death: Home 2 s into a boss countdown, `adb shell am kill com.dftgames.mgdsamples`, wait 20 s, relaunch | Launcher opens; reopening Resume pays the offline reward for the whole time away; no exception | Process gone (`pidof` empty); relaunch opened the Launcher; reopening Resume showed `Away 36 s, offline reward 36 coins`; a new boss countdown started from 5 s (timer progress is not saved; see the vault note) | Pass | |

Notes on this run:

- The waits are shorter than the lab's (30 s and a minute) to keep the run quick; the cases are the same.
- Case 1 starts after the boss has spawned on purpose, to show the snippet below re-firing. The countdown carrying over a pause is checked in the editor (see the Resume sample note, `MGD_MODULE_GDD/Samples/Resume.md`).
- The build used for this run rounded the seconds and truncated the coins (10 s, 9 coins); the sample now floors both, so they agree.
- In case 6 the time away (36 s) is longer than the 20 s wait because it also covers the relaunch and opening the scene from the Launcher, where the reward is collected (see the Resume sample note's Known limits).

The only error line in the whole run is the known `ClassNotFoundException` for `AssetPackManager` at start-up (noted in every release since 0.6.0), which is not caused by the resume path.

## The lab sheet's original snippet, run alongside

For comparison, the Part C snippet as the lab sheet first gave it (corrected since) ran verbatim on an extra empty object in the same build. Its resume branch restarts the boss timer on every `OnApplicationPause(false)`, even after the boss has spawned. In this run it logged `Boss spawned` three times: once on schedule, once after case 1 and once after case 4. The sample's `ResumeGuard` logged it once.
