using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Systems
{
    [DisableAutoCreation]
    public partial struct SelectedViewSystem : ISystem
    {
        private ComponentLookup<LocalTransform> _localTransformLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _localTransformLookup = state.GetComponentLookup<LocalTransform>(false);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _localTransformLookup.Update(ref state);

            var hideJob = new HideSelectedViewJob {
                LocalTransformLookup = _localTransformLookup
            };
            var showJob = new ShowSelectedViewJob {
                LocalTransformLookup = _localTransformLookup
            };

            var hideHandle = hideJob.ScheduleParallel(state.Dependency);
            var showHandle = showJob.ScheduleParallel(hideHandle);
            state.Dependency = showHandle;
        }
        
        [BurstCompile]
        [WithDisabled(typeof(Selected))]
        public partial struct HideSelectedViewJob : IJobEntity
        {
            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> LocalTransformLookup;

            [BurstCompile]
            private void Execute(in Selected selected)
            {
                if (!LocalTransformLookup.HasComponent(selected.SelectedView))
                    return;

                var viewTransform = LocalTransformLookup[selected.SelectedView];
                viewTransform.Scale = 0f;
                LocalTransformLookup[selected.SelectedView] = viewTransform;
            }
        }

        [BurstCompile]
        public partial struct ShowSelectedViewJob : IJobEntity
        {
            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> LocalTransformLookup;

            [BurstCompile]
            private void Execute(in Selected selected)
            {
                if (!LocalTransformLookup.HasComponent(selected.SelectedView))
                    return;

                var viewTransform = LocalTransformLookup[selected.SelectedView];
                viewTransform.Scale = selected.ShowScale;
                LocalTransformLookup[selected.SelectedView] = viewTransform;
            }
        }
    }
}