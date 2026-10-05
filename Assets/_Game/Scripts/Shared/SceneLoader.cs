using System;
using System.Diagnostics;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace MGD.Samples
{
    /// <summary>
    /// The one loader every scene change goes through. It lives on a
    /// DontDestroyOnLoad object created by the first scene, shows a loading screen
    /// whose bar follows the real load, refuses a second load while one runs, and
    /// logs how long each load took. The load holds at 90% with
    /// <c>allowSceneActivation = false</c> until the loop lets it go, which is the
    /// moment to finish anything the next scene needs (a pool prewarm, say). A pause
    /// that lands mid-load (Home, a call) is kept, so the new scene opens paused.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SceneLoader : MonoBehaviour
    {
        // AsyncOperation.progress stops here while activation is not allowed.
        const float ReadyProgress = 0.9f;

        [SerializeField] GameObject loadingScreen;
        [SerializeField] Slider bar;
        [SerializeField] TMP_Text percent;

        bool _loading;
        bool _pausedDuringLoad;

        /// <summary>The persistent loader. Null until the first scene has woken it.</summary>
        public static SceneLoader Instance { get; private set; }

        /// <summary>True from the start of a load until the new scene is active.</summary>
        public static bool IsLoading => Instance != null && Instance._loading;

        // With domain reloading off in the editor, statics survive between Play
        // sessions; this clears the stale reference before the first scene loads.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Instance = null;
        }

        void Awake()
        {
            // The first scene is loaded again every time the player goes back to it;
            // its copy of the loader must give way to the one already running.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            loadingScreen.SetActive(false);
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Loads the scene at this Build Settings index behind the loading screen,
        /// holding activation at 0.9 until the bar is full, and logs the time. Does
        /// nothing if a load is already running.
        /// </summary>
        public Awaitable Load(int buildIndex, CancellationToken ct)
        {
            return LoadAsync(null, buildIndex, ct);
        }

        /// <summary>
        /// Loads the scene by name, as the lab sheet's <c>Load</c> does: behind the
        /// loading screen, holding activation at 0.9 until the bar is full, then logs
        /// the time. Does nothing if a load is already running.
        /// </summary>
        public Awaitable Load(string sceneName, CancellationToken ct)
        {
            return LoadAsync(sceneName, -1, ct);
        }

        /// <summary>Load progress (0 to 0.9 before activation) as a 0 to 1 bar value.</summary>
        public static float BarValue(float progress)
        {
            return Mathf.Clamp01(progress / ReadyProgress);
        }

        // The lab sheet's Load. Exactly one of sceneName (when not null) and
        // buildIndex is used; the two public overloads above fill in the other.
        async Awaitable LoadAsync(string sceneName, int buildIndex, CancellationToken ct)
        {
            if (_loading)
            {
                return;
            }

            _loading = true;
            _pausedDuringLoad = false;
            LifecycleGuard.PausedChanged += OnPausedChanged;
            Stopwatch clock = Stopwatch.StartNew();
            ShowProgress(0f);
            loadingScreen.SetActive(true);
            try
            {
                AsyncOperation op = sceneName != null
                    ? SceneManager.LoadSceneAsync(sceneName)
                    : SceneManager.LoadSceneAsync(buildIndex);
                if (op == null)
                {
                    // LoadSceneAsync has already logged why (a scene not in Build Settings).
                    return;
                }

                op.allowSceneActivation = false;
                // Never `while (!op.isDone)`: isDone cannot turn true until activation
                // is allowed, so that loop never ends. The Loading sample shows it.
                while (op.progress < ReadyProgress)
                {
                    ShowProgress(BarValue(op.progress));
                    // NextFrameAsync counts frames, not seconds, so a pause
                    // (timeScale 0) does not stall the load.
                    await Awaitable.NextFrameAsync(ct);
                }

                ShowProgress(1f);
                op.allowSceneActivation = true;
                await op;

                // Not in the lab sheet: this pairs the loader with LifecycleGuard.
                // The outgoing scene's LifecycleGuard clears the pause when it is
                // destroyed. If Home, a call or the shade paused the game during the
                // load, the player is not looking, so put the pause back: the new
                // scene's PauseMenu shows its panel and resuming stays their choice.
                if (_pausedDuringLoad)
                {
                    LifecycleGuard.SetPaused(true);
                }

                Debug.Log($"[perf] scene {SceneManager.GetActiveScene().name} loaded in {clock.ElapsedMilliseconds} ms");
            }
            catch (OperationCanceledException)
            {
                // The app is quitting (or Play mode stopping); nothing to finish.
            }
            finally
            {
                LifecycleGuard.PausedChanged -= OnPausedChanged;
                _loading = false;
                loadingScreen.SetActive(false);
            }
        }

        // SetPaused raises no event when the game is already paused, so only a pause
        // that starts during the load sets the flag. A load started while paused
        // (back while paused) is the player leaving on purpose, and that pause clears.
        void OnPausedChanged(bool paused)
        {
            if (paused)
            {
                _pausedDuringLoad = true;
            }
        }

        void ShowProgress(float value)
        {
            bar.value = value;
            percent.SetText("{0}%", Mathf.Round(value * 100f));
        }
    }
}
