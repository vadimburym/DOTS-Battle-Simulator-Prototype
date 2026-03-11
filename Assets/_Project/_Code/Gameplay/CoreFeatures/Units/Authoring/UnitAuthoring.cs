using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using _Project._Code.Gameplay.CoreFeatures.Entities.Systems;
using Unity.Entities;
using UnityEngine;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Authoring
{
    public sealed class UnitAuthoring : MonoBehaviour
    {
        [Header("References")] 
        public GameObject SelectedView;
        
        [Header("Default Data")]
        public float Speed = 0f;
        public float RotationSpeed = 0f;
        
        public sealed class Baker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MovementComponent
                {
                    Speed = authoring.Speed,
                    RotationSpeed = authoring.RotationSpeed
                });
                AddComponent(entity, new TargetPosition());
                AddComponent(entity, new Selected {
                    SelectedView = GetEntity(authoring.SelectedView, TransformUsageFlags.Dynamic),
                    ShowScale = authoring.SelectedView.transform.localScale.x
                });
                SetComponentEnabled<Selected>(entity, false);
            }
        }
    }
}