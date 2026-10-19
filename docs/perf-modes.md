# Performance modes

Worked example of the Week 7 Lab A, Part E table for this project. Submit yours as `/docs/CA2/baseline/perf-modes.md`, with the two screenshots in `/docs/CA2/baseline/perf-modes/` and the logcat excerpt as `/docs/CA2/baseline/logcat-ap-events.txt`. The settings are `PerformanceMode.cs` (`Scripts/Shared/`); the toggle is on every scene's pause card. Measure in the Adaptive scene with the load on Hot, the phone unplugged, 30 s in each mode, read the average frame time from the overlay, and take the 99th percentile, which the overlay does not show, from Profile Analyzer on a 30 s Profiler capture (Development build with the Profiler attached, not the release APK). Phone cells stay empty until the device run; nothing is guessed.

| Mode | targetFrameRate | Render scale | Frame time avg (ms) | 99th (ms) | Bottleneck | Panel Hz |
|------|-----------------|--------------|---------------------|-----------|------------|----------|
| Balanced (default) | 60 | 1.0 | | | | |
| High FPS | 120 | 0.8 | | | | |

Device, build and conditions: (fill in: model, Android version, versionName / versionCode, Game Mode setting, display refresh setting).

## What to expect

- On a 60 Hz panel, or a 90/120 Hz phone that Android keeps at 60 for games, High FPS still reads about 16.7 ms: the request is clamped. The difference shows in the render scale (softer edges) and often in the bottleneck line.
- On a phone that grants 90 or 120 Hz, frame time drops towards 11.1 or 8.3 ms. If it does not, check the system Game Mode (Battery caps the frame rate) and the display's refresh setting.
- `perf_mode=` appears in logcat once per flip of the toggle; `[AP] bottleneck=` appears when the limiting factor changes (`TargetFrameRate` means the cap is the limit, which is the healthy state).

## Editor check (not a measurement)

2026-10-19, editor Play mode, Simulator provider: flipping the toggle logs `perf_mode=HighFps high=True target=120 scale=0.80`, the overlay reads `High FPS  120 fps  scale 0.80`, and the URP asset is back at 1.00 after Stop. The PC never leaves the frame cap, so its frame times say nothing about a phone.
