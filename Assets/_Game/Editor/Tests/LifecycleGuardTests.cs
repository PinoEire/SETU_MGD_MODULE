using NUnit.Framework;
using UnityEngine;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// The guard touches global state (timeScale, AudioListener.pause, its own
    /// statics); every test puts all of it back.
    /// </summary>
    public class LifecycleGuardTests
    {
        float _timeScaleBefore;

        [SetUp]
        public void SetUp()
        {
            _timeScaleBefore = Time.timeScale;
            LifecycleGuard.SetPaused(false);
        }

        [TearDown]
        public void TearDown()
        {
            LifecycleGuard.SetPaused(false);
            Time.timeScale = _timeScaleBefore;
            AudioListener.pause = false;
        }

        [Test]
        public void SetPaused_FromAFrozenGame_ResumesToTheLastRunningScale()
        {
            Time.timeScale = 0.5f;
            LifecycleGuard.SetPaused(true);
            LifecycleGuard.SetPaused(false);

            // A game frozen by its own code pauses and resumes without adopting 0.
            Time.timeScale = 0f;
            LifecycleGuard.SetPaused(true);
            LifecycleGuard.SetPaused(false);

            Assert.That(Time.timeScale, Is.EqualTo(0.5f));
            Assert.That(LifecycleGuard.IsPaused, Is.False);
        }

        [Test]
        public void SetPaused_True_StopsTimeAndAudioAndRaisesOnce()
        {
            int raised = 0;
            void Count(bool _) => raised++;
            LifecycleGuard.PausedChanged += Count;
            try
            {
                LifecycleGuard.SetPaused(true);
                LifecycleGuard.SetPaused(true); // Home fires pause and focus loss; only act once

                Assert.IsTrue(LifecycleGuard.IsPaused);
                Assert.AreEqual(0f, Time.timeScale);
                Assert.IsTrue(AudioListener.pause);
                Assert.AreEqual(1, raised);
            }
            finally
            {
                LifecycleGuard.PausedChanged -= Count;
            }
        }

        [Test]
        public void SetPaused_False_RestoresTheTimeScaleTheGameWasRunningAt()
        {
            Time.timeScale = 0.5f; // a slow-motion game must not come back at 1

            LifecycleGuard.SetPaused(true);
            LifecycleGuard.SetPaused(false);

            Assert.IsFalse(LifecycleGuard.IsPaused);
            Assert.AreEqual(0.5f, Time.timeScale, 0.0001f);
            Assert.IsFalse(AudioListener.pause);
        }

        // OnDestroy resetting the state while paused is a Play-mode behaviour
        // (Unity does not call OnDestroy on a plain MonoBehaviour in edit mode);
        // it is covered by the Lifecycle note's How to test step 4: pause, then
        // tap Back to samples, and the launcher arrives unpaused.
    }
}
