using UnityEngine;

namespace MGD.Samples
{
    /// <summary>The wallet in PlayerPrefs. <c>Save</c> flushes to disk at once, so a force-stop straight after keeps it.</summary>
    public static class WalletSave
    {
        public const string CoinsKey = "wallet.coins";
        public const string LevelKey = "wallet.level";

        public static void Save(Wallet wallet)
        {
            PlayerPrefs.SetInt(CoinsKey, wallet.Coins);
            PlayerPrefs.SetInt(LevelKey, wallet.Level);
            PlayerPrefs.Save();
        }

        public static void Load(Wallet wallet)
        {
            wallet.Restore(PlayerPrefs.GetInt(CoinsKey, 0), PlayerPrefs.GetInt(LevelKey, 0));
        }
    }
}
