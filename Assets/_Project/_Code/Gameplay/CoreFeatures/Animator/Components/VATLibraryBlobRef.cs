using Unity.Entities;

namespace VATDots
{
    public struct VATLibraryBlobRef : IComponentData
    {
        public BlobAssetReference<VATAnimationLibraryBlob> Value;
    }
}