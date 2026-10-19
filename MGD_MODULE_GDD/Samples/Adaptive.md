---
type: sample
status: done
lab: [W07-A, W07-B]
unity: 6000.6.6f1
scene: Assets/_Game/Scenes/Adaptive/Adaptive.unity
updated: 2026-10-19
tags: [sample, performance, adaptive-performance, thermal, frame-pacing, w07]
---

# Adaptive

## Goal

Show students how to read the phone's thermal state and bottleneck through [[Glossary#Adaptive Performance|Adaptive Performance]], how to offer a Balanced / High FPS choice that changes the frame-rate target and the render scale and survives a restart, and how to run a thermal soak that produces evidence rather than impressions.

## Lab it supports

Week 7 Lab A, *Adaptive performance and the frame-rate toggle*: Parts A to E (install and enable the provider, `AdaptiveBootstrap`, `PerformanceMode` and its toggle in the pause menu, both modes on the phone, the perf-modes table). Week 7 Lab B, *Thermal soak*: Parts A to E (`ThermalOverlay`, the before capture, the 10-minute soak with screenshots at minutes 0, 5 and 10, the after capture, the *Thermals* section of the baseline sheet).

## What the student learns

- Adaptive Performance is a Unity feature since 6.3: the module ships with the editor, and the Android provider package (`com.unity.adaptiveperformance.google.android`) is the one install. It wraps Android's ADPF thermal and performance-hint APIs, which need Android 11 or later.
- Thermal and bottleneck events fire on change only; a cool phone can go minutes without one. Poll `ThermalStatus.ThermalMetrics` for a value now, as the overlay does.
- [[Glossary#Thermal throttling|Thermal throttling]] is what sustained performance is about: a phone that holds 16.7 ms for two minutes may not hold it for ten, and only a soak shows it.
- A performance setting changes global state (`Application.targetFrameRate`, the URP asset, a saved key), so it is applied once at start-up and from one place, and it belongs in the pause or settings menu, not on the HUD.
- A toggle initialised with `isOn` fires its event and re-applies the setting each time the menu opens; `SetIsOnWithoutNotify` does not.
- The URP asset changed at runtime in the editor is the project asset; it must be put back or the change ends up in git.
- High FPS asks for 120 and a 60 Hz panel clamps it; the render-scale drop is what buys the GPU headroom, and the bottleneck line shows whether it worked.
- An overlay that runs in the release build allocates nothing per frame: `SetText` with arguments for the numbers, a string rebuilt only when a value changes.

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/Shared/PerformanceMode.cs` | Shared, static. `PerfMode` Balanced (60 fps, scale 1.0) or HighFps (120, 0.8); `Apply` sets `targetFrameRate` and the active URP asset's `renderScale`, saves `perf_mode` and logs `perf_mode=<mode> high=<bool> target=<fps> scale=<scale>`; `SetHighFps(bool)` is the toggle form (a static method cannot be picked in a UnityEvent, so the toggle reaches it through `PauseMenu.OnHighFpsChanged`); `ApplySaved` runs before the first scene loads; an unknown saved value reads as Balanced. In the editor it puts the asset's render scale back once Play mode has ended. What students copy. |
| `Assets/_Game/Scripts/Shared/AdaptiveBootstrap.cs` | Shared, on a DontDestroyOnLoad `Adaptive Performance` object the Launcher creates (a later copy destroys itself). `Start` reads `Holder.Instance`, logs `[AP] provider inactive on this device` or subscribes to both events and logs `[AP] active warning=... level=...`; the handlers log `[AP] thermal ...` and `[AP] bottleneck=...` with invariant number formatting; `OnDestroy` unsubscribes. What students copy. |
| `Assets/_Game/Scripts/Shared/PauseMenu.cs` | Gains the `highFps` toggle: set with `SetIsOnWithoutNotify` from `PerformanceMode.IsHighFps` each time the panel opens, wired to `OnHighFpsChanged(bool)`. Belongs to [[Lifecycle]]. |
| `Assets/_Game/Scripts/Shared/MobileBootstrap.cs` | No longer sets the frame target (`PerformanceMode` does, earlier); its `[Boot]` line now reports the applied target and mode. Belongs to [[Performance]]. |
| `Assets/_Game/Scripts/Adaptive/ThermalOverlay.cs` | Three lines on two labels. The numbers label: smoothed unscaled frame time (the first frame, which carries the load, skipped), temperature level and trend, refreshed four times a second with `SetText`. The status label: warning level and bottleneck, then mode, target and render scale, a string rebuilt only when one of them changes (`StatusLine` is the pure formatter); `(AP inactive)` replaces the first status line when there is no provider. The deck uses one label; two keep the steady state allocation-free. What students copy. |
| `Assets/_Game/Scripts/Adaptive/SoakTimer.cs` | Real-time clock since *Restart soak*, the time the warning level first left `NoWarning`, and a `[Soak]` line every minute with the thermal metrics and mode. `MinuteCrossed` and `Split` are the pure parts. Demo scaffolding students may borrow for their soak. |
| `Assets/_Game/Scripts/Adaptive/HeatLoad.cs` | Creates 24 full-screen faint (5% alpha) layers once in `Awake` (built-in UISprite, URP `Sprite-Unlit-Default`), sized to cover the view in either orientation while turning; *Load* cycles Off, Warm (8) and Hot (24, the default) and logs `[Heat]`. Overdraw, so render scale changes its cost. Demo only. |
| `Assets/_Game/Scripts/Adaptive/README.md` | Two-paragraph summary for students who open the folder without this vault. |
| `Assets/_Game/Editor/Adaptive/AdaptiveSceneBuilder.cs` | Menu item *MGD Samples > Build Adaptive Scene*. The heat load, a dark readout card at the top (title, overlay lines, soak line, a hint that back opens the pause menu), *Load* and *Restart soak* buttons at the bottom, the pause card with back toggling pause and the *Back to samples* button, placed below the readout so the overlay stays readable while the menu is open in portrait (in landscape the card covers part of it). |
| `Assets/_Game/Editor/Shared/SampleSceneBuild.cs` | `AddPauseMenu` adds the High FPS toggle row (130 units, about 50 dp) to every pause card; the card grows by 150 units, and the Lifecycle card moved up to 0.4 so it still fits in landscape. |
| `Assets/_Game/Scripts/Performance/RenderScaleProbe.cs` | Restores the current mode's scale instead of the one it saw in `Awake`, because the mode can now change while the Performance scene is open. Belongs to [[Performance]]. |
| `Assets/_Game/Editor/Launcher/LauncherSceneBuilder.cs` | Creates the `Adaptive Performance` object. |
| `Assets/_Game/Editor/Tests/PerformanceModeTests.cs`, `ThermalOverlayTests.cs`, `SoakTimerTests.cs` | 23 EditMode tests: the two modes' values, unknown saved values, `SetHighFps` both ways (target, saved key, render scale, the log line), `ApplySaved` with and without a key; the status line active and inactive; minute crossings, a long stall, the minutes and seconds split. The mode tests put the key, the target and the URP asset back. |
| `Assets/_Game/Scenes/Adaptive/Adaptive.unity` | Generated; not hand-edited. Every other scene was regenerated for the new pause card. |
| `Assets/Adaptive Performance/` | Generated by Unity when the providers were ticked: the per-platform settings, the provider loaders and their settings (Android: Google Android, then Basic as fallback; logging on, indexer active, every scaler off). See [[2026-10-19 Adaptive Performance settings stay where Unity creates them]]. |
| `Packages/manifest.json` | Adds `com.unity.adaptiveperformance.google.android` 6.0.0. |
| `docs/perf-modes.md`, `docs/baseline-sheet.md` | The Lab A table and the Lab B *Thermals* section as worked examples, phone cells empty until the device run. |

## How to test

1. **Editor, Launcher:** Play the Launcher. The console shows `[Boot] ... target 60 fps (Balanced)` and `[AP] active warning=NoWarning level=0.00` (the editor runs the Simulator provider), and soon `[AP] bottleneck=TargetFrameRate`.
2. **Editor, Adaptive scene:** tap Adaptive. The readout shows the frame time, `temp 0.00  trend 0.00`, `NoWarning | bottleneck ...`, `Balanced  60 fps  scale 1.00` and the soak clock counting. *Load* cycles Off, Warm, Hot and logs `[Heat]`.
3. **Editor, toggle:** press Escape (Android back): the pause card opens below the readout with High FPS off. Tick it: the console reads `perf_mode=HighFps high=True target=120 scale=0.80` and the readout `High FPS  120 fps  scale 0.80`. Close and reopen the menu: the box is still ticked and no second `perf_mode` line appears. *Back to samples*, then Play again later: the mode is kept.
4. **Editor, thermal:** Window > General > Device Simulator, Adaptive Performance panel: drag the thermal level up; an `[AP] thermal warning=...` line appears and the readout's first line follows.
5. **Editor, clean-up:** stop Play in High FPS. `Assets/Settings/UniversalRP.asset` shows no change in git and its render scale reads 1.
6. **Phone, Lab A:** release build, `adb logcat -s Unity | findstr /L "[AP] perf_mode"` (`/L` makes `[AP]` literal; without it findstr reads it as a character class), an `[AP] active` line at start; 30 s in each mode in the Adaptive scene on Hot with a screenshot each; fill `docs/perf-modes.md`.
7. **Phone, Lab B:** `adb shell dumpsys battery | findstr temperature`, *Restart soak*, `adb logcat -v time -s Unity > logcat-soak.txt`, ten minutes on Hot in Balanced, screenshots at 0, 5 and 10 from the overlay, temperature again; fill the *Thermals* section of `docs/baseline-sheet.md` from the `[Soak]` and `[AP] thermal` lines.
8. **EditMode tests:** *MGD Samples > Run EditMode Tests*: 165 passed.

## Known limits

- Below Android 11 the provider loads but is not active: the overlay says so and the soak falls back to `dumpsys battery` and the Profiler, as the lab sheet's troubleshooting says.
- Scalers are all off, as Lab A asks; the sample reads the metrics but does not react to them. Adaptive degradation (lowering quality under thermal load) is a later step, not this sample.
- The heat load is overdraw and nothing else; a CPU-bound game heats a phone differently. Tune the layer counts per phone so Hot holds the frame rate at first.
- The soak clock counts real time, so leaving the app does not stop it; a soak with a break in it should be restarted.
- Adaptive Performance logging is on, which prints a status block every 50 frames in development builds; the lab sheet says to switch it off before the CA2 candidate build.
- In the editor, a script recompile during Play drops the hook that restores the URP render scale; stop Play before editing scripts, or check `UniversalRP.asset` in git.
- No automated test covers the toggle being set without firing when the menu opens (`OnEnable` does not run in EditMode); How to test step 3 checks it by hand.
- The PC never leaves the frame cap, so the editor confirms behaviour, not numbers. Verified 2026-10-19 in editor Play mode; phone numbers pending.

## Cuts list

1. The *Restart soak* button: the clock could start with the scene, as the lab's timer does; kept so a soak can start after the phone has cooled without reloading.
2. The Warm level: Off and Hot are enough for the demo; kept so a phone that throttles too fast on Hot still gets a ten-minute soak.
3. The minute `[Soak]` lines: the screenshots alone meet the lab; kept because they put a timeline in the logcat recording for free.

## Decisions

- [[2026-10-19 Performance mode is shared and its toggle is on every pause card]]
- [[2026-10-19 Adaptive Performance settings stay where Unity creates them]]
- [[2026-10-04 Shared scripts are the ones every scene needs]]
- [[2026-10-04 Every scene pauses on focus loss]]

## References

- Lab sheets: `Week 07/Lab A/W07-LabA-LabSheet.md` and `Week 07/Lab B/W07-LabB-LabSheet.md` in the curriculum folder, and the decks' `PerformanceMode` and `ThermalOverlay` slides.
- Adaptive Performance manual: https://docs.unity3d.com/Packages/com.unity.adaptiveperformance@latest
- Unity and ADPF: https://developer.android.com/games/engines/unity/unity-adpf, thermal API: https://developer.android.com/games/optimize/adpf/thermal
- Game Mode FPS throttling: https://developer.android.com/games/optimize/adpf/gamemode/fps-throttling
- `Application.targetFrameRate`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Application-targetFrameRate.html
