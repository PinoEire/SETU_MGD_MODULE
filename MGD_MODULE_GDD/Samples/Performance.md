---
type: sample
status: done
lab: [W03-A, W03-B]
unity: 6000.6.0f1
scene: Assets/_Game/Scenes/Performance/Performance.unity
updated: 2026-10-04
tags: [sample, performance, profiling, frame-pacing, w03]
---

# Performance

## Goal

Give students a scene that is deliberately heavy enough to profile, with the three tools Week 3 asks them to add to their own project: a bootstrap that sets the frame target, a render-scale probe that tells CPU-bound from GPU-bound, and a frame-time sampler that prints average and 99th percentile without allocating. Alongside it, worked examples of the bottleneck note and the first row of the Performance Baseline Sheet.

## Lab it supports

Week 3 Lab A, *Rendering budgets and profiling on device*: Parts B to F (the worst case to capture, the `RenderScaleProbe` test, the bottleneck note). Week 3 Lab B, *Frame pacing and your performance baseline*: Parts A to F (frame target, `FrameTimeSampler`, the three load states, the baseline sheet). Students who have no heavy scene of their own yet use this one for the captures.

## What the student learns

- Android runs at 30 fps until `Application.targetFrameRate` is set; the setting belongs in the first scene, once, with `QualitySettings.vSyncCount = 0` because Android ignores vSync.
- [[Glossary#Frame pacing|Frame pacing]] is measured in milliseconds with `Time.unscaledDeltaTime`, never in fps, and the number that matters is the 99th percentile, not the average.
- A sampler that runs in the release build must not allocate: fixed arrays, `Array.Sort` on a reused buffer, one `Debug.Log` per window of 600 frames.
- Halving the URP render scale is the cheapest CPU-versus-GPU experiment there is: frame time drops by a third or more, the GPU was the bottleneck; it barely moves, the CPU was.
- The same worst case must be measured in the same three states every time (idle, steady, worst) or the rows of the baseline sheet cannot be compared.
- Creating every object once at load and switching them on and off is the crude form of pooling; Week 5 replaces it with a real pool, but the "no `Instantiate` in the steady state" rule starts here.
- Draw count and overdraw are separate costs: 1500 small sprites are a CPU problem, 1500 large overlapping translucent sprites are a GPU problem too.

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/Shared/MobileBootstrap.cs` | Shared, on a `Bootstrap` object in the Launcher. `Awake` sets `targetFrameRate` 60, `vSyncCount` 0, `Screen.sleepTimeout` never, and logs the `[Boot]` line with device, OS, graphics API and resolution. The Week 1 script at project quality. What students copy. |
| `Assets/_Game/Scripts/Performance/FrameTimeSampler.cs` | Fixed buffer of 600 unscaled frame times, reset after each window. When full, copies into a reused sort buffer, computes average and p99 through `FrameStats`, logs `[Baseline] avg X ms  p99 Y ms` and writes the same text to an optional label with `SetText`. Skips the first frame after a load or a `Restart()`, whose delta carries the load time. No allocation in `Update`. What students copy. |
| `Assets/_Game/Scripts/Performance/FrameStats.cs` | Static. `Compute(samples, scratch, out average, out p99)`: the maths on its own so it can be unit tested. What students copy with the sampler. |
| `Assets/_Game/Scripts/Performance/RenderScaleProbe.cs` | `Toggle()` switches the active URP asset's `renderScale` between its starting value and 0.5, logs `[Probe] renderScale = x`, exposes `IsLow`. `OnDestroy` restores the starting value so the asset is not left modified after Play in the editor. What students copy. |
| `Assets/_Game/Scripts/Performance/LoadGenerator.cs` | Owns the sprite field. Creates `worstCount` SpriteRenderers once in `Awake` (built-in Knob sprite, URP `Sprite-Unlit-Default` material, random translucent colour), keeps positions and velocities in arrays, moves the active ones in one loop and bounces them off the camera edges, re-read when the screen size changes. `SetLoad(LoadState)` activates 0, `steadyCount` (200) or `worstCount` (1500) of them; the worst state also scales them up so they overlap. Counts and scales are serialised fields. Demo scaffolding students may borrow as a stress test. `Update` does nothing while `LifecycleGuard.IsPaused`. |
| `Assets/_Game/Scripts/Performance/PerformanceDemoHud.cs` | Demo only. Wires the Idle, Steady, Worst and Probe buttons; keeps the status label current (load state and active sprite count, target fps, panel refresh rate from `Screen.currentResolution.refreshRateRatio`, render scale); hides the Probe button unless `Debug.isDebugBuild`. |
| `Assets/_Game/Scripts/Performance/README.md` | Two-paragraph summary for students who open the folder without this vault. |
| `Assets/_Game/Editor/Performance/PerformanceSceneBuilder.cs` | Menu item *MGD Samples > Build Performance Scene*. Camera, event system, the `Load Generator` object with its sprite and material assigned, a dark translucent HUD card in the bottom third with title, status line, sampler line, the three load buttons in a row and the Probe button, then adds the scene to Build Settings. |
| `Assets/_Game/Editor/Launcher/LauncherSceneBuilder.cs` | Creates the `Bootstrap` object carrying `MobileBootstrap`, which is why the frame target belongs to the first scene. |
| `Assets/_Game/Editor/Tests/FrameStatsTests.cs` | Twelve EditMode tests for `FrameStats`: average and p99 of a known buffer, samples left unsorted, all-equal input, five spikes in 600 frames leaving p99 alone and seven moving it, the index at 600, 100, 2 and 1 samples, percentile clamped to 0..1, empty buffer and short scratch buffer throw. Lives in an `Editor` folder so it compiles into the editor assembly without an assembly definition. |
| `Assets/_Game/Scenes/Performance/Performance.unity` | Generated; not hand-edited. |
| `Assets/_Game/Scenes/Launcher/Launcher.unity` | Regenerated for the `Bootstrap` object. |
| `Assets/_Game/Scripts/Shared/BackToLauncher.cs` | On the Canvas with the listener on. Not part of what students copy. |
| `Assets/_Game/Scripts/Shared/LifecycleGuard.cs` | Shared, on the `Lifecycle Guard` object every scene gets through `SampleSceneBuild.AddPauseMenu`: pauses on focus loss and Home. Belongs to [[Lifecycle]]. |
| `Assets/_Game/Scripts/Shared/PauseMenu.cs` | Shared, on the Canvas with the hidden `Pause Panel`: shows the panel and handles Resume. Its back toggle is off here because `BackToLauncher` owns the gesture. Belongs to [[Lifecycle]]. |
| `ProjectSettings/ProjectSettings.asset` | *Optimized Frame Pacing* (Player > Resolution and Presentation, serialised as `androidUseSwappy: 1`) was already on; noted so the Lab B Part A check has an answer. *Run In Background* turned on so Play mode keeps ticking while the editor is driven from outside; it has no effect on Android. |
| `docs/bottleneck-01.md` | The Lab A note (What, Where, Numbers, Verdict, Fix to try) written for the Worst state of this scene. Editor numbers are marked as editor; phone numbers are filled in after the device run. |
| `docs/baseline-sheet.md` | The Lab B table with row one for this project. Fields that need the phone (frame times, PSS, cold start, APK size) are left empty and marked until the device run; the Week 5 and Week 7 rows stay empty by design. |

## How to test

1. **Editor, Launcher:** Play the Launcher. The console shows one `[Boot]` line naming this PC and its graphics API. Tap Performance.
2. **Editor, load states:** the scene opens in Idle: no sprites, the status line reads `Idle, 0 sprites | target 60 fps` over `panel 60 Hz | render scale 1.00` (panel value is the monitor's). Tap Steady: 200 small sprites bounce; tap Worst: 1500 larger translucent sprites fill the view and the Profiler's CPU frame time rises. Tap Idle: everything stops and the count is 0. The Stats overlay shows batches and SetPass calls climbing between the states.
3. **Editor, sampler:** after 600 frames a `[Baseline] avg X ms  p99 Y ms` line appears in the console and on the sampler label, and repeats every 600 frames. The Profiler's GC Alloc column for `FrameTimeSampler.Update` reads 0 B on every frame except the one that logs.
4. **Editor, probe:** the Probe button is visible (the editor counts as a debug build). Tap it: the console reads `[Probe] renderScale = 0.5`, the status line's render scale reads 0.50 and the Game view goes softer. Tap again: back to 1.00. Stop Play: the URP asset in `Assets/Settings/` shows no pending change.
5. **Editor, back:** Escape returns to the Launcher; the URP render scale is at its starting value.
6. **Phone, Lab A:** Android Dev build with the Profiler attached, Worst state for 30 s, then Probe and 30 s more. Fill *Numbers* and *Verdict* in `docs/bottleneck-01.md` from the capture.
7. **Phone, Lab B:** release build, `adb logcat -s Unity`, 20 s in each state, record the second `[Baseline]` line of each into `docs/baseline-sheet.md`, then `dumpsys meminfo`, three cold starts and the APK size for the remaining rows.
8. **EditMode tests:** Window > General > Test Runner, EditMode, run all: the `FrameStats` tests pass.

## Known limits

- The probe changes the shared URP asset at runtime, which is what the lab does; a real game would keep a per-quality asset or use `UniversalRenderPipeline.asset.renderScale` behind the Week 7 performance-mode toggle.
- `Screen.currentResolution.refreshRateRatio` reports the panel mode Android has chosen, which is 60 Hz for most games even on 120 Hz phones; requesting more is the Week 7 High FPS toggle.
- The sprite field is a stress test, not gameplay. It has no pooling API; Week 5 adds one.
- The bounce box follows the screen size, so rotating the phone re-fits it, but the three baseline rows should still come from one orientation or they are not comparable.
- The `[Baseline]` log line allocates its string once every 600 frames, which is the one deliberate allocation in the scene.
- Baseline and bottleneck numbers depend on the phone; the docs record which device they came from and are not budgets.
- The development PC never leaves the 16.7 ms frame cap in any load state, so the editor cannot give a CPU-bound or GPU-bound verdict; the probe only means something on the phone.
- Verified 2026-09-28: editor Play mode (load states, sampler lines, probe toggle and restore, back to launcher, oversized `steadyCount` clamped), the EditMode tests, and the scene on an Android device. 2026-10-04: the sampler skips the first frame after a restart, the bounce box follows the screen size, six tests were added for the percentile maths, and the three load buttons shrank to 220 units so they keep a gap on a 20:9 phone. Release build 0.2.0 (11) on a Pixel 7a: 16.66 / 16.77 ms (Idle), 16.66 / 16.81 (Steady), 16.65 / 16.73 (Worst), all frame-rate capped, in `docs/baseline-sheet.md`.

## Cuts list

1. The Probe button's `Debug.isDebugBuild` gate could go, showing it in release too; kept because the lab sheet tells students to gate it and the release build is for the sampler, not the probe.
2. The sampler label could go, leaving only the log line as in the lab sheet; kept because the release build then shows the numbers without logcat.
3. `docs/bottleneck-01.md` could be dropped, leaving the baseline sheet as the only worked example; kept because Lab A's exit ticket is exactly that note.

## Decisions

- [[2026-09-15 Back navigation returns to the launcher]]
- [[2026-10-04 Every scene pauses on focus loss]]
- [[2026-10-04 Shared scripts are the ones every scene needs]]

## References

- Lab sheets: `Week 03/Lab A/W03-LabA-LabSheet.md` and `Week 03/Lab B/W03-LabB-LabSheet.md` in the curriculum folder; `Week 01/W01-LabSheet.md` Part C for `MobileBootstrap`.
- Handout: `Handouts/Human-Vision-and-FPS.md`.
- Unity docs: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Application-targetFrameRate.html, https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Time-unscaledDeltaTime.html, https://docs.unity3d.com/6000.6/Documentation/Manual/urp/universalrp-asset.html, https://docs.unity3d.com/6000.6/Documentation/Manual/class-PlayerSettingsAndroid.html
- Android frame pacing library: https://developer.android.com/games/sdk/frame-pacing
