using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using _Project._Code.Gameplay.CoreFeatures.Units.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Authoring
{
    public sealed class UnitAuthoring : MonoBehaviour
    {
        [Header("References")] 
        public GameObject SelectedView;
        
        [Header("MovementStats")]
        public float Speed = 0f;
        public float RotationSpeed = 0f;
        [Header("Team")]
        public byte Team = 0;
        [Header("Footprint")]
        public byte FootprintX = 1;
        public byte FootprintY = 1;
        [Header("AttackStats")]
        public float AttackInterval = 3f;
        public byte AttackRangeCells = 2;
        public ushort Damage = 5;
        [Header("EyeSensorStats")]
        public float DetectRadius = 10f;
        public float ChaseRadius = 15f;
        public float UpdateNearestInterval = 3f;
        public float ScanInterval = 0.3f;
        
        public sealed class Baker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MovementStats
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
                AddComponent(entity, new Footprint {
                    FootprintX = (byte)math.max(1, authoring.FootprintX),
                    FootprintY = (byte)math.max(1, authoring.FootprintY)
                });
                AddComponent(entity, new GridNavigationState {
                    OccupiedCell = int2.zero,
                    MovingCell = int2.zero,
                    HasOccupiedCell = 0,
                });
                AddComponent<Team>(entity, new Team { Value = authoring.Team });
                AddComponent<MyTeamTag>(entity);
                AddComponent<IsMovingTag>(entity);
                AddComponent<AttackStats>(entity, new AttackStats {
                    AttackInterval = authoring.AttackInterval,
                    AttackRangeCells = authoring.AttackRangeCells,
                    Damage = authoring.Damage
                });
                SetComponentEnabled<MyTeamTag>(entity, false);
                AddComponent<EyeSensorStats>(entity, new EyeSensorStats {
                    DetectRadius = authoring.DetectRadius,
                    ChaseRadius = authoring.ChaseRadius,
                    UpdateNearestInterval = authoring.UpdateNearestInterval,
                    ScanInterval = authoring.ScanInterval
                });
                AddComponent<EyeSensor>(entity);
                AddComponent<CommandPriorityMode>(entity);
            }
        }
    }
}