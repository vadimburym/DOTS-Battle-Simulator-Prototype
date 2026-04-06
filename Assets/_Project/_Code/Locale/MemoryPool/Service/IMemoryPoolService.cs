using _Project._Code.Core.Keys;
using UnityEngine;

namespace _Project._Code.Locale
{
    public interface IMemoryPoolService
    {
        void AddGameObjectMemoryPool(MemoryPoolId id, GameObjectMemoryPool memoryPool);
        
        T SpawnGameObject<T>(MemoryPoolId name) where T : Component;
        GameObject SpawnGameObject(MemoryPoolId name);
        void UnspawnGameObject<T>(MemoryPoolId name, T item) where T : Component;
        void UnspawnGameObject(MemoryPoolId name, GameObject item);
        void ClearGameObjectPool(MemoryPoolId name);

        ObjectMemoryPool<T> GetObjectPool<T>() where T : class, new();
        void EnqueueObject<T>(T item) where T : class, new();
        T DequeueObject<T>() where T : class, new();
        void ClearObjectPool<T>() where T : class, new();
        void DisposeObjectPool<T>() where T : class, new();
    }
}