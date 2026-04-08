using NUnit.Framework;

namespace VadimBurym.DodBehaviourTree.Tests
{
    public sealed class SequenceRuntimeTests
    {
        [Test]
        public void Sequence_WhenAllChildrenSucceed_TickAllChildren_AndReturnsSuccess()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Sequence(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Success)));

            var status = runner.Tick();

            Assert.That(status, Is.EqualTo(NodeStatus.Success));
            Assert.That(runner.Recording("A").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("C").TickCount, Is.EqualTo(1));
        }

        [Test]
        public void Sequence_WhenChildFails_DoesNotTickRemainingChildren_AndReturnsFailure()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Sequence(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Failure),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Success)));

            var status = runner.Tick();

            Assert.That(status, Is.EqualTo(NodeStatus.Failure));
            Assert.That(runner.Recording("A").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("C").TickCount, Is.EqualTo(0));
        }

        [Test]
        public void Sequence_WhenChildIsRunning_DoesNotTickRemainingChildren_AndReturnsRunning()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Sequence(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Running),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Success)));

            var status = runner.Tick();

            Assert.That(status, Is.EqualTo(NodeStatus.Running));
            Assert.That(runner.Recording("A").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("C").TickCount, Is.EqualTo(0));
        }

        [Test]
        public void Sequence_WhenEarlierChildBecomesRunningAfterLaterChildWasRunning_AbortsPreviouslyRunningChild()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Sequence(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Success, NodeStatus.Running),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Running, NodeStatus.Success)));

            runner.Tick();
            var secondTick = runner.Tick();

            Assert.That(secondTick, Is.EqualTo(NodeStatus.Running));
            Assert.That(runner.Recording("A").TickCount, Is.EqualTo(2));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B").AbortCount, Is.EqualTo(1));

            CollectionAssert.AreEqual(new[] {
                    "enter:A",
                    "tick:A:Success",
                    "exit:A",
                    "enter:B",
                    "tick:B:Running",
                    "enter:A",
                    "tick:A:Running",
                    "abort:B" },
                runner.Events);
        }

        [Test]
        public void Sequence_WhenEarlierChildFailsAfterLaterChildWasRunning_AbortsRunningChild()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Sequence(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Success, NodeStatus.Failure),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Running, NodeStatus.Success)));

            runner.Tick();
            var secondTick = runner.Tick();

            Assert.That(secondTick, Is.EqualTo(NodeStatus.Failure));
            Assert.That(runner.Recording("A").TickCount, Is.EqualTo(2));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B").AbortCount, Is.EqualTo(1));

            CollectionAssert.AreEqual(new[] {
                    "enter:A",
                    "tick:A:Success",
                    "exit:A",
                    "enter:B",
                    "tick:B:Running",
                    "enter:A",
                    "tick:A:Failure",
                    "exit:A",
                    "abort:B" },
                runner.Events);
        }

        [Test]
        public void Sequence_Abort_WhenLeafIsRunning_AbortsRunningChild()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Sequence(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Running),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Success)));

            runner.Tick();
            runner.Abort();

            Assert.That(runner.Recording("A").AbortCount, Is.EqualTo(0));
            Assert.That(runner.Recording("B").AbortCount, Is.EqualTo(1));
            Assert.That(runner.Recording("C").AbortCount, Is.EqualTo(0));
        }

        [Test]
        public void Sequence_With255Children_EvaluatesEveryChild_AndSupportsMaximumCompositeWidth()
        {
            var children = new TestNodeSpec[255];
            for (var i = 0; i < 255; i++)
                children[i] = TestNodeSpec.RecordingLeaf("A" + i.ToString("000"), NodeStatus.Success);

            using var runner = TestTreeFactory.CreateRunner(TestNodeSpec.Sequence(children));
            var status = runner.Tick();

            Assert.That(status, Is.EqualTo(NodeStatus.Success));
            for (var i = 0; i < 255; i++)
                Assert.That(
                    runner.Recording("A" + i.ToString("000")).TickCount, Is.EqualTo(1),
                    "Unexpected tick count for leaf index " + i);
        }
    }
}
