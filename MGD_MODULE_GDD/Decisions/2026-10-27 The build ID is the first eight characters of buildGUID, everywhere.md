---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-10-27
tags: [decision, release, telemetry, about]
---

# The build ID is the first eight characters of buildGUID, everywhere

## Context

Week 8 Lab A has students stamp every build with `Application.version` and the first eight characters of `Application.buildGUID`, so feedback from a closed test names one binary, and add the same ID to `session_start`. This project already records each release in `releases/manifest.md` by version, versionCode, commit and SHA-256, but none of those is visible on the phone, so a tester's report could not be matched to a manifest row.

## Decision

`BuildStamp.ShortId` in `Scripts/Shared/` is the one definition of the build ID: the first eight characters of the GUID, or `editor` when there is no build (an empty or all-zero GUID, which is what Play mode reports). It is shown on the [[About]] screen and in the Launcher footer, sent as `build` in `session_start`, and written by `ReleaseBuild` into a new last column of the release manifest from the build report's GUID, which is the built player's `buildGUID`. The column goes last so the rows written before it still line up.

## Consequences

- An About screenshot, a telemetry log and a manifest row can be matched by eye.
- `BuildStamp` is shared under [[2026-10-04 Shared scripts are the ones every scene needs]], because the bootstrap and the Launcher need the ID too.
- Releases up to 0.4.0 have no build ID in the manifest; their rows end at Download.
- Notes changed: [[About]], [[ReleasePipeline]], [[Launcher]], [[Home]], [[Glossary]].
