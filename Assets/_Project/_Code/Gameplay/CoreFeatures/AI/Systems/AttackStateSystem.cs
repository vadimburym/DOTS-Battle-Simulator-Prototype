using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.AiSystems
{
    [BurstCompile]
    [DisableAutoCreation]
    public partial struct AttackStateSystem : ISystem
    {
        private ComponentLookup<GridNavigationState> _gridLookup;
        private ComponentLookup<IsMovingTag> _movementLookup;
        private ComponentLookup<AttackStats> _attackStatsLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();

            _gridLookup = state.GetComponentLookup<GridNavigationState>(true);
            _movementLookup = state.GetComponentLookup<IsMovingTag>(true);
            _attackStatsLookup = state.GetComponentLookup<AttackStats>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _gridLookup.Update(ref state);
            _movementLookup.Update(ref state);
            _attackStatsLookup.Update(ref state);

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            state.Dependency = new AttackStateJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Ecb = ecb,
                //Utils = new BattlefieldGridUtils(),
                GridLookup = _gridLookup,
                MovementLookup = _movementLookup,
                AttackStatsLookup = _attackStatsLookup
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct AttackStateJob : IJobEntity
        {
            public float DeltaTime;
            public EntityCommandBuffer.ParallelWriter Ecb;
            //public BattlefieldGridUtils Utils;

            [ReadOnly] public ComponentLookup<GridNavigationState> GridLookup;
            [ReadOnly] public ComponentLookup<IsMovingTag> MovementLookup;
            [ReadOnly] public ComponentLookup<AttackStats> AttackStatsLookup;

            [BurstCompile]
            private void Execute(
                [ChunkIndexInQuery] int sortKey,
                Entity attackStateEntity,
                ref AttackState attackState)
            {
                attackState.RemainingTime = math.max(0f, attackState.RemainingTime - DeltaTime);

                var owner = attackState.Owner;
                var target = attackState.Target;
                var ownerGrid = GridLookup[owner];
                var targetGrid = GridLookup[target];
                
                if (MovementLookup[owner].IsMoving != 0)
                    return;
                int range = math.max(1, AttackStatsLookup[owner].AttackRangeCells);
                int dist = BattlefieldGridUtils.CellDistanceChebyshev(
                    ownerGrid.OccupiedCell,
                    targetGrid.MovingCell);

                if (dist > range)
                    return;

                if (attackState.RemainingTime > 0f)
                    return;

    #if UNITY_EDITOR
                UnityEngine.Debug.Log(
                    $"Attack: owner={owner.Index} target={target.Index} state={attackStateEntity.Index}");
    #endif

                attackState.RemainingTime = AttackStatsLookup[owner].AttackInterval;
            }
        }
    }
}