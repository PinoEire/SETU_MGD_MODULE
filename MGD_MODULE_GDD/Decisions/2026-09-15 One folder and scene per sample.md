---
type: decision
status: superseded
decision: superseded
supersedes: ""
updated: 2026-09-15
tags: [decision, structure]
---

# One folder and scene per sample

> Superseded by [[2026-09-15 Project-owned assets live under Assets _Game]]. Kept unchanged as history.

## Context

The project must teach a different mobile practice in each lab, to students who are each building one of five different genres. A single integrated demo game would couple the samples together and make each one harder to lift out.

## Decision

Every sample lives in its own folder under `Assets/Samples/<Name>/` with its own scene, scripts, a short `README.md` and any assets it needs. Samples do not reference each other's scripts. Shared helpers, if they ever appear, go in `Assets/Shared/` and must stay tiny.

## Consequences

- A student can copy one folder into their project and it works.
- The Build Settings scene list becomes a menu of samples; a small launcher scene can list them later if needed.
- Some code will be duplicated across samples. That is accepted: readability for a 4th year student beats reuse here.
- Each sample gets one note in `Samples/` with the same headings, so the vault and the project mirror each other.
