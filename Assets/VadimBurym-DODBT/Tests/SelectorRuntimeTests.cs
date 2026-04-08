using NUnit.Framework;

namespace VadimBurym.DodBehaviourTree.Tests
{
    public sealed class SelectorRuntimeTests
    {
        [Test]
        public void Selector_WhenAllChildrenFail_TickAllChildren_AndReturnsFailure()
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
        public void Selector_WhenChildSucceeds_DoesNotTickRemainingChildren_AndReturnsSuccess()
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
        public void Selector_WhenChildIsRunning_DoesNotTickRemainingChildren_AndReturnsRunning()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Selector(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Failure),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Running),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Success)));

            var status = runner.Tick();

            Assert.That(status, Is.EqualTo(NodeStatus.Running));
            Assert.That(runner.Recording("A").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("C").TickCount, Is.EqualTo(0));
        }

        [Test]
        public void Selector_WhenEarlierChildBecomesRunningAfterLaterChildWasRunning_AbortsLaterChild()
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
        public void Selector_WhenEarlierChildSucceedsAfterLaterChildWasRunning_AbortsRunningChild()
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
        public void Selector_Abort_WhenLeafIsRunning_AbortsRunningChild()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Selector(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Failure),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Running),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Success)));

            runner.Tick();
            runner.Abort();

            Assert.That(runner.Recording("A").AbortCount, Is.EqualTo(0));
            Assert.That(runner.Recording("B").AbortCount, Is.EqualTo(1));
            Assert.That(runner.Recording("C").AbortCount, Is.EqualTo(0));
        }

        [Test]
        public void Selector_With255Children_EvaluatesEveryChild_AndSupportsMaximumCompositeWidth()
        {
            var children = new TestNodeSpec[255];
            for (var i = 0; i < 255; i++)
                children[i] = TestNodeSpec.RecordingLeaf("A" + i.ToString("000"), NodeStatus.Failure);

            using var runner = TestTreeFactory.CreateRunner(TestNodeSpec.Selector(children));
            var status = runner.Tick();

            Assert.That(status, Is.EqualTo(NodeStatus.Failure));
            for (var i = 0; i < 255; i++)
                Assert.That(
                    runner.Recording("A" + i.ToString("000")).TickCount, Is.EqualTo(1),
                    "Unexpected tick count for leaf index " + i);
        }
    }
}
