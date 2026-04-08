using System;
using Unity.Entities;

namespace VadimBurym.DodBehaviourTree.Tests
{
    [Serializable]
    [InternalBufferCapacity(0)]
    public struct RecordingLeafState : IBufferElementData
    {
        public byte IsEntered;
        public ushort BufferIndex;
        public byte StatusCursor;
        public byte EnterCount;
        public byte ExitCount;
        public byte TickCount;
        public byte AbortCount;
        public NodeStatus LastStatus;
    }
}
