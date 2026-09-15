---
type: sample
status: done
lab: none
unity: 6000.6.0f1
scene: Assets/_Game/Scenes/Launcher/Launcher.unity
updated: 2026-09-15
tags: [sample, infrastructure, navigation]
---

# Launcher

## Goal

The first scene in the build: a menu that lists every sample and loads the one tapped, with a footer that shows app version, Unity version and device. It is infrastructure rather than a lab topic, but it is also the first place students see three module rules applied: async loading with `Awaitable`, an About / Build Info line, and back navigation that never quits the app.

## Lab it supports

None directly. The footer is the About / Build Info screen required for CA3; the load pattern previews Week 5 Lab B (*async loading that never hangs*).

## What the student learns

- Build Settings can be read at runtime (`SceneManager.sceneCountInBuildSettings`, `SceneUtility.GetScenePathByBuildIndex`), so a menu need not hard-code its entries.
- `await SceneManager.LoadSceneAsync(...)` inside an `async Awaitable` method is the whole async pattern; there is no coroutine.
- Disable the buttons the moment a load starts, or a second tap starts a second load.
- Every sample must have a way back. The rule is in [[2026-09-15 Back navigation returns to the launcher]].

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/Launcher/LauncherMenu.cs` | On the Canvas. Builds one button per scene in Build Settings (except itself), loads on tap, fills the footer. |
| `Assets/_Game/Scripts/Launcher/README.md` | Two-paragraph summary for students who open the folder without this vault. |
| `Assets/_Game/Scripts/Shared/BackToLauncher.cs` | Shared. Loads build index 0 on Android back (Escape), or from a button through `Go()` with the listener off. Every sample scene carries one. |
| `Assets/_Game/Editor/Launcher/LauncherSceneBuilder.cs` | Menu item *MGD Samples > Build Launcher Scene*. Builds the scene and moves it to index 0 in Build Settings. |
| `Assets/_Game/Scenes/Launcher/Launcher.unity` | Title, subtitle, a vertical list with an inactive button template, footer. Generated; not hand-edited. |

## How to test

1. **Editor:** open the Launcher scene and Play. One button per other scene appears; the footer shows the version and this PC. Tap SafeArea: it loads; press Escape: the launcher returns. Tap Lifecycle, press Escape to pause, tap *Back to samples*: the launcher returns with the game unpaused (the guard resets timeScale on destroy).
2. **Phone:** the same with the back gesture. Back on the launcher itself does nothing; the app never exits from back.
3. **Adding a sample:** run its builder, then Play the launcher: the new button is there without touching the launcher.

## Known limits

- Buttons show the scene file name only. A description per sample would need a catalogue asset; not worth it while the names are self-explanatory.
- The list does not scroll. It has room for about eight samples at the reference resolution; add a ScrollRect when the count gets there.
- Back on the launcher is deliberately ignored rather than minimising the app.
- Verified 2026-09-15: editor Play mode (list, load, both return paths) and on an Android device.

## Cuts list

1. The footer could be dropped from the launcher and made its own About sample later. Kept because the module wants build info visible from the first build.

## Decisions

- [[2026-09-15 Back navigation returns to the launcher]]
- [[2026-09-15 Awaitable over coroutines]]

## References

- Unity docs: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html and https://docs.unity3d.com/6000.6/Documentation/Manual/AwaitSupport.html
- Handout: `Handouts/Quit-Buttons-in-Mobile-Apps-and-Games.md`.
