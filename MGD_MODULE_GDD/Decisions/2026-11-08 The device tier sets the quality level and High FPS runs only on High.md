---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-11-08
tags: [decision, performance, quality, shared]
---

# The device tier sets the quality level and High FPS runs only on High

## Context

Week 9 Lab A adds three quality levels and a `DeviceTier` that picks one at start-up, each tier with its own frame cap and render scale, and says the Week 7 Balanced / High FPS toggle should then adjust both "on top"; its table allows High FPS only on the High tier. Until now [[2026-10-19 Performance mode is shared and its toggle is on every pause card]] gave Balanced a fixed 60 fps at render scale 1.0, and `PerformanceMode` applied itself before the first scene. Two start-up steps that both set the frame rate and the render scale need a fixed order.

## Decision

`DeviceTier` is a static class in `Scripts/Shared/` under [[2026-10-04 Shared scripts are the ones every scene needs]]. Before the first scene loads it resolves the tier (a valid `tier_override` key, else the deck's RAM and cores rule), sets the matching quality level and then calls `PerformanceMode.ApplySaved`, which no longer runs on its own. Balanced runs at the tier's cap and scale (Low 30 fps at 0.75, Mid 60 at 0.9, High 60 at 1.0); High FPS (120 at 0.8) takes effect only on the High tier, the player's choice is saved whatever the tier, and below High the pause card's toggle is greyed out and labelled *High FPS (High tier only)*. The override is chosen in the [[Tiers]] scene, not on the pause card, which has no room left, and applies from the next launch. In the editor the quality level and render scale are restored when Play ends.

## Consequences

- [[2026-10-19 Performance mode is shared and its toggle is on every pause card]] is not superseded: the mode stays shared and its toggle stays on every pause card; only its fixed Balanced values and its own start-up hook are replaced as described here.
- One explicit start-up order replaces the lab's Script Execution Order settings; the README says why.
- Balanced on a High-tier phone is what it was in Week 7 (60 fps at 1.0); on Mid and Low it is slower or softer by design.
- The Week 7 perf-modes table is measured on a High-tier phone, or with High forced from the Tiers scene.
- `session_start` and the `[Boot]` line name the tier.
- Notes changed: [[Tiers]], [[Adaptive]], [[Lifecycle]], [[Performance]], [[Home]], [[Glossary]].
