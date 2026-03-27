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
    [LeafCodeGen((byte)LeafId_BtContext.ChaseEnemy)]
    public struct ChaseEnemyLeaf : ILeaf
    {
        public LeafData GetCompiledData()
        {
            return new LeafData {
                LeafId = (byte)LeafId_BtContext.ChaseEnemy,
            };
        }
        
        public static NodeStatus OnTick(
            ref Entity agent,
            in LeafData leafData,
            ref LeafStateElement leafState,
            in BtContext leafContext,
            int sortKey)
        {
            if (leafContext.IsMovingTagLookup[agent].IsMoving == 0)
            {
                AnimatorUtils.PlayAnimation(
                    renderer: leafContext.RenderEntityLookup[agent].Value,
                    animationId: AnimationId.Idle,
                    ecb: leafContext.Ecb,
                    sortKey: sortKey);
            }
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
            var random = leafContext.Random;
            var entity = ecb.CreateEntity(sortKey);
            ecb.AddComponent<ChaseState>(sortKey, entity, new ChaseState {
                Owner = agent,
                Target = enemy,
                UpdateInterval = 0.5f,
                UpdateTimer = random.NextFloat(0, 0.5f)
            });
            ecb.AddComponent(sortKey, ecb.CreateEntity(sortKey), new LeafStateWriteRequest {
                Entity = agent,
                Index = leafState.BufferIndex,
                Value = entity
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
        }
    }
}