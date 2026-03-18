using _Project._Code.Gameplay.CoreFeatures.AI.BtLeafs;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Burst;
using Unity.Collections;

namespace VadimBurym.DodBehaviourTree.Generated
{
    public static class LeafTables_BtContext
    {
        public static NativeArray<FunctionPointer<LeafDelegateTick<BtContext>>> TickTable;
        public static NativeArray<FunctionPointer<LeafDelegate<BtContext>>> EnterTable;
        public static NativeArray<FunctionPointer<LeafDelegate<BtContext>>> ExitTable;
        public static NativeArray<FunctionPointer<LeafDelegate<BtContext>>> AbortTable;

        public static void Initialize()
        {
            TickTable = new(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            EnterTable = new(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            ExitTable = new(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            AbortTable = new(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            TickTable[0] = BurstCompiler.CompileFunctionPointer<LeafDelegateTick<BtContext>>(TestLeaf.OnTick);
            EnterTable[0] = BurstCompiler.CompileFunctionPointer<LeafDelegate<BtContext>>(TestLeaf.OnEnter);
            ExitTable[0] = BurstCompiler.CompileFunctionPointer<LeafDelegate<BtContext>>(TestLeaf.OnExit);
            AbortTable[0] = BurstCompiler.CompileFunctionPointer<LeafDelegate<BtContext>>(TestLeaf.OnAbort);
        }

        public static void Dispose()
        {
            if (TickTable.IsCreated) TickTable.Dispose();
            if (EnterTable.IsCreated) EnterTable.Dispose();
            if (ExitTable.IsCreated) ExitTable.Dispose();
            if (AbortTable.IsCreated) AbortTable.Dispose();
        }
    }
}
