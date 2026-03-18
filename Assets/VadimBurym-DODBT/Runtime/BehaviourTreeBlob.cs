// DODBT (Data Oriented Design Behaviour Tree for Unity)
// Repository: https://github.com/vadimburym/DODBT
// Copyright (c) 2026 vadimburym (Vadim Burym)
// Licensed under the Custom Game-Use and Redistribution License.
// See LICENSE file in the project root for full license information.

using Unity.Entities;

namespace VadimBurym.DodBehaviourTree
{
    public struct BehaviourTreeBlob
    {
        internal int RootIndex;
        internal BlobArray<Node> Nodes;
        internal BlobArray<SelectorNode> SelectorNodes;
        internal BlobArray<SequenceNode> SequenceNodes;
        internal BlobArray<MemorySelectorNode> MemorySelectorNodes;
        internal BlobArray<MemorySequenceNode> MemorySequenceNodes;
        internal BlobArray<ParallelNode> ParallelNodes;
        internal BlobArray<LeafData> Leafs;
    }
}