# Known issues

Worked example of the Week 8 Lab B, Part D file for this project: issues found and not fixed yet, each with enough for a stranger to reproduce it. A fixed issue is removed in the commit that fixes it. Submit yours as `KnownIssues.md` at your repo root.

## 1. Adaptive: the pause card covers the readout in landscape

- **Scenario:** 3, rotation.
- **Steps:** auto-rotate on; open the Adaptive sample; rotate to landscape; press Android back.
- **Expected:** the pause card below the readout, so the mode change shows on the overlay while the menu is open (as in portrait).
- **Actual:** the card overlaps the bottom of the readout, and the Load button overlaps the readout card.
- **Logcat:** none (layout only).
- **Build, device:** 0.4.0 (17); found in code review from the layout sizes, not yet seen on a phone.

## 2. Performance: the status line is stale after the High FPS toggle

- **Scenario:** 1, resume (pause menu).
- **Steps:** open the Performance sample; pull the notification shade to pause; tick High FPS; Resume.
- **Expected:** the status line shows `target 120 fps` and `render scale 0.80`.
- **Actual:** it still shows the old target and scale until the next button press.
- **Logcat:** `perf_mode=HighFps high=True target=120 scale=0.80` confirms the mode did change.
- **Build, device:** 0.4.0 (17); found in code review, not yet seen on a phone.

## 3. Performance: one Probe press does nothing after a mode change while the probe is on

- **Scenario:** 1, resume (pause menu).
- **Steps:** in a development build (the Probe button is hidden in release builds), open the Performance sample; press Probe (scale 0.50); pause and tick High FPS (scale 0.80); Resume; press Probe.
- **Expected:** the scale drops to 0.50.
- **Actual:** the first press writes 0.80 again and logs `[Probe] renderScale = 0.8`; the next press works.
- **Logcat:** `[Probe] renderScale = 0.8` straight after the toggle's `perf_mode=HighFps` line.
- **Build, device:** 0.4.0 (17); found in code review, not yet seen on a phone.

## 4. QA: the status line runs into the bottom hint in landscape

- **Scenario:** 3, rotation.
- **Steps:** auto-rotate on; open the QA sample; rotate to landscape.
- **Expected:** every line readable, as in portrait.
- **Actual:** the status line under the three buttons overlaps the trim-memory hint at the bottom; the canvas is about 966 units tall in landscape, and the stack needs more.
- **Logcat:** none (layout only).
- **Build, device:** the first build after 0.4.0 (17); found in code review from the layout sizes, not yet seen on a phone.
