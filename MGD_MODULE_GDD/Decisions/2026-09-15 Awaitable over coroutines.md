---
type: decision
status: done
decision: accepted
supersedes: ""
updated: 2026-09-15
tags: [decision, async]
---

# Awaitable over coroutines

## Context

Loading, timers, pooled spawns and telemetry flushes all need asynchronous code. Unity 6 ships [[Glossary#Awaitable|Awaitable]], and the module has two handouts on it (*Awaitable Design Pattern*, *Coroutine vs Awaitable*). Coroutines are still what most tutorials show.

## Decision

All asynchronous code in the samples uses `async` methods returning `Awaitable` (or `Awaitable<T>`), with a `CancellationToken` sourced from `destroyCancellationToken` on the owning `MonoBehaviour`. No `StartCoroutine`, no `IEnumerator` yields. Exceptions are allowed to surface; they are not swallowed with empty catches.

## Consequences

- Samples match the handouts and the Week 5 lab (*Vertical-slice skeleton, Awaitable and pooling*).
- Cancellation on scene unload is explicit and visible, which is the lifecycle behaviour the module cares about.
- Students who arrive knowing only coroutines have one clear pattern to learn from, and the *Coroutine vs Awaitable* handout to bridge it.
- Third-party async libraries (UniTask and similar) are out of scope; no SDKs is a module constraint.
