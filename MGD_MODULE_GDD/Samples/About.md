---
type: sample
status: done
lab: W08-A
unity: 6000.6.6f1
scene: Assets/_Game/Scenes/About/About.unity
updated: 2026-10-27
tags: [sample, about, build-stamp, distribution, w08]
---

# About

## Goal

Show the About / Build Info screen every submission needs, with a build stamp that names exactly one binary, so feedback from a closed test can be traced to the build the tester ran. The same ID is in the release manifest and the telemetry, and the screen carries the feedback-form link.

## Lab it supports

Week 8 Lab A, *Peer distribution: APK, QR code and feedback*: Part B (`BuildStamp.cs` on the About screen, the build ID in `session_start`), Part C.4 (the form link on the About screen), and the Part A and E documents as worked examples (`docs/qa/feedback-form.md`, the Week 8 section of `docs/device-matrix.md`, the Build ID column of `releases/manifest.md`).

## What the student learns

- `Application.version` is the human-readable version; `Application.buildGUID` changes with every build, so its first eight characters make a [[Glossary#Build ID|build ID]] a tester can quote and a release log can record.
- A stamp that also lists device model, Android version, RAM, resolution and panel refresh rate turns a feedback response into a row of the device matrix.
- The build ID belongs everywhere evidence is kept: the About screen, the `session_start` telemetry event, the release manifest next to the SHA-256.
- Opening a link with `Application.OpenURL` hands it to the browser; the game itself needs no `INTERNET` permission for a feedback button.
- There is no Quit button on an About screen either; Android back leaves it.

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/Shared/BuildStamp.cs` | Shared. On the About screen, writes four lines once in `Start` (`v<version> (<build ID>)`, device model, OS, `<RAM> MB RAM  <w>x<h> @ <Hz> Hz`) and logs `build=<version>/<id>`. Static `BuildId` (`ShortId(Application.buildGUID)`: first eight characters, `editor` for an empty or all-zero GUID, which is what Play mode reports), pure `Format` with invariant number formatting. Differs from the deck's version in three ways (static and tested, the `editor` case, invariant formatting); it lives in `Scripts/Shared/` because the bootstrap and the Launcher use the ID too. What students copy. |
| `Assets/_Game/Scripts/About/FeedbackLink.cs` | A serialised `url`: with a link, the screen shows `Feedback: <link>` and an *Open feedback form* button (`Application.OpenURL`), and warns in the console if the link has no `https://`, because Android finds no app for a link without a scheme; with none, a build hides both and the editor shows a reminder. Demo, but small enough to copy. |
| `Assets/_Game/Scripts/About/README.md` | Two-paragraph summary for students who open the folder without this vault. |
| `Assets/_Game/Scripts/Shared/MobileBootstrap.cs` | `session_start` gains `build`, the same ID. Belongs to [[Performance]]. |
| `Assets/_Game/Scripts/Launcher/LauncherMenu.cs` | The footer shows the build ID after the version. Belongs to [[Launcher]]. |
| `Assets/_Game/Editor/Release/ReleaseBuild.cs`, `ReleaseChecklist.cs` | The manifest row gains a last column, Build ID, from the build report's GUID, which is the built player's `Application.buildGUID`. Belongs to [[ReleasePipeline]]. |
| `Assets/_Game/Editor/About/AboutSceneBuilder.cs` | Menu item *MGD Samples > Build About Scene*: title, the `BuildLabel` stamp, a hint to quote the build ID, the feedback line and button (the link from its `FeedbackFormUrl` constant), a footer; `BackToLauncher` listening and the shared pause card. |
| `Assets/_Game/Editor/Tests/BuildStampTests.cs` | Eight EditMode tests: eight-character IDs, a short GUID, empty, null and all-zero GUIDs as `editor`, a real GUID that starts with zeros, the stamp layout, and the refresh rate rounded under a comma-decimal locale. `ReleaseChecklistTests` checks the manifest row's new column. |
| `Assets/_Game/Scenes/About/About.unity` | Generated; not hand-edited. |
| `releases/manifest.md` | Header gains the Build ID column; earlier rows end at Download. |
| `docs/qa/feedback-form.md`, `docs/device-matrix.md` | The Part C field list and the Part E tier table as worked examples. |

## How to test

1. **Editor:** Play the Launcher. The footer reads `SETU_MGD_MODULE <version> (editor)  |  Unity ...`. Tap About: four stamp lines with `(editor)`, this PC's model, OS, RAM and Game view size, the hint to quote the build ID, `Feedback: <the form link>` and the *Open feedback form* button.
2. **Editor, link:** tap *Open feedback form*: the browser opens the form. Stop Play, open the About scene, clear `url` on `FeedbackLink` and Play: the line becomes `Feedback: set the form link on FeedbackLink` with no button (a build hides both); then discard the change (do not save the scene).
3. **Release build:** *MGD Samples > Build Release APK*. The `[ReleaseBuild]` line names the build ID, and the new manifest row ends with it.
4. **Phone:** install it; About shows `v<version> (<id>)` with the same ID as the manifest row; `adb logcat -s Unity | findstr /L "build="` shows `build=<version>/<id>`, and the `session_start` telemetry line carries `build=<id>`. `adb shell dumpsys package com.dftgames.mgdsamples | findstr "versionName versionCode"` matches the stamp's version.
5. **EditMode tests:** *MGD Samples > Run EditMode Tests*: 179 passed.

## Known limits

- The form link is set in `AboutSceneBuilder` (`FeedbackFormUrl`, the class demo form); a new form means changing it there and rebuilding the scene, because the builder rewrites the scene.
- The stamp is written once in `Start`, so after a rotation the resolution line shows the orientation the screen opened in.
- `SystemInfo.systemMemorySize` reports what the OS gives the app, which on some phones is a little below the advertised RAM.
- Verified 2026-10-27 in editor Play mode; the manifest Build ID and the phone stamp are checked at the next release build.

## Cuts list

1. The *Open feedback form* button: the lab only asks for the link as text; kept because a tester cannot tap text on a phone.
2. The build ID in the Launcher footer: the About screen has it; kept so a screenshot of any visit to the Launcher identifies the build.

## Decisions

- [[2026-10-27 The build ID is the first eight characters of buildGUID, everywhere]]
- [[2026-09-15 Back navigation returns to the launcher]]
- [[2026-10-04 Every scene pauses on focus loss]]

## References

- Lab sheet: `Week 08/Lab A/W08-LabA-LabSheet.md` in the curriculum folder, and the deck's `BuildStamp` slide.
- `Application.buildGUID`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Application-buildGUID.html
- `SystemInfo`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/SystemInfo.html
- Alternative distribution on Android: https://developer.android.com/distribute/marketing-tools/alternative-distribution
