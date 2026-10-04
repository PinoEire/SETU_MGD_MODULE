# Store-asset checklist

Worked example of the Week 4 Lab B, Part A checklist, written for this samples app as if it were going to Play. Submit yours as `/docs/CA1/store-assets-checklist.md`. Sizes are from the Play Console help page *Add preview assets* (https://support.google.com/googleplay/android-developer/answer/9866151); confirm them on the day you submit, because they change. Nothing is uploaded this semester.

| Asset | Spec | Status | Notes |
|-------|------|--------|-------|
| App icon | 512 x 512, 32-bit PNG with alpha, up to 1024 KB | planned | No rounded corners or drop shadow: Play masks the icon itself. |
| Feature graphic | 1024 x 500, JPEG or 24-bit PNG, no alpha | planned | Shown above a listing video; keep text out of the outer 10 per cent. |
| Phone screenshots | 2 to 8; each side 320 to 3840 px, the long side at most twice the short side; JPEG or 24-bit PNG, no alpha. For promotion on Play: at least four at 1080 px minimum, 16:9 or 9:16 | planned | Portrait 9:16 at 1080 x 1920 or more, captured on device at 1080 x 2400 with `adb shell screencap` and cropped to 9:16 (the 2400-tall raw capture is 20:9 and fails the ratio). |
| 7-inch tablet screenshots | at least 4 if declared; each side 1080 to 7680 px; 16:9 or 9:16 | not planned | Portrait phone layout only; declare no tablet support. |
| Promo video | YouTube link, public or unlisted | not planned | |

## Screenshot plan

1. **Launcher**: the sample list with the build-info footer visible. Caption: "One app, every mobile good practice from the module."
2. **SafeArea**: the HUD sitting inside the notch and gesture bar on the test phone. Caption: "Nothing under the notch, on any phone."
3. **Lifecycle**: the pause card over the running clocks. Caption: "Home, calls and screen-off pause the game; the player resumes."
4. **Accessibility**: the Settings card with Large text and the 48 dp square. Caption: "Haptics, text size and reduce motion from the first build."
5. **Performance**: the Worst state with the `[Baseline]` line readable. Caption: "Measure frame time, not FPS."
6. **TouchDrag**: two fingers on two circles mid-drag, the status line naming both. Caption: "Touch that works the way thumbs do."

Sources in Week 10: in-game capture at device resolution for shots 1 to 6; icon and feature graphic from the module's key art (not yet made).
