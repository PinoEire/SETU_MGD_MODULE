---
type: sample
status: done
lab: [W09-A, W09-B]
unity: 6000.6.8f1
scene: Assets/_Game/Scenes/Tiers/Tiers.unity
updated: 2026-11-08
tags: [sample, performance, quality, device-tiers, asset-budgets, w09]
---

# Tiers

## Goal

Show students how one build adapts to the phone it runs on: three quality levels, a [[Glossary#Device tier|device tier]] chosen at start-up from RAM and cores with an override for testing, the Week 7 Balanced / High FPS choice applied on top of the tier, and a way to check texture and audio import settings against a budget before measuring memory on the phone.

## Lab it supports

Week 9 Lab A, *Art pass, asset budgets and device tiers*: Part A (three [[Glossary#Quality level|quality levels]] with one URP asset each), Part B (`DeviceTier.cs`, the tier override, the tier in `session_start`), Parts C and D as a worked example and a tool (`docs/asset-budgets.md` and *Audit Asset Budgets*). Week 9 Lab B's final-sprint targets are worked in `docs/targets.md`.

## What the student learns

- A quality level bundles settings that cost the GPU (render scale, MSAA) and memory (texture resolution) into one switch, and each level can point at its own URP asset.
- The tier is decided once, before the first scene, from what the phone reports (`SystemInfo.systemMemorySize`, `SystemInfo.processorCount`); an override saved in PlayerPrefs lets a tester force a tier, and it applies from the next launch because swapping quality level mid-scene reloads the pipeline asset.
- Two systems that both set the frame rate and render scale need one order. The lab uses Script Execution Order; here the order is a line of code: the tier, then the performance mode.
- High FPS only pays off on a phone with headroom to spend; on a Mid or Low phone it is disabled and says why, rather than doing nothing silently.
- [[Glossary#ASTC|ASTC]] compression, a maximum size and mipmaps only where a texture is drawn scaled keep texture memory inside a budget; streaming long music and decompressing only short effects do the same for audio.
- A budget is checked twice: by the import rules in the editor, then by the Memory Profiler on the phone.

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/Shared/DeviceTier.cs` | Shared, static. `Tier` Low, Mid, High (the quality level indices). `Detect(ram, cores)` is the deck's rule; `Resolve` lets a valid `tier_override` win; per-tier `FrameRateFor` (30, 60, 60) and `RenderScaleFor` (0.75, 0.9, 1.0); `LevelsMatch` checks the levels are Low, Mid, High in order. At `BeforeSceneLoad` it sets the quality level, logs `tier=... detected=... override=... ram=...MB cores=...` and calls `PerformanceMode.ApplySaved`. `SetOverride` and `SavedOverrideOption` back the dropdown. In the editor the quality level is restored when Play ends. What students copy. |
| `Assets/_Game/Scripts/Shared/PerformanceMode.cs` | Now applied on top of the tier: Balanced is the tier's cap and scale, High FPS (120, 0.8) only on the High tier (`Effective`, `HighFpsAllowedOn`); the saved choice is kept whatever the tier; the log line ends with `tier=`. Belongs to [[Adaptive]]. |
| `Assets/_Game/Scripts/Shared/PauseMenu.cs` | Below the High tier the High FPS toggle is not interactable and reads *High FPS (High tier only)*. Belongs to [[Lifecycle]]. |
| `Assets/_Game/Scripts/Shared/MobileBootstrap.cs` | The `[Boot]` line and `session_start` name the tier. Belongs to [[Performance]]. |
| `Assets/_Game/Scripts/Performance/RenderScaleProbe.cs` | Restores `PerformanceMode.CurrentRenderScale`, the tier's scale. Belongs to [[Performance]]. |
| `Assets/_Game/Scripts/Tiers/TierReadout.cs` | The scene: the four-line readout (`Describe`: tier and why, quality level, render scale, MSAA, frame cap, texture resolution), the override dropdown (dynamic int, `SetValueWithoutNotify` at start, closed on pause so its list cannot sit above the pause card) with the note `OverrideNote` (what applies on the next launch), and a turning bar for MSAA. The readout is rebuilt on resume, because the pause card's High FPS toggle can change it. Demo, but `Describe` is worth copying for a debug screen. |
| `Assets/_Game/Scripts/Tiers/README.md` | Two-paragraph summary for students who open the folder without this vault. |
| `Assets/_Game/Editor/Tiers/QualityTiersSetup.cs` | Menu item *MGD Samples > Set Up Quality Tiers*: copies the template's URP asset to `URP-Low`, `URP-Mid`, `URP-High` in `Assets/_Game/Settings/Quality/` (scale 0.75 / 0.9 / 1.0, MSAA 1 / 2 / 4), replaces the template's six quality levels with Low, Mid, High (mipmap limit 1 / 0 / 0, anisotropic off / per texture / forced, texture streaming on), Android default Mid. Run once; running again updates in place. |
| `Assets/_Game/Editor/Tiers/AssetBudgetRules.cs`, `AssetBudgetAudit.cs` | The Part C rules as pure checks on `TextureFacts` and `AudioFacts`, and the menu item *MGD Samples > Audit Asset Budgets* that applies them to every texture and audio clip under `Assets/_Game` and prints `[Budget]` lines. What students copy. |
| `Assets/_Game/Scenes/Tiers/Tiers.unity` | The scene: the turning bar, title, readout, a 560-unit checker, the override label, a note and a TextMeshPro dropdown whose closed control and every list item are 130 units tall (about 49 dp, where the default items are about 20); `BackToLauncher` listening and the shared pause card. Changed in place. |
| `Assets/_Game/Textures/Tiers/Checker.png`, `Bar.png` | `Checker.png` is 512 px with mipmaps, Android ASTC 6x6. `Bar.png` is a 4x4 opaque white sprite with a full-rect mesh, so the turning bar's edge is geometry that MSAA can smooth. |
| `Assets/_Game/Settings/Quality/URP-Low.asset`, `URP-Mid.asset`, `URP-High.asset` | Generated by the set-up menu; see [[2026-11-08 Quality tier assets live in Assets _Game Settings]]. |
| `ProjectSettings/QualitySettings.asset` | Three levels instead of the template's six. |
| `Assets/_Game/Editor/Tests/DeviceTierTests.cs`, `PerformanceModeTests.cs`, `TierReadoutTests.cs`, `AssetBudgetRulesTests.cs`, `QualityTiersSetupTests.cs` | The tier thresholds, overrides and per-tier values; the mode rules on each tier and the saved choice; the readout and note texts; every texture and audio rule, alone, together and at the 1024, 1 s and 30 s boundaries; and the committed quality levels and URP assets (names, scale, MSAA), which catches a render scale left at 0.8 by an interrupted Play. |
| `docs/asset-budgets.md`, `docs/targets.md`, `docs/baseline-sheet.md` | The import rules, this project's audit result and memory budgets; the three final-sprint targets; the Week 9 baseline row. |
| `KnownIssues.md`, `docs/privacy-statement.md`, `docs/economy-telemetry-map.md`, `docs/perf-modes.md` | Known Issue 5 (landscape); the `tier_override` key and the tier in `session_start`; Balanced now following the tier. |

## How to test

1. **Editor, set-up:** *MGD Samples > Set Up Quality Tiers*. Project Settings > Quality lists Low, Mid, High, with Mid as the Android default, each pointing at its `URP-` asset.
2. **Editor, start-up:** Play the Launcher. The console shows `tier=High detected=High override=False ram=...MB cores=...` (a development PC is High), then `[Boot] ... (Balanced, tier High)` and `session_start ... tier=High`.
3. **Editor, Tiers:** tap Tiers. The readout reads `Tier High (detected: ...)`, `Render scale 1.00  MSAA 4x  60 fps`, `Textures at full resolution`. Choose *Low*: the note reads `Low from the next launch.`
4. **Editor, Low:** stop and Play again. The readout reads `Tier Low (override; detected High: ...)`, `Render scale 0.75  MSAA off  30 fps`, `Textures at half resolution`, and the checker is blurrier. Pause: the High FPS toggle is greyed out and reads *High FPS (High tier only)*. Stop: Project Settings > Quality is back on High. Choose *Auto* to clear the override.
5. **Editor, audit:** *MGD Samples > Audit Asset Budgets*: six `[Budget]` lines for `HeavyTexture.png` and `GridBackground.png` and a summary; the Tiers sample's checker and bar textures pass.
6. **Phone:** release build. `adb logcat -s Unity | findstr /L "tier="` shows the tier the phone picked (the start-up `tier=` line; the `perf_mode=` and `session_start` lines also carry `tier=`); force another from Tiers, relaunch, check the line and the readout. Fill the Week 9 row of `docs/baseline-sheet.md` and the table in `docs/asset-budgets.md` from a Memory Profiler capture of a Development build.
7. **EditMode tests:** *MGD Samples > Run EditMode Tests*: 240 passed.

## Known limits

- The deck's thresholds count RAM and cores only; a phone with plenty of both and a weak GPU lands too high. The override is the escape hatch; a real game would also time a few frames.
- `SystemInfo.systemMemorySize` reports the RAM Android gives apps, a little below the size on the box, so a phone sold as 8 GB can fall below the deck's 8192 MB line and land on Mid: the Pixel 7a reports `ram=7466MB` and picks Mid (the `tier=` line's `ram=` value shows what yours reports), where High FPS is greyed out; force High from the Tiers scene to try High FPS on such a phone.
- The lab table's Main Light Shadows row is a Universal (3D) Renderer setting and does nothing under the 2D Renderer this project uses. Post-processing and 2D shadows (Light2D with ShadowCaster2D) do exist in 2D, but no sample uses them, so the levels leave them out; a 2D game that uses them tiers the camera's post-processing and the lights' shadows instead.
- URP shader stripping (Part A.4) is not set up: the project's builds are small and no sample has the features to strip.
- `Shader.globalMaximumLOD` is not set: none of this project's shaders has LOD blocks.
- In landscape the checker covers the override controls (Known Issue 5).
- Verified 2026-11-08 in editor Play mode on 6000.6.8f1, and on a Pixel 7a with release 0.6.0 (19): `tier=Mid detected=Mid override=False ram=7466MB cores=8`, with `[Boot]` and `session_start` carrying `tier=Mid`. Memory Profiler numbers pending.

## Cuts list

1. The checker texture: the readout already says "half resolution"; kept because seeing the blur is what makes a mipmap limit stick.
2. The turning bar: MSAA is in the readout too; kept because aliasing on a moving edge is visible on a phone at arm's length.
3. The audit menu: the lab only asks for import settings by hand; kept because it turns the rules into a check that takes a second.

## Decisions

- [[2026-11-08 The device tier sets the quality level and High FPS runs only on High]]
- [[2026-11-08 Quality tier assets live in Assets _Game Settings]]
- [[2026-10-19 Performance mode is shared and its toggle is on every pause card]]
- [[2026-10-04 Shared scripts are the ones every scene needs]]
- [[2026-11-08 Scenes are changed in place, never rebuilt from scratch]]

## References

- Lab sheets: `Week 09/Lab A/W09-LabA-LabSheet.md` and `Week 09/Lab B/W09-LabB-LabSheet.md` in the curriculum folder, and the deck's `DeviceTier` slide.
- Quality settings: https://docs.unity3d.com/6000.6/Documentation/Manual/class-QualitySettings.html
- `SystemInfo.systemMemorySize`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/SystemInfo-systemMemorySize.html
- Texture compression formats for Android: https://docs.unity3d.com/6000.6/Documentation/Manual/android-texture-compression-format.html
- Audio clip import settings: https://docs.unity3d.com/6000.6/Documentation/Manual/class-AudioClip.html
- Memory Profiler: https://docs.unity3d.com/Packages/com.unity.memoryprofiler@latest
