using System;
using System.Collections.Generic;

namespace VadimBurym.DodBehaviourTree.Tests
{
    internal sealed class TestContext
    {
        private readonly string[] _leafNames;

        public TestContext(string[] leafNames)
        {
            _leafNames = leafNames ?? throw new ArgumentNullException(nameof(leafNames));
        }

        public List<string> Events { get; } = new List<string>();

        public byte GetLeafIndex(string name)
        {
            for (int i = 0; i < _leafNames.Length; i++)
                if (name == _leafNames[i]) return (byte)i;

            throw new ArgumentOutOfRangeException(nameof(name), name, "Leaf name is outside of registered test leaves.");
        }

        public string GetLeafName(ushort bufferIndex)
        {
            if (bufferIndex >= _leafNames.Length)
                throw new ArgumentOutOfRangeException(nameof(bufferIndex), bufferIndex, "Leaf buffer index is outside of registered test leaves.");

            return _leafNames[bufferIndex];
        }
    }
}
