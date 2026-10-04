---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-10-04
tags: [decision, lifecycle, shared, pause]
---

# Every scene pauses on focus loss

## Context

Only the [[Lifecycle]] scene carried `LifecycleGuard` and `PauseMenu`, because each sample demonstrates one topic and sample scripts never reference each other. On the phone that meant the shade, Home or a call left the other five scenes running, which the lecturer noticed on 2026-10-04 while testing release build 11. The module rule students build under is "reliable pause/resume and focus-loss recovery" for the whole game, not for one scene, and the samples should show the rule everywhere, not contradict it.

## Decision

`LifecycleGuard` and `PauseMenu` are shared scripts in `Scripts/Shared/` under [[2026-10-04 Shared scripts are the ones every scene needs]]. Every scene builder calls `SampleSceneBuild.AddPauseMenu`, which adds the guard on its own object, a hidden pause panel with a Resume button, and the menu on the Canvas. The menu's back toggle is on only where the scene owns the back gesture (Lifecycle, which also gets a Back to samples button, and the Launcher, which has nowhere to go back to); everywhere else `BackToLauncher` keeps the gesture and the toggle is off, so the two never act on the same key. Gameplay that reads input outside the UI raycast (Enhanced Touch in [[TouchDrag]]) gates itself on `LifecycleGuard.IsPaused`, which is the rule the Lifecycle README already gave students.

## Consequences

- Every scene pauses on the open shade, quick settings, recents, the assistant, a dialog, Home, a call and screen off, with the same panel and the same Resume button, so the behaviour students see is the one they must ship.
- The Lifecycle sample still owns the explanation and the on-screen callback log; its note's Files table now points at `Scripts/Shared/` for the two scripts.
- The Launcher pauses too. A paused menu is harmless (its buttons are blocked by the dim), and one rule with no exceptions is easier to copy than a rule with one.
- [[2026-09-15 Back navigation returns to the launcher]] is not superseded; only its "on the launcher, back is ignored" remark is replaced: back on the launcher now toggles the pause panel.
- System UI that does not take window focus (the status bar peek of a full-screen game, a heads-up notification, the volume popup) still cannot pause anything; see Known limits in [[Lifecycle]].
- Notes changed: [[Lifecycle]], [[Launcher]], [[SafeArea]], [[Accessibility]], [[Performance]], [[TouchDrag]], [[Home]].
