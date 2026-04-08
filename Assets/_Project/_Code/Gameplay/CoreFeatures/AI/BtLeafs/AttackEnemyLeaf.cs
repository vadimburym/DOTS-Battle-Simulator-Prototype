using System;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using VadimBurym.DodBehaviourTree;
using VadimBurym.DodBehaviourTree.Generated;
using VATDots;

namespace _Project._Code.Gameplay.CoreFeatures.AI.BtLeafs
{
    [Serializable]
    [LeafCodeGen((byte)LeafId_BtContext.AttackEnemy)]
    public struct AttackEnemyLeaf : ILeaf
    {
        public LeafData GetCompiledData()
        {
            return new LeafData {
                LeafId = (byte)LeafId_BtContext.AttackEnemy,
            };
        }

        public static NodeStatus OnTick(ref RunnerState_BtContext state)
        {
            var lookUp = state.Context.AttackStateLookup;
            if (!lookUp.HasComponent(state.LeafState.StateEntity))
                return NodeStatus.Running;
            if (!state.Context.EntityInfoLookup.Exists(lookUp[state.LeafState.StateEntity].Target))
                return NodeStatus.Failure;
            if (state.Context.EyeSensorLookup[state.Agent].DetectedEntity != lookUp[state.LeafState.StateEntity].Target)
                return NodeStatus.Failure;

            return NodeStatus.Running;
        }

        public static void OnEnter(ref RunnerState_BtContext state)
        {
            var context = state.Context;
            var agent = state.Agent;
            var sortKey = state.SortKey;
            var enemy = context.EyeSensorLookup[agent].DetectedEntity;

            var ecb = context.Ecb;
            var entity = ecb.CreateEntity(sortKey);
            ecb.AddComponent<AttackState>(sortKey, entity, new AttackState {
                Owner = agent,
                Target = enemy
            });
            ecb.AddComponent(sortKey, ecb.CreateEntity(sortKey), new LeafStateWriteRequest {
                Entity = agent,
                Index = state.LeafState.BufferIndex,
                Value = entity
            });
            ecb.SetComponentEnabled<SeeToDetectedTag>(sortKey, agent, true);

            AnimatorUtils.PlayAnimation(
                renderer: context.RenderEntityLookup[agent].Value,
                animationId: AnimationId.Attack,
                ecb: ecb,
                sortKey: sortKey);
        }

        public static void OnExit(ref RunnerState_BtContext state)
        {
            var ecb = state.Context.Ecb;
            ecb.DestroyEntity(state.SortKey, state.LeafState.StateEntity);
            ecb.SetComponentEnabled<SeeToDetectedTag>(state.SortKey, state.Agent, false);
        }

        public static void OnAbort(ref RunnerState_BtContext state)
        {
            var ecb = state.Context.Ecb;
            ecb.DestroyEntity(state.SortKey, state.LeafState.StateEntity);
            ecb.SetComponentEnabled<SeeToDetectedTag>(state.SortKey, state.Agent, false);
        }
    }
}
