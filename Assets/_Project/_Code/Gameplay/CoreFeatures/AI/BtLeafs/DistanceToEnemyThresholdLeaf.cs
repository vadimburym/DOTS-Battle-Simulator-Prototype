using System;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using VadimBurym.DodBehaviourTree;

namespace _Project._Code.Gameplay.CoreFeatures.AI.BtLeafs
{
    [Serializable]
    [LeafCodeGen((byte)LeafId_BtContext.DistanceToEnemyThreshold)]
    public struct DistanceToEnemyThresholdLeaf : ILeaf
    {
        public enum ComparisonId : byte
        {
            Less = 0,
            LessOrEqual = 1,
            GreaterOrEqual = 2,
            Greater = 3,
            Equal = 4,
        }
        
        [SerializeField] private float _threshold;
        [SerializeField] private ComparisonId _comparison;
        
        public LeafData GetCompiledData()
        {
            return new LeafData {
                LeafId = (byte)LeafId_BtContext.DistanceToEnemyThreshold,
                Float0 = _threshold,
                Byte0 = (byte)_comparison,
            };
        }
        
        public static NodeStatus OnTick(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext)
        {
            //TODO Вынести distance в sensor
            var enemy = leafContext.EyeSensorLookup[agent].DetectedEntity;
            var agentPosition = leafContext.LocalTransformLookup[agent].Position;
            var enemyPosition = leafContext.LocalTransformLookup[enemy].Position;
            var threshold = leafData.Float0;
            bool comparison = leafData.Byte0 switch
            {
                0 => math.lengthsq(agentPosition - enemyPosition) < threshold * threshold,
                1 => math.lengthsq(agentPosition - enemyPosition) <= threshold * threshold,
                2 => math.lengthsq(agentPosition - enemyPosition) >= threshold * threshold,
                3 => math.lengthsq(agentPosition - enemyPosition) > threshold * threshold,
                4 => math.lengthsq(agentPosition - enemyPosition) - threshold * threshold < 1e-6f,
                _ => false
            };
            return comparison ? NodeStatus.Success : NodeStatus.Failure;
        }
        
        public static void OnEnter(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
        public static void OnExit(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
        public static void OnAbort(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
    }
}