using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MGD.Samples.Editor
{
    public class LauncherSceneTests
    {
        const string LauncherPath = "Assets/_Game/Scenes/Launcher/Launcher.unity";

        [Test]
        public void SampleList_ScrollsVerticallyInAClippedViewport()
        {
            // Open the saved scene additively, unless it is already loaded, and put
            // the editor back as it was: close only what this test loaded, and keep a
            // scene that was in the hierarchy (unloaded) there.
            Scene scene = SceneManager.GetSceneByPath(LauncherPath);
            bool openedHere = !scene.isLoaded;
            bool wasInHierarchy = scene.IsValid();
            if (openedHere)
            {
                scene = EditorSceneManager.OpenScene(LauncherPath, OpenSceneMode.Additive);
            }

            try
            {
                LauncherMenu menu = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    menu = menu != null ? menu : root.GetComponentInChildren<LauncherMenu>(true);
                }

                Assert.IsNotNull(menu, "LauncherMenu is in the Launcher");
                var listRoot = (RectTransform)new SerializedObject(menu).FindProperty("listRoot").objectReferenceValue;

                ScrollRect scroll = listRoot.GetComponentInParent<ScrollRect>(true);
                Assert.IsNotNull(scroll, "the list sits inside a ScrollRect");
                Assert.AreSame(listRoot, scroll.content, "the list is the scroll content");
                Assert.IsTrue(scroll.vertical, "scrolls vertically");
                Assert.IsFalse(scroll.horizontal, "does not scroll sideways");
                Assert.AreEqual(ScrollRect.MovementType.Clamped, scroll.movementType);
                Assert.IsNotNull(scroll.viewport, "has a viewport");
                Assert.IsNotNull(scroll.viewport.GetComponent<RectMask2D>(), "the viewport clips the buttons");
                Assert.IsNotNull(scroll.verticalScrollbar, "a scrollbar shows there is more below");
                Assert.AreEqual(ScrollRect.ScrollbarVisibility.AutoHide, scroll.verticalScrollbarVisibility,
                    "the scrollbar hides while everything fits");

                var fitter = listRoot.GetComponent<ContentSizeFitter>();
                Assert.IsNotNull(fitter, "the list grows with its buttons");
                Assert.AreEqual(ContentSizeFitter.FitMode.PreferredSize, fitter.verticalFit);
            }
            finally
            {
                if (openedHere)
                {
                    EditorSceneManager.CloseScene(scene, removeScene: !wasInHierarchy);
                }
            }
        }
    }
}
