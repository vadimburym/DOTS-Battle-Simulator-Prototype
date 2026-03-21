using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using _Project._Code.Gameplay.CoreFeatures.Units.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using VadimBurym.DodBehaviourTree;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Authoring
{
    public sealed class UnitAuthoring : MonoBehaviour
    {
        [Header("References")] 
        public GameObject SelectedView;
        
        public sealed class Baker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<MovementStats>(entity);
                AddComponent<TargetPosition>(entity);
                AddComponent(entity, new Selected {
                    SelectedView = GetEntity(authoring.SelectedView, TransformUsageFlags.Dynamic),
                    ShowScale = authoring.SelectedView.transform.localScale.x
                });
                SetComponentEnabled<Selected>(entity, false);
                AddComponent<MyTeamTag>(entity);
                SetComponentEnabled<MyTeamTag>(entity, false);
                AddComponent<Footprint>(entity);
                AddComponent(entity, new GridNavigationState {
                    OccupiedCell = int2.zero,
                    MovingCell = int2.zero,
                    HasOccupiedCell = 0,
                });
                AddComponent<Team>(entity);
                AddComponent<IsMovingTag>(entity);
                AddComponent<AttackStats>(entity);
                AddComponent<EyeSensorStats>(entity);
                AddComponent<EyeSensor>(entity);
                AddComponent<CommandPriorityMode>(entity);
                AddComponent<AiBrain>(entity);
                AddBuffer<NodeStateElement>(entity);
                AddBuffer<LeafStateElement>(entity);
            }
        }
    }
}