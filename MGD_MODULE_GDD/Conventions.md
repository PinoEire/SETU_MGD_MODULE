---
type: conventions
status: living
updated: 2026-09-15
tags: [conventions, meta]
---

# Conventions: an AI- and human-friendly GDD

A GDD is read far more often than it is written, and increasingly it is read by an AI assistant that has been handed a note as context. These rules keep every note useful to both readers. They are deliberately few.

## 1. One note, one thing

A note describes exactly one sample, one decision or one reference topic. If a note needs a second `# heading`, it is two notes. Small notes fit in a human's attention and in an AI's context window, and they can be linked from many places without dragging unrelated text along.

## 2. Frontmatter is the contract

Every note begins with YAML frontmatter. An AI reads it as structured data; Obsidian shows it as Properties. The required keys are:

| Key | Values | Purpose |
|-----|--------|---------|
| `type` | `home`, `conventions`, `glossary`, `sample`, `decision`, `template` | What kind of note this is. |
| `status` | `planned`, `in-progress`, `done`, `superseded`, `living` | Whether the content can be trusted as current. `living` is for notes that are always being updated. |
| `updated` | ISO date `YYYY-MM-DD` | Last meaningful edit. Bump it whenever the body changes. |
| `tags` | list | Lowercase, hyphenated. Always include the type as a tag. |

Sample notes add `lab` (for example `W02-A`), `unity` (editor version the sample was last verified in) and `scene` (path of the sample scene in the Unity project). Decision notes add `decision` (`accepted`, `superseded`) and `supersedes` (a wikilink or empty).

Anything a reader or tool would want to filter on goes in frontmatter, not in prose.

## 3. Headings in a fixed order

Each note type uses the headings from its template, in that order, even when a section is one line. A reader learns where to look once. An AI can be told "update the *How to test* section" and find it. Do not invent new headings; add a sub-bullet instead.

## 4. Link, do not repeat

Use `[[wikilinks]]` for every reference to another note and to glossary terms the first time they appear. Never paste the same paragraph into two notes; link to the one that owns it. If an AI edits the owner, every link stays correct.

External sources (lab sheets, Unity docs) are linked by path or URL under *References*, never summarised at length. The vault records decisions and intent; the lab sheets record procedure.

## 5. Decisions are append-only

When a design choice changes, do not edit the old text into the new one. Write a new decision note in `Decisions/`, set the old note's `decision: superseded` and point the new note's `supersedes` at it, and update the sample note to match. History stays readable, and an AI asked "why is it like this?" has somewhere to look.

## 6. Plain, specific language

Short sentences. Name concrete files, scenes and menu paths. Say "the HUD moves 84 px in from the top on a Pixel 8" rather than "the HUD respects the notch". Avoid words that only mean something to the author ("the usual setup"). UK English spelling.

## 7. No secrets, ever

No keystore passwords, alias passwords, API keys, personal device identifiers or student names. The vault is committed to Git and may be handed to an AI service.

## 8. How to hand a note to an AI

Paste the whole note, frontmatter included, and say which section you want changed. Because the frontmatter names the type, status and lab, the assistant does not need the rest of the vault to act sensibly. When it returns the note, check three things before saving: the frontmatter keys are unchanged, `updated` was bumped, and no heading was added or renamed.

## Folder map

```
MGD_MODULE_GDD/
  Home.md            entry point and the only full index
  Conventions.md     this note
  Glossary.md        module vocabulary
  Samples/           one note per sample
  Decisions/         append-only decision records, named YYYY-MM-DD Title
  Templates/         Obsidian templates (Templates core plugin points here)
```
