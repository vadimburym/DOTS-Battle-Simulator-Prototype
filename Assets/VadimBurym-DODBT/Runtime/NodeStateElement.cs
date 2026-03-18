// DODBT (Data Oriented Design Behaviour Tree for Unity)
// Repository: https://github.com/vadimburym/DODBT
// Copyright (c) 2026 vadimburym (Vadim Burym)
// Licensed under the Custom Game-Use and Redistribution License.
// See LICENSE file in the project root for full license information.

using System;
using Unity.Entities;
using UnityEngine;

namespace VadimBurym.DodBehaviourTree
{
    [Serializable]
    public struct NodeStateElement : IBufferElementData
    {
        [SerializeField] internal byte IsEntered;
        [SerializeField] internal byte Cursor; 
        [SerializeField] internal byte MemoryCursor; 
        [SerializeField] internal byte CachedStatus;  
        [SerializeField] internal byte TmpA;  
        [SerializeField] internal byte TmpB;  
#if DODBT_SMALL_SIZE
        [SerializeField] internal byte LeafStateIndex;
#else
        [SerializeField] internal ushort LeafStateIndex;
#endif
    }
}