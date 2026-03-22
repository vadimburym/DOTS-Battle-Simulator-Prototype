using System;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using VadimBurym.DodBehaviourTree;

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
        
        public static NodeStatus OnTick(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext)
        {
            //TODO: now - GREEN, refactoring: вынести логику в сенсор
            var enemy = leafContext.EyeSensorLookup[agent].DetectedEntity;
            var agentCell = leafContext.GridNavigationStateLookup[agent].MovingCell;
            var enemyCell = leafContext.GridNavigationStateLookup[enemy].MovingCell;
            var threshold = leafContext.AttackStatsLookup[agent].AttackRangeCells;
            return BattlefieldGridUtils.CellDistanceChebyshev(agentCell, enemyCell) <= threshold ? NodeStatus.Success : NodeStatus.Failure;
        }
        
        public static void OnEnter(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
        public static void OnExit(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
        public static void OnAbort(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
    }
}