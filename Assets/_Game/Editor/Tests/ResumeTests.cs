using System;
using NUnit.Framework;

namespace MGD.Samples.Editor
{
    /// <summary>
    /// The pausable boss countdown and the offline clock behind the Resume sample.
    /// The Awaitables themselves do not tick in EditMode; they are a Play-mode and
    /// phone check (see the vault note).
    /// </summary>
    public class ResumeTests
    {
        static readonly long Noon = new DateTime(2026, 11, 23, 12, 0, 0, DateTimeKind.Utc).Ticks;

        [Test]
        public void Countdown_PausedPartWay_KeepsTheTimeLeft()
        {
            var boss = new PausableCountdown(5f);
            boss.Start(10f);
            boss.Stop(13f);
            Assert.AreEqual(2f, boss.Remaining(100f), 1e-5f, "frozen while stopped");
            boss.Start(100f);
            Assert.AreEqual(1.5f, boss.Remaining(100.5f), 1e-5f);
        }

        [Test]
        public void Countdown_NeverGoesBelowZero()
        {
            var boss = new PausableCountdown(5f);
            boss.Start(10f);
            Assert.AreEqual(0f, boss.Remaining(30f));
        }

        [Test]
        public void Countdown_OnceFired_DoesNotStartAgain()
        {
            var boss = new PausableCountdown(5f);
            boss.Start(0f);
            boss.Fire();
            boss.Stop(6f);
            Assert.IsFalse(boss.Start(7f));
            Assert.IsFalse(boss.Running);
            Assert.IsTrue(boss.Fired);
        }

        [Test]
        public void Countdown_Reset_StartsFromTheFullDurationAgain()
        {
            var boss = new PausableCountdown(5f);
            boss.Start(0f);
            boss.Fire();
            boss.Reset();
            Assert.IsTrue(boss.Start(20f));
            Assert.AreEqual(5f, boss.Remaining(20f));
        }

        [Test]
        public void SecondsAway_IsTheWallClockGap()
        {
            long later = Noon + TimeSpan.FromSeconds(90).Ticks;
            Assert.AreEqual(90.0, OfflineProgress.SecondsAway(Noon, later, 3600.0), 1e-9);
        }

        [Test]
        public void SecondsAway_ClockMovedBack_IsZero()
        {
            long earlier = Noon - TimeSpan.FromHours(1).Ticks;
            Assert.AreEqual(0.0, OfflineProgress.SecondsAway(Noon, earlier, 3600.0));
        }

        [Test]
        public void SecondsAway_IsCappedAtTheMaximum()
        {
            long nextDay = Noon + TimeSpan.FromDays(1).Ticks;
            Assert.AreEqual(8 * 3600.0, OfflineProgress.SecondsAway(Noon, nextDay, 8 * 3600.0));
        }
    }
}
