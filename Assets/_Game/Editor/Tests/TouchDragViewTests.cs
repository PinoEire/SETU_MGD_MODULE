using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace MGD.Samples.Editor
{
    public class TouchDragViewTests
    {
        static readonly string[] Labels = { "Red", "Blue", "Green" };

        static string Status(int fingers, int heldMask)
        {
            var sb = new StringBuilder();
            TouchDragHud.FormatStatus(sb, fingers, heldMask, Labels);
            return sb.ToString();
        }

        [Test]
        public void FormatStatus_NoFingers()
        {
            Assert.AreEqual("0 fingers, nothing held", Status(0, 0));
        }

        [Test]
        public void FormatStatus_OneFingerOneSprite_Singular()
        {
            Assert.AreEqual("1 finger, holding Red", Status(1, 1 << 0));
        }

        [Test]
        public void FormatStatus_TwoFingersTwoSprites_JoinedWithAnd()
        {
            Assert.AreEqual("2 fingers, holding Red and Blue", Status(2, (1 << 0) | (1 << 1)));
        }

        [Test]
        public void FormatStatus_FingerOnEmptySpace()
        {
            Assert.AreEqual("1 finger, nothing held", Status(1, 0));
        }

        [Test]
        public void VisibleWorldRect_OrthographicCamera_MatchesSizeAndAspect()
        {
            var go = new GameObject("Camera", typeof(Camera));
            try
            {
                var cam = go.GetComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 5f;
                cam.aspect = 0.5f;
                go.transform.position = new Vector3(1f, 2f, -10f);

                Rect rect = TouchDragController.VisibleWorldRect(cam);

                Assert.AreEqual(-1.5f, rect.xMin, 0.0001f);
                Assert.AreEqual(3.5f, rect.xMax, 0.0001f);
                Assert.AreEqual(-3f, rect.yMin, 0.0001f);
                Assert.AreEqual(7f, rect.yMax, 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
