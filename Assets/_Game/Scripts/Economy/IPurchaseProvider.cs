using System.Threading;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// What gameplay knows about a store. A real billing SDK goes behind this
    /// interface; only the line that creates it changes. Returns true when the
    /// purchase succeeded. Unlike the lab sheet's version it takes a token, because
    /// the wait inside accepts one (decision: cancellation tokens where the awaited
    /// API takes one).
    /// </summary>
    public interface IPurchaseProvider
    {
        Awaitable<bool> PurchaseAsync(string productId, CancellationToken ct);
    }
}
