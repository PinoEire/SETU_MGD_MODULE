# SETU_MGD_MODULE

Sample scenes and scripts for the **Mobile Game Development (A12581)** module at SETU, academic year 2026/2027. Lecturer: **Pino De Francesco**. This is the lecturer's teaching project, not a game and not one of the student project options: each sample shows one mobile good practice from the lab schedule (safe areas, lifecycle, frame pacing, Awaitable-based loading, pooling, telemetry, adaptive performance, and so on), small enough to read in one sitting and copy into your own project.

## Requirements

- Unity **6000.6** (the current Unity 6.x release) with Android Build Support.
- An Android phone with USB debugging enabled. Builds are release-signed APKs, IL2CPP, ARM64, sideloaded with `adb install -r`. No store consoles are used.
- The release keystore lives in `Keystore/` and is not in the repository. Set your own in Project Settings > Player > Android > Publishing Settings if you build your own copy.

## Where things are

| Path | What it is |
|------|------------|
| `MGD_MODULE_GDD/` | The design document, as an Obsidian vault. **Start here.** Open the folder as a vault and read `Home.md`, or read the Markdown directly on GitHub. |
| `Assets/_Game/` | Everything this project owns, in standard type folders. Each sample is a same-named subfolder inside the folders it needs, for example `Scripts/SafeArea/` and `Scenes/SafeArea/`. |
| `Assets/_Game/Scripts/<Sample>/README.md` | A two-paragraph summary of that sample for anyone who opens the folder without the vault. |
| Everything else under `Assets/` | Template content, packages and imported assets. Not edited in place. |

## The vault

`MGD_MODULE_GDD` doubles as a worked example of a design document that people and AI tools can both read and maintain. `Home.md` is the only full index, `Conventions.md` explains the rules and why they exist, `Samples/` has one note per sample with a fixed set of headings, and `Decisions/` holds append-only decision records. If you want to know what a sample does, which lab it supports, how to test it on a phone, or why something is the way it is, the answer is in the vault, not in this file.

## Curriculum

Lab sheets, decks and assessment briefs are distributed through Moodle. Each sample note names the lab it supports.
