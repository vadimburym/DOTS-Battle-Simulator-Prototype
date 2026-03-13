using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using _Project._Code.Gameplay.CoreFeatures.Entities.Systems;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Authoring
{
    public sealed class UnitAuthoring : MonoBehaviour
    {
        [Header("References")] 
        public GameObject SelectedView;
        
        [Header("Movement")]
        public float Speed = 0f;
        public float RotationSpeed = 0f;
        
        [Header("Body")]
        public byte Team = 0;
        public byte FootprintX = 1;
        public byte FootprintY = 1;
        
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
                AddComponent(entity, new UnitBody {
                    Team = authoring.Team,
                    FootprintX = (byte)math.max(1, authoring.FootprintX),
                    FootprintY = (byte)math.max(1, authoring.FootprintY)
                });
                AddComponent(entity, new GridNavigationState {
                    OccupiedCell = int2.zero,
                    ReservedCell = int2.zero,
                    HasOccupiedCell = 0,
                    HasReservedCell = 0
                });
            }
        }
    }
}