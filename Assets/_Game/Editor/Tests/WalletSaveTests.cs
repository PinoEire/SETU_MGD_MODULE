using NUnit.Framework;
using UnityEngine;

namespace MGD.Samples.Editor
{
    // Uses the real PlayerPrefs keys, so it backs them up and puts them back:
    // running the tests must not reset the wallet you were testing by hand.
    public class WalletSaveTests
    {
        GameObject _go;
        bool _hadCoins;
        bool _hadLevel;
        int _coins;
        int _level;

        [SetUp]
        public void SetUp()
        {
            _hadCoins = PlayerPrefs.HasKey(WalletSave.CoinsKey);
            _hadLevel = PlayerPrefs.HasKey(WalletSave.LevelKey);
            _coins = PlayerPrefs.GetInt(WalletSave.CoinsKey);
            _level = PlayerPrefs.GetInt(WalletSave.LevelKey);
            _go = new GameObject("Wallet");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            Put(WalletSave.CoinsKey, _hadCoins, _coins);
            Put(WalletSave.LevelKey, _hadLevel, _level);
            PlayerPrefs.Save();
        }

        static void Put(string key, bool had, int value)
        {
            if (had)
            {
                PlayerPrefs.SetInt(key, value);
            }
            else
            {
                PlayerPrefs.DeleteKey(key);
            }
        }

        [Test]
        public void SaveThenLoad_RestoresCoinsAndLevel()
        {
            var saved = _go.AddComponent<Wallet>();
            saved.Restore(123, 4);
            WalletSave.Save(saved);

            var loaded = new GameObject("Loaded").AddComponent<Wallet>();
            try
            {
                WalletSave.Load(loaded);

                Assert.AreEqual(123, loaded.Coins);
                Assert.AreEqual(4, loaded.Level);
            }
            finally
            {
                Object.DestroyImmediate(loaded.gameObject);
            }
        }

        [Test]
        public void Load_WithNothingSaved_StartsAtZero()
        {
            PlayerPrefs.DeleteKey(WalletSave.CoinsKey);
            PlayerPrefs.DeleteKey(WalletSave.LevelKey);
            var wallet = _go.AddComponent<Wallet>();

            WalletSave.Load(wallet);

            Assert.AreEqual(0, wallet.Coins);
            Assert.AreEqual(0, wallet.Level);
        }
    }
}
