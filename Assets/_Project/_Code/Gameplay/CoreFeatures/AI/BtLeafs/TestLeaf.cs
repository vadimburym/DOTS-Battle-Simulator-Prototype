using System;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using VadimBurym.DodBehaviourTree;

namespace _Project._Code.Gameplay.CoreFeatures.AI.BtLeafs
{
    [Serializable]
    [BurstCompile]
    [LeafCodeGen((byte)LeafId_BtContext.Test)]
    public struct TestLeaf : ILeaf
    {
        [SerializeField] private int _testCooldown;
        
        public LeafData GetCompiledData()
        {
            return new LeafData {
                LeafId = (byte)LeafId_BtContext.Test,
                Int0 = _testCooldown
            };
        }

        [BurstCompile]
        public static NodeStatus OnTick(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext)
        {
            return NodeStatus.Failure;
        }

        [BurstCompile]
        public static void OnEnter(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
        [BurstCompile]
        public static void OnExit(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
        [BurstCompile]
        public static void OnAbort(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
    }
}