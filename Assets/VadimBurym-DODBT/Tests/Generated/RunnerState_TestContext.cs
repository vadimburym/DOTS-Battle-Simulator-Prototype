using Unity.Collections;

namespace VadimBurym.DodBehaviourTree.Tests
{
    internal ref struct RunnerState_TestContext
    {
        [ReadOnly] public LeafData LeafData;
        public RecordingLeafState LeafState;
        [ReadOnly] public TestContext Context;
    }
}
