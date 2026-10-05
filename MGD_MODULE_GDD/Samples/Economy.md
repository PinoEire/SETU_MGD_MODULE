---
type: sample
status: done
lab: [W06-A, W06-B]
unity: 6000.6.0f1
scene: Assets/_Game/Scenes/Economy/Economy.unity
updated: 2026-10-05
tags: [sample, economy, telemetry, iap, persistence, w06]
---

# Economy

## Goal

Show the smallest economy loop a vertical slice needs (earn on the core verb, spend on an upgrade the player feels, keep it through a force-stop) together with the local [[Glossary#Telemetry stub|telemetry stub]] that explains it: five events in [[Glossary#Logcat|logcat]] and in a file, no SDK. The store is a fake behind an interface, so a real one could replace it with one new class and a one-line change in the shop, without touching gameplay. The core verb is tapping targets in a timed round, which fits any of the five project genres.

## Lab it supports

Week 6 Lab A, *Economy loop: earn, spend, upgrade and IAP awareness*: Parts A to D (`Wallet` and HUD, earn and upgrade in the loop, the store stub, persistence) and the numbers for Part E's pacing pass. Week 6 Lab B, *Telemetry hooks for balancing*: Parts A and B (`Telemetry`, `session_start`, the four gameplay events), Part C's event map as a worked example, and Part D's save and resume steps. Async Pack 1 is outside the project.

## What the student learns

- A soft-currency loop is three numbers: what a verb earns, what the next upgrade costs, and how much the upgrade changes play. Here a tap earns `5 × (level + 1)` coins and the next upgrade costs `100 × 1.5^level` (100, 150, 225, 337, 506), so the first upgrade lands inside the first round and doubles the earn rate, which the player feels at once.
- `TryUpgrade` reads the cost before raising the level; logging it afterwards reports the next upgrade's price, the bug Lab B warns about.
- The HUD listens to the wallet's `Changed` event and redraws with `SetText` and numbers; it never polls in `Update`.
- Gameplay depends on `IPurchaseProvider`, never on a store, so a real billing SDK is one new class plus the one line in `CoinShop` that creates the provider; the fake waits half a second, logs `[iap-stub]` and grants the coins. The shop appears only between rounds, never in the middle of play.
- `async void` is acceptable only for a UI event handler, because a Button cannot await; everything else returns [[Glossary#Awaitable|Awaitable]], and the awaited calls take a cancellation token (decision [[2026-10-04 Cancellation tokens where the awaited API takes one]]).
- PlayerPrefs survive a force-stop only if they were saved before it: save on every change and on `OnApplicationPause(true)`; `OnApplicationQuit` does not run when Android kills the process.
- Telemetry events have `snake_case` names and number or id parameters, are logged per event (never per frame, because building the line allocates), and go to logcat and to `telemetry.log` in `persistentDataPath`, which never leaves the device.
- `session_start` belongs to the first scene and fires once per app run, even though the first scene is loaded again every time the player goes back to it (decision [[2026-10-05 Telemetry is shared and session_start fires once per run]]).

## Files

| Path | Purpose |
|------|---------|
| `Assets/_Game/Scripts/Shared/Telemetry.cs` | The lab's static `Telemetry`. `Log(string ev, params (string k, object v)[] p)` writes `[telemetry] <time> <session> <event> k=v ...` to the console (logcat on the phone) and appends the line to `telemetry.log` in `Application.persistentDataPath`. The eight-character session id is made once per run. `Format(...)` builds the line and is pure, so it is tested. What students copy. |
| `Assets/_Game/Scripts/Shared/MobileBootstrap.cs` | Changed: logs `session_start` with `app_version`, `device_model` and `android`, and the existing `[Boot]` line, once per app run behind a static flag reset at `SubsystemRegistration` (the Launcher is reloaded on every back). |
| `Assets/_Game/Scripts/Economy/Wallet.cs` | The lab's `Wallet`: `Coins`, `Level`, `NextCost`, `Earn`, `TryUpgrade` (cost captured before `Level++`, then `upgrade_purchased` with `upgrade_id`, `level`, `cost`, `coins_left`), `Restore`, the `Changed` event, and the static pure `CoinsPerTap(level)`. On the `Economy` object in the scene. What students copy. |
| `Assets/_Game/Scripts/Economy/WalletHud.cs` | The lab's HUD: coins label, `Upgrade (cost)` label, Upgrade button interactable only when affordable; subscribes in `OnEnable`, unsubscribes in `OnDisable`, `SetText` with numbers. What students copy. |
| `Assets/_Game/Scripts/Economy/WalletSave.cs`, `WalletPersistence.cs` | The lab's save: keys `wallet.coins` and `wallet.level`, `PlayerPrefs.Save()` on every save; `WalletPersistence` loads in `Awake`, saves on `Changed` and on `OnApplicationPause(true)`. What students copy. |
| `Assets/_Game/Scripts/Economy/IPurchaseProvider.cs`, `FakePurchaseProvider.cs`, `CoinShop.cs` | The lab's store stub, with one change: `PurchaseAsync(string productId, CancellationToken ct)` takes a token because its wait accepts one. `CoinShop.OnBuyPack` is the one `async void`, a Button handler; it grants 500 coins when the purchase returns true. What students copy. |
| `Assets/_Game/Scripts/Economy/TapRound.cs` | Demo scaffolding for the round: Start shows three target buttons that jump to a new random spot in their own column of the play area when tapped (repositioned, never instantiated, never overlapping); each tap earns `Wallet.CoinsPerTap(level)` and adds to the score; a 20 s timer runs on `Awaitable.WaitForSecondsAsync` with a token and stops while paused. 15 taps before time runs out is `level_complete`, time running out is `level_fail` with `cause=timeout` (and leaving the scene mid-round is `level_fail` with `cause=quit`); `level_start` carries `level_id`, `attempt` (counted across the whole app run, as the lab's "attempts this session") and `coins`; `coins_earned` counts the coins the taps earned, so an upgrade bought mid-round does not distort it. The pure `Outcome(score, goal, secondsLeft)` decides it. Shows the shop and Start only between rounds; a Reset wallet button appears in development builds only, for the pacing pass. |
| `Assets/_Game/Scripts/Economy/README.md` | Two-paragraph summary for students who open the folder without this vault. |
| `Assets/_Game/Editor/Economy/EconomySceneBuilder.cs` | Menu item *MGD Samples > Build Economy Scene*: title, coins label, Upgrade button and status line at the top, the play area with three targets below them, and the Start round, Buy 500 coins and Reset wallet buttons at the bottom; `BackToLauncher` with its listener on, the pause menu; adds the scene to Build Settings after LoadingHeavy. |
| `Assets/_Game/Editor/Tests/WalletTests.cs`, `WalletSaveTests.cs`, `TelemetryTests.cs`, `TapRoundTests.cs` | EditMode: the cost sequence, `Earn`, `TryUpgrade` refused and accepted (with the logged cost checked), `Restore`, `Changed`, `CoinsPerTap`; a save and load round trip that backs up and restores the real keys; the telemetry line layout; the round outcome rule. |
| `Assets/_Game/Scenes/Economy/Economy.unity` | Generated; not hand-edited. |
| `docs/economy-telemetry-map.md` | The Lab B Part C event map for this sample, as a worked example of the CA2 deliverable. |
| `docs/privacy-statement.md` | Changed: the PlayerPrefs keys `wallet.coins` and `wallet.level` and the `telemetry.log` file under data stored on the device. |

## How to test

1. **Editor, session:** Play the Launcher. The console shows one `[telemetry] ... session_start` line. Go to any sample and back twice: still one.
2. **Editor, earning:** open Economy, tap Start: a `level_start` line with `attempt=1`; three targets appear and the timer counts down from 20. Each tap adds 5 coins and moves the target.
3. **Editor, round end:** reach 15 taps: `level_complete` with `time_s`, `coins_earned`, `score`; the shop and Start come back. Start again and stop tapping: after 20 s, `level_fail` with `cause=timeout`; `attempt=2` on that round's `level_start`.
4. **Editor, upgrade:** with 100 coins the Upgrade button lights up; tap it: `upgrade_purchased` with `level=1 cost=100`, the button now reads `Upgrade (150)`, and a tap earns 10.
5. **Editor, store:** between rounds tap Buy 500 coins: `[iap-stub] purchase coins_500`, half a second later the coins rise by 500. The button is hidden during a round.
6. **Editor, pause and persistence:** pause mid-round (focus loss): the timer stops; Resume continues it. Back to the Launcher and into Economy again: coins and level are as they were. Leave in the middle of a round: one `level_fail` with `cause=quit`.
7. **Phone, Lab B Part D:** with coins in the wallet, Home and relaunch, `am force-stop` and relaunch, Home then `am kill` and relaunch: coins survive each. `adb pull` the `telemetry.log` from the app's files folder: several session ids, every event in order.
8. **Phone, pacing:** Reset wallet (development build), stopwatch: the first upgrade lands inside two minutes.
9. **EditMode tests:** *MGD Samples > Run EditMode Tests*: all pass.

## Known limits

- The economy has one currency and one upgrade line; a real game has sinks, several upgrade tracks and a reason to spend that changes over time. That design is the students' job.
- Telemetry builds one string per event and appends to the file synchronously; fine for a handful of events per minute, wrong for anything per frame.
- `telemetry.log` grows without limit; a shipped game would rotate or cap it.
- The fake store always succeeds, and `CoinShop` creates it in its one `new FakePurchaseProvider()` line; exercising the failure path means swapping that line for a provider that returns false. The fake's half-second wait runs on scaled time, so a pause during it holds the purchase until Resume (a real store dialog takes focus, which pauses the game the same way).
- The wallet is saved on every change, as the lab sheet asks, which on Android is one synchronous PlayerPrefs flush per tap. Fine for this demo; a game with fast earning marks the wallet dirty and saves at the end of a round, after a purchase and on pause, so no tap pays for a disk write.
- A round left mid-way (back to the samples) logs `level_fail` with `cause=quit`; a round cut short because Android killed the process logs nothing, so a pulled log can show a `level_start` with no outcome.
- The round timer counts whole seconds, each a frame or so long, so a round lasts slightly over 20 s (`time_s=20.2` in the verified run).
- `WalletTests` logs a real `upgrade_purchased`, so running the tests adds a line to the editor's own `telemetry.log`.
- The wallet lives in this scene and persists through PlayerPrefs; a game whose wallet must be reachable from several scenes puts it on a persistent object, as the lab sheet does.
- Phone checks (steps 7 and 8) are not recorded until the device run.
- Verified 2026-10-05 in the editor: one `session_start` per run, also after returns to the Launcher; a won round logged `level_complete ... coins_earned=75 score=15`; a timed-out round logged `level_fail ... time_s=20.2 cause=timeout` with 6.7 s of pause in the middle not counted; Buy logged `[iap-stub] purchase coins_500`, granted 500 after the fake dialog, and was hidden during rounds; Upgrade logged `cost=100 coins_left=500` and a tap then earned 10; an upgrade bought mid-round left `coins_earned=125` against a wallet that rose by 25; leaving mid-round logged `level_fail ... cause=quit coins_earned=40`, leaving mid-purchase granted nothing, and neither raised an exception; coins and level came back after leaving and returning; `attempt` went on counting after a return; `telemetry.log` held the session's lines in order. 142 EditMode tests pass, 31 of them for this sample.

## Cuts list

1. The Reset wallet button could go, leaving the reset to *Edit > Clear All PlayerPrefs*; kept because the pacing pass is done on the phone, where there is no editor menu.
2. The file half of the telemetry could go, leaving logcat only; kept because Lab B's Part D pulls the file as evidence and a relaunch clears logcat's view of earlier sessions.
3. The jumping targets could become one fixed button; kept because a moving target is closer to a real core verb and keeps the round from being a tapping contest on one spot.

## Decisions

- [[2026-10-05 Telemetry is shared and session_start fires once per run]]
- [[2026-10-04 Cancellation tokens where the awaited API takes one]]
- [[2026-10-04 Shared scripts are the ones every scene needs]]
- [[2026-09-15 Back navigation returns to the launcher]]
- [[2026-10-04 Every scene pauses on focus loss]]

## References

- Lab sheets: `SETU/2026-2027/Mobile Game Development/Week 06/Lab A/W06-LabA-LabSheet.md` and `Week 06/Lab B/W06-LabB-LabSheet.md` (Dropbox).
- Decks: `A12581-W06-LabA-Economy-loop-earn-spend-upgrade-and-IAP-awareness.pptx`, `A12581-W06-LabB-Telemetry-hooks-for-balancing-and-Async-Pack-1.pptx`.
- Unity 6.6 `Awaitable<T>`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Awaitable_1.html
- Unity 6.6 `PlayerPrefs`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/PlayerPrefs.html
- Unity 6.6 `Application.persistentDataPath`: https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Application-persistentDataPath.html
