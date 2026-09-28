---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-09-28
tags: [decision, android, permissions, release]
---

# Release builds strip the INTERNET permission

## Context

The first release build of 0.2.0 requested `android.permission.INTERNET` although Internet Access is Auto, no code uses the network, and Analytics, Crash Reporting, Engine Diagnostics and hardware statistics are all off. Removing the built-in Unity Analytics package module changed nothing: Unity's own `unityLibrary` manifest adds the permission and the engine's analytics common module is kept in every build (reproduced four times in 6000.6.0f1). The data-use statement says "no network"; Week 4 Lab B tells students that when the statement and `dumpsys` disagree, one of them must change. A custom launcher manifest in `Assets/Plugins/Android` would remove it, but from every build, and Development builds need the permission for the Profiler's Wi-Fi autoconnect that Week 3 relies on.

## Decision

`Editor/Release/StripInternetPermission.cs` implements `IPostGenerateGradleAndroidProject` and deletes the `INTERNET` line from the generated `unityLibrary` manifest for non-development builds only; `IPreprocessBuildWithReport` on the same class records whether the build is a development build. The privacy statement stays "no network" and the release build proves it with `aapt dump badging`. Nothing lives in `Assets/Plugins`.

## Consequences

- The release APK requests only `VIBRATE` (from `Handheld.Vibrate`) and the app's own not-exported receiver permission; development builds still request `INTERNET` and profile over Wi-Fi as before.
- Any future sample that needs the network deletes the hook and updates the statement in the same commit; the [[ReleasePipeline]] note records the check.
- Students who see `INTERNET` in their own `dumpsys` output have a worked example of the fix and of the reason not to apply it to every build.
- If a Unity upgrade changes the manifest line's shape, the hook logs "nothing to remove" and `aapt` shows the permission again; `StripInternetPermissionTests` pins the current shape.
