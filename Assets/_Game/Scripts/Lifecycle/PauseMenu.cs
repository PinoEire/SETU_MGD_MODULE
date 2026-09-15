using UnityEngine;
using UnityEngine.InputSystem;

namespace MGD.Samples
{
    /// <summary>
    /// Shows the pause panel whenever <see cref="LifecycleGuard"/> pauses, and lets
    /// the Android back gesture toggle the pause. Back arrives in the Input System
    /// as the Escape key. There is no Quit button: mobile games do not have one.
    /// </summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] LifecycleGuard guard;

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
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            guard.SetPaused(!LifecycleGuard.IsPaused);
        }

        /// <summary>Wired to the Resume button's OnClick.</summary>
        public void OnResumePressed()
        {
            guard.SetPaused(false);
        }
    }
}
