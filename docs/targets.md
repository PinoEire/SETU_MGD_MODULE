# Final-sprint targets (set Wed 11 Nov 2026)

Worked example of the Week 9 Lab B, Part D targets for this project. Submit yours as `/docs/CA3/targets.md`: one target per heading, one number each, taken from the Baseline Sheet and the test plan rather than from hope. The numbers below are targets to meet by CA3; the measurements they start from are named next to each one.

## Thermals

Balanced mode holds the 99th-percentile frame time at or below 18.0 ms at minute 10 of a soak on the Pixel 7a, in the Adaptive scene with the load on Hot.
Tier: Mid, which the Pixel 7a picks (it reports 7466 MB, under the deck's 8192 MB line), so 60 fps at render scale 0.9.
Starting point: 16.73 ms at the 99th percentile in the Week 3 baseline (Worst state, not soaked); the Week 7 soak row in `docs/baseline-sheet.md` is still to be measured.
Method: if the soak shows `ThrottlingImminent` before minute 10, drop the tier's render scale one step when that warning arrives.

## Resilience

All six rows of `docs/qa/test-plan.md` pass on the Pixel 7a and on one peer phone, with evidence per row.
Starting point: the plan is written and not yet run on a phone; `KnownIssues.md` lists five open issues.
Closed-test responses that report a crash, freeze or "not responding": 0 of at least 10 by Sun 29 Nov. (The lab counts crash-free sessions from `session_start` and `session_end`; this project logs no `session_end`, so it counts the closed-test form instead.)

## Store assets (awareness only)

Icon 512x512, feature graphic 1024x500, 4 screenshots, short and long description, and the privacy statement in `docs/privacy-statement.md` matching what the app stores, reviewed in the Week 10 Lab B critique.
Starting point: `docs/store-assets-checklist.md` from Week 4.
