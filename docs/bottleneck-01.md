# Bottleneck 01: Performance sample, Worst state

Worked example of the Week 3 Lab A, Part F note, written for `Assets/_Game/Scenes/Performance/Performance.unity` in the Worst state (1500 sprites at scale 4, half transparent). Submit yours as `/docs/CA2/baseline/bottleneck-01.md`. Editor numbers come from the Unity Profiler in Play mode on the development PC and are labelled as such; they are not a phone measurement and cannot give a verdict, because a desktop GPU draws this scene inside the 16.7 ms budget at every load state. Phone cells are filled in from the Android Dev build with the Profiler attached.

**What:** Every frame draws 1500 overlapping translucent sprites on top of one C# loop that moves 1500 transforms. Overdraw, worked: the Knob sprite is 0.2 world units across at its 200 pixels per unit, so 0.8 units at scale 4, a quad of 0.64 square units; the camera shows 10 units by 5.625 at 9:16, 56.25 square units; 1500 x 0.64 / 56.25 is about 17 layers of quads per pixel (about 13 where the circle actually covers the pixel), every one of them blended.

**Where:** Editor: no marker stands out; the frame is capped by `targetFrameRate` at 16.6 ms in Idle, Steady and Worst alike, so the PC never reaches the bottleneck. Phone: (to fill after the device run; expect `Gfx.WaitForPresentOnGfxThread` to grow in Worst, or `LoadGenerator.Update` if the CPU is the limit).

**Numbers:**

| Measure | Editor (PC, Play mode) | Phone (Android Dev build) |
|---------|------------------------|---------------------------|
| Main thread ms, Worst | 16.6 median, 17.8 max (frame-rate capped) | |
| SetPass calls / batches / triangles, Worst | 14 / 1,506 / 10,895 | |
| GC allocated per frame | 0 B from the sample's scripts (the editor's own loop adds a constant that is identical in Idle and Worst) | |
| Frame time at full scale / at 0.5 | 16.6 ms / 16.6 ms (capped both ways) | |

**Verdict:** Editor: no verdict possible; the PC is not the target and never leaves the frame cap. Phone: (to fill after the device run; expect GPU-bound on most phones, because the overdraw is the largest cost and halving the render scale cuts it by three quarters).

**Fix to try:** Drop `worstScale` from 4 to 2 to cut overdraw by about three quarters and re-measure; if the frame time barely moves, the cost is in `LoadGenerator.Update` and the fix is fewer active sprites.

Linked evidence: `w03-profile.data` and `w03-bad-frame.png` are not committed for this sample; students commit theirs next to this note.
