using System;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Burst;
using Unity.Entities;
using VadimBurym.DodBehaviourTree;
using VadimBurym.DodBehaviourTree.Generated;

namespace _Project._Code.Gameplay.CoreFeatures.AI.BtLeafs
{
    [Serializable]
    [LeafCodeGen((byte)LeafId_BtContext.IsEnemyDetected)]
    public struct IsEnemyDetectedLeaf : ILeaf
    {
        public LeafData GetCompiledData()
        {
            return new LeafData {
                LeafId = (byte)LeafId_BtContext.IsEnemyDetected,
            };
        }

        public static NodeStatus OnTick(ref RunnerState_BtContext state)
        {
            var sensor = state.Context.EyeSensorLookup[state.Agent];
            bool isEntityExist = state.Context.EntityInfoLookup.Exists(sensor.DetectedEntity);
            return sensor.IsDetected == 1 && isEntityExist ? NodeStatus.Success : NodeStatus.Failure;
        }

        public static void OnEnter(ref RunnerState_BtContext state) { }
        public static void OnExit(ref RunnerState_BtContext state) { }
        public static void OnAbort(ref RunnerState_BtContext state) { }
    }
}
