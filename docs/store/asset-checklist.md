# Store asset checklist

Worked example of the Week 10 Lab A, Part D checklist for MGD Samples 0.6.0. Submit yours as `/docs/store/asset-checklist.md`. Specs checked against the Play Console help page *Add preview assets* (https://support.google.com/googleplay/android-developer/answer/9866151) on 8 Nov 2026; check them again on the day you submit, because they change. Nothing is uploaded this semester. The Week 4 plan is `docs/store-assets-checklist.md`.

| File | Required spec | Actual | Tick |
|------|---------------|--------|------|
| `icon-512.png` (the Lab A icon: version A) | 512x512, 32-bit PNG (alpha allowed), up to 1024 KB | 512x512, 32-bit PNG, 10 KB | yes |
| `icon-512-A.png`, `icon-512-B.png` (the Lab B pair) | as above | 512x512, 32-bit PNG, 10 KB and 9 KB | yes |
| `feature-1024x500.png` | 1024x500, JPEG or 24-bit PNG, no alpha; focal point and key elements towards the centre, background towards the edges | 1024x500, 24-bit PNG, no alpha, 41 KB; content in about x 134 to 890, y 97 to 410 | yes |
| `screenshots/01-economy-round.png` | JPEG or 24-bit PNG, no alpha; each side 320 to 3840 px, long side at most twice the short; for promotion 9:16 at 1080x1920 or more | 1080x1920, 24-bit PNG | yes |
| `screenshots/02-device-tiers.png` | as above | 1080x1920, 24-bit PNG | yes |
| `screenshots/03-thermal-overlay.png` | as above | 1080x1920, 24-bit PNG | yes |
| `screenshots/04-sample-list.png` | as above | 1080x1920, 24-bit PNG | yes |
| `screenshots/05-about-build-stamp.png` | as above | 1080x1920, 24-bit PNG | yes |
| Phone screenshot count | up to 8 per device type, at least 2 in total; for promotion, apps at least four | 5 | yes |

## Icon

One silhouette, one focal colour: a white phone with a check mark cut into it, inside the central 80%, no text, no thin lines, corners left square because Play masks the icon itself. A and B differ only in background colour (blue and orange), the one variable Lab B votes on; `icon-512.png` is version A under the name Lab A asks for, and `icon-48-A.png` and `icon-48-B.png` are the launcher-size previews. The art is flat shapes drawn by a script, a stand-in until real key art exists.

## Feature graphic

Built to the Play help page's guidance:

- **Focal point towards the centre.** The app name, a one-line tagline, and the Economy round drawn large below them (three tap targets, the tapped one lifted with "+35"), as one group in the middle (about x 134 to 890, y 97 to 410).
- **Background elements towards the edges.** Soft circles in the corners only; nothing that matters near an edge, where some formats crop.
- **The experience, not the icon, and no device.** The page asks for graphics that show the app or game, and advises against branding that repeats the icon and against device imagery, so the hero is the tap round itself, drawn large, with no phone frame and nothing like the check-mark phone of the icon.
- **Vibrant colour.** A teal to blue gradient: the page says pure white, black and dark grey blend into Play's own background.
- **No fine detail.** Large type and large shapes; a pasted screenshot's small print would disappear on a phone.

Play overlays a play button on the graphic when the listing has a video that does not autoplay; it does not say where, so the composition keeps the subject large enough to read around it.

## Screenshots

Captured on a Pixel 7a (0.6.0, Development Build) with the StoreShot button, which hides itself for the captured frame. Two things needed fixing before the files met the spec, and both will happen to you:

1. **The "Development Build" watermark.** Unity draws it in the bottom-right corner of every Development Build, and the capture includes it. It was painted over with the background colour; capturing from a release build would avoid it, but the Shot button exists only in Development Builds.
2. **The 20:9 screen.** The phone captures 1080x2400, a long side 2.22 times the short one, which Play refuses (at most twice) and which is not the 9:16 promotion needs. Each capture was scaled to 1920 tall and padded to 1080x1920 with the scene's own background colour, which keeps the whole screen; cropping 480 px off would have cut the title or the controls on most of these screens.

Order: the screen where something happens first (a timed round), then what is special about this app (tiers, the thermal overlay), the menu last but one. The SafeArea capture was left out: its content runs into all four corners, so padding it made it the least readable. Scaling to 0.8 softens fine detail: in `02-device-tiers.png` the checker texture's fine pattern is blurred by the resize, not by the tier, so a screenshot meant to show texture resolution would need a crop at full size instead.

## Screenshots for games

These five are portrait 9:16. For Play's promotion formats an app needs at least four at 1080 px or more, which these are; a game needs at least three 16:9 landscape shots (1920x1080 or more) or three 9:16 portrait shots (1080x1920 or more), showing the in-game experience, in the game's own orientation.
