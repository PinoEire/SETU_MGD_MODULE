---
type: sample
status: done
lab: W11-A
unity: 6000.6.8f1
scene: Assets/_Game/Scenes/Resume/Resume.unity
updated: 2026-11-08
tags: [sample, lifecycle, awaitable, cancellation, offline-progress, w11]
---

# Resume

## Goal

Show timed [[Glossary#Awaitable|Awaitables]] that survive pause and resume: no timer fires twice, fires while the game is paused, or outlives its scene. Also show offline progress measured on the wall clock, so an idle game pays for the time away even after Android has killed the process. Students copy the timer shape into every timed loop in their own game.

## Lab it supports

Week 11 Lab A, Part C (*Apply the ResumeGuard pattern*) and Part D (*Run the six-case resume matrix*). Parts A and B (Perfetto and AGI) are tools run against any build and need no sample. Week 11 Lab B's `FrameTimeSampler` is already in [[Performance]], and its test plan and bug list build on [[QA]].

## What the student learns

- One `CancellationTokenSource` feeds every timed await. Pausing cancels it, so every await stops; resuming creates a new source and restarts the loops. Cancelling the old source before the new one exists means no loop can ever run twice.
- `Awaitable.WaitForSecondsAsync` throws `OperationCanceledException` when its token is cancelled. Catching it is the expected path, not an error.
- A one-shot timer must remember how far it got and that it has fired. Restarting it blindly on every resume fires it again.
- A repeating loop can simply start a fresh interval on resume.
- Link the source to `destroyCancellationToken` and `Application.exitCancellationToken` (`CancellationTokenSource.CreateLinkedTokenSource`), so nothing outlives the scene or the app.
- `WaitForSecondsAsync` counts game time: at `Time.timeScale = 0` it stops counting.
- Restart timers on the game's own resume, here the player's Resume tap (`LifecycleGuard.PausedChanged`), not on `OnApplicationPause(false)`: see [[2026-11-08 Timers restart on the game's resume, not on OnApplicationPause(false)]].
- Offline progress needs the wall clock: save `DateTime.UtcNow` on `OnApplicationPause(true)` and compare on return. `Time.time` does not advance while the app is suspended and restarts from zero in a new process. Clamp the gap to 0 (clock moved back) and a maximum (design cap, and a limit on clock-forward cheating).

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/Resume/ResumeGuard.cs` | What students copy. A one-shot boss timer that keeps its remaining time and fires once, and a repeating wave loop, both on one linked token source that is cancelled on pause and replaced on resume. Restarts its timers when re-enabled. |
| `Assets/_Game/Scripts/Resume/PausableCountdown.cs` | Copied with the guard. Plain C#: the time left, running and fired state of a one-shot timer, given the time by the caller, so it is testable in EditMode. |
| `Assets/_Game/Scripts/Shared/LifecycleGuard.cs`, `PauseMenu.cs` | Shared, needed by `ResumeGuard`: the static pause state and its `PausedChanged` event, and the Resume button that raises it (see [[Lifecycle]]). |
| `Assets/_Game/Scripts/Resume/OfflineProgress.cs` | What idle-game students copy. Saves the UTC time on `OnApplicationPause(true)`, collects on `OnApplicationPause(false)` or, after a process kill, on `Start`; `SecondsAway` clamps the gap. |
| `Assets/_Game/Scripts/Resume/ResumeDemoHud.cs` | Demo only. Boss countdown, wave count, last offline reward and an eight-line log of the boss spawn, pause and resume, the offline reward, and lifecycle callbacks with wall-clock times. It watches the guard, so the guard has no hooks of its own. |
| `Assets/_Game/Scripts/Resume/README.md` | Two-paragraph summary for students who open the folder without this vault. |
| `Assets/_Game/Editor/Tests/ResumeTests.cs` | Seven EditMode tests: the countdown keeps its time left across a stop, never goes negative, does not start again once fired, and starts from the full duration after Reset; time away is the wall-clock gap, 0 when the clock moved back, and capped. |
| `Assets/_Game/Scenes/Resume/Resume.unity` | A `Resume Guard` object with both scripts; the Canvas with the HUD, a Restart button, the shared pause menu and `BackToLauncher` (back returns to the list). Changed in place. |
| `docs/qa/resume-tests.md` | The six-case matrix as a worked example, run on a Pixel 7a. |

## How to test

1. **Editor:** Play (click Resume if the panel is up). The boss counts down from 5 and spawns once; waves count every 2 s. Mid-countdown, click outside the Game view (focus loss pauses): the log shows `Paused: timers stopped` and the countdown freezes. Escape does not pause here; it returns to the Launcher. Click Resume: the countdown carries on from where it stopped. Pause and resume after the boss has spawned: it does not spawn again. Restart timers repeats the run.
2. **Phone, the six-case matrix:** see `docs/qa/resume-tests.md`. Watch `adb logcat -s Unity` for `[Resume]` lines and the on-screen log.
3. **Tests:** *MGD Samples > Run EditMode Tests*: the seven `ResumeTests` pass.

## Known limits

- The boss timer's progress is not saved, so after a process kill the countdown starts again. A real game saves timer state along with the rest of its save on `OnApplicationPause(true)`.
- Offline progress is collected when the Resume scene starts, so after a process kill the time away includes the time spent in the Launcher before the scene was opened. A game collects it at start-up instead.
- The wave loop loses the part of an interval that had run when the game paused. That is a choice for a repeating tick; a slow spawner would keep its remaining time the way the boss does.
- In the editor, a stamp written when Play stops before the app returns stays in PlayerPrefs, so the next Play pays for the whole gap, up to the 8-hour cap. Delete the key `MGD.Resume.PausedAtUtc` to start clean.
- Incoming call (case 2) was not run, because it needs a second phone. The lab sheet treats a call like Home (`OnApplicationPause(true)`); a heads-up call that is not answered may only take focus. Rotation (case 5) was not run: forcing it over adb does not turn a sensor-orientation app.
- Verified 2026-11-08 in editor Play mode (pause mid-countdown, resume, pause after the spawn) and on a Pixel 7a (Android 17) with a release-settings build: Home, shade, screen lock and process death. The lab sheet's original snippet (corrected the same day) ran alongside in that build and spawned its boss three times (`docs/qa/resume-tests.md`). Its restarted timer fired 5 s after the player tapped Resume, not 5 s after the app returned: this is how `WaitForSecondsAsync` was seen to stop at time scale 0.

## Cuts list

1. The wave loop could go, leaving only the one-shot timer. Kept because the two shapes behave differently on resume, and students' games have both.
2. `OfflineProgress` could go, leaving offline time to the lab sheet's one sentence. Kept because the process-death case needs something that survives the kill, and the idle option depends on it.

## Decisions

- [[2026-09-15 Awaitable over coroutines]]
- [[2026-10-04 Cancellation tokens where the awaited API takes one]]
- [[2026-10-04 Every scene pauses on focus loss]]
- [[2026-11-08 Timers restart on the game's resume, not on OnApplicationPause(false)]]
- [[2026-11-08 Scenes are changed in place, never rebuilt from scratch]]

## References

- Lab sheet: `Week 11/Lab A/W11-LabA-LabSheet.md` in the curriculum folder (Parts C and D, Troubleshooting).
- Unity docs: https://docs.unity3d.com/6000.6/Documentation/Manual/async-awaitable-continuations.html, https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Awaitable.WaitForSecondsAsync.html and https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Application-exitCancellationToken.html
