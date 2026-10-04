---
type: sample
status: done
lab: W02-A
unity: 6000.6.0f1
scene: Assets/_Game/Scenes/TouchDrag/TouchDrag.unity
updated: 2026-10-04
tags: [sample, touch, input-system, w02]
---

# TouchDrag

## Goal

Show how to drag a sprite with a finger through the Input System's Enhanced Touch API, with one finger per sprite, the grabbed point staying under the finger, and nothing left stuck to a finger that has gone. Dragging is the touch verb behind units on a tactics map, items in a crafting inventory, a slingshot or a slider, so the pattern is genre-agnostic.

## Lab it supports

Week 2 Lab A, *Release signing, touch input and safe areas*, Part D (*Drive your core verb from touch*): the Enhanced Touch loop, `TouchPhase`, `startScreenPosition`, and the 48 dp touch-target rule.

## What the student learns

- `EnhancedTouchSupport.Enable()` first, or `Touch.activeTouches` is always empty; disable it again in `OnDisable`.
- Own a drag by `touch.touchId`, never by finger index: Enhanced Touch hands a freed finger slot to the next touch, so a thumb lifted and replanted looks like the same finger.
- Read the touches present each frame and drop any drag whose touch is not among them. A finger that vanishes while the app is in the background never sends Ended.
- Grab at `startScreenPosition`, not `screenPosition`: a fast finger has already moved by its first frame. One attempt per touch: a finger that missed must not keep trying, or it snatches a sprite dragged under it later, and a finger refused because the sprite was held must not take it the moment the other finger lets go.
- Keep the grab offset so the sprite does not jump to the fingertip; clamp to the camera so it cannot be dragged off screen.
- Find what is under a finger with `Physics2D.OverlapPoint` and a `Collider2D` at least 48 dp across; the collider is the touch target, not the sprite. Give the sprite a kinematic `Rigidbody2D`: a collider moved by its transform with no body is static to Physics2D, which rebuilds the static world every time it moves.
- Show pick-up in shape and brightness, not colour alone, so it reads in monochrome.
- `TouchSimulation.Enable()` in the editor turns the mouse into one finger for Play-mode testing and ships nothing.

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/TouchDrag/DragTracker.cs` | Pure logic: which touch holds which transform and at what offset, and which touches have had their one grab attempt. `ShouldAttempt`, `TryBegin`, `TryMove` (with optional bounds), `TryGetTarget`, `Forget`, `IsHeld`, `Prune`. What students copy, with its tests. |
| `Assets/_Game/Scripts/TouchDrag/TouchDragController.cs` | The Enhanced Touch loop: a touch seen for the first time gets one grab attempt at its start position (not only on Began, because a fast finger can be first seen already Moved), a tracked touch moves, Ended and Canceled release and forget the touch (Android may reuse the id next frame), then `Prune`. Camera bounds re-read when the screen size changes. `Update` returns at once while `LifecycleGuard.IsPaused`, because Enhanced Touch bypasses the pause panel's raycast. What students copy. |
| `Assets/_Game/Scripts/TouchDrag/Draggable.cs` | Marker on each sprite with a `Collider2D` and a kinematic `Rigidbody2D`; grows and brightens while held. What students copy. |
| `Assets/_Game/Scripts/TouchDrag/TouchDragHud.cs` | Demo only. "2 fingers, holding Red and Blue" in a reused StringBuilder, rebuilt only on change; the wording is a tested static. |
| `Assets/_Game/Scripts/TouchDrag/README.md` | Two-paragraph summary for students who open the folder without this vault. |
| `Assets/_Game/Editor/TouchDrag/TouchDragSceneBuilder.cs` | Menu item *MGD Samples > Build TouchDrag Scene*: camera, three Knob sprites at scale 5 (about 73 dp on the reference phone, 90 dp on a 20:9 one) with circle colliders sized to the sprite and kinematic bodies, controller, HUD, Build Settings entry. |
| `Assets/_Game/Editor/Tests/DragTrackerTests.cs` | Fourteen EditMode tests: offset kept, second finger on a held sprite refused, same touch twice refused, null target refused, unknown touch ignored, forget then re-grab, bounds clamp and bounds cleared, prune drops vanished touches and reports them, one grab attempt per touch until pruned, and forgetting a touch so a reused id starts fresh. |
| `Assets/_Game/Editor/Tests/TouchDragViewTests.cs` | Five EditMode tests: the status wording for zero, one and two fingers, and the visible world rectangle of an orthographic camera. |
| `Assets/_Game/Scenes/TouchDrag/TouchDrag.unity` | Generated; not hand-edited. |
| `Assets/_Game/Scripts/Shared/BackToLauncher.cs` | On the Canvas with the listener on. Not part of what students copy. |

## How to test

1. **Editor:** Play the Launcher, tap TouchDrag. Press and drag a circle with the mouse (touch simulation): it grows, brightens and follows with the grabbed point under the cursor; the status reads "1 finger, holding Red". Release: it shrinks back, "0 fingers, nothing held". Drag towards the edge: it stops at the screen edge.
2. **Phone:** the same with a finger, then two fingers on two circles at once: both move independently and the status names both. Put one finger on a circle, a second finger on the same circle: the second is ignored. Mid-drag press Home, come back and tap Resume: the circle was released when the pause landed, nothing is stuck. A finger kept on the screen through a pause is a fresh touch after Resume and must lift and grab again.
3. **Tests:** Window > General > Test Runner, EditMode, run all: the `DragTracker` and `TouchDragView` tests pass.

## Known limits

- A pause releases every drag at once (`LifecycleGuard.PausedChanged`), because touches that end and begin while paused are never seen and Android reuses their ids; the cost is that a finger held down through the shade has to lift and grab again after Resume. The precise alternative, forgetting only touches that started after the pause, is more code than the lesson is worth.
- One camera, orthographic, sprites at z 0: `ScreenToWorldPoint` needs nothing more. A perspective camera needs a plane or depth.
- Overlapping sprites: `OverlapPoint` returns one collider, not the top-most sprite; sort by sorting order if a game needs that.
- No inertia, snapping or drop targets; those are gameplay, not the touch pattern.
- A sprite can change hands within one frame (one finger's Ended is listed before another's Began on the same spot); the new holder keeps it drawn as held. `OverlapPoint` is answered from the last physics step, so a moving sprite's collider trails its transform by a frame; nobody can tap that fast.
- Verified 2026-09-28: editor Play mode with simulated touch events (grab, move, release, clamp) and the release APK on a Pixel 7a. One attempt per touch, kinematic bodies and rotation-aware bounds added 2026-10-04.

## Cuts list

1. The HUD status could go, leaving the grow-and-brighten as the only feedback; kept because it shows the finger count, which is how students confirm multi-touch works on their phone.
2. Bounds clamping could go; kept because a sprite dragged off screen is the first bug every student hits.

## Decisions

- [[2026-09-15 Back navigation returns to the launcher]]
- [[2026-10-04 Every scene pauses on focus loss]]

## References

- Lab sheet: `Week 02/Lab A/W02-LabA-LabSheet.md` in the curriculum folder, Part D.
- Input System: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/Touch.html and https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.EnhancedTouch.Touch.html
- Unity docs: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.OverlapPoint.html
- Android touch target size: https://support.google.com/accessibility/android/answer/7101858
