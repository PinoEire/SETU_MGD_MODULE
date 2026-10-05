using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MGD.Samples
{
    /// <summary>
    /// Demo scaffolding for the Loading scene. "Load heavy scene" goes through the
    /// shared <see cref="SceneLoader"/>, the right way. "Load with the bug" runs the
    /// classic broken loop on purpose so you can see what an infinite loading
    /// screen looks like: frames keep ticking, the bar is full (the same
    /// progress / 0.9 bar as the loader), the label shows progress stuck at 0.90,
    /// and nothing will ever change it. A watchdog explains and then lets the load
    /// finish, which a real game must never need. Do not copy the bug loop.
    /// </summary>
    public sealed class LoadingDemoHud : MonoBehaviour
    {
        const float ReadyProgress = 0.9f;

        [SerializeField] string heavyScene = "LoadingHeavy";
        [SerializeField] Button heavyButton;
        [SerializeField] Button bugButton;
        [SerializeField] GameObject demoPanel;
        [SerializeField] Slider demoBar;
        [SerializeField] TMP_Text demoLabel;
        [SerializeField] BackToLauncher back;

        [Tooltip("Seconds at progress 0.9 before the watchdog explains.")]
        [SerializeField, Min(0.5f)] float stuckLimit = 3f;

        [Tooltip("Seconds the explanation stays up before the load is let go.")]
        [SerializeField, Min(0f)] float messageSeconds = 2f;

        /// <summary>True once the load has sat at progress 0.9 for <paramref name="limit"/> seconds.</summary>
        public static bool IsStuck(float progress, float secondsHeld, float limit)
        {
            return progress >= ReadyProgress && secondsHeld >= limit;
        }

        void Start()
        {
            demoPanel.SetActive(false);
        }

        public void OnHeavyPressed()
        {
            if (SceneLoader.Instance == null)
            {
                Debug.LogWarning("[LoadingDemoHud] No SceneLoader: start from the Launcher scene.", this);
                return;
            }

            SetButtons(false);
            // The exit token, not destroyCancellationToken: this scene is unloaded by
            // the very load it starts, and the loader must carry on past that.
            _ = SceneLoader.Instance.Load(heavyScene, Application.exitCancellationToken);
        }

        public void OnBugPressed()
        {
            SetButtons(false);
            // Back mid-load would start a second load on top of the stuck one.
            back.enabled = false;
            _ = LoadWithIsDoneBugAsync(destroyCancellationToken);
        }

        // THE BUG, on purpose. With allowSceneActivation false, progress stops at 0.9
        // and isDone never turns true, so `while (!op.isDone)` never ends. Compare the
        // loop in SceneLoader, which waits for progress 0.9 instead.
        async Awaitable LoadWithIsDoneBugAsync(CancellationToken ct)
        {
            demoPanel.SetActive(true);
            AsyncOperation op = SceneManager.LoadSceneAsync(heavyScene);
            op.allowSceneActivation = false;
            float heldSince = -1f;
            float releaseAt = float.MaxValue;
            try
            {
                while (!op.isDone)
                {
                    float now = Time.realtimeSinceStartup;
                    // The same bar as SceneLoader: full at progress 0.9, which is
                    // exactly what the lab sheet's broken loop shows students.
                    demoBar.value = SceneLoader.BarValue(op.progress);
                    if (releaseAt == float.MaxValue)
                    {
                        demoLabel.SetText("progress {0:2}   isDone false", op.progress);
                    }

                    if (op.progress >= ReadyProgress && heldSince < 0f)
                    {
                        heldSince = now;
                    }

                    if (releaseAt == float.MaxValue && IsStuck(op.progress, heldSince < 0f ? -1f : now - heldSince, stuckLimit))
                    {
                        releaseAt = now + messageSeconds;
                        demoLabel.text = "Stuck at progress 0.9: isDone never turns true while allowSceneActivation is false.";
                    }

                    if (now >= releaseAt)
                    {
                        // The watchdog lets go. A real game never gets here.
                        op.allowSceneActivation = true;
                    }

                    await Awaitable.NextFrameAsync(ct);
                }
            }
            catch (OperationCanceledException)
            {
                // This scene was unloaded by the load finishing: the normal way out.
            }
        }

        void SetButtons(bool interactable)
        {
            heavyButton.interactable = interactable;
            bugButton.interactable = interactable;
        }
    }
}
