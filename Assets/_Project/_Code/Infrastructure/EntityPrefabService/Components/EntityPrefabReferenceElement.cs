using _Project._Code.Core.Keys;
using Unity.Entities;
using Unity.Entities.Serialization;

namespace _Project._Code.Infrastructure
{
    public struct EntityPrefabReferenceElement : IBufferElementData
    {
        public EntityPoolId EntityPoolId;
        public EntityPrefabReference PrefabReference;
    }
}