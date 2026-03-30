using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.StaticData._Root;
using Unity.Collections;
using Unity.Entities;
using VadimBurym.DodBehaviourTree;
using VContainer;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.AiSystems
{
    [DisableAutoCreation]
    public partial class BehaviourTreeInitSystem : SystemBase
    {
        private const int Capacity = 256;
        
        [Inject]
        private void Construct(
            StaticDataService staticDataService,
            IAddressableService addressableService)
        {
            var btAssets = staticDataService.BehaviourTreeStaticData.Assets;
            var blobs = new NativeArray<BlobAssetReference<BehaviourTreeBlob>>(
                Capacity,
                Allocator.Persistent,
                NativeArrayOptions.ClearMemory);
            foreach (var asset in btAssets)
            {
                if (addressableService.TryGetLoadedObject<BehaviourTreeAsset>(asset.Value, out var loadedAsset))
                {
                    var id = (byte)asset.Key;
                    blobs[id] = loadedAsset.CreateBlob();
                }
            }
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(entity, new BehaviourTreeSingleton {
                Blobs = blobs
            });
        }
        
        protected override void OnDestroy()
        {
            if (!SystemAPI.HasSingleton<BehaviourTreeSingleton>())
                return;
            var trees = SystemAPI.GetSingleton<BehaviourTreeSingleton>().Blobs;
            if (trees.IsCreated)
            {
                for (int i = 0; i < trees.Length; i++)
                    if (trees[i].IsCreated)
                        trees[i].Dispose();
                trees.Dispose();
            }
            if (SystemAPI.TryGetSingletonEntity<BehaviourTreeSingleton>(out var e))
                EntityManager.DestroyEntity(e);
        }

        protected override void OnUpdate() { }
    }
}