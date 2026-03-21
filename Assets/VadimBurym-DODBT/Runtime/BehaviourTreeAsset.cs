// DODBT (Data Oriented Design Behaviour Tree for Unity)
// Repository: https://github.com/vadimburym/DODBT
// Copyright (c) 2026 vadimburym (Vadim Burym)
// Licensed under the Custom Game-Use and Redistribution License.
// See LICENSE file in the project root for full license information.

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace VadimBurym.DodBehaviourTree
{
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    [Icon("Assets/VadimBurym-DODBT/Editor/Icons/dodbt-icon.png")]
    public sealed class BehaviourTreeAsset : ScriptableObject
    {
        public string GUID => InternalGUID;
        
        [SerializeField, HideInInspector] internal string InternalGUID;
        [SerializeField, HideInInspector] internal int RootIndex;
        [SerializeField, HideInInspector] internal Node[] Nodes;
        [SerializeField, HideInInspector] internal SelectorNode[] SelectorNodes;
        [SerializeField, HideInInspector] internal SequenceNode[] SequenceNodes;
        [SerializeField, HideInInspector] internal MemorySelectorNode[] MemorySelectorNodes;
        [SerializeField, HideInInspector] internal MemorySequenceNode[] MemorySequenceNodes;
        [SerializeField, HideInInspector] internal ParallelNode[] ParallelNodes;
        [SerializeField, HideInInspector] internal LeafData[] Leafs;

        public void FillAgentStateBuffers(
            //DynamicBuffer<NodeStateElement> nodesBuffer,
            //DynamicBuffer<LeafStateElement> leafsBuffer,
            Entity entity,
            EntityCommandBuffer ecb)
        {
            //nodesBuffer.Clear();
            //leafsBuffer.Clear();
            
            const ushort None = 0xFFFF;
            ushort leafIndex = 0;
            for (int i = 0; i < Nodes.Length; i++)
            {
                var isLeaf = Nodes[i].Id == NodeId.Leaf;
                ecb.AppendToBuffer(entity, new NodeStateElement {
                    CachedStatus = 0,
                    Cursor = 0xFF,
                    IsEntered = 0,
                    LeafStateIndex = isLeaf ? leafIndex : None,
                    MemoryCursor = 0xFF,
                    TmpA = 0,
                    TmpB = 0,
                });
                if (isLeaf) leafIndex++;
            }
            for (int i = 0; i < Leafs.Length; i++)
                ecb.AppendToBuffer(entity, new LeafStateElement {
                    StateEntity = Entity.Null
                });
        }
        
        public BlobAssetReference<BehaviourTreeBlob> CreateBlob()
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BehaviourTreeBlob>();
            
            var nodes = builder.Allocate(ref root.Nodes, Nodes.Length);
            for (int i = 0; i < Nodes.Length; i++)
                nodes[i] = Nodes[i];
            var selectorNodes = builder.Allocate(ref root.SelectorNodes, SelectorNodes.Length);
            for (int i = 0; i < SelectorNodes.Length; i++)
                selectorNodes[i] = SelectorNodes[i];
            var sequenceNodes = builder.Allocate(ref root.SequenceNodes, SequenceNodes.Length);
            for (int i = 0; i < SequenceNodes.Length; i++)
                sequenceNodes[i] = SequenceNodes[i];
            var memorySelectorNodes = builder.Allocate(ref root.MemorySelectorNodes, MemorySelectorNodes.Length);
            for (int i = 0; i < MemorySelectorNodes.Length; i++)
                memorySelectorNodes[i] = MemorySelectorNodes[i];
            var memorySequenceNodes = builder.Allocate(ref root.MemorySequenceNodes, MemorySequenceNodes.Length);
            for (int i = 0; i < MemorySequenceNodes.Length; i++)
                memorySequenceNodes[i] = MemorySequenceNodes[i];
            var parallelNodes = builder.Allocate(ref root.ParallelNodes, ParallelNodes.Length);
            for (int i = 0; i < ParallelNodes.Length; i++)
                parallelNodes[i] = ParallelNodes[i];
            var leafs = builder.Allocate(ref root.Leafs, Leafs.Length);
            for (int i = 0; i < Leafs.Length; i++)
                leafs[i] = Leafs[i];
            root.RootIndex = RootIndex;

            var blob = builder.CreateBlobAssetReference<BehaviourTreeBlob>(Allocator.Persistent);
            builder.Dispose();
            return blob;
        }
        
#if UNITY_EDITOR
        internal void SetupCompiledTree(
            Node[] nodes,
            int rootIndex,
            SelectorNode[] selectorNodes,
            SequenceNode[] sequenceNodes,
            MemorySelectorNode[] memorySelectorNodes,
            MemorySequenceNode[] memorySequenceNodes,
            ParallelNode[] parallelNodes,
            LeafData[] leafs)
        {
            Nodes = nodes;
            RootIndex = rootIndex;
            SelectorNodes = selectorNodes;
            SequenceNodes = sequenceNodes;
            MemorySelectorNodes = memorySelectorNodes;
            MemorySequenceNodes = memorySequenceNodes;
            ParallelNodes = parallelNodes;
            Leafs = leafs;
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
    }
}