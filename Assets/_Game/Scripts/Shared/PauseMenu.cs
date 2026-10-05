using UnityEngine;
using UnityEngine.InputSystem;

namespace MGD.Samples
{
    /// <summary>
    /// Shows the pause panel whenever <see cref="LifecycleGuard"/> pauses, and, where
    /// the scene owns the back gesture, lets Android back toggle the pause. Back
    /// arrives in the Input System as the Escape key; in a scene where back already
    /// returns to the launcher, <see cref="backTogglesPause"/> is off so the two do
    /// not fight over one key. There is no Quit button: mobile games do not have
    /// one. The pause state is static on the guard, so this needs no reference to it.
    /// </summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] bool backTogglesPause = true;

        void OnEnable()
        {
            LifecycleGuard.PausedChanged += Show;
            Show(LifecycleGuard.IsPaused);
        }

        void OnDisable()
        {
            LifecycleGuard.PausedChanged -= Show;
        }

        void Show(bool paused)
        {
            // Toggle the panel itself, not a parent: an inactive parent would also
            // stop this component from receiving the event.
            panel.SetActive(paused);
        }

        void Update()
        {
            // No pausing mid-load: the loading screen covers the panel anyway.
            if (!backTogglesPause || SceneLoader.IsLoading)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            LifecycleGuard.SetPaused(!LifecycleGuard.IsPaused);
        }

        /// <summary>Wired to the Resume button's OnClick.</summary>
        public void OnResumePressed()
        {
            LifecycleGuard.SetPaused(false);
        }
    }
}
