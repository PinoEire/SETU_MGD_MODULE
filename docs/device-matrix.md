# Device matrix

Worked example of the Week 4 Lab A, Part D matrix. Submit yours as `/docs/CA1/device-matrix.md` with at least two rows: your phone and a neighbour's. One row per phone the release APK was installed on, with the versionCode `dumpsys` reported and anything odd you saw in a minute of play.

| Device | Android | Serial (last 4) | Install result | Notes |
|--------|---------|-----------------|----------------|-------|
| Google Pixel 7a (lecturer) | 17 | 4834 | Success, versionCode 10 | Punch-hole camera at the top: SafeArea HUD moves 118 px down. 20:9 panel, 60 Hz in-game although 90 Hz exists. No install prompt beyond the USB debugging dialog. |
| (second phone: fill in from a neighbour's device) | | | | |

Fill a row from:

```bash
adb devices -l
adb -s <serial> install -r releases/SETU_MGD_MODULE-0.2.0-<code>-arm64.apk
adb -s <serial> shell dumpsys package com.dftgames.mgdsamples | grep version
adb -s <serial> shell getprop ro.product.model
adb -s <serial> shell getprop ro.build.version.release
```

Things worth a note: a different notch or punch-hole moving the SafeArea HUD, a 120 Hz panel still reporting 60 Hz in the Performance scene, a brand that needs *Install via USB* in Developer options, a lower-end phone where the Worst state visibly stutters.
