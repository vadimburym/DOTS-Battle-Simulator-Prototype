using System;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using VadimBurym.DodBehaviourTree;
using VadimBurym.DodBehaviourTree.Generated;

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

        public static NodeStatus OnTick(ref RunnerState_BtContext state)
        {
            //TODO: now - GREEN, refactoring: вынести логику в сенсор
            var enemy = state.Context.EyeSensorLookup[state.Agent].DetectedEntity;
            var agentPosition = state.Context.LocalTransformLookup[state.Agent].Position;
            var enemyPosition = state.Context.LocalTransformLookup[enemy].Position;
            var threshold = state.LeafData.Float0;
            bool comparison = state.LeafData.Byte0 switch
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

        public static void OnEnter(ref RunnerState_BtContext state) { }
        public static void OnExit(ref RunnerState_BtContext state) { }
        public static void OnAbort(ref RunnerState_BtContext state) { }
    }
}
