using System;
using _Project._Code.Core.Keys;
using UnityEngine;

namespace _Project._Code.Infrastructure
{
    [CreateAssetMenu(fileName = nameof(EntityPrefabPipeline), menuName ="_Project/Infrastructure/New EntityPrefabPipeline")]
    public sealed class EntityPrefabPipeline : ScriptableObject
    {
        [Serializable]
        public struct EntityMemoryPoolData
        {
            public EntityPoolId PoolId;
            public GameObject Asset;
        }
        
        public EntityMemoryPoolData[] EntityMemoryPools;
    }
}