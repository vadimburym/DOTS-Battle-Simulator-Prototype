using _Project._Code.Core.Keys;
using Unity.Entities;

namespace _Project._Code.Infrastructure
{
    public struct EntityPrefabLoading : IComponentData
    {
        public EntityPoolId EntityPoolId;
        public Entity LoadHandleEntity;
    }
}