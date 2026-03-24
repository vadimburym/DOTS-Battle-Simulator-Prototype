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
            
            var renderer = leafContext.RenderEntityLookup[agent].Value;
            var unitId = leafContext.UnitTagLookup[agent].UnitId;
            ecb.SetComponent(sortKey, renderer, new VATAnimationCommand {
                RequestedClipIndex = VATIndexUtils.GetAnimationIndex(unitId, AnimationId.Attack),
                StartNormalizedTime = 0f,
                TransitionDuration = 0.3f,
                RestartIfSame = 0
            });
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