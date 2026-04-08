using System;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Entities;
using VadimBurym.DodBehaviourTree;
using VadimBurym.DodBehaviourTree.Generated;
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

        public static NodeStatus OnTick(ref RunnerState_BtContext state)
        {
            return NodeStatus.Running;
        }

        public static void OnEnter(ref RunnerState_BtContext state)
        {
            AnimatorUtils.PlayAnimation(
                renderer: state.Context.RenderEntityLookup[state.Agent].Value,
                animationId: AnimationId.Idle,
                ecb: state.Context.Ecb,
                sortKey: state.SortKey);
        }

        public static void OnExit(ref RunnerState_BtContext state) { }
        public static void OnAbort(ref RunnerState_BtContext state) { }
    }
}
