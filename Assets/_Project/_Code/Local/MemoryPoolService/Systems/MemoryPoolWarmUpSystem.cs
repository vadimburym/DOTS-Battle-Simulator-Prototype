using _Project._Code.Core.Contracts;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.StaticData._Root;
using UnityEngine;

namespace _Project._Code.Locale
{
    public sealed class MemoryPoolWarmUpSystem : IWarmUp
    {
        private readonly ITransformProvider _transformProvider;
        private readonly IMemoryPoolService _memoryPoolService;
        private readonly IAddressableService _addressableService;
        private readonly StaticDataService _staticDataService;
        
        public MemoryPoolWarmUpSystem(
            ITransformProvider transformProvider,
            IMemoryPoolService memoryPoolService,
            IAddressableService addressableService,
            StaticDataService staticDataService)
        {
            _addressableService = addressableService;
            _staticDataService = staticDataService;
            _memoryPoolService = memoryPoolService;
            _transformProvider = transformProvider;
        }

        public void WarmUp()
        {
            //var pipeline = _addressableService.GetLoadedObject<MemoryPoolPipeline>(
            //    _staticDataService.MemoryPoolPipeline);
            var pipeline = _staticDataService.MemoryPoolPipeline.GameObjectMemoryPools;
            for (int i = 0; i < pipeline.Length; i++)
            {
                var pool = pipeline[i];
                if (_addressableService.TryGetLoadedObject<GameObject>(pool.AssetReference, out var asset))
                {
                    var transform = _transformProvider.GetTransform(pool.TransformId);
                    var gameObjectPool = new GameObjectMemoryPool(asset, transform, pool.InitialCount);
                    gameObjectPool.WarmUp();
                    _memoryPoolService.AddGameObjectMemoryPool(pool.PoolId, gameObjectPool);
                }
            }
        }
    }
}