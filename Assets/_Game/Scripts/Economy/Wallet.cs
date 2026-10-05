using System;
using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// A soft-currency wallet: coins earned by the core verb, spent on upgrades
    /// whose cost grows by half each level (100, 150, 225, 337, 506). Anything that
    /// shows or saves the wallet listens to <see cref="Changed"/> instead of
    /// polling it. A successful upgrade logs <c>upgrade_purchased</c>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Wallet : MonoBehaviour
    {
        const int BaseCost = 100;
        const float CostGrowth = 1.5f;
        const int BaseCoinsPerTap = 5;

        public int Coins { get; private set; }

        public int Level { get; private set; }

        /// <summary>Raised after coins or level change.</summary>
        public event Action Changed;

        /// <summary>What the next upgrade costs.</summary>
        public int NextCost => CostAt(Level);

        /// <summary>
        /// What the upgrade at this level costs. Worked out in double and capped at
        /// int.MaxValue, so a steeper curve or a very high level cannot wrap round to
        /// a negative price that every wallet could afford.
        /// </summary>
        public static int CostAt(int level)
        {
            double cost = BaseCost * Math.Pow(CostGrowth, level);
            return cost >= int.MaxValue ? int.MaxValue : (int)cost;
        }

        /// <summary>What one tap earns at this level: the upgrade this sample sells.</summary>
        public static int CoinsPerTap(int level)
        {
            // In long, capped, so a damaged save with a huge level cannot make a tap
            // worth a negative amount (which Earn would ignore).
            long coins = (long)BaseCoinsPerTap * ((long)level + 1);
            return coins >= int.MaxValue ? int.MaxValue : (int)coins;
        }

        /// <summary>
        /// Adds coins. Zero or negative amounts are ignored (spending goes through
        /// <see cref="TryUpgrade"/>), and the total stops at int.MaxValue instead of
        /// wrapping to a negative balance.
        /// </summary>
        public void Earn(int coins)
        {
            if (coins <= 0)
            {
                return;
            }

            Coins = coins > int.MaxValue - Coins ? int.MaxValue : Coins + coins;
            Changed?.Invoke();
        }

        /// <summary>Buys the next upgrade if the wallet can afford it; returns whether it did.</summary>
        public bool TryUpgrade()
        {
            // Read the cost before the level rises: afterwards NextCost is the
            // price of the upgrade after this one.
            int cost = NextCost;
            if (Coins < cost)
            {
                return false;
            }

            Coins -= cost;
            Level++;
            Telemetry.Log("upgrade_purchased",
                ("upgrade_id", "coins_per_tap"), ("level", Level), ("cost", cost), ("coins_left", Coins));
            Changed?.Invoke();
            return true;
        }

        /// <summary>Puts back saved values (see <see cref="WalletSave"/>). Negative values, from a damaged save, become 0.</summary>
        public void Restore(int coins, int level)
        {
            Coins = Mathf.Max(0, coins);
            Level = Mathf.Max(0, level);
            Changed?.Invoke();
        }
    }
}
