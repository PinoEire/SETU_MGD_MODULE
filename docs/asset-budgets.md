# Asset budgets

Worked example of the Week 9 Lab A, Parts C and D budgets for this project. Put your budget table in your Baseline Sheet (Week 3 names it `/docs/CA2/baseline/baseline-sheet.md`, later lab sheets `Performance_Baseline.md`: use one file) and keep the import rules below for your own art pass. This project keeps its table here, and the Week 9 row of `docs/baseline-sheet.md` points to it.

## Import rules (Part C)

| Asset | Android import settings |
|-------|-------------------------|
| Albedo and emissive textures | Override for Android, ASTC 6x6, max size 1024 (2048 only for atlases and one hero texture), Generate Mipmaps on, Streaming Mipmaps on for large ones |
| Normal maps, UI sprites | ASTC 4x4, max 1024 (2048 for atlases), mipmaps off for sprites and UI |
| Sprite atlases | One per screen (HUD, menu, in-game UI), Include in Build, ASTC 4x4, max 2048 |
| Music (30 s or longer) | Streaming, Vorbis, quality 70 |
| Sound effects under 1 s | Decompress On Load, PCM or ADPCM |
| Longer effects and voice | Compressed In Memory, Vorbis |
| Every clip | Force To Mono, unless the stereo is audible; 22050 Hz for effects with nothing above 11 kHz |

*MGD Samples > Audit Asset Budgets* checks the texture and audio rules above against every asset under `Assets/_Game` and prints one `[Budget]` warning per broken rule. The 30 s and 1 s boundaries between music, longer effects and short effects are this project's choice; the lab sheet names the categories, not the lengths, so a short music loop under 30 s gets the longer-effect rule. The audit holds every sprite to ASTC 4x4, the UI rule; a 2D game's gameplay sprites are albedo and may use 6x6, so read those lines as a prompt to decide, not an error. A 2048 atlas or hero texture is reported every time by design: the list is for review. The audit does not check atlases, sample rates, Vorbis quality or streaming mipmaps, which need a person to judge.

### Audit of this project, 2026-11-08

```
[Budget] Assets/_Game/Textures/Loading/HeavyTexture.png: no Android override: tick Override for Android in the Inspector's Android tab
[Budget] Assets/_Game/Textures/Loading/HeavyTexture.png: format RGB24, expected ASTC_4x4
[Budget] Assets/_Game/Textures/Loading/HeavyTexture.png: max size 2048: 1024 unless this is an atlas or the lane's hero texture
[Budget] Assets/_Game/Textures/SafeArea/GridBackground.png: no Android override: tick Override for Android in the Inspector's Android tab
[Budget] Assets/_Game/Textures/SafeArea/GridBackground.png: format ETC2_RGB4, expected ASTC_4x4
[Budget] Assets/_Game/Textures/SafeArea/GridBackground.png: max size 2048: 1024 unless this is an atlas or the lane's hero texture
[Budget] 6 problem(s) in 4 texture(s) and audio clip(s) under Assets/_Game.
```

`HeavyTexture.png` is over budget on purpose: the Loading sample needs a slow load to show its progress bar. `GridBackground.png` is the real offender: a 2048 sprite without an Android override, which a release build imports as ETC2 at the full 2048 (the format `TextureImporter.GetAutomaticFormat` reports for Android, from the project's default texture compression). The fix is an Android override at ASTC 4x4 (4x4 rather than the background 6x6, because its 1-pixel grid lines would smear in 6x6 blocks), max 1024, then a rebuild and a fresh Memory Profiler capture. The Tiers sample's own checker and bar textures pass.

## Budgets and measurements (Part D)

Measure on the phone with a Development build: *Window > Analysis > Memory Profiler > Capture* after 60 s in the busiest scene, then read Texture2D, AudioClip and Managed from the snapshot's *All Of Memory* view (or, more directly for the per-type totals, its *Unity Objects* tab). Total PSS comes from `adb shell dumpsys meminfo com.dftgames.mgdsamples` (Week 8). The budgets are this project's: it is a set of small samples, so they are far below what a student's game needs. Phone cells stay empty until the device run; nothing is guessed.

| Category | Budget (MB) | Measured (MB) | Inside? |
|----------|-------------|---------------|---------|
| Textures (incl. atlases) | 64 | | |
| Audio | 8 | | |
| Managed heap | 32 | | |
| Total PSS (from Week 8) | 400 | | |

Device, build and scene: (fill in: model, version and build ID, scene measured).
