using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MGD.Samples
{
    /// <summary>
    /// Returns to the launcher scene (build index 0). With <see cref="listenForBack"/>
    /// on, the Android back gesture (delivered by the Input System as Escape) does it;
    /// samples that use back for something else, such as pausing, turn the listener
    /// off and wire a button to <see cref="Go"/> instead.
    /// </summary>
    public sealed class BackToLauncher : MonoBehaviour
    {
        const int LauncherBuildIndex = 0;

        [SerializeField] bool listenForBack = true;

        bool _loading;

        void Update()
        {
            if (!listenForBack || _loading)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            Go();
        }

        /// <summary>Wired to a button's OnClick, or called from code.</summary>
        public void Go()
        {
            if (_loading)
            {
                return;
            }

            _loading = true;
            _ = LoadLauncherAsync();
        }

        // Awaitable, not a coroutine: the module's rule for async work. Any exception
        // surfaces in the console instead of being swallowed.
        async Awaitable LoadLauncherAsync()
        {
            await SceneManager.LoadSceneAsync(LauncherBuildIndex);
        }
    }
}
