using System.Threading;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// The stand-in store: logs <c>[iap-stub]</c>, waits half a second where the
    /// store's dialog would be, and always succeeds. Nothing here talks to a store.
    /// </summary>
    public sealed class FakePurchaseProvider : IPurchaseProvider
    {
        public async Awaitable<bool> PurchaseAsync(string productId, CancellationToken ct)
        {
            Debug.Log($"[iap-stub] purchase {productId}");
            await Awaitable.WaitForSecondsAsync(0.5f, ct);
            return true; // A real SDK's result goes here.
        }
    }
}
