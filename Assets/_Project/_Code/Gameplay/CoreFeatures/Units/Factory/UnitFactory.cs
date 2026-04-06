using _Project._Code.Core.Keys;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.EcsContext;
using _Project._Code.Infrastructure.StaticData._Root;
using _Project._Code.Infrastructure.StaticData.Units;
using _Project._Code.Locale;
using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Factory
{
    public interface IUnitFactory
    {
        
    }
    public sealed class UnitFactory : IUnitFactory
    {
        private readonly UnitsStaticData _staticData;
        private readonly IAddressableService _addressableService;
        private readonly IEcsContext _ecsContext;
        
        public UnitFactory(
            StaticDataService staticDataService,
            IAddressableService addressableService,
            IEcsContext ecsContext)
        {
            _staticData = staticDataService.UnitsStaticData;
            _addressableService = addressableService;
            _ecsContext = ecsContext;
        }

        public void Create(UnitId unitId)
        {
            var unitData = _addressableService.GetLoadedObject<UnitConfig>(_staticData.GetUnitData(unitId));
        }
    }
}