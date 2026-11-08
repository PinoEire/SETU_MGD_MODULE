---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-11-08
tags: [decision, store, release]
---

# StoreShot is persistent and its button exists in Development Builds only

## Context

Week 10 Lab A has students add `StoreShot`, which hides the HUD for one frame and captures a store screenshot, with a `DebugShotButton` in a corner of the HUD that exists only in Development Builds. This project has fourteen scenes with no common HUD, and in most of them the UI is the content: hiding it would leave an empty screen to capture.

## Decision

The Launcher creates one `StoreShot` on a DontDestroyOnLoad object, as it does the Scene Loader and `AdaptiveBootstrap` (a later copy destroys itself), with its own canvas holding a single Shot button; the button is active only when `Debug.isDebugBuild`, so release builds carry the object but show nothing. The only thing hidden for the captured frame is that canvas. The capture waits in real time until the file is complete, instead of 0.5 s of game time.

## Consequences

- Every scene can be captured without touching its builder; the deck's separate `DebugShotButton` is folded into `StoreShot`.
- `StoreShot` lives in `Scripts/Store/`, not `Scripts/Shared/`: it is a development tool for one topic that a release build does not need, so [[2026-10-04 Shared scripts are the ones every scene needs]] does not apply, although the Launcher creates it.
- The lab removes both scripts in Week 12; here release builds keep the object with the button switched off, which `Debug.isDebugBuild` already guarantees, so there is nothing to remove before CA3.
- Release builds keep the object and its listener but no visible button, so a player can never trigger a capture.
- In a Development Build the button sits over the right edge of every screen and can cover a little of a sample's text while it is shown.
- Students are told to put their HUD root in `hideWhileCapturing`, the case this project does not have.
- Notes changed: [[Store]], [[Launcher]], [[Home]].
