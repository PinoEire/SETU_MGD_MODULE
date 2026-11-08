---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-11-08
tags: [decision, project-layout, quality]
---

# Quality tier assets live in Assets _Game Settings

## Context

Week 9 Lab A needs one URP asset per quality level. The template's `UniversalRP.asset` lives in `Assets/Settings/`, which is template content this project does not edit, and [[2026-09-15 Project-owned assets live under Assets _Game]] lists the type folders inside `_Game` (`Audio`, `Editor`, `Materials`, `Prefabs`, `Resources`, `Scenes`, `Scripts`, `Textures`, `UI`), none of which fits a render pipeline asset.

## Decision

`_Game` gains a `Settings` type folder for project-owned settings assets. The three tier assets are `Assets/_Game/Settings/Quality/URP-Low.asset`, `URP-Mid.asset` and `URP-High.asset`, created and updated only by *MGD Samples > Set Up Quality Tiers*, which copies the template's asset rather than changing it. The template's `UniversalRP.asset` stays where it is and remains the Graphics default; each quality level overrides it with its own.

## Consequences

- The template asset is untouched, so removing the template content later does not break the tiers.
- A new settings asset of our own (a volume profile, a second renderer) has an agreed home.
- [[2026-09-15 Project-owned assets live under Assets _Game]] is not superseded: its list of type folders gains one entry.
- Notes changed: [[Tiers]], [[Home]].
