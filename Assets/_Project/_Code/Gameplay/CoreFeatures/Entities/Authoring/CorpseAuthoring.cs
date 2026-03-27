using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Entities;
using UnityEngine;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Authoring
{
    public sealed class CorpseAuthoring : MonoBehaviour
    {
        public sealed class Baker : Baker<CorpseAuthoring>
        {
            public override void Bake(CorpseAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<CorpseTag>(entity);
                SetComponentEnabled<CorpseTag>(entity, false);
            }
        }
    }
}