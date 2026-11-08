# Release checklist

Worked example of the Week 12 Lab A checklist (Parts A and D) for this project. Submit yours as `docs/QA/release-checklist.md`, as the lab sheet names it (this project keeps its worked examples in `docs/qa/`). Values are read from Player Settings, the Android build profile and the phone, not from memory.

Build: 0.8.0, versionCode 23, build ID `74ce07b5`, made with *MGD Samples > Build Release APK* from commit `2d212c0`. APK: `releases/SETU_MGD_MODULE-0.8.0-23-arm64.apk`, 46,445,112 bytes (44.3 MB). Device: Google Pixel 7a, Android 17 (API 37). Date: 2026-11-08.

## Part A: the seven points

| # | Point | Value seen | Tick |
|---|-------|------------|------|
| 1 | versionCode bumped, versionName set | versionCode 23 (22 was the last installed), versionName 0.8.0 | yes |
| 2 | Release keystore | Custom Keystore, the same file since CA1, backed up outside the repo, passwords typed per session and never committed; the release step refuses to build without them | yes |
| 3 | No Development Build | Development Build, Autoconnect Profiler, Deep Profiling and Script Debugging off in the Android profile; `DEBUGGABLE` absent on the phone (below); the StoreShot button (the lab's debug-shot button) is not created outside Development Builds. `FrameTimeSampler` stays on purpose: it is what the Performance sample demonstrates, and it lives only in that scene | yes |
| 4 | No Quit or Exit button | None in any scene; Android back returns to the Launcher, and from the Launcher the system handles leaving | yes |
| 5 | About / Build Info screen | Reachable from the Launcher; 0.8.0 shows version, build ID, device and Android version. The Week 12 build date and commit line came after 0.8.0: it was checked on a release-settings test build from a later commit, `3669742` (`Built 2026-11-08 from 3669742`) and ships in the next release | yes |
| 6 | Permissions minimal | Two, both explained below; no `INTERNET` (stripped from release builds) | yes |
| 7 | Target API current | Target API Level: Automatic (highest installed), which built as targetSdk 36; Minimum API Level 26. Play floor checked 2026-11-08 on the target API policy page: new apps and updates must target API 36 since 31 August 2026 (extensions ended 1 November 2026), so 36 meets it | yes |

## Part D: what actually shipped

Commands from the lab sheet, run with `adb -s 41271JEHN14834` against the Pixel 7a right after installing the 0.8.0 release APK (package `com.dftgames.mgdsamples`):

| Check | Result |
|-------|--------|
| `dumpsys package ... \| grep version` | `versionCode=23 minSdk=26 targetSdk=36`, `versionName=0.8.0` |
| Requested permissions | `android.permission.VIBRATE` (the Accessibility sample's haptic pulse) and `com.dftgames.mgdsamples.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION` (app-private, declared by an Android library for its own receivers); see `docs/privacy-statement.md` |
| `DEBUGGABLE` | Absent; `pkgFlags=[ HAS_CODE ALLOW_CLEAR_USER_DATA ALLOW_BACKUP KILL_AFTER_RESTORE ]` |
| Cold start, `am start -W` after `am force-stop`, three runs | `LaunchState: COLD`, `TotalTime` 134, 132 and 129 ms |
| Activity | `com.dftgames.mgdsamples/com.unity3d.player.UnityPlayerGameActivity` (from `cmd package resolve-activity --brief`) |

`TotalTime` is Android's time to the activity's first frame. It is not the time until the Launcher is usable, which also includes Unity's start-up and the first scene load.

Second phone: not run here (one test device). The lab installs on a neighbour's phone with a different Android version; record its model, Android version, versionCode and `TotalTime` in a second row.
