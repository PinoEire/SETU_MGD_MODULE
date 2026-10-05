# Economy and telemetry map: MGD Samples

Worked example of the Week 6 Lab B, Part C event map for the Economy sample. Submit yours as `/docs/CA2/Economy_Telemetry_Map.md`; keep the names here and in the code identical.

| Event | Parameters (type) | Fires when | Question it answers |
|-------|-------------------|------------|---------------------|
| `session_start` | `app_version` (string), `device_model` (string), `android` (string) | once per app run, in the Launcher's bootstrap | which build and device produced this log |
| `level_start` | `level_id` (int), `attempt` (int, counted across the run), `coins` (int) | Start round is tapped | how often players retry; how rich they are when they start |
| `level_complete` | `level_id` (int), `time_s` (float), `coins_earned` (int), `score` (int) | the 15th tap lands inside 20 s | run length and earn rate on success |
| `level_fail` | `level_id` (int), `time_s` (float), `cause` (id: `timeout` or `quit`), `coins_earned` (int) | the timer reaches 0 first, or the scene is left mid-round | where and why players lose or leave; earn rate on failure |
| `upgrade_purchased` | `upgrade_id` (id: `coins_per_tap`), `level` (int), `cost` (int), `coins_left` (int) | inside `Wallet.TryUpgrade` on success | when the first upgrade lands; spend pacing |

Every line is `<seconds since start> <session id> <event> key=value ...`, with a dot for decimals whatever the phone's language and any whitespace in values (spaces, tabs, line breaks) replaced by `_`. The data goes to logcat (`adb logcat -s Unity | grep telemetry`) and to `telemetry.log` in the app's `persistentDataPath` (`adb pull /sdcard/Android/data/com.dftgames.mgdsamples/files/telemetry.log`). Nothing leaves the device; `docs/privacy-statement.md` says the same. A round cut short because Android killed the app logs no outcome, so count `level_start` lines without a matching end as kills.
