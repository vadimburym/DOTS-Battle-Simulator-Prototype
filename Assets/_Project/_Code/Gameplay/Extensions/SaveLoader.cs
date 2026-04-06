using _Project._Code.Core.Abstractions;
using _Project._Code.Core.Contracts;
using _Project._Code.Infrastructure;
using VContainer;

namespace _Project._Code.Gameplay.Extensions
{
    public sealed class SaveLoader<TService, TSaveData> : ISaveLoader
        where TService : ISaveDataSetup<TSaveData>, ISaveDataConvert<TSaveData>
    {
        [Inject] private readonly ISaveRepository _repository;
        [Inject] private readonly TService _service;

        public void LoadData()
        {
            if (_repository.TryGetData(out TSaveData data))
                _service.SetupSaveData(data);
        }

        public void SaveData()
        {
            var data = _service.ConvertToSaveData();
            _repository.SetData(data);
        }
    }
}