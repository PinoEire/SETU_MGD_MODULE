---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-11-08
tags: [decision, about, release, build-info]
---

# The build date and commit are stamped into a tracked BuildInfo asset before every build

## Context

Week 12 Lab A asks for an About screen with the build date, from a `BuildInfo` ScriptableObject that students update by hand, with an optional editor step to automate it. A date typed by hand goes stale on the first build someone forgets, and the phone has no git to ask. The project already names each build with a build ID ([[2026-10-27 The build ID is the first eight characters of buildGUID, everywhere]]), but an ID says nothing about when or from what the build was made.

## Decision

`Assets/_Game/Settings/BuildInfo.asset` holds `buildDate` and `gitHash`; `Settings` is the type folder for project-owned configuration assets, the nearest fit for build metadata. `BuildInfoStamper`, an `IPreprocessBuildWithReport`, writes today's UTC date (invariant culture, yyyy-MM-dd) and `git rev-parse --short HEAD` into it before every player build and saves it. The asset is tracked and is committed with each release, as the versionCode bump is. The About screen shows it as one line under the build stamp; the asset is referenced by the scene, not loaded from `Resources`.

## Consequences

- Every build carries its own date and commit without anyone remembering to update them.
- Every build, including a test build and a failed one, leaves the asset modified in the working tree; it goes into the release commit, and between releases the change is committed or discarded.
- The hash does not say whether the tree had uncommitted changes; the manifest's Commit column (`git describe --dirty`) keeps that record, and its hash is the part after the `g` (`v0.8.0-1-g3669742` is `3669742`).
- A machine without git stamps `unknown`, and the build still succeeds.
- [[About]] carries the files and the worked release checklist.
