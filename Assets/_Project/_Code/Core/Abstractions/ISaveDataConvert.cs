namespace _Project._Code.Core.Abstractions
{
    public interface ISaveDataConvert<TSaveData>
    {
        TSaveData ConvertToSaveData();
    }
}