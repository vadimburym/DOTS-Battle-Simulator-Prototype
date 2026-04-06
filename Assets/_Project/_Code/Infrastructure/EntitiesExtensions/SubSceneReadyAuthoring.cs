using Unity.Entities;
using UnityEngine;

namespace _Project._Code.Infrastructure.EntitiesExtensions
{
    public sealed class SubSceneReadyAuthoring : MonoBehaviour
    {
        public sealed class Baker : Baker<SubSceneReadyAuthoring>
        {
            public override void Bake(SubSceneReadyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new SubSceneReady());
            }
        }
    }
    
    public struct SubSceneReady : IComponentData
    {
    }
}