---
type: decision
status: done
decision: accepted
supersedes: "[[2026-09-15 Awaitable over coroutines]]"
updated: 2026-10-04
tags: [decision, async]
---

# Cancellation tokens where the awaited API takes one

## Context

[[2026-09-15 Awaitable over coroutines]] says every async method takes a `CancellationToken` from `destroyCancellationToken`. The only async code in the project so far, `LauncherMenu.LoadAsync` and `BackToLauncher.LoadLauncherAsync`, awaits `SceneManager.LoadSceneAsync`, which has nowhere to put a token and cannot be cancelled once started. The decision template asks for decisions "specific enough to check code against", and the vault is shown to students as a worked example; the first check against this one fails.

## Decision

All asynchronous code still uses `async` methods returning `Awaitable` with no coroutines and no swallowed exceptions. A `CancellationToken` from `destroyCancellationToken` (or one linked to `Application.exitCancellationToken` for loops) is passed wherever the awaited API accepts one: `Awaitable.WaitForSecondsAsync`, `Awaitable.NextFrameAsync`, pooled spawners and timers from Week 5 on. A method whose only await is an `AsyncOperation` such as `LoadSceneAsync` takes no token, and says so in a comment if it is not obvious.

## Consequences

- The two scene loads in the project are correct as written and need no change.
- The Week 5 samples (`WaveTimer`, the loading screen) are where the token rule first bites, and they must show it.
- A reader can now check any async method against this note in one glance: does the awaited call take a token?
- The 2026-09-15 note stays in the vault, marked superseded, as the record of the original intent.
