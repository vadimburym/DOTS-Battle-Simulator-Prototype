using System;
using _Project._Code.Core.Keys;
using VadimBurym.DodBehaviourTree;
using VadimBurym.DodBehaviourTree.Generated;

namespace _Project._Code.Gameplay.CoreFeatures.AI.BtLeafs
{
    [Serializable]
    [LeafCodeGen((byte)LeafId_BtContext.InAttackRange)]
    public struct InAttackRangeLeaf : ILeaf
    {
        public LeafData GetCompiledData()
        {
            return new LeafData {
                LeafId = (byte)LeafId_BtContext.InAttackRange,
            };
        }

        public static NodeStatus OnTick(ref RunnerState_BtContext state)
        {
            //TODO: now - GREEN, refactoring: вынести логику в сенсор
            var enemy = state.Context.EyeSensorLookup[state.Agent].DetectedEntity;
            if (!state.Context.GridNavigationStateLookup.HasComponent(enemy))
                return NodeStatus.Failure;
            var agentCell = state.Context.GridNavigationStateLookup[state.Agent].MovingCell;
            var enemyCell = state.Context.GridNavigationStateLookup[enemy].MovingCell;
            var threshold = state.Context.AttackStatsLookup[state.Agent].AttackRangeCells;
            return BattlefieldGridUtils.CellDistanceChebyshev(agentCell, enemyCell) <= threshold ? NodeStatus.Success : NodeStatus.Failure;
        }

        public static void OnEnter(ref RunnerState_BtContext state) { }
        public static void OnExit(ref RunnerState_BtContext state) { }
        public static void OnAbort(ref RunnerState_BtContext state) { }
    }
}
