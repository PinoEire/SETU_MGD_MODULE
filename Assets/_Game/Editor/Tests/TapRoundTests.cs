using NUnit.Framework;

namespace MGD.Samples.Editor
{
    public class TapRoundTests
    {
        [TestCase(0, 15, 20f, RoundOutcome.Playing)]
        [TestCase(14, 15, 1f, RoundOutcome.Playing)]
        [TestCase(15, 15, 5f, RoundOutcome.Won)]
        [TestCase(15, 15, 0f, RoundOutcome.Won)]   // the rule: reaching the goal wins even with no time left
        [TestCase(14, 15, 0f, RoundOutcome.Lost)]
        [TestCase(3, 15, -1f, RoundOutcome.Lost)]
        public void Outcome_WinsAtTheGoalAndLosesWhenTimeRunsOut(int score, int goal, float left, RoundOutcome expected)
        {
            Assert.AreEqual(expected, TapRound.Outcome(score, goal, left));
        }
    }
}
