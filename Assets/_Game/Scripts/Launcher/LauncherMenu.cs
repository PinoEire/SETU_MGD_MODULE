using System.IO;
using TMPro;
using UnityEngine;
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

            _ = LoadAsync(buildIndex);
        }

        async Awaitable LoadAsync(int buildIndex)
        {
            await SceneManager.LoadSceneAsync(buildIndex);
        }
    }
}
