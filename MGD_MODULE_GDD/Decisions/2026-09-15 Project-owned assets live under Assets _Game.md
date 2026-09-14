---
type: decision
status: done
decision: accepted
supersedes: "[[2026-09-15 One folder and scene per sample]]"
updated: 2026-09-15
tags: [decision, structure]
---

# Project-owned assets live under Assets/_Game

## Context

A Unity project fills up with things the team did not write: template scenes and settings, Unity packages that unpack into `Assets/`, Asset Store purchases, plugin SDKs. Each arrives with its own folder layout and its own idea of where `Scripts/` or `Materials/` should be. Once they are mixed with the team's own work it becomes hard to tell what is safe to edit, delete or upgrade, and hard to hand a student a clean copy of just the teaching material.

## Decision

Everything this project owns lives under one folder, `Assets/_Game/`, physically separated on disk from everything imported. The leading underscore sorts it to the top of the Project window. Inside it the standard Unity type folders apply:

```
Assets/_Game/
  Audio/
  Editor/        editor-only scripts; Unity compiles this folder into the Editor assembly
  Materials/
  Prefabs/
  Resources/     only for assets that must be loaded by name at runtime; keep it near-empty
  Scenes/
  Scripts/
  Textures/
  UI/            UI Toolkit documents, styles and uGUI sprites
```

Each sample is gathered by name across those folders: `Scenes/SafeArea/`, `Scripts/SafeArea/`, `UI/SafeArea/` and so on, so a student copies the subfolders with that name. Shared code that more than one sample uses goes in `Scripts/Shared/` and stays small.

Anything imported (Unity template content such as `Assets/Scenes/SampleScene.unity`, `Assets/Settings/`, `Assets/Welcome/`; Asset Store packages; third-party plugins) stays wherever its importer puts it and is never edited in place. If an imported asset needs changing, a copy is made under `_Game/`.

Empty folders hold a `.gitkeep` so the skeleton survives in Git; Unity ignores dot-files and does not generate `.meta` files for them.

## Consequences

- One glance at the Project window shows what is ours. Upgrading a package or deleting a store asset cannot touch `_Game/`.
- Students are shown the same layout in Week 1 and asked to use it in their own projects.
- Supersedes [[2026-09-15 One folder and scene per sample]]: a sample is now a set of same-named subfolders across the type folders rather than one folder. The "no cross-sample script references" rule from that decision still holds.
- Sample notes list every path a sample owns under *Files*, because a sample is no longer a single folder that can be found by name.
- Template content left at the root of `Assets/` is not ours; it can be deleted once nothing references it, and that deletion is recorded here when it happens.
