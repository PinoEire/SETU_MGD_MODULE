---
type: home
status: living
updated: 2026-09-15
tags: [home, index]
---

# MGD Module Samples: Design Document

This vault is the design document for the **SETU Mobile Game Development (A12581)** teaching project. The Unity project next to it (`SETU_MGD_MODULE`) is not a game: it is a set of **sample scenes and scripts that show mobile good practice**, one per lab topic, for students to study and copy into their own projects.

The vault is also a **worked example of a GDD that both people and AI tools can read and maintain**. Every convention here exists so that a note makes sense when opened in Obsidian *and* when pasted raw into an AI session. Read [[Conventions]] before adding or changing anything.

## How to read this vault

1. Start here. The tables below are the only place that lists every note.
2. Open a sample note to learn what a sample does, which lab it supports and how to test it on a phone.
3. Follow `[[wikilinks]]` for anything unfamiliar; [[Glossary]] defines the module vocabulary.
4. Decisions are never edited into history. If you want to know *why* something is the way it is, look in `Decisions/`.

## Samples

| Sample | Lab | Status | One line |
|--------|-----|--------|----------|
| [[Launcher]] | none | done | First scene: lists every sample from Build Settings, loads with Awaitable, shows build info. |
| [[SafeArea]] | W02 Lab A | done | Keep the HUD out of the notch and the gesture bar, on any phone, in both orientations. |
| [[Lifecycle]] | W02 Lab B | done | Pause and save on Home, calls, screen off and focus loss; Android back toggles pause; resume is the player's choice. |
| [[Accessibility]] | W02 Lab B | done | Haptics toggle, text size and reduce motion in PlayerPrefs on one Settings card, plus the ten-check pass as a worked example. |

Planned samples follow the lab schedule in the curriculum README (Dropbox, `SETU/2026-2027/Mobile Game Development/README.md`). A sample is added to this table only when its note exists.

## Decisions

| Date | Decision |
|------|----------|
| 2026-09-15 | [[2026-09-15 One folder and scene per sample]] (superseded) |
| 2026-09-15 | [[2026-09-15 Awaitable over coroutines]] |
| 2026-09-15 | [[2026-09-15 Project-owned assets live under Assets _Game]] |
| 2026-09-15 | [[2026-09-15 Back navigation returns to the launcher]] |

## Reference

- [[Conventions]]: how notes are structured and why.
- [[Glossary]]: module vocabulary.
- `Templates/`: Obsidian templates for a [[Templates/Sample|Sample]] and a [[Templates/Decision|Decision]] note. Insert with the Templates core plugin (command palette: *Templates: Insert template*).

## Constraints every sample inherits

- Unity 6.6 (latest Unity 6.x), Android only, sideloaded release-signed [[Glossary#APK|APK]], [[Glossary#IL2CPP|IL2CPP]], [[Glossary#ARM64|ARM64]].
- Stable frame-time over peak FPS. No per-frame allocations in the steady state.
- [[Glossary#Awaitable|Awaitable]] for async work, never coroutines (see the decision above).
- Genre-agnostic: a sample must fit any of the five student project options.
- Everything we write lives under `Assets/_Game/`; imported content is never edited in place (see the decision above).
- C# lives in namespace `MGD.Samples` (editor code in `MGD.Samples.Editor`). It is also the project's root namespace, so new scripts get it without typing it.
- No third-party SDKs, no store uploads, no secrets in the repo.
