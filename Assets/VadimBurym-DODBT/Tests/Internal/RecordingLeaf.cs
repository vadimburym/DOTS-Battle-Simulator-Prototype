using System;

namespace VadimBurym.DodBehaviourTree.Tests
{
    [Serializable]
    internal static class RecordingLeaf
    {
        public const byte LeafId = 0;

        public static NodeStatus OnTick(
            in LeafData leafData,
            ref RecordingLeafState leafState,
            in TestContext leafContext)
        {
            leafState.TickCount++;
            var cursor = leafState.StatusCursor;

            var statusIndex = cursor < leafData.Bytes.Length
                ? cursor
                : leafData.Bytes.Length - 1;

            var status = (NodeStatus)leafData.Bytes[statusIndex];
            leafState.StatusCursor = (byte)(cursor + 1);

            leafContext.Events.Add("tick:" + leafContext.GetLeafName(leafState.BufferIndex) + ":" + status);
            leafState.LastStatus = status;
            return status;
        }

        public static void OnEnter(
            in LeafData leafData,
            ref RecordingLeafState leafState,
            in TestContext leafContext)
        {
            leafState.EnterCount++;
            leafContext.Events.Add("enter:" + leafContext.GetLeafName(leafState.BufferIndex));
        }

        public static void OnExit(
            in LeafData leafData,
            ref RecordingLeafState leafState,
            in TestContext leafContext)
        {
            leafState.ExitCount++;
            leafContext.Events.Add("exit:" + leafContext.GetLeafName(leafState.BufferIndex));
        }

        public static void OnAbort(
            in LeafData leafData,
            ref RecordingLeafState leafState,
            in TestContext leafContext)
        {
            leafState.AbortCount++;
            leafContext.Events.Add("abort:" + leafContext.GetLeafName(leafState.BufferIndex));
        }
    }
}
