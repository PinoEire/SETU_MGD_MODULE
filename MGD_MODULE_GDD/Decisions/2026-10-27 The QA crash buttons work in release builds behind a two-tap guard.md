---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-10-27
tags: [decision, qa, release]
---

# The QA crash buttons work in release builds behind a two-tap guard

## Context

The [[QA]] sample has buttons that throw an exception, cause an ANR and crash the app, so students see each logcat signature before Week 8 Lab B. The Performance sample hides its debug-only probe in release builds with `Debug.isDebugBuild`, but the lecturer demonstrates from the release APK, and Lab B tells students to test release builds because development builds hide some failures.

## Decision

The three buttons work in every build. Each needs a second tap within 3 s (`ArmedTrigger`), and the label reads *Tap again to confirm* while armed, so a stray tap does nothing. In the editor the ANR and the native crash only log, because they would freeze or close the editor.

## Consequences

- The release APK can demonstrate every signature; no separate development build is needed for the lab.
- A curious player of the sample app can crash it on purpose; it is a teaching app, and the guard makes that a deliberate act.
- The QA note and README say plainly that a student's own game should not ship such buttons.
- Notes changed: [[QA]], [[Home]], [[Glossary]].
