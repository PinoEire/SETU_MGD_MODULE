using UnityEngine;

namespace MGD.Samples
{
    /// <summary>
    /// Loads the wallet when the scene starts and saves it on every change and when
    /// the app goes to the background. <c>OnApplicationQuit</c> is not used: Android
    /// often kills the process without calling it.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Wallet))]
    public sealed class WalletPersistence : MonoBehaviour
    {
        Wallet _wallet;

        void Awake()
        {
            _wallet = GetComponent<Wallet>();
            WalletSave.Load(_wallet);
            // Subscribed after the load, so loading does not write straight back.
            _wallet.Changed += Save;
        }

        void OnDestroy()
        {
            if (_wallet != null)
            {
                _wallet.Changed -= Save;
            }
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                Save();
            }
        }

        void Save()
        {
            WalletSave.Save(_wallet);
        }
    }
}
