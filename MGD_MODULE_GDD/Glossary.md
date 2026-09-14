---
type: glossary
status: living
updated: 2026-09-15
tags: [glossary]
---

# Glossary

Module vocabulary, one line each. Link a term with `[[Glossary#Term]]` the first time it appears in a note.

## ANR
Application Not Responding. Android shows a dialog when the main thread is blocked for about five seconds. Visible in Logcat; a CA3 QA-pack item.

## APK
Android Package. The single-file installable used for sideloading in this module. Distinct from an AAB (App Bundle), which is only for store upload.

## ARM64
The 64-bit ARM architecture. Required for modern Android distribution; only selectable in Unity once the scripting backend is IL2CPP.

## Awaitable
Unity's built-in async type (`UnityEngine.Awaitable`) for awaiting frames, seconds, async operations and background threads without coroutines.

## Cuts list
An ordered list of features to drop first if time runs out. Every sample and every student project keeps one.

## dp
Density-independent pixel. One dp is one pixel on a 160 dpi screen; convert with `Screen.dpi / 160f`. Touch targets should be at least 48 dp.

## Frame pacing
Delivering frames at a steady interval rather than as fast as possible. Optimized Frame Pacing in Player Settings plus a sensible `Application.targetFrameRate`.

## Gesture bar
The bottom strip Android reserves for the home swipe on gesture-navigation phones. Part of the area excluded by `Screen.safeArea`.

## IL2CPP
Unity's ahead-of-time scripting backend that converts IL to C++. Required for ARM64 and for release builds in this module.

## Keystore
The file holding the private key that signs a release APK. Kept outside version control; losing it prevents updating an installed app.

## Logcat
Android's system log. Read through the Android Logcat package in the editor or `adb logcat` on the command line.

## MDA
Mechanics, Dynamics, Aesthetics (Hunicke, LeBlanc, Zubek 2004). The one-page design framing students submit in Week 2.

## Safe area
The part of the screen not covered by a notch, camera cut-out, rounded corner or gesture bar. Unity exposes it as `Screen.safeArea` in pixels.

## Sideload
Installing an APK directly on a device with `adb install -r`, bypassing any store.

## Telemetry stub
Local `Debug.Log` events (`session_start`, `level_start`, and so on) that stand in for an analytics SDK.

## versionCode
The integer Android uses to order builds. Must increase on every install over an existing build; the human-readable version string does not matter to the installer.
