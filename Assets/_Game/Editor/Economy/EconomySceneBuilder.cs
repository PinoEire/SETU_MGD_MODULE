using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MGD.Samples.Editor.SampleSceneBuild;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// Builds the Economy sample scene from a menu item. Every run assigns fresh
    /// object IDs, so run it only when this builder changes.
    /// </summary>
    public static class EconomySceneBuilder
    {
        const string BuilderName = "EconomySceneBuilder";
        const string ScenePath = "Assets/_Game/Scenes/Economy/Economy.unity";

        // 180 units is about 68 dp on the reference phone: a comfortable target.
        static readonly Vector2 TargetSize = new Vector2(180f, 180f);
        static readonly Vector2 ButtonSize = new Vector2(560f, 140f);
        const int TargetCount = 3;

        [MenuItem("MGD Samples/Build Economy Scene")]
        public static void Build()
        {
            Scene? scene = BeginScene(BuilderName);
            if (scene == null)
            {
                return;
            }

            var economy = new GameObject("Economy", typeof(Wallet), typeof(WalletPersistence));
            var wallet = economy.GetComponent<Wallet>();

            Canvas canvas = CreateCanvas();
            RectTransform root = CreateHudRoot(canvas);

            TextMeshProUGUI title = CreateLabel(root, "Title", "Economy sample", 56f);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 100f), stretchWidth: true);

            TextMeshProUGUI coins = CreateLabel(root, "Coins", "", 48f);
            Place(coins.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 80f), stretchWidth: true, offset: new Vector2(0f, -110f));

            Button upgrade = CreateButton(root, "Button Upgrade", "Upgrade (100)", ButtonSize, 44f);
            Place(upgrade.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), ButtonSize, offset: new Vector2(0f, -210f));

            TextMeshProUGUI status = CreateLabel(root, "Status", "", 36f);
            Place(status.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 110f), stretchWidth: true, offset: new Vector2(0f, -370f));

            // The play area fills the middle; the between-rounds panel sits below it.
            RectTransform playArea = CreateUiObject("Play Area", root);
            playArea.anchorMin = new Vector2(0f, 0f);
            playArea.anchorMax = new Vector2(1f, 1f);
            playArea.offsetMin = new Vector2(0f, 560f);
            playArea.offsetMax = new Vector2(0f, -500f);
            var targets = new Button[TargetCount];
            for (int i = 0; i < TargetCount; i++)
            {
                targets[i] = CreateButton(playArea, $"Target {i + 1}", "Tap", TargetSize, 44f);
                Place(targets[i].GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), TargetSize);
            }

            RectTransform between = CreateUiObject("Between Rounds", root);
            Place(between, new Vector2(0.5f, 0f), new Vector2(0f, 520f), stretchWidth: true);
            Button start = CreateButton(between, "Button Start", "Start round", ButtonSize, 44f);
            Place(start.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), ButtonSize);
            Button buy = CreateButton(between, "Button Buy", "Buy 500 coins", ButtonSize, 44f);
            Place(buy.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), ButtonSize, offset: new Vector2(0f, -170f));
            Button reset = CreateButton(between, "Button Reset", "Reset wallet (dev)", ButtonSize, 36f);
            Place(reset.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), ButtonSize, offset: new Vector2(0f, -340f));

            var hud = root.gameObject.AddComponent<WalletHud>();
            SetField(hud, "wallet", wallet);
            SetField(hud, "coinsLabel", coins);
            SetField(hud, "upgradeLabel", upgrade.GetComponentInChildren<TextMeshProUGUI>());
            SetField(hud, "upgradeButton", upgrade);

            var shop = root.gameObject.AddComponent<CoinShop>();
            SetField(shop, "wallet", wallet);
            SetField(shop, "buyButton", buy);

            var round = root.gameObject.AddComponent<TapRound>();
            SetField(round, "wallet", wallet);
            SetField(round, "playArea", playArea);
            SetField(round, "targets", targets);
            SetField(round, "betweenRounds", between.gameObject);
            SetField(round, "resetButton", reset.gameObject);
            SetField(round, "status", status);

            // Persistent listeners: what a student wires by hand in the Inspector.
            UnityEventTools.AddPersistentListener(upgrade.onClick, new UnityAction(hud.OnUpgradePressed));
            UnityEventTools.AddPersistentListener(buy.onClick, new UnityAction(shop.OnBuyPack));
            UnityEventTools.AddPersistentListener(start.onClick, new UnityAction(round.OnStartPressed));
            UnityEventTools.AddPersistentListener(reset.onClick, new UnityAction(round.OnResetPressed));
            foreach (Button target in targets)
            {
                UnityEventTools.AddObjectPersistentListener(target.onClick, new UnityAction<Button>(round.OnTargetPressed), target);
            }

            // Android back returns to the launcher; this sample does not use back
            // itself, so the pause panel's own back toggle is off.
            canvas.gameObject.AddComponent<BackToLauncher>();
            AddPauseMenu(canvas, backTogglesPause: false, addSamplesButton: false);

            FinishScene(scene.Value, ScenePath, BuilderName);
        }
    }
}
