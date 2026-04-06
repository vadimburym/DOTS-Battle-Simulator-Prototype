namespace _Project._Code.Core.Abstractions
{
    public interface ISaveDataSetup<TSaveData>
    {
        void SetupSaveData(TSaveData saveData);
    }
}