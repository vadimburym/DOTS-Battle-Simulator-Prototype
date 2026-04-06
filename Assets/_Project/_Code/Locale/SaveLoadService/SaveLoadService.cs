using System.Collections.Generic;
using _Project._Code.Core.Contracts;
using _Project._Code.Infrastructure;
using Cysharp.Threading.Tasks;

namespace _Project._Code.Locale
{
    public sealed class SaveLoadService : ISaveLoadService
    {
        private readonly IReadOnlyList<ISaveLoader> _saveLoaders;
        private readonly ISaveRepository _saveRepository;
        
        public SaveLoadService(
            IReadOnlyList<ISaveLoader> saveLoaders,
            ISaveRepository saveRepository)
        {
            _saveLoaders = saveLoaders;
            _saveRepository = saveRepository;
        }
        
        public void Save()
        {
            for (int i = 0; i < _saveLoaders.Count; i++)
                _saveLoaders[i].SaveData();
            _saveRepository.Save().Forget();
        }

        public void Load()
        {
            for (int i = 0; i < _saveLoaders.Count; i++)
                _saveLoaders[i].LoadData();
        }
    }
}