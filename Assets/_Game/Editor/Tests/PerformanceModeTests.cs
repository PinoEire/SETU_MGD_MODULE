using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;

namespace MGD.Samples.Editor
{
    // Apply writes the real PlayerPrefs key, the global frame-rate target and the
    // project's URP asset, so every test puts all three back.
    public class PerformanceModeTests
    {
        bool _hadKey;
        int _saved;
        int _frameRate;
        UniversalRenderPipelineAsset _urp;
        float _scale;

        [SetUp]
        public void SetUp()
        {
            _hadKey = PlayerPrefs.HasKey(PerformanceMode.Key);
            _saved = PlayerPrefs.GetInt(PerformanceMode.Key);
            _frameRate = Application.targetFrameRate;
            _urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            _scale = _urp != null ? _urp.renderScale : 1f;
        }

        [TearDown]
        public void TearDown()
        {
            if (_hadKey)
            {
                PlayerPrefs.SetInt(PerformanceMode.Key, _saved);
            }
            else
            {
                PlayerPrefs.DeleteKey(PerformanceMode.Key);
            }

            PlayerPrefs.Save();
            // Current goes back to what the saved key says, as at start-up; the
            // global target and the asset go back to their editor values.
            PerformanceMode.ApplySaved();
            Application.targetFrameRate = _frameRate;
            if (_urp != null)
            {
                _urp.renderScale = _scale;
            }
        }

        [Test]
        public void Balanced_Is60FpsAtFullScale()
        {
            Assert.AreEqual(60, PerformanceMode.FrameRateFor(PerfMode.Balanced));
            Assert.AreEqual(1f, PerformanceMode.RenderScaleFor(PerfMode.Balanced));
        }

        [Test]
        public void HighFps_Asks120AtEightyPercentScale()
        {
            Assert.AreEqual(120, PerformanceMode.FrameRateFor(PerfMode.HighFps));
            Assert.AreEqual(0.8f, PerformanceMode.RenderScaleFor(PerfMode.HighFps));
        }

        [TestCase(0, PerfMode.Balanced)]
        [TestCase(1, PerfMode.HighFps)]
        [TestCase(2, PerfMode.Balanced)]
        [TestCase(-1, PerfMode.Balanced)]
        public void FromSaved_ReadsUnknownValuesAsBalanced(int saved, PerfMode expected)
        {
            Assert.AreEqual(expected, PerformanceMode.FromSaved(saved));
        }

        [Test]
        public void SetHighFps_On_AppliesSavesAndLogsHighFps()
        {
            LogAssert.Expect(LogType.Log, new Regex("^perf_mode=HighFps high=True target=120 scale=0.80$"));

            PerformanceMode.SetHighFps(true);

            Assert.AreEqual(PerfMode.HighFps, PerformanceMode.Current);
            Assert.IsTrue(PerformanceMode.IsHighFps);
            Assert.AreEqual(120, Application.targetFrameRate);
            Assert.AreEqual(1, PlayerPrefs.GetInt(PerformanceMode.Key));
            if (_urp != null)
            {
                Assert.AreEqual(0.8f, _urp.renderScale, 1e-5f);
            }
        }

        [Test]
        public void SetHighFps_Off_AppliesAndSavesBalanced()
        {
            PerformanceMode.SetHighFps(true);
            PerformanceMode.SetHighFps(false);

            Assert.AreEqual(PerfMode.Balanced, PerformanceMode.Current);
            Assert.IsFalse(PerformanceMode.IsHighFps);
            Assert.AreEqual(60, Application.targetFrameRate);
            Assert.AreEqual(0, PlayerPrefs.GetInt(PerformanceMode.Key));
            if (_urp != null)
            {
                Assert.AreEqual(1f, _urp.renderScale, 1e-5f);
            }
        }

        [Test]
        public void ApplySaved_UsesTheSavedMode()
        {
            PlayerPrefs.SetInt(PerformanceMode.Key, 1);

            PerformanceMode.ApplySaved();

            Assert.AreEqual(PerfMode.HighFps, PerformanceMode.Current);
            Assert.AreEqual(120, Application.targetFrameRate);
        }

        [Test]
        public void ApplySaved_WithNoKey_IsBalanced()
        {
            PlayerPrefs.DeleteKey(PerformanceMode.Key);

            PerformanceMode.ApplySaved();

            Assert.AreEqual(PerfMode.Balanced, PerformanceMode.Current);
            Assert.AreEqual(60, Application.targetFrameRate);
        }
    }
}
