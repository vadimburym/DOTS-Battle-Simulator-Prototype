using _Project._Code.Gameplay.CoreFeatures.AI.BtLeafs;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace VadimBurym.DodBehaviourTree.Generated
{
    public struct LeafTables_BtContext
    {
        public static NodeStatus TickLeaf(
            byte leafId,
            ref Entity agent,
            in LeafData leaf,
            ref LeafStateElement leafState,
            in BtContext leafContext)
        {
            switch (leafId)
            {
                case 0: return DebugLeaf.OnTick(ref agent, leaf, ref leafState, leafContext);
                case 1: return IsEnemyDetectedLeaf.OnTick(ref agent, leaf, ref leafState, leafContext);
                case 2: return DistanceToEnemyThresholdLeaf.OnTick(ref agent, leaf, ref leafState, leafContext);
                case 3: return InAttackRangeLeaf.OnTick(ref agent, leaf, ref leafState, leafContext);
            }
            return NodeStatus.None;
        }
        
        public static void EnterLeaf(
            byte leafId,
            ref Entity agent,
            in LeafData leaf,
            ref LeafStateElement leafState,
            in BtContext leafContext)
        {
            switch (leafId)
            {
                case 0: DebugLeaf.OnEnter(ref agent, leaf, ref leafState, leafContext); break;
                case 1: IsEnemyDetectedLeaf.OnEnter(ref agent, leaf, ref leafState, leafContext); break;
                case 2: DistanceToEnemyThresholdLeaf.OnEnter(ref agent, leaf, ref leafState, leafContext); break;
                case 3: InAttackRangeLeaf.OnEnter(ref agent, leaf, ref leafState, leafContext); break;
            }
        }
        
        public static void ExitLeaf(
            byte leafId,
            ref Entity agent,
            in LeafData leaf,
            ref LeafStateElement leafState,
            in BtContext leafContext)
        {
            switch (leafId)
            {
                case 0: DebugLeaf.OnExit(ref agent, leaf, ref leafState, leafContext); break;
                case 1: IsEnemyDetectedLeaf.OnExit(ref agent, leaf, ref leafState, leafContext); break;
                case 2: DistanceToEnemyThresholdLeaf.OnExit(ref agent, leaf, ref leafState, leafContext); break;
                case 3: InAttackRangeLeaf.OnExit(ref agent, leaf, ref leafState, leafContext); break;
            }
        }
        
        public static void AbortLeaf(
            byte leafId,
            ref Entity agent,
            in LeafData leaf,
            ref LeafStateElement leafState,
            in BtContext leafContext)
        {
            switch (leafId)
            {
                case 0: DebugLeaf.OnAbort(ref agent, leaf, ref leafState, leafContext); break;
                case 1: IsEnemyDetectedLeaf.OnAbort(ref agent, leaf, ref leafState, leafContext); break;
                case 2: DistanceToEnemyThresholdLeaf.OnAbort(ref agent, leaf, ref leafState, leafContext); break;
                case 3: InAttackRangeLeaf.OnAbort(ref agent, leaf, ref leafState, leafContext); break;
            }
        }
    }
}
