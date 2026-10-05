using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MGD.Samples
{
    /// <summary>
    /// Shows the wallet and sells the upgrade. Redraws only when the wallet raises
    /// <c>Changed</c>, never in Update, and with SetText and numbers, so it builds
    /// no string in a player build.
    /// </summary>
    public sealed class WalletHud : MonoBehaviour
    {
        [SerializeField] Wallet wallet;
        [SerializeField] TMP_Text coinsLabel;
        [SerializeField] TMP_Text upgradeLabel;
        [SerializeField] Button upgradeButton;

        void OnEnable()
        {
            wallet.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            wallet.Changed -= Refresh;
        }

        /// <summary>Wired to the Upgrade button's OnClick. The redraw arrives through Changed.</summary>
        public void OnUpgradePressed()
        {
            wallet.TryUpgrade();
        }

        void Refresh()
        {
            // SetText's number overloads take floats; the casts pick them over the
            // ReadOnlySpan<char> ones, which would otherwise be ambiguous with ints.
            coinsLabel.SetText("{0} coins   {1} per tap", (float)wallet.Coins, (float)Wallet.CoinsPerTap(wallet.Level));
            upgradeLabel.SetText("Upgrade ({0})", (float)wallet.NextCost);
            upgradeButton.interactable = wallet.Coins >= wallet.NextCost;
        }
    }
}
