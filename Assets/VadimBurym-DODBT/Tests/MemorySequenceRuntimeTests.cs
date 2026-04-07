using NUnit.Framework;

namespace VadimBurym.DodBehaviourTree.Tests
{
    public sealed class MemorySequenceRuntimeTests
    {
        [Test]
        public void MemorySequence_WhenChildRunning_ResumesFromRememberedLeafOnNextTick()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.MemorySequence(
                    true,
                    true,
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Running, NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Success)));

            var firstTick = runner.Tick();
            var secondTick = runner.Tick();

            Assert.That(firstTick, Is.EqualTo(NodeStatus.Running));
            Assert.That(secondTick, Is.EqualTo(NodeStatus.Success));
            Assert.That(runner.Recording("A").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B").EnterCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(2));
            Assert.That(runner.Recording("C").TickCount, Is.EqualTo(1));
        }

        [Test]
        public void MemorySequence_WithResetOnFailureTrue_RestartsFromFirstChildAfterFailure()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.MemorySequence(
                    true,
                    true,
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Success, NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Failure, NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Success)));

            var firstTick = runner.Tick();
            var secondTick = runner.Tick();

            Assert.That(firstTick, Is.EqualTo(NodeStatus.Failure));
            Assert.That(secondTick, Is.EqualTo(NodeStatus.Success));
            Assert.That(runner.Recording("A").TickCount, Is.EqualTo(2));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(2));
            Assert.That(runner.Recording("C").TickCount, Is.EqualTo(1));
        }

        [Test]
        public void MemorySequence_WithResetOnFailureFalse_RechecksFailedChildInsteadOfRestartingTree()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.MemorySequence(
                    false,
                    true,
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("B", NodeStatus.Failure, NodeStatus.Success),
                    TestNodeSpec.RecordingLeaf("C", NodeStatus.Success)));

            var firstTick = runner.Tick();
            var secondTick = runner.Tick();

            Assert.That(firstTick, Is.EqualTo(NodeStatus.Failure));
            Assert.That(secondTick, Is.EqualTo(NodeStatus.Success));
            Assert.That(runner.Recording("A").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B").TickCount, Is.EqualTo(2));
            Assert.That(runner.Recording("C").TickCount, Is.EqualTo(1));
        }

        [Test]
        public void MemorySequence_WithResetOnAbortFalse_ResumesRememberedLeafAfterAbort()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Selector(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Failure, NodeStatus.Success, NodeStatus.Failure),
                    TestNodeSpec.MemorySequence(
                        true,
                        false,
                        TestNodeSpec.RecordingLeaf("B0", NodeStatus.Success),
                        TestNodeSpec.RecordingLeaf("B1", NodeStatus.Running, NodeStatus.Running))));

            var firstTick = runner.Tick();
            var secondTick = runner.Tick();
            var thirdTick = runner.Tick();

            Assert.That(firstTick, Is.EqualTo(NodeStatus.Running));
            Assert.That(secondTick, Is.EqualTo(NodeStatus.Success));
            Assert.That(thirdTick, Is.EqualTo(NodeStatus.Running));
            Assert.That(runner.Recording("B0").TickCount, Is.EqualTo(1));
            Assert.That(runner.Recording("B1").EnterCount, Is.EqualTo(2));
            Assert.That(runner.Recording("B1").AbortCount, Is.EqualTo(1));
        }

        [Test]
        public void MemorySequence_WithResetOnAbortTrue_RestartsFromFirstChildAfterAbort()
        {
            using var runner = TestTreeFactory.CreateRunner(
                TestNodeSpec.Selector(
                    TestNodeSpec.RecordingLeaf("A", NodeStatus.Failure, NodeStatus.Success, NodeStatus.Failure),
                    TestNodeSpec.MemorySequence(
                        true,
                        true,
                        TestNodeSpec.RecordingLeaf("B0", NodeStatus.Success, NodeStatus.Success),
                        TestNodeSpec.RecordingLeaf("B1", NodeStatus.Running, NodeStatus.Running))));

            var firstTick = runner.Tick();
            var secondTick = runner.Tick();
            var thirdTick = runner.Tick();

            Assert.That(firstTick, Is.EqualTo(NodeStatus.Running));
            Assert.That(secondTick, Is.EqualTo(NodeStatus.Success));
            Assert.That(thirdTick, Is.EqualTo(NodeStatus.Running));
            Assert.That(runner.Recording("B0").TickCount, Is.EqualTo(2));
            Assert.That(runner.Recording("B1").EnterCount, Is.EqualTo(2));
            Assert.That(runner.Recording("B1").AbortCount, Is.EqualTo(1));
        }
    }
}
