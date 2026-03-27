using System;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Burst;
using Unity.Entities;
using VadimBurym.DodBehaviourTree;

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
        
        public static NodeStatus OnTick(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext)
        {
            var sensor = leafContext.EyeSensorLookup[agent];
            bool isEntityExist = leafContext.EntityInfoLookup.Exists(sensor.DetectedEntity);
            return sensor.IsDetected == 1 && isEntityExist ? NodeStatus.Success : NodeStatus.Failure;
        }
        
        public static void OnEnter(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
        public static void OnExit(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
        public static void OnAbort(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
    }
}