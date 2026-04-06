namespace _Project._Code.Gameplay.CoreFeatures.Units.Service
{
    public interface IUnitSpawnService
    {
        bool IsSpawnMode { get; }
        UnitSpawnData UnitSpawnData { get; }
        void SetUnitSpawnData(UnitSpawnData spawnData);
        void ClearUnitSpawnData();
    }
}