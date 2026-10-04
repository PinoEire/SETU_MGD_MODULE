# SETU_MGD_MODULE

Sample scenes and scripts for the **Mobile Game Development (A12581)** module at SETU, academic year 2026/2027. Lecturer: **Pino De Francesco**. This is the lecturer's teaching project, not a game and not one of the student project options: each sample shows one mobile good practice from the lab schedule (safe areas, lifecycle, frame pacing, Awaitable-based loading, pooling, telemetry, adaptive performance, and so on), small enough to read in one sitting and copy into your own project.

## Requirements

- Unity **6000.6** (the current Unity 6.x release) with Android Build Support.
- An Android phone with USB debugging enabled. Builds are release-signed APKs, IL2CPP, ARM64, sideloaded with `adb install -r`. No store consoles are used.
- The release keystore lives in `Keystore/` and is not in the repository. Set your own in Project Settings > Player > Android > Publishing Settings if you build your own copy.

## Where things are

| Path | What it is |
|------|------------|
| `MGD_MODULE_GDD/` | The design document, as an Obsidian vault. **Start here.** Open the folder as a vault and read `Home.md`, or read the Markdown directly on GitHub. |
| `Assets/_Game/` | Everything this project owns, in standard type folders. Each sample is a same-named subfolder inside the folders it needs, for example `Scripts/SafeArea/` and `Scenes/SafeArea/`. |
| `Assets/_Game/Scripts/<Sample>/README.md` | A two-paragraph summary of that sample for anyone who opens the folder without the vault. |
| Everything else under `Assets/` | Template content, packages and imported assets. Not edited in place. |

## The vault

`MGD_MODULE_GDD` doubles as a worked example of a design document that people and AI tools can both read and maintain. `Home.md` is the only full index, `Conventions.md` explains the rules and why they exist, `Samples/` has one note per sample with a fixed set of headings, and `Decisions/` holds append-only decision records. If you want to know what a sample does, which lab it supports, how to test it on a phone, or why something is the way it is, the answer is in the vault, not in this file.

## Curriculum

Lab sheets, decks and assessment briefs are distributed through Moodle. Each sample note names the lab it supports.

## Signing

The release keystore is `Keystore/user.keystore`, alias `android`. It sits inside the repo folder (the lab sheet has students move theirs out) so that the Player Settings path stays project-relative on every machine; the `Keystore/` folder, `*.keystore` and `*.jks` are git-ignored, so the file is never in the repository; a backup lives outside the repo in the password manager. Both passwords are in the password manager only and are typed into Project Settings > Player > Android > Publishing Settings for the editor session; Unity does not save them. Certificate details (from `keytool -list -v -keystore Keystore/user.keystore -alias android`, run from Unity's OpenJDK `bin` folder):

| Field | Value |
|-------|-------|
| Alias | android |
| Valid from / until | left blank on purpose: keytool needs the keystore password, so this is filled by hand, never by a script or an AI session |
| SHA-256 fingerprint | left blank for the same reason |

Lose the keystore and no future build can replace an installed one with the same package name.

## Build

1. Clone the repository and open the folder in Unity 6000.6 (Unity Hub > Add). Android Build Support must be installed with the editor.
2. File > Build Profiles: select the **Android** profile and make it active.
3. Project Settings > Player > Android, check: Package Name `com.dftgames.mgdsamples`, Version `0.2.0`, Scripting Backend **IL2CPP**, Target Architectures **ARM64** only (smaller APK; every supported phone is 64-bit), Target API Level **Automatic**, Publishing Settings **Custom Keystore** selected with both passwords typed in. In the Build Profiles window, Platform Settings: **Build App Bundle** off.
4. Commit first, so the manifest row records the commit the APK was built from. Then menu **MGD Samples > Build Release APK**. The script (`Assets/_Game/Editor/Release/ReleaseBuild.cs`) checks the package name, IL2CPP, ARM64 only, Target API Automatic, App Bundle off, the custom keystore, that its file exists, and both passwords, and refuses with one console line per unmet item; it does not check the version string. Otherwise it bumps Bundle Version Code, builds `releases/SETU_MGD_MODULE-<version>-<code>-arm64.apk` and appends a row to `releases/manifest.md` (a commit marked `-dirty` means the tree had uncommitted changes at build time). The APK is git-ignored; the manifest is committed.
5. Install: `adb install -r releases/SETU_MGD_MODULE-0.2.0-<code>-arm64.apk`, then `adb shell dumpsys package com.dftgames.mgdsamples | grep version` should show the versionCode from the manifest. "App not installed" or `INSTALL_FAILED_VERSION_DOWNGRADE` means the phone has a higher code: build again.
6. Publish: commit the manifest row, tag it (`git tag v0.2.0`, then `git push --tags`), create a GitHub Release on that tag with the APK attached, paste the link into the manifest's Download column and commit again. See the vault decision *2026-09-28 Release APKs live on GitHub Releases*.

Batch-mode builds are not supported, because the keystore passwords exist only in the editor session.

## Device targets

- Minimum API level 26 (Android 8.0), Unity's default for 6000.6. Target API level Automatic (highest installed SDK), as Play's yearly target-API policy requires: https://support.google.com/googleplay/android-developer/answer/11926878
- ARM64 only. 64-bit is a Play requirement and every supported phone has it.
- Tested devices are listed in `docs/device-matrix.md`.

## AI assistance

The samples, scene builders, tests and documentation in this project are written with Claude Code, reviewed and tested on device by the lecturer. Students must declare their own AI use in their README, as the CA handouts ask.
