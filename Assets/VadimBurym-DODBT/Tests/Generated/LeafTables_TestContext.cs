using System.Runtime.CompilerServices;

namespace VadimBurym.DodBehaviourTree.Tests
{
    internal static class LeafTables_TestContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeStatus TickLeaf(byte leafId, ref RunnerState_TestContext state)
        {
            switch (leafId)
            {
                case RecordingLeaf.LeafId:
                    return RecordingLeaf.OnTick(ref state);
                default:
                    return NodeStatus.Failure;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnterLeaf(byte leafId, ref RunnerState_TestContext state)
        {
            switch (leafId)
            {
                case RecordingLeaf.LeafId:
                    RecordingLeaf.OnEnter(ref state);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExitLeaf(byte leafId, ref RunnerState_TestContext state)
        {
            switch (leafId)
            {
                case RecordingLeaf.LeafId:
                    RecordingLeaf.OnExit(ref state);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AbortLeaf(byte leafId, ref RunnerState_TestContext state)
        {
            switch (leafId)
            {
                case RecordingLeaf.LeafId:
                    RecordingLeaf.OnAbort(ref state);
                    break;
            }
        }
    }
}
