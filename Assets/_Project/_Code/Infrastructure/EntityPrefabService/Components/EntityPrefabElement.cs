using _Project._Code.Core.Keys;
using Unity.Entities;

namespace _Project._Code.Infrastructure
{
    public struct EntityPrefabElement : IBufferElementData
    {
        public EntityPoolId EntityPoolId;
        public Entity Prefab;
        public Entity LoadHandleEntity;
    }
}