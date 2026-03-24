using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;

namespace _Project._Code.Infrastructure
{
    public sealed class EntityPrefabSingletonAuthoring : MonoBehaviour
    {
        public EntityPrefabPipeline Pipeline;

        public sealed class Baker : Baker<EntityPrefabSingletonAuthoring>
        {
            public override void Bake(EntityPrefabSingletonAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<EntityPrefabSingleton>(entity);
                AddBuffer<EntityPrefabElement>(entity);
                var referenceBuffer = AddBuffer<EntityPrefabReferenceElement>(entity);
                
                if (authoring.Pipeline == null)
                    return;
                var entries = authoring.Pipeline.EntityMemoryPools;
#if UNITY_EDITOR
                for (int i = 0; i < entries.Length; i++)
                {
                    var entry = entries[i];
                    if (entry.Asset == null)
                        continue;
                    referenceBuffer.Add(new EntityPrefabReferenceElement
                    {
                        EntityPoolId = entry.PoolId,
                        PrefabReference = new EntityPrefabReference(entry.Asset)
                    });
                }
#endif
            }
        }
    }
}