using System.Runtime.CompilerServices;

namespace VadimBurym.DodBehaviourTree.Tests
{
    internal static class LeafTables_TestContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeStatus TickLeaf(
            byte leafId,
            in LeafData leaf,
            ref RecordingLeafState leafState,
            in TestContext leafContext)
        {
            switch (leafId)
            {
                case RecordingLeaf.LeafId:
                    return RecordingLeaf.OnTick(in leaf, ref leafState, in leafContext);
                default:
                    return NodeStatus.Failure;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnterLeaf(
            byte leafId,
            in LeafData leaf,
            ref RecordingLeafState leafState,
            in TestContext leafContext)
        {
            switch (leafId)
            {
                case RecordingLeaf.LeafId:
                    RecordingLeaf.OnEnter(in leaf, ref leafState, in leafContext);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExitLeaf(
            byte leafId,
            in LeafData leaf,
            ref RecordingLeafState leafState,
            in TestContext leafContext)
        {
            switch (leafId)
            {
                case RecordingLeaf.LeafId:
                    RecordingLeaf.OnExit(in leaf, ref leafState, in leafContext);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AbortLeaf(
            byte leafId,
            in LeafData leaf,
            ref RecordingLeafState leafState,
            in TestContext leafContext)
        {
            switch (leafId)
            {
                case RecordingLeaf.LeafId:
                    RecordingLeaf.OnAbort(in leaf, ref leafState, in leafContext);
                    break;
            }
        }
    }
}
