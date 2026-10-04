using NUnit.Framework;
using UnityEngine;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// The three player settings share one shape: a cached value, PlayerPrefs
    /// behind it, and a Changed event raised only when the value really changes.
    /// These tests write the editor's PlayerPrefs through the settings themselves
    /// and put the values the lecturer had back afterwards.
    /// </summary>
    public class AccessibilitySettingsTests
    {
        bool _reduceMotionBefore;
        bool _hapticsBefore;
        float _factorBefore;

        [SetUp]
        public void SetUp()
        {
            _reduceMotionBefore = MotionSetting.ReduceMotion;
            _hapticsBefore = Haptics.Enabled;
            _factorBefore = TextScale.Factor;
            MotionSetting.ReduceMotion = false;
            Haptics.Enabled = true;
            TextScale.Factor = TextScale.Normal;
        }

        [TearDown]
        public void TearDown()
        {
            MotionSetting.ReduceMotion = _reduceMotionBefore;
            Haptics.Enabled = _hapticsBefore;
            TextScale.Factor = _factorBefore;
        }

        [Test]
        public void ReduceMotion_SetToNewValue_RaisesChangedOnce()
        {
            int raised = 0;
            void Count() => raised++;
            MotionSetting.Changed += Count;
            try
            {
                MotionSetting.ReduceMotion = true;
                MotionSetting.ReduceMotion = true;

                Assert.AreEqual(1, raised);
                Assert.IsTrue(MotionSetting.ReduceMotion);
                Assert.AreEqual(1, PlayerPrefs.GetInt("MGD.ReduceMotion", -1));
            }
            finally
            {
                MotionSetting.Changed -= Count;
            }
        }

        [Test]
        public void HapticsEnabled_SetToNewValue_RaisesChangedOnce()
        {
            int raised = 0;
            void Count() => raised++;
            Haptics.Changed += Count;
            try
            {
                Haptics.Enabled = false;
                Haptics.Enabled = false;

                Assert.AreEqual(1, raised);
                Assert.IsFalse(Haptics.Enabled);
                Assert.AreEqual(0, PlayerPrefs.GetInt("MGD.Haptics", -1));
            }
            finally
            {
                Haptics.Changed -= Count;
            }
        }

        [Test]
        public void TextScaleFactor_SameValue_DoesNotRaiseChanged()
        {
            int raised = 0;
            void Count() => raised++;
            TextScale.Changed += Count;
            try
            {
                TextScale.Factor = TextScale.Normal;

                Assert.AreEqual(0, raised);
            }
            finally
            {
                TextScale.Changed -= Count;
            }
        }

        [Test]
        public void TextScaleFactor_OutOfRange_IsClamped()
        {
            TextScale.Factor = 5f;
            Assert.AreEqual(2f, TextScale.Factor, 0.0001f);

            TextScale.Factor = 0.1f;
            Assert.AreEqual(0.5f, TextScale.Factor, 0.0001f);
        }

        [Test]
        public void DpToPixels_UsesScreenDpi()
        {
            Assert.AreEqual(126f, AccessibilityDemoHud.DpToPixels(48f, 420f), 0.0001f);
        }

        [Test]
        public void DpToPixels_ZeroDpi_AssumesBaselineDensity()
        {
            Assert.AreEqual(48f, AccessibilityDemoHud.DpToPixels(48f, 0f), 0.0001f);
        }
    }
}
