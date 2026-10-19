---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-10-19
tags: [decision, performance, shared, pause-menu]
---

# Performance mode is shared and its toggle is on every pause card

## Context

Week 7 Lab A has students add a `PerformanceMode` component with a Balanced / High FPS toggle "in your pause or settings menu (not the HUD)", applied from a persistent boot object. In this project the frame target was set by `MobileBootstrap` in the Launcher, which runs again on every return to the Launcher and would reset a High FPS choice to 60. Every scene already has the shared pause card, and the setting must hold in every scene, because the frame target and the render scale are global.

## Decision

`PerformanceMode` is a static class in `Scripts/Shared/` under [[2026-10-04 Shared scripts are the ones every scene needs]], not a component: what it changes (`Application.targetFrameRate`, the active URP asset's `renderScale`, the `perf_mode` key) is global already. It applies the saved mode from a `RuntimeInitializeOnLoadMethod` before the first scene loads, so `MobileBootstrap` stops setting the target and only reports it. `SampleSceneBuild.AddPauseMenu` adds a High FPS toggle to every pause card; `PauseMenu` sets it with `SetIsOnWithoutNotify` when the panel opens and forwards changes to `PerformanceMode.SetHighFps`. In the editor, the render scale is restored when Play mode has ended. `AdaptiveBootstrap` is shared too, created once by the Launcher on a DontDestroyOnLoad object, so `[AP]` lines are logged whichever scene is open.

## Consequences

- One place sets the frame target, and the player's choice holds across scenes and restarts; the repeated 60 on each Launcher visit is gone.
- The toggle is reachable in every scene from the pause menu: by back where back toggles pause (Launcher, Lifecycle, Adaptive) and on focus loss elsewhere.
- Students who follow the lab literally write a component on a boot object; the README and the sample note explain why this project's version is static and where the toggle lives.
- Every scene was regenerated for the taller pause card.
- The Performance sample's render-scale probe restores the current mode's scale rather than the one it saw on load.
- Notes changed: [[Adaptive]], [[Performance]], [[Lifecycle]], [[Launcher]], [[Home]], [[Glossary]].
