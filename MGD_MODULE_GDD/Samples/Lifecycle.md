---
type: sample
status: done
lab: W02-B
unity: 6000.6.0f1
scene: Assets/_Game/Scenes/Lifecycle/Lifecycle.unity
updated: 2026-09-15
tags: [sample, lifecycle, android, w02]
---

# Lifecycle

## Goal

Show a game surviving everything Android does to it: Home, app switch, incoming call, screen off, the notification shade, a permission dialog and the back gesture. The game pauses and saves on its own; it resumes only when the player says so. Students copy the guard and the pause menu into their own project.

## Lab it supports

Week 2 Lab B, Part A: *Lifecycle, back, accessibility and scope lock*. The lab sheet gives the procedure and the five-row test matrix; this sample is the finished result students compare against. Parts B and C (haptics toggle, text size) are separate samples.

## What the student learns

- `OnApplicationPause(true)` is the last reliable moment to save; the OS may kill the process afterwards. `OnApplicationPause(false)` also fires once at launch on Android, so only the `true` case acts.
- `OnApplicationFocus(false)` covers the cases pause does not: the notification shade, permission dialogs, the on-screen keyboard.
- Home fires both callbacks, so `SetPaused` ignores repeats.
- `Time.timeScale = 0` stops physics, animation and `deltaTime` but not `Update`; gameplay must check `LifecycleGuard.IsPaused` itself. The demo's spinner stops, its real-time clock does not.
- `AudioListener.pause` silences every source that does not opt out with `ignoreListenerPause`.
- Android back arrives through the Input System as the Escape key. It toggles pause; it never quits. Mobile games have no Quit button (see the handout).
- Static state such as `IsPaused` and `timeScale` outlives the scene, and in the editor `timeScale` even survives leaving Play mode, so the guard resets both in `OnDestroy`.
- Toggle the pause panel itself, not its parent: a component under an inactive parent stops receiving the event.
- In the editor, pressing Play while the editor window is not the focused window fires `OnApplicationPause(true)` on the first frame, so the sample starts paused with the game clock at zero. That is the guard doing its job, not a bug; click Resume.

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/Lifecycle/LifecycleGuard.cs` | On the Bootstrap object. Saves and pauses on pause or focus loss, exposes `IsPaused` and `PausedChanged`, sets `timeScale` and `AudioListener.pause`. What students copy. |
| `Assets/_Game/Scripts/Lifecycle/PauseMenu.cs` | On the Canvas. Shows the panel from the event, toggles pause on Escape (Android back), handles Resume. What students copy. |
| `Assets/_Game/Scripts/Lifecycle/LifecycleDemoHud.cs` | Demo only. Spinner, game-time and real-time clocks, a generated looping tone, a tap counter kept in PlayerPrefs, and a six-line log of pause and focus callbacks with wall-clock times. |
| `Assets/_Game/Scripts/Lifecycle/README.md` | Two-paragraph summary for students who open the folder without this vault. |
| `Assets/_Game/Editor/Lifecycle/LifecycleSceneBuilder.cs` | Menu item *MGD Samples > Build Lifecycle Scene*. Builds and saves the scene, wires the buttons, adds the scene to Build Settings. |
| `Assets/_Game/Editor/Shared/SampleSceneBuild.cs` | Camera, event system, canvas and uGUI helpers shared by every scene builder. Editor-only. |
| `Assets/_Game/Scenes/Lifecycle/Lifecycle.unity` | Bootstrap with the guard; Canvas with the demo HUD and Tap button, plus a pause panel: a half-transparent full-screen dim that blocks taps, and an opaque card (title, Resume, Back to samples, hint) placed below the HUD readouts so the clocks stay readable while paused. Generated; not hand-edited. |
| `Assets/_Game/Scripts/Shared/BackToLauncher.cs` | On the Canvas with the back listener off, since back pauses here; the card's *Back to samples* button calls `Go()`. Not part of what students copy. |

## How to test

1. **Editor:** Play. The spinner turns, the tone plays, both clocks advance. Press Escape: the panel appears, the spinner and game-time clock stop, the real-time clock continues, the tone goes silent, the log shows `IsPaused = true`. Click Resume or press Escape again. Tap the button a few times, stop Play, Play again: the count is restored.
2. **Phone, the five-row matrix** from the lab sheet, reading the on-screen log instead of Logcat:

| Test | Expected |
|------|----------|
| Press Home, wait 10 s, return | Panel visible, tone silent, log shows focus lost then pause; game clock stopped, real clock jumped ahead |
| Pull the notification shade down and up | Paused (focus lost only) |
| Take an incoming call, hang up | Paused; play resumes only on Resume |
| Screen off with the power button, back on | Paused |
| Tap the counter, force stop from Settings, relaunch | Count restored |

3. **Back gesture:** with the panel closed, swipe back: panel opens. Swipe back again: it closes. The app never exits.

## Known limits

- No Settings button on the pause panel. The lab adds one for haptics and text size; those are separate samples and the button belongs with them.
- Back in menus outside play (a `MenuStack` with push and pop) is out of scope, as the lab sheet allows.
- The HUD uses a fixed margin instead of the SafeArea panel, because samples do not reference each other's scripts. A real game would put this HUD under `SafeArea`.
- The tone is generated at runtime rather than shipped as an audio asset, to keep the sample free of binary files.
- Verified 2026-09-15: editor Play mode (pause, resume, save and restore, exit to launcher) and the five-row matrix on an Android device.

## Cuts list

1. The callback log could go, leaving Logcat as the only evidence. Kept because it makes the matrix checkable in the lab without a laptop.
2. The tone could go, leaving `AudioListener.pause` untested. Kept because "audio silent" is a row in the matrix.

## Decisions

- [[2026-09-15 Project-owned assets live under Assets _Game]]

## References

- Lab sheet: `Week 02/Lab B/W02-LabB-LabSheet.md` in the curriculum folder (Part A, Troubleshooting).
- Handout: `Handouts/Quit-Buttons-in-Mobile-Apps-and-Games.md`.
- Unity docs: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnApplicationPause.html and https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnApplicationFocus.html
- Android predictive back: https://developer.android.com/guide/navigation/custom-back/predictive-back-gesture
