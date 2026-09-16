# Accessibility pass: Accessibility sample scene

Worked example of the ten-check pass from Week 2 Lab B, Part D, run on `Assets/_Game/Scenes/Accessibility/Accessibility.unity`. This is the format to submit under `/docs/CA1/accessibility-pass.md` in your own project: one row per check, a finding, the fix, and whether the fix is done or on the cuts list. Device used: a 1080 x 2400, 420 dpi Android phone; Device Simulator at 1080 x 2340 for measurements.

| Check | Finding / Fix |
|-------|---------------|
| One-handed reach: every control reachable with the thumb | The Settings card sits in the bottom half, so both toggles and the three size buttons are in thumb reach. The Hit button is in the upper half and needs a stretch. **Cut:** acceptable for a demo; in a game the primary verb belongs in the bottom third. |
| Smallest button at least 48 dp including padding | Toggles are 130 units tall (49 dp on the reference phone), size buttons 250 x 130, Hit 520 x 150. The on-screen 48 dp square confirms the toggle box alone (64 units, 24 dp) is too small on its own, which is why the whole row is the tap target. **Done.** |
| Display size and text at maximum: HUD still inside the safe area | At maximum display size the root's 96 unit margin keeps every element clear of the notch and gesture bar. The body paragraph wraps to four lines at Large and still fits its 190 unit box. **Done.** |
| Simulate colour space, Monochromacy: every state readable | Haptics result is words ("HIT 3 (haptic pulse sent)"), not a colour flash. Toggle state is the checkmark, not a colour. The disabled size button is grey, which is a brightness change and still readable. **Done.** |
| Volume at zero: the game still tells you what happened | The scene has no audio; every event has a text or shape change. **Done.** |
| Text contrast 4.5:1 | White on 0.08/0.09/0.12 background is about 17:1. Black button text on the light UISprite is about 15:1. Grey disabled text on the card is about 5:1. **Done.** |
| Motion or screen shake behind a toggle | Reduce motion toggle stored by `MotionSetting`; the demo square stops shaking immediately when it is on. **Done.** |
| No timed tap without an alternative or a slower mode | Nothing in this scene is timed. **Not applicable.** |
| Haptics off: nothing is lost | With the toggle off the Hit button still reports "HIT n (haptics are off)". **Done.** |
| 60 seconds of silent observation: first thing a neighbour got wrong | Tester tapped the orange shaking square expecting it to do something. **Cut:** label it "decoration" or make it the Hit target; on the cuts list for the sample. |

Findings marked **Cut** go on the cuts list with the date; the pass is repeated in Week 11 as part of the QA evidence pack.
