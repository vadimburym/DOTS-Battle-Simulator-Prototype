using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using _Project._Code.Gameplay.CoreFeatures.Units.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using VadimBurym.DodBehaviourTree;
using VadimBurym.DodBehaviourTree.Generated;
using VATDots;
using Random = Unity.Mathematics.Random;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.AiSystems
{
    [DisableAutoCreation]
    [BurstCompile]
    public partial struct BehaviourTreeTickSystem : ISystem
    {
        private const float UpdateInterval = 0.2f;
        private const int RandomSeed = 55555;

        private BTRunner_BtContext _runner;
        private Random _random;
        private EntityStorageInfoLookup _entityInfoLookup;

        private ComponentTypeHandle<AiBrain> _aiBrainHandle;
        private BufferTypeHandle<NodeStateElement> _nodeStateHandle;
        private BufferTypeHandle<LeafStateElement> _leafStateHandle;
        private EntityTypeHandle _entityHandle;
        private EntityQuery _query;

        private ComponentLookup<EyeSensor> _eyeSensorLookup;
        private ComponentLookup<LocalTransform> _localTransformLookup;
        private ComponentLookup<AttackStats> _attackStatsLookup;
        private ComponentLookup<GridNavigationState> _gridNavigationStateLookup;
        private ComponentLookup<IsMovingTag> _isMovingTagLookup;
        private ComponentLookup<RendererEntityRef> _rendererEntityLookup;

        private ComponentLookup<AttackState> _attackStateLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BehaviourTreeSingleton>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            _runner = new BTRunner_BtContext();
            _random = new Random(RandomSeed);
            _query = SystemAPI.QueryBuilder()
                .WithAllRW<AiBrain>()
                .Build();
            _entityInfoLookup = state.GetEntityStorageInfoLookup();
            _aiBrainHandle = state.GetComponentTypeHandle<AiBrain>(false);
            _nodeStateHandle = state.GetBufferTypeHandle<NodeStateElement>(false);
            _leafStateHandle = state.GetBufferTypeHandle<LeafStateElement>(false);
            _entityHandle = state.GetEntityTypeHandle();

            _eyeSensorLookup = state.GetComponentLookup<EyeSensor>(isReadOnly: true);
            _localTransformLookup = state.GetComponentLookup<LocalTransform>(isReadOnly: true);
            _attackStatsLookup = state.GetComponentLookup<AttackStats>(isReadOnly: true);
            _gridNavigationStateLookup = state.GetComponentLookup<GridNavigationState>(isReadOnly: true);
            _isMovingTagLookup = state.GetComponentLookup<IsMovingTag>(isReadOnly: true);
            _rendererEntityLookup = state.GetComponentLookup<RendererEntityRef>(isReadOnly: true);

            _attackStateLookup = state.GetComponentLookup<AttackState>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var trees = SystemAPI.GetSingleton<BehaviourTreeSingleton>().Blobs;
            _entityInfoLookup.Update(ref state);

            _eyeSensorLookup.Update(ref state);
            _localTransformLookup.Update(ref state);
            _attackStatsLookup.Update(ref state);
            _gridNavigationStateLookup.Update(ref state);
            _isMovingTagLookup.Update(ref state);
            _rendererEntityLookup.Update(ref state);

            _attackStateLookup.Update(ref state);

            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            var context = new BtContext
            {
                Ecb = ecb.AsParallelWriter(),
                EyeSensorLookup = _eyeSensorLookup,
                LocalTransformLookup = _localTransformLookup,
                AttackStatsLookup = _attackStatsLookup,
                GridNavigationStateLookup = _gridNavigationStateLookup,
                IsMovingTagLookup = _isMovingTagLookup,
                RenderEntityLookup = _rendererEntityLookup,
                AttackStateLookup = _attackStateLookup,
                EntityInfoLookup = _entityInfoLookup
            };

            _aiBrainHandle.Update(ref state);
            _nodeStateHandle.Update(ref state);
            _leafStateHandle.Update(ref state);
            _entityHandle.Update(ref state);
            var job = new BehaviourTreeTickChunkJob
            {
                Runner = _runner,
                Trees = trees,
                Context = context,
                Random = _random,
                AiBrainHandle = _aiBrainHandle,
                NodeStateHandle = _nodeStateHandle,
                LeafStateHandle = _leafStateHandle,
                EntityHandle = _entityHandle,
                CurrentTime = (float)SystemAPI.Time.ElapsedTime
            };
            state.Dependency = job.ScheduleParallel(_query, state.Dependency);
        }

        [BurstCompile]
        public struct BehaviourTreeTickChunkJob : IJobChunk
        {
            public BTRunner_BtContext Runner;
            public BtContext Context;
            [ReadOnly] public NativeArray<BlobAssetReference<BehaviourTreeBlob>> Trees;
            public Random Random;
            public float CurrentTime;

            public ComponentTypeHandle<AiBrain> AiBrainHandle;
            public BufferTypeHandle<NodeStateElement> NodeStateHandle;
            public BufferTypeHandle<LeafStateElement> LeafStateHandle;

            [ReadOnly] public EntityTypeHandle EntityHandle;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var aiBrains = chunk.GetNativeArray(ref AiBrainHandle);
                var nodeBuffers = chunk.GetBufferAccessor(ref NodeStateHandle);
                var leafBuffers = chunk.GetBufferAccessor(ref LeafStateHandle);
                var entities = chunk.GetNativeArray(EntityHandle);

                var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

                while (enumerator.NextEntityIndex(out int i))
                {
                    var aiBrain = aiBrains[i];

                    if (aiBrain.UpdateTime > CurrentTime)
                        continue;

                    var entity = entities[i];
                    var nodeStates = nodeBuffers[i];
                    var leafStates = leafBuffers[i];
                    var treeBlob = Trees[aiBrain.BlobId];

                    Runner.Tick(
                        entity,
                        ref treeBlob.Value,
                        ref Random,
                        nodeStates,
                        leafStates,
                        Context,
                        unfilteredChunkIndex);

                    aiBrain.UpdateTime += UpdateInterval;
                    aiBrains[i] = aiBrain;
                }
            }
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}
