using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MGD.Samples
{
    /// <summary>
    /// Returns to the launcher scene (build index 0). With <see cref="listenForBack"/>
    /// on, the Android back gesture (delivered by the Input System as Escape) does it;
    /// samples that use back for something else, such as pausing, turn the listener
    /// off and wire a button to <see cref="Go"/> instead. While a load is running,
    /// back and Go() do nothing.
    /// </summary>
    public sealed class BackToLauncher : MonoBehaviour
    {
        const int LauncherBuildIndex = 0;

        [SerializeField] bool listenForBack = true;

        bool _loading;

        void Update()
        {
            if (!listenForBack || _loading || SceneLoader.IsLoading)
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
            if (_loading || SceneLoader.IsLoading)
            {
                return;
            }

            _loading = true;
            _ = LoadLauncherAsync();
        }

        // Through the shared SceneLoader. A scene opened directly in the editor has
        // no loader (the Launcher creates it), so it falls back to a plain load.
        async Awaitable LoadLauncherAsync()
        {
            if (SceneLoader.Instance != null)
            {
                await SceneLoader.Instance.Load(LauncherBuildIndex, Application.exitCancellationToken);
                return;
            }

            Debug.Log("[BackToLauncher] No SceneLoader (this scene was opened directly), so the launcher loads without the loading screen.");
            await SceneManager.LoadSceneAsync(LauncherBuildIndex);
        }
    }
}
