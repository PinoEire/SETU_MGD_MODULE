---
type: sample
status: done
lab: W02-B
unity: 6000.6.0f1
scene: Assets/_Game/Scenes/Accessibility/Accessibility.unity
updated: 2026-09-16
tags: [sample, accessibility, haptics, ui, w02]
---

# Accessibility

## Goal

Show the three player settings every mobile game should ship with from the first build: a haptics toggle, a text-size choice and a reduce-motion toggle, all in PlayerPrefs, on one Settings card, with a demo area that makes each one visible. Alongside it, a worked example of the ten-check accessibility pass in the format students submit.

## Lab it supports

Week 2 Lab B, Parts B (haptics), C (text size) and D (accessibility pass): *Lifecycle, back, accessibility and scope lock*. Part A is the [[Lifecycle]] sample.

## What the student learns

- One haptic pulse from one meaningful event, behind a setting, never from `Update`. `Handheld.Vibrate` has no duration or intensity control and is a no-op outside Android and iOS.
- Text size is a multiplier on each label's own base size, applied by a component on every readable text with Auto Size off. Overflow is fixed with ellipsis on labels and wrapping on paragraphs, not by shrinking the text back.
- A body size of about 16 sp at Normal is the floor. At the 1080-wide reference that is roughly 42 canvas units on a 420 dpi phone.
- The reduce-motion toggle exists before there is any motion to reduce; effects check it every frame so it takes effect at once.
- Initialise controls with `SetIsOnWithoutNotify`, or the panel writes the saved value straight back to disk on load.
- Every state must survive monochrome, silence and haptics off: report in words or shapes, never colour alone.
- 48 dp is the minimum tap target and it includes padding: a 64 unit checkbox is only 24 dp, so the whole toggle row is the target.
- 1 dp is 1 px at 160 dpi; the demo prints the maths for the phone it runs on.

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/Accessibility/Haptics.cs` | Static. `Enabled` in PlayerPrefs, `Pulse()` vibrates once when enabled. What students copy. |
| `Assets/_Game/Scripts/Accessibility/TextScale.cs` | On every readable text. Static `Factor` (Small 0.85, Normal 1, Large 1.25) with a `Changed` event; each instance re-applies its base size times the factor. What students copy. |
| `Assets/_Game/Scripts/Accessibility/MotionSetting.cs` | Static `ReduceMotion` flag in PlayerPrefs. What students copy. |
| `Assets/_Game/Scripts/Accessibility/SettingsPanel.cs` | Binds two toggles and three buttons to the settings, loads saved values on Start, disables the current size button. What students copy. |
| `Assets/_Game/Scripts/Accessibility/AccessibilityDemoHud.cs` | Demo only. Hit button reporting in words, shaking square, 48 dp reference square sized from `Screen.dpi`, current text-size label. |
| `Assets/_Game/Scripts/Accessibility/README.md` | Two-paragraph summary for students who open the folder without this vault. |
| `Assets/_Game/Editor/Accessibility/AccessibilitySceneBuilder.cs` | Menu item *MGD Samples > Build Accessibility Scene*. Builds the scene, attaches `TextScale` to every text with Auto Size off, adds the scene to Build Settings. |
| `Assets/_Game/Editor/Shared/SampleSceneBuild.cs` | Gains `CreateToggle`, a TextMeshPro toggle resized so the whole row is a 48 dp tap target. |
| `Assets/_Game/Scenes/Accessibility/Accessibility.unity` | Demo area (status, Hit, motion and 48 dp squares, body paragraph, size label) above a Settings card (Haptics, Reduce motion, Small / Normal / Large). Generated; not hand-edited. |
| `Assets/_Game/Scripts/Shared/BackToLauncher.cs` | On the Canvas with the listener on. Not part of what students copy. |
| `docs/accessibility-pass.md` | The ten-check pass run on this scene, in the two-column format students submit for CA1. |

## How to test

1. **Editor:** Play. Tap Hit: the status reads "HIT 1 (haptic pulse sent)"; no buzz in the editor. Turn Haptics off: the status wording changes on the next tap. Turn Reduce motion on: the orange square stops at once. Tap Large: every label grows by a quarter, the paragraph wraps, nothing is cut off; the Large button greys out. Stop and Play again: all three settings come back as set.
2. **Phone:** the same, plus one short buzz per Hit with Haptics on and none with it off. Compare your own buttons against the blue 48 dp square. Set Android *Display size and text* to maximum and check the card still fits inside the margin.
3. **Monochromacy:** Developer options > Simulate colour space > Monochromacy. Every state is still readable because none of it relies on colour alone.

## Known limits

- `Handheld.Vibrate` is the only haptic API the lab uses: one fixed pulse. Rich haptics need a native plugin, which the module rules out.
- The Settings card is not a shared prefab. Each sample that needs settings builds its own; a shared prefab would couple samples through a scene asset.
- The shaking square looks tappable and is not; noted as a cut in the accessibility pass.
- Verified 2026-09-16: editor Play mode (all three settings, persistence across restart, Large layout) and haptics on an Android device.

## Cuts list

1. The 48 dp square and its maths label could go, leaving the Device Simulator as the only measure. Kept because it puts the number on the phone in the student's hand.
2. The text-size label could go; the size buttons already show the state. Kept because it prints the factor students must quote.

## Decisions

- [[2026-09-15 Back navigation returns to the launcher]]

## References

- Lab sheet: `Week 02/Lab B/W02-LabB-LabSheet.md` in the curriculum folder (Parts B, C, D, Troubleshooting).
- Unity docs: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Handheld.Vibrate.html and https://docs.unity3d.com/6000.6/Documentation/ScriptReference/PlayerPrefs.html
- Android touch target size: https://support.google.com/accessibility/android/answer/7101858
- Android accessibility principles: https://developer.android.com/guide/topics/ui/accessibility/principles
