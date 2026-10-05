---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-10-05
tags: [decision, loading, shared, async]
---

# Every scene load goes through the SceneLoader

## Context

Until Week 5 the [[Launcher]] and `BackToLauncher` each called `SceneManager.LoadSceneAsync` directly, with no loading screen. Week 5 Lab B has students build one persistent `SceneLoader` and route every load through it, the first one included, so the loading screen, the load-time measurement and the "no second load while loading" rule hold everywhere. A teaching project that shows the loader in one sample while its own scene changes bypass it would contradict the pattern it teaches.

## Decision

`SceneLoader` is a shared script in `Scripts/Shared/` under [[2026-10-04 Shared scripts are the ones every scene needs]]. The Launcher scene creates it once on a `DontDestroyOnLoad` object with its own loading canvas, and a later copy destroys itself. `LauncherMenu` and `BackToLauncher` load through `SceneLoader.Instance`; while `SceneLoader.IsLoading` is true, back and second loads do nothing. When no loader exists (a scene opened directly in the editor), `BackToLauncher` falls back to a plain `LoadSceneAsync` and logs one line, so samples stay testable on their own; `LauncherMenu` needs no fallback, because its own scene creates the loader.

## Consequences

- Every scene change shows the same loading screen and logs a `[perf] scene ... loaded in N ms` line, which feeds the baseline sheet's Week 5 row.
- Samples still reference no other sample's scripts; `SceneLoader` joins `BackToLauncher`, `LifecycleGuard` and `PauseMenu` as shared infrastructure.
- A static `Instance` is accepted for this one persistent object, as the lab sheet allows; it is not a pattern for anything else.
- The fallback path means opening a scene directly still works in the editor, but it does not exercise the loader; How to test steps start from the Launcher.
- Notes changed: [[Loading]], [[Launcher]], [[Home]].
