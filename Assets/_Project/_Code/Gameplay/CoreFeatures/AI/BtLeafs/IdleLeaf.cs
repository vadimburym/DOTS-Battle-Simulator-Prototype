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
            AnimatorUtils.PlayAnimation(
                renderer: leafContext.RenderEntityLookup[agent].Value,
                animationId: AnimationId.Idle,
                ecb: leafContext.Ecb,
                sortKey: sortKey);
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