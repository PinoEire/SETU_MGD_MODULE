using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MGD.Samples
{
    /// <summary>
    /// The Week 10 Lab A store-screenshot capture: hide what must not be in the
    /// shot, wait a frame, capture at device resolution (or a multiple of it), wait
    /// until the file is on disk, then show everything again. Files go to
    /// <c>Application.persistentDataPath</c> as <c>Scene_NN_WxH.png</c>; pull them
    /// with <c>adb pull /sdcard/Android/data/com.dftgames.mgdsamples/files/</c>.
    ///
    /// It differs from the deck's version mainly in four ways. The Launcher
    /// creates it once on a DontDestroyOnLoad object with its own small overlay
    /// canvas, so the Shot button is there in every scene; the button shows in
    /// Development Builds only, as the deck's <c>DebugShotButton</c> does. The lab
    /// removes both scripts in Week 12; here release builds keep the object with
    /// the button switched off. A game hides its HUD root, but in this
    /// app the UI is the content, so <see cref="hideWhileCapturing"/> (a list, where
    /// the deck has one <c>hudRoot</c>) holds only the Shot button's own canvas. It
    /// waits until the file is complete, in real time, rather than for 0.5 s of game
    /// time, which never passes while the game is paused, and it deletes an older
    /// file of the same name first. And the file name carries a shot number
    /// (<c>Scene_NN_WxH.png</c>) and the log line reads <c>[Shot] Saved</c>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StoreShot : MonoBehaviour
    {
        const float WriteTimeoutSeconds = 3f;

        [Tooltip("Hidden for the captured frame: a game's HUD root; here, the Shot button's canvas.")]
        [SerializeField] GameObject[] hideWhileCapturing;

        [Tooltip("Shown only in Development Builds.")]
        [SerializeField] GameObject debugButton;

        [Tooltip("1 on a 1080p or larger screen; 2 on a 720p phone, so the long side clears 1080 px.")]
        [SerializeField, Range(1, 4)] int superSize = 1;

        static StoreShot s_instance;

        int _count;
        bool _capturing;

        // With domain reloading off in the editor, statics survive between Play
        // sessions; this clears the stale reference before the first scene loads.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            s_instance = null;
        }

        void Awake()
        {
            // The first scene is loaded again on every return to it; its copy gives way.
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            DontDestroyOnLoad(gameObject);
            debugButton.SetActive(Debug.isDebugBuild);
        }

        void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
            }
        }

        /// <summary>Wired to the Shot button's OnClick. A second tap while a capture runs does nothing.</summary>
        public async void OnShotPressed()
        {
            try
            {
                await CaptureAsync(SceneManager.GetActiveScene().name);
            }
            catch (OperationCanceledException)
            {
                // The object was destroyed mid-capture (Play stopped); nothing to finish.
            }
        }

        /// <summary>Captures one screenshot named after <paramref name="tag"/> and returns once the file is written.</summary>
        public async Awaitable CaptureAsync(string tag)
        {
            if (_capturing)
            {
                return;
            }

            _capturing = true;
            try
            {
                foreach (GameObject hidden in hideWhileCapturing)
                {
                    hidden.SetActive(false);
                }

                // One frame later the hidden objects are gone from the image.
                await Awaitable.NextFrameAsync(destroyCancellationToken);

                string file = FileName(tag, _count++, Screen.width * superSize, Screen.height * superSize);
                string path = Path.Combine(Application.persistentDataPath, file);

                // The count restarts every launch, so a name can already exist from an
                // earlier session; delete it, or the wait below would see the old file,
                // stop at once and show the hidden objects before the capture.
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                // On a phone Unity puts a bare file name in persistentDataPath; in the
                // editor a bare name lands in the project folder, so pass the full path.
                ScreenCapture.CaptureScreenshot(Application.isEditor ? path : file, superSize);

                // The capture happens at the end of this frame and the write after it.
                // Wait, in real time (which runs even while paused), until the file
                // exists and its size has stopped changing between two frames.
                float deadline = Time.realtimeSinceStartup + WriteTimeoutSeconds;
                long lastSize = -1;
                bool written = false;
                while (!written && Time.realtimeSinceStartup < deadline)
                {
                    await Awaitable.NextFrameAsync(destroyCancellationToken);
                    long size = File.Exists(path) ? new FileInfo(path).Length : -1;
                    written = size > 0 && size == lastSize;
                    lastSize = size;
                }

                Debug.Log(written ? $"[Shot] Saved {path}" : $"[Shot] No complete file after {WriteTimeoutSeconds} s: {path}");
            }
            finally
            {
                foreach (GameObject hidden in hideWhileCapturing)
                {
                    if (hidden != null)
                    {
                        hidden.SetActive(true);
                    }
                }

                _capturing = false;
            }
        }

        /// <summary><c>Scene_NN_WxH.png</c>: which screen, which shot, and the size Play will check.</summary>
        public static string FileName(string tag, int number, int width, int height)
        {
            return $"{tag}_{number:00}_{width}x{height}.png";
        }
    }
}
