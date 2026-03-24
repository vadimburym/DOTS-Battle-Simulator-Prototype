using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using _Project._Code.Gameplay.CoreFeatures.Units.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using VATDots;

namespace _Project._Code.Gameplay.CoreFeatures.AI._Root
{
    public struct BtContext
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        
        [ReadOnly] public ComponentLookup<EyeSensor> EyeSensorLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        [ReadOnly] public ComponentLookup<AttackStats> AttackStatsLookup;
        [ReadOnly] public ComponentLookup<GridNavigationState> GridNavigationStateLookup;
        [ReadOnly] public ComponentLookup<IsMovingTag> IsMovingTagLookup;
        [ReadOnly] public ComponentLookup<RendererEntityRef> RenderEntityLookup;
        [ReadOnly] public ComponentLookup<UnitTag> UnitTagLookup;
    }
}