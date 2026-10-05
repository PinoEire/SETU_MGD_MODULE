---
type: sample
status: done
lab: W05-A
unity: 6000.6.0f1
scene: Assets/_Game/Scenes/Pooling/Pooling.unity
updated: 2026-10-04
tags: [sample, pooling, awaitable, gc, w05]
---

# Pooling

## Goal

Show the two patterns Week 5 asks every project to adopt before the vertical slice grows: a timed loop written with [[Glossary#Awaitable|Awaitable]] and a cancellation token instead of a coroutine, and an [[Glossary#Object pool|object pool]] that prewarms at load so that spawning in play never calls `Instantiate`, and the pool itself allocates nothing. The spawned things are plain coloured circles, so the same code fits enemies, runner chunks, tiles, units or particles.

## Lab it supports

Week 5 Lab A, *Vertical-slice skeleton: Awaitable and pooling*: Part B (`WaveTimer` as the coroutine-conversion template), Part C (`EnemyPool` and `Enemy`, here `SpawnPool` and `Pooled`) and Part D (the GC Alloc Profiler check; see *Known limits* for the few bytes the cancellable wait costs per spawn). Part A's Boot > Menu > Game > Result flow is left to the students: the [[Launcher]] already shows the bootstrap and an Awaitable scene load, and Lab B's loading sample covers the rest.

## What the student learns

- A coroutine converts to an `async Awaitable` method one line at a time: `yield return new WaitForSeconds(s)` becomes `await Awaitable.WaitForSecondsAsync(s, ct)`, and `StopCoroutine` becomes `cts.Cancel()`.
- The token is linked to `Application.exitCancellationToken`, so the loop stops when Play mode stops in the editor and when the app quits on the phone; `OperationCanceledException` is caught because cancellation is the normal way out, not an error (decision [[2026-10-04 Cancellation tokens where the awaited API takes one]]).
- An async method runs synchronously up to its first `await`, so code before it runs inside `OnEnable`, possibly before another component's `Awake`. The timer therefore waits first and spawns after, so the pool has always prewarmed.
- A pool prewarms in `Awake` (behind the load), hands out from a `Stack<T>` in `Spawn` and takes back in `Release`; `Instantiate` belongs to load time, never to the steady state.
- A pooled object resets its state in `OnEnable` (timers, colour, velocity), because it is reused, not new.
- When the stack runs dry the pool still works but has to create; the `created` counter climbing during play is the signal to raise `size`, and the warning says so once.
- Releasing the same object twice puts it in the stack twice and hands it out to two owners; the pool refuses and warns.
- GC Alloc is checked on the phone in the Profiler, not in the editor (the editor adds allocations of its own, such as TextMeshPro copying HUD text for the Inspector).
- HUD labels update only when a number changes, and through `SetText` with numbers, so the HUD builds no string while the pool cycles.
- The Profiler shows where each allocation comes from: a pool that is working leaves 0 B in the spawn and release code, and the only bytes left on a spawn frame belong to the cancellation token of the wait, which is a deliberate trade for a loop that stops at once.

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/Pooling/SpawnPool.cs` | The deck's `EnemyPool` at project quality. `Prewarm()` (called from `Awake`) creates `size` inactive children; `Spawn(Vector2)` pops from a `Stack<Pooled>`, creating only when the stack is empty and warning the first time; `Release(Pooled)` deactivates and pushes, ignoring and warning on a double release. Exposes `ActiveCount`, `FreeCount`, `CreatedCount`. What students copy. |
| `Assets/_Game/Scripts/Pooling/Pooled.cs` | The lab's `Enemy` without the genre. `Init(pool)` stores the owner; `OnEnable` calls `ResetState()` (age back to 0 against a 4 s lifetime by default, colour, drift velocity); `Release()` hands it back; it also releases itself when its lifetime runs out, so the pool cycles with nobody tapping. What students copy. |
| `Assets/_Game/Scripts/Pooling/WaveTimer.cs` | The lab's `WaveTimer`: token linked to `Application.exitCancellationToken` in `OnEnable`, cancelled and disposed in `OnDisable`, the loop awaits `Awaitable.WaitForSecondsAsync(interval, ct)` first and then spawns at a random point in the upper part of the screen (`bottomReserved` keeps the HUD clear), `OperationCanceledException` caught. Skips spawning while `LifecycleGuard.IsPaused`. `Interval` can be changed at runtime. What students copy. |
| `Assets/_Game/Scripts/Pooling/TapToRelease.cs` | Enhanced Touch: a finger down on an item releases it, through `Physics2D.OverlapPoint` (no allocation). Stands in for "your core verb kills it". Ignored while paused. |
| `Assets/_Game/Scripts/Pooling/PoolingDemoHud.cs` | Demo only. Shows `active / free / created` and the spawn interval, rewriting a label only when its number changes; Faster and Slower buttons halve and double the interval so the pool can be pushed past its prewarm on purpose. |
| `Assets/_Game/Scripts/Pooling/README.md` | Two-paragraph summary for students who open the folder without this vault. |
| `Assets/_Game/Prefabs/Pooling/Pooled.prefab` | Generated by the builder: built-in Knob sprite, URP `Sprite-Unlit-Default` material, `CircleCollider2D` sized to the sprite, a Kinematic `Rigidbody2D` (the item moves itself, and a moving collider without a body rebuilds the static physics world), `Pooled`. Not hand-edited. |
| `Assets/_Game/Editor/Pooling/PoolingSceneBuilder.cs` | Menu item *MGD Samples > Build Pooling Scene*. Writes the prefab, then the scene: camera, event system, `Spawn Pool` (size 32, as on the deck) with `WaveTimer` and `TapToRelease`, a HUD card in the bottom third with title, counters line, interval line and the Faster and Slower buttons; `BackToLauncher` with its back listener on, the pause menu through `SampleSceneBuild.AddPauseMenu`; adds the scene to Build Settings after TouchDrag. |
| `Assets/_Game/Editor/Tests/SpawnPoolTests.cs` | EditMode tests. `SpawnPoolTests`: prewarm creates `size` inactive children and a second prewarm creates nothing; `Spawn` reuses a free item, and after a release hands the same one back; `Spawn` on an empty stack creates, counts and warns once; `Release` deactivates and returns; a double release and a release to another pool are refused with a warning; no prefab logs one error and `Spawn` returns null; `ResetState` restores age and colour; `Advance` past the lifetime releases to the pool. `WaveTimerTests`: the interval clamp, and the spawn area that keeps the HUD share and the edge inset clear. EditMode never runs `Awake`, `OnEnable` or `Update`, which is why `Prewarm()`, `ResetState()` and `Advance()` are public. |
| `Assets/_Game/Scenes/Pooling/Pooling.unity` | Generated; not hand-edited. |
| `Assets/_Game/Scripts/Shared/BackToLauncher.cs`, `LifecycleGuard.cs`, `PauseMenu.cs` | Shared, added by the builder like every scene. Belong to [[Launcher]] and [[Lifecycle]]. |

## How to test

1. **Editor, steady state:** Play the Launcher, tap Pooling. Circles appear every 0.5 s at random points and fade out over their 4 s lifetime and vanish. The HUD reads `active A / free F / created 32`; `created` stays at 32 for as long as you watch, and the `Spawn Pool` children toggle active and inactive in the Hierarchy without their count growing.
2. **Editor, tap:** click a circle: it disappears at once and `free` goes up by one.
3. **Editor, overflow:** press Faster three times (0.5 s to 0.0625 s), so about 64 circles are alive at once against a pool of 32. `created` climbs past 32 and the console shows one `[SpawnPool] ... Raise size` warning. Press Slower: `created` stops climbing and never goes back down (the pool keeps what it made).
4. **Editor, cancellation:** with spawning running, stop Play mode. No error appears in the console after the stop: no `OperationCanceledException`, and no `MissingReferenceException` from a loop that kept running into destroyed objects. The timer does not log each spawn, because a log line per spawn would itself allocate.
5. **Editor, pause and back:** focus loss shows the pause panel and spawning stops until Resume; Escape returns to the Launcher.
6. **Phone, Part D:** Development Build with Autoconnect Profiler, open Pooling, wait for two minutes of spawning at the default interval. In the CPU module Hierarchy, `Instantiate` does not appear after load and the GC Alloc column reads 0 B on frames without a spawn; a spawn frame shows only the token registration under `Awaitable.RunContinuation` (about 48 B in the editor) and nothing under `SpawnPool` or `Pooled` apart from one-off entries: the very first release (270 B in the editor), and the stack growing once if the pool has overflowed and the extra items come back. Record the GC Alloc reading and the `created` count in the verified line under *Known limits*. (The Week 5 row of `docs/baseline-sheet.md` is load time, which belongs to Lab B.)
7. **EditMode tests:** Window > General > Test Runner, EditMode, run all: the `SpawnPool` and `WaveTimer` tests pass.

## Known limits

- The pool is a hand-rolled `Stack<T>` because that is what the lab sheet and deck show. `UnityEngine.Pool.ObjectPool<T>` does the same job with create, get, release and destroy callbacks; the pattern and the checks are identical.
- The pool never shrinks: items created on overflow stay for the life of the scene, which is the right trade on a phone (memory is cheaper than a hitch) but should be noticed and fixed by raising `size`.
- One prefab per pool. A project spawning several kinds keeps one pool each, or a dictionary of pools keyed by prefab; not shown, to keep the copyable part short.
- Tap release uses colliders and `Physics2D.OverlapPoint`, which is fine for a few dozen items; a project with hundreds of tappable things would test distances itself.
- Passing the cancellation token to `Awaitable.WaitForSecondsAsync` allocates once per wait (about 48 B in the editor Profiler; without the token the same loop measured 0 B), because Unity registers a callback on the token so the wait ends the moment it is cancelled. Kept, as decision [[2026-10-04 Cancellation tokens where the awaited API takes one]] asks: a few bytes every half second is no frame-time risk, and the alternative (wait without the token, check it afterwards) keeps a dead loop waiting for up to two seconds after the scene has gone. The lab sheet's exit ticket ("0 B per frame during spawning") is met by the spawn code, not by this registration.
- The phone Profiler reading (step 6) is not recorded until the device run.
- Verified 2026-10-04 in the editor: at the default 0.5 s the pool held 8 active and `created` stayed at 32 with 32 children; three Faster presses (62.5 ms) took `created` to 61 with one warning; a 3.4 s pause froze every item's age and stopped spawning, and spawning resumed after Resume; back to the Launcher and stopping Play mid-wait left no exception in the console. After the review fixes (timer waits before its first spawn, Kinematic body on the prefab) the scene was rebuilt and loaded three times in a row with no warning and `created` at 32 each time, and a 4 s editor Profiler capture of the game loop showed nothing allocated under `SpawnPool` or `Pooled` beyond a single 270 B first-release entry, about 48 B per spawn under `Awaitable.RunContinuation`, and editor-only TextMeshPro allocations under `PoolingDemoHud.Update`. 94 EditMode tests pass, 15 of them for this sample.

## Cuts list

1. The Faster and Slower buttons could go, leaving the overflow test to the Inspector; kept because pushing the pool past its prewarm on the phone is the clearest way to show what `created` is for.
2. `TapToRelease` could go, leaving the lifetime as the only release; kept because a pool that only times out does not show a gameplay event returning an object.
3. The double-release guard could go to match the deck exactly; kept because it is the bug students hit first when a hit and a timeout both release the same object.

## Decisions

- [[2026-09-15 Awaitable over coroutines]] (superseded by the next, which this sample is the first to exercise)
- [[2026-10-04 Cancellation tokens where the awaited API takes one]]
- [[2026-09-15 Back navigation returns to the launcher]]
- [[2026-10-04 Every scene pauses on focus loss]]
- [[2026-09-15 Project-owned assets live under Assets _Game]]

## References

- Lab sheet: `SETU/2026-2027/Mobile Game Development/Week 05/Lab A/W05-LabA-LabSheet.md` (Dropbox).
- Deck: `A12581-W05-LabA-Vertical-slice-skeleton-Awaitable-and-pooling.pptx`, `EnemyPool` slide.
- Handouts: *Awaitable Design Pattern*, *Coroutine vs Awaitable*.
- Unity 6.6 `Awaitable`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Awaitable.html
- Unity 6.6 `Application.exitCancellationToken`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Application-exitCancellationToken.html
- Unity 6.6 `ObjectPool<T>`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Pool.ObjectPool_1.html
