using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Authoring
{
    public sealed class UnitAuthoring : MonoBehaviour
    {
        [Header("Default Data")]
        public float Speed = 0f;
        public float3 Direction = float3.zero; 
        
        public sealed class Baker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MovementComponent
                {
                    Speed = authoring.Speed,
                    Direction = authoring.Direction,
                });
            }
        }
    }
}