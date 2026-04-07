using NUnit.Framework;

namespace VadimBurym.DodBehaviourTree.Tests
{
    public sealed class SelectorRuntimeTests
    {
        [Test]
        public void Selector_WhenAllChildrenFail_ReturnsFailure()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Selector(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Failure),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Failure),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Failure)));

            var status = runner.Tick();

            Assert.That(status, Is.EqualTo(NodeStatus.Failure));
            Assert.That(runner.Recording("A").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("C").TickCount, Is.EqualTo(1));
        }

        [Test]
        public void Selector_WhenChildSucceeds_ReturnsSuccess_AndDoesNotTickRemainingChildren()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Selector(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Failure),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Success)));

            var status = runner.Tick();

            Assert.That(status, Is.EqualTo(NodeStatus.Success));
            Assert.That(runner.Recording("A").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("C").TickCount, Is.EqualTo(0));
        }

        [Test]
        public void Selector_WhenChildIsRunning_ReturnsRunning_AndKeepsLeafEnteredUntilNextTick()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Selector(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Failure),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Running, NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Success)));

            var firstTick = runner.Tick();
            var secondTick = runner.Tick();

            Assert.That(firstTick, Is.EqualTo(NodeStatus.Running));
            Assert.That(secondTick, Is.EqualTo(NodeStatus.Success));
            Assert.That(runner.Recording("B").EnterCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(2));
            Assert.That(runner.Recording("B").ExitCount, Is.EqualTo(1));
            Assert.That(runner.Recording("C").TickCount, Is.EqualTo(0));
            Assert.That(runner.Recording("B").IsEntered, Is.EqualTo(0));
            Assert.That(runner.Recording("B").LastStatus, Is.EqualTo(NodeStatus.Success));
        }

        [Test]
        public void Selector_WhenEarlierChildBecomesRunningAfterLaterChildWasRunning_AbortsLaterLeaf()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Selector(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Failure, NodeStatus.Running),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Running, NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Success)));

            runner.Tick();
            var secondTick = runner.Tick();

            Assert.That(secondTick, Is.EqualTo(NodeStatus.Running));
            Assert.That(runner.Recording("B").AbortCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("C").TickCount, Is.EqualTo(0));

            CollectionAssert.AreEqual(new[] {
                    "enter:A",
                    "tick:A:Failure",
                    "exit:A",
                    "enter:B",
                    "tick:B:Running",
                    "enter:A",
                    "tick:A:Running",
                    "abort:B", },
                runner.Events);
        }

        [Test]
        public void Selector_WhenEarlierChildSucceedsAfterLaterChildWasRunning_AbortsRunningLeaf()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Selector(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Failure, NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Running, NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Success)));

            runner.Tick();
            var secondTick = runner.Tick();

            Assert.That(secondTick, Is.EqualTo(NodeStatus.Success));
            Assert.That(runner.Recording("A").TickCount, Is.EqualTo(2));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B").AbortCount, Is.EqualTo(1));
            Assert.That(runner.Recording("C").TickCount, Is.EqualTo(0));
        }

        [Test]
        public void Selector_Abort_WhenLeafIsRunning_AbortsRunningLeaf_AndNextTickRestartsFromBeginning()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Selector(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Failure, NodeStatus.Failure),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Running, NodeStatus.Running),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Success)));

            var firstTick = runner.Tick();
            runner.Abort();
            var secondTick = runner.Tick();

            Assert.That(firstTick, Is.EqualTo(NodeStatus.Running));
            Assert.That(secondTick, Is.EqualTo(NodeStatus.Running));
            Assert.That(runner.Recording("B").AbortCount, Is.EqualTo(1));
            Assert.That(runner.Recording("A").TickCount, Is.EqualTo(2));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(2));
            Assert.That(runner.Recording("C").TickCount, Is.EqualTo(0));
            Assert.That(runner.Recording("B").IsEntered, Is.EqualTo(1));
        }
    }
}
