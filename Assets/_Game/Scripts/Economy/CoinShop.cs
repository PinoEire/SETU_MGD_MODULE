using System;
using UnityEngine;
using UnityEngine.UI;

namespace MGD.Samples
{
    /// <summary>
    /// The Buy 500 coins button. It asks an <see cref="IPurchaseProvider"/>, never a
    /// store, and grants the coins only when the purchase succeeds. Shown only
    /// between rounds: a shop that interrupts play is a dark pattern.
    /// </summary>
    public sealed class CoinShop : MonoBehaviour
    {
        public const string PackId = "coins_500";
        public const int PackCoins = 500;

        [SerializeField] Wallet wallet;
        [SerializeField] Button buyButton;

        readonly IPurchaseProvider _store = new FakePurchaseProvider();

        // async void is acceptable here only: a Button's OnClick cannot await.
        // Everywhere else returns Awaitable.
        public async void OnBuyPack()
        {
            buyButton.interactable = false; // one purchase at a time
            try
            {
                if (await _store.PurchaseAsync(PackId, destroyCancellationToken))
                {
                    wallet.Earn(PackCoins);
                }
            }
            catch (OperationCanceledException)
            {
                // The scene was left during the fake dialog; nothing to grant.
            }
            finally
            {
                if (buyButton != null)
                {
                    buyButton.interactable = true;
                }
            }
        }
    }
}
