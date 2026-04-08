using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace VadimBurym.DodBehaviourTree.Tests
{
    internal static class TestTreeFactory
    {
        private const ushort None = 0xFFFF;

        public static TestTreeRunner CreateRunner(TestNodeSpec rootSpec, int randomSeed = 1)
        {
            if (rootSpec == null)
                throw new ArgumentNullException(nameof(rootSpec));

            BuildLayout(
                rootSpec,
                out var nodes,
                out var selectorNodes,
                out var sequenceNodes,
                out var memorySelectorNodes,
                out var memorySequenceNodes,
                out var parallelNodes,
                out var leafSpecs);

            var asset = BuildAsset(
                nodes,
                selectorNodes,
                sequenceNodes,
                memorySelectorNodes,
                memorySequenceNodes,
                parallelNodes,
                leafSpecs);

            var blob = asset.CreateBlob();

            var leafNames = new string[leafSpecs.Count];
            for (var i = 0; i < leafSpecs.Count; i++)
                leafNames[i] = leafSpecs[i].Name;

            return new TestTreeRunner(blob, new TestContext(leafNames), randomSeed);
        }

        public static BehaviourTreeAsset CreateAsset(TestNodeSpec rootSpec)
        {
            if (rootSpec == null)
                throw new ArgumentNullException(nameof(rootSpec));

            BuildLayout(
                rootSpec,
                out var nodes,
                out var selectorNodes,
                out var sequenceNodes,
                out var memorySelectorNodes,
                out var memorySequenceNodes,
                out var parallelNodes,
                out var leafSpecs);

            var asset = BuildAsset(
                nodes,
                selectorNodes,
                sequenceNodes,
                memorySelectorNodes,
                memorySequenceNodes,
                parallelNodes,
                leafSpecs);

            return asset;
        }

        private static BehaviourTreeAsset BuildAsset(
            List<Node> nodes,
            List<SelectorNode> selectorNodes,
            List<SequenceNode> sequenceNodes,
            List<MemorySelectorNode> memorySelectorNodes,
            List<MemorySequenceNode> memorySequenceNodes,
            List<ParallelNode> parallelNodes,
            List<RecordingLeafSpec> leafSpecs)
        {
            var asset = ScriptableObject.CreateInstance<BehaviourTreeAsset>();
            asset.InternalGUID = Guid.NewGuid().ToString("N");
            asset.RootIndex = 0;
            asset.Nodes = nodes.ToArray();
            asset.SelectorNodes = selectorNodes.ToArray();
            asset.SequenceNodes = sequenceNodes.ToArray();
            asset.MemorySelectorNodes = memorySelectorNodes.ToArray();
            asset.MemorySequenceNodes = memorySequenceNodes.ToArray();
            asset.ParallelNodes = parallelNodes.ToArray();

            var result = new LeafData[leafSpecs.Count];
            for (var i = 0; i < leafSpecs.Count; i++)
            {
                var leafData = new LeafData {
                    LeafId = RecordingLeaf.LeafId,
                    Bytes = default };

                for (var j = 0; j < leafSpecs[i].Statuses.Length; j++)
                    leafData.Bytes.Add((byte)leafSpecs[i].Statuses[j]);

                result[i] = leafData;
            }
            asset.Leafs = result;
            return asset;
        }

        private static void BuildLayout(
            TestNodeSpec rootSpec,
            out List<Node> nodes,
            out List<SelectorNode> selectorNodes,
            out List<SequenceNode> sequenceNodes,
            out List<MemorySelectorNode> memorySelectorNodes,
            out List<MemorySequenceNode> memorySequenceNodes,
            out List<ParallelNode> parallelNodes,
            out List<RecordingLeafSpec> leafSpecs)
        {
            nodes = new List<Node>();
            selectorNodes = new List<SelectorNode>();
            sequenceNodes = new List<SequenceNode>();
            memorySelectorNodes = new List<MemorySelectorNode>();
            memorySequenceNodes = new List<MemorySequenceNode>();
            parallelNodes = new List<ParallelNode>();
            leafSpecs = new List<RecordingLeafSpec>();

            BuildNode(
                rootSpec,
                None,
                true,
                nodes,
                selectorNodes,
                sequenceNodes,
                memorySelectorNodes,
                memorySequenceNodes,
                parallelNodes,
                leafSpecs);
        }

        private static int BuildNode(
            TestNodeSpec spec,
            ushort parentIndex,
            bool isRoot,
            List<Node> nodes,
            List<SelectorNode> selectorNodes,
            List<SequenceNode> sequenceNodes,
            List<MemorySelectorNode> memorySelectorNodes,
            List<MemorySequenceNode> memorySequenceNodes,
            List<ParallelNode> parallelNodes,
            List<RecordingLeafSpec> leafSpecs)
        {
            switch (spec)
            {
                case RecordingLeafSpec leafSpec:
                    return BuildLeaf(leafSpec, parentIndex, isRoot, nodes, leafSpecs);
                case SelectorSpec selectorSpec:
                    return BuildSelector(selectorSpec, parentIndex, isRoot, nodes, selectorNodes, sequenceNodes, memorySelectorNodes, memorySequenceNodes, parallelNodes, leafSpecs);
                case SequenceSpec sequenceSpec:
                    return BuildSequence(sequenceSpec, parentIndex, isRoot, nodes, selectorNodes, sequenceNodes, memorySelectorNodes, memorySequenceNodes, parallelNodes, leafSpecs);
                case MemorySelectorSpec memorySelectorSpec:
                    return BuildMemorySelector(memorySelectorSpec, parentIndex, isRoot, nodes, selectorNodes, sequenceNodes, memorySelectorNodes, memorySequenceNodes, parallelNodes, leafSpecs);
                case MemorySequenceSpec memorySequenceSpec:
                    return BuildMemorySequence(memorySequenceSpec, parentIndex, isRoot, nodes, selectorNodes, sequenceNodes, memorySelectorNodes, memorySequenceNodes, parallelNodes, leafSpecs);
                case ParallelSpec parallelSpec:
                    return BuildParallel(parallelSpec, parentIndex, isRoot, nodes, selectorNodes, sequenceNodes, memorySelectorNodes, memorySequenceNodes, parallelNodes, leafSpecs);
                default:
                    throw new ArgumentOutOfRangeException(nameof(spec), spec, "Unsupported test node spec.");
            }
        }

        private static int BuildLeaf(
            RecordingLeafSpec spec,
            ushort parentIndex,
            bool isRoot,
            List<Node> nodes,
            List<RecordingLeafSpec> leafSpecs)
        {
            var nodeIndex = nodes.Count;
            var leafIndex = checked((ushort)leafSpecs.Count);
            leafSpecs.Add(spec);

            nodes.Add(new Node
            {
                Id = NodeId.Leaf,
                DataIndex = leafIndex,
                ParentIndex = isRoot ? None : parentIndex
            });

            return nodeIndex;
        }

        private static int BuildSelector(
            SelectorSpec spec,
            ushort parentIndex,
            bool isRoot,
            List<Node> nodes,
            List<SelectorNode> selectorNodes,
            List<SequenceNode> sequenceNodes,
            List<MemorySelectorNode> memorySelectorNodes,
            List<MemorySequenceNode> memorySequenceNodes,
            List<ParallelNode> parallelNodes,
            List<RecordingLeafSpec> leafSpecs)
        {
            var nodeIndex = nodes.Count;
            nodes.Add(default);

            var dataIndex = checked((ushort)selectorNodes.Count);
            var firstChild = checked((ushort)nodes.Count);
            for (var i = 0; i < spec.Children.Length; i++)
                BuildNode(
                    spec.Children[i],
                    checked((ushort)nodeIndex),
                    false,
                    nodes,
                    selectorNodes,
                    sequenceNodes,
                    memorySelectorNodes,
                    memorySequenceNodes,
                    parallelNodes,
                    leafSpecs);

            selectorNodes.Add(new SelectorNode
            {
                FirstChild = firstChild,
                ChildCount = checked((byte)spec.Children.Length)
            });

            nodes[nodeIndex] = new Node
            {
                Id = NodeId.Selector,
                DataIndex = dataIndex,
                ParentIndex = isRoot ? None : parentIndex
            };

            return nodeIndex;
        }

        private static int BuildSequence(
            SequenceSpec spec,
            ushort parentIndex,
            bool isRoot,
            List<Node> nodes,
            List<SelectorNode> selectorNodes,
            List<SequenceNode> sequenceNodes,
            List<MemorySelectorNode> memorySelectorNodes,
            List<MemorySequenceNode> memorySequenceNodes,
            List<ParallelNode> parallelNodes,
            List<RecordingLeafSpec> leafSpecs)
        {
            var nodeIndex = nodes.Count;
            nodes.Add(default);

            var dataIndex = checked((ushort)sequenceNodes.Count);
            var firstChild = checked((ushort)nodes.Count);
            for (var i = 0; i < spec.Children.Length; i++)
                BuildNode(
                    spec.Children[i],
                    checked((ushort)nodeIndex),
                    false,
                    nodes,
                    selectorNodes,
                    sequenceNodes,
                    memorySelectorNodes,
                    memorySequenceNodes,
                    parallelNodes,
                    leafSpecs);

            sequenceNodes.Add(new SequenceNode
            {
                FirstChild = firstChild,
                ChildCount = checked((byte)spec.Children.Length)
            });

            nodes[nodeIndex] = new Node
            {
                Id = NodeId.Sequence,
                DataIndex = dataIndex,
                ParentIndex = isRoot ? None : parentIndex
            };

            return nodeIndex;
        }

        private static int BuildMemorySelector(
            MemorySelectorSpec spec,
            ushort parentIndex,
            bool isRoot,
            List<Node> nodes,
            List<SelectorNode> selectorNodes,
            List<SequenceNode> sequenceNodes,
            List<MemorySelectorNode> memorySelectorNodes,
            List<MemorySequenceNode> memorySequenceNodes,
            List<ParallelNode> parallelNodes,
            List<RecordingLeafSpec> leafSpecs)
        {
            var nodeIndex = nodes.Count;
            nodes.Add(default);

            var dataIndex = checked((ushort)memorySelectorNodes.Count);
            var firstChild = checked((ushort)nodes.Count);
            for (var i = 0; i < spec.Children.Length; i++)
                BuildNode(
                    spec.Children[i],
                    checked((ushort)nodeIndex),
                    false,
                    nodes,
                    selectorNodes,
                    sequenceNodes,
                    memorySelectorNodes,
                    memorySequenceNodes,
                    parallelNodes,
                    leafSpecs);

            memorySelectorNodes.Add(new MemorySelectorNode
            {
                FirstChild = firstChild,
                ChildCount = checked((byte)spec.Children.Length),
                PickRandom = spec.PickRandom ? (byte)1 : (byte)0,
                ResetOnAbort = spec.ResetOnAbort ? (byte)1 : (byte)0,
            });

            nodes[nodeIndex] = new Node
            {
                Id = NodeId.MemorySelector,
                DataIndex = dataIndex,
                ParentIndex = isRoot ? None : parentIndex
            };

            return nodeIndex;
        }

        private static int BuildMemorySequence(
            MemorySequenceSpec spec,
            ushort parentIndex,
            bool isRoot,
            List<Node> nodes,
            List<SelectorNode> selectorNodes,
            List<SequenceNode> sequenceNodes,
            List<MemorySelectorNode> memorySelectorNodes,
            List<MemorySequenceNode> memorySequenceNodes,
            List<ParallelNode> parallelNodes,
            List<RecordingLeafSpec> leafSpecs)
        {
            var nodeIndex = nodes.Count;
            nodes.Add(default);

            var dataIndex = checked((ushort)memorySequenceNodes.Count);
            var firstChild = checked((ushort)nodes.Count);
            for (var i = 0; i < spec.Children.Length; i++)
                BuildNode(
                    spec.Children[i],
                    checked((ushort)nodeIndex),
                    false,
                    nodes,
                    selectorNodes,
                    sequenceNodes,
                    memorySelectorNodes,
                    memorySequenceNodes,
                    parallelNodes,
                    leafSpecs);

            memorySequenceNodes.Add(new MemorySequenceNode
            {
                FirstChild = firstChild,
                ChildCount = checked((byte)spec.Children.Length),
                ResetOnFailure = spec.ResetOnFailure ? (byte)1 : (byte)0,
                ResetOnAbort = spec.ResetOnAbort ? (byte)1 : (byte)0,
            });

            nodes[nodeIndex] = new Node
            {
                Id = NodeId.MemorySequence,
                DataIndex = dataIndex,
                ParentIndex = isRoot ? None : parentIndex
            };

            return nodeIndex;
        }

        private static int BuildParallel(
            ParallelSpec spec,
            ushort parentIndex,
            bool isRoot,
            List<Node> nodes,
            List<SelectorNode> selectorNodes,
            List<SequenceNode> sequenceNodes,
            List<MemorySelectorNode> memorySelectorNodes,
            List<MemorySequenceNode> memorySequenceNodes,
            List<ParallelNode> parallelNodes,
            List<RecordingLeafSpec> leafSpecs)
        {
            var nodeIndex = nodes.Count;
            nodes.Add(default);

            var dataIndex = checked((ushort)parallelNodes.Count);
            var firstChild = checked((ushort)nodes.Count);
            for (var i = 0; i < spec.Children.Length; i++)
                BuildNode(
                    spec.Children[i],
                    checked((ushort)nodeIndex),
                    false,
                    nodes,
                    selectorNodes,
                    sequenceNodes,
                    memorySelectorNodes,
                    memorySequenceNodes,
                    parallelNodes,
                    leafSpecs);

            parallelNodes.Add(new ParallelNode
            {
                FirstChild = firstChild,
                ChildCount = checked((byte)spec.Children.Length),
                SuccessThreshold = spec.SuccessThreshold,
                FailsThreshold = spec.FailsThreshold,
                CacheChildStatus = spec.CacheChildStatus ? (byte)1 : (byte)0,
            });

            nodes[nodeIndex] = new Node
            {
                Id = NodeId.Parallel,
                DataIndex = dataIndex,
                ParentIndex = isRoot ? None : parentIndex
            };

            return nodeIndex;
        }
    }
}
