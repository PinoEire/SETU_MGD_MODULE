---
type: sample
status: done
lab: W05-B
unity: 6000.6.0f1
scene: Assets/_Game/Scenes/Loading/Loading.unity
updated: 2026-10-05
tags: [sample, loading, awaitable, scenes, w05]
---

# Loading

## Goal

Show a scene load that never hangs: one persistent `SceneLoader` that every scene change in the app goes through, with a loading screen whose bar follows the real `AsyncOperation.progress`, and the `allowSceneActivation` trap demonstrated live so students recognise the infinite loading screen when they meet it. Alongside it, the `[perf]` lines that put load time and time to interactive into the baseline sheet.

## Lab it supports

Week 5 Lab B, *Greybox lane and async loading that never hangs*: Part B (the `SceneLoader`, the loading canvas, `Instance`, every load through one path), Part C (break it on purpose, the Home and back resilience checks) and Part D (`[perf]` scene load and time to interactive; cold start comes from `am start -W`). Part A (the greybox lane) and Part E (playtest) belong to the students' own projects. The [[Launcher]] plays the role of the lab's `Boot` and `Menu` scenes.

## What the student learns

- `SceneManager.LoadSceneAsync` with `allowSceneActivation = false` loads to `progress` 0.9 and then waits; the bar is `progress / 0.9`, and the loop condition is `progress < 0.9f`, never `!isDone`, because `isDone` cannot become true until activation is allowed.
- The infinite loading screen is not a freeze: frames keep ticking, the bar is full, `progress` sits at 0.90, and nothing will ever change it. The trap button shows exactly that, as the lab sheet's Part C describes.
- One loader, created once in the first scene, kept with `DontDestroyOnLoad`, reached through a static `Instance`; a second copy (when the first scene is loaded again) destroys itself in `Awake`.
- Every scene change goes through the same path, the first one included, so the loading screen, the `[perf]` line and the "no second load while loading" rule apply everywhere (decision [[2026-10-05 Every scene load goes through the SceneLoader]]).
- [[Glossary#Awaitable|Awaitable]] stepping with `Awaitable.NextFrameAsync(ct)` keeps running while `timeScale` is 0, so a pause does not stall a load; the token comes from `Application.exitCancellationToken` and nothing in the pause code cancels it.
- A pause that lands during a load (Home, a call, the shade) would be lost, because the outgoing scene's `LifecycleGuard` clears it on the way out; the loader notices it and puts it back once the new scene is active, so the player returns to a pause panel, not a running game.
- Back does nothing while a load runs: there is no sensible place to go mid-load.
- Load time is measured with a `Stopwatch` around the load and time to interactive with `Time.realtimeSinceStartup` in the first menu `Update`; cold start is Android's number (`am start -W`), not Unity's.

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/Shared/SceneLoader.cs` | The lab's `SceneLoader` at project quality. `Awake` keeps the first instance with `DontDestroyOnLoad` and sets `Instance`, and destroys any later copy. `Load(int buildIndex, CancellationToken)` and `Load(string sceneName, CancellationToken)`: refuse a second load while `IsLoading`, show the loading canvas, load with `allowSceneActivation = false`, step frames with `NextFrameAsync(ct)` while `progress < 0.9f` setting the bar to `BarValue(progress)`, then allow activation, await the operation, put back a pause that started during the load (not in the lab sheet; see What the student learns), log `[perf] scene <name> loaded in N ms` and hide the canvas. `BarValue` is static and pure so it can be tested. What students copy. |
| `Assets/_Game/Scripts/Shared/MenuReady.cs` | The lab's time-to-interactive marker. On the Launcher; its first `Update` logs `[perf] interactive X.XXs` once per app run (a static flag, reset on load for domain-reload-off editors). What students copy. |
| `Assets/_Game/Scripts/Launcher/LauncherMenu.cs` | Changed: buttons call `SceneLoader.Instance.Load(buildIndex, ...)`. No fallback is needed, because the Launcher scene creates the loader itself. |
| `Assets/_Game/Scripts/Shared/BackToLauncher.cs` | Changed: `Go()` and the back listener do nothing while `SceneLoader.IsLoading`; the load goes through the loader. With no loader (a scene opened directly in the editor) it falls back to a plain `LoadSceneAsync` and logs one line. |
| `Assets/_Game/Scripts/Shared/PauseMenu.cs` | Changed: back does not toggle the pause panel while `SceneLoader.IsLoading`, so back on the Launcher mid-load does nothing as well. |
| `Assets/_Game/Scripts/Loading/LoadingDemoHud.cs` | Demo only. "Load heavy scene" calls the loader. "Load with the bug" runs the lab's broken `while (!op.isDone)` loop in its own method, with its own bar (the same `progress / 0.9` mapping, so it fills and then sticks, as in the lab sheet) on the Loading scene's canvas, never in `SceneLoader`; a watchdog (`IsStuck(progress, secondsHeld, limit)`, pure) shows "Stuck at progress 0.9: isDone never turns true while allowSceneActivation is false" after 3 s at 0.9, holds the message 2 s, then allows activation. Switches the scene's `BackToLauncher` off while the demo load runs. |
| `Assets/_Game/Scripts/Loading/README.md` | Two-paragraph summary for students who open the folder without this vault. |
| `Assets/_Game/Editor/Launcher/LauncherSceneBuilder.cs` | Changed: adds the `Scene Loader` object (persistent) with its own loading canvas (Screen Space Overlay, sort order above every other canvas, full-screen dim that blocks taps, non-interactive Slider with no handle, percentage label), and `MenuReady`. The list buttons are 130 units tall (about 49 dp) with 16 units between them, so the eight scenes fit above the footer. |
| `Assets/_Game/Editor/Loading/LoadingSceneBuilder.cs` | Menu item *MGD Samples > Build Loading Scene*. Builds `Loading.unity` (title, explanation, the two buttons, the demo bar, `BackToLauncher`, pause menu) and `LoadingHeavy.unity` (the generated weight, a Back button wired to `BackToLauncher.Go`, pause menu); adds both to Build Settings after Pooling. Weight constants (sprite count, texture size) live at the top. |
| `Assets/_Game/Textures/Loading/HeavyTexture.png` | Generated by the builder: one 2048 x 2048 texture that the heavy scene references, so the load reads real data. A smooth pattern rather than noise, so the PNG in the repo stays small; the import settings decide its size in the build. |
| `Assets/_Game/Editor/Tests/SceneLoaderTests.cs` | EditMode: `BarValue` (0, 0.45, 0.9, above 0.9 clamped), `IsStuck` (below 0.9, at 0.9 before and after the limit), the Launcher scene holds exactly one `SceneLoader` whose canvas sorts above every other canvas in the scene, `LoadingHeavy.unity` is under the 5 MB file budget. |
| `Assets/_Game/Scenes/Loading/Loading.unity`, `Assets/_Game/Scenes/Loading/LoadingHeavy.unity` | Generated; not hand-edited. |
| `docs/baseline-sheet.md` | The Week 5 *Load time* row, to fill from the phone run (pending). |

## How to test

1. **Editor, every load:** Play the Launcher. The console shows `[perf] interactive X.XXs` once. Tap any sample: the loading canvas covers the screen for the load and a `[perf] scene <name> loaded in N ms` line appears. Back (Escape) returns through the same canvas. Return to the Launcher several times: still exactly one `Scene Loader` object under DontDestroyOnLoad.
2. **Editor, heavy load:** open Loading, tap *Load heavy scene*: the bar moves from 0 to 100% and LoadingHeavy appears; its Back button returns to the Launcher.
3. **Editor, the trap:** tap *Load with the bug*: the demo bar fills and stops, the label reads `progress 0.90   isDone false`, the frame keeps ticking, after 3 s the message appears, 2 s later the heavy scene opens. Escape during any of this does nothing.
4. **Editor, guards:** start a load and press Escape or tap again: nothing happens. Pause during a load (focus loss): the load still completes and the new scene opens with its pause panel up. Back while paused: the Launcher arrives unpaused, because that pause was already there when the load started. Open `Loading.unity` directly and press Play, then Back: the fallback line is logged and the Launcher loads.
5. **Phone, Part C:** start the heavy load, press Home, wait five seconds, come back: the load completes and the canvas disappears. Android back on the loading screen does nothing.
6. **Phone, Part D:** release build, `adb logcat -s Unity | grep perf`: three cold starts with `am start -W` after `am force-stop`, the `interactive` line and the heavy scene's `loaded in` line go into the Week 5 row of `docs/baseline-sheet.md`.
7. **EditMode tests:** Window > General > Test Runner, EditMode, run all: the `SceneLoader` tests pass.

## Known limits

- The heavy scene is weight for its own sake (static sprites and one texture). Its numbers measure this project's load path on one phone, not a budget for students' scenes.
- The loader loads single scenes only; additive loading and Addressables are awareness topics, not shown.
- `Instance` is a static singleton, which the lab sheet accepts for exactly one persistent object; anything more (services, save system) deserves a proper bootstrap and is out of scope.
- `LoadSceneAsync` cannot be cancelled once started, so the token only stops the frame loop (on quit); a load always finishes.
- The editor loads far faster than a phone; editor `[perf]` numbers are recorded as editor numbers and are not comparable with the device row.
- Only a pause that starts during the load is put back. If the player presses back while paused and then Home within the same load, the new scene arrives unpaused; the window is a fraction of a second.
- LoadingHeavy also appears in the Launcher's list, because the Launcher lists every scene in Build Settings.
- `BackToLauncher.Go()` called from code bypasses the back listener, which is all the trap demo switches off; nothing in the scene calls it during the demo, but a button wired to `Go()` would start a second load, which queues behind the stuck one and runs once the watchdog lets go.
- In the editor the heavy load takes under 100 ms, so the bar barely shows on a desktop; the phone number decides whether the weight constants need raising.
- Verified 2026-10-05 in the editor: `[perf] interactive 0.54s`; loads of 46 to 83 ms (LoadingHeavy 83 ms); one loader with the same id after two returns to the Launcher; a heavy load with Back, a second load and a pause in the same frame made one load that completed into a paused scene with its panel up; the trap held at 0.90, explained about 4 s after the tap and opened LoadingHeavy by 6.1 s; stopping Play mid-load and Back from a directly opened Loading scene (fallback line) left no exception. Phone numbers pending. Re-checked after the fix that puts a mid-load pause back: a pause during the load arrives paused with `timeScale` 0 and the panel up, and a load started while paused (back while paused) and a plain load both arrive unpaused.

## Cuts list

1. The trap button could go, leaving the broken loop to the lab sheet; kept because seeing the bar sit at 90% with frames ticking is what makes the bug recognisable later.
2. `MenuReady` could go, leaving time to interactive to the cold-start number; kept because Part D asks for both and they measure different things.
3. The percentage label on the loading canvas could go, leaving the bar alone; kept because a number makes the 90% hold obvious.

## Decisions

- [[2026-10-05 Every scene load goes through the SceneLoader]]
- [[2026-10-04 Cancellation tokens where the awaited API takes one]]
- [[2026-09-15 Back navigation returns to the launcher]]
- [[2026-10-04 Every scene pauses on focus loss]]
- [[2026-10-04 Shared scripts are the ones every scene needs]]

## References

- Lab sheet: `SETU/2026-2027/Mobile Game Development/Week 05/Lab B/W05-LabB-LabSheet.md` (Dropbox).
- Deck: `A12581-W05-LabB-Greybox-lane-and-async-loading-that-never-hangs.pptx`.
- Unity 6.6 `SceneManager.LoadSceneAsync`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html
- Unity 6.6 `AsyncOperation.allowSceneActivation`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/AsyncOperation-allowSceneActivation.html
- Unity 6.6 `Awaitable.NextFrameAsync`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Awaitable.NextFrameAsync.html
- Android vitals, app startup time: https://developer.android.com/topic/performance/vitals/launch-time
