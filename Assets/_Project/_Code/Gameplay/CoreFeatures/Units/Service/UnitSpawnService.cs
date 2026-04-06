using _Project._Code.Core.Keys;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Service
{
    public sealed class UnitSpawnService : IUnitSpawnService
    {
        public bool IsSpawnMode => _isSpawnMode;
        public UnitSpawnData UnitSpawnData => _spawnData;
        
        private bool _isSpawnMode;
        private UnitSpawnData _spawnData;

        public void SetUnitSpawnData(UnitSpawnData spawnData)
        {
            _isSpawnMode = true;
            _spawnData = spawnData;
        }
        
        public void ClearUnitSpawnData()
        {
            _isSpawnMode = false;
            _spawnData = default;
        }
    }
    
    public struct UnitSpawnData
    {
        public UnitId UnitId;
        public int Count;
    }
}