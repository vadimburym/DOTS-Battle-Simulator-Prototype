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
        [WithDisabled(typeof(SelectedTag))]
        public partial struct HideSelectedViewJob : IJobEntity
        {
            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> LocalTransformLookup;

            [BurstCompile]
            private void Execute(in SelectedTag selectedTag)
            {
                if (!LocalTransformLookup.HasComponent(selectedTag.SelectedView))
                    return;

                var viewTransform = LocalTransformLookup[selectedTag.SelectedView];
                viewTransform.Scale = 0f;
                LocalTransformLookup[selectedTag.SelectedView] = viewTransform;
            }
        }

        [BurstCompile]
        public partial struct ShowSelectedViewJob : IJobEntity
        {
            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> LocalTransformLookup;

            [BurstCompile]
            private void Execute(in SelectedTag selectedTag)
            {
                if (!LocalTransformLookup.HasComponent(selectedTag.SelectedView))
                    return;

                var viewTransform = LocalTransformLookup[selectedTag.SelectedView];
                viewTransform.Scale = selectedTag.ShowScale;
                LocalTransformLookup[selectedTag.SelectedView] = viewTransform;
            }
        }
    }
}