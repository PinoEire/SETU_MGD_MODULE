# Feedback form

Worked example of the Week 8 Lab A, Part C form for this project. Submit yours as `/docs/CA3/qa/survey/form-link.md`. Build the form in Microsoft Forms (forms.office.com, SETU account) or Google Forms, with anonymous responses on and no names or emails collected, and keep it under three minutes.

Form link: https://forms.gle/jQPJpaKcuoKtJj6S8 (a Google Form). The About scene shows it and opens it with the *Open feedback form* button; it is set in `AboutSceneBuilder.FeedbackFormUrl`, so change it there and rebuild the scene for a new form.

## Fields, in this order

1. **Device model and Android version** (choice from `docs/device-matrix.md`, plus *Other* with text).
2. **Build ID shown on the About screen** (short text; hint: copy it from the About screen, do not type it from memory).
3. **Install** (choice: worked first time / needed help / failed). If failed, what did the phone say? (text).
4. **Minutes played** (number).
5. **Crash, freeze or "not responding" seen?** (choice: none / freeze / crash / not responding). When, and what were you doing? (text).
6. **Feel, 1 to 5** (rating grid): smoothness, heat, battery drain, control responsiveness.
7. **Loop, 1 to 5** (rating grid): I understood the goal; I wanted one more go; difficulty felt fair.
8. **One thing to fix** (text) and **One thing to keep** (text).
9. **Consent** (required checkbox): anonymous responses used for a university project; no personal data collected.

## Distribution

Each build that goes out gets a row in `releases/manifest.md` with its SHA-256 and build ID, and the APK on the GitHub Release for its tag. Testers check the file with `certutil -hashfile <apk> SHA256` (Windows) or `shasum -a 256 <apk>` (macOS, Linux) against the manifest, and quote the build ID from the About screen in the form, so every response maps to one binary. Share the download link as a QR code (Chrome's share menu makes one) and only with the closed test group.
