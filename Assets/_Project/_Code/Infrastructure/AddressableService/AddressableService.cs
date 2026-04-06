using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _Project._Code.Infrastructure
{
    public sealed class AddressableService : IAddressableService
    {
        private readonly Dictionary<string, AsyncOperationHandle<Object>> _loadedObject = new();
        private readonly List<UniTask> _taskCache = new();

        public async UniTask LoadObjectsByLabelsAsync(IEnumerable<string> addressableLabels, Addressables.MergeMode labelsMergeMode)
        {
            var locationHandle = Addressables.LoadResourceLocationsAsync(addressableLabels, labelsMergeMode, typeof(Object));
            await locationHandle.Task;    
            var locations = locationHandle.Result;

            _taskCache.Clear();
            for (int i = 0; i < locations.Count; i++)
            {
                var location = locations[i];
                _taskCache.Add(LoadObjectAsync(location.PrimaryKey));
            }
            await UniTask.WhenAll(_taskCache);
            Addressables.Release(locationHandle);
        }

        public async UniTask LoadObjectAsync(string addressableName)
        {
            if (_loadedObject.ContainsKey(addressableName))
                return;

            var operationHandle = Addressables.LoadAssetAsync<Object>(addressableName);
            _loadedObject[addressableName] = operationHandle;
            await operationHandle.Task;
        }

        public async UniTask LoadObjectAsync(AssetReference assetReference)
        {
            var locationHandle = Addressables.LoadResourceLocationsAsync(assetReference.RuntimeKey, typeof(Object));
            await locationHandle.Task;
            var location = locationHandle.Result[0].PrimaryKey;
            Addressables.Release(locationHandle);

            await LoadObjectAsync(location);
        }

        public bool TryGetLoadedObject<T>(string addressableName, out T asset) where T : Object
        {
            asset = null;
            if (!_loadedObject.ContainsKey(addressableName))
                return false;

            var operationHandler = _loadedObject[addressableName];

            if (!operationHandler.Task.IsCompleted)
                return false;

            asset = operationHandler.Result as T;
            if (asset == null)
                throw new Exception($"Can't cast object with name {addressableName} to target type {typeof(T)}!");

            return true;
        }

        public bool TryGetLoadedObject<T>(AssetReference assetReference, out T asset) where T : Object
        {
            asset = null;
            var locationHandle = Addressables.LoadResourceLocationsAsync(assetReference.RuntimeKey, typeof(Object));
            if (!locationHandle.IsDone)
                return false;

            var location = locationHandle.Result[0].PrimaryKey;
            Addressables.Release(locationHandle);

            return TryGetLoadedObject<T>(location, out asset);
        }

        public T GetLoadedObject<T>(string addressableName) where T : Object
        {
            if (!_loadedObject.ContainsKey(addressableName))
                throw new Exception($"Can't find object with name {addressableName} in loaded operations!");

            var operationHandler = _loadedObject[addressableName];
            if (!operationHandler.Task.IsCompleted)
                throw new Exception($"Object with name {addressableName} is trying to resolve during loading!");

            var asset = operationHandler.Result as T;
            if (asset == null)
                throw new Exception($"Can't cast object with name {addressableName} to target type {typeof(T)}!");

            return asset;
        }

        public T GetLoadedObject<T>(AssetReference assetReference) where T : Object
        {
            var locationHandle = Addressables.LoadResourceLocationsAsync(assetReference.RuntimeKey, typeof(Object));
            if (!locationHandle.IsDone)
                throw new Exception($"Can't Get Cached Resource Location with {assetReference.RuntimeKey}!");

            var location = locationHandle.Result[0].PrimaryKey;
            Addressables.Release(locationHandle);

            return GetLoadedObject<T>(location);
        }

        public async UniTask<T> GetObjectAsync<T>(string addressableName) where T : Object
        {
            if (_loadedObject.ContainsKey(addressableName))
            {
                var operationHandler = _loadedObject[addressableName];

                if (!operationHandler.Task.IsCompleted)
                    await operationHandler.Task;

                var asset = operationHandler.Result as T;
                if (asset == null)
                    throw new Exception($"Can't cast object with name {addressableName} to target type {typeof(T)}!");

                return asset;
            }
            else
            {
                await LoadObjectAsync(addressableName);
                var asset = _loadedObject[addressableName].Result as T;

                if (asset == null)
                    throw new Exception($"Can't cast object with name {addressableName} to target type {typeof(T)}!");

                return asset;
            }
        }

        public async UniTask<T> GetObjectAsync<T>(AssetReference assetReference) where T : Object
        {
            var locationHandle = Addressables.LoadResourceLocationsAsync(assetReference.RuntimeKey, typeof(Object));
            await locationHandle.Task;
            var location = locationHandle.Result[0].PrimaryKey;
            Addressables.Release(locationHandle);

            var asset = await GetObjectAsync<T>(location);
            return asset;
        }

        public void Release(string addressableName)
        {
            if (_loadedObject.ContainsKey(addressableName))
            {
                var operationHandler = _loadedObject[addressableName];
                Addressables.Release(operationHandler);
                _loadedObject.Remove(addressableName);
            }
        }

        public void Release(AssetReference assetReference)
        {
            var locationHandle = Addressables.LoadResourceLocationsAsync(assetReference.RuntimeKey, typeof(Object));
            if (!locationHandle.IsDone)
                throw new Exception($"Can't Get Cached Resource Location with {assetReference.RuntimeKey}!");

            var location = locationHandle.Result[0].PrimaryKey;
            Addressables.Release(locationHandle);

            Release(location);
        }

        public async UniTask ReleaseByLabels(IEnumerable<string> addressableLabels, Addressables.MergeMode labelsMergeMode)
        {
            var locationHandle = Addressables.LoadResourceLocationsAsync(addressableLabels, labelsMergeMode, typeof(Object));
            await locationHandle.Task;
            var locations = locationHandle.Result;

            for (int i = 0; i < locations.Count; i++)
            {
                var location = locations[i];
                Release(location.PrimaryKey);
            }
            Addressables.Release(locationHandle);
        }

        public void Dispose()
        {
            foreach (var operation in _loadedObject.Values)
            {
                Addressables.Release(operation);
            }
            _loadedObject.Clear();
        }
    }
}