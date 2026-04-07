using System;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Entities;
using VadimBurym.DodBehaviourTree;
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

        public static NodeStatus OnTick(
            ref Entity agent,
            in LeafData leafData,
            ref LeafStateElement leafState,
            in BtContext leafContext)
        {
            var lookUp = leafContext.AttackStateLookup;
            if (!lookUp.HasComponent(leafState.StateEntity))
                return NodeStatus.Running;
            if (!leafContext.EntityInfoLookup.Exists(lookUp[leafState.StateEntity].Target))
                return NodeStatus.Failure;
            if (leafContext.EyeSensorLookup[agent].DetectedEntity != lookUp[leafState.StateEntity].Target)
                return NodeStatus.Failure;

            return NodeStatus.Running;
        }

        public static void OnEnter(
            ref Entity agent,
            in LeafData leafData,
            ref LeafStateElement leafState,
            in BtContext leafContext,
            int sortKey)
        {
            var enemy = leafContext.EyeSensorLookup[agent].DetectedEntity;

            var ecb = leafContext.Ecb;
            var entity = ecb.CreateEntity(sortKey);
            ecb.AddComponent<AttackState>(sortKey, entity, new AttackState {
                Owner = agent,
                Target = enemy
            });
            ecb.AddComponent(sortKey, ecb.CreateEntity(sortKey), new LeafStateWriteRequest {
                Entity = agent,
                Index = leafState.BufferIndex,
                Value = entity
            });
            ecb.SetComponentEnabled<SeeToDetectedTag>(sortKey, agent, true);

            AnimatorUtils.PlayAnimation(
                renderer: leafContext.RenderEntityLookup[agent].Value,
                animationId: AnimationId.Attack,
                ecb: ecb,
                sortKey: sortKey);
        }

        public static void OnExit(
            ref Entity agent,
            in LeafData leafData,
            ref LeafStateElement leafState,
            in BtContext leafContext,
            int sortKey)
        {
            var ecb = leafContext.Ecb;
            ecb.DestroyEntity(sortKey, leafState.StateEntity);
            ecb.SetComponentEnabled<SeeToDetectedTag>(sortKey, agent, false);
        }

        public static void OnAbort(
            ref Entity agent,
            in LeafData leafData,
            ref LeafStateElement leafState,
            in BtContext leafContext,
            int sortKey)
        {
            var ecb = leafContext.Ecb;
            ecb.DestroyEntity(sortKey, leafState.StateEntity);
            ecb.SetComponentEnabled<SeeToDetectedTag>(sortKey, agent, false);
        }
    }
}
