using Unity.Entities;

namespace VATDots
{
    public struct VATAnimationLibraryBlob
    {
        public BlobArray<VATClipRuntimeData> Clips;
        public byte ClipsLength;
    }
}