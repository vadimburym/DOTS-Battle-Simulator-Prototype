using System;
using _Project._Code.Core.Keys;
using UnityEngine;
using VadimBurym.DodBehaviourTree;
using VadimBurym.DodBehaviourTree.Generated;

namespace _Project._Code.Gameplay.CoreFeatures.AI.BtLeafs
{
    [Serializable]
    [LeafCodeGen((byte)LeafId_BtContext.IsMoving)]
    public struct IsMovingLeaf : ILeaf
    {
        [SerializeField] private byte _isMoving;

        public LeafData GetCompiledData()
        {
            return new LeafData {
                LeafId = (byte)LeafId_BtContext.IsMoving,
                Byte0 = _isMoving,
            };
        }

        public static NodeStatus OnTick(ref RunnerState_BtContext state)
        {
            return state.Context.IsMovingTagLookup[state.Agent].IsMoving == state.LeafData.Byte0 ? NodeStatus.Success : NodeStatus.Failure;
        }

        public static void OnEnter(ref RunnerState_BtContext state) { }
        public static void OnExit(ref RunnerState_BtContext state) { }
        public static void OnAbort(ref RunnerState_BtContext state) { }
    }
}
