using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace _Project._Code.Infrastructure
{
    public interface IAddressableService
    {
        UniTask LoadObjectsByLabelsAsync(IEnumerable<string> addressableLabels, Addressables.MergeMode labelsMergeMode);
        UniTask LoadObjectAsync(string addressableName);
        UniTask LoadObjectAsync(AssetReference assetReference);
        bool TryGetLoadedObject<T>(string addressableName, out T asset) where T : Object;
        bool TryGetLoadedObject<T>(AssetReference assetReference, out T asset) where T : Object;
        T GetLoadedObject<T>(string addressableName) where T : Object;
        T GetLoadedObject<T>(AssetReference assetReference) where T : Object;
        UniTask<T> GetObjectAsync<T>(string addressableName) where T : Object;
        UniTask<T> GetObjectAsync<T>(AssetReference assetReference) where T : Object;
        void Release(string addressableName);
        void Release(AssetReference assetReference);
        UniTask ReleaseByLabels(IEnumerable<string> addressableLabels, Addressables.MergeMode labelsMergeMode);
        void Dispose();
    }
}