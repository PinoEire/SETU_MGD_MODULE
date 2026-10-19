---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-10-19
tags: [decision, adaptive-performance, project-layout]
---

# Adaptive Performance settings stay where Unity creates them

## Context

Ticking the Adaptive Performance providers makes Unity create `Assets/Adaptive Performance/`: the per-platform general settings, one loader asset per provider and one settings asset per provider, registered in `ProjectSettings/EditorBuildSettings.asset` as config objects. [[2026-09-15 Project-owned assets live under Assets _Game]] says everything this project owns lives under `Assets/_Game/`, and the lab sheet asks students to commit these files so the provider choice travels with the repo.

## Decision

The folder stays at `Assets/Adaptive Performance/`, committed with its `.meta` files, and is edited only through Project Settings > Adaptive Performance, never by hand. It is treated like `Assets/Settings/` and `Assets/TextMesh Pro/`: configuration a Unity feature creates at a path it chooses, not content this project writes.

## Consequences

- The settings stay where Unity's settings page creates and looks for them, so there is no risk of it creating a second set at the default path after a move.
- [[2026-09-15 Project-owned assets live under Assets _Game]] is not superseded: the `_Game` rule gains a named exception for generated feature settings; new samples still put everything they write under `_Game`.
- Notes changed: [[Adaptive]], [[Home]].
