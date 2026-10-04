---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-10-04
tags: [decision, folders, shared]
---

# Shared scripts are the ones every scene needs

## Context

[[2026-09-15 Back navigation returns to the launcher]] made `BackToLauncher` the first file in `Scripts/Shared/` and said it would stay the only one "unless another rule like this appears". On 2026-09-28 the Performance sample added `MobileBootstrap` there, because the frame target must be set once in the first scene and the Launcher is that scene, and no decision recorded it. [[Conventions]] section 5 says exactly this situation needs a new note.

## Decision

`Scripts/Shared/` holds scripts that every build needs regardless of which sample is open: today `BackToLauncher` (every sample scene) and `MobileBootstrap` (the first scene). A script goes there only when a module rule applies to the whole app, not to one topic; sample scripts never reference each other and never reference Shared scripts beyond what their scene builder attaches. Each addition gets a decision note.

## Consequences

- `MobileBootstrap` is documented in [[Performance]] (the sample that needed it) and in [[Launcher]] (the scene it lives in).
- The 2026-09-15 note is not superseded: the back rule stands; only its "stays the only thing" remark is replaced by this rule.
- Students copying a sample take `Scripts/Shared/` files knowingly, as app-level scripts, or replace them with their own.
