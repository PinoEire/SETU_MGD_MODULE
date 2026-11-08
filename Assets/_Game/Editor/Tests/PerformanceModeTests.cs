using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;

namespace MGD.Samples.Editor
{
    // Apply writes the real PlayerPrefs key, the global frame-rate target and the
    // active URP asset, so every test puts all three back. The tier is whatever
    // DeviceTier.Current is in the editor, so applied values are compared with
    // what the rules give for that tier.
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

        [TestCase(Tier.Low, 30, 0.75f)]
        [TestCase(Tier.Mid, 60, 0.9f)]
        [TestCase(Tier.High, 60, 1f)]
        public void Balanced_IsTheTiersOwnCapAndScale(Tier tier, int fps, float scale)
        {
            Assert.AreEqual(fps, PerformanceMode.FrameRateFor(PerfMode.Balanced, tier));
            Assert.AreEqual(scale, PerformanceMode.RenderScaleFor(PerfMode.Balanced, tier), 1e-6f);
        }

        [Test]
        public void HighFps_OnTheHighTier_Asks120AtEightyPercentScale()
        {
            Assert.AreEqual(120, PerformanceMode.FrameRateFor(PerfMode.HighFps, Tier.High));
            Assert.AreEqual(0.8f, PerformanceMode.RenderScaleFor(PerfMode.HighFps, Tier.High), 1e-6f);
        }

        [TestCase(Tier.Low)]
        [TestCase(Tier.Mid)]
        public void HighFps_BelowTheHighTier_RunsAsBalanced(Tier tier)
        {
            Assert.AreEqual(PerfMode.Balanced, PerformanceMode.Effective(PerfMode.HighFps, tier));
            Assert.AreEqual(DeviceTier.FrameRateFor(tier), PerformanceMode.FrameRateFor(PerfMode.HighFps, tier));
            Assert.AreEqual(DeviceTier.RenderScaleFor(tier), PerformanceMode.RenderScaleFor(PerfMode.HighFps, tier), 1e-6f);
        }

        [TestCase(Tier.Low, false)]
        [TestCase(Tier.Mid, false)]
        [TestCase(Tier.High, true)]
        public void HighFpsAllowed_OnlyOnTheHighTier(Tier tier, bool allowed)
        {
            Assert.AreEqual(allowed, PerformanceMode.HighFpsAllowedOn(tier));
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
        public void SetHighFps_On_SavesTheChoiceAndAppliesWhatTheTierAllows()
        {
            Tier tier = DeviceTier.Current;
            PerfMode effective = PerformanceMode.Effective(PerfMode.HighFps, tier);
            LogAssert.Expect(LogType.Log, new Regex(FormattableString.Invariant(
                $"^perf_mode={effective} high={effective == PerfMode.HighFps} target={PerformanceMode.FrameRateFor(effective, tier)} scale={PerformanceMode.RenderScaleFor(effective, tier):0.00} tier={tier}$")));

            PerformanceMode.SetHighFps(true);

            Assert.AreEqual(1, PlayerPrefs.GetInt(PerformanceMode.Key), "the choice is saved whatever the tier");
            Assert.AreEqual(effective, PerformanceMode.Current);
            Assert.AreEqual(PerformanceMode.FrameRateFor(effective, tier), Application.targetFrameRate);
            if (_urp != null)
            {
                Assert.AreEqual(PerformanceMode.RenderScaleFor(effective, tier), _urp.renderScale, 1e-5f);
            }
        }

        [Test]
        public void SetHighFps_Off_AppliesAndSavesBalanced()
        {
            Tier tier = DeviceTier.Current;
            PerformanceMode.SetHighFps(true);
            PerformanceMode.SetHighFps(false);

            Assert.AreEqual(PerfMode.Balanced, PerformanceMode.Current);
            Assert.IsFalse(PerformanceMode.IsHighFps);
            Assert.AreEqual(DeviceTier.FrameRateFor(tier), Application.targetFrameRate);
            Assert.AreEqual(0, PlayerPrefs.GetInt(PerformanceMode.Key));
            if (_urp != null)
            {
                Assert.AreEqual(DeviceTier.RenderScaleFor(tier), _urp.renderScale, 1e-5f);
            }
        }

        [Test]
        public void ApplySaved_UsesTheSavedChoice()
        {
            Tier tier = DeviceTier.Current;
            PlayerPrefs.SetInt(PerformanceMode.Key, 1);

            PerformanceMode.ApplySaved();

            Assert.AreEqual(PerformanceMode.Effective(PerfMode.HighFps, tier), PerformanceMode.Current);
            Assert.AreEqual(PerformanceMode.FrameRateFor(PerfMode.HighFps, tier), Application.targetFrameRate);
        }

        [Test]
        public void ApplySaved_WithNoKey_IsBalanced()
        {
            PlayerPrefs.DeleteKey(PerformanceMode.Key);

            PerformanceMode.ApplySaved();

            Assert.AreEqual(PerfMode.Balanced, PerformanceMode.Current);
            Assert.AreEqual(DeviceTier.FrameRateFor(DeviceTier.Current), Application.targetFrameRate);
        }
    }
}
