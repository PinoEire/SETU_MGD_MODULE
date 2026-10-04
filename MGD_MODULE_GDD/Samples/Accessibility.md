---
type: sample
status: done
lab: W02-B
unity: 6000.6.0f1
scene: Assets/_Game/Scenes/Accessibility/Accessibility.unity
updated: 2026-10-04
tags: [sample, accessibility, haptics, ui, w02]
---

# Accessibility

## Goal

Show the three player settings every mobile game should ship with from the first build: a haptics toggle, a text-size choice and a reduce-motion toggle, all in PlayerPrefs, on one Settings card, with a demo area that makes each one visible. Alongside it, a worked example of the ten-check accessibility pass in the format students submit.

## Lab it supports

Week 2 Lab B, Parts B (haptics), C (text size) and D (accessibility pass): *Lifecycle, back, accessibility and scope lock*. Part A is the [[Lifecycle]] sample.

## What the student learns

- One haptic pulse from one meaningful event, behind a setting, never from `Update`. `Handheld.Vibrate` has no duration or intensity control and is a no-op outside Android and iOS.
- Text size is a multiplier on each label's own base size, applied by a component on every readable text with Auto Size off. Overflow is fixed with an ellipsis on single-line labels (`Overflow: Ellipsis`, wrapping off) and wrapping on paragraphs, never by shrinking the text back.
- Body text at Normal must stay above about 14 sp (the lab sheet's floor); the sample's 42 canvas units are about 16 sp on a 1080-wide 420 dpi phone.
- The reduce-motion toggle exists before there is any motion to reduce. The three settings share one shape: the value is read from PlayerPrefs once and cached, so effects can check it every frame for free, and the setter raises a `Changed` event only when the value really changes.
- Initialise controls with `SetIsOnWithoutNotify`, or the panel writes the saved value straight back to disk on load.
- Every state must survive monochrome, silence and haptics off: report in words or shapes, never colour alone.
- 48 dp is the minimum tap target and it includes padding: a 64 unit checkbox is only 24 dp, so the whole toggle row is the target.
- 1 dp is 1 px at 160 dpi; the demo prints the maths for the phone it runs on.

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/Accessibility/Haptics.cs` | Static. `Enabled` cached from PlayerPrefs with a `Changed` event, `Pulse()` vibrates once when enabled. What students copy. |
| `Assets/_Game/Scripts/Accessibility/TextScale.cs` | On every readable text. Static `Factor` (Small 0.85, Normal 1, Large 1.25) with a `Changed` event; each instance re-applies its base size times the factor. What students copy. |
| `Assets/_Game/Scripts/Accessibility/MotionSetting.cs` | Static `ReduceMotion` flag cached from PlayerPrefs with a `Changed` event. What students copy. |
| `Assets/_Game/Scripts/Accessibility/SettingsPanel.cs` | Binds two toggles and three buttons to the settings, loads saved values on Start, disables the current size button. What students copy. |
| `Assets/_Game/Scripts/Accessibility/AccessibilityDemoHud.cs` | Demo only. Hit button reporting in words, shaking square, 48 dp reference square sized from `Screen.dpi` through the tested `DpToPixels`, current text-size label. |
| `Assets/_Game/Scripts/Accessibility/README.md` | Two-paragraph summary for students who open the folder without this vault. |
| `Assets/_Game/Editor/Accessibility/AccessibilitySceneBuilder.cs` | Menu item *MGD Samples > Build Accessibility Scene*. Builds the scene, attaches `TextScale` to every text with Auto Size off, sets labels to no-wrap with an ellipsis and the body paragraph to wrap, adds the scene to Build Settings. |
| `Assets/_Game/Editor/Shared/SampleSceneBuild.cs` | Shared builder helpers; `CreateToggle` is a TextMeshPro toggle resized so the whole row is a 48 dp tap target. |
| `Assets/_Game/Editor/Tests/AccessibilitySettingsTests.cs` | Six EditMode tests: the two toggles raise `Changed` once and write through to PlayerPrefs, `TextScale` clamps and stays quiet when unchanged, and the dp-to-pixel maths with its 160 dpi fallback. |
| `Assets/_Game/Scenes/Accessibility/Accessibility.unity` | Demo area (status, Hit, motion and 48 dp squares, body paragraph, size label) above a Settings card (Haptics, Reduce motion, Small / Normal / Large). Generated; not hand-edited. |
| `Assets/_Game/Scripts/Shared/BackToLauncher.cs` | On the Canvas with the listener on. Not part of what students copy. |
| `Assets/_Game/Scripts/Shared/LifecycleGuard.cs` | Shared, on the `Lifecycle Guard` object every scene gets through `SampleSceneBuild.AddPauseMenu`: pauses on focus loss and Home. Belongs to [[Lifecycle]]. |
| `Assets/_Game/Scripts/Shared/PauseMenu.cs` | Shared, on the Canvas with the hidden `Pause Panel`: shows the panel and handles Resume. Its back toggle is off here because `BackToLauncher` owns the gesture. Belongs to [[Lifecycle]]. |
| `docs/accessibility-pass.md` | The ten-check pass run on this scene, in the two-column format students submit for CA1. |

## How to test

1. **Editor:** Play. Tap Hit: the status reads "HIT 1 (haptic pulse sent)"; no buzz in the editor. Turn Haptics off: the status wording changes on the next tap. Turn Reduce motion on: the orange square stops at once. Tap Large: every label grows by a quarter, the paragraph wraps, nothing is cut off; the Large button greys out. Stop and Play again: all three settings come back as set.
2. **Phone:** the same, plus one short buzz per Hit with Haptics on and none with it off. Compare your own buttons against the blue 48 dp square. Set Android *Display size and text* to maximum and check the card still fits inside the margin.
3. **Monochromacy:** Developer options > Simulate colour space > Monochromacy. Every state is still readable because none of it relies on colour alone.
4. **Tests:** Window > General > Test Runner, EditMode, run all: the `AccessibilitySettings` tests pass.

## Known limits

- `Handheld.Vibrate` is the only haptic API the lab uses: one fixed pulse. Rich haptics need a native plugin, which the module rules out.
- The Settings card is not a shared prefab. Each sample that needs settings builds its own; a shared prefab would couple samples through a scene asset.
- Match Width Or Height at 0.5 makes the canvas narrower in units on a 20:9 phone (about 966 wide instead of 1080), so three buttons across a stretched card must be sized for that width, not the 16:9 reference.
- The shaking square looks tappable and is not; noted as a cut in the accessibility pass.
- Verified 2026-09-16: editor Play mode (all three settings, persistence across restart, Large layout) and haptics on an Android device. Cached settings with `Changed` events and the label ellipsis added 2026-10-04; the same day the *Text size* caption's box grew to 60 units (TextMeshPro blanks an Ellipsis label whose line does not fit vertically, and 36 pt at Large is 45 pt, about 51 units) and the three size buttons shrank to 220 units so they keep a gap on a 20:9 phone, where the card is only about 774 units wide. Checked on a Pixel 7a. The shared pause card's labels scale too (its title box is 110 units for the same reason).

## Cuts list

1. The 48 dp square and its maths label could go, leaving the Device Simulator as the only measure. Kept because it puts the number on the phone in the student's hand.
2. The text-size label could go; the size buttons already show the state. Kept because it prints the factor students must quote.

## Decisions

- [[2026-09-15 Back navigation returns to the launcher]]
- [[2026-10-04 Every scene pauses on focus loss]]

## References

- Lab sheet: `Week 02/Lab B/W02-LabB-LabSheet.md` in the curriculum folder (Parts B, C, D, Troubleshooting).
- Unity docs: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Handheld.Vibrate.html and https://docs.unity3d.com/6000.6/Documentation/ScriptReference/PlayerPrefs.html
- Android touch target size: https://support.google.com/accessibility/android/answer/7101858
- Android accessibility principles: https://developer.android.com/guide/topics/ui/accessibility/principles
