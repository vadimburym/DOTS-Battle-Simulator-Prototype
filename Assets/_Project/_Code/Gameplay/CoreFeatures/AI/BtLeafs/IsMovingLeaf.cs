using System;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Entities;
using UnityEngine;
using VadimBurym.DodBehaviourTree;

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
        
        public static NodeStatus OnTick(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext)
        {
            return leafContext.IsMovingTagLookup[agent].IsMoving == leafData.Byte0 ? NodeStatus.Success : NodeStatus.Failure;
        }
        
        public static void OnEnter(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
        public static void OnExit(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
        public static void OnAbort(ref Entity agent, in LeafData leafData, ref LeafStateElement leafState, in BtContext leafContext) { }
    }
}