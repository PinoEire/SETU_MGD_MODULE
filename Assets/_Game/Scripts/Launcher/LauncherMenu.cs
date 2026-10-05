using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MGD.Samples
{
    /// <summary>
    /// First scene of the build. Lists every other scene in Build Settings as a
    /// button, so a new sample appears here as soon as its builder registers the
    /// scene, and loads the chosen one asynchronously. The footer shows the app
    /// version, Unity version and device, which is the About / Build Info line the
    /// module asks every submission to have.
    /// </summary>
    public sealed class LauncherMenu : MonoBehaviour
    {
        [SerializeField] RectTransform listRoot;
        [SerializeField] Button buttonTemplate;
        [SerializeField] TMP_Text footer;

        bool _loading;

        // Start, not Awake: the TextMeshPro labels may not have initialised yet.
        void Start()
        {
            ScaleDragThreshold();

            footer.text =
                $"{Application.productName} {Application.version}  |  Unity {Application.unityVersion}\n" +
                $"{SystemInfo.deviceModel}  |  {SystemInfo.operatingSystem}";

            int self = SceneManager.GetActiveScene().buildIndex;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                if (i == self)
                {
                    continue;
                }

                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = Path.GetFileNameWithoutExtension(path);

                Button button = Instantiate(buttonTemplate, listRoot);
                button.name = $"Button {sceneName}";
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<TMP_Text>().text = sceneName;

                int buildIndex = i; // captured per button
                button.onClick.AddListener(() => Load(buildIndex));
            }
        }

        // uGUI's default drag threshold is 10 pixels, about 0.6 mm on a 420 dpi
        // phone, so a tap that wobbles starts a scroll and the button never gets its
        // click. Scale it with the screen density (10 px at 160 dpi, about 1.6 mm).
        static void ScaleDragThreshold()
        {
            EventSystem events = EventSystem.current;
            if (events != null)
            {
                events.pixelDragThreshold = DragThresholdFor(Screen.dpi, events.pixelDragThreshold);
            }
        }

        /// <summary>
        /// The drag threshold for this screen density: 10 px at 160 dpi, scaled,
        /// clamped to 10..60 px because some Android devices report a wrong density,
        /// and never lower than <paramref name="current"/>. An unknown density (0)
        /// keeps <paramref name="current"/>. Pure, so it is tested.
        /// </summary>
        public static int DragThresholdFor(float dpi, int current)
        {
            if (dpi <= 0f)
            {
                return current;
            }

            int scaled = Mathf.Clamp(Mathf.RoundToInt(10f * dpi / 160f), 10, 60);
            return Mathf.Max(current, scaled);
        }

        void Load(int buildIndex)
        {
            if (_loading)
            {
                return;
            }

            _loading = true;
            foreach (Button button in listRoot.GetComponentsInChildren<Button>())
            {
                button.interactable = false;
            }

            // Through the shared SceneLoader, which this scene creates; see SceneLoader.
            // The discard means "fire and forget"; any exception still surfaces in
            // the console instead of being swallowed.
            _ = LoadAsync(buildIndex);
        }

        async Awaitable LoadAsync(int buildIndex)
        {
            // The loader persists from this scene, so it always exists here.
            await SceneLoader.Instance.Load(buildIndex, Application.exitCancellationToken);
        }
    }
}
