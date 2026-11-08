---
type: sample
status: done
lab: [W10-A, W10-B]
unity: 6000.6.8f1
scene: none
updated: 2026-11-08
tags: [sample, store, screenshots, w10]
---

# Store

## Goal

Show students how to capture store screenshots from their own build and turn them into a store kit that meets Play's specifications: an icon that reads at 48 px, a [[Glossary#Feature graphic|feature graphic]], screenshots at a ratio Play accepts, and honest short and long descriptions, with the A/B pairs for the Lab B critique set up.

## Lab it supports

Week 10 Lab A, *Store presence that converts*: Part C (`StoreShot` and a capture button in Development Builds only), and Parts A, B, D and E as worked examples in `docs/store/`. Week 10 Lab B, *A/B critique*: the three pairs and the recording file, `docs/store/critique-notes.md`.

## What the student learns

- A store screenshot is captured from the build, at device resolution, with whatever must not be seen (the HUD in a game, a debug button) hidden for exactly the captured frame.
- `ScreenCapture.CaptureScreenshot` writes at the end of the frame and saves after it; wait for the file, in real time, before showing the hidden things again.
- A Development Build stamps a watermark into every capture, and a modern 20:9 phone screen is outside Play's rule that the long side is at most twice the short one; both have to be dealt with before the files meet the spec.
- The specs and the design guidance live on Play's help page and change: check them on the day, write the date down, and follow the page where a lab sheet or a deck says otherwise.
- A feature graphic shows the experience at its centre: the focal point towards the centre, background towards the edges, vibrant colour, not the icon again, no device frame.
- For Play's promotion formats a game needs at least three 16:9 landscape shots (1920x1080 or more) or three 9:16 portrait shots (1080x1920 or more); an app needs at least four at 1080 px or more.
- A listing describes only what the build contains; a feature hidden in release builds does not go in it.
- An A/B pair changes one variable, so the vote says something about that variable.

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/Store/StoreShot.cs` | Hides `hideWhileCapturing`, waits a frame, deletes an older file of the same name, captures `Scene_NN_WxH.png` into `persistentDataPath` (a full path in the editor, a bare name on the phone, where Unity adds the folder), waits up to 3 s in real time until the file's size stops changing, shows them again, logs `[Shot] Saved`. A DontDestroyOnLoad singleton; its Shot button is active only when `Debug.isDebugBuild`. `FileName` is the pure part. What students copy. |
| `Assets/_Game/Scripts/Store/README.md` | Two-paragraph summary for students who open the folder without this vault. |
| `Assets/_Game/Editor/Launcher/LauncherSceneBuilder.cs` | Creates the `Store Shot` object: its own canvas (sorting order 90, above every sample, below the loading screen) with a 140-unit Shot button on the right edge a quarter of the way down. Belongs to [[Launcher]]. |
| `Assets/_Game/Editor/Tests/StoreShotTests.cs` | Two EditMode tests for the file name. |
| `docs/store/asset-checklist.md` | Every file against the spec checked on 8 Nov 2026, and how the watermark and the 20:9 screen were dealt with. |
| `docs/store/icon-512.png`, `icon-512-A.png`, `icon-512-B.png`, `icon-48-A.png`, `icon-48-B.png` | Flat stand-in icons drawn by a script: one silhouette, A and B differing only in background colour, `icon-512.png` being A under the Lab A name. |
| `docs/store/feature-1024x500.png` | Built by a script to the Play help page: the app name and the Economy round drawn large at the centre, a vibrant gradient, decoration only in the corners, no copy of the icon, no device frame. |
| `docs/store/screenshots/` | Five 1080x1920 captures from a Pixel 7a: Economy mid-round, Tiers, the thermal overlay, the sample list, About. |
| `docs/store/descriptions.md` | Short A (77 characters), short B (76) and the long description for 0.6.0. |
| `docs/store/critique-notes.md` | The Lab B recording file with the three pairs set up and the votes left for the session. |

## How to test

1. **Editor:** Play the Launcher: the Shot button is on the right edge (the editor counts as a development build). Tap it: `[Shot] Saved .../Launcher_00_<w>x<h>.png`, the PNG does not show the button, and the button comes back. Stop, Play again and tap again: the same file name is written afresh, still without the button.
2. **Phone:** Development Build, install, open any sample, tap Shot; `adb logcat -s Unity | findstr /L "[Shot]"` gives the path; `adb pull` it. Check the size, and the watermark in the bottom-right corner.
3. **Release build:** no Shot button anywhere.
4. **EditMode tests:** *MGD Samples > Run EditMode Tests*: 242 passed.

## Known limits

- The Shot button sits over the right edge of every screen in a Development Build and can cover part of a sample's text while it is shown (Economy's status line, for example); it is hidden in the capture itself.
- The art is a stand-in, not designed key art.
- The worked screenshots were painted and padded after capture. A game captured in landscape still needs the watermark removed, and is cropped from 20:9 (2400x1080) to 16:9 (1920x1080) rather than padded.
- Verified 2026-11-08 on a Pixel 7a (Development Build of 0.6.0, the five screenshots) with the first version of `StoreShot`; the delete-then-wait fix that followed was verified in editor Play mode on 6000.6.8f1 (a second session's capture under the same name is rewritten without the Shot button).

## Cuts list

1. The B icon and the 48 px previews: Lab A needs one icon; kept because Lab B's first round needs a pair.
2. Real-time waiting for the file: the deck waits 0.5 s; kept because a paused game's 0.5 s never ends.

## Decisions

- [[2026-11-08 StoreShot is persistent and its button exists in Development Builds only]]
- [[2026-10-04 Shared scripts are the ones every scene needs]]

## References

- Lab sheets: `Week 10/Lab A/W10-LabA-LabSheet.md` and `Week 10/Lab B/W10-LabB-LabSheet.md` in the curriculum folder, and the deck's `StoreShot` slide.
- Play Console, *Add preview assets*: https://support.google.com/googleplay/android-developer/answer/9866151
- `ScreenCapture.CaptureScreenshot`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/ScreenCapture.CaptureScreenshot.html
