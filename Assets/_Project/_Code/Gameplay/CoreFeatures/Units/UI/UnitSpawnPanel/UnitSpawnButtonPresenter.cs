using _Project._Code.Gameplay.CoreFeatures.Units.Service;
using _Project._Code.Infrastructure.StaticData.Units;
using UnityEngine;

namespace _Project._Code.Gameplay.CoreFeatures.Units.UI
{
    public sealed class UnitSpawnButtonPresenter : IUnitSpawnButtonPresenter
    {
        public Sprite Icon => _unitData.Icon;

        private readonly UnitConfig _unitData;
        private readonly IUnitSpawnService _unitSpawnService;
        
        public UnitSpawnButtonPresenter(
            IUnitSpawnService unitSpawnService,
            UnitConfig unitData)
        {
            _unitData = unitData;
            _unitSpawnService = unitSpawnService;
        }
        
        public void OnSpawnDataClicked(int count)
        {
            _unitSpawnService.SetUnitSpawnData(new UnitSpawnData {
                UnitId = _unitData.UnitId,
                Count = count,
            });
        }
    }
}