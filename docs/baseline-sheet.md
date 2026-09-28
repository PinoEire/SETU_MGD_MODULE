# Performance Baseline Sheet

Worked example of the Week 3 Lab B, Part F table for this project. Submit yours as `/docs/CA2/baseline/baseline-sheet.md`, one column per build, and add a column whenever something meaningful changes. The Menu row is the Performance scene at Idle, Steady gameplay is the Steady state (200 sprites), Worst case is the Worst state (1500 sprites at scale 4). Cells marked *phone* are filled from the release build on the device with `adb logcat -s Unity`; they are left empty rather than guessed. The Week 5 and Week 7 rows stay empty until those labs.

| Field | Row 1 |
|-------|-------|
| Date, commit, versionName / versionCode, Release or Dev, Unity version | 2026-09-28, (commit), 1.0 / 1, Release, 6000.6.0f1 |
| Device model, Android version, SoC / GPU, graphics API, refresh rate | *phone* |
| Target fps | 60 (`MobileBootstrap`, first scene) |
| Menu: avg ms / p99 ms | *phone* (editor: 16.67 / 17.60, frame-rate capped) |
| Steady gameplay: avg ms / p99 ms | *phone* (editor: 16.80 / 18.41, frame-rate capped) |
| Worst case: avg ms / p99 ms | *phone* (editor: 17.16 / 17.85, frame-rate capped) |
| GC allocated per frame (bytes), allocating markers | 0 B from the sample's scripts in Steady and Worst; `FrameTimeSampler.Update` allocates one string every 600 frames (editor Profiler) |
| Peak Total Reserved (MB) / TOTAL PSS (MB) | *phone* |
| Worst case: SetPass / batches / triangles | 14 / 1,506 / 10,895 (editor Stats; 1,500 of the batches go through the SRP Batcher, which is why SetPass stays at 14) |
| Cold start ms (median of 3), first interactive s | *phone* |
| APK size (MB) | *phone* |
| Thermal delta, throttling (Week 7) | |
| Load time (Week 5) | |
