# Data-use statement: MGD Samples 0.2.0

Worked example of the Week 4 Lab B, Part B statement, written from the code and the Player Settings of this build rather than from intentions. Submit yours as `/docs/CA1/privacy-statement.md` and read it back against `dumpsys` before you do. Package `com.dftgames.mgdsamples`, sideloaded only; nothing is uploaded to Play this semester.

## Data collected

None. The app has no accounts, no analytics, no crash reporting and no advertising.

## Data stored on the device

PlayerPrefs only. The keys this project's code writes are listed below; Unity's engine may add a few `unity.*` keys of its own (session counters), which hold no personal data.

| Key | Written by | Meaning |
|-----|-----------|---------|
| `MGD.Haptics` | Accessibility | Haptics on or off |
| `MGD.TextScale` | Accessibility | Text-size factor |
| `MGD.ReduceMotion` | Accessibility | Reduce-motion on or off |
| `MGD.Lifecycle.TapCount` | Lifecycle demo | The demo counter saved on pause |

No files are written under `Application.persistentDataPath`. The Week 6 telemetry stub, when it arrives, will log to the local Unity log only. A player removes everything by uninstalling the app or clearing its storage in Android settings.

## Network activity

None. The code contains no `UnityWebRequest`, sockets or third-party network calls (checked with `grep -rn "UnityWebRequest\|HttpClient\|Socket" Assets/_Game`), and the built APK does not request `INTERNET`; see *Permissions requested* for why that took more than setting *Internet Access* to Auto.

## Third-party SDKs

None. Unity packages only (URP, Input System, TextMeshPro, 2D packages, Test Framework); none of them collects data in this configuration.

## Permissions requested

From `adb shell dumpsys package com.dftgames.mgdsamples | grep -A4 requested` on the installed release build:

```
(paste the dumpsys lines here after the device run)
```

From `aapt dump badging` on the built APK: `android.permission.VIBRATE`, added by Unity because the Accessibility sample calls `Handheld.Vibrate` (the one haptic pulse behind the Haptics toggle), and `com.dftgames.mgdsamples.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION`, an app-private permission Android's support library declares for its own receivers; neither reaches outside the app.

Worth knowing: the first build of 0.2.0 also requested `android.permission.INTERNET`, with Internet Access on Auto and every Unity service off, because Unity's engine adds it to every Android build. This app never opens a connection, so a build hook (`Assets/_Game/Editor/Release/StripInternetPermission.cs`, decision note *2026-09-28 Release builds strip the INTERNET permission*) deletes the line from release builds. Development builds keep it because the Profiler's Wi-Fi connection needs it, so `dumpsys` on a Development build will list `INTERNET`; this statement describes the release APK. If `INTERNET` ever appears in a release build, either the hook is gone or a sample started using the network, and this statement must change with it.

## How this would be declared on Play

- **Data safety form, data collection and security**: "No data collected"; encryption in transit not applicable; the developer provides no deletion mechanism beyond uninstall, which is the allowed answer when nothing is collected.
- **Data sharing**: none.
- **Privacy policy URL**: required by Play for every app regardless of collection; this page would be published at a public URL and linked from the listing.
- The form is required before release to closed, open or production tracks; the internal testing track is exempt, and sideloading needs none of it.
