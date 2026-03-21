using Unity.Collections;
using Unity.Entities;
using VadimBurym.DodBehaviourTree;

namespace _Project._Code.Gameplay.CoreFeatures.AI._Root
{
    public struct BehaviourTreeSingleton : IComponentData
    {
        public NativeArray<BlobAssetReference<BehaviourTreeBlob>> Blobs;
    }
}