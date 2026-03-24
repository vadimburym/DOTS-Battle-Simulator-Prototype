using System;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Entities;
using VadimBurym.DodBehaviourTree;
using VATDots;

namespace _Project._Code.Gameplay.CoreFeatures.AI.BtLeafs
{
    [Serializable]
    [LeafCodeGen((byte)LeafId_BtContext.Idle)]
    public struct IdleLeaf : ILeaf
    {
        public LeafData GetCompiledData()
        {
            return new LeafData {
                LeafId = (byte)LeafId_BtContext.Idle,
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
            var ecb = leafContext.Ecb;
            var renderer = leafContext.RenderEntityLookup[agent].Value;
            var unitId = leafContext.UnitTagLookup[agent].UnitId;
            ecb.SetComponent(sortKey, renderer, new VATAnimationCommand {
                RequestedClipIndex = VATIndexUtils.GetAnimationIndex(unitId, AnimationId.Idle),
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
            
        }

        public static void OnAbort(
            ref Entity agent,
            in LeafData leafData,
            ref LeafStateElement leafState,
            in BtContext leafContext,
            int sortKey)
        {
            
        }
    }
}