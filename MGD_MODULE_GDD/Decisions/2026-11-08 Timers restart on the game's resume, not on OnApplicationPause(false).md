---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-11-08
tags: [decision, lifecycle, awaitable, cancellation]
---

# Timers restart on the game's resume, not on OnApplicationPause(false)

## Context

Week 11 Lab A gave a `ResumeGuard` snippet that cancels its token source in `OnApplicationPause` and restarts the boss timer in the `false` branch. In this app the game does not resume on its own: [[Lifecycle]] keeps it paused, at time scale 0, until the player taps Resume. Android also calls `OnApplicationPause(false)` once at launch, which can start a second copy of a timer that `Start` has already started. On a Pixel 7a the snippet, run verbatim, spawned its boss again after every return from the background, because it restarted a timer that had already fired. The sheet and the deck were corrected the same day: the snippet now ignores a call that does not change the pause state and restarts only a timer that has not fired.

## Decision

Timed Awaitables in this project stop when `LifecycleGuard.PausedChanged` reports a pause and restart when it reports a resume. Each restart uses a new `CancellationTokenSource`, linked to `destroyCancellationToken` and `Application.exitCancellationToken`, and the old source is always cancelled first. One-shot timers keep their remaining time and do not restart once fired. `OnApplicationPause` is used only for what must happen when the app leaves or returns: saving, and offline progress on the wall clock.

## Consequences

- Timers stay in step with what the player sees: frozen behind the pause panel, running after Resume, never twice.
- The launch-time `OnApplicationPause(false)` does nothing to the timers, and a timer that has fired is never re-armed by a resume.
- The corrected lab snippet still restarts in `OnApplicationPause(false)`, as a bare script to test the shape. Step 3 of Part C then moves the cancel and the restart to the pause menu's pause and resume, as here (the project also keeps a one-shot's remaining time; the sheet restarts it from full); [[Resume]] explains why, and `docs/qa/resume-tests.md` records the snippet's double spawn on the phone.
- A timer that must keep running in the background (none in this project) would need a different design, such as wall-clock deadlines.
