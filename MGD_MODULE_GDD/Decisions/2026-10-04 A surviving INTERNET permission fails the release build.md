---
type: decision
status: done
decision: accepted
supersedes: "[[2026-09-28 Release builds strip the INTERNET permission]]"
updated: 2026-10-04
tags: [decision, android, permissions, release]
---

# A surviving INTERNET permission fails the release build

## Context

[[2026-09-28 Release builds strip the INTERNET permission]] removes the permission Unity adds to every Android build and accepted, as a consequence, that a change in the manifest's shape would make the hook log "nothing to remove" and only `aapt` would show the permission again. A review on 2026-10-04 pointed out that this is the silent path the whole decision exists to close: the privacy statement would become false and nobody would be told.

## Decision

The hook still strips `INTERNET` from release builds only, and now treats its own failure as a build failure: if the permission is still present in the generated `unityLibrary` manifest after the strip, or the manifest cannot be found, the build stops with a `BuildFailedException` naming the file. The launcher module's manifest is stripped and checked the same way. The pattern matches the element whatever its attribute order, quoting or closing style, and projects that set Internet Access to Require keep the permission on purpose.

## Consequences

- A Unity upgrade that changes the manifest cannot ship a release whose statement is wrong; the build fails and the pattern (with its tests) is updated.
- `aapt dump badging` remains the proof students see; it is no longer the only safeguard.
- The 2026-09-28 note stays as history, marked superseded.
