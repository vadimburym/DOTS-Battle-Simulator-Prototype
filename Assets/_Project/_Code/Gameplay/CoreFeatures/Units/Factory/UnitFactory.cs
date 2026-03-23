using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using _Project._Code.Gameplay.CoreFeatures.Units.Components;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.EcsContext;
using _Project._Code.Infrastructure.StaticData._Root;
using _Project._Code.Infrastructure.StaticData.AI;
using _Project._Code.Infrastructure.StaticData.Units;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VadimBurym.DodBehaviourTree;
using Random = Unity.Mathematics.Random;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Factory
{
    public sealed class UnitFactory : IUnitFactory
    {
        private readonly UnitsStaticData _unitsStaticData;
        private readonly BehaviourTreeStaticData _behaviourTreeStaticData;
        private readonly IAddressableService _addressableService;
        private readonly IEntityPrefabService _entityPrefabService;
        private readonly IEcsContext _ecsContext;
        private Random _random;
        
        public UnitFactory(
            StaticDataService staticDataService,
            IAddressableService addressableService,
            IEntityPrefabService entityPrefabService,
            IEcsContext ecsContext)
        {
            _unitsStaticData = staticDataService.UnitsStaticData;
            _behaviourTreeStaticData = staticDataService.BehaviourTreeStaticData;
            _addressableService = addressableService;
            _entityPrefabService = entityPrefabService;
            _ecsContext = ecsContext;
            _random = new Random(12345);
        }

        public Entity Create(UnitId unitId, float3 position, byte team, EntityCommandBuffer ecb)
        {
            var unitData = _addressableService.GetLoadedObject<UnitConfig>(_unitsStaticData.GetUnitData(unitId));
            var entityPrefab = _entityPrefabService.GetEntityPrefab(unitData.EntityPoolId);
            var entityManager = _ecsContext.EntityManager;
            var entity = ecb.Instantiate(entityPrefab);
            
            ecb.SetComponent(entity, LocalTransform.FromPosition(position));
            ecb.SetComponent(entity, new TargetPosition{ Position = position });
            ecb.SetComponent(entity, new MovementStats {
                Speed = unitData.Speed,
                RotationSpeed = unitData.RotationSpeed,
            });
            ecb.SetComponentEnabled<SelectedTag>(entity, false);
            ecb.SetComponent(entity, new Footprint {
                FootprintX = unitData.FootprintX,
                FootprintY = unitData.FootprintY,
            });
            ecb.SetComponent(entity, new Team { Value = team });
            ecb.SetComponentEnabled<MyTeamTag>(entity, team == 0);
            ecb.SetComponent(entity, new AttackStats {
                AttackInterval = unitData.AttackInterval,
                AttackRangeCells = unitData.AttackRangeCells,
                Damage = unitData.Damage
            });
            ecb.SetComponent(entity, new EyeSensorStats {
                DetectRadius = unitData.DetectRadius,
                ChaseRadius = unitData.ChaseRadius,
                UpdateNearestInterval = unitData.UpdateNearestInterval,
                ScanInterval = unitData.ScanInterval
            });
            ecb.SetComponent(entity, new AiBrain {
                BlobId = (byte)unitData.BehaviourTreeId,
                UpdateTime = (float)entityManager.WorldUnmanaged.Time.ElapsedTime + _random.NextFloat(0f, 0.2f)
            });
            ecb.SetComponent(entity, new EyeSensor {
                ScanTimer = _random.NextFloat(0f, unitData.ScanInterval)
            });
            
            var btAsset = _addressableService.GetLoadedObject<BehaviourTreeAsset>(
                _behaviourTreeStaticData.GetAsset(unitData.BehaviourTreeId));
            btAsset.FillAgentStateBuffers(entity, ecb);
            
            return entity;
        }
    }
}