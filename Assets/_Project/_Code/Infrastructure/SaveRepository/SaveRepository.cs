using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using _Project._Code.Core.Abstractions;
using UnityEngine;

namespace _Project._Code.Infrastructure
{
    public sealed class SaveRepository : ISaveRepository
    {
        private Dictionary<string, string> _repository = new();
        private readonly ISaveStrategy _saveStrategy;

        public SaveRepository(ISaveStrategy saveStrategy)
        {
            _saveStrategy = saveStrategy;
        }
        
        public async UniTask Load()
        {
            _repository = await _saveStrategy.LoadRepository();
#if UNITY_EDITOR
            Debug.Log($"<color=green>Repository loaded!</color>");
#endif
        }
        
        public async UniTask Save()
        {
            await _saveStrategy.SaveRepository(_repository);
#if UNITY_EDITOR
            Debug.Log($"<color=green>Repository saved!</color>");
#endif
        }

        public void SetData<T>(T data)
        {
            string key = typeof(T).Name;
            var jsonData = JsonConvert.SerializeObject(data);
            _repository[key] = jsonData;
        }

        public bool TryGetData<T>(out T data)
        {
            string key = typeof(T).Name;
            if (_repository.TryGetValue(key, out var jsonData))
            {
                try
                {
                    data = JsonConvert.DeserializeObject<T>(jsonData);
                    return true;
                }
                catch
                {
                    data = default;
                    return false;
                }
            }
            data = default;
            return false;
        }

        public void Delete()
        {
            _saveStrategy.DeleteRepository();
        }
    }
}