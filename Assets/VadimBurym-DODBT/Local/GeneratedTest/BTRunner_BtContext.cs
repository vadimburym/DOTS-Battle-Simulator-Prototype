using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Random = Unity.Mathematics.Random;

namespace VadimBurym.DodBehaviourTree.Generated
{
    public struct BTRunner_BtContext
    {
        private const ushort None = 0xFFFF;
        
        public NodeStatus Tick(
            Entity entity,
            ref BehaviourTreeBlob blob,
            ref Random rng,
            DynamicBuffer<NodeStateElement> nodeStates,
            DynamicBuffer<LeafStateElement> leafStates,
            in BtContext leafContext,
            int sortKey)
        {
            var pc = blob.RootIndex;
            var childStatus = NodeStatus.Running;
            bool returning = false;
            
            while (pc != None)
            {
                var nodeData = blob.Nodes[pc];
                var nodeStatePc = pc;
                var nodeState = nodeStates[nodeStatePc];
                ushort abortingNode = None;
                switch (nodeData.Id)
                {
                    case NodeId.Leaf:
                    {
                        var leafState = leafStates[nodeState.LeafStateIndex];
                        var leafData = blob.Leafs[nodeData.DataIndex];
                        
                        if (leafState.IsEntered == 0)
                        {
                            LeafTables_BtContext.EnterLeaf(leafData.LeafId, ref entity, in leafData, ref leafState, in leafContext, sortKey);
                            leafState.IsEntered = 1;
                        }
                        var status = LeafTables_BtContext.TickLeaf(leafData.LeafId, ref entity, in leafData, ref leafState, in leafContext);
                        if (status != NodeStatus.Running)
                        {
                            LeafTables_BtContext.ExitLeaf(leafData.LeafId, ref entity, in leafData, ref leafState, in leafContext, sortKey);
                            leafState.IsEntered = 0;
                        }
                        
                        leafStates[nodeState.LeafStateIndex] = leafState;
                        childStatus = status;
                        returning = true;
                        pc = nodeData.ParentIndex;
                        break;
                    }
                    case NodeId.Sequence:
                    {
                        NodeExecutor.ExecuteSequence(
                            ref pc,
                            ref childStatus,
                            ref returning,
                            ref abortingNode,
                            ref nodeState,
                            ref nodeData,
                            ref blob);
                        break;
                    }
                    case NodeId.Selector:
                    {
                        NodeExecutor.ExecuteSelector(
                            ref pc,
                            ref childStatus,
                            ref returning,
                            ref abortingNode,
                            ref nodeState,
                            ref nodeData,
                            ref blob);
                        break;
                    }
                    case NodeId.MemorySequence:
                    {
                        NodeExecutor.ExecuteMemorySequence(
                            ref pc,
                            ref childStatus,
                            ref returning,
                            ref nodeState,
                            ref nodeData,
                            ref blob);
                        break;
                    }
                    case NodeId.MemorySelector:
                    {
                        NodeExecutor.ExecuteMemorySelector(
                            ref pc,
                            ref childStatus,
                            ref returning,
                            ref nodeState,
                            ref nodeData,
                            ref blob,
                            ref rng);
                        break;
                    }
                    case NodeId.Parallel:
                    {
                        NodeExecutor.ExecuteParallel(
                            ref pc,
                            ref childStatus,
                            ref returning,
                            ref abortingNode,
                            ref nodeState,
                            ref nodeData,
                            ref blob,
                            ref nodeStates);
                        break;
                    }
                    default:
                    {
                        childStatus = NodeStatus.Failure;
                        returning = true;
                        pc = nodeData.ParentIndex;
                        break;
                    }
                }
                if (abortingNode != None)
                    AbortSubtree(entity, ref blob, nodeStates, leafStates, abortingNode, leafContext, sortKey);
                nodeStates[nodeStatePc] = nodeState;
            }
            return childStatus;
        }
        
        public void Abort(
            Entity entity,
            ref BehaviourTreeBlob blob,
            DynamicBuffer<NodeStateElement> nodeStates,
            DynamicBuffer<LeafStateElement> leafStates,
            in BtContext leafContext,
            int sortKey)
        {
            AbortSubtree(entity, ref blob, nodeStates, leafStates, (ushort)blob.RootIndex, leafContext, sortKey);
        }
        
        private void AbortSubtree(
            Entity entity,
            ref BehaviourTreeBlob blob,
            DynamicBuffer<NodeStateElement> nodeStates,
            DynamicBuffer<LeafStateElement> leafStates,
            ushort root,
            in BtContext leafContext,
            int sortKey)
        {
            FixedList4096Bytes<int> stack = default;
            stack.Add(root);

            while (stack.Length > 0)
            {
                var nodeIndex = stack[^1];
                stack.RemoveAt(stack.Length - 1);
                var nodeData = blob.Nodes[nodeIndex];
                var nodeState = nodeStates[nodeIndex];
                switch (nodeData.Id)
                {
                    case NodeId.Leaf:
                    {
                        var leafState = leafStates[nodeState.LeafStateIndex];
                        if (leafState.IsEntered != 0)
                        {
                            leafState.IsEntered = 0;
                            var leafData = blob.Leafs[nodeData.DataIndex];
                            LeafTables_BtContext.AbortLeaf(leafData.LeafId, ref entity, in leafData, ref leafState, in leafContext, sortKey);
                        }
                        leafStates[nodeState.LeafStateIndex] = leafState;
                        break;
                    }
                    case NodeId.Selector:
                    {
                        NodeExecutor.AbortSelector(
                            ref stack,
                            ref nodeState,
                            ref nodeData,
                            ref blob);
                        break;
                    }
                    case NodeId.Sequence:
                    {
                        NodeExecutor.AbortSequence(
                            ref stack,
                            ref nodeState,
                            ref nodeData,
                            ref blob);
                        break;
                    }
                    case NodeId.MemorySelector:
                    {
                        NodeExecutor.AbortMemorySelector(
                            ref stack,
                            ref nodeState,
                            ref nodeData,
                            ref blob);
                        break;
                    }
                    case NodeId.MemorySequence:
                    {
                        NodeExecutor.AbortMemorySequence(
                            ref stack,
                            ref nodeState,
                            ref nodeData,
                            ref blob);
                        break;
                    }
                    case NodeId.Parallel:
                    {
                        NodeExecutor.AbortParallel(
                            ref stack,
                            ref nodeState,
                            ref nodeData,
                            ref blob,
                            ref nodeStates);
                        break;
                    }
                }
                nodeStates[nodeIndex] = nodeState;
            }
        }
    }
}