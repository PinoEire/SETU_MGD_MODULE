# Performance Baseline Sheet

Worked example of the Week 3 Lab B, Part F table for this project. Submit yours as `/docs/CA2/baseline/baseline-sheet.md`, one column per build, and add a column whenever something meaningful changes. The Menu row is the Performance scene at Idle, Steady gameplay is the Steady state (200 sprites), Worst case is the Worst state (1500 sprites at scale 4). Phone cells come from the release build on the device with `adb logcat -s Unity` (the second `[Baseline]` line of each state), `dumpsys meminfo` and `am start -W`; nothing is guessed. The Week 5 and Week 7 rows stay empty until those labs.

| Field | Row 1 |
|-------|-------|
| Date, commit, versionName / versionCode, Release or Dev, Unity version | 2026-10-04, 9d7d162 (tree dirty at build time), 0.2.0 / 10, Release, 6000.6.0f1 |
| Device model, Android version, SoC / GPU, graphics API, refresh rate | Google Pixel 7a, Android 17 (API 37), Tensor G2 / Mali-G710, Vulkan, 60 Hz panel mode (90 Hz available; Android keeps games at 60 unless asked) |
| Target fps | 60 (`MobileBootstrap`, first scene) |
| Menu: avg ms / p99 ms | 16.66 / 16.77 (editor: 16.67 / 17.60); both frame-rate capped |
| Steady gameplay: avg ms / p99 ms | 16.66 / 16.81 (editor: 16.80 / 18.41); both frame-rate capped |
| Worst case: avg ms / p99 ms | 16.65 / 16.73 (editor: 17.16 / 17.85); the Pixel 7a holds 60 fps with 1500 translucent sprites, so this scene is not a worst case for it and `worstCount` should be raised before it is used as one |
| GC allocated per frame (bytes), allocating markers | 0 B from the sample's scripts in Steady and Worst; `FrameTimeSampler.Update` allocates one string every 600 frames (editor Profiler) |
| Peak Total Reserved (MB) / TOTAL PSS (MB) | Total Reserved needs the Profiler on a Development build (not measured in this release run) / 300 MB (TOTAL PSS 307,289 kB after 50 s in Worst) |
| Worst case: SetPass / batches / triangles | 14 / 1,506 / 10,895 (editor Stats; 1,500 of the batches go through the SRP Batcher, which is why SetPass stays at 14) |
| Cold start ms (median of 3), first interactive s | 122 ms (171, 116, 122 from `am start -W`; the Launcher is interactive as soon as it draws, under 1 s) |
| APK size (MB) | 42.8 (44,928,385 bytes) |
| Thermal delta, throttling (Week 7) | |
| Load time (Week 5) | |
