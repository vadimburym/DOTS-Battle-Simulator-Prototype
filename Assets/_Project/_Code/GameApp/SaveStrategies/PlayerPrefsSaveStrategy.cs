using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using _Project._Code.Core.Abstractions;
using UnityEngine;

namespace _Project._Code.GameApp.SaveStrategies
{
    [Serializable]
    public sealed class PlayerPrefsSaveStrategy : ISaveStrategy
    {
        [SerializeField] private string _key = "save";
        
        async UniTask<Dictionary<string, string>> ISaveStrategy.LoadRepository()
        {
            if (PlayerPrefs.HasKey(_key))
            {
                await UniTask.Delay(0);
                var jsonData = PlayerPrefs.GetString(_key);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);
            }
            else
            {
                return new();
            }
        }

        async UniTask ISaveStrategy.SaveRepository(Dictionary<string, string> gameState)
        {
            await UniTask.Delay(0);
            var jsonData = JsonConvert.SerializeObject(gameState);
            PlayerPrefs.SetString(_key, jsonData);
        }

        void ISaveStrategy.DeleteRepository()
        {
            PlayerPrefs.DeleteKey(_key);
        }
    }
}