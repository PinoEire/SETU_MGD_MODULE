using NUnit.Framework;

namespace MGD.Samples.Editor
{
    public class ArmedTriggerTests
    {
        [Test]
        public void FirstPress_ArmsButDoesNotFire()
        {
            var trigger = new ArmedTrigger(3f);

            Assert.IsFalse(trigger.Press(10f));
            Assert.IsTrue(trigger.IsArmed(10f));
        }

        [Test]
        public void SecondPressInsideTheWindow_Fires()
        {
            var trigger = new ArmedTrigger(3f);
            trigger.Press(10f);

            Assert.IsTrue(trigger.Press(12.9f));
        }

        [Test]
        public void SecondPressExactlyAtTheEndOfTheWindow_StillFires()
        {
            var trigger = new ArmedTrigger(3f);
            trigger.Press(10f);

            Assert.IsTrue(trigger.Press(13f));
        }

        [Test]
        public void SecondPressAfterTheWindow_ArmsAgainInstead()
        {
            var trigger = new ArmedTrigger(3f);
            trigger.Press(10f);

            Assert.IsFalse(trigger.Press(13.1f));
            Assert.IsTrue(trigger.IsArmed(13.1f));
        }

        [Test]
        public void Firing_Disarms()
        {
            var trigger = new ArmedTrigger(3f);
            trigger.Press(10f);
            trigger.Press(11f);

            Assert.IsFalse(trigger.IsArmed(11f));
            Assert.IsFalse(trigger.Press(11.5f), "a third press starts over");
        }

        [Test]
        public void Arming_RunsOutOnItsOwn()
        {
            var trigger = new ArmedTrigger(3f);
            trigger.Press(10f);

            Assert.IsFalse(trigger.IsArmed(13.5f));
        }
    }
}
