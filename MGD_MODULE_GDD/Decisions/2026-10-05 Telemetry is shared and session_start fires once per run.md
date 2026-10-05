---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-10-05
tags: [decision, telemetry, shared]
---

# Telemetry is shared and session_start fires once per run

## Context

Week 6 Lab B has students add a static `Telemetry` class and log `session_start` from the bootstrap in their first scene, then the four gameplay events from their game. In this project the first scene is the [[Launcher]], whose `MobileBootstrap` runs every time the Launcher is loaded, and the Launcher is loaded again whenever the player goes back to it. Logging there unguarded would write a `session_start` per visit, and later samples will want to log events too.

## Decision

`Telemetry` is a shared script in `Scripts/Shared/` under [[2026-10-04 Shared scripts are the ones every scene needs]], so any sample can log without referencing another sample; sample scripts call `Telemetry.Log` directly, as Pooling and TouchDrag already read `LifecycleGuard.IsPaused`. `MobileBootstrap` logs `session_start` (and its existing `[Boot]` line) once per app run, behind a static flag that is reset at `RuntimeInitializeLoadType.SubsystemRegistration` so it also behaves with domain reloading off in the editor. Gameplay events are logged by the sample that owns the gameplay; the [[Economy]] sample logs `level_start`, `level_complete`, `level_fail` and `upgrade_purchased`.

## Consequences

- One `session_start` per run, and one session id per run in every line, which is what lets a pulled `telemetry.log` be split into sessions.
- The repeated `[Boot]` line seen on every return to the Launcher goes away.
- `telemetry.log` is written from any scene, so the privacy statement names it once for the whole app.
- Notes changed: [[Economy]], [[Launcher]], [[Performance]], [[Glossary]], [[Home]].
