using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MGD.Samples.Editor
{
    public class WalletTests
    {
        GameObject _go;
        Wallet _wallet;
        int _changed;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("Wallet");
            _wallet = _go.AddComponent<Wallet>();
            _changed = 0;
            _wallet.Changed += () => _changed++;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [TestCase(0, 100)]
        [TestCase(1, 150)]
        [TestCase(2, 225)]
        [TestCase(3, 337)]
        [TestCase(4, 506)]
        public void CostAt_FollowsTheLabCurve(int level, int cost)
        {
            Assert.AreEqual(cost, Wallet.CostAt(level));
        }

        [Test]
        public void CostAt_SaturatesInsteadOfOverflowing()
        {
            Assert.AreEqual(int.MaxValue, Wallet.CostAt(100));
        }

        [Test]
        public void Earn_SaturatesInsteadOfWrapping()
        {
            _wallet.Restore(int.MaxValue - 10, 0);

            _wallet.Earn(500);

            Assert.AreEqual(int.MaxValue, _wallet.Coins);
        }

        [TestCase(0, 5)]
        [TestCase(1, 10)]
        [TestCase(2, 15)]
        public void CoinsPerTap_GrowsWithLevel(int level, int coins)
        {
            Assert.AreEqual(coins, Wallet.CoinsPerTap(level));
        }

        [Test]
        public void Earn_AddsCoinsAndRaisesChanged()
        {
            _wallet.Earn(5);

            Assert.AreEqual(5, _wallet.Coins);
            Assert.AreEqual(1, _changed);
        }

        [Test]
        public void Earn_IgnoresZeroAndNegative()
        {
            _wallet.Earn(0);
            _wallet.Earn(-5);

            Assert.AreEqual(0, _wallet.Coins);
            Assert.AreEqual(0, _changed);
        }

        [Test]
        public void TryUpgrade_WithoutEnoughCoins_ChangesNothing()
        {
            _wallet.Restore(99, 0);
            _changed = 0;

            Assert.IsFalse(_wallet.TryUpgrade());
            Assert.AreEqual(99, _wallet.Coins);
            Assert.AreEqual(0, _wallet.Level);
            Assert.AreEqual(0, _changed);
        }

        [Test]
        public void TryUpgrade_WithEnough_SpendsTheCostBeforeTheLevelRises()
        {
            _wallet.Restore(250, 0);
            LogAssert.Expect(LogType.Log,
                new Regex(@"\[telemetry\] .* upgrade_purchased upgrade_id=coins_per_tap level=1 cost=100 coins_left=150$"));

            Assert.IsTrue(_wallet.TryUpgrade());
            Assert.AreEqual(150, _wallet.Coins);
            Assert.AreEqual(1, _wallet.Level);
            Assert.AreEqual(150, _wallet.NextCost);
        }

        [Test]
        public void Restore_SetsBothValuesAndRaisesChanged()
        {
            _wallet.Restore(42, 3);

            Assert.AreEqual(42, _wallet.Coins);
            Assert.AreEqual(3, _wallet.Level);
            Assert.AreEqual(1, _changed);
        }

        [Test]
        public void Restore_ClampsNegativeValuesToZero()
        {
            _wallet.Restore(-10, -2);

            Assert.AreEqual(0, _wallet.Coins);
            Assert.AreEqual(0, _wallet.Level);
        }
    }
}
