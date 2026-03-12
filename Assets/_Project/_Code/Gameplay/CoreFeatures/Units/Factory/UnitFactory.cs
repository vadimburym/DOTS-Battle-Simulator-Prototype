using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.EcsContext;
using _Project._Code.Infrastructure.StaticData._Root;
using _Project._Code.Infrastructure.StaticData.Units;
using _Project._Code.Locale;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Factory
{
    public sealed class UnitFactory : IUnitFactory
    {
        private readonly UnitsStaticData _staticData;
        private readonly IAddressableService _addressableService;
        private readonly IEntityPrefabService _entityPrefabService;
        private readonly IEcsContext _ecsContext;
        
        public UnitFactory(
            StaticDataService staticDataService,
            IAddressableService addressableService,
            IEntityPrefabService entityPrefabService,
            IEcsContext ecsContext)
        {
            _staticData = staticDataService.UnitsStaticData;
            _addressableService = addressableService;
            _entityPrefabService = entityPrefabService;
            _ecsContext = ecsContext;
        }

        public Entity Create(UnitId unitId, float3 position)
        {
            var unitData = _addressableService.GetLoadedObject<UnitConfig>(_staticData.GetUnitData(unitId));
            var entityPrefab = _entityPrefabService.GetEntityPrefab(unitData.EntityPoolId);
            var entity = _ecsContext.EntityManager.Instantiate(entityPrefab);
            var entityManager = _ecsContext.EntityManager; //TODO ECB
            
            entityManager.SetComponentData(entity, LocalTransform.FromPosition(position));
            entityManager.SetComponentData(entity, new TargetPosition{ Position = position });
            
            return entity;
        }
    }
}