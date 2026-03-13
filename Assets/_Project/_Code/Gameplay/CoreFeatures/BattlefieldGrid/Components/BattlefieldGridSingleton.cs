using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures
{
    public struct BattlefieldGridSingleton : IComponentData
    {
        public BlobAssetReference<BattlefieldGridBlob> Value;
    }
}