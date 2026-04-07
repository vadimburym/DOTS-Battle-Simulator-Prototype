// DODBT (Data Oriented Design Behaviour Tree for Unity)
// Repository: https://github.com/vadimburym/DODBT
// Copyright (c) 2026 vadimburym (Vadim Burym)
// Licensed under the Custom Game-Use and Redistribution License.
// See LICENSE file in the project root for full license information.

using System;
using Unity.Collections;

namespace VadimBurym.DodBehaviourTree
{
    [Serializable]
    public struct LeafData
    {
        public byte LeafId;
        public int Int0;
        public float Float0;
        public byte Byte0;
        public FixedList32Bytes<byte> Bytes;
    }
}
