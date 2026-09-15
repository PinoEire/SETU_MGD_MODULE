---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-09-15
tags: [decision, navigation, android]
---

# Back navigation returns to the launcher

## Context

With more than one sample in the build, the app needs a first scene that lists them and a way back from each. Android delivers the back gesture to Unity's Input System as the Escape key; some samples (Lifecycle) already use it to pause. The module's handout rules out Quit buttons, and back must never exit the app.

## Decision

The launcher scene is build index 0 and every sample scene carries `BackToLauncher` from `Scripts/Shared/`. By default it loads index 0 on Escape. A sample that needs back for itself turns the listener off and wires an on-screen button to `Go()` instead; for Lifecycle that is the *Back to samples* button on the pause card, which is also how real games do it (back pauses, the pause menu exits). On the launcher, back is ignored.

## Consequences

- Adding a sample means: its builder registers the scene and adds `BackToLauncher` to the Canvas. The launcher picks it up automatically.
- `BackToLauncher` is the first shared runtime script; it stays the only thing in `Scripts/Shared/` unless another rule like this appears.
- Samples still do not reference each other's scripts; they reference the launcher only by build index.
- Students copying a sample into their own project delete `BackToLauncher` or point it at their own menu.
