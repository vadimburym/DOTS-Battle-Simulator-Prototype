using System;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Entities;
using VadimBurym.DodBehaviourTree;
using VadimBurym.DodBehaviourTree.Generated;
using VATDots;

namespace _Project._Code.Gameplay.CoreFeatures.AI.BtLeafs
{
    [Serializable]
    [LeafCodeGen((byte)LeafId_BtContext.ChaseEnemy)]
    public struct ChaseEnemyLeaf : ILeaf
    {
        public LeafData GetCompiledData()
        {
            return new LeafData {
                LeafId = (byte)LeafId_BtContext.ChaseEnemy,
            };
        }

        public static NodeStatus OnTick(ref RunnerState_BtContext state)
        {
            if (state.Context.IsMovingTagLookup[state.Agent].IsMoving == 0)
            {
                AnimatorUtils.PlayAnimation(
                    renderer: state.Context.RenderEntityLookup[state.Agent].Value,
                    animationId: AnimationId.Idle,
                    ecb: state.Context.Ecb,
                    sortKey: state.SortKey);
            }
            return NodeStatus.Running;
        }

        public static void OnEnter(ref RunnerState_BtContext state)
        {
            var context = state.Context;
            var agent = state.Agent;
            var sortKey = state.SortKey;
            var enemy = context.EyeSensorLookup[agent].DetectedEntity;

            var ecb = context.Ecb;
            var random = state.Random;
            var entity = ecb.CreateEntity(sortKey);
            ecb.AddComponent<ChaseState>(sortKey, entity, new ChaseState {
                Owner = agent,
                Target = enemy,
                UpdateInterval = 0.5f,
                UpdateTimer = random.NextFloat(0, 0.5f)
            });
            ecb.AddComponent(sortKey, ecb.CreateEntity(sortKey), new LeafStateWriteRequest {
                Entity = agent,
                Index = state.LeafState.BufferIndex,
                Value = entity
            });
        }

        public static void OnExit(ref RunnerState_BtContext state)
        {
            var ecb = state.Context.Ecb;
            ecb.DestroyEntity(state.SortKey, state.LeafState.StateEntity);
        }

        public static void OnAbort(ref RunnerState_BtContext state)
        {
            var ecb = state.Context.Ecb;
            ecb.DestroyEntity(state.SortKey, state.LeafState.StateEntity);
        }
    }
}
