using System;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using VadimBurym.DodBehaviourTree;
using VadimBurym.DodBehaviourTree.Generated;

namespace _Project._Code.Gameplay.CoreFeatures.AI.BtLeafs
{
    [Serializable]
    [LeafCodeGen((byte)LeafId_BtContext.Debug)]
    public struct DebugLeaf : ILeaf
    {
        [SerializeField] private int _debugInt;
        [SerializeField] private NodeStatus _debugStatus;

        public LeafData GetCompiledData()
        {
            return new LeafData {
                LeafId = (byte)LeafId_BtContext.Debug,
                Int0 = _debugInt,
                Byte0 = (byte)_debugStatus,
            };
        }

        public static NodeStatus OnTick(ref RunnerState_BtContext state)
        {
            //Debug.Log($"{leafData.Int0}");
            return (NodeStatus)state.LeafData.Byte0;
        }

        public static void OnEnter(ref RunnerState_BtContext state) { }
        public static void OnExit(ref RunnerState_BtContext state) { }
        public static void OnAbort(ref RunnerState_BtContext state) { }
    }
}
