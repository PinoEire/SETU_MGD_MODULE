using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MGD.Samples.Editor
{
    public class SceneLoaderTests
    {
        [TestCase(0f, 0f)]
        [TestCase(0.45f, 0.5f)]
        [TestCase(0.9f, 1f)]
        [TestCase(1f, 1f)]
        [TestCase(-0.1f, 0f)]
        public void BarValue_MapsProgressToTheFullBar(float progress, float expected)
        {
            Assert.AreEqual(expected, SceneLoader.BarValue(progress), 0.0001f);
        }

        const string LauncherPath = "Assets/_Game/Scenes/Launcher/Launcher.unity";

        [Test]
        public void LauncherScene_HasOneSceneLoaderSortedAboveEveryCanvas()
        {
            Scene scene = EditorSceneManager.OpenScene(LauncherPath, OpenSceneMode.Additive);
            try
            {
                var loaders = new List<SceneLoader>();
                var canvases = new List<Canvas>();
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    loaders.AddRange(root.GetComponentsInChildren<SceneLoader>(true));
                    canvases.AddRange(root.GetComponentsInChildren<Canvas>(true));
                }

                Assert.AreEqual(1, loaders.Count, "exactly one SceneLoader");
                Canvas loaderCanvas = loaders[0].GetComponentInChildren<Canvas>(true);
                Assert.IsNotNull(loaderCanvas, "the loader owns its canvas");
                foreach (Canvas other in canvases)
                {
                    if (other != loaderCanvas)
                    {
                        Assert.Greater(loaderCanvas.sortingOrder, other.sortingOrder, other.name);
                    }
                }

                Assert.IsNotNull(FindIn<MenuReady>(scene), "MenuReady is in the Launcher");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        static T FindIn<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T found = root.GetComponentInChildren<T>(true);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        [TestCase(0.5f, 10f, 3f, false)]
        [TestCase(0.9f, 2.9f, 3f, false)]
        [TestCase(0.9f, 3f, 3f, true)]
        [TestCase(0.9f, -1f, 3f, false)]
        public void IsStuck_OnlyAtNinetyPercentAfterTheLimit(float progress, float held, float limit, bool expected)
        {
            Assert.AreEqual(expected, LoadingDemoHud.IsStuck(progress, held, limit));
        }

        [Test]
        public void HeavyScene_StaysUnderTheFileBudget()
        {
            var file = new System.IO.FileInfo("Assets/_Game/Scenes/Loading/LoadingHeavy.unity");
            Assert.IsTrue(file.Exists, "build the Loading scene first");
            Assert.Less(file.Length, 5L * 1024 * 1024);
        }
    }
}
