---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-09-28
tags: [decision, release, git]
---

# Release APKs live on GitHub Releases

## Context

Week 4 Lab A tells students to commit their release APK under `releases/` so the hand-in is one tag. This project is cloned by every student and rebuilt many times a semester; an APK is 30 to 40 MB, `.gitattributes` routes `*.apk` through Git LFS, and the free LFS quota is shared by everyone who clones. The repo already ignores `*.apk`.

## Decision

The build script writes the APK to `releases/` and appends a row to `releases/manifest.md` (version, versionCode, date, commit, size, SHA-256, download link). The manifest is committed; the APK is not. Each release is a GitHub Release on the tag with the APK attached, and its link goes in the manifest row. Students follow the lab sheet and commit theirs; the manifest is what they compare against.

## Consequences

- The repo stays small and `git clone` stays fast for the class.
- Anyone verifying a build checks the SHA-256 in the manifest against the file they downloaded.
- Uploading the APK is a manual step after the build (the GitHub CLI is not installed here), so the Download column is filled in by hand.
- `README.md` documents the flow; [[ReleasePipeline]] is the sample note for the script.
