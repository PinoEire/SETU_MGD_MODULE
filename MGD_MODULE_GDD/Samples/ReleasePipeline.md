---
type: sample
status: done
lab: [W04-A, W04-B]
unity: 6000.6.0f1
scene: none
updated: 2026-09-28
tags: [sample, release, signing, versioning, w04]
---

# ReleasePipeline

## Goal

Show the Week 4 release pipeline as one menu item that students can read: a checklist that refuses to build until the Player Settings are right, a versionCode bump on every build, a release-signed IL2CPP ARM64 APK in `releases/`, and a manifest row with the SHA-256 so anyone can verify the file they downloaded. Alongside it, the README sections and the four worked-example docs the CA1 hand-in asks for.

## Lab it supports

Week 4 Lab A, *Harden the release pipeline: signing and versioning*, Parts A to E (identification, keystore documented without secrets, release build with install proof, device matrix, README build steps). Week 4 Lab B, *Store-awareness assets and the CA1 submission*, Parts A, B and D (store-asset checklist, descriptions, data-use statement, package layout) through the docs.

## What the student learns

- The package name is permanent and must be lower case `com.<name>.<title>`; Unity's default derived from the company name fails the checklist on purpose.
- versionCode is an integer Android compares; bump it on every build or the install fails with `INSTALL_FAILED_VERSION_DOWNGRADE`. versionName is for people.
- Keystore passwords are session-only in Unity: the script reads them from Player Settings and refuses when they are empty, which is also why it cannot run from batch mode.
- A checklist in code beats a checklist in a document: `ReleaseChecklist.Check` names the Player Settings page for each failure, and it is unit tested.
- A release is a file plus a record: size, SHA-256 and commit in `releases/manifest.md`, the APK on the GitHub Release, never in git (see the decision below).
- The data-use statement is written from evidence (`grep` the code, `dumpsys` the build), not from intentions.

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Editor/Release/ReleaseChecklist.cs` | `ReleaseSettings` (a plain snapshot of the Android Player Settings), `ReleaseChecklist.Check` (one rule per line, one message per failure) and `ReleaseManifest` (row format and append). What students copy. |
| `Assets/_Game/Editor/Release/ReleaseBuild.cs` | Menu item *MGD Samples > Build Release APK*: checklist, bump, build with the Android build profile, revert the bump on failure, SHA-256 and manifest row on success. What students copy. |
| `Assets/_Game/Editor/Tests/ReleaseChecklistTests.cs` | Eighteen EditMode tests: clean settings pass; each rule fails alone (Mono, ARMv7 ticked, fixed Target API, both passwords, no custom keystore, App Bundle on); six bad and two good package names; the manifest row format; append on a fresh file and on a file without a trailing newline. |
| `releases/manifest.md` | One row per release build. Committed. |
| `releases/*.apk` | Git-ignored (with the IL2CPP symbols folder Unity writes beside it). Attached to the GitHub Release instead. |
| `README.md` | Signing, Build, Device targets and AI assistance sections. |
| `docs/store-assets-checklist.md`, `docs/descriptions.md` | Lab B Part A worked examples for this app. |
| `docs/privacy-statement.md` | Lab B Part B, written from the code: PlayerPrefs keys, no network, `VIBRATE` expected; the `dumpsys` block is pasted after the device run. |
| `docs/device-matrix.md`, `docs/install-proof.txt` | Lab A Parts C and D structure; the phone rows are filled from the device. |
| `ProjectSettings/ProjectSettings.asset` | Package name `com.dftgames.mgdsamples`, version 0.2.0, Bundle Version Code bumped by the script (7 after the builds it took to get the permissions right); hardware statistics upload off. |
| `ProjectSettings/UnityConnectSettings.asset` | Engine Diagnostics off, so no engine data leaves the phone. |
| `Assets/_Game/Editor/Release/StripInternetPermission.cs` | Build hook that deletes the `INTERNET` line from the generated `unityLibrary` manifest in release builds only; development builds keep it for the Profiler. `Strip` is pure and tested. See the decision below. |
| `Assets/_Game/Editor/Tests/StripInternetPermissionTests.cs` | Four EditMode tests: only the `INTERNET` line goes, the line count drops by one, a manifest without it is unchanged, spacing and self-closing variants are matched. |

## How to test

1. **Refusal:** in Build Profiles > Android > Platform Settings turn *Build App Bundle* on (or leave a keystore password empty in Player Settings) and run *MGD Samples > Build Release APK*. Expected: one red console line per problem naming the settings page, then "Not built"; Bundle Version Code unchanged. Turn the setting back.
2. **Build:** with both passwords typed in, run the menu item. Expected: `releases/SETU_MGD_MODULE-0.2.0-arm64.apk` appears, Bundle Version Code is one higher, and `releases/manifest.md` gains a row with size, SHA-256 and the current commit. `aapt dump badging` on the APK (in Unity's SDK `build-tools`) shows the package name, versionCode, `native-code: 'arm64-v8a'`, and the permissions `VIBRATE` and the app's own `DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION`, and no `INTERNET`.
3. **Phone:** `adb install -r` the APK, then `adb shell dumpsys package com.dftgames.mgdsamples | grep version` shows the same versionCode. Paste both outputs into `docs/install-proof.txt`, add the row to `docs/device-matrix.md`, and the permission block to `docs/privacy-statement.md`.
4. **Tests:** Window > General > Test Runner, EditMode, run all: 28 tests pass (22 here across the checklist, manifest and hook, six from [[Performance]]).

## Known limits

- Editor only: the passwords live in the editor session, so there is no batch-mode or CI build. A CI build would read them from secrets and set `PlayerSettings.Android.keystorePass` before calling the same code.
- The script builds whatever the Android build profile says; it checks the Player Settings checklist, not the profile's own Development Build flag, which is off in the committed profile.
- Uploading the APK to the GitHub Release and pasting the link is manual (no GitHub CLI on this machine).
- The keytool validity and fingerprint lines in the README are filled by hand because keytool needs the keystore password.
- Unity requests `INTERNET` in every Android build whatever the settings say; the build hook removes it from release builds only, so a Development build (Week 3 profiling) still shows it in `dumpsys`. Turning off Engine Diagnostics and hardware statistics was still right (both send data) but did not remove the permission on its own, and neither did removing the built-in Analytics package module.
- The manifest row for a build made from an uncommitted tree shows the commit as `<hash>-dirty`; commit first when the row is meant as evidence, which the README's Build section says.
- Verified 2026-09-28: refusal path (App Bundle on) and six real builds from the editor (versionCodes 2 to 7, 42.8 MB); APK badging checked with aapt after each, `INTERNET` gone at 7 with the build hook. Phone install proof pending.

## Cuts list

1. The manifest could be a plain list without SHA-256; kept because the hash is how a marker verifies the file matches the tag.
2. The refusal for the default package name could be dropped; kept because it is the one mistake that is permanent.

## Decisions

- [[2026-09-28 Release APKs live on GitHub Releases]]
- [[2026-09-28 Release builds strip the INTERNET permission]]

## References

- Lab sheets: `Week 04/Lab A/W04-LabA-LabSheet.md` and `Week 04/Lab B/W04-LabB-LabSheet.md` in the curriculum folder.
- Unity docs: https://docs.unity3d.com/6000.6/Documentation/Manual/class-PlayerSettingsAndroid.html, https://docs.unity3d.com/6000.6/Documentation/Manual/build-profiles.html, https://docs.unity3d.com/6000.6/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html
- Android: https://developer.android.com/studio/publish/versioning and https://developer.android.com/studio/publish/app-signing
- Play target API policy: https://support.google.com/googleplay/android-developer/answer/11926878
