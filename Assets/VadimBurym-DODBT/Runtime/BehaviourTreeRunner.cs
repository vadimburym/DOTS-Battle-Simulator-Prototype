// DODBT (Data Oriented Design Behaviour Tree for Unity)
// Repository: https://github.com/vadimburym/DODBT
// Copyright (c) 2026 vadimburym (Vadim Burym)
// Licensed under the Custom Game-Use and Redistribution License.
// See LICENSE file in the project root for full license information.

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace VadimBurym.DodBehaviourTree
{
    public delegate void LeafDelegate<TContext>(
        ref Entity agent,
        in LeafData leaf,
        ref LeafStateElement leafState,
        in TContext leafContext) where TContext : struct;
        
    public delegate NodeStatus LeafDelegateTick<TContext>(
        ref Entity agent,
        in LeafData leaf,
        ref LeafStateElement leafState,
        in TContext leafContext) where TContext : struct;
    
    [BurstCompile]
    public struct BehaviourTreeRunner<TContext> where TContext : struct
    {
        private const ushort NoParent = 0xFFFF;
        private const byte Unknown = 0;
        private const byte Failure = 1;
        private const byte Success = 2;
        private const byte Running = 3;
        private const byte None = 0xFF;

        private NativeArray<FunctionPointer<LeafDelegateTick<TContext>>> _tickTable;
        private NativeArray<FunctionPointer<LeafDelegate<TContext>>> _enterTable;
        private NativeArray<FunctionPointer<LeafDelegate<TContext>>> _exitTable;
        private NativeArray<FunctionPointer<LeafDelegate<TContext>>> _abortTable;

        public BehaviourTreeRunner(
            NativeArray<FunctionPointer<LeafDelegateTick<TContext>>> tickTable,
            NativeArray<FunctionPointer<LeafDelegate<TContext>>> enterTable,
            NativeArray<FunctionPointer<LeafDelegate<TContext>>> exitTable,
            NativeArray<FunctionPointer<LeafDelegate<TContext>>> abortTable)
        {
            _tickTable = tickTable;
            _enterTable = enterTable;
            _exitTable = exitTable;
            _abortTable = abortTable;
        }
        
        [BurstCompile]
        public NodeStatus Tick(
            Entity entity,
            ref BehaviourTreeBlob blob,
            ref Random rng,
            DynamicBuffer<NodeStateElement> nodeStates,
            DynamicBuffer<LeafStateElement> leafStates,
            in TContext leafContext)
        {
            var pc = blob.RootIndex;
            var childStatus = NodeStatus.Running;
            bool returning = false;

            while (true)
            {
                var nodeData = blob.Nodes[pc];
                var nodeState = nodeStates[pc];

                switch (nodeData.Id)
                {
                    case NodeId.Leaf:
                    {
                        ref var leafState = ref leafStates.ElementAt(nodeState.LeafStateIndex);
                        var leafData = blob.Leafs[nodeData.DataIndex];
                        
                        if (nodeState.IsEntered == 0)
                        {
                            _enterTable[leafData.LeafId].Invoke(ref entity, in leafData, ref leafState, in leafContext);
                            nodeState.IsEntered = 1;
                            nodeStates[pc] = nodeState;
                        }
                        var status = _tickTable[leafData.LeafId].Invoke(ref entity, in leafData, ref leafState, in leafContext);
                        if (status != NodeStatus.Running)
                        {
                            _exitTable[leafData.LeafId].Invoke(ref entity, in leafData, ref leafState, in leafContext);
                            nodeState.IsEntered = 0;
                            nodeStates[pc] = nodeState;
                        }

                        childStatus = status;
                        returning = true;
                        var parent = nodeData.ParentIndex;
                        if (parent == NoParent) return childStatus;
                        pc = parent;
                        break;
                    }

                    case NodeId.Sequence:
                    {
                        var sequenceData = blob.SequenceNodes[nodeData.DataIndex];

                        if (!returning)
                        {
                            nodeState.Cursor = 0;
                        }
                        else
                        {
                            if (childStatus == NodeStatus.Success)
                            {
                                nodeState.Cursor++;
                            }
                            else if (childStatus == NodeStatus.Running)
                            {
                                if (nodeState.MemoryCursor != None &&
                                    nodeState.MemoryCursor > nodeState.Cursor)
                                {
                                    AbortSubtree(entity, ref blob, nodeStates, leafStates,
                                        (ushort)(sequenceData.FirstChild + nodeState.MemoryCursor), leafContext);
                                }
                                nodeState.MemoryCursor = nodeState.Cursor;
                                
                                nodeStates[pc] = nodeState;
                                returning = true;
                                var parent = nodeData.ParentIndex;
                                if (parent == NoParent) return childStatus;
                                pc = parent;
                                break;
                            }
                            else // Failure
                            {
                                if (nodeState.MemoryCursor != None &&
                                    nodeState.MemoryCursor > nodeState.Cursor)
                                {
                                    AbortSubtree(entity, ref blob, nodeStates, leafStates,
                                        (ushort)(sequenceData.FirstChild + nodeState.MemoryCursor), leafContext);
                                }
                                nodeState.MemoryCursor = None;
                                
                                nodeState.Cursor = None;
                                nodeStates[pc] = nodeState;
                                returning = true;
                                var parent = nodeData.ParentIndex;
                                if (parent == NoParent) return childStatus;
                                pc = parent;
                                break;
                            }
                        }

                        if (nodeState.Cursor >= sequenceData.ChildCount)
                        {
                            nodeState.Cursor = None;
                            nodeState.MemoryCursor = None;
                            nodeStates[pc] = nodeState;
                            returning = true;
                            childStatus = NodeStatus.Success;
                            var parent = nodeData.ParentIndex;
                            if (parent == NoParent) return childStatus;
                            pc = parent;
                            break;
                        }

                        nodeStates[pc] = nodeState;
                        pc = sequenceData.FirstChild + nodeState.Cursor;
                        returning = false;
                        break;
                    }

                    case NodeId.Selector:
                    {
                        var selectorData = blob.SelectorNodes[nodeData.DataIndex];

                        if (!returning)
                        {
                            nodeState.Cursor = 0;
                        }
                        else
                        {
                            if (childStatus == NodeStatus.Failure)
                            {
                                nodeState.Cursor++;
                            }
                            else if (childStatus == NodeStatus.Running)
                            {
                                if (nodeState.MemoryCursor != None &&
                                    nodeState.MemoryCursor > nodeState.Cursor)
                                {
                                    AbortSubtree(entity, ref blob, nodeStates, leafStates,
                                        (ushort)(selectorData.FirstChild + nodeState.MemoryCursor), leafContext);
                                }
                                nodeState.MemoryCursor = nodeState.Cursor;
                                
                                nodeStates[pc] = nodeState;
                                returning = true;
                                var parent = nodeData.ParentIndex;
                                if (parent == NoParent) return childStatus;
                                pc = parent;
                                break;
                            }
                            else // Success
                            {
                                if (nodeState.MemoryCursor != None &&
                                    nodeState.MemoryCursor > nodeState.Cursor)
                                {
                                    AbortSubtree(entity, ref blob, nodeStates, leafStates,
                                        (ushort)(selectorData.FirstChild + nodeState.MemoryCursor), leafContext);
                                }
                                nodeState.MemoryCursor = None;
                                
                                nodeState.Cursor = None;
                                nodeStates[pc] = nodeState;
                                returning = true;
                                var parent = nodeData.ParentIndex;
                                if (parent == NoParent) return childStatus;
                                pc = parent;
                                break;
                            }
                        }

                        if (nodeState.Cursor >= selectorData.ChildCount)
                        {
                            nodeState.Cursor = None;
                            nodeState.MemoryCursor = None;
                            nodeStates[pc] = nodeState;
                            returning = true;
                            childStatus = NodeStatus.Failure;
                            var parent = nodeData.ParentIndex;
                            if (parent == NoParent) return childStatus;
                            pc = parent;
                            break;
                        }
                        
                        nodeStates[pc] = nodeState;
                        pc = selectorData.FirstChild + nodeState.Cursor;
                        returning = false;
                        break;
                    }

                    case NodeId.MemorySequence:
                    {
                        var memorySequenceData = blob.MemorySequenceNodes[nodeData.DataIndex];

                        if (!returning)
                        {
                            if (nodeState.MemoryCursor == None)
                                nodeState.MemoryCursor = 0;
                        }
                        else
                        {
                            if (childStatus == NodeStatus.Success)
                            {
                                nodeState.MemoryCursor++;
                            }
                            else if (childStatus == NodeStatus.Running)
                            {
                                nodeStates[pc] = nodeState;
                                returning = true;
                                var parent = nodeData.ParentIndex;
                                if (parent == NoParent) return childStatus;
                                pc = parent;
                                break;
                            }
                            else // Failure
                            {
                                if (memorySequenceData.ResetOnFailure != 0) nodeState.MemoryCursor = None;
                                nodeStates[pc] = nodeState;
                                returning = true;
                                var parent = nodeData.ParentIndex;
                                if (parent == NoParent) return childStatus;
                                pc = parent;
                                break;
                            }
                        }

                        if (nodeState.MemoryCursor >= memorySequenceData.ChildCount)
                        {
                            nodeState.MemoryCursor = None;
                            nodeStates[pc] = nodeState;
                            returning = true;
                            childStatus = NodeStatus.Success;
                            var parent = nodeData.ParentIndex;
                            if (parent == NoParent) return childStatus;
                            pc = parent;
                            break;
                        }

                        nodeStates[pc] = nodeState;
                        pc = memorySequenceData.FirstChild + nodeState.MemoryCursor;
                        returning = false;
                        break;
                    }

                    case NodeId.MemorySelector:
                    {
                        var memorySelectorData = blob.MemorySelectorNodes[nodeData.DataIndex];

                        if (!returning)
                        {
                            if (nodeState.MemoryCursor == None)
                            {
                                if (memorySelectorData.PickRandom != 0)
                                    nodeState.MemoryCursor = (byte)rng.NextInt(0, memorySelectorData.ChildCount);
                                else
                                    nodeState.MemoryCursor = 0;
                            }
                        }
                        else
                        {
                            if (childStatus == NodeStatus.Failure)
                            {
                                nodeState.MemoryCursor++;
                            }
                            else if (childStatus == NodeStatus.Running)
                            {
                                nodeStates[pc] = nodeState;
                                returning = true;
                                var parent = nodeData.ParentIndex;
                                if (parent == NoParent) return childStatus;
                                pc = parent;
                                break;
                            }
                            else
                            {
                                nodeState.MemoryCursor = None;
                                nodeStates[pc] = nodeState;
                                returning = true;
                                var parent = nodeData.ParentIndex;
                                if (parent == NoParent) return childStatus;
                                pc = parent;
                                break;
                            }
                        }
                        
                        if (nodeState.MemoryCursor >= memorySelectorData.ChildCount)
                        {
                            nodeState.MemoryCursor = None;
                            nodeStates[pc] = nodeState;
                            returning = true;
                            childStatus = NodeStatus.Failure;
                            var parent = nodeData.ParentIndex;
                            if (parent == NoParent) return childStatus;
                            pc = parent;
                            break;
                        }
                        
                        nodeStates[pc] = nodeState;
                        pc = memorySelectorData.FirstChild + nodeState.MemoryCursor;
                        returning = false;
                        break;
                    }

                    case NodeId.Parallel:
                    {
                        var parallelData = blob.ParallelNodes[nodeData.DataIndex];
                        
                        if (!returning)
                        {
                            nodeState.Cursor = 0;
                            nodeState.TmpA = 0;    
                            nodeState.TmpB = 0;        
                            nodeStates[pc] = nodeState;
                        }
                        else
                        {
                            if (childStatus == NodeStatus.Failure)
                            {
                                nodeState.TmpB++;
                                if (parallelData.CacheChildStatus == 1)
                                {
                                    var cachedState = nodeStates[parallelData.FirstChild + nodeState.MemoryCursor];
                                    cachedState.CachedStatus = Failure;
                                    nodeStates[parallelData.FirstChild + nodeState.MemoryCursor] = cachedState;
                                }
                            }
                            else if (childStatus == NodeStatus.Success)
                            {
                                nodeState.TmpA++;
                                if (parallelData.CacheChildStatus == 1)
                                {
                                    var cachedState = nodeStates[parallelData.FirstChild + nodeState.MemoryCursor];
                                    cachedState.CachedStatus = Success;
                                    nodeStates[parallelData.FirstChild + nodeState.MemoryCursor] = cachedState;
                                }
                            }
                            nodeState.MemoryCursor++;
                        }
                        
                        var nextNodeState = nodeStates[parallelData.FirstChild + nodeState.MemoryCursor];
                        while (nextNodeState.CachedStatus != Unknown && parallelData.CacheChildStatus == 1)
                        {
                            if (nextNodeState.CachedStatus == Success)
                                nodeState.TmpA++;
                            else if (nextNodeState.CachedStatus == Failure)
                                nodeState.TmpB++;
                            nodeState.MemoryCursor++;
                            if (nodeState.MemoryCursor >= parallelData.ChildCount)
                                break;
                            nextNodeState = nodeStates[parallelData.FirstChild + nodeState.MemoryCursor];
                        }
                        
                        if (nodeState.MemoryCursor >= parallelData.ChildCount)
                        {
                            childStatus = NodeStatus.Running;
                            if (nodeState.TmpA >= parallelData.SuccessThreshold)
                                childStatus = NodeStatus.Success;
                            if (nodeState.TmpB >= parallelData.SuccessThreshold)
                                childStatus = NodeStatus.Failure;
                            if (childStatus != NodeStatus.Running)
                            {
                                for (int i = 0; i < parallelData.ChildCount; i++)
                                {
                                    AbortSubtree(entity, ref blob, nodeStates, leafStates,
                                        (ushort)(parallelData.FirstChild + i), leafContext);
                                }
                            }
                            var parent = nodeData.ParentIndex;
                            if (parent == NoParent) return childStatus;
                            pc = parent;
                            break;
                        }
                        
                        nodeStates[pc] = nodeState;
                        pc = parallelData.FirstChild + nodeState.MemoryCursor;
                        returning = false;
                        break;
                    }

                    default:
                    {
                        childStatus = NodeStatus.Failure;
                        returning = true;
                        var parent = nodeData.ParentIndex;
                        if (parent == NoParent) return childStatus;
                        pc = parent;
                        break;
                    }
                }
            }
        }

        [BurstCompile]
        public void Abort(
            Entity entity,
            ref BehaviourTreeBlob blob,
            DynamicBuffer<NodeStateElement> nodeStates,
            DynamicBuffer<LeafStateElement> leafStates,
            in TContext leafContext)
        {
            AbortSubtree(entity, ref blob, nodeStates, leafStates, (ushort)blob.RootIndex, leafContext);
        }
        
        [BurstCompile]
        private void AbortSubtree(
            Entity entity,
            ref BehaviourTreeBlob blob,
            DynamicBuffer<NodeStateElement> nodeStates,
            DynamicBuffer<LeafStateElement> leafStates,
            ushort root,
            in TContext leafContext)
        {
            FixedList4096Bytes<int> stack = default;
            stack.Add(root);

            while (stack.Length > 0)
            {
                var nodeIndex = stack[^1];
                stack.RemoveAt(stack.Length - 1);

                var nodeData = blob.Nodes[nodeIndex];
                var nodeState = nodeStates[nodeIndex];
                nodeState.CachedStatus = Unknown;
                
                switch (nodeData.Id)
                {
                    case NodeId.Leaf:
                    {
                        if (nodeState.IsEntered != 0)
                        {
                            nodeState.IsEntered = 0;
                            nodeStates[nodeIndex] = nodeState;

                            ref var leafState = ref leafStates.ElementAt(nodeState.LeafStateIndex);
                            var leafData = blob.Leafs[nodeData.DataIndex];
                            _abortTable[leafData.LeafId].Invoke(ref entity, in leafData, ref leafState, in leafContext);
                        }
                        break;
                    }
                    case NodeId.Selector:
                    {
                        if (nodeState.MemoryCursor == None)
                            break;
                        var selectorData = blob.SelectorNodes[nodeData.DataIndex];
                        stack.Add(selectorData.FirstChild + nodeState.MemoryCursor);
                        nodeState.MemoryCursor = None;
                        nodeStates[nodeIndex] = nodeState;
                        break;
                    }
                    case NodeId.Sequence:
                    {
                        if (nodeState.MemoryCursor == None)
                            break;
                        var sequenceData = blob.SequenceNodes[nodeData.DataIndex];
                        stack.Add(sequenceData.FirstChild + nodeState.MemoryCursor);
                        nodeState.MemoryCursor = None;
                        nodeStates[nodeIndex] = nodeState;
                        break;
                    }
                    case NodeId.MemorySelector:
                    {
                        if (nodeState.MemoryCursor == None)
                            break;
                        var memorySelectorData = blob.MemorySelectorNodes[nodeData.DataIndex];
                        stack.Add(memorySelectorData.FirstChild + nodeState.MemoryCursor);
                        nodeState.MemoryCursor = memorySelectorData.ResetOnAbort != 0 ? None : nodeState.MemoryCursor;
                        nodeStates[nodeIndex] = nodeState;
                        break;
                    }
                    case NodeId.MemorySequence:
                    {
                        if (nodeState.MemoryCursor == None)
                            break;
                        var memorySequenceData = blob.MemorySequenceNodes[nodeData.DataIndex];
                        stack.Add(memorySequenceData.FirstChild + nodeState.MemoryCursor);
                        nodeState.MemoryCursor = memorySequenceData.ResetOnAbort != 0 ? None : nodeState.MemoryCursor;
                        nodeStates[nodeIndex] = nodeState;
                        break;
                    }
                    case NodeId.Parallel:
                    {
                        var parallelData = blob.ParallelNodes[nodeData.DataIndex];
                        for (int i = 0; i < parallelData.ChildCount; i++)
                            stack.Add(parallelData.FirstChild + i);
                        nodeState.Cursor = None;
                        nodeState.TmpA = 0;
                        nodeState.TmpB = 0;
                        nodeStates[nodeIndex] = nodeState;
                        break;
                    }
                }
            }
        }
    }
}