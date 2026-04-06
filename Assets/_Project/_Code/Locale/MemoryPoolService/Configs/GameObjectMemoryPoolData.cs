using System;
using _Project._Code.Core.Keys;
using UnityEngine.AddressableAssets;

namespace _Project._Code.Locale
{
    [Serializable]
    public struct GameObjectMemoryPoolData
    {
        public MemoryPoolId PoolId;
        public TransformId TransformId;
        public AssetReferenceGameObject AssetReference;
        public int InitialCount;
    }
}